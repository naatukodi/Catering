using Catering.Api.Models;
using Microsoft.Azure.Cosmos;

namespace Catering.Api.Repositories;

public sealed class CatalogRepository : ICatalogRepository
{
    private readonly Container _container;

    public CatalogRepository(CosmosClient client, IConfiguration cfg)
    {
        var dbId = cfg["Cosmos:DatabaseId"]!;
        var cId = cfg["Cosmos:CatalogContainerId"]!;
        _container = client.GetContainer(dbId, cId);
    }

    public static async Task EnsureDbAndContainerAsync(CosmosClient client, IConfiguration cfg)
    {
        var dbId = cfg["Cosmos:DatabaseId"]!;
        var cId = cfg["Cosmos:CatalogContainerId"]!;
        var dbResponse = await client.CreateDatabaseIfNotExistsAsync(dbId);
        var db = dbResponse.Database;

        var containerProperties = new ContainerProperties(cId, partitionKeyPath: "/catererId");
        await db.CreateContainerIfNotExistsAsync(containerProperties);
    }

    // ---------- Caterer ----------
    public async Task<CatererDoc?> GetCatererAsync(string catererId, CancellationToken ct = default)
    {
        // id == catererId for the caterer profile
        try
        {
            var resp = await _container.ReadItemAsync<CatererDoc>(catererId, new PartitionKey(catererId), cancellationToken: ct);
            return resp.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task UpsertCatererAsync(CatererDoc caterer, CancellationToken ct = default)
    {
        EnsureDbAndContainerAsync(_container.Database.Client, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()).GetAwaiter().GetResult();
        if (string.IsNullOrWhiteSpace(caterer.Id)) caterer.Id = caterer.CatererId;
        caterer.Type = "caterer";
        await _container.UpsertItemAsync(caterer, new PartitionKey(caterer.CatererId), cancellationToken: ct);
    }

    // ---------- Menu Items ----------
    public async Task<PagedResult<MenuItemDoc>> ListMenuItemsAsync(string catererId, string? category = null, string? vegType = null, bool? onlyActive = null, int pageSize = 50, string? continuation = null, CancellationToken ct = default)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("SELECT * FROM c WHERE c.type='menuItem' ");
        sb.Append("AND c.catererId = @cid ");

        if (!string.IsNullOrEmpty(category)) sb.Append("AND c.category = @cat ");
        if (!string.IsNullOrEmpty(vegType)) sb.Append("AND c.vegType = @veg ");
        if (onlyActive == true) sb.Append("AND c.isActive = true ");

        sb.Append("ORDER BY c.createdAt DESC");

        var q = new QueryDefinition(sb.ToString())
            .WithParameter("@cid", catererId);
        if (!string.IsNullOrEmpty(category)) q = q.WithParameter("@cat", category);
        if (!string.IsNullOrEmpty(vegType)) q = q.WithParameter("@veg", vegType);

        var opts = new QueryRequestOptions { PartitionKey = new PartitionKey(catererId), MaxItemCount = pageSize };
        var it = _container.GetItemQueryIterator<MenuItemDoc>(q, continuation, opts);

        var page = await it.ReadNextAsync(ct);
        return new PagedResult<MenuItemDoc>(page.Resource.ToList(), page.ContinuationToken);
    }

    public async Task<MenuItemDoc> CreateMenuItemAsync(string catererId, MenuItemDoc item, CancellationToken ct = default)
    {
        item.CatererId = catererId;
        item.Type = "menuItem";
        item.Id ??= $"MI_{Guid.NewGuid():N}";

        var resp = await _container.CreateItemAsync(item, new PartitionKey(catererId), cancellationToken: ct);
        return resp.Resource;
    }

    // ---------- Packages ----------
    public async Task<PagedResult<PackageDoc>> ListPackagesAsync(string catererId, bool? onlyActive = null, int pageSize = 50, string? continuation = null, CancellationToken ct = default)
    {
        var sql = "SELECT * FROM c WHERE c.type='package' AND c.catererId=@cid "
                + (onlyActive == true ? "AND c.isActive=true " : "")
                + "ORDER BY c.createdAt DESC";

        var q = new QueryDefinition(sql).WithParameter("@cid", catererId);
        var opts = new QueryRequestOptions { PartitionKey = new PartitionKey(catererId), MaxItemCount = pageSize };
        var it = _container.GetItemQueryIterator<PackageDoc>(q, continuation, opts);

        var page = await it.ReadNextAsync(ct);
        return new PagedResult<PackageDoc>(page.Resource.ToList(), page.ContinuationToken);
    }

    public async Task<PackageDoc> CreatePackageAsync(string catererId, PackageDoc pkg, CancellationToken ct = default)
    {
        pkg.CatererId = catererId;
        pkg.Type = "package";
        pkg.Id ??= $"PKG_{Guid.NewGuid():N}";

        var resp = await _container.CreateItemAsync(pkg, new PartitionKey(catererId), cancellationToken: ct);
        return resp.Resource;
    }
}
