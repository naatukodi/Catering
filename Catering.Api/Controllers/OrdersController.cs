using Catering.Api.DTOs;
using Catering.Api.Models;
using Catering.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Catering.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrdersRepository _repo;

    public OrdersController(IOrdersRepository repo) => _repo = repo;

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderDoc>> GetById(string orderId, CancellationToken ct)
    {
        var doc = await _repo.GetAsync(orderId, ct);
        return doc is null ? NotFound() : Ok(doc);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        var id = $"ORD_{Guid.NewGuid():N}";

        var order = new OrderDoc
        {
            Id = id,
            OrderId = id,
            CatererId = req.CatererId,
            Status = "Pending",
            Customer = new OrderCustomer { UserId = req.CustomerUserId, Name = req.CustomerName, Phone = req.CustomerPhone },
            EventDateTime = req.EventDateTime,
            GuestCount = req.GuestCount,
            Location = new EventLocation { Pincode = req.Pincode, Address = req.Address },
            Package = new OrderPackage { PackageId = req.PackageId, Name = req.PackageName, PerPlatePrice = req.PerPlatePrice }
        };

        await _repo.CreateAsync(order, ct);
        return CreatedAtAction(nameof(GetById), new { orderId = id }, new { orderId = id });
    }

    [HttpPost("{orderId}/status")]
    public async Task<ActionResult> UpdateStatus(string orderId, [FromBody] UpdateStatusRequest req, CancellationToken ct)
    {
        // optional: validate allowed transitions
        await _repo.UpdateStatusAsync(orderId, req.Status, ct);
        return NoContent();
    }

    [HttpGet("by-customer/{userId}")]
    public async Task<ActionResult<PagedResult<OrderDoc>>> ListByCustomer(string userId, [FromQuery] int pageSize = 20, [FromQuery] string? continuationToken = null, CancellationToken ct = default)
    {
        var page = await _repo.ListByCustomerAsync(userId, pageSize, continuationToken, ct);
        return Ok(page);
    }

    [HttpGet("by-caterer/{catererId}/day")]
    public async Task<ActionResult<PagedResult<CatererOrderSummary>>> ListForCatererOnDay(
        string catererId,
        [FromQuery] DateTime dateUtc,                 // e.g., 2025-09-20
        [FromQuery] int pageSize = 50,
        [FromQuery] string? continuationToken = null,
        CancellationToken ct = default)
    {
        var page = await _repo.ListByCatererAndDayAsync(catererId, dateUtc, pageSize, continuationToken, ct);
        return Ok(page);
    }
}
