using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Catering.Api.Models;

public sealed class OrderDoc
{
    [JsonProperty("id")]
    public string Id { get; set; } = default!;

    [JsonProperty("type")]
    public string Type { get; set; } = "order";

    // MUST match container PK path exactly: /orderId
    [JsonProperty("orderId")]
    public string OrderId { get; set; } = default!;

    [JsonProperty("catererId")]
    public string CatererId { get; set; } = default!;

    [JsonProperty("status")]
    public string Status { get; set; } = "Pending";

    [JsonProperty("customer")]
    public OrderCustomer Customer { get; set; } = default!;

    [JsonProperty("eventDateTime")]
    public DateTime EventDateTime { get; set; }

    [JsonProperty("guestCount")]
    public int GuestCount { get; set; }

    [JsonProperty("location")]
    public EventLocation Location { get; set; } = default!;

    [JsonProperty("package")]
    public OrderPackage Package { get; set; } = default!;

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("lastUpdatedAt")]
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class OrderCustomer
{
    [JsonProperty("userId")] public string UserId { get; set; } = default!;
    [JsonProperty("name")] public string Name { get; set; } = default!;
    [JsonProperty("phone")] public string Phone { get; set; } = default!;
}

public sealed class EventLocation
{
    [JsonProperty("pincode")] public string Pincode { get; set; } = default!;
    [JsonProperty("address")] public string Address { get; set; } = default!;
}

public sealed class OrderPackage
{
    [JsonProperty("packageId")] public string PackageId { get; set; } = default!;
    [JsonProperty("name")] public string Name { get; set; } = default!;
    [JsonProperty("perPlatePrice")] public decimal PerPlatePrice { get; set; }
}

// Simple paging wrapper (optional)
public sealed record PagedResult<T>(IReadOnlyList<T> Items, string? ContinuationToken);
