using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaEventosQR.Models;

[Table("sieusuarios")]
public class UsuarioSistema
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("idusuario")]
    public int IdUsuario { get; set; }

    [Column("usuario")]
    [StringLength(50)]
    public string Usuario { get; set; } = null!;

    [Column("password")]
    [StringLength(100)]
    public string Password { get; set; } = null!;

    [Column("nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column("usrARC")]
    [StringLength(2)]
    public string UsrArc { get; set; } = "99";
}