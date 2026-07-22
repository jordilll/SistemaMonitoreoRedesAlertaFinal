using Microsoft.EntityFrameworkCore;
using SistemaMonitoreoRedes.Data;

namespace SistemaMonitoreoRedes.Services
{
    public class ChatbotService
    {
        private readonly RedesDbContext _context;

        public ChatbotService(RedesDbContext context)
        {
            _context = context;
        }

        public async Task<string> ResponderAsync(string pregunta)
        {
            var p = pregunta.ToLower().Trim();
            var redes = await _context.Redes.ToListAsync();

            if (p.Contains("activo") || p.Contains("activos"))
                return $"Actualmente hay <strong>{redes.Count(r => r.Estado == "Activo")}</strong> equipos en estado <span class='badge bg-success'>Activo</span>.";

            if (p.Contains("falla") || p.Contains("fallos") || p.Contains("fallando"))
            {
                var enFalla = redes.Where(r => r.Estado == "Falla").ToList();
                if (!enFalla.Any()) return "No hay equipos en estado de falla en este momento. ✅";
                var lista = string.Join(", ", enFalla.Take(5).Select(r => $"<strong>{r.NombreDispositivo}</strong> ({r.Ip})"));
                return $"Hay <strong>{enFalla.Count}</strong> equipo(s) en falla: {lista}{(enFalla.Count > 5 ? " y más..." : "")}.";
            }

            if (p.Contains("advertencia") || p.Contains("warning"))
                return $"Hay <strong>{redes.Count(r => r.Estado == "Advertencia")}</strong> equipos en estado de <span class='badge bg-warning text-dark'>Advertencia</span>.";

            if (p.Contains("mayor cpu") || p.Contains("más cpu") || p.Contains("cpu más alto") || p.Contains("mayor uso de cpu"))
            {
                var top = redes.Where(r => r.CpuPorcentaje.HasValue).OrderByDescending(r => r.CpuPorcentaje).FirstOrDefault();
                return top != null
                    ? $"El equipo con mayor CPU es <strong>{top.NombreDispositivo}</strong> ({top.Ip}) con <strong>{top.CpuPorcentaje}%</strong> de uso."
                    : "No se encontraron datos de CPU.";
            }

            if (p.Contains("mayor ram") || p.Contains("más ram") || p.Contains("ram más alta"))
            {
                var top = redes.Where(r => r.RamPorcentaje.HasValue).OrderByDescending(r => r.RamPorcentaje).FirstOrDefault();
                return top != null
                    ? $"El equipo con mayor RAM es <strong>{top.NombreDispositivo}</strong> ({top.Ip}) con <strong>{top.RamPorcentaje}%</strong> de uso."
                    : "No se encontraron datos de RAM.";
            }

            if (p.Contains("mayor latencia") || p.Contains("más latencia") || p.Contains("latencia más alta"))
            {
                var top = redes.Where(r => r.LatenciaMs.HasValue).OrderByDescending(r => r.LatenciaMs).FirstOrDefault();
                return top != null
                    ? $"El equipo con mayor latencia es <strong>{top.NombreDispositivo}</strong> ({top.Ip}) con <strong>{top.LatenciaMs} ms</strong>."
                    : "No se encontraron datos de latencia.";
            }

            if (p.Contains("router") || p.Contains("routers"))
                return $"Existen <strong>{redes.Count(r => r.Tipo != null && r.Tipo.ToLower().Contains("router"))}</strong> router(s) registrado(s) en el sistema.";

            if (p.Contains("switch") || p.Contains("switches"))
                return $"Existen <strong>{redes.Count(r => r.Tipo != null && r.Tipo.ToLower().Contains("switch"))}</strong> switch(es) registrado(s) en el sistema.";

            if (p.Contains("servidor") || p.Contains("servidores") || p.Contains("server"))
                return $"Existen <strong>{redes.Count(r => r.Tipo != null && r.Tipo.ToLower().Contains("servidor"))}</strong> servidor(es) registrado(s) en el sistema.";

            if (p.Contains("access point") || p.Contains("ap") || p.Contains("punto de acceso"))
                return $"Existen <strong>{redes.Count(r => r.Tipo != null && (r.Tipo.ToLower().Contains("access point") || r.Tipo.ToLower().Contains("ap")))}</strong> Access Point(s) registrado(s).";

            if (p.Contains("total") || p.Contains("cuántos equipos") || p.Contains("cuantos equipos"))
                return $"El sistema tiene <strong>{redes.Count}</strong> registros en total, de los cuales <strong>{redes.Select(r => r.IdDispositivo).Distinct().Count()}</strong> son dispositivos únicos.";

            if (p.Contains("riesgo alto") || p.Contains("alto riesgo"))
                return $"Hay <strong>{redes.Count(r => r.RiesgoIa == "Alto")}</strong> equipo(s) con riesgo IA clasificado como Alto.";

            if (p.Contains("cpu") && (p.Contains("promedio") || p.Contains("media")))
            {
                var prom = redes.Where(r => r.CpuPorcentaje.HasValue).Average(r => r.CpuPorcentaje!.Value);
                return $"El promedio de CPU en todos los dispositivos es <strong>{prom:F1}%</strong>.";
            }

            if (p.Contains("ram") && (p.Contains("promedio") || p.Contains("media")))
            {
                var prom = redes.Where(r => r.RamPorcentaje.HasValue).Average(r => r.RamPorcentaje!.Value);
                return $"El promedio de RAM en todos los dispositivos es <strong>{prom:F1}%</strong>.";
            }

            if (p.Contains("ubicacion") || p.Contains("ubicación") || p.Contains("ubicaciones"))
            {
                var ubs = redes.Where(r => r.Ubicacion != null).GroupBy(r => r.Ubicacion!).Select(g => $"<li><strong>{g.Key}</strong>: {g.Count()} dispositivo(s)</li>");
                return $"Distribución por ubicación:<ul>{string.Join("", ubs)}</ul>";
            }

            if (p.Contains("tipo") || p.Contains("tipos"))
            {
                var tipos = redes.Where(r => r.Tipo != null).GroupBy(r => r.Tipo!).Select(g => $"<li><strong>{g.Key}</strong>: {g.Count()}</li>");
                return $"Distribución por tipo de dispositivo:<ul>{string.Join("", tipos)}</ul>";
            }

            if (p.Contains("hola") || p.Contains("buenos") || p.Contains("buenas"))
                return "¡Hola! 👋 Soy el asistente del Sistema de Monitoreo de Redes. Puedes preguntarme sobre equipos activos, en falla, uso de CPU/RAM, latencia, tipos de dispositivos y más.";

            if (p.Contains("ayuda") || p.Contains("qué puedes") || p.Contains("que puedes"))
                return "Puedo responder preguntas como:<ul>" +
                    "<li>¿Cuántos equipos activos existen?</li>" +
                    "<li>¿Qué equipos están en falla?</li>" +
                    "<li>¿Cuál tiene mayor CPU / RAM / latencia?</li>" +
                    "<li>¿Cuántos routers / switches / servidores / Access Point existen?</li>" +
                    "<li>¿Cuál es el promedio de CPU / RAM?</li>" +
                    "<li>¿Cuántos equipos hay por ubicación?</li></ul>";

            return "No entendí tu consulta. Intenta preguntar sobre equipos activos, en falla, uso de CPU, RAM, latencia, tipos de dispositivos o ubicaciones. Escribe <strong>ayuda</strong> para ver ejemplos.";
        }
    }
}
