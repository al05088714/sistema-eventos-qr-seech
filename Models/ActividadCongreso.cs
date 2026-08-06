using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaEventosQR.Models;

[Table("congresoEventos")]
public class ActividadCongreso
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("idactividad")]
    public int IdActividad { get; set; }

    [Column("aannoo")]
    public short? Aannoo { get; set; }

    [Column("cvearc")]
    [StringLength(10)]
    public string? CveArc { get; set; }

    [Column("actividad")]
    [StringLength(50)]
    public string? Actividad { get; set; }

    [Column("codigo")]
    [StringLength(5)]
    public string? Codigo { get; set; }

    [Column("nombre")]
    [StringLength(300)]
    public string? Nombre { get; set; }

    [Column("ponente")]
    [StringLength(300)]
    public string? Ponente { get; set; }

    [Column("subsistema")]
    [StringLength(50)]
    public string? Subsistema { get; set; }

    [Column("nivel")]
    [StringLength(50)]
    public string? Nivel { get; set; }

    [Column("lugar")]
    [StringLength(150)]
    public string? Lugar { get; set; }

    [Column("domicilio")]
    [StringLength(200)]
    public string? Domicilio { get; set; }

    [Column("aforo")]
    public int? Aforo { get; set; }

    [Column("fecha")]
    [StringLength(10)]
    public string? Fecha { get; set; }

    [Column("hora")]
    [StringLength(50)]
    public string? Hora { get; set; }

    [Column("federal")]
    [StringLength(50)]
    public string? Federal { get; set; }

    [Column("estatal")]
    [StringLength(3)]
    public string? Estatal { get; set; }

    [Column("staff")]
    public int? Staff { get; set; }

    [Column("responsablestaff")]
    [StringLength(100)]
    public string? ResponsableStaff { get; set; }

    [Column("modalidad")]
    [StringLength(50)]
    public string? Modalidad { get; set; }

    [Column("curriculum")]
    public string? Curriculum { get; set; }

    [Column("materiales")]
    [StringLength(200)]
    public string? Materiales { get; set; }

    [Column("duracion")]
    [StringLength(50)]
    public string? Duracion { get; set; }

    [Column("guidactividad")]
    [StringLength(36)]
    public string? GuidActividad { get; set; }

    public virtual ICollection<CuotaAforo> Cuotas { get; set; } = new List<CuotaAforo>();
    public virtual ICollection<RegistroAlumno> Registros { get; set; } = new List<RegistroAlumno>();
}