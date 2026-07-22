-- =========================================================
-- Sistema Inteligente de Gestión y Monitoreo de Redes
-- Consultas SQL utilizadas en el proyecto
-- Base de datos: redes | Tabla: dbo.redes
-- =========================================================

-- 1. Total de registros
SELECT COUNT(*) AS TotalRegistros FROM dbo.redes;

-- 2. Equipos por estado
SELECT estado, COUNT(*) AS Cantidad FROM dbo.redes GROUP BY estado;

-- 3. Equipos activos
SELECT * FROM dbo.redes WHERE estado = 'Activo' ORDER BY fecha_hora DESC;

-- 4. Equipos en falla
SELECT * FROM dbo.redes WHERE estado = 'Falla' ORDER BY fecha_hora DESC;

-- 5. Equipos en advertencia
SELECT * FROM dbo.redes WHERE estado = 'Advertencia' ORDER BY fecha_hora DESC;

-- 6. Promedios generales
SELECT
    ROUND(AVG(CAST([cpu_%] AS FLOAT)), 1)         AS CPU_Promedio,
    ROUND(AVG(CAST([ram_%] AS FLOAT)), 1)         AS RAM_Promedio,
    ROUND(AVG(CAST(descarga_Mbps AS FLOAT)), 1)   AS Descarga_Promedio,
    ROUND(AVG(CAST(subida_Mbps AS FLOAT)), 1)     AS Subida_Promedio,
    ROUND(AVG(CAST(latencia_ms AS FLOAT)), 1)     AS Latencia_Promedio
FROM dbo.redes;

-- 7. Dispositivos con CPU mayor al 90%
SELECT nombre_dispositivo, ip, [cpu_%], estado
FROM dbo.redes
WHERE [cpu_%] > 90
ORDER BY [cpu_%] DESC;

-- 8. Dispositivos con RAM mayor al 90%
SELECT nombre_dispositivo, ip, [ram_%], estado
FROM dbo.redes
WHERE [ram_%] > 90
ORDER BY [ram_%] DESC;

-- 9. Dispositivos con latencia mayor a 50ms
SELECT nombre_dispositivo, ip, latencia_ms, estado
FROM dbo.redes
WHERE latencia_ms > 50
ORDER BY latencia_ms DESC;

-- 10. Dispositivo con mayor CPU
SELECT TOP 1 nombre_dispositivo, ip, [cpu_%]
FROM dbo.redes
ORDER BY [cpu_%] DESC;

-- 11. Dispositivo con mayor RAM
SELECT TOP 1 nombre_dispositivo, ip, [ram_%]
FROM dbo.redes
ORDER BY [ram_%] DESC;

-- 12. Dispositivo con mayor latencia
SELECT TOP 1 nombre_dispositivo, ip, latencia_ms
FROM dbo.redes
ORDER BY latencia_ms DESC;

-- 13. Conteo por tipo de dispositivo
SELECT tipo, COUNT(*) AS Cantidad
FROM dbo.redes
GROUP BY tipo
ORDER BY Cantidad DESC;

-- 14. Distribución por ubicación
SELECT ubicacion, COUNT(*) AS Cantidad
FROM dbo.redes
GROUP BY ubicacion
ORDER BY Cantidad DESC;

-- 15. Distribución por marca
SELECT marca, COUNT(*) AS Cantidad
FROM dbo.redes
GROUP BY marca
ORDER BY Cantidad DESC;

-- 16. Routers
SELECT * FROM dbo.redes WHERE LOWER(tipo) LIKE '%router%';

-- 17. Switches
SELECT * FROM dbo.redes WHERE LOWER(tipo) LIKE '%switch%';

-- 18. Servidores
SELECT * FROM dbo.redes WHERE LOWER(tipo) LIKE '%servidor%';

-- 19. Access Points
SELECT * FROM dbo.redes WHERE LOWER(tipo) LIKE '%access point%' OR LOWER(tipo) LIKE '% ap%';

-- 20. Dispositivos con riesgo IA Alto
SELECT nombre_dispositivo, ip, riesgo_ia, recomendacion_ia
FROM dbo.redes
WHERE riesgo_ia = 'Alto';

-- 21. Últimos 50 registros para monitoreo
SELECT TOP 50 * FROM dbo.redes ORDER BY fecha_hora DESC;

-- 22. Reporte filtrado por fecha y estado (ejemplo)
SELECT * FROM dbo.redes
WHERE fecha_hora BETWEEN '2024-01-01' AND '2024-12-31'
  AND estado = 'Activo'
ORDER BY fecha_hora DESC;

-- 23. Registros con alertas
SELECT nombre_dispositivo, ip, alerta, incidencia, riesgo_ia
FROM dbo.redes
WHERE alerta IS NOT NULL AND alerta <> ''
ORDER BY fecha_hora DESC;

-- =========================================================
-- MÓDULO ALERTAS – consultas adicionales
-- =========================================================

-- Crear tabla alertas manualmente (si no se usa el auto-create de Program.cs)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='alertas'
)
BEGIN
    CREATE TABLE dbo.alertas (
        id               INT IDENTITY(1,1) PRIMARY KEY,
        fecha_hora       DATETIME      NOT NULL DEFAULT GETDATE(),
        dispositivo      NVARCHAR(200) NULL,
        ip               NVARCHAR(50)  NULL,
        tipo_alerta      NVARCHAR(100) NOT NULL,
        descripcion      NVARCHAR(500) NULL,
        severidad        NVARCHAR(20)  NOT NULL DEFAULT 'Media',
        estado           NVARCHAR(20)  NOT NULL DEFAULT 'Pendiente',
        fecha_atendida   DATETIME      NULL,
        usuario_atendio  NVARCHAR(100) NULL
    );
    CREATE INDEX IX_alertas_estado    ON dbo.alertas(estado);
    CREATE INDEX IX_alertas_severidad ON dbo.alertas(severidad);
    CREATE INDEX IX_alertas_fecha     ON dbo.alertas(fecha_hora DESC);
END;

-- Total alertas pendientes
SELECT COUNT(*) AS AlertasPendientes FROM dbo.alertas WHERE estado = 'Pendiente';

-- Alertas críticas activas
SELECT * FROM dbo.alertas WHERE severidad = 'Crítica' AND estado = 'Pendiente' ORDER BY fecha_hora DESC;

-- Últimas 10 alertas
SELECT TOP 10 * FROM dbo.alertas ORDER BY fecha_hora DESC;

-- Marcar alerta como atendida
UPDATE dbo.alertas
SET estado = 'Atendida', fecha_atendida = GETDATE(), usuario_atendio = 'admin'
WHERE id = 1;

-- Resumen por severidad
SELECT severidad, COUNT(*) AS Total FROM dbo.alertas WHERE estado = 'Pendiente' GROUP BY severidad;
