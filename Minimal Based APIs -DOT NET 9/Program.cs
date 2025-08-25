


using M02.MinimalEndpointAnatomy.Data;
using M02.MinimalEndpointAnatomy.Responses;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<ProductRepository>();

var app = builder.Build();

app.MapGet("/api/products", (ProductRepository repository) =>
{
    return Results.Ok(repository.GetProductsPage());
}    );

app.MapGet("/api/products/{id:guid}", (Guid id, ProductRepository repository) =>
{
    var product = repository.GetProductById(id);
    return product is null ? Results.NotFound() : Results.Ok(ProductResponse.FromModel(product));
});

IResult GetProduct(Guid id, ProductRepository repository)
{
    // Handles retrieving a single product by its unique identifier
    var product = repository.GetProductById(id);

    return product is null ? Results.NotFound() : Results.Ok(ProductResponse.FromModel(product));
}

async Task<IResult> GetProducts(ProductRepository repository)
{
    // Handles retrieving a list of products
    await Task.Delay(100);
    return Results.Ok(repository.GetProductsPage());
}




app.Run();
