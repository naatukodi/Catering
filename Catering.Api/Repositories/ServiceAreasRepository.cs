using Catering.Api.Models;
using Microsoft.Azure.Cosmos;

namespace Catering.Api.Repositories;

public sealed class ServiceAreasRepository : IServiceAreasRepository
{
    private readonly Container _container;

    public ServiceAreasRepository(CosmosClient client, IConfiguration cfg)
    {
        var dbId = cfg["Cosmos:DatabaseId"]!;
        var cId = cfg["Cosmos:ServiceAreasContainerId"]!;
        _container = client.GetContainer(dbId, cId);
    }

    private static string ComposeId(string pincode, string catererId) => $"{pincode}_{catererId}";

    public async Task<ServiceAreaDoc?> GetAsync(string pincode, string catererId, CancellationToken ct = default)
    {
        var id = ComposeId(pincode, catererId);
        try
        {
            var resp = await _container.ReadItemAsync<ServiceAreaDoc>(id, new PartitionKey(pincode), cancellationToken: ct);
            return resp.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public static async Task EnsureContainerExistsAsync(CosmosClient client, IConfiguration cfg, CancellationToken ct = default)
    {
        var dbId = cfg["Cosmos:DatabaseId"]!;
        var svcId = cfg["Cosmos:ServiceAreasContainerId"]!;

        var dbResponse = await client.CreateDatabaseIfNotExistsAsync(dbId, cancellationToken: ct);

        var svcProps = new ContainerProperties(svcId, "/pincode")
        {
            IndexingPolicy = new IndexingPolicy
            {
                Automatic = true,
                IndexingMode = IndexingMode.Consistent,
                CompositeIndexes =
                {
                    new System.Collections.ObjectModel.Collection<CompositePath>
                    {
                        new CompositePath { Path = "/rank",      Order = CompositePathSortOrder.Descending },
                        new CompositePath { Path = "/createdAt", Order = CompositePathSortOrder.Descending }
                    }
                }
            }
        };

        await dbResponse.Database.CreateContainerIfNotExistsAsync(svcProps, cancellationToken: ct);
    }

    public async Task UpsertAsync(ServiceAreaDoc doc, CancellationToken ct = default)
    {
        EnsureContainerExistsAsync(_container.Database.Client, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()).GetAwaiter().GetResult();
        doc.Id = ComposeId(doc.Pincode, doc.CatererId);
        doc.Type = "serviceArea";
        await _container.UpsertItemAsync(doc, new PartitionKey(doc.Pincode), cancellationToken: ct);
    }

    public async Task DeleteAsync(string pincode, string catererId, CancellationToken ct = default)
    {
        var id = ComposeId(pincode, catererId);
        await _container.DeleteItemAsync<ServiceAreaDoc>(id, new PartitionKey(pincode), cancellationToken: ct);
    }

    public async Task<PagedResult<ServiceAreaDoc>> ListByPincodeAsync(
        string pincode, int pageSize = 50, string? continuation = null, CancellationToken ct = default)
    {
        var q = new QueryDefinition(
            "SELECT * FROM c WHERE c.type='serviceArea' AND c.pincode=@pc ORDER BY c.rank DESC, c.createdAt DESC")
            .WithParameter("@pc", pincode);

        var it = _container.GetItemQueryIterator<ServiceAreaDoc>(
            q, continuation, new QueryRequestOptions { PartitionKey = new PartitionKey(pincode), MaxItemCount = pageSize });

        var page = await it.ReadNextAsync(ct);
        return new PagedResult<ServiceAreaDoc>(page.Resource.ToList(), page.ContinuationToken);
    }

    // cross-partition helper: all pincodes a caterer serves
    public async Task<PagedResult<string>> ListPincodesForCatererAsync(
        string catererId, int pageSize = 100, string? continuation = null, CancellationToken ct = default)
    {
        var q = new QueryDefinition(
           "SELECT VALUE c.pincode FROM c WHERE c.type='serviceArea' AND c.catererId=@cid ORDER BY c.pincode")
            .WithParameter("@cid", catererId); ;

        var it = _container.GetItemQueryIterator<string>(q, continuation, new QueryRequestOptions { MaxItemCount = pageSize });
        var page = await it.ReadNextAsync(ct);
        return new PagedResult<string>(page.Resource.ToList(), page.ContinuationToken);
    }
}
