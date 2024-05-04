using System.Text.Json.Serialization;

namespace LinkedInAuth.DAL.Entities;

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
    
    [JsonPropertyName("localizedFirstName")]
    public string LocalizedFirstName { get; set; } = null!;
    
    [JsonPropertyName("profilePicture")]
    public ProfilePicture ProfilePicture { get; set; }
}