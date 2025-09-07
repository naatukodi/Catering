using Newtonsoft.Json;

namespace Catering.Api.Models;

public sealed class ServiceAreaDoc
{
    [JsonProperty("id")] public string Id { get; set; } = default!;   // $"{pincode}_{catererId}"
    [JsonProperty("type")] public string Type { get; set; } = "serviceArea";

    // PK
    [JsonProperty("pincode")] public string Pincode { get; set; } = default!;

    [JsonProperty("catererId")] public string CatererId { get; set; } = default!;

    // optional metadata used for sorting/filters
    [JsonProperty("regions")] public List<string>? Regions { get; set; }
    [JsonProperty("rank")] public int? Rank { get; set; }   // higher = show first

    [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class UpsertServiceAreaRequest
{
    public string CatererId { get; set; } = default!;
    public List<string>? Regions { get; set; }
    public int? Rank { get; set; }
}
