-- H7 · Estado persistido del agente (permite suspender y reanudar) + traza de pasos
USE UesanShoppingAI;
GO
IF OBJECT_ID('ai.EjecucionAgente') IS NULL
CREATE TABLE ai.EjecucionAgente (
    Id                 UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    ClienteId          INT              NOT NULL REFERENCES dbo.Cliente(Id),
    Objetivo           NVARCHAR(300)    NOT NULL,
    MensajeInicial     NVARCHAR(2000)   NOT NULL,
    Estado             VARCHAR(30)      NOT NULL,  -- EN_CURSO | ESPERANDO_EVIDENCIA | ESPERANDO_APROBACION | COMPLETADO | ESCALADO | DETENIDO
    Pasos              INT              NOT NULL,
    TokensUsados       BIGINT           NOT NULL,
    HistorialJson      NVARCHAR(MAX)    NOT NULL,  -- mensajes (memoria de trabajo del agente)
    PendienteJson      NVARCHAR(MAX)    NULL,      -- llamada a herramienta en espera
    AprobacionesJson   NVARCHAR(MAX)    NULL,      -- aprobaciones humanas concedidas
    RespuestaFinal     NVARCHAR(MAX)    NULL,
    MotivoFin          NVARCHAR(300)    NULL,
    FechaInicio        DATETIME2        NOT NULL DEFAULT SYSDATETIME(),
    FechaActualizacion DATETIME2        NOT NULL DEFAULT SYSDATETIME()
);
GO
IF OBJECT_ID('ai.PasoAgente') IS NULL
CREATE TABLE ai.PasoAgente (
    Id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    EjecucionId UNIQUEIDENTIFIER NOT NULL REFERENCES ai.EjecucionAgente(Id),
    Numero      INT              NOT NULL,
    Tipo        VARCHAR(30)      NOT NULL,  -- HERRAMIENTA | ESPERA | APROBACION | REANUDACION | FIN
    Detalle     NVARCHAR(MAX)    NOT NULL,
    Fecha       DATETIME2        NOT NULL DEFAULT SYSDATETIME()
);
GO
