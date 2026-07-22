using Microsoft.EntityFrameworkCore;
using SistemaMonitoreoRedes.Models;

namespace SistemaMonitoreoRedes.Data
{
    public class RedesDbContext : DbContext
    {
        public RedesDbContext(DbContextOptions<RedesDbContext> options) : base(options) { }

        // ── Tabla original – NO SE MODIFICA ──────────────────────────
        public DbSet<Red> Redes { get; set; }

        // ── Nueva tabla de alertas ────────────────────────────────────
        public DbSet<Alerta> Alertas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tabla original dbo.redes – sin cambios
            modelBuilder.Entity<Red>(entity =>
            {
                entity.ToTable("redes", "dbo");
                entity.HasKey(e => e.IdMonitoreo);
                entity.Property(e => e.CpuPorcentaje).HasColumnName("cpu_%");
                entity.Property(e => e.RamPorcentaje).HasColumnName("ram_%");
                entity.Property(e => e.DescargaMbps).HasColumnName("descarga_Mbps");
                entity.Property(e => e.SubidaMbps).HasColumnName("subida_Mbps");
            });

            // Nueva tabla dbo.alertas
            modelBuilder.Entity<Alerta>(entity =>
            {
                entity.ToTable("alertas", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FechaHora).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Severidad).HasDefaultValue("Media");
                entity.Property(e => e.Estado).HasDefaultValue("Pendiente");
                entity.Property(e => e.TipoAlerta).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Dispositivo).HasMaxLength(200);
                entity.Property(e => e.Ip).HasMaxLength(50);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.Severidad).HasMaxLength(20);
                entity.Property(e => e.Estado).HasMaxLength(20);
                entity.Property(e => e.UsuarioAtendio).HasMaxLength(100);

                // Índice para consultas frecuentes
                entity.HasIndex(e => e.Estado);
                entity.HasIndex(e => e.FechaHora);
                entity.HasIndex(e => e.Severidad);
            });
        }
    }
}
