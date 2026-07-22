using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SistemaMonitoreoRedes.Data;
using SistemaMonitoreoRedes.Hubs;
using SistemaMonitoreoRedes.Models;
using SistemaMonitoreoRedes.ViewModels;

namespace SistemaMonitoreoRedes.Services
{
    public class AlertService
    {
        private readonly RedesDbContext _context;
        private readonly IHubContext<AlertasHub> _hub;
        private readonly ILogger<AlertService> _logger;

        public const int UmbralLatenciaMs = 50;

        public AlertService(
            RedesDbContext context,
            IHubContext<AlertasHub> hub,
            ILogger<AlertService> logger)
        {
            _context = context;
            _hub = hub;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────
        // ANALIZAR TODOS LOS REGISTROS DE dbo.redes
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Recorre TODOS los registros de dbo.redes y genera alertas
        /// para cada condición que lo requiera.
        /// Retorna cuántas alertas nuevas se crearon.
        /// </summary>
        public async Task<int> AnalizarTodosAsync()
        {
            var registros = await _context.Redes.ToListAsync();
            int nuevas = 0;

            foreach (var r in registros)
                nuevas += await EvaluarRegistroAsync(r);

            _logger.LogInformation("Análisis completo: {N} alertas nuevas generadas de {T} registros.",
                nuevas, registros.Count);

            return nuevas;
        }

        // ─────────────────────────────────────────────────────────────
        // EVALUAR UN REGISTRO INDIVIDUAL
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Evalúa un registro y genera alertas si corresponde.
        /// Retorna cantidad de alertas creadas.
        /// </summary>
        public async Task EvaluarYRegistrarAsync(Red registro, Red? estadoAnterior = null)
        {
            await EvaluarRegistroAsync(registro, estadoAnterior);
        }

        private async Task<int> EvaluarRegistroAsync(Red r, Red? anterior = null)
        {
            int creadas = 0;

            // 1. Estado = Falla
            if (r.Estado == "Falla")
            {
                bool ok = await RegistrarSiNoExisteRecienteAsync(new Alerta
                {
                    Dispositivo = r.NombreDispositivo,
                    Ip          = r.Ip,
                    TipoAlerta  = "Dispositivo en Falla",
                    Descripcion = $"{r.NombreDispositivo} ({r.Ip}) está en estado Falla. Sin respuesta al ping.",
                    Severidad   = "Crítica"
                });
                if (ok) creadas++;
            }

            // 2. Latencia alta
            if (r.LatenciaMs.HasValue && r.LatenciaMs.Value > UmbralLatenciaMs)
            {
                bool ok = await RegistrarSiNoExisteRecienteAsync(new Alerta
                {
                    Dispositivo = r.NombreDispositivo,
                    Ip          = r.Ip,
                    TipoAlerta  = "Latencia Alta",
                    Descripcion = $"Latencia de {r.LatenciaMs} ms supera el umbral de {UmbralLatenciaMs} ms en {r.NombreDispositivo} ({r.Ip}).",
                    Severidad   = r.LatenciaMs.Value > 200 ? "Alta" : "Media"
                });
                if (ok) creadas++;
            }

            // 3. CPU crítica
            if (r.CpuPorcentaje.HasValue && r.CpuPorcentaje.Value > 90)
            {
                bool ok = await RegistrarSiNoExisteRecienteAsync(new Alerta
                {
                    Dispositivo = r.NombreDispositivo,
                    Ip          = r.Ip,
                    TipoAlerta  = "CPU Crítica",
                    Descripcion = $"CPU al {r.CpuPorcentaje}% en {r.NombreDispositivo} ({r.Ip}). Revisar procesos.",
                    Severidad   = "Alta"
                });
                if (ok) creadas++;
            }

            // 4. RAM crítica
            if (r.RamPorcentaje.HasValue && r.RamPorcentaje.Value > 90)
            {
                bool ok = await RegistrarSiNoExisteRecienteAsync(new Alerta
                {
                    Dispositivo = r.NombreDispositivo,
                    Ip          = r.Ip,
                    TipoAlerta  = "RAM Crítica",
                    Descripcion = $"RAM al {r.RamPorcentaje}% en {r.NombreDispositivo} ({r.Ip}). Riesgo de saturación.",
                    Severidad   = "Alta"
                });
                if (ok) creadas++;
            }

            // 5. Riesgo IA = Alto
            if (r.RiesgoIa == "Alto")
            {
                bool ok = await RegistrarSiNoExisteRecienteAsync(new Alerta
                {
                    Dispositivo = r.NombreDispositivo,
                    Ip          = r.Ip,
                    TipoAlerta  = "Riesgo IA Alto",
                    Descripcion = r.RecomendacionIa ?? $"Dispositivo {r.NombreDispositivo} clasificado con riesgo IA Alto.",
                    Severidad   = "Alta"
                });
                if (ok) creadas++;
            }

            // 6. Cambio Activo → Inactivo/Falla
            if (anterior != null && anterior.Estado == "Activo" && r.Estado != "Activo")
            {
                await RegistrarAlertaAsync(new Alerta
                {
                    Dispositivo = r.NombreDispositivo,
                    Ip          = r.Ip,
                    TipoAlerta  = "Dispositivo Inactivo",
                    Descripcion = $"{r.NombreDispositivo} ({r.Ip}) cambió de Activo a {r.Estado}.",
                    Severidad   = "Alta"
                });
                creadas++;
            }

            // 7. Recuperación: Inactivo/Falla → Activo
            if (anterior != null && anterior.Estado != "Activo" && r.Estado == "Activo")
            {
                await RegistrarAlertaAsync(new Alerta
                {
                    Dispositivo = r.NombreDispositivo,
                    Ip          = r.Ip,
                    TipoAlerta  = "Dispositivo Recuperado",
                    Descripcion = $"{r.NombreDispositivo} ({r.Ip}) volvió a estar Activo.",
                    Severidad   = "Baja"
                });
                creadas++;
            }

            return creadas;
        }

        // ─────────────────────────────────────────────────────────────
        // REGISTRO DIRECTO
        // ─────────────────────────────────────────────────────────────

        public async Task RegistrarAlertaAsync(Alerta alerta)
        {
            try
            {
                alerta.FechaHora = DateTime.Now;
                alerta.Estado    = "Pendiente";
                _context.Alertas.Add(alerta);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Alerta [{Sev}] {Tipo} – {Ip}",
                    alerta.Severidad, alerta.TipoAlerta, alerta.Ip);

                await EmitirAlertaSignalRAsync(alerta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar alerta para {Ip}", alerta.Ip);
            }
        }

        /// <summary>
        /// Registra solo si no existe la misma alerta en los últimos 60 minutos.
        /// Devuelve true si se creó, false si ya existía.
        /// </summary>
        private async Task<bool> RegistrarSiNoExisteRecienteAsync(Alerta alerta)
        {
            var limite = DateTime.Now.AddHours(-1);
            var existe = await _context.Alertas.AnyAsync(a =>
                a.Ip        == alerta.Ip &&
                a.TipoAlerta == alerta.TipoAlerta &&
                a.FechaHora >= limite);

            if (existe) return false;

            await RegistrarAlertaAsync(alerta);
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // CONSULTAS
        // ─────────────────────────────────────────────────────────────

        public async Task<List<Alerta>> GetTodasAsync()
            => await _context.Alertas.OrderByDescending(a => a.FechaHora).ToListAsync();

        public async Task<List<Alerta>> GetPendientesAsync()
            => await _context.Alertas
                .Where(a => a.Estado == "Pendiente")
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();

        public async Task<List<Alerta>> GetUltimasAsync(int cantidad = 10)
            => await _context.Alertas
                .OrderByDescending(a => a.FechaHora)
                .Take(cantidad)
                .ToListAsync();

        public async Task<int> ContarPendientesAsync()
            => await _context.Alertas.CountAsync(a => a.Estado == "Pendiente");

        public async Task<int> ContarCriticasActivasAsync()
            => await _context.Alertas.CountAsync(a => a.Severidad == "Crítica" && a.Estado == "Pendiente");

        public async Task<ResumenAlertasViewModel> GetResumenAsync()
        {
            var todas = await _context.Alertas.ToListAsync();
            return new ResumenAlertasViewModel
            {
                Total            = todas.Count,
                Pendientes       = todas.Count(a => a.Estado == "Pendiente"),
                Atendidas        = todas.Count(a => a.Estado == "Atendida"),
                Criticas         = todas.Count(a => a.Severidad == "Crítica"  && a.Estado == "Pendiente"),
                Altas            = todas.Count(a => a.Severidad == "Alta"     && a.Estado == "Pendiente"),
                Medias           = todas.Count(a => a.Severidad == "Media"    && a.Estado == "Pendiente"),
                Bajas            = todas.Count(a => a.Severidad == "Baja"     && a.Estado == "Pendiente"),
                UltimasAlertas   = todas.OrderByDescending(a => a.FechaHora).Take(10).ToList()
            };
        }

        // ─────────────────────────────────────────────────────────────
        // MARCAR COMO ATENDIDA
        // ─────────────────────────────────────────────────────────────

        public async Task<bool> MarcarAtendidaAsync(int id, string usuario = "admin")
        {
            var alerta = await _context.Alertas.FindAsync(id);
            if (alerta == null) return false;

            alerta.Estado        = "Atendida";
            alerta.FechaAtendida = DateTime.Now;
            alerta.UsuarioAtendio = usuario;
            await _context.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("ActualizarContadorAlertas",
                await ContarPendientesAsync());
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // SignalR
        // ─────────────────────────────────────────────────────────────

        private async Task EmitirAlertaSignalRAsync(Alerta alerta)
        {
            try
            {
                await _hub.Clients.All.SendAsync("NuevaAlerta", new
                {
                    alerta.Id,
                    FechaHora   = alerta.FechaHora.ToString("dd/MM/yyyy HH:mm:ss"),
                    alerta.Dispositivo,
                    alerta.Ip,
                    alerta.TipoAlerta,
                    alerta.Descripcion,
                    alerta.Severidad,
                    alerta.Estado
                });

                await _hub.Clients.All.SendAsync("ActualizarContadorAlertas",
                    await ContarPendientesAsync());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error emitiendo alerta por SignalR");
            }
        }
    }
}
