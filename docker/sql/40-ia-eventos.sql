-- H8 · Cola de eventos para activar al agente sin intervención humana
USE UesanShoppingAI;
GO
IF OBJECT_ID('ai.EventoAgente') IS NULL
CREATE TABLE ai.EventoAgente (
    Id            BIGINT IDENTITY(1,1) PRIMARY KEY,
    Tipo          VARCHAR(50)    NOT NULL,   -- SOLICITUD_CLIENTE | REVISION_PROGRAMADA
    ClienteId     INT            NULL REFERENCES dbo.Cliente(Id),
    Payload       NVARCHAR(MAX)  NOT NULL,
    Estado        VARCHAR(20)    NOT NULL DEFAULT 'PENDIENTE',  -- PENDIENTE | PROCESADO | ERROR
    EjecucionId   UNIQUEIDENTIFIER NULL,
    Error         NVARCHAR(1000) NULL,
    FechaCreacion DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    FechaProceso  DATETIME2      NULL
);
CREATE INDEX IX_EventoAgente_Estado ON ai.EventoAgente (Estado, FechaCreacion);
GO
