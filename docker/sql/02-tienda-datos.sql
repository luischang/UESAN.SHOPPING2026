-- H0 · Datos de ejemplo. Las fechas son relativas a HOY para que las reglas
-- de plazo (p. ej. 30 días para devoluciones) siempre se puedan demostrar.
USE UesanShoppingAI;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Categoria)
BEGIN
    INSERT dbo.Categoria (Id, Nombre) VALUES
    (1, 'Tecnología'), (2, 'Hogar'), (3, 'Deportes'), (4, 'Libros');

    INSERT dbo.Producto (Id, Nombre, Descripcion, Precio, Stock, CategoriaId) VALUES
    (1, 'Audífonos inalámbricos Pulse', 'Bluetooth 5.3, cancelación de ruido, 30 h de batería', 249.90, 25, 1),
    (2, 'Teclado mecánico Nova', 'Switches rojos, retroiluminado, formato TKL', 329.00, 12, 1),
    (3, 'Monitor 27" QHD', 'Panel IPS 2560x1440, 165 Hz', 1299.00, 6, 1),
    (4, 'Mouse ergonómico Vertix', 'Inalámbrico, diseño vertical', 119.90, 40, 1),
    (5, 'Licencia antivirus 1 año', 'Producto digital, se entrega por correo', 89.90, 999, 1),
    (6, 'Cafetera de goteo Aroma', '12 tazas, jarra de vidrio', 189.00, 18, 2),
    (7, 'Juego de sábanas 2 plazas', 'Algodón 200 hilos', 139.00, 30, 2),
    (8, 'Zapatillas running Trail X', 'Suela antideslizante, talla 38 a 44', 359.00, 20, 3),
    (9, 'Balón de fútbol Pro', 'Tamaño 5, cosido a máquina', 99.00, 50, 3),
    (10, 'Clean Architecture (libro)', 'Robert C. Martin, edición en español', 159.00, 15, 4),
    (11, 'Diseño de APIs (libro)', 'Buenas prácticas REST', 129.00, 10, 4),
    (12, 'Cepillo dental eléctrico Sonic', 'Producto de higiene personal', 199.00, 22, 2);

    INSERT dbo.Cliente (Id, Nombres, Email) VALUES
    (1, 'Ana Torres', 'ana.torres@example.com'),
    (2, 'Bruno Díaz', 'bruno.diaz@example.com'),
    (3, 'Carla Quispe', 'carla.quispe@example.com');

    -- Ana: un pedido entregado hace 10 días (devolución posible) y uno enviado
    INSERT dbo.Pedido (Id, ClienteId, Fecha, Estado, FechaEntrega, Total) VALUES
    (1001, 1, DATEADD(DAY, -14, SYSDATETIME()), 'ENTREGADO', DATEADD(DAY, -10, SYSDATETIME()), 369.80),
    (1002, 1, DATEADD(DAY, -2,  SYSDATETIME()), 'ENVIADO',   NULL,                              329.00),
    -- Bruno: un pedido entregado hace 45 días (fuera de plazo) y uno con monitor (monto alto)
    (1003, 2, DATEADD(DAY, -50, SYSDATETIME()), 'ENTREGADO', DATEADD(DAY, -45, SYSDATETIME()), 359.00),
    (1004, 2, DATEADD(DAY, -8,  SYSDATETIME()), 'ENTREGADO', DATEADD(DAY, -5,  SYSDATETIME()), 1299.00),
    -- Carla: producto de higiene (no retornable) y licencia digital
    (1005, 3, DATEADD(DAY, -9,  SYSDATETIME()), 'ENTREGADO', DATEADD(DAY, -6,  SYSDATETIME()), 288.90),
    (1006, 3, DATEADD(DAY, -1,  SYSDATETIME()), 'PENDIENTE', NULL,                              99.00);

    INSERT dbo.PedidoDetalle (PedidoId, ProductoId, Cantidad, PrecioUnitario) VALUES
    (1001, 1, 1, 249.90), (1001, 4, 1, 119.90),
    (1002, 2, 1, 329.00),
    (1003, 8, 1, 359.00),
    (1004, 3, 1, 1299.00),
    (1005, 12, 1, 199.00), (1005, 5, 1, 89.90),
    (1006, 9, 1, 99.00);
END
GO
