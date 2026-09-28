# H9 · Seguridad y gobierno

**Concepto:** dar autonomía a un agente (H7, H8) obliga a reforzar quién puede pedirle qué,
desde dónde, y qué hacer si alguien intenta manipular sus instrucciones.

## Qué se agrega
- **JWT**: `AddJwtUesan` valida `Issuer`, `Audience` y la firma. `/dev/token` (solo Development)
  emite un token de prueba sin flujo de login — en el proyecto real esto lo hace un Identity Provider.
- **`UsuarioActual`**: ahora lee el cliente del claim del JWT; solo si no hay token y el entorno
  es Development, cae a la cabecera `X-Cliente-Id` de los hitos anteriores.
- **Rol `Supervisor`**: protege `/h7/aprobaciones` con `RequireAuthorization("Supervisor")`.
- **CORS**: solo `localhost:9000` (Quasar) y `localhost:5173` (Vite) pueden llamar a la API.
- **Rate limiting**: 20 solicitudes por minuto por cliente (o IP) en todo `/api/ia`.
- **`GuardiaContenido`** (OWASP LLM01 - Prompt Injection): detecta patrones sospechosos y
  envuelve el mensaje del cliente en `<dato_no_confiable>` antes de pasarlo al agente.

## Probar
1. `POST /dev/token?clienteId=1&supervisor=false` y usar el token en cualquier endpoint de IA.
2. `GET /h7/aprobaciones` sin token → 401. Con token de cliente normal → 403. Con `supervisor=true` → 200.
3. `POST /h9/guardia` con un intento de prompt injection vs. un mensaje normal.
4. Iniciar un agente (H7) con un mensaje que incluya "ignora las instrucciones anteriores...":
   revisa en `/h7/agente/{id}/pasos` cómo reacciona.

## Preguntas
- ¿Por qué `GuardiaContenido` no reemplaza a la `PuertaHerramientas` de H5? ¿Qué protege cada una?
- ¿Qué pasaría si el frontend llamara desde un origen que no está en la política de CORS?
