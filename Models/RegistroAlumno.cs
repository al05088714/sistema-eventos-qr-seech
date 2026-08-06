using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaEventosQR.Models;

[Table("congresoRegistros")]
public class RegistroAlumno
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("idregistro")]
    public int IdRegistro { get; set; }

    [Column("idactividad")]
    public int IdActividad { get; set; }

    [Column("idcuota")]
    public int IdCuota { get; set; }

    [Column("cct")]
    [StringLength(10)]
    public string Cct { get; set; } = null!;

    [Column("rfc")]
    [StringLength(13)]
    public string Rfc { get; set; } = null!;

    [Column("fecharegistro")]
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    [Column("fechaasistencia")]
    public DateTime? FechaAsistencia { get; set; }

    [ForeignKey("IdActividad")]
    public virtual ActividadCongreso? Actividad { get; set; }

    [ForeignKey("IdCuota")]
    public virtual CuotaAforo? Cuota { get; set; }
}