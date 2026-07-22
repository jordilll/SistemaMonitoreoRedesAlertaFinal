using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaMonitoreoRedes.Models
{
    [Table("alertas", Schema = "dbo")]
    public class Alerta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("fecha_hora")]
        [Display(Name = "Fecha/Hora")]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Column("dispositivo")]
        [MaxLength(200)]
        [Display(Name = "Dispositivo")]
        public string? Dispositivo { get; set; }

        [Column("ip")]
        [MaxLength(50)]
        [Display(Name = "IP")]
        public string? Ip { get; set; }

        [Column("tipo_alerta")]
        [MaxLength(100)]
        [Display(Name = "Tipo de Alerta")]
        public string TipoAlerta { get; set; } = string.Empty;

        [Column("descripcion")]
        [MaxLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        /// <summary>Baja | Media | Alta | Crítica</summary>
        [Column("severidad")]
        [MaxLength(20)]
        [Display(Name = "Severidad")]
        public string Severidad { get; set; } = "Media";

        /// <summary>Pendiente | Atendida</summary>
        [Column("estado")]
        [MaxLength(20)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente";

        [Column("fecha_atendida")]
        [Display(Name = "Fecha Atendida")]
        public DateTime? FechaAtendida { get; set; }

        [Column("usuario_atendio")]
        [MaxLength(100)]
        [Display(Name = "Atendida por")]
        public string? UsuarioAtendio { get; set; }
    }
}
