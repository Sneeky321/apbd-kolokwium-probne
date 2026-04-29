namespace KolokwiumProbne.DTOs;

public class CreateRentalWithMoviesDto
{
    public DateTime RentalDate { get; set; }
    public List<CreateMoviesDetailsDto> Movies { get; set; } = [];
}