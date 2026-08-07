namespace SistemaEventosQR.Models
{
    public class ReporteAsistenciaViewModel
    {
        public int IdRegistro { get; set; }
        public string Rfc { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Cct { get; set; } = string.Empty;
        public string Escuela { get; set; } = string.Empty;
        public DateTime? FechaInscripcion { get; set; }
        public DateTime? FechaAsistencia { get; set; }
        public bool HaAsistido { get; set; }
    }
}