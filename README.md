# Sistema Inteligente de Gestión y Monitoreo de Redes

## Descripción

Sistema web universitario completo desarrollado en **ASP.NET Core MVC (.NET 8)** para la gestión, monitoreo, análisis y reporte de dispositivos de red. Incluye dashboard interactivo, monitoreo en tiempo real, CRUD completo, exportación a Excel/PDF, análisis de inteligencia artificial y chatbot.

---

## Tecnologías Utilizadas

| Tecnología | Versión | Uso |
|---|---|---|
| ASP.NET Core MVC | .NET 8 | Framework principal |
| C# | 12 | Lenguaje de programación |
| SQL Server | Express | Base de datos |
| Entity Framework Core | 8.0 | ORM (Database First) |
| Bootstrap | 5.3 | Diseño responsive |
| Chart.js | 4.4 | Gráficos interactivos |
| jQuery | 3.7.1 | AJAX y manipulación DOM |
| ClosedXML | 0.102 | Exportación Excel |
| QuestPDF | 2024.3 | Exportación PDF |
| Bootstrap Icons | 1.11 | Íconos |

---

## Arquitectura

```
SistemaMonitoreoRedes/
├── Controllers/          # Controladores MVC
│   ├── AuthController.cs
│   ├── DashboardController.cs
│   ├── CrudController.cs
│   ├── MonitoreoController.cs
│   ├── ReportesController.cs
│   ├── IaController.cs
│   └── ChatbotController.cs
├── Models/               # Entidades de base de datos
│   └── Red.cs
├── Data/                 # DbContext EF Core
│   └── RedesDbContext.cs
├── Services/             # Lógica de negocio
│   ├── DashboardService.cs
│   ├── IaService.cs
│   ├── ChatbotService.cs
│   └── ExportService.cs
├── Repositories/         # Acceso a datos
│   └── RedRepository.cs
├── Helpers/              # Utilidades
│   ├── AuthHelper.cs
│   └── AuthFilter.cs
├── ViewModels/           # Modelos de vista
│   └── ViewModels.cs
├── Views/                # Vistas Razor
│   ├── Auth/
│   ├── Dashboard/
│   ├── Crud/
│   ├── Monitoreo/
│   ├── Reportes/
│   ├── Ia/
│   ├── Chatbot/
│   └── Shared/
├── wwwroot/              # Archivos estáticos
│   ├── css/site.css
│   └── js/site.js
├── SQL/                  # Consultas SQL
│   └── consultas.sql
└── appsettings.json      # Configuración
```

---

## Base de Datos

**Servidor:** `localhost\SQLEXPRESS`  
**Base de datos:** `redes`  
**Tabla:** `dbo.redes`

### Columnas de la tabla `dbo.redes`

| Columna | Tipo | Descripción |
|---|---|---|
| id_monitoreo | int (PK) | Identificador único |
| fecha_hora | datetime | Fecha y hora del registro |
| id_dispositivo | varchar | ID del dispositivo |
| nombre_dispositivo | varchar | Nombre del equipo |
| tipo | varchar | Tipo (Router, Switch, etc.) |
| marca | varchar | Marca del dispositivo |
| modelo | varchar | Modelo |
| ip | varchar | Dirección IP |
| ubicacion | varchar | Ubicación física |
| estado | varchar | Activo / Advertencia / Falla |
| cpu_% | decimal | Porcentaje de uso CPU |
| ram_% | decimal | Porcentaje de uso RAM |
| descarga_Mbps | decimal | Velocidad de descarga |
| subida_Mbps | decimal | Velocidad de subida |
| latencia_ms | decimal | Latencia en milisegundos |
| alerta | varchar | Tipo de alerta activa |
| incidencia | varchar | Incidencia registrada |
| riesgo_ia | varchar | Clasificación IA (Bajo/Medio/Alto) |
| recomendacion_ia | varchar | Recomendación generada por IA |

---

## Cadena de Conexión

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=redes;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Ubicada en `appsettings.json`.

---

## Credenciales de Acceso

| Campo | Valor |
|---|---|
| Usuario | `admin` |
| Contraseña | `admin123` |

---

## Módulos del Sistema

### 🖥️ Dashboard
- Tarjetas KPI con totales, promedios y estados
- Gráficos: Distribución por estado (doughnut), tipo (pie), ubicación (bar)
- Gráficos de línea: CPU/RAM y Descarga/Subida en los últimos registros

### 📡 Monitoreo en Tiempo Real
- Tabla actualizada automáticamente cada 10 segundos via AJAX
- Indicadores visuales de CPU, RAM, estado y riesgo IA

### 📋 Gestión de Datos (CRUD)
- Listar con búsqueda, filtros y ordenamiento
- Paginación de 10 registros por página
- Crear, Editar, Ver detalle, Eliminar con confirmación

### 📊 Reportes
- Filtros por fecha, estado, tipo, marca, ubicación
- Estadísticas del conjunto filtrado
- Exportar a **Excel** (.xlsx) con formato profesional
- Exportar a **PDF** (.pdf) con tabla formateada

### 🤖 Inteligencia Artificial
- Detección automática de CPU > 90%, RAM > 90%, Latencia > 50ms
- Detección de dispositivos en Falla y con Riesgo IA = Alto
- Tarjetas de recomendaciones automáticas por categoría
- Tabla detallada de alertas con severidad

### 💬 Chatbot
- Interfaz estilo ChatGPT
- Consultas en lenguaje natural resueltas con datos reales de SQL Server
- Botones de acceso rápido con preguntas frecuentes

---

## Cómo Ejecutar el Proyecto

### Requisitos previos
- Visual Studio 2022 (versión 17.8 o superior)
- .NET 8 SDK
- SQL Server Express instalado y en ejecución
- Base de datos `redes` con la tabla `dbo.redes` creada y con datos

### Pasos

1. **Abrir el proyecto** en Visual Studio 2022  
   Doble clic en `SistemaMonitoreoRedes.sln`

2. **Verificar la cadena de conexión** en `appsettings.json`  
   Ajustar `localhost\SQLEXPRESS` si tu instancia SQL tiene otro nombre

3. **Restaurar paquetes NuGet**  
   Visual Studio lo hace automáticamente al abrir, o manualmente:  
   `Tools > NuGet Package Manager > Manage NuGet Packages for Solution > Restore`

4. **Ejecutar el proyecto**  
   Presiona `F5` o el botón `▶ IIS Express`

5. **Iniciar sesión**  
   Usar las credenciales: `admin` / `admin123`

---

## Notas Técnicas

- Autenticación implementada con **Session** (sin ASP.NET Identity)
- Todas las páginas protegidas con `[AuthFilter]`
- EF Core en modo **Database First** (sin migraciones)
- Los gráficos usan **Chart.js** cargado desde CDN
- El monitoreo usa **AJAX** con `$.getJSON` cada 10 segundos
- QuestPDF en modo **Community License**
