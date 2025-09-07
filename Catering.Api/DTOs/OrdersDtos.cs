namespace Catering.Api.DTOs;

public sealed class CreateOrderRequest
{
    public string CatererId { get; set; } = default!;
    public string CustomerUserId { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;

    public DateTime EventDateTime { get; set; }
    public int GuestCount { get; set; }

    public string Pincode { get; set; } = default!;
    public string Address { get; set; } = default!;

    public string PackageId { get; set; } = default!;
    public string PackageName { get; set; } = default!;
    public decimal PerPlatePrice { get; set; }
}

public sealed class UpdateStatusRequest
{
    public string Status { get; set; } = default!; // Accepted|Declined|Cancelled|Completed
}
