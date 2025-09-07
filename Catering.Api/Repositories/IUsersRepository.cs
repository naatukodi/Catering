using Catering.Api.Models;

namespace Catering.Api.Repositories;

public interface IUsersRepository
{
    Task<UserDoc?> GetAsync(string userId, CancellationToken ct = default);
    Task<UserDoc> CreateAsync(UserDoc user, CancellationToken ct = default);
    Task UpsertAsync(UserDoc user, CancellationToken ct = default);

    Task<UserDoc?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<UserDoc?> GetByPhoneAsync(string phone, CancellationToken ct = default);

    Task<PagedResult<UserDoc>> ListByRoleAsync(string role, int pageSize = 50, string? continuation = null, CancellationToken ct = default);
}
