using Subspace.Shared.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add CORS service
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSubspaceWeb", corsBuilder =>
    {
        corsBuilder
            .WithOrigins(
                "http://localhost:5235",      // Local
                "https://subspaceapi.com"     // Prod
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// MariaDB Connection
builder.Services.AddDbContext<SubspaceDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("SubspaceApiDb"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("SubspaceApiDb"))
    )
);

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

// Enable CORS
app.UseCors("AllowSubspaceWeb");

// Always enable Swagger
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Subspace API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Subspace API - Swagger Documentation";
});

app.UseReDoc(c =>
{
    c.RoutePrefix = "redoc";
    c.SpecUrl("/swagger/v1/swagger.json");
    c.DocumentTitle = "Subspace API - ReDoc Documentation";
});

// Redirect '/' to https://subspaceapi.com
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("https://subspaceapi.com/", permanent: true);
        return;
    }

    await next();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();