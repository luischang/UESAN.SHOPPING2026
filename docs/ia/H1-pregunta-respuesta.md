# H1 · Pregunta y respuesta con un LLM

**Concepto:** aplicación conversacional. Usuario → aplicación → API del modelo → respuesta.

## Qué se agrega
- `Microsoft.Extensions.AI`: la interfaz `IChatClient` desacopla el código del proveedor.
- `ProveedoresIa`: crea el cliente según `AI:Proveedor`.
  - `Ollama` = **modelo local** (gratis, privado, depende de tu hardware).
  - `OpenAI` = **servicio en la nube** (más capaz, con costo por token y datos que salen de tu red).
- `ChatBasicoService`: una llamada, una respuesta. También en streaming.

## Probar
- `GET /api/ia/h1/proveedor`
- `POST /api/ia/h1/chat` con "¿Cuántos días tengo para devolver un producto?"

Observa que la respuesta es **genérica**: el modelo no conoce las políticas de UESAN Shopping.

## Cambiar a la nube
```bash
dotnet user-secrets set "AI:OpenAI:ApiKey" "<tu-clave>" --project UESAN.SHOPPING.AI.API
```
y en `appsettings.Development.json` pon `"AI": { "Proveedor": "OpenAI" }`. El código no cambia.

## Preguntas
- ¿Por qué un chatbot no es automáticamente un agente?
- ¿Qué ventajas y riesgos tiene un modelo local frente a uno en la nube?
