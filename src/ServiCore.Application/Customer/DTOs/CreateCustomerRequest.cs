namespace ServiCore.Application.Customers.DTOs;

public record CreateCustomerRequest(
    string Name,
    string Email,
    string? PhoneNumber);