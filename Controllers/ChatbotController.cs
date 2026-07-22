using Microsoft.AspNetCore.Mvc;
using SistemaMonitoreoRedes.Helpers;
using SistemaMonitoreoRedes.Services;

namespace SistemaMonitoreoRedes.Controllers
{
    [AuthFilter]
    public class ChatbotController : Controller
    {
        private readonly ChatbotService _chatbotService;

        public ChatbotController(ChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Consultar([FromBody] ConsultaRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Pregunta))
                return Json(new { respuesta = "Por favor, escribe una pregunta." });

            var respuesta = await _chatbotService.ResponderAsync(req.Pregunta);
            return Json(new { respuesta });
        }
    }

    public class ConsultaRequest
    {
        public string Pregunta { get; set; } = "";
    }
}
