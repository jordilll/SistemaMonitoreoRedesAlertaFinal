using Microsoft.AspNetCore.Mvc;
using SistemaMonitoreoRedes.Helpers;
using SistemaMonitoreoRedes.Models;
using SistemaMonitoreoRedes.Repositories;
using SistemaMonitoreoRedes.ViewModels;

namespace SistemaMonitoreoRedes.Controllers
{
    [AuthFilter]
    public class CrudController : Controller
    {
        private readonly IRedRepository _repo;
        private const int PageSize = 10;

        public CrudController(IRedRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index(string? busqueda, string? filtroEstado, string? filtroTipo,
            string? filtroMarca, string? filtroUbicacion, string? filtroIp, string? filtroNombre,
            string? orden, int pagina = 1)
        {
            var registros = await _repo.GetFilteredAsync(filtroEstado, filtroTipo, filtroMarca, filtroUbicacion,
                filtroIp, filtroNombre, busqueda, orden, pagina, PageSize, out int total);

            var vm = new CrudViewModel
            {
                Registros = registros,
                PaginaActual = pagina,
                TotalPaginas = (int)Math.Ceiling((double)total / PageSize),
                TotalRegistros = total,
                Busqueda = busqueda,
                FiltroEstado = filtroEstado,
                FiltroTipo = filtroTipo,
                FiltroMarca = filtroMarca,
                FiltroUbicacion = filtroUbicacion,
                FiltroIp = filtroIp,
                FiltroNombre = filtroNombre,
                Orden = orden,
                Estados = await _repo.GetDistinctEstadosAsync(),
                Tipos = await _repo.GetDistinctTiposAsync(),
                Marcas = await _repo.GetDistinctMarcasAsync(),
                Ubicaciones = await _repo.GetDistinctUbicacionesAsync()
            };

            return View(vm);
        }

        public IActionResult Create()
        {
            return View(new Red { FechaHora = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Red red)
        {
            if (ModelState.IsValid)
            {
                await _repo.CreateAsync(red);
                TempData["Exito"] = "Registro creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(red);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var red = await _repo.GetByIdAsync(id);
            if (red == null) return NotFound();
            return View(red);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Red red)
        {
            if (ModelState.IsValid)
            {
                await _repo.UpdateAsync(red);
                TempData["Exito"] = "Registro actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(red);
        }

        public async Task<IActionResult> Details(int id)
        {
            var red = await _repo.GetByIdAsync(id);
            if (red == null) return NotFound();
            return View(red);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            TempData["Exito"] = "Registro eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
