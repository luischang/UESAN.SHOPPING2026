-- H0 · Esquema transaccional mínimo de la tienda (database-first)
USE UesanShoppingAI;
GO
IF OBJECT_ID('dbo.Categoria') IS NULL
CREATE TABLE dbo.Categoria (
    Id     INT          NOT NULL PRIMARY KEY,
    Nombre VARCHAR(80)  NOT NULL
);
GO
IF OBJECT_ID('dbo.Producto') IS NULL
CREATE TABLE dbo.Producto (
    Id          INT            NOT NULL PRIMARY KEY,
    Nombre      VARCHAR(120)   NOT NULL,
    Descripcion VARCHAR(400)   NULL,
    Precio      DECIMAL(10,2)  NOT NULL,
    Stock       INT            NOT NULL,
    CategoriaId INT            NOT NULL REFERENCES dbo.Categoria(Id),
    Activo      BIT            NOT NULL DEFAULT 1
);
GO
IF OBJECT_ID('dbo.Cliente') IS NULL
CREATE TABLE dbo.Cliente (
    Id      INT           NOT NULL PRIMARY KEY,
    Nombres VARCHAR(120)  NOT NULL,
    Email   VARCHAR(120)  NOT NULL
);
GO
IF OBJECT_ID('dbo.Pedido') IS NULL
CREATE TABLE dbo.Pedido (
    Id           INT            NOT NULL PRIMARY KEY,
    ClienteId    INT            NOT NULL REFERENCES dbo.Cliente(Id),
    Fecha        DATETIME2      NOT NULL,
    Estado       VARCHAR(20)    NOT NULL,   -- PENDIENTE | ENVIADO | ENTREGADO | CANCELADO
    FechaEntrega DATETIME2      NULL,
    Total        DECIMAL(10,2)  NOT NULL
);
GO
IF OBJECT_ID('dbo.PedidoDetalle') IS NULL
CREATE TABLE dbo.PedidoDetalle (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    PedidoId       INT            NOT NULL REFERENCES dbo.Pedido(Id),
    ProductoId     INT            NOT NULL REFERENCES dbo.Producto(Id),
    Cantidad       INT            NOT NULL,
    PrecioUnitario DECIMAL(10,2)  NOT NULL
);
GO
