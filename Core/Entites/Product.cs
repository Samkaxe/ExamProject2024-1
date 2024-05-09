using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Entites;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string BrandId { get; set; }  
    public string CategoryId { get; set; }  

    // Optional: Navigational properties
    [BsonIgnore]
    public Brand Brand { get; set; }
    [BsonIgnore]
    public Category Category { get; set; }
}