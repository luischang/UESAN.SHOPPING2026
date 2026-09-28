namespace UESAN.SHOPPING.AI.Datos;

// DTOs de lectura de la tienda. Son clases con propiedades (no records posicionales)
// para que Dapper las mapee sin depender del orden ni del tipo exacto del constructor.

public sealed class ProductoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string Categoria { get; set; } = "";
}

public sealed class ClienteDto
{
    public int Id { get; set; }
    public string Nombres { get; set; } = "";
    // El correo existe en la BD pero NO se expone a la IA (minimización de datos).
}

public sealed class PedidoDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Cliente { get; set; } = "";
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = "";
    public DateTime? FechaEntrega { get; set; }
    public decimal Total { get; set; }
    public List<PedidoDetalleDto> Detalle { get; set; } = [];

    /// <summary>Días transcurridos desde la entrega (null si aún no se entrega).</summary>
    public int? DiasDesdeEntrega(DateTime hoy) =>
        FechaEntrega is { } f ? (int)(hoy.Date - f.Date).TotalDays : null;
}

public sealed class PedidoDetalleDto
{
    public int ProductoId { get; set; }
    public string Producto { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
