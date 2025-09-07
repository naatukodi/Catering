using Newtonsoft.Json;

namespace Catering.Api.Models;

public sealed class CatererOrderSummary
{
    [JsonProperty("orderId")] public string OrderId { get; set; } = default!;
    [JsonProperty("eventDateTime")] public DateTime EventDateTime { get; set; }
    [JsonProperty("status")] public string Status { get; set; } = default!;
    [JsonProperty("guestCount")] public int GuestCount { get; set; }

    [JsonProperty("pincode")] public string Pincode { get; set; } = default!;
    [JsonProperty("address")] public string Address { get; set; } = default!;

    [JsonProperty("packageName")] public string PackageName { get; set; } = default!;
}
