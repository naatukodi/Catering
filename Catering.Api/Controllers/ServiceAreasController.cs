using Catering.Api.DTOs;
using Catering.Api.Models;
using Catering.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Catering.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ServiceAreasController : ControllerBase
{
    private readonly IServiceAreasRepository _repo;
    public ServiceAreasController(IServiceAreasRepository repo) => _repo = repo;

    // List caterers for a pincode (single-partition read)
    [HttpGet("{pincode}")]
    public async Task<ActionResult<PagedResult<ServiceAreaDoc>>> ListForPincode(
        string pincode,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? continuationToken = null,
        CancellationToken ct = default)
    {
        var page = await _repo.ListByPincodeAsync(pincode, pageSize, continuationToken, ct);
        return Ok(page);
    }

    // Upsert one mapping pincode -> caterer
    [HttpPost("{pincode}")]
    public async Task<ActionResult> Upsert(string pincode, [FromBody] UpsertServiceAreaRequest req, CancellationToken ct)
    {
        var doc = new ServiceAreaDoc
        {
            Pincode = pincode,
            CatererId = req.CatererId,
            Regions = req.Regions,
            Rank = req.Rank
        };
        await _repo.UpsertAsync(doc, ct);
        return NoContent();
    }

    // Delete mapping
    [HttpDelete("{pincode}/{catererId}")]
    public async Task<ActionResult> Delete(string pincode, string catererId, CancellationToken ct)
    {
        await _repo.DeleteAsync(pincode, catererId, ct);
        return NoContent();
    }

    // Helper: list pincodes that a caterer covers (cross-partition)
    [HttpGet("by-caterer/{catererId}")]
    public async Task<ActionResult<PagedResult<string>>> ListPincodesForCaterer(
        string catererId,
        [FromQuery] int pageSize = 100,
        [FromQuery] string? continuationToken = null,
        CancellationToken ct = default)
    {
        var page = await _repo.ListPincodesForCatererAsync(catererId, pageSize, continuationToken, ct);
        return Ok(page);
    }
}
