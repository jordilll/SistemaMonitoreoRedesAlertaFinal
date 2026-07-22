using Microsoft.AspNetCore.SignalR;

namespace SistemaMonitoreoRedes.Hubs
{
    /// <summary>
    /// Hub de SignalR para notificaciones de alertas en tiempo real.
    /// Eventos emitidos:
    ///   – NuevaAlerta(objeto):          nueva alerta creada
    ///   – ActualizarContadorAlertas(n): actualiza el badge de la campana
    /// </summary>
    public class AlertasHub : Hub
    {
        private readonly ILogger<AlertasHub> _logger;

        public AlertasHub(ILogger<AlertasHub> logger)
        {
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogDebug("Cliente conectado al hub de alertas: {Id}", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogDebug("Cliente desconectado del hub de alertas: {Id}", Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
