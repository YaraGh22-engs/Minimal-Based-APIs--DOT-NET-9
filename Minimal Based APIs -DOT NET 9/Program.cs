


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();


app.MapGet("/api/products", () => Results.Ok());

app.MapPost("/api/products", () => Results.Ok());

app.MapPut("/api/products/{id}", (Guid id) => Results.NoContent());

app.MapPatch("/api/products/{id}", (Guid id) => Results.NoContent());

app.MapDelete("/api/products/{id}", (Guid id) => Results.NoContent());

//GENIRIC
app.MapMethods("api/products", ["OPTIONS"], () => Results.NoContent());
app.MapMethods("api/products", ["GET"], () => Results.Ok());
app.MapMethods("api/products", ["GET","POST"], () => Results.NoContent());


app.Run();
