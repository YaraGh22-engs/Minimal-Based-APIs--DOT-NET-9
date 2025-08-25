 
using M04.BuildingRESTFulAPI.Data;
using M04.BuildingRESTFulAPI.Models;
using M04.BuildingRESTFulAPI.Requests;
using M04.BuildingRESTFulAPI.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace M04.BuildingRESTFulAPI.Endpoints
{


    public static class ProductEndpoints
    {

        public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
        {
            var productApi = app.MapGroup("/api/products");

            productApi.MapMethods("", ["OPTIONS"], OptionsProducts);
            productApi.MapMethods("{productId:guid}", ["HEAD"], HeadProduct);
            productApi.MapGet("", GetPaged);
            productApi.MapGet("{productId:guid}", GetProductById).WithName(nameof(GetProductById));
            productApi.MapPost("", CreateProduct);
            productApi.MapPost("{productId:guid}/reviews", CreateProductReview);
            productApi.MapPut("{productId:guid}", Put);
            productApi.MapPatch("{productId:guid}", Patch);
            productApi.MapDelete("{productId:guid}", Delete);
            productApi.MapPost("process", ProcessAsync);
            productApi.MapGet("status/{jobId:guid}", GetProcessingStatus);
            productApi.MapGet("csv", GetProductsCSV);
            productApi.MapGet("physical-csv-file", GetPhysicalFile);
            productApi.MapGet("products-legacy", GetRedirect);
            productApi.MapGet("temp-products", TempProducts);
            productApi.MapGet("legacy-products", GetPermanentRedirect);
            productApi.MapGet("product-catalog", Catalog);

            return productApi;
        }

        private static IResult OptionsProducts(HttpResponse response)
        {
            response.Headers.Append("Allow", "GET, HEAD, POST, PUT, PATCH, DELETE, OPTIONS");
            return Results.NoContent();
        }

        private static IResult HeadProduct(Guid productId, ProductRepository repository)
        {
            return repository.ExistsById(productId) ? Results.Ok() : Results.NotFound();
        }

        private static IResult GetPaged(
            ProductRepository repository,
            int page = 1,
            int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            int totalCount = repository.GetProductsCount();

            var products = repository.GetProductsPage(page, pageSize);

            var pagedResult = PagedResult<ProductResponse>.Create(
                ProductResponse.FromModels(products),
                totalCount,
                page,
                pageSize);

            return Results.Ok(pagedResult);
        }

        private static Results<Ok<ProductResponse>, NotFound<string>> GetProductById(
            Guid productId,
            ProductRepository repository,
            bool includeReviews = false)
        {
            var product = repository.GetProductById(productId);

            if (product is null)
                return TypedResults.NotFound($"Product with Id '{productId}' not found");

            List<ProductReview>? reviews = null;

            if (includeReviews == true)
            {
                reviews = repository.GetProductReviews(productId);
            }

            return TypedResults.Ok(ProductResponse.FromModel(product, reviews));
        }

        private static IResult CreateProduct(CreateProductRequest request, ProductRepository repository)
        {
            if (repository.ExistsByName(request.Name))
                return Results.Conflict($"A product with the name '{request.Name}' already exists.");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price
            };

            repository.AddProduct(product);

            return Results.CreatedAtRoute(routeName: nameof(GetProductById),
                                  routeValues: new { productId = product.Id },
                                  value: ProductResponse.FromModel(product));
        }

        private static IResult CreateProductReview(
            Guid productId,
            CreateProductReviewRequest request,
            ProductRepository repository)
        {
            if (!repository.ExistsById(productId))
                return Results.NotFound($"Product with Id '{productId}' not found");

            var productReview = new ProductReview
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Reviewer = request.Reviewer,
                Stars = request.Stars
            };

            repository.AddProductReview(productReview);

            return Results.Created(
                    $"/api/products/{productId}/reviews/{productReview.Id}",
                    ProductReviewResponse.FromModel(productReview)
            );
        }
        private static IResult Put(Guid productId, UpdateProductRequest request, ProductRepository repository)
        {
            var product = repository.GetProductById(productId);

            if (product is null)
                return Results.NotFound($"Product with Id '{productId}' not found");

            product.Name = request.Name;
            product.Price = request.Price ?? 0;

            var succeeded = repository.UpdateProduct(product);

            if (!succeeded)
                return Results.StatusCode(500);

            return Results.NoContent();
        }

        private static async Task<IResult> Patch(
            Guid productId,
            ProductRepository repository,
            HttpRequest httpRequest)
        {
            using var reader = new StreamReader(httpRequest.Body);

            var json = await reader.ReadToEndAsync();

            var patchDoc = JsonConvert.DeserializeObject<JsonPatchDocument<UpdateProductRequest>>(json);

            if (patchDoc is null)
                return Results.BadRequest("Invalid patch document.");

            var product = repository.GetProductById(productId);

            if (product is null)
                return Results.NotFound($"Product with Id '{productId}' not found.");

            var updateModel = new UpdateProductRequest
            {
                Name = product.Name,
                Price = product.Price
            };

            patchDoc.ApplyTo(updateModel);

            product.Name = updateModel.Name;
            product.Price = updateModel.Price ?? 0;

            var succeeded = repository.UpdateProduct(product);

            if (!succeeded)
                return Results.StatusCode(500);

            return Results.NoContent();
        }

        private static IResult Delete(Guid productId, ProductRepository repository)
        {
            if (!repository.ExistsById(productId))
                return Results.NotFound($"Product with Id '{productId}' not found");

            var succeeded = repository.DeleteProduct(productId);

            if (!succeeded)
                return Results.StatusCode(500);

            return Results.NoContent();
        }

        private static IResult ProcessAsync()
        {
            var jobId = Guid.NewGuid();

            return Results.Accepted(
                $"/api/products/status/{jobId}",
                new { jobId, status = "Processing" }
            );
        }

        private static IResult GetProcessingStatus(Guid jobId)
        {
            var isStillProcessing = false; // fake it

            return Results.Ok(new { jobId, status = isStillProcessing ? "Processing" : "Completed" });
        }

        private static IResult GetProductsCSV(ProductRepository repository)
        {
            var products = repository.GetProductsPage(1, 100);

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Id,Name,Price");

            foreach (var p in products)
            {
                csvBuilder.AppendLine($"{p.Id},{p.Name},{p.Price}");
            }

            var fileBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());

            return Results.File(fileBytes, "text/csv", "product-catalog_1_100.csv");
        }

        private static IResult GetPhysicalFile()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "products.csv");

            return TypedResults.PhysicalFile(filePath, "text/csv", "products-export.csv");
        }

        private static IResult GetRedirect()
        {
            return Results.Redirect("/api/products/temp-products");
        }

        private static IResult TempProducts()
        {
            return Results.Ok(new { message = "You're in the temp endpoint. Chill." });
        }

        private static IResult GetPermanentRedirect()
        {
            return Results.Redirect("/api/products/product-catalog", permanent: true);
        }

        private static IResult Catalog()
        {
            return Results.Ok(new { message = "This is the permanent new location." });
        }
    }


}

