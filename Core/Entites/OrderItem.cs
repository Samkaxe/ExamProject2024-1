using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Entites;

public class OrderItem
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProductId { get; set; }
    
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}