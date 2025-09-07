using Catering.Api.DTOs;
using Catering.Api.Models;
using Catering.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Catering.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUsersRepository _repo;
    public UsersController(IUsersRepository repo) => _repo = repo;

    // Register
    [HttpPost]
    public async Task<ActionResult<object>> Register([FromBody] RegisterUserRequest req, CancellationToken ct)
    {
        // Optional: check duplicates
        if (!string.IsNullOrWhiteSpace(req.Email))
        {
            var existing = await _repo.GetByEmailAsync(req.Email, ct);
            if (existing is not null) return Conflict(new { message = "Email already registered." });
        }
        if (!string.IsNullOrWhiteSpace(req.Phone))
        {
            var existing = await _repo.GetByPhoneAsync(req.Phone, ct);
            if (existing is not null) return Conflict(new { message = "Phone already registered." });
        }

        var id = $"USR_{Guid.NewGuid():N}";
        var user = new UserDoc
        {
            Id = id,
            UserId = id, // PK
            Role = req.Role,
            CatererId = req.CatererId,
            Name = req.Name,
            Email = req.Email,
            Phone = req.Phone,
            Status = "Active"
        };

        var created = await _repo.CreateAsync(user, ct);
        return CreatedAtAction(nameof(GetById), new { userId = created.UserId }, new { userId = created.UserId });
    }

    // Get by id
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDoc>> GetById(string userId, CancellationToken ct)
    {
        var doc = await _repo.GetAsync(userId, ct);
        return doc is null ? NotFound() : Ok(doc);
    }

    // Update basic profile
    [HttpPut("{userId}")]
    public async Task<ActionResult> Update(string userId, [FromBody] UpdateUserRequest req, CancellationToken ct)
    {
        var doc = await _repo.GetAsync(userId, ct);
        if (doc is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(req.Name)) doc.Name = req.Name;
        if (!string.IsNullOrWhiteSpace(req.Phone)) doc.Phone = req.Phone;
        if (!string.IsNullOrWhiteSpace(req.Status)) doc.Status = req.Status;

        await _repo.UpsertAsync(doc, ct);
        return NoContent();
    }

    // Lookup helpers
    [HttpGet("by-email")]
    public async Task<ActionResult<UserDoc?>> GetByEmail([FromQuery] string email, CancellationToken ct)
        => Ok(await _repo.GetByEmailAsync(email, ct));

    [HttpGet("by-phone")]
    public async Task<ActionResult<UserDoc?>> GetByPhone([FromQuery] string phone, CancellationToken ct)
        => Ok(await _repo.GetByPhoneAsync(phone, ct));

    // Admin: list users by role
    [HttpGet("by-role/{role}")]
    public async Task<ActionResult<PagedResult<UserDoc>>> ListByRole(string role, [FromQuery] int pageSize = 50, [FromQuery] string? continuationToken = null, CancellationToken ct = default)
    {
        var page = await _repo.ListByRoleAsync(role, pageSize, continuationToken, ct);
        return Ok(page);
    }
}
