# apbd-kolokwium-probne

dotnet add package Microsoft.Data.SqlClient

dotnet add package Swashbuckle.AspNetCore

"ConnectionStrings": {
    "DefaultConnection": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=kolokwium;Integrated Security=True;"
  }

"ConnectionStrings": {
    "DefaultConnection": "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True"
  }

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IDbService, DbService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
