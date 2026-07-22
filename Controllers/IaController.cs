using Microsoft.AspNetCore.Mvc;
using SistemaMonitoreoRedes.Helpers;
using SistemaMonitoreoRedes.Services;

namespace SistemaMonitoreoRedes.Controllers
{
    [AuthFilter]
    public class IaController : Controller
    {
        private readonly IaService _iaService;

        public IaController(IaService iaService)
        {
            _iaService = iaService;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await _iaService.AnalizarAsync();
            return View(vm);
        }
    }
}
