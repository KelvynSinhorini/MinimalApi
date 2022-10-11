using Microsoft.EntityFrameworkCore;
using MinimalApi.Data;
using MinimalApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MinimalContextDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/provider", async (MinimalContextDb context) =>
        await context.Providers.ToListAsync())
    .WithName("GetProvider")
    .WithTags("Provider");

app.MapGet("/provider/{id}", async (Guid id, MinimalContextDb context) =>
        await context.Providers.FindAsync(id) is Provider provider 
            ? Results.Ok(provider) 
            : Results.NotFound())
    .Produces<Provider>(StatusCodes.Status200OK)
    .Produces<Provider>(StatusCodes.Status404NotFound)
    .WithName("GetProviderById")
    .WithTags("Provider");

app.Run();