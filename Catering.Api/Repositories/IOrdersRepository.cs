using Catering.Api.Models;

namespace Catering.Api.Repositories;

public interface IOrdersRepository
{
    Task<OrderDoc?> GetAsync(string orderId, CancellationToken ct = default);
    Task CreateAsync(OrderDoc order, CancellationToken ct = default);
    Task UpdateStatusAsync(string orderId, string newStatus, CancellationToken ct = default);
    Task<PagedResult<OrderDoc>> ListByCustomerAsync(string customerUserId, int pageSize = 20, string? continuation = null, CancellationToken ct = default);
}
