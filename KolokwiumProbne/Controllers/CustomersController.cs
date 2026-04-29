using KolokwiumProbne.DTOs;
using KolokwiumProbne.Services;
using KolokwiumProbne.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace KolokwiumProbne.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly IDbService _dbService;

    public CustomersController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [Route("{id}/rentals")]
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _dbService.GetCustomerRentalsAsync(id);
            return Ok(result);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [Route("{id}/rentals")]
    [HttpPost]
    public async Task<IActionResult> Post([FromRoute] int id, [FromBody] CreateRentalWithMoviesDto dto)
    {
        if (!dto.Movies.Any())
        {
            return BadRequest("At least one item is required");
        }

        try
        {
            await _dbService.CreateRentalWithMoviesAsync(id, dto);
            return Created($"api/customers/{id}/rentals", dto);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}