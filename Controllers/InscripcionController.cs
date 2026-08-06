using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEventosQR.Data;
using SistemaEventosQR.Models;
using SistemaEventosQR.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace SistemaEventosQR.Controllers
{
    public class InscripcionController : Controller
    {
        private readonly AppDbContext _context;

        public InscripcionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Inscripcion?g=...
        [HttpGet]
        [Route("Inscripcion")]
        public async Task<IActionResult> Index([FromQuery] string? g)
        {
            var model = new InscripcionViewModel();
            List<ActividadCongreso> actividades;

            if (!string.IsNullOrWhiteSpace(g))
            {
                var guidLimpio = g.Trim().ToLower();
                var actividadUnica = await _context.Actividades
                    .FirstOrDefaultAsync(a => a.GuidActividad.ToString().ToLower() == guidLimpio);

                if (actividadUnica == null)
                {
                    ViewBag.ErrorModal = "El enlace del código QR no es válido o el evento no se encuentra activo.";
                    actividades = new List<ActividadCongreso>();
                }
                else
                {
                    actividades = new List<ActividadCongreso> { actividadUnica };
                    model.IdActividad = actividadUnica.IdActividad;
                    ViewBag.EventoUnico = true; // Flag para bloquear la selección en la vista
                }
            }
            else
            {
                // Si no viene parámetro 'g', cargamos el catálogo o mostramos mensaje de requerimiento de QR
                actividades = await _context.Actividades
                    .OrderBy(a => a.Nombre)
                    .ToListAsync();
                ViewBag.EventoUnico = false;
            }

            ViewBag.Actividades = actividades;
            return View(model);
        }

        // GET: Inscripcion/BuscarProfesor?rfc=...
        [HttpGet]
        public async Task<IActionResult> BuscarProfesor(string rfc)
        {
            if (string.IsNullOrWhiteSpace(rfc))
                return Json(new { encontrado = false });

            string rfcLimpio = rfc.Trim().ToUpper();

            var profe = await _context.Profesores
                .FirstOrDefaultAsync(p => p.Rfc == rfcLimpio);

            if (profe != null)
            {
                return Json(new
                {
                    encontrado = true,
                    nombre = profe.Nombre,
                    apePat = profe.ApePat,
                    apeMat = profe.ApeMat,
                    cct = profe.Cct,
                    subsistema = profe.Subsistema
                });
            }

            return Json(new { encontrado = false });
        }

        // POST: Inscripcion/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(InscripcionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await RecargarVistaConError(model);
            }

            string rfcLimpio = model.Curp.Trim().ToUpper();
            string cctLimpio = model.Cct.Trim().ToUpper();

            // 1. Validar existencia de la Actividad
            var actividad = await _context.Actividades.FindAsync(model.IdActividad);
            if (actividad == null)
            {
                ModelState.AddModelError("", "La actividad seleccionada no existe.");
                return await RecargarVistaConError(model);
            }

            // 2. RS-01: Evitar registros duplicados en el evento
            bool duplicado = await _context.Registros
                .AnyAsync(r => r.IdActividad == model.IdActividad && r.Rfc == rfcLimpio);

            if (duplicado)
            {
                ModelState.AddModelError("Curp", "Usted ya se encuentra registrado/a en esta actividad.");
                return await RecargarVistaConError(model);
            }

            // 3. Buscar si existe en la tabla 'profesores', si no, CREARLO
            var profesor = await _context.Profesores.FindAsync(rfcLimpio);
            if (profesor == null)
            {
                profesor = new Profesor
                {
                    Rfc = rfcLimpio,
                    Nombre = model.Nombre.Trim().ToUpper(),
                    ApePat = model.ApePat.Trim().ToUpper(),
                    ApeMat = model.ApeMat.Trim().ToUpper(),
                    Cct = cctLimpio,
                    Subsistema = string.IsNullOrEmpty(model.Subsistema) ? "F" : model.Subsistema.Trim().ToUpper()
                };
                _context.Profesores.Add(profesor);
            }

            // 4. Crear el Registro de Asistencia
            var cuota = await _context.Cuotas.FirstOrDefaultAsync(c => c.IdActividad == model.IdActividad);

            var nuevoRegistro = new RegistroAlumno
            {
                IdActividad = model.IdActividad,
                IdCuota = cuota?.IdCuota ?? (await _context.Cuotas.Select(c => c.IdCuota).FirstOrDefaultAsync()),
                Cct = cctLimpio,
                Rfc = rfcLimpio,
                FechaRegistro = DateTime.Now
            };

            _context.Registros.Add(nuevoRegistro);

            if (cuota != null)
            {
                cuota.Registros++;
            }

            await _context.SaveChangesAsync();

            string tokenRaw = $"EVENTO-{nuevoRegistro.IdRegistro}-{nuevoRegistro.Rfc}-{nuevoRegistro.IdActividad}";
            string tokenHash = GenerarSha256(tokenRaw);

            return RedirectToAction("Comprobante", new { id = nuevoRegistro.IdRegistro, token = tokenHash });
        }

        // GET: Inscripcion/Comprobante
        [HttpGet]
        public async Task<IActionResult> Comprobante(int id, string token)
        {
            var registro = await _context.Registros
                .Include(r => r.Actividad)
                .FirstOrDefaultAsync(r => r.IdRegistro == id);

            if (registro == null)
                return NotFound("Registro no encontrado.");

            // Traer datos completos del Profesor para el Comprobante
            ViewBag.Profesor = await _context.Profesores.FirstOrDefaultAsync(p => p.Rfc == registro.Rfc);
            ViewBag.TokenHash = token;

            return View(registro);
        }

        private async Task<IActionResult> RecargarVistaConError(InscripcionViewModel model)
        {
            ViewBag.Actividades = await _context.Actividades.OrderBy(a => a.Nombre).ToListAsync();
            return View("Index", model);
        }

        private static string GenerarSha256(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString().Substring(0, 32).ToUpper();
            }
        }
    }
}