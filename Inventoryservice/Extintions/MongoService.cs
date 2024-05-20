using System.Security.Cryptography;
using System.Text;
using Core.Dtos;
using Core.Entites;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace InventoryService.Extintions;
    public class MongoService
    {
        private readonly IMongoDatabase _database;

        public MongoService(IConfiguration configuration)
        {
            try
            {
                var client = new MongoClient(configuration["MongoSettings:ConnectionString"]);
                _database = client.GetDatabase("ecommerce"); // Hardcoded database name

                // Create collections if they don't exist
                CreateCollections();
                CreateIndexes();
               // var mongoService = new MongoService(configuration);
               // mongoService.InsertMockData("mockData.json");
               
             //  CreateUsersWithRoles();
            }
            catch (Exception ex)
            {
                // Handle connection error
                throw new Exception("Failed to connect to MongoDB.", ex);
            }
        }
        
       
        //In the Product collection, BrandId and CategoryId are potential candidates for indexing,
        //as they are likely to be used in queries to retrieve products by brand or category.
        //In the Order collection, UserId might be a field commonly used for filtering orders by user.
        private void CreateIndexes()
        {
            // Index for Product collection
            /*
             * Here, we're obtaining a reference to the "Products" collection
             * in the MongoDB database _database. Similarly, we get a reference to the "Orders" collection for indexing orders.
             */
            var productCollection = _database.GetCollection<Product>("Products");
            /*
             * We use Builders<Product>.IndexKeys to construct index key definitions for the Product collection.
             * In this case, we're specifying a compound index with ascending sorting on the BrandId and CategoryId fields.
             * This means MongoDB will create an index that orders documents based on both BrandId and CategoryId.
             */
            var productIndexKeys = Builders<Product>.IndexKeys.Ascending(p => p.BrandId).Ascending(p => p.CategoryId);
            var productIndexModel = new CreateIndexModel<Product>(productIndexKeys);
            /*
             * We create an index model for the Product collection using the defined index keys.
             * An index model encapsulates all the information needed to create an index,
             * including the keys and any additional options.
             */
            productCollection.Indexes.CreateOne(productIndexModel);

            // Index for Order collection
            var orderCollection = _database.GetCollection<Order>("Orders");
            var orderIndexKeys = Builders<Order>.IndexKeys.Ascending(o => o.UserId);
            var orderIndexModel = new CreateIndexModel<Order>(orderIndexKeys);
            /*
             * These indexes help improve query performance when filtering, sorting, or aggregating data based on the indexed fields.
             */
            orderCollection.Indexes.CreateOne(orderIndexModel);
        }

        private void CreateCollections()
        {
            CreateCollection<User>("Users");
            CreateCollection<Product>("Products");
            CreateCollection<Brand>("Brands");
            CreateCollection<Category>("Categories");
            CreateCollection<Order>("Orders");
            
            
        }
        
       
        private void CreateCollection<T>(string collectionName)
        {
            try
            {
                _database.CreateCollection(collectionName);
            }
            catch (Exception ex)
            {
              
                throw new Exception($"Failed to create collection '{collectionName}'.", ex);
            }
        }

        public bool IsConnectionEstablished()
        {
            try
            {
               
                _database.Client.ListDatabaseNames();
                return true;
            }
            catch (Exception)
            {
             
                return false;
            }
        }

    
        private IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            try
            {
                return _database.GetCollection<T>(collectionName);
            }
            catch (Exception ex)
            {
               
                throw new Exception($"Failed to retrieve collection '{collectionName}'.", ex);
            }
        }

        public IMongoCollection<User> Users => GetCollection<User>("Users");
        public IMongoCollection<Product> Products => GetCollection<Product>("Products");
        public IMongoCollection<Brand> Brands => GetCollection<Brand>("Brands");
        public IMongoCollection<Category> Categories => GetCollection<Category>("Categories");
        public IMongoCollection<Order> Orders => GetCollection<Order>("Orders");
        
       // security 
       // User Management Methods
       public async Task<bool> RegisterUser(UserRegisterDto userDto)
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
       }

       public async Task<User> AuthenticateUser(UserLoginDto userDto)
       {
           var user = await Users.Find(u => u.Email == userDto.Email).FirstOrDefaultAsync();
           if (user == null || !VerifyPassword(userDto.Password, user.PasswordHash))
               return null;

           return user;
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
               var computedHash = Convert.ToBase64String(hmac
                   .ComputeHash(Encoding.UTF8.GetBytes(password)));
               Console.WriteLine($"Computed hash: {computedHash}");
               Console.WriteLine($"Stored hash: {storedPasswordHash}");
               return computedHash == storedPasswordHash;
           }
       }
    }

    