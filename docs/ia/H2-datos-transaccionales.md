# H2 · Datos transaccionales como contexto

**Concepto:** aplicación enriquecida con datos. El código consulta la base de datos
(consulta exacta, por id de cliente) y agrega el resultado al prompt.

## Qué se agrega
- `AsistentePedidosService`: consulta cliente y pedidos con Dapper, arma un JSON y lo envía al modelo.
- La respuesta devuelve `contextoEnviado` para ver exactamente qué recibió el modelo.

## Observa
- "¿Cuándo me entregaron mis audífonos?" → responde bien: el dato está en la BD.
- "¿Todavía puedo devolverlos?" → no puede responder con certeza: el **plazo** está en una
  política (conocimiento documental), no en una tabla. Eso motiva H3 y H4.
- El correo del cliente no se envía al modelo (minimización de datos).

## Preguntas
- ¿Quién decidió consultar los pedidos: el modelo o el código?
- ¿Qué pasaría si enviáramos toda la tabla de pedidos en el prompt?
