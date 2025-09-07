using Catering.Api.Models;

namespace Catering.Api.Repositories;

public interface IServiceAreasRepository
{
    Task<ServiceAreaDoc?> GetAsync(string pincode, string catererId, CancellationToken ct = default);
    Task UpsertAsync(ServiceAreaDoc doc, CancellationToken ct = default);
    Task DeleteAsync(string pincode, string catererId, CancellationToken ct = default);

    Task<PagedResult<ServiceAreaDoc>> ListByPincodeAsync(
        string pincode, int pageSize = 50, string? continuation = null, CancellationToken ct = default);

    Task<PagedResult<string>> ListPincodesForCatererAsync(
        string catererId, int pageSize = 100, string? continuation = null, CancellationToken ct = default);
}
