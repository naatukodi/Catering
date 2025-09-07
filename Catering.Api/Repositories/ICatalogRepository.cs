using Catering.Api.Models;

namespace Catering.Api.Repositories;

public interface ICatalogRepository
{
    // Caterer
    Task<CatererDoc?> GetCatererAsync(string catererId, CancellationToken ct = default);
    Task UpsertCatererAsync(CatererDoc caterer, CancellationToken ct = default);

    // Menu items
    Task<PagedResult<MenuItemDoc>> ListMenuItemsAsync(string catererId, string? category = null, string? vegType = null, bool? onlyActive = null, int pageSize = 50, string? continuation = null, CancellationToken ct = default);
    Task<MenuItemDoc> CreateMenuItemAsync(string catererId, MenuItemDoc item, CancellationToken ct = default);

    // Packages
    Task<PagedResult<PackageDoc>> ListPackagesAsync(string catererId, bool? onlyActive = null, int pageSize = 50, string? continuation = null, CancellationToken ct = default);
    Task<PackageDoc> CreatePackageAsync(string catererId, PackageDoc pkg, CancellationToken ct = default);
}
