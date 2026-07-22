using Microsoft.EntityFrameworkCore;
using SistemaMonitoreoRedes.Data;
using SistemaMonitoreoRedes.ViewModels;

namespace SistemaMonitoreoRedes.Services
{
    public class DashboardService
    {
        private readonly RedesDbContext _context;

        public DashboardService(RedesDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var redes = await _context.Redes.ToListAsync();

            var vm = new DashboardViewModel
            {
                TotalRegistros    = redes.Count,
                EquiposActivos    = redes.Count(r => r.Estado == "Activo"),
                EquiposAdvertencia = redes.Count(r => r.Estado == "Advertencia"),
                EquiposFalla      = redes.Count(r => r.Estado == "Falla"),
                CpuPromedio       = redes.Any(r => r.CpuPorcentaje.HasValue)
                    ? Math.Round(redes.Where(r => r.CpuPorcentaje.HasValue).Average(r => r.CpuPorcentaje!.Value), 1) : 0,
                RamPromedio       = redes.Any(r => r.RamPorcentaje.HasValue)
                    ? Math.Round(redes.Where(r => r.RamPorcentaje.HasValue).Average(r => r.RamPorcentaje!.Value), 1) : 0,
                DescargaPromedio  = redes.Any(r => r.DescargaMbps.HasValue)
                    ? Math.Round(redes.Where(r => r.DescargaMbps.HasValue).Average(r => r.DescargaMbps!.Value), 1) : 0,
                SubidaPromedio    = redes.Any(r => r.SubidaMbps.HasValue)
                    ? Math.Round(redes.Where(r => r.SubidaMbps.HasValue).Average(r => r.SubidaMbps!.Value), 1) : 0,
                LatenciaPromedio  = redes.Any(r => r.LatenciaMs.HasValue)
                    ? Math.Round(redes.Where(r => r.LatenciaMs.HasValue).Average(r => r.LatenciaMs!.Value), 1) : 0,
                EstadoDistribucion   = redes.Where(r => r.Estado != null).GroupBy(r => r.Estado!).ToDictionary(g => g.Key, g => g.Count()),
                TipoDistribucion     = redes.Where(r => r.Tipo != null).GroupBy(r => r.Tipo!).ToDictionary(g => g.Key, g => g.Count()),
                UbicacionDistribucion = redes.Where(r => r.Ubicacion != null).GroupBy(r => r.Ubicacion!).ToDictionary(g => g.Key, g => g.Count()),
            };

            var recientes = redes.OrderByDescending(r => r.FechaHora).Take(10).OrderBy(r => r.FechaHora).ToList();
            vm.FechasRecientes  = recientes.Select(r => r.FechaHora?.ToString("HH:mm") ?? "").ToList();
            vm.CpuReciente      = recientes.Select(r => r.CpuPorcentaje ?? 0).ToList();
            vm.RamReciente      = recientes.Select(r => r.RamPorcentaje ?? 0).ToList();
            vm.DescargaReciente = recientes.Select(r => r.DescargaMbps ?? 0).ToList();
            vm.SubidaReciente   = recientes.Select(r => r.SubidaMbps ?? 0).ToList();

            // ── Datos de alertas para el Dashboard ───────────────────
            var alertas = await _context.Alertas.OrderByDescending(a => a.FechaHora).Take(50).ToListAsync();
            vm.TotalAlertas      = await _context.Alertas.CountAsync();
            vm.AlertasPendientes = await _context.Alertas.CountAsync(a => a.Estado == "Pendiente");
            vm.AlertasCriticas   = await _context.Alertas.CountAsync(a => a.Severidad == "Crítica" && a.Estado == "Pendiente");
            vm.UltimasAlertas    = alertas.Take(5).ToList();

            return vm;
        }
    }
}
