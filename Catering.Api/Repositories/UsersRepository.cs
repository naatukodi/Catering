using Catering.Api.Models;
using Microsoft.Azure.Cosmos;

namespace Catering.Api.Repositories;

public sealed class UsersRepository : IUsersRepository
{
    private readonly Container _container;

    public UsersRepository(CosmosClient client, IConfiguration cfg)
    {
        var dbId = cfg["Cosmos:DatabaseId"]!;
        var cId = cfg["Cosmos:UsersContainerId"]!;
        _container = client.GetContainer(dbId, cId);
    }

    public async Task<UserDoc?> GetAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _container.ReadItemAsync<UserDoc>(userId, new PartitionKey(userId), cancellationToken: ct);
            return resp.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public static async Task EnsureContainerExistsAsync(CosmosClient client, IConfiguration cfg)
    {
        var dbId = cfg["Cosmos:DatabaseId"]!;
        var cId = cfg["Cosmos:UsersContainerId"]!;

        var dbResponse = await client.CreateDatabaseIfNotExistsAsync(dbId);
        var db = dbResponse.Database;

        var containerProperties = new ContainerProperties(cId, partitionKeyPath: "/userId")
        {
            // Add indexing policy, unique keys, etc. if needed
        };

        await db.CreateContainerIfNotExistsAsync(containerProperties);
    }

    public async Task<UserDoc> CreateAsync(UserDoc user, CancellationToken ct = default)
    {
        EnsureContainerExistsAsync(_container.Database.Client, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()).GetAwaiter().GetResult();
        user.Id = user.UserId; // keep id == userId for simple lookups
        user.Email = user.Email.ToLowerInvariant();

        var resp = await _container.CreateItemAsync(user, new PartitionKey(user.UserId), cancellationToken: ct);
        return resp.Resource;
    }

    public async Task UpsertAsync(UserDoc user, CancellationToken ct = default)
    {
        EnsureContainerExistsAsync(_container.Database.Client, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()).GetAwaiter().GetResult();

        user.Id = user.UserId;
        if (!string.IsNullOrEmpty(user.Email)) user.Email = user.Email.ToLowerInvariant();

        await _container.UpsertItemAsync(user, new PartitionKey(user.UserId), cancellationToken: ct);
    }

    public async Task<UserDoc?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        EnsureContainerExistsAsync(_container.Database.Client, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()).GetAwaiter().GetResult();

        var q = new QueryDefinition(
            "SELECT TOP 1 * FROM c WHERE c.type='user' AND c.email=@e")
            .WithParameter("@e", email.ToLowerInvariant());

        var it = _container.GetItemQueryIterator<UserDoc>(q, requestOptions: new QueryRequestOptions { MaxItemCount = 1 });
        if (!it.HasMoreResults) return null;
        var page = await it.ReadNextAsync(ct);
        return page.Resource.FirstOrDefault();
    }

    public async Task<UserDoc?> GetByPhoneAsync(string phone, CancellationToken ct = default)
    {
        EnsureContainerExistsAsync(_container.Database.Client, new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()).GetAwaiter().GetResult();

        var q = new QueryDefinition(
            "SELECT TOP 1 * FROM c WHERE c.type='user' AND c.phone=@p")
            .WithParameter("@p", phone);

        var it = _container.GetItemQueryIterator<UserDoc>(q, requestOptions: new QueryRequestOptions { MaxItemCount = 1 });
        if (!it.HasMoreResults) return null;
        var page = await it.ReadNextAsync(ct);
        return page.Resource.FirstOrDefault();
    }

    public async Task<PagedResult<UserDoc>> ListByRoleAsync(string role, int pageSize = 50, string? continuation = null, CancellationToken ct = default)
    {
        var q = new QueryDefinition(
            "SELECT * FROM c WHERE c.type='user' AND c.role=@r ORDER BY c.createdAt DESC")
            .WithParameter("@r", role);

        var it = _container.GetItemQueryIterator<UserDoc>(q, continuation, new QueryRequestOptions { MaxItemCount = pageSize });
        var page = await it.ReadNextAsync(ct);
        return new PagedResult<UserDoc>(page.Resource.ToList(), page.ContinuationToken);
    }
}
