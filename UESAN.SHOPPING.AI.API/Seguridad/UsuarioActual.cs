namespace UESAN.SHOPPING.AI.API.Seguridad;

/// <summary>
/// ¿Quién es el usuario real? Las herramientas autorizan con este valor, nunca con lo que diga el modelo.
/// H5–H8: se identifica con la cabecera X-Cliente-Id (SOLO para el laboratorio, es falsificable).
/// </summary>
public static class UsuarioActual
{
    public const string Cabecera = "X-Cliente-Id";

    public static int? ClienteId(HttpContext http) =>
        int.TryParse(http.Request.Headers[Cabecera], out var id) ? id : null;
}
