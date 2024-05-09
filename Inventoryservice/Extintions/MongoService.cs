using Core.Entites;
using MongoDB.Driver;

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
            }
            catch (Exception ex)
            {
                // Handle connection error
                throw new Exception("Failed to connect to MongoDB.", ex);
            }
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
                // Handle collection creation error
                throw new Exception($"Failed to create collection '{collectionName}'.", ex);
            }
        }

        public bool IsConnectionEstablished()
        {
            try
            {
                // Try to list the databases in the MongoDB server
                _database.Client.ListDatabaseNames();
                return true;
            }
            catch (Exception)
            {
                // If an exception is thrown, the connection is not established
                return false;
            }
        }

        // Add error handling for MongoDB collection retrieval
        private IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            try
            {
                return _database.GetCollection<T>(collectionName);
            }
            catch (Exception ex)
            {
                // Handle collection retrieval error
                throw new Exception($"Failed to retrieve collection '{collectionName}'.", ex);
            }
        }

        public IMongoCollection<User> Users => GetCollection<User>("Users");
        public IMongoCollection<Product> Products => GetCollection<Product>("Products");
        public IMongoCollection<Brand> Brands => GetCollection<Brand>("Brands");
        public IMongoCollection<Category> Categories => GetCollection<Category>("Categories");
        public IMongoCollection<Order> Orders => GetCollection<Order>("Orders");
    }