namespace Catering.Api.DTOs;

public sealed class RegisterUserRequest
{
    public string Role { get; set; } = "Customer"; // Customer|Caterer|Admin
    public string? CatererId { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
}

public sealed class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Status { get; set; } // Active|Blocked
}
