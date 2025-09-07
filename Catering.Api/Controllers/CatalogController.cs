using Catering.Api.DTOs;
using Catering.Api.Models;
using Catering.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Catering.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CatalogController : ControllerBase
{
    private readonly ICatalogRepository _repo;
    public CatalogController(ICatalogRepository repo) => _repo = repo;

    // ---- Caterer profile ----
    [HttpGet("{catererId}")]
    public async Task<ActionResult<CatererDoc>> GetCaterer(string catererId, CancellationToken ct)
    {
        var doc = await _repo.GetCatererAsync(catererId, ct);
        return doc is null ? NotFound() : Ok(doc);
    }

    [HttpPost("{catererId}")]
    public async Task<ActionResult> UpsertCaterer(string catererId, [FromBody] UpsertCatererRequest req, CancellationToken ct)
    {
        var model = new CatererDoc
        {
            Id = catererId,
            CatererId = catererId,
            Name = req.Name,
            LogoUrl = req.LogoUrl,
            ContactPhone = req.ContactPhone,
            GsTin = req.Gstin,
            IsVerified = req.IsVerified
        };
        await _repo.UpsertCatererAsync(model, ct);
        return NoContent();
    }

    // ---- Menu items ----
    [HttpGet("{catererId}/menuitems")]
    public async Task<ActionResult<PagedResult<MenuItemDoc>>> ListMenuItems(
        string catererId,
        [FromQuery] string? category,
        [FromQuery] string? vegType,
        [FromQuery] bool? onlyActive,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? continuationToken = null,
        CancellationToken ct = default)
    {
        var page = await _repo.ListMenuItemsAsync(catererId, category, vegType, onlyActive, pageSize, continuationToken, ct);
        return Ok(page);
    }

    [HttpPost("{catererId}/menuitems")]
    public async Task<ActionResult<object>> CreateMenuItem(string catererId, [FromBody] CreateMenuItemRequest req, CancellationToken ct)
    {
        var doc = new MenuItemDoc
        {
            CatererId = catererId,
            Name = req.Name,
            VegType = req.VegType,
            Category = req.Category,
            Unit = req.Unit,
            BaseCost = req.BaseCost,
            Tags = req.Tags
        };
        var created = await _repo.CreateMenuItemAsync(catererId, doc, ct);
        return CreatedAtAction(nameof(ListMenuItems), new { catererId }, new { id = created.Id });
    }

    // ---- Packages ----
    [HttpGet("{catererId}/packages")]
    public async Task<ActionResult<PagedResult<PackageDoc>>> ListPackages(
        string catererId,
        [FromQuery] bool? onlyActive,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? continuationToken = null,
        CancellationToken ct = default)
    {
        var page = await _repo.ListPackagesAsync(catererId, onlyActive, pageSize, continuationToken, ct);
        return Ok(page);
    }

    [HttpPost("{catererId}/packages")]
    public async Task<ActionResult<object>> CreatePackage(string catererId, [FromBody] CreatePackageRequest req, CancellationToken ct)
    {
        var pkg = new PackageDoc
        {
            CatererId = catererId,
            Name = req.Name,
            Description = req.Description,
            PerPlatePrice = req.PerPlatePrice,
            VegOnly = req.VegOnly,
            Items = req.Items.Select(i => new PackageItem { MenuItemId = i.MenuItemId, QtyPerPlate = i.QtyPerPlate }).ToList(),
            IsActive = req.IsActive
        };
        var created = await _repo.CreatePackageAsync(catererId, pkg, ct);
        return CreatedAtAction(nameof(ListPackages), new { catererId }, new { id = created.Id });
    }
}
