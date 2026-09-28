# H3 · Embeddings y búsqueda vectorial

**Concepto:** un embedding convierte texto en un vector; textos de significado parecido
quedan cerca. La búsqueda vectorial encuentra fragmentos por **significado**, no por palabras.

## Qué se agrega
- `conocimiento/*.md`: políticas de la tienda (conocimiento documental).
- `Fragmentador`: chunking por secciones `##`.
- `IngestaService`: documentos → fragmentos → embeddings (en lote) → tabla `ai.Fragmento`.
- `BuscadorSemantico`: embedding de la consulta + `TensorPrimitives.CosineSimilarity`.
- Modelo de embeddings local: `bge-m3` (multilingüe) vía Ollama.

## Probar
1. `GET /h3/similitud` con sinónimos y con temas distintos.
2. `POST /h3/ingesta` y luego `GET /h3/indice`.
3. `GET /h3/buscar?q=me llegó roto el producto`: recupera garantía/devoluciones sin usar esas palabras.

## Observa
- Los vectores de modelos distintos **no se comparan**: por eso se guarda la columna `Modelo`.
- La búsqueda encuentra lo **parecido**, no garantiza lo **correcto**.

## Preguntas
- ¿Por qué cada fragmento incluye el título del documento y de la sección?
- ¿Qué pasa con la similitud si cambias `bge-m3` por otro modelo?
