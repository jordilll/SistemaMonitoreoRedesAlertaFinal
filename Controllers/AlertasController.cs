using Microsoft.AspNetCore.Mvc;
using SistemaMonitoreoRedes.Helpers;
using SistemaMonitoreoRedes.Services;
using SistemaMonitoreoRedes.ViewModels;

namespace SistemaMonitoreoRedes.Controllers
{
    [AuthFilter]
    public class AlertasController : Controller
    {
        private readonly AlertService _alertService;

        public AlertasController(AlertService alertService)
        {
            _alertService = alertService;
        }

        // GET /Alertas
        public async Task<IActionResult> Index(
            string? filtroEstado,
            string? filtroSeveridad,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            var todas = await _alertService.GetTodasAsync();

            if (!string.IsNullOrEmpty(filtroEstado))
                todas = todas.Where(a => a.Estado == filtroEstado).ToList();

            if (!string.IsNullOrEmpty(filtroSeveridad))
                todas = todas.Where(a => a.Severidad == filtroSeveridad).ToList();

            if (fechaInicio.HasValue)
                todas = todas.Where(a => a.FechaHora >= fechaInicio.Value).ToList();

            if (fechaFin.HasValue)
                todas = todas.Where(a => a.FechaHora <= fechaFin.Value.AddDays(1)).ToList();

            var vm = new AlertasIndexViewModel
            {
                Alertas         = todas,
                FiltroEstado    = filtroEstado,
                FiltroSeveridad = filtroSeveridad,
                FechaInicio     = fechaInicio,
                FechaFin        = fechaFin,
                TotalAlertas    = todas.Count,
                TotalPendientes = todas.Count(a => a.Estado == "Pendiente"),
                TotalCriticas   = todas.Count(a => a.Severidad == "Crítica" && a.Estado == "Pendiente")
            };

            return View(vm);
        }

        // POST /Alertas/AnalizarDatos  ← NUEVO: escanea dbo.redes y genera alertas
        [HttpPost]
        public async Task<IActionResult> AnalizarDatos()
        {
            var nuevas = await _alertService.AnalizarTodosAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { ok = true, nuevas, mensaje = $"Se generaron {nuevas} alerta(s) nueva(s)." });

            TempData["Exito"] = nuevas > 0
                ? $"Análisis completado: {nuevas} alerta(s) nueva(s) generada(s)."
                : "Análisis completado. No se encontraron nuevas condiciones de alerta.";

            return RedirectToAction(nameof(Index));
        }

        // POST /Alertas/MarcarAtendida/{id}
        [HttpPost]
        public async Task<IActionResult> MarcarAtendida(int id)
        {
            var ok = await _alertService.MarcarAtendidaAsync(id, "admin");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { ok });

            TempData["Exito"] = ok ? "Alerta marcada como atendida." : "No se encontró la alerta.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Alertas/MarcarTodasAtendidas
        [HttpPost]
        public async Task<IActionResult> MarcarTodasAtendidas()
        {
            var pendientes = await _alertService.GetPendientesAsync();
            foreach (var a in pendientes)
                await _alertService.MarcarAtendidaAsync(a.Id, "admin");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { ok = true, atendidas = pendientes.Count });

            TempData["Exito"] = $"{pendientes.Count} alerta(s) marcadas como atendidas.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Alertas/ContadorPendientes
        [HttpGet]
        public async Task<IActionResult> ContadorPendientes()
        {
            var count = await _alertService.ContarPendientesAsync();
            return Json(new { count });
        }

        // GET /Alertas/Resumen
        [HttpGet]
        public async Task<IActionResult> Resumen()
        {
            var resumen = await _alertService.GetResumenAsync();
            return Json(resumen);
        }
    }
}
