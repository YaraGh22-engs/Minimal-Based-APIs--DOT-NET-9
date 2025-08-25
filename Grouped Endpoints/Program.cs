

using M03.MinimalEndpointAnatomy.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



var app = builder.Build();



app.MapProductEndpoints();

app.Run();
