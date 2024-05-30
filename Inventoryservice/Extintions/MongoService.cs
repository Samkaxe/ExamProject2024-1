using System.Security.Cryptography;
using System.Text;
using Core.Dtos;
using Core.Entites;
using MongoDB.Bson;
using MongoDB.Driver;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace InventoryService.Extintions
{
    public class MongoService
    {
        private readonly IMongoDatabase _database;
        private readonly IAsyncPolicy _policyWrap;
        private readonly ILogger<MongoService> _logger;

        public MongoService(IConfiguration configuration, IAsyncPolicy policyWrap, ILogger<MongoService> logger)
        {
            _policyWrap = policyWrap;
            _logger = logger;

            try
            {
                var connectionString = configuration["MongoSettings:ConnectionString"];
                _logger.LogInformation($"Connecting to MongoDB with connection string: {connectionString}");

                var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
                clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());

                var client = new MongoClient(clientSettings);
                _database = client.GetDatabase("ecommerce");

                _policyWrap.ExecuteAsync(async () =>
                {
                    _logger.LogInformation("Creating collections...");
                    await CreateCollections();
                    _logger.LogInformation("Creating indexes...");
                    await CreateIndexes();
                }).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MongoDB.");
                throw new Exception("Failed to connect to MongoDB.", ex);
            }
        }

        private async Task CreateIndexes()
        {
            await _policyWrap.ExecuteAsync(async () =>
            {
                var productCollection = _database.GetCollection<Product>("Products");
                var productIndexKeys = Builders<Product>.IndexKeys.Ascending(p => p.BrandId).Ascending(p => p.CategoryId);
                var productIndexModel = new CreateIndexModel<Product>(productIndexKeys);
                await productCollection.Indexes.CreateOneAsync(productIndexModel);

                var orderCollection = _database.GetCollection<Order>("Orders");
                var orderIndexKeys = Builders<Order>.IndexKeys.Ascending(o => o.UserId);
                var orderIndexModel = new CreateIndexModel<Order>(orderIndexKeys);
                await orderCollection.Indexes.CreateOneAsync(orderIndexModel);
            });
        }

        private async Task CreateCollections()
        {
            await _policyWrap.ExecuteAsync(async () =>
            {
                await CreateCollection<User>("Users");
                await CreateCollection<Product>("Products");
                await CreateCollection<Brand>("Brands");
                await CreateCollection<Category>("Categories");
                await CreateCollection<Order>("Orders");
            });
        }

        private async Task CreateCollection<T>(string collectionName)
        {
            try
            {
                await _database.CreateCollectionAsync(collectionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create collection '{collectionName}'.");
                throw new Exception($"Failed to create collection '{collectionName}'.", ex);
            }
        }

        public bool IsConnectionEstablished()
        {
            try
            {
                _policyWrap.ExecuteAsync(async () =>
                {
                    await _database.Client.ListDatabaseNamesAsync();
                }).Wait();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish connection to MongoDB.");
                return false;
            }
        }

        private IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            try
            {
                return _policyWrap.ExecuteAsync(async () =>
                {
                    return await Task.FromResult(_database.GetCollection<T>(collectionName));
                }).Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve collection '{collectionName}'.");
                throw new Exception($"Failed to retrieve collection '{collectionName}'.", ex);
            }
        }

        public IMongoCollection<User> Users => GetCollection<User>("Users");
        public IMongoCollection<Product> Products => GetCollection<Product>("Products");
        public IMongoCollection<Brand> Brands => GetCollection<Brand>("Brands");
        public IMongoCollection<Category> Categories => GetCollection<Category>("Categories");
        public IMongoCollection<Order> Orders => GetCollection<Order>("Orders");

        public List<Brand> GetAllBrands()
        {
            return _policyWrap.ExecuteAsync(async () =>
            {
                return await Task.FromResult(Brands.Find(_ => true).ToList());
            }).Result;
        }

        public void InsertBrand(Brand brand)
        {
            _policyWrap.ExecuteAsync(async () =>
            {
                await Brands.InsertOneAsync(brand);
            }).Wait();
        }

        public List<Category> GetAllCategories()
        {
            return _policyWrap.ExecuteAsync(async () =>
            {
                return await Task.FromResult(Categories.Find(_ => true).ToList());
            }).Result;
        }

        public void InsertCategory(Category category)
        {
            _policyWrap.ExecuteAsync(async () =>
            {
                await Categories.InsertOneAsync(category);
            }).Wait();
        }

        public List<Product> GetAllProducts()
        {
            return _policyWrap.ExecuteAsync(async () =>
            {
                return await Task.FromResult(Products.Find(_ => true).ToList());
            }).Result;
        }

        public Brand GetBrandById(string brandId)
        {
            var objectId = new ObjectId(brandId);
            return _policyWrap.ExecuteAsync(async () =>
            {
                return await Task.FromResult(Brands.Find(b => b.BrandId == objectId).FirstOrDefault());
            }).Result;
        }
        
        public Category GetCategoryById(string categoryId)
        {
            var objectId = new ObjectId(categoryId);
            return _policyWrap.ExecuteAsync(async () =>
            {
                return await Task.FromResult(Categories.Find(c => c.CategoryId == objectId).FirstOrDefault());
            }).Result;
        }

        public void InsertProduct(Product product)
        {
            _policyWrap.ExecuteAsync(async () =>
            {
                await Products.InsertOneAsync(product);
            }).Wait();
        }

        // User Management Methods
        public async Task<bool> RegisterUser(UserRegisterDto userDto)
        {
            return await _policyWrap.ExecuteAsync(async () =>
            {
                if (await Users.Find(u => u.Email == userDto.Email).AnyAsync())
                    return false;

                var user = new User
                {
                    Username = userDto.Username,
                    Email = userDto.Email,
                    PasswordHash = HashPassword(userDto.Password),
                    Roles = new List<string> { "User" }
                };

                await Users.InsertOneAsync(user);
                return true;
            });
        }

        public async Task<User> AuthenticateUser(UserLoginDto userDto)
        {
            return await _policyWrap.ExecuteAsync(async () =>
            {
                var user = await Users.Find(u => u.Email == userDto.Email).FirstOrDefaultAsync();
                if (user == null || !VerifyPassword(userDto.Password, user.PasswordHash))
                    return null;

                return user;
            });
        }

        private string HashPassword(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                var salt = hmac.Key;
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var storedPasswordHash = parts[1];

            using (var hmac = new HMACSHA512(salt))
            {
                var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
                Console.WriteLine($"Computed hash: {computedHash}");
                Console.WriteLine($"Stored hash: {storedPasswordHash}");
                return computedHash == storedPasswordHash;
            }
        }
    }
}
