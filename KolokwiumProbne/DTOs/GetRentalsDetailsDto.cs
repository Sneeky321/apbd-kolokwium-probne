namespace KolokwiumProbne.DTOs;

public class GetRentalsDetailsDto
{
    public int Id { get; set; }
    public DateTime RentalDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<GetMovieDetailsDto> Movies { get; set; } = [];
}