-- H3 · Almacén de conocimiento documental (fragmentos + embeddings)
USE UesanShoppingAI;
GO
IF SCHEMA_ID('ai') IS NULL EXEC('CREATE SCHEMA ai');
GO
IF OBJECT_ID('ai.Fragmento') IS NULL
BEGIN
    CREATE TABLE ai.Fragmento (
        Id            INT IDENTITY(1,1) PRIMARY KEY,
        Fuente        VARCHAR(200)   NOT NULL,   -- nombre del documento
        Seccion       NVARCHAR(300)  NOT NULL,   -- título de la sección
        Contenido     NVARCHAR(MAX)  NOT NULL,   -- texto del fragmento
        Embedding     NVARCHAR(MAX)  NOT NULL,   -- vector como JSON: [0.12, -0.03, ...]
        Dimensiones   INT            NOT NULL,
        Modelo        VARCHAR(100)   NOT NULL,   -- p. ej. ollama:bge-m3 (vectores de modelos distintos no se comparan)
        FechaIndexado DATETIME2      NOT NULL DEFAULT SYSDATETIME()
    );
    CREATE INDEX IX_Fragmento_Modelo ON ai.Fragmento (Modelo);
END
GO
-- Nota: SQL Server 2025 incluye el tipo VECTOR y VECTOR_DISTANCE. Aquí usamos JSON + cálculo
-- en C# para que funcione desde SQL Server 2017 (requisito del curso) y se vea la similitud coseno.
