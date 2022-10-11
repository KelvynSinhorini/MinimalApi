using Microsoft.EntityFrameworkCore;
using MinimalApi.Data;
using MinimalApi.Models;
using MiniValidation;

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

app.MapGet("/provider/{id}", async (MinimalContextDb context, Guid id) =>
        await context.Providers.FindAsync(id) is Provider provider 
            ? Results.Ok(provider) 
            : Results.NotFound())
    .Produces<Provider>(StatusCodes.Status200OK)
    .Produces<Provider>(StatusCodes.Status404NotFound)
    .WithName("GetProviderById")
    .WithTags("Provider");

app.MapPost("/provider", async (MinimalContextDb context, Provider provider) =>
{
    if(!MiniValidator.TryValidate(provider, out var errors))
        return Results.ValidationProblem(errors);

    await context.Providers.AddAsync(provider);
    var result = await context.SaveChangesAsync();

    return result > 0 
    ? Results.Created($"/fornecedor/{provider.Id}", provider)
    : Results.BadRequest("Houve algum problema ao salvar o resgistro");
})
.ProducesValidationProblem()
.Produces<Provider>(StatusCodes.Status201Created)
.Produces<Provider>(StatusCodes.Status400BadRequest)
.WithName("CreateProvider")
.WithTags("Provider");

app.Run();