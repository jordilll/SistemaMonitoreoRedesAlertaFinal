using Microsoft.EntityFrameworkCore;
using SistemaMonitoreoRedes.Data;
using SistemaMonitoreoRedes.ViewModels;

namespace SistemaMonitoreoRedes.Services
{
    public class IaService
    {
        private readonly RedesDbContext _context;

        public IaService(RedesDbContext context)
        {
            _context = context;
        }

        public async Task<IaViewModel> AnalizarAsync()
        {
            var redes = await _context.Redes.ToListAsync();
            var vm = new IaViewModel();

            // Detectar alertas individuales
            foreach (var r in redes)
            {
                if (r.CpuPorcentaje > 90)
                    vm.Alertas.Add(new AlertaIa
                    {
                        NombreDispositivo = r.NombreDispositivo ?? "-",
                        Tipo = r.Tipo ?? "-",
                        Ip = r.Ip ?? "-",
                        TipoAlerta = "CPU Crítico",
                        Valor = $"{r.CpuPorcentaje}%",
                        Severidad = "danger",
                        Recomendacion = "Verificar procesos en ejecución y considerar balanceo de carga o actualización de hardware."
                    });

                if (r.RamPorcentaje > 90)
                    vm.Alertas.Add(new AlertaIa
                    {
                        NombreDispositivo = r.NombreDispositivo ?? "-",
                        Tipo = r.Tipo ?? "-",
                        Ip = r.Ip ?? "-",
                        TipoAlerta = "RAM Crítica",
                        Valor = $"{r.RamPorcentaje}%",
                        Severidad = "danger",
                        Recomendacion = "Liberar memoria RAM, revisar fugas de memoria o ampliar capacidad."
                    });

                if (r.LatenciaMs > 50)
                    vm.Alertas.Add(new AlertaIa
                    {
                        NombreDispositivo = r.NombreDispositivo ?? "-",
                        Tipo = r.Tipo ?? "-",
                        Ip = r.Ip ?? "-",
                        TipoAlerta = "Latencia Alta",
                        Valor = $"{r.LatenciaMs} ms",
                        Severidad = "warning",
                        Recomendacion = "Revisar congestión de red, configuración de QoS y rutas de enrutamiento."
                    });

                if (r.Estado == "Falla")
                    vm.Alertas.Add(new AlertaIa
                    {
                        NombreDispositivo = r.NombreDispositivo ?? "-",
                        Tipo = r.Tipo ?? "-",
                        Ip = r.Ip ?? "-",
                        TipoAlerta = "Dispositivo en Falla",
                        Valor = "Falla",
                        Severidad = "danger",
                        Recomendacion = "Revisar conectividad física, reiniciar servicio o escalar a soporte de nivel 2."
                    });

                if (r.RiesgoIa == "Alto")
                    vm.Alertas.Add(new AlertaIa
                    {
                        NombreDispositivo = r.NombreDispositivo ?? "-",
                        Tipo = r.Tipo ?? "-",
                        Ip = r.Ip ?? "-",
                        TipoAlerta = "Riesgo IA Alto",
                        Valor = "Alto",
                        Severidad = "danger",
                        Recomendacion = r.RecomendacionIa ?? "Monitorear continuamente y aplicar medidas preventivas."
                    });
            }

            vm.TotalAlertas = vm.Alertas.Count;
            vm.DispositivosCpuCritico = redes.Count(r => r.CpuPorcentaje > 90);
            vm.DispositivosRamCritico = redes.Count(r => r.RamPorcentaje > 90);
            vm.DispositivosLatenciaCritica = redes.Count(r => r.LatenciaMs > 50);
            vm.DispositivosEnFalla = redes.Count(r => r.Estado == "Falla");
            vm.DispositivosRiesgoAlto = redes.Count(r => r.RiesgoIa == "Alto");

            // Recomendaciones generales
            if (vm.DispositivosCpuCritico > 0)
                vm.Recomendaciones.Add(new RecomendacionIa
                {
                    Categoria = "Rendimiento CPU",
                    Descripcion = $"{vm.DispositivosCpuCritico} dispositivo(s) con CPU superior al 90%. Se recomienda revisar procesos, redistribuir carga y evaluar upgrades de hardware.",
                    Icono = "bi-cpu",
                    ColorClase = "danger",
                    CantidadAfectados = vm.DispositivosCpuCritico
                });

            if (vm.DispositivosRamCritico > 0)
                vm.Recomendaciones.Add(new RecomendacionIa
                {
                    Categoria = "Uso de Memoria RAM",
                    Descripcion = $"{vm.DispositivosRamCritico} dispositivo(s) con RAM superior al 90%. Considera ampliar la memoria o eliminar procesos que no sean críticos.",
                    Icono = "bi-memory",
                    ColorClase = "warning",
                    CantidadAfectados = vm.DispositivosRamCritico
                });

            if (vm.DispositivosLatenciaCritica > 0)
                vm.Recomendaciones.Add(new RecomendacionIa
                {
                    Categoria = "Latencia de Red",
                    Descripcion = $"{vm.DispositivosLatenciaCritica} dispositivo(s) con latencia superior a 50ms. Revisar rutas, configuración DNS y posibles cuellos de botella.",
                    Icono = "bi-speedometer2",
                    ColorClase = "warning",
                    CantidadAfectados = vm.DispositivosLatenciaCritica
                });

            if (vm.DispositivosEnFalla > 0)
                vm.Recomendaciones.Add(new RecomendacionIa
                {
                    Categoria = "Dispositivos en Falla",
                    Descripcion = $"{vm.DispositivosEnFalla} dispositivo(s) en estado de falla. Priorizar revisión física, reinicio de servicios y notificación al equipo técnico.",
                    Icono = "bi-exclamation-triangle",
                    ColorClase = "danger",
                    CantidadAfectados = vm.DispositivosEnFalla
                });

            if (vm.DispositivosRiesgoAlto > 0)
                vm.Recomendaciones.Add(new RecomendacionIa
                {
                    Categoria = "Riesgo IA Alto",
                    Descripcion = $"{vm.DispositivosRiesgoAlto} dispositivo(s) clasificado(s) con riesgo alto por IA. Aplicar medidas preventivas inmediatas según recomendaciones específicas.",
                    Icono = "bi-robot",
                    ColorClase = "danger",
                    CantidadAfectados = vm.DispositivosRiesgoAlto
                });

            if (!vm.Recomendaciones.Any())
                vm.Recomendaciones.Add(new RecomendacionIa
                {
                    Categoria = "Estado General Óptimo",
                    Descripcion = "Todos los dispositivos operan dentro de parámetros normales. Continúe con el monitoreo periódico.",
                    Icono = "bi-check-circle",
                    ColorClase = "success",
                    CantidadAfectados = 0
                });

            return vm;
        }
    }
}
