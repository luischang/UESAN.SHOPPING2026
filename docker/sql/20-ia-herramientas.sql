-- H5 · Tablas para herramientas que ACTÚAN: devoluciones y auditoría
USE UesanShoppingAI;
GO
-- Regla de negocio como dato: qué productos se pueden devolver
IF COL_LENGTH('dbo.Producto', 'Retornable') IS NULL
    ALTER TABLE dbo.Producto ADD Retornable BIT NOT NULL CONSTRAINT DF_Producto_Retornable DEFAULT 1;
GO
UPDATE dbo.Producto SET Retornable = 0 WHERE Id IN (5, 12);  -- licencia digital y producto de higiene
GO
IF OBJECT_ID('ai.SolicitudDevolucion') IS NULL
CREATE TABLE ai.SolicitudDevolucion (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    PedidoId       INT            NOT NULL REFERENCES dbo.Pedido(Id),
    ProductoId     INT            NOT NULL REFERENCES dbo.Producto(Id),
    ClienteId      INT            NOT NULL REFERENCES dbo.Cliente(Id),
    Motivo         NVARCHAR(500)  NOT NULL,
    Estado         VARCHAR(30)    NOT NULL,   -- REGISTRADA | REEMBOLSO_EMITIDO | RECHAZADA
    MontoReembolso DECIMAL(10,2)  NULL,
    Origen         VARCHAR(80)    NOT NULL,   -- quién la registró (asistente, workflow, agente...)
    FechaRegistro  DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_Devolucion_PedidoProducto UNIQUE (PedidoId, ProductoId)  -- idempotencia
);
GO
IF OBJECT_ID('ai.Auditoria') IS NULL
CREATE TABLE ai.Auditoria (
    Id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    Fecha       DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    Usuario     VARCHAR(60)    NOT NULL,
    Origen      VARCHAR(80)    NOT NULL,
    Herramienta VARCHAR(80)    NOT NULL,
    Parametros  NVARCHAR(MAX)  NULL,
    Resultado   NVARCHAR(MAX)  NULL,
    Exitoso     BIT            NOT NULL
);
GO
