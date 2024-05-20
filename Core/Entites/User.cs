using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Entites;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; } // Store hashed passwords
    public List<string> Roles { get; set; } // For authorization
}