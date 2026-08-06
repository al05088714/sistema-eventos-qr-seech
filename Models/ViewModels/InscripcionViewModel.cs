using System.ComponentModel.DataAnnotations;

namespace SistemaEventosQR.Models.ViewModels
{
    public class InscripcionViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar una actividad.")]
        [Display(Name = "Actividad")]
        public int IdActividad { get; set; }

        [Required(ErrorMessage = "El RFC es obligatorio.")]
        [StringLength(13, MinimumLength = 12, ErrorMessage = "El RFC debe tener entre 12 y 13 caracteres.")]
        [RegularExpression(@"^[A-Za-zñÑ&]{3,4}\d{6}[A-Za-z0-9]{3}$", ErrorMessage = "El formato del RFC no es válido.")]
        [Display(Name = "RFC del Participante")]
        public string Curp { get; set; } = string.Empty; // Campo utilizado para RFC

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres.")]
        [Display(Name = "Nombre(s)")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido paterno no puede exceder 50 caracteres.")]
        [Display(Name = "Apellido Paterno")]
        public string ApePat { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido materno es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido materno no puede exceder 50 caracteres.")]
        [Display(Name = "Apellido Materno")]
        public string ApeMat { get; set; } = string.Empty;

        [Required(ErrorMessage = "La CCT es obligatoria.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "La CCT debe tener exactamente 10 caracteres.")]
        [Display(Name = "Clave de Centro de Trabajo (CCT)")]
        public string Cct { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione el subsistema.")]
        [StringLength(1)]
        [Display(Name = "Subsistema")]
        public string Subsistema { get; set; } = "F";
    }
}