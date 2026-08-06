using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaEventosQR.Models;

[Table("congresoEventosCuotas")]
public class CuotaAforo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("idcuota")]
    public int IdCuota { get; set; }

    [Column("idactividad")]
    public int IdActividad { get; set; }

    [Column("subsistema")]
    [StringLength(1)]
    public string Subsistema { get; set; } = null!;

    [Column("modalidad")]
    [StringLength(100)]
    public string Modalidad { get; set; } = null!;

    [Column("aforo")]
    public short Aforo { get; set; }

    [Column("registros")]
    public short Registros { get; set; }

    [ForeignKey("IdActividad")]
    public virtual ActividadCongreso? Actividad { get; set; }
}