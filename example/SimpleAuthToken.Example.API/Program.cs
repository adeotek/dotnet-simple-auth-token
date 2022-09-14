using SimpleAuthToken.Example.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<SimpleAuthTokenMiddleware>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

// Custom Authentication Middleware
app.UseMiddleware<SimpleAuthTokenMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();