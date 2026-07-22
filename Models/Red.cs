using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaMonitoreoRedes.Models
{
    [Table("redes", Schema = "dbo")]
    public class Red
    {
        [Key]
        [Column("id_monitoreo")]
        public int IdMonitoreo { get; set; }

        [Column("fecha_hora")]
        [Display(Name = "Fecha/Hora")]
        public DateTime? FechaHora { get; set; }

        [Column("id_dispositivo")]
[Display(Name = "ID Dispositivo")]
public int? IdDispositivo { get; set; }

        [Column("nombre_dispositivo")]
        [Display(Name = "Nombre Dispositivo")]
        public string? NombreDispositivo { get; set; }

        [Column("tipo")]
        [Display(Name = "Tipo")]
        public string? Tipo { get; set; }

        [Column("marca")]
        [Display(Name = "Marca")]
        public string? Marca { get; set; }

        [Column("modelo")]
        [Display(Name = "Modelo")]
        public string? Modelo { get; set; }

        [Column("ip")]
        [Display(Name = "IP")]
        public string? Ip { get; set; }

        [Column("ubicacion")]
        [Display(Name = "Ubicación")]
        public string? Ubicacion { get; set; }

        [Column("estado")]
        [Display(Name = "Estado")]
        public string? Estado { get; set; }

        [Column("cpu_%")]
        [Display(Name = "CPU %")]
        public decimal? CpuPorcentaje { get; set; }

        [Column("ram_%")]
        [Display(Name = "RAM %")]
        public decimal? RamPorcentaje { get; set; }

        [Column("descarga_Mbps")]
        [Display(Name = "Descarga (Mbps)")]
        public decimal? DescargaMbps { get; set; }

        [Column("subida_Mbps")]
        [Display(Name = "Subida (Mbps)")]
        public decimal? SubidaMbps { get; set; }

        [Column("latencia_ms")]
        [Display(Name = "Latencia (ms)")]
        public decimal? LatenciaMs { get; set; }

        [Column("alerta")]
        [Display(Name = "Alerta")]
        public string? Alerta { get; set; }

        [Column("incidencia")]
        [Display(Name = "Incidencia")]
        public string? Incidencia { get; set; }

        [Column("riesgo_ia")]
        [Display(Name = "Riesgo IA")]
        public string? RiesgoIa { get; set; }

        [Column("recomendacion_ia")]
        [Display(Name = "Recomendación IA")]
        public string? RecomendacionIa { get; set; }
    }
}
