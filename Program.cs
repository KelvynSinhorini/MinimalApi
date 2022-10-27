using Microsoft.EntityFrameworkCore;
using MinimalApi.Data;
using MinimalApi.Models;
using MiniValidation;
using NetDevPack.Identity.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MinimalContextDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityEntityFrameworkContextConfiguration(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("MinimalApi")));

builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtConfiguration(builder.Configuration, "AppSettings");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthConfiguration();
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

app.MapPut("/provider/{id}", async (MinimalContextDb context, Guid id, Provider provider) =>
{
    var providerExisting = await context.Providers.AsNoTracking<Provider>()
                                            .FirstOrDefaultAsync(p => p.Id == id);

    if(providerExisting == null) return Results.NotFound();

    if (!MiniValidator.TryValidate(provider, out var errors))
        return Results.ValidationProblem(errors);

    context.Providers.Update(provider);
    var result = await context.SaveChangesAsync();

    return result > 0
    ? Results.NoContent()
    : Results.BadRequest("Houve algum problema ao salvar o resgistro");
})
.ProducesValidationProblem()
.Produces<Provider>(StatusCodes.Status204NoContent)
.Produces<Provider>(StatusCodes.Status400BadRequest)
.WithName("EditProvider")
.WithTags("Provider");

app.MapDelete("/provider/{id}", async (MinimalContextDb context, Guid id) =>
{
    var providerExisting = await context.Providers.FindAsync(id);
    if (providerExisting == null) return Results.NotFound();

    context.Providers.Remove(providerExisting);
    var result = await context.SaveChangesAsync();

    return result > 0
    ? Results.NoContent()
    : Results.BadRequest("Houve algum problema ao salvar o resgistro");
})
.Produces<Provider>(StatusCodes.Status404NotFound)
.Produces<Provider>(StatusCodes.Status204NoContent)
.Produces<Provider>(StatusCodes.Status400BadRequest)
.WithName("DeleteProvider")
.WithTags("Provider");

app.Run();