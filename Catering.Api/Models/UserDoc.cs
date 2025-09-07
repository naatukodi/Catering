using Newtonsoft.Json;

namespace Catering.Api.Models;

public sealed class UserDoc
{
    [JsonProperty("id")] public string Id { get; set; } = default!;
    [JsonProperty("type")] public string Type { get; set; } = "user";

    // PK: must be present with the exact name /userId
    [JsonProperty("userId")] public string UserId { get; set; } = default!;

    [JsonProperty("role")] public string Role { get; set; } = "Customer"; // Customer|Caterer|Admin
    [JsonProperty("catererId")] public string? CatererId { get; set; }

    [JsonProperty("name")] public string Name { get; set; } = default!;
    [JsonProperty("email")] public string Email { get; set; } = default!;   // store lower-cased
    [JsonProperty("phone")] public string Phone { get; set; } = default!;

    [JsonProperty("status")] public string Status { get; set; } = "Active";  // Active|Blocked
    [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [JsonProperty("lastLoginAt")] public DateTime? LastLoginAt { get; set; }
}
