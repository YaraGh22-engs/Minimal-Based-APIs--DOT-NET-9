namespace M03.MinimalEndpointAnatomy.Endpoints;

public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var productApi = app.MapGroup("/api/products");

        productApi.MapGet("/", GetAllProducts);

        productApi.MapGet("/{id}", GetProductById);

        productApi.MapPost("/", CreateProduct);

        productApi.MapPut("/{id}", UpdateProduct);

        productApi.MapDelete("/{id}", DeleteProduct);

        return productApi;
    }


    static IResult GetAllProducts() => Results.Ok();

    static IResult GetProductById(Guid id) => Results.Ok();

    static IResult CreateProduct() => Results.Ok();

    static IResult UpdateProduct(Guid id) => Results.NoContent();

    static IResult DeleteProduct(Guid id) => Results.NoContent();
}