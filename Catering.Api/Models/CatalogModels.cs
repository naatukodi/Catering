using Newtonsoft.Json;

namespace Catering.Api.Models;

// One caterer profile (stored in the same container, pk=/catererId)
public sealed class CatererDoc
{
    [JsonProperty("id")] public string Id { get; set; } = default!;
    [JsonProperty("type")] public string Type { get; set; } = "caterer";
    [JsonProperty("catererId")] public string CatererId { get; set; } = default!; // PK
    [JsonProperty("name")] public string Name { get; set; } = default!;
    [JsonProperty("logoUrl")] public string? LogoUrl { get; set; }
    [JsonProperty("contactPhone")] public string? ContactPhone { get; set; }
    [JsonProperty("gstin")] public string? GsTin { get; set; }
    [JsonProperty("isVerified")] public bool IsVerified { get; set; }
    [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Menu item
public sealed class MenuItemDoc
{
    [JsonProperty("id")] public string Id { get; set; } = default!;
    [JsonProperty("type")] public string Type { get; set; } = "menuItem";
    [JsonProperty("catererId")] public string CatererId { get; set; } = default!; // PK
    [JsonProperty("name")] public string Name { get; set; } = default!;
    [JsonProperty("vegType")] public string VegType { get; set; } = "Veg"; // Veg|NonVeg|Egg
    [JsonProperty("category")] public string Category { get; set; } = "Main"; // Starter|Main|Dessert|Beverage|LiveCounter
    [JsonProperty("unit")] public string Unit { get; set; } = "plate";
    [JsonProperty("baseCost")] public decimal BaseCost { get; set; }
    [JsonProperty("tags")] public List<string>? Tags { get; set; }
    [JsonProperty("isActive")] public bool IsActive { get; set; } = true;
    [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class PackageItem
{
    [JsonProperty("menuItemId")] public string MenuItemId { get; set; } = default!;
    [JsonProperty("qtyPerPlate")] public decimal QtyPerPlate { get; set; }
}

public sealed class PackageDoc
{
    [JsonProperty("id")] public string Id { get; set; } = default!;
    [JsonProperty("type")] public string Type { get; set; } = "package";
    [JsonProperty("catererId")] public string CatererId { get; set; } = default!; // PK
    [JsonProperty("name")] public string Name { get; set; } = default!;
    [JsonProperty("description")] public string? Description { get; set; }
    [JsonProperty("perPlatePrice")] public decimal PerPlatePrice { get; set; }
    [JsonProperty("vegOnly")] public bool VegOnly { get; set; }
    [JsonProperty("items")] public List<PackageItem> Items { get; set; } = new();
    [JsonProperty("isActive")] public bool IsActive { get; set; } = true;
    [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}