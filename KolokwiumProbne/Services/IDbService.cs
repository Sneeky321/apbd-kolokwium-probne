using KolokwiumProbne.DTOs;

namespace KolokwiumProbne.Services;

public interface IDbService
{
    Task<GetCustomerRentalsDto> GetCustomerRentalsAsync(int customerId);
    Task CreateRentalWithMoviesAsync(int customerId, CreateRentalWithMoviesDto dto);
}