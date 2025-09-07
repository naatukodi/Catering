using Catering.Api.Models;
using Microsoft.Azure.Cosmos;

namespace Catering.Api.Repositories;

public sealed class OrdersRepository : IOrdersRepository
{
    private readonly Container _container;

    public OrdersRepository(CosmosClient client, IConfiguration cfg)
    {
        var dbId = cfg["Cosmos:DatabaseId"]!;
        var cId = cfg["Cosmos:OrdersContainerId"]!;
        _container = client.GetContainer(dbId, cId);
    }

    public async Task<OrderDoc?> GetAsync(string orderId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _container.ReadItemAsync<OrderDoc>(orderId, new PartitionKey(orderId), cancellationToken: ct);
            return resp.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task CreateAsync(OrderDoc order, CancellationToken ct = default)
    {
        EnsureContainerExistsAsync(_container.Database.Client, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()).GetAwaiter().GetResult();
        order.Id = order.OrderId;      // keep id == orderId
        order.CreatedAt = DateTime.UtcNow;
        order.LastUpdatedAt = order.CreatedAt;

        await _container.CreateItemAsync(order, new PartitionKey(order.OrderId), cancellationToken: ct);
    }
    public static async Task EnsureContainerExistsAsync(CosmosClient client, IConfiguration cfg)
    {
        var dbId = cfg["Cosmos:DatabaseId"]!;
        var cId = cfg["Cosmos:OrdersContainerId"]!;

        var dbResponse = await client.CreateDatabaseIfNotExistsAsync(dbId);
        var db = dbResponse.Database;

        var containerProperties = new ContainerProperties(cId, partitionKeyPath: "/orderId")
        {
            // Add indexing policy, unique keys, etc. if needed
        };

        await db.CreateContainerIfNotExistsAsync(containerProperties);
    }
    public async Task UpdateStatusAsync(string orderId, string newStatus, CancellationToken ct = default)
    {
        // Patch avoids full doc RU cost
        var ops = new[]
        {
            PatchOperation.Replace("/status", newStatus),
            PatchOperation.Replace("/lastUpdatedAt", DateTime.UtcNow)
        };

        await _container.PatchItemAsync<OrderDoc>(
            id: orderId,
            partitionKey: new PartitionKey(orderId),
            patchOperations: ops,
            cancellationToken: ct
        );
    }

    public async Task<PagedResult<OrderDoc>> ListByCustomerAsync(string customerUserId, int pageSize = 20, string? continuation = null, CancellationToken ct = default)
    {
        // Cross-partition query (fine for per-user pages); read-only & selective
        var query = new QueryDefinition(
            @"SELECT * FROM c 
              WHERE c.type = 'order' AND c.customer.userId = @uid
              ORDER BY c.createdAt DESC")
            .WithParameter("@uid", customerUserId);

        var itr = _container.GetItemQueryIterator<OrderDoc>(
            query,
            continuationToken: continuation,
            requestOptions: new QueryRequestOptions { MaxItemCount = pageSize, EnableLowPrecisionOrderBy = false });

        if (!itr.HasMoreResults) return new PagedResult<OrderDoc>(Array.Empty<OrderDoc>(), null);

        var page = await itr.ReadNextAsync(ct);
        return new PagedResult<OrderDoc>(page.Resource.ToList(), page.ContinuationToken);
    }

    public async Task<PagedResult<Catering.Api.Models.CatererOrderSummary>> ListByCatererAndDayAsync(
    string catererId,
    DateTime dayUtc,
    int pageSize = 50,
    string? continuation = null,
    CancellationToken ct = default)
    {
        // Normalize to day boundaries in UTC
        var from = new DateTime(dayUtc.Year, dayUtc.Month, dayUtc.Day, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddDays(1);

        // Project only needed fields to keep RU low
        var q = new QueryDefinition(@"
            SELECT 
                c.orderId       AS orderId,
                c.eventDateTime AS eventDateTime,
                c.status        AS status,
                c.guestCount    AS guestCount,
                c.location.pincode AS pincode,
                c.location.address AS address,
                c.package.name  AS packageName
            FROM c
            WHERE c.type = 'order'
            AND c.catererId = @cid
            AND c.eventDateTime >= @from
            AND c.eventDateTime <  @to
            ORDER BY c.eventDateTime ASC")
            .WithParameter("@cid", catererId)
            .WithParameter("@from", from)
            .WithParameter("@to", to);

        var opts = new QueryRequestOptions { MaxItemCount = pageSize };
        var it = _container.GetItemQueryIterator<CatererOrderSummary>(q, continuation, opts);

        if (!it.HasMoreResults) return new PagedResult<CatererOrderSummary>(Array.Empty<CatererOrderSummary>(), null);

        var page = await it.ReadNextAsync(ct);
        return new PagedResult<CatererOrderSummary>(page.Resource.ToList(), page.ContinuationToken);
    }
}
