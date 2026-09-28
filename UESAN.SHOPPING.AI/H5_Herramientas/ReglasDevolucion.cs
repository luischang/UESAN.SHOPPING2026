using UESAN.SHOPPING.AI.Datos;

namespace UESAN.SHOPPING.AI.H5_Herramientas;

/// <summary>
/// Reglas de negocio de la política de devoluciones, implementadas EN CÓDIGO.
/// El documento de política explica las reglas; este código las hace cumplir.
/// Aunque el modelo "crea" que algo procede, si estas reglas fallan, no se ejecuta.
/// </summary>
public static class ReglasDevolucion
{
    public const int PlazoDias = 30;

    public static void Validar(PedidoDto pedido, int productoId, bool retornable, DateTime hoy)
    {
        if (pedido.Estado != "ENTREGADO")
            throw new ReglaNegocioException(
                $"Solo se pueden devolver productos de pedidos ENTREGADOS (estado actual: {pedido.Estado}).");

        if (pedido.Detalle.All(d => d.ProductoId != productoId))
            throw new ReglaNegocioException($"El producto {productoId} no pertenece al pedido {pedido.Id}.");

        var dias = pedido.DiasDesdeEntrega(hoy) ?? 0;
        if (dias > PlazoDias)
            throw new ReglaNegocioException(
                $"El plazo de {PlazoDias} días venció: el pedido se entregó hace {dias} días.");

        if (!retornable)
            throw new ReglaNegocioException("El producto no es retornable (higiene personal o producto digital).");
    }
}
