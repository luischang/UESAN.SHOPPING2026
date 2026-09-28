using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace UESAN.SHOPPING.AI.Datos;

/// <summary>
/// Fábrica de conexiones a la base de datos del laboratorio (UesanShoppingAI).
/// La cadena se lee de ConnectionStrings:ShoppingAI.
/// </summary>
public sealed class ConexionSql(IConfiguration configuration)
{
    private readonly string _cadena = configuration.GetConnectionString("ShoppingAI")
        ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:ShoppingAI'.");

    public SqlConnection Crear() => new(_cadena);
}
