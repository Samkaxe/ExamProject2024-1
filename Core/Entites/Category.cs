using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Entites;

public class Category
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId CategoryId { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
}