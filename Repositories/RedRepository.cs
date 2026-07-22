using Microsoft.EntityFrameworkCore;
using SistemaMonitoreoRedes.Data;
using SistemaMonitoreoRedes.Models;

namespace SistemaMonitoreoRedes.Repositories
{
    public interface IRedRepository
    {
        Task<List<Red>> GetAllAsync();
        Task<Red?> GetByIdAsync(int id);
        Task<Red> CreateAsync(Red red);
        Task<Red> UpdateAsync(Red red);
        Task DeleteAsync(int id);
        Task<List<Red>> GetFilteredAsync(string? estado, string? tipo, string? marca, string? ubicacion, string? ip, string? nombre, string? busqueda, string? orden, int pagina, int pageSize, out int total);
        Task<List<string>> GetDistinctEstadosAsync();
        Task<List<string>> GetDistinctTiposAsync();
        Task<List<string>> GetDistinctMarcasAsync();
        Task<List<string>> GetDistinctUbicacionesAsync();
        Task<List<Red>> GetRecentAsync(int count);
        Task<List<Red>> GetForReportAsync(DateTime? fechaInicio, DateTime? fechaFin, string? estado, string? tipo, string? marca, string? ubicacion);
    }

    public class RedRepository : IRedRepository
    {
        private readonly RedesDbContext _context;

        public RedRepository(RedesDbContext context)
        {
            _context = context;
        }

        public async Task<List<Red>> GetAllAsync()
            => await _context.Redes.OrderByDescending(r => r.FechaHora).ToListAsync();

        public async Task<Red?> GetByIdAsync(int id)
            => await _context.Redes.FindAsync(id);

        public async Task<Red> CreateAsync(Red red)
        {
            _context.Redes.Add(red);
            await _context.SaveChangesAsync();
            return red;
        }

        public async Task<Red> UpdateAsync(Red red)
        {
            _context.Redes.Update(red);
            await _context.SaveChangesAsync();
            return red;
        }

        public async Task DeleteAsync(int id)
        {
            var red = await _context.Redes.FindAsync(id);
            if (red != null)
            {
                _context.Redes.Remove(red);
                await _context.SaveChangesAsync();
            }
        }

        public Task<List<Red>> GetFilteredAsync(string? estado, string? tipo, string? marca, string? ubicacion, string? ip, string? nombre, string? busqueda, string? orden, int pagina, int pageSize, out int total)
        {
            var query = _context.Redes.AsQueryable();

            if (!string.IsNullOrEmpty(estado)) query = query.Where(r => r.Estado == estado);
            if (!string.IsNullOrEmpty(tipo)) query = query.Where(r => r.Tipo == tipo);
            if (!string.IsNullOrEmpty(marca)) query = query.Where(r => r.Marca == marca);
            if (!string.IsNullOrEmpty(ubicacion)) query = query.Where(r => r.Ubicacion == ubicacion);
            if (!string.IsNullOrEmpty(ip)) query = query.Where(r => r.Ip != null && r.Ip.Contains(ip));
            if (!string.IsNullOrEmpty(nombre)) query = query.Where(r => r.NombreDispositivo != null && r.NombreDispositivo.Contains(nombre));
            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(r =>
                    (r.NombreDispositivo != null && r.NombreDispositivo.Contains(busqueda)) ||
                    (r.Ip != null && r.Ip.Contains(busqueda)) ||
                    (r.Marca != null && r.Marca.Contains(busqueda)) ||
                    (r.Tipo != null && r.Tipo.Contains(busqueda)) ||
                    (r.Ubicacion != null && r.Ubicacion.Contains(busqueda)));

            query = orden switch
            {
                "nombre" => query.OrderBy(r => r.NombreDispositivo),
                "nombre_desc" => query.OrderByDescending(r => r.NombreDispositivo),
                "cpu" => query.OrderByDescending(r => r.CpuPorcentaje),
                "ram" => query.OrderByDescending(r => r.RamPorcentaje),
                "latencia" => query.OrderByDescending(r => r.LatenciaMs),
                "fecha" => query.OrderBy(r => r.FechaHora),
                _ => query.OrderByDescending(r => r.FechaHora)
            };

            total = query.Count();
            var result = query.Skip((pagina - 1) * pageSize).Take(pageSize).ToList();
            return Task.FromResult(result);
        }

        public async Task<List<string>> GetDistinctEstadosAsync()
            => await _context.Redes.Where(r => r.Estado != null).Select(r => r.Estado!).Distinct().OrderBy(x => x).ToListAsync();

        public async Task<List<string>> GetDistinctTiposAsync()
            => await _context.Redes.Where(r => r.Tipo != null).Select(r => r.Tipo!).Distinct().OrderBy(x => x).ToListAsync();

        public async Task<List<string>> GetDistinctMarcasAsync()
            => await _context.Redes.Where(r => r.Marca != null).Select(r => r.Marca!).Distinct().OrderBy(x => x).ToListAsync();

        public async Task<List<string>> GetDistinctUbicacionesAsync()
            => await _context.Redes.Where(r => r.Ubicacion != null).Select(r => r.Ubicacion!).Distinct().OrderBy(x => x).ToListAsync();

        public async Task<List<Red>> GetRecentAsync(int count)
            => await _context.Redes.OrderByDescending(r => r.FechaHora).Take(count).ToListAsync();

        public async Task<List<Red>> GetForReportAsync(DateTime? fechaInicio, DateTime? fechaFin, string? estado, string? tipo, string? marca, string? ubicacion)
        {
            var query = _context.Redes.AsQueryable();
            if (fechaInicio.HasValue) query = query.Where(r => r.FechaHora >= fechaInicio.Value);
            if (fechaFin.HasValue) query = query.Where(r => r.FechaHora <= fechaFin.Value.AddDays(1));
            if (!string.IsNullOrEmpty(estado)) query = query.Where(r => r.Estado == estado);
            if (!string.IsNullOrEmpty(tipo)) query = query.Where(r => r.Tipo == tipo);
            if (!string.IsNullOrEmpty(marca)) query = query.Where(r => r.Marca == marca);
            if (!string.IsNullOrEmpty(ubicacion)) query = query.Where(r => r.Ubicacion == ubicacion);
            return await query.OrderByDescending(r => r.FechaHora).ToListAsync();
        }
    }
}
