namespace KolokwiumProbne.DTOs;

public class GetMovieDetailsDto
{
    public string Title { get; set; } = string.Empty;
    public decimal PriceAtRental { get; set; }
}