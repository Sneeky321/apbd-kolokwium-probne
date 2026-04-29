using System.ComponentModel.DataAnnotations;

namespace KolokwiumProbne.DTOs;

public class CreateMoviesDetailsDto
{
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    public decimal PriceAtRental { get; set; }
}