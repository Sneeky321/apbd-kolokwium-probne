using KolokwiumProbne.DTOs;
using KolokwiumProbne.Exceptions;
using Microsoft.Data.SqlClient;

namespace KolokwiumProbne.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;

    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    }

    public async Task<GetCustomerRentalsDto> GetCustomerRentalsAsync(int customerId)
    {
	    var query = """
	                SELECT c.first_name AS FirstName,
	                	c.last_name AS LastName,
	                	r.rental_id AS RentalId,
	                	r.rental_date AS RentalDate,
	                	r.return_date AS ReturnDate,
	                	s.name AS StatusName,
	                	m.title AS MovieTitle,
	                	ri.price_at_rental AS PriceAtRental
	                FROM Customer c
	                JOIN Rental r ON r.customer_id = c.customer_id
	                JOIN Status s ON s.status_id = r.status_id
	                JOIN Rental_Item ri ON ri.rental_id = r.rental_id
	                JOIN Movie m ON m.movie_id = ri.movie_id
	                WHERE c.customer_id = @customerId;
	                """;

	    await using var connection = new SqlConnection(_connectionString);
	    await connection.OpenAsync();

	    await using var command = new SqlCommand();
	    command.Connection = connection;
	    command.CommandText = query;
	    command.Parameters.AddWithValue("@customerId", customerId);

	    await using var reader = await command.ExecuteReaderAsync();

	    GetCustomerRentalsDto? result = null;
	    
	    var ordFirstName = reader.GetOrdinal("FirstName");
	    var ordLastName = reader.GetOrdinal("LastName");
	    var ordRentalId = reader.GetOrdinal("RentalId");
	    var ordRentalDate = reader.GetOrdinal("RentalDate");
	    var ordReturnDate = reader.GetOrdinal("ReturnDate");
	    var ordStatus = reader.GetOrdinal("StatusName");
	    var ordMovieTitle = reader.GetOrdinal("MovieTitle");
	    var ordPrice = reader.GetOrdinal("PriceAtRental");

	    while (await reader.ReadAsync())
	    {
		    if (result is null)
		    {
			    result = new GetCustomerRentalsDto
			    {
				    FirstName = reader.GetString(ordFirstName),
				    LastName = reader.GetString(ordLastName),
				    Rentals = new List<GetRentalsDetailsDto>()
			    };
		    }
		    
		    var rentalId = reader.GetInt32(ordRentalId);
		    
		    var rental = result.Rentals.FirstOrDefault(e => e.Id.Equals(rentalId));

		    if (rental is null)
		    {
			    rental = new GetRentalsDetailsDto()
			    {
				    Id = rentalId,
				    RentalDate = reader.GetDateTime(ordRentalDate),
				    ReturnDate = reader.IsDBNull(ordReturnDate)
					    ? null
					    : reader.GetDateTime(ordReturnDate),
				    Status = reader.GetString(ordStatus),
				    Movies = new List<GetMovieDetailsDto>()
			    };
			    result.Rentals.Add(rental);
		    }
		    
		    rental.Movies.Add(new GetMovieDetailsDto
		    {
			    Title = reader.GetString(ordMovieTitle),
			    PriceAtRental = reader.GetDecimal(ordPrice)
		    });
	    }

	    return result ?? throw new NotFoundException($"No rentals found for customer: {customerId}");
    }

    public async Task CreateRentalWithMoviesAsync(int customerId, CreateRentalWithMoviesDto dto)
    {
	    var createRentalQuery = """
	                            INSERT INTO Rental
	                            VALUES(@RentalDate, @ReturnDate, @CustomerId, @StatusId)
	                            SELECT @@IDENTITY;
	                            """;

	    var createRentalItemQuery = """
	                                INSERT INTO Rental_Item
	                                VALUES(@RentalId, @MovieId, @Price);
	                                """;

	    var getMovieIdQuery = """
	                          SELECT movie_id
	                          FROM Movie
	                          WHERE title = @MovieTitle;
	                          """;

	    var checkCustomerQuery = """
	                             SELECT 1
	                             FROM Customer
	                             WHERE customer_id = @IdCustomer;
	                             """;

	    await using var connection = new SqlConnection(_connectionString);
	    await connection.OpenAsync();
	    
	    await using var transaction = await connection.BeginTransactionAsync();

	    await using var command = new SqlCommand();
	    command.Connection = connection;
	    command.Transaction = transaction as SqlTransaction;

	    try
	    {
		    command.Parameters.Clear();
		    command.CommandText = checkCustomerQuery;
		    command.Parameters.AddWithValue("@IdCustomer", customerId);
		    
		    var customerIdRes = await command.ExecuteScalarAsync();
		    if (customerIdRes == null)
		    {
			    throw new NotFoundException($"Customer {customerId} not found");
		    }
		    
		    command.Parameters.Clear();
		    command.CommandText = createRentalQuery;
		    command.Parameters.AddWithValue("@RentalDate", dto.RentalDate);
		    command.Parameters.AddWithValue("@ReturnDate", DBNull.Value);
		    command.Parameters.AddWithValue("@CustomerId", customerId);
		    command.Parameters.AddWithValue("@StatusId", 1);
		    
		    var rentalObject = await command.ExecuteScalarAsync();
		    var rentalId = Convert.ToInt32(rentalObject);

		    foreach (var movie in dto.Movies)
		    {
			    command.Parameters.Clear();
			    command.CommandText = getMovieIdQuery;
			    command.Parameters.AddWithValue("@MovieTitle", movie.Title);
			    
			    var movieObject = await command.ExecuteScalarAsync();
			    if (movieObject == null)
			    {
				    throw new NotFoundException($"Movie {movie.Title} not found");
			    }
			    
			    var movieId = Convert.ToInt32(movieObject);
			    
			    command.Parameters.Clear();
			    command.CommandText = createRentalItemQuery;
			    command.Parameters.AddWithValue("@RentalId", rentalId);
			    command.Parameters.AddWithValue("@MovieId", movieId);
			    command.Parameters.AddWithValue("@Price", movie.PriceAtRental);
			    
			    await command.ExecuteNonQueryAsync();
		    }
		    
		    await transaction.CommitAsync();
	    }
	    catch (Exception ex)
	    {
		    await transaction.RollbackAsync();
		    throw;
	    }
    }
}