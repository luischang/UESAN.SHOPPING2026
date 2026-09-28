using System.Threading.RateLimiting;
using UESAN.SHOPPING.AI;
using UESAN.SHOPPING.AI.API.Endpoints;
using UESAN.SHOPPING.AI.API.Seguridad;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddShoppingAi(builder.Configuration);

// H11 · servidor MCP: expone la tienda (solo lectura) a cualquier cliente MCP
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// H9 · seguridad y gobierno ----------------------------------------------------------

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.Seccion));
builder.Services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<JwtOptions>>().Value);
builder.Services.AddJwtUesan(builder.Configuration);
builder.Services.AddAuthorization();

// CORS: solo los orígenes del frontend Vue/Quasar del curso pueden llamar a esta API.
const string PoliticaCors = "FrontendUesan";
builder.Services.AddCors(o => o.AddPolicy(PoliticaCors, p => p
    .WithOrigins("http://localhost:9000", "http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

// Rate limiting: un cliente autenticado (o su IP si no lo está) no puede saturar al agente
// -las llamadas al LLM tienen costo y latencia- ni la cola de eventos de H8.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("ia", http => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: UsuarioActual.ClienteId(http)?.ToString() ?? http.Connection.RemoteIpAddress?.ToString() ?? "anon",
        factory: _ => new FixedWindowRateLimiterOptions { Window = TimeSpan.FromMinutes(1), PermitLimit = 20 }));
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors(PoliticaCors);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Documentación: /openapi/v1.json y Swagger UI en /swagger
app.MapOpenApi();
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "UESAN Shopping · IA"));
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapDevTokenEndpoint(app.Environment);   // H9 · SOLO Development: emitir un JWT de prueba

// H0 · datos transaccionales de la tienda (sin IA)
app.MapTiendaEndpoints();

// Endpoints de IA: todos cuelgan de /api/ia y comparten el límite de tasa "ia"
var ia = app.MapGroup("/api/ia").RequireRateLimiting("ia");
ia.MapH1Endpoints();   // H1 · pregunta y respuesta
ia.MapH2Endpoints();   // H2 · datos transaccionales como contexto
ia.MapH3Endpoints();   // H3 · embeddings y búsqueda vectorial
ia.MapH4Endpoints();   // H4 · RAG
ia.MapH5Endpoints();   // H5 · herramientas (tool calling)
ia.MapH6Endpoints();   // H6 · workflow con IA
ia.MapH7Endpoints();   // H7 · agente
ia.MapH8Endpoints();   // H8 · activación (cola + worker)
ia.MapH9Endpoints();   // H9 · verificación de la guardia de contenido
ia.MapH10Endpoints();  // H10 · multiagente (orquestador-trabajadores)

app.MapMcp("/mcp");    // H11 · servidor MCP (solo lectura: ConsultarProducto, BuscarPolitica)

app.Run();
