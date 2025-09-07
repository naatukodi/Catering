namespace Catering.Api.DTOs;

public sealed class UpsertCatererRequest
{
    public string CatererId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? LogoUrl { get; set; }
    public string? ContactPhone { get; set; }
    public string? Gstin { get; set; }
    public bool IsVerified { get; set; }
}

public sealed class CreateMenuItemRequest
{
    public string Name { get; set; } = default!;
    public string VegType { get; set; } = "Veg";
    public string Category { get; set; } = "Main";
    public string Unit { get; set; } = "plate";
    public decimal BaseCost { get; set; }
    public List<string>? Tags { get; set; }
}

public sealed class CreatePackageRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal PerPlatePrice { get; set; }
    public bool VegOnly { get; set; }
    public List<PackageItemDto> Items { get; set; } = new();
    public bool IsActive { get; set; } = true;
}
public sealed class PackageItemDto { public string MenuItemId { get; set; } = default!; public decimal QtyPerPlate { get; set; } }
