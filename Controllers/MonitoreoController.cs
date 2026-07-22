using Microsoft.AspNetCore.Mvc;
using SistemaMonitoreoRedes.Helpers;
using SistemaMonitoreoRedes.Repositories;

namespace SistemaMonitoreoRedes.Controllers
{
    [AuthFilter]
    public class MonitoreoController : Controller
    {
        private readonly IRedRepository _repo;

        public MonitoreoController(IRedRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDatosMonitoreo()
        {
            var datos = await _repo.GetRecentAsync(50);
            var result = datos.Select(r => new
            {
                r.IdMonitoreo,
                FechaHora = r.FechaHora?.ToString("dd/MM/yyyy HH:mm:ss") ?? "",
                r.NombreDispositivo,
                r.Tipo,
                r.Ip,
                r.Estado,
                Cpu = r.CpuPorcentaje ?? 0,
                Ram = r.RamPorcentaje ?? 0,
                Descarga = r.DescargaMbps ?? 0,
                Subida = r.SubidaMbps ?? 0,
                Latencia = r.LatenciaMs ?? 0,
                r.Alerta,
                r.Incidencia,
                r.RiesgoIa
            });
            return Json(result);
        }
    }
}
