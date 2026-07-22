using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaMonitoreoRedes.Data;
using SistemaMonitoreoRedes.Services;

namespace SistemaMonitoreoRedes.Controllers
{
    /// <summary>
    /// API REST para consumo desde aplicación Android (APK).
    /// Todos los endpoints devuelven JSON.
    /// Base URL: /api/v1/
    /// 
    /// Autenticación básica via header: X-Api-Key: netmonitor2024
    /// (para producción se recomienda JWT; esta implementación es suficiente para demo universitaria)
    /// </summary>
    [Route("api/v1")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly RedesDbContext _context;
        private readonly AlertService _alertService;
        private const string ApiKey = "netmonitor2024";

        public ApiController(RedesDbContext context, AlertService alertService)
        {
            _context = context;
            _alertService = alertService;
        }

        // ── Auth simple ───────────────────────────────────────────────
        private bool ApiKeyValida()
        {
            Request.Headers.TryGetValue("X-Api-Key", out var key);
            return key == ApiKey;
        }

        // ── GET /api/v1/dispositivos ──────────────────────────────────
        /// <summary>Retorna todos los dispositivos de dbo.redes.</summary>
        [HttpGet("dispositivos")]
        public async Task<IActionResult> GetDispositivos()
        {
            if (!ApiKeyValida()) return Unauthorized(new { error = "API Key inválida" });

            var datos = await _context.Redes
                .OrderByDescending(r => r.FechaHora)
                .Select(r => new
                {
                    r.IdMonitoreo,
                    FechaHora         = r.FechaHora != null ? r.FechaHora.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    r.NombreDispositivo,
                    r.Tipo,
                    r.Marca,
                    r.Modelo,
                    r.Ip,
                    r.Ubicacion,
                    r.Estado,
                    Cpu               = r.CpuPorcentaje,
                    Ram               = r.RamPorcentaje,
                    Descarga          = r.DescargaMbps,
                    Subida            = r.SubidaMbps,
                    Latencia          = r.LatenciaMs,
                    r.Alerta,
                    r.Incidencia,
                    r.RiesgoIa,
                    r.RecomendacionIa
                })
                .ToListAsync();

            return Ok(new { ok = true, total = datos.Count, datos });
        }

        // ── GET /api/v1/dispositivos/activos ─────────────────────────
        [HttpGet("dispositivos/activos")]
        public async Task<IActionResult> GetDispositvosActivos()
        {
            if (!ApiKeyValida()) return Unauthorized(new { error = "API Key inválida" });

            var datos = await _context.Redes
                .Where(r => r.Estado == "Activo")
                .OrderBy(r => r.NombreDispositivo)
                .Select(r => new { r.IdMonitoreo, r.NombreDispositivo, r.Ip, r.Tipo, r.Estado })
                .ToListAsync();

            return Ok(new { ok = true, total = datos.Count, datos });
        }

        // ── GET /api/v1/alertas ───────────────────────────────────────
        /// <summary>Retorna todas las alertas, más recientes primero.</summary>
        [HttpGet("alertas")]
        public async Task<IActionResult> GetAlertas([FromQuery] string? estado = null)
        {
            if (!ApiKeyValida()) return Unauthorized(new { error = "API Key inválida" });

            var query = _context.Alertas.AsQueryable();
            if (!string.IsNullOrEmpty(estado))
                query = query.Where(a => a.Estado == estado);

            var datos = await query
                .OrderByDescending(a => a.FechaHora)
                .Select(a => new
                {
                    a.Id,
                    FechaHora    = a.FechaHora.ToString("yyyy-MM-dd HH:mm:ss"),
                    a.Dispositivo,
                    a.Ip,
                    a.TipoAlerta,
                    a.Descripcion,
                    a.Severidad,
                    a.Estado,
                    FechaAtendida = a.FechaAtendida != null ? a.FechaAtendida.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    a.UsuarioAtendio
                })
                .ToListAsync();

            return Ok(new { ok = true, total = datos.Count, datos });
        }

        // ── GET /api/v1/alertas/pendientes ───────────────────────────
        [HttpGet("alertas/pendientes")]
        public async Task<IActionResult> GetAlertasPendientes()
        {
            if (!ApiKeyValida()) return Unauthorized(new { error = "API Key inválida" });

            var datos = await _context.Alertas
                .Where(a => a.Estado == "Pendiente")
                .OrderByDescending(a => a.FechaHora)
                .Select(a => new
                {
                    a.Id,
                    FechaHora = a.FechaHora.ToString("yyyy-MM-dd HH:mm:ss"),
                    a.Dispositivo,
                    a.Ip,
                    a.TipoAlerta,
                    a.Descripcion,
                    a.Severidad,
                    a.Estado
                })
                .ToListAsync();

            return Ok(new { ok = true, total = datos.Count, datos });
        }

        // ── PUT /api/v1/alertas/{id}/atender ─────────────────────────
        /// <summary>Marca una alerta como atendida desde el APK.</summary>
        [HttpPut("alertas/{id}/atender")]
        public async Task<IActionResult> AtenderAlerta(int id, [FromBody] AtenderRequest? req)
        {
            if (!ApiKeyValida()) return Unauthorized(new { error = "API Key inválida" });

            var usuario = req?.Usuario ?? "android-app";
            var ok = await _alertService.MarcarAtendidaAsync(id, usuario);
            if (!ok) return NotFound(new { ok = false, error = $"Alerta #{id} no encontrada" });

            return Ok(new { ok = true, mensaje = $"Alerta #{id} marcada como atendida por {usuario}" });
        }

        // ── GET /api/v1/red/estado ───────────────────────────────────
        /// <summary>Estado general de la red: conteos y promedios.</summary>
        [HttpGet("red/estado")]
        public async Task<IActionResult> GetEstadoRed()
        {
            if (!ApiKeyValida()) return Unauthorized(new { error = "API Key inválida" });

            var redes = await _context.Redes.ToListAsync();
            var alertasPendientes = await _context.Alertas.CountAsync(a => a.Estado == "Pendiente");
            var alertasCriticas   = await _context.Alertas.CountAsync(a => a.Severidad == "Crítica" && a.Estado == "Pendiente");

            return Ok(new
            {
                ok = true,
                ultimaActualizacion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                dispositivos = new
                {
                    total       = redes.Count,
                    activos     = redes.Count(r => r.Estado == "Activo"),
                    advertencia = redes.Count(r => r.Estado == "Advertencia"),
                    falla       = redes.Count(r => r.Estado == "Falla")
                },
                promedios = new
                {
                    cpu      = redes.Any(r => r.CpuPorcentaje.HasValue) ? Math.Round(redes.Where(r => r.CpuPorcentaje.HasValue).Average(r => (double)r.CpuPorcentaje!.Value), 1) : 0,
                    ram      = redes.Any(r => r.RamPorcentaje.HasValue) ? Math.Round(redes.Where(r => r.RamPorcentaje.HasValue).Average(r => (double)r.RamPorcentaje!.Value), 1) : 0,
                    latencia = redes.Any(r => r.LatenciaMs.HasValue) ? Math.Round(redes.Where(r => r.LatenciaMs.HasValue).Average(r => (double)r.LatenciaMs!.Value), 1) : 0
                },
                alertas = new
                {
                    pendientes = alertasPendientes,
                    criticas   = alertasCriticas
                }
            });
        }

        // ── GET /api/v1/ping ─────────────────────────────────────────
        /// <summary>Health-check para el APK.</summary>
        [HttpGet("ping")]
        public IActionResult Ping() => Ok(new { ok = true, mensaje = "NetMonitor API activa", hora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
    }

    public class AtenderRequest
    {
        public string? Usuario { get; set; }
    }
}
