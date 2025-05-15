// Program.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Cors policy for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowStaticWebApp", policy =>
    {
        policy.WithOrigins("https://[votre-frontend-url]")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Get connection string from configuration (for local dev)TargetFramework
// In production, this will come from App Service configuration
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=tcp:[your-server].database.windows.net,1433;Database=NamesDB;User ID=[username];Password=[password];Encrypt=true";

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowStaticWebApp");

// API Endpoints
app.MapGet("/api/personnes", async (AppDbContext db) =>
{
    return await db.Personnes.ToListAsync();
})
.WithName("GetAllPersonnes");

app.MapGet("/api/personnes/{id}", async (int id, AppDbContext db) =>
{
    var personne = await db.Personnes.FindAsync(id);
    return personne is null ? Results.NotFound() : Results.Ok(personne);
})
.WithName("GetPersonne");

app.MapPost("/api/personnes", async (PersonneDTO personneDto, AppDbContext db) =>
{
    var personne = new Personne { Nom = personneDto.Nom };
    db.Personnes.Add(personne);
    await db.SaveChangesAsync();
    return Results.Created($"/api/personnes/{personne.Id}", personne);
})
.WithName("CreatePersonne");

app.MapPut("/api/personnes/{id}", async (int id, PersonneDTO personneDto, AppDbContext db) =>
{
    var personne = await db.Personnes.FindAsync(id);
    if (personne is null) return Results.NotFound();
    
    personne.Nom = personneDto.Nom;
    await db.SaveChangesAsync();
    
    return Results.NoContent();
})
.WithName("UpdatePersonne");

app.MapDelete("/api/personnes/{id}", async (int id, AppDbContext db) =>
{
    var personne = await db.Personnes.FindAsync(id);
    if (personne is null) return Results.NotFound();
    
    db.Personnes.Remove(personne);
    await db.SaveChangesAsync();
    
    return Results.NoContent();
})
.WithName("DeletePersonne");

app.Run();

// Data model
class Personne
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
}

class PersonneDTO
{
    [Required]
    public string Nom { get; set; } = string.Empty;
}

// DbContext
class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Personne> Personnes => Set<Personne>();
}