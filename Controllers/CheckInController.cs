using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEventosQR.Data;
using SistemaEventosQR.Models;

namespace SistemaEventosQR.Controllers
{
    public class CheckInController : Controller
    {
        private readonly AppDbContext _context;

        public CheckInController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CheckIn
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: CheckIn/ValidarAcceso
        [HttpPost]
        public async Task<IActionResult> ValidarAcceso([FromBody] RequestCheckIn request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Token))
            {
                return Json(new { exito = false, estatus = "INVALIDO", mensaje = "Código QR o Token no proporcionado." });
            }

            string tokenLimpio = request.Token.Trim().ToUpper();
            RegistroAlumno? registro = null;

            // 1. Caso A: Si el QR es el token estructurado "EVENTO-{IdRegistro}-{Rfc}-{IdActividad}"
            if (tokenLimpio.StartsWith("EVENTO-"))
            {
                var partes = tokenLimpio.Split('-');
                if (partes.Length >= 3 && int.TryParse(partes[1], out int idRegistro))
                {
                    registro = await _context.Registros
                        .Include(r => r.Actividad)
                        .FirstOrDefaultAsync(r => r.IdRegistro == idRegistro);
                }
            }

            // 2. Caso B: Si el token es solo un ID numérico
            else if (int.TryParse(tokenLimpio, out int idRegDirecto))
            {
                registro = await _context.Registros
                    .Include(r => r.Actividad)
                    .FirstOrDefaultAsync(r => r.IdRegistro == idRegDirecto);
            }

            // 3. Caso C: Si el token escaneado fue el RFC directo
            if (registro == null)
            {
                registro = await _context.Registros
                    .Include(r => r.Actividad)
                    .FirstOrDefaultAsync(r => r.Rfc == tokenLimpio);
            }

            // Si después de los 3 casos no se encontró registro
            if (registro == null)
            {
                return Json(new
                {
                    exito = false,
                    estatus = "INVALIDO",
                    mensaje = "El pase de acceso no es válido o no existe en la base de datos."
                });
            }

            // 4. Buscar datos del Profesor
            var profesor = await _context.Profesores
                .FirstOrDefaultAsync(p => p.Rfc == registro.Rfc);

            string nombreParticipante = profesor != null
                ? $"{profesor.Nombre} {profesor.ApePat} {profesor.ApeMat}".Trim()
                : $"RFC: {registro.Rfc}";

            // 5. Verificar si YA registró asistencia previamente
            if (registro.FechaAsistencia.HasValue)
            {
                return Json(new
                {
                    exito = false,
                    estatus = "DUPLICADO",
                    mensaje = $"¡ATENCIÓN! Este pase ya fue utilizado el {registro.FechaAsistencia.Value:dd/MM/yyyy a las HH:mm:ss} hrs.",
                    participante = nombreParticipante,
                    actividad = registro.Actividad?.Nombre ?? "Evento",
                    rfc = registro.Rfc,
                    cct = registro.Cct
                });
            }

            // 6. Registrar Asistencia (Acreditación Exitosa)
            registro.FechaAsistencia = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new
            {
                exito = true,
                estatus = "EXITO",
                mensaje = "¡Acceso Concedido! Asistencia registrada correctamente.",
                participante = nombreParticipante,
                actividad = registro.Actividad?.Nombre ?? "Evento",
                rfc = registro.Rfc,
                cct = registro.Cct,
                fechaAsistencia = registro.FechaAsistencia.Value.ToString("dd/MM/yyyy HH:mm:ss")
            });
        }

    }

    public class RequestCheckIn
    {
        public string Token { get; set; } = string.Empty;
    }
}