using UESAN.SHOPPING.AI;
using UESAN.SHOPPING.AI.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddShoppingAi(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Documentación: /openapi/v1.json y Swagger UI en /swagger
app.MapOpenApi();
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "UESAN Shopping · IA"));
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// H0 · datos transaccionales de la tienda (sin IA)
app.MapTiendaEndpoints();

// Endpoints de IA: todos cuelgan de /api/ia
var ia = app.MapGroup("/api/ia");
ia.MapH1Endpoints();   // H1 · pregunta y respuesta
ia.MapH2Endpoints();   // H2 · datos transaccionales como contexto

app.Run();
