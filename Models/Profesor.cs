using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaEventosQR.Models;

[Table("profesores")]
public class Profesor
{
    [Key]
    [Column("rfc")]
    [StringLength(13)]
    public string Rfc { get; set; } = null!;

    [Column("nombre")]
    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    [Column("apepat")]
    [StringLength(50)]
    public string ApePat { get; set; } = null!;

    [Column("apemat")]
    [StringLength(50)]
    public string ApeMat { get; set; } = null!;

    [Column("cct")]
    [StringLength(10)]
    public string Cct { get; set; } = null!;

    [Column("subsistema")]
    [StringLength(1)]
    public string Subsistema { get; set; } = "F";
}