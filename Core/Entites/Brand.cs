using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Entites;

public class Brand
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId BrandId { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
}