using M02.MinimalEndpointAnatomy.Data;
using M02.MinimalEndpointAnatomy.Models;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddSingleton<ProductRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapGet("/text", () => "Hello World");

app.MapGet("/json", () => new { Message = "Hello" });

app.MapGet("/api/products-le-ir/{id:guid}", (Guid id, ProductRepository repository) =>
{
    var product = repository.GetProductById(id);

    return product is null
            ? Results.NotFound()
            : Results.Ok(product);
});

app.MapGet("/api/products-le-tr/{id:guid}",
Results<Ok<Product>, NotFound> (Guid id, ProductRepository repository) =>
{
    var product = repository.GetProductById(id);

    return product is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(product);
});


app.Run();
