namespace KolokwiumProbne.DTOs;

public class GetCustomerRentalsDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<GetRentalsDetailsDto> Rentals { get; set; } = [];
}