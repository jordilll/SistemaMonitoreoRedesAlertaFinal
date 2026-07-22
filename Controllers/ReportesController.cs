using Microsoft.AspNetCore.Mvc;
using SistemaMonitoreoRedes.Helpers;
using SistemaMonitoreoRedes.Repositories;
using SistemaMonitoreoRedes.Services;
using SistemaMonitoreoRedes.ViewModels;

namespace SistemaMonitoreoRedes.Controllers
{
    [AuthFilter]
    public class ReportesController : Controller
    {
        private readonly IRedRepository _repo;
        private readonly ExportService _exportService;

        public ReportesController(IRedRepository repo, ExportService exportService)
        {
            _repo = repo;
            _exportService = exportService;
        }

        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin,
            string? estado, string? tipo, string? marca, string? ubicacion)
        {
            var resultados = await _repo.GetForReportAsync(fechaInicio, fechaFin, estado, tipo, marca, ubicacion);

            var vm = new ReporteFiltroViewModel
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Estado = estado,
                Tipo = tipo,
                Marca = marca,
                Ubicacion = ubicacion,
                Resultados = resultados,
                TotalResultados = resultados.Count,
                CpuPromedio = resultados.Any(r => r.CpuPorcentaje.HasValue) ? Math.Round(resultados.Where(r => r.CpuPorcentaje.HasValue).Average(r => r.CpuPorcentaje!.Value), 1) : 0,
                RamPromedio = resultados.Any(r => r.RamPorcentaje.HasValue) ? Math.Round(resultados.Where(r => r.RamPorcentaje.HasValue).Average(r => r.RamPorcentaje!.Value), 1) : 0,
                LatenciaPromedio = resultados.Any(r => r.LatenciaMs.HasValue) ? Math.Round(resultados.Where(r => r.LatenciaMs.HasValue).Average(r => r.LatenciaMs!.Value), 1) : 0,
                Estados = await _repo.GetDistinctEstadosAsync(),
                Tipos = await _repo.GetDistinctTiposAsync(),
                Marcas = await _repo.GetDistinctMarcasAsync(),
                Ubicaciones = await _repo.GetDistinctUbicacionesAsync()
            };

            return View(vm);
        }

        public async Task<IActionResult> ExportarExcel(DateTime? fechaInicio, DateTime? fechaFin,
            string? estado, string? tipo, string? marca, string? ubicacion)
        {
            var datos = await _repo.GetForReportAsync(fechaInicio, fechaFin, estado, tipo, marca, ubicacion);
            var bytes = _exportService.ExportarExcel(datos);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ReporteRedes_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        public async Task<IActionResult> ExportarPdf(DateTime? fechaInicio, DateTime? fechaFin,
            string? estado, string? tipo, string? marca, string? ubicacion)
        {
            var datos = await _repo.GetForReportAsync(fechaInicio, fechaFin, estado, tipo, marca, ubicacion);
            var bytes = _exportService.ExportarPdf(datos);
            return File(bytes, "application/pdf", $"ReporteRedes_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
    }
}
