using System.Text.Json.Serialization;

namespace LinkedInAuth.DAL.Entities;

public class ProfilePicture
{
    [JsonPropertyName("displayImage")]
    public string DisplayImage { get; set; }
}