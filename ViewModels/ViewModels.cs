using SistemaMonitoreoRedes.Models;

namespace SistemaMonitoreoRedes.ViewModels
{
    // ══════════════════════════════════════════════════════════════
    // VIEWMODELS ORIGINALES – SIN MODIFICACIONES
    // ══════════════════════════════════════════════════════════════

    public class DashboardViewModel
    {
        public int TotalRegistros { get; set; }
        public int EquiposActivos { get; set; }
        public int EquiposAdvertencia { get; set; }
        public int EquiposFalla { get; set; }
        public decimal CpuPromedio { get; set; }
        public decimal RamPromedio { get; set; }
        public decimal DescargaPromedio { get; set; }
        public decimal SubidaPromedio { get; set; }
        public decimal LatenciaPromedio { get; set; }

        // Chart data
        public Dictionary<string, int> EstadoDistribucion { get; set; } = new();
        public Dictionary<string, int> TipoDistribucion { get; set; } = new();
        public Dictionary<string, int> UbicacionDistribucion { get; set; } = new();
        public List<string> FechasRecientes { get; set; } = new();
        public List<decimal> CpuReciente { get; set; } = new();
        public List<decimal> RamReciente { get; set; } = new();
        public List<decimal> DescargaReciente { get; set; } = new();
        public List<decimal> SubidaReciente { get; set; } = new();

        // ── Nuevo: datos de alertas para el Dashboard ────────────────
        public int TotalAlertas { get; set; }
        public int AlertasPendientes { get; set; }
        public int AlertasCriticas { get; set; }
        public List<Alerta> UltimasAlertas { get; set; } = new();
    }

    public class CrudViewModel
    {
        public List<Red> Registros { get; set; } = new();
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string? Busqueda { get; set; }
        public string? FiltroEstado { get; set; }
        public string? FiltroTipo { get; set; }
        public string? FiltroMarca { get; set; }
        public string? FiltroUbicacion { get; set; }
        public string? FiltroIp { get; set; }
        public string? FiltroNombre { get; set; }
        public string? Orden { get; set; }
        public List<string> Estados { get; set; } = new();
        public List<string> Tipos { get; set; } = new();
        public List<string> Marcas { get; set; } = new();
        public List<string> Ubicaciones { get; set; } = new();
    }

    public class ReporteFiltroViewModel
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Estado { get; set; }
        public string? Tipo { get; set; }
        public string? Marca { get; set; }
        public string? Ubicacion { get; set; }
        public List<Red> Resultados { get; set; } = new();
        public List<string> Estados { get; set; } = new();
        public List<string> Tipos { get; set; } = new();
        public List<string> Marcas { get; set; } = new();
        public List<string> Ubicaciones { get; set; } = new();
        public int TotalResultados { get; set; }
        public decimal CpuPromedio { get; set; }
        public decimal RamPromedio { get; set; }
        public decimal LatenciaPromedio { get; set; }
    }

    public class IaViewModel
    {
        public List<AlertaIa> Alertas { get; set; } = new();
        public List<RecomendacionIa> Recomendaciones { get; set; } = new();
        public int TotalAlertas { get; set; }
        public int DispositivosRiesgoAlto { get; set; }
        public int DispositivosCpuCritico { get; set; }
        public int DispositivosRamCritico { get; set; }
        public int DispositivosLatenciaCritica { get; set; }
        public int DispositivosEnFalla { get; set; }
    }

    public class AlertaIa
    {
        public string NombreDispositivo { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Ip { get; set; } = "";
        public string TipoAlerta { get; set; } = "";
        public string Valor { get; set; } = "";
        public string Severidad { get; set; } = "";
        public string Recomendacion { get; set; } = "";
    }

    public class RecomendacionIa
    {
        public string Categoria { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Icono { get; set; } = "";
        public string ColorClase { get; set; } = "";
        public int CantidadAfectados { get; set; }
    }

    public class ChatMensaje
    {
        public string Rol { get; set; } = "user";
        public string Contenido { get; set; } = "";
        public DateTime Hora { get; set; } = DateTime.Now;
    }

    // ══════════════════════════════════════════════════════════════
    // NUEVOS VIEWMODELS – MÓDULO ALERTAS
    // ══════════════════════════════════════════════════════════════

    public class AlertasIndexViewModel
    {
        public List<Alerta> Alertas { get; set; } = new();
        public string? FiltroEstado { get; set; }
        public string? FiltroSeveridad { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int TotalPendientes { get; set; }
        public int TotalCriticas { get; set; }
        public int TotalAlertas { get; set; }
    }

    public class ResumenAlertasViewModel
    {
        public int Total { get; set; }
        public int Pendientes { get; set; }
        public int Atendidas { get; set; }
        public int Criticas { get; set; }
        public int Altas { get; set; }
        public int Medias { get; set; }
        public int Bajas { get; set; }
        public List<Alerta> UltimasAlertas { get; set; } = new();
    }
}
