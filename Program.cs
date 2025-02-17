var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000") // Use the correct frontend URL
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

app.UseHttpsRedirection();

// Apply CORS policy
app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

app.Run();
