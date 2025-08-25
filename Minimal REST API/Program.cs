using M04.BuildingRESTFulAPI.Data;
using M04.BuildingRESTFulAPI.Endpoints;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault;
});

builder.Services.AddSingleton<ProductRepository>();

var app = builder.Build();

app.MapProductEndpoints();

app.Run();