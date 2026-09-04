namespace ServiCore.Application.Customers.DTOs;

public record UpdateCustomerRequest(
    string Name,
    string Email,
    string? PhoneNumber);