using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
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

        // GET: Inscripcion/Asistencia/5 o Inscripcion/Asistencia?g=guid-de-evento
        [HttpGet]
        [Route("Inscripcion/Asistencia/{idActividad:int?}")]
        public async Task<IActionResult> Asistencia(int? idActividad, [FromQuery] string? g)
        {
            var actividades = await _context.Actividades
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            ViewBag.Actividades = actividades;

            int idSeleccionado = 0;

            // Si viene por GUID (ejemplo: ?g=123e4567-e89b-12d3-a456-426614174000)
            if (!string.IsNullOrWhiteSpace(g))
            {
                var actPorGuid = actividades.FirstOrDefault(a => a.GuidActividad.ToString().ToLower() == g.Trim().ToLower());
                if (actPorGuid != null) idSeleccionado = actPorGuid.IdActividad;
            }
            // Si viene por ID directo (ejemplo: /Inscripcion/Asistencia/5)
            else if (idActividad.HasValue)
            {
                idSeleccionado = idActividad.Value;
            }
            // Si no viene ningún parámetro, tomar la primera actividad por defecto
            else
            {
                idSeleccionado = actividades.FirstOrDefault()?.IdActividad ?? 0;
            }

            ViewBag.IdActividadSeleccionada = idSeleccionado;

            if (idSeleccionado == 0) return View(new List<ReporteAsistenciaViewModel>());
            // 2. Consultar registros de la actividad combinando con la tabla profesores
            var datosCrudos = await (from r in _context.Registros
                                     where r.IdActividad == idSeleccionado
                                     join p in _context.Profesores on r.Rfc equals p.Rfc into profGroup
                                     from p in profGroup.DefaultIfEmpty()
                                     select new
                                     {
                                         r.IdRegistro,
                                         r.Rfc,
                                         r.Cct,
                                         r.FechaRegistro,
                                         r.FechaAsistencia,
                                         ProfNombre = p != null ? p.Nombre : null,
                                         ProfApePat = p != null ? p.ApePat : null,
                                         ProfApeMat = p != null ? p.ApeMat : null,
                                         ProfEscuela = p != null ? p.Cct: null
                                     })
                                     .ToListAsync();

            // 3. Formatear nombres y ordenar en memoria (C#)
            var registros = datosCrudos
                .Select(d => new ReporteAsistenciaViewModel
                {
                    IdRegistro = d.IdRegistro,
                    Rfc = d.Rfc,
                    NombreCompleto = !string.IsNullOrEmpty(d.ProfNombre)
                        ? $"{d.ProfNombre} {d.ProfApePat} {d.ProfApeMat}".Trim()
                        : "No registrado en padrón",
                    Cct = d.Cct,
                    Escuela = !string.IsNullOrEmpty(d.ProfEscuela) ? d.ProfEscuela : d.Cct,
                    FechaInscripcion = d.FechaRegistro,
                    FechaAsistencia = d.FechaAsistencia,
                    HaAsistido = d.FechaAsistencia.HasValue
                })
                .OrderByDescending(r => r.HaAsistido)
                .ThenBy(r => r.NombreCompleto)
                .ToList();

            // 3. Métricas para las tarjetas de resumen (KPIs)
            ViewBag.TotalInscritos = registros.Count;
            ViewBag.TotalAsistieron = registros.Count(r => r.HaAsistido);
            ViewBag.TotalPendientes = registros.Count(r => !r.HaAsistido);
            ViewBag.PorcentajeAsistencia = registros.Count > 0
                ? Math.Round((double)ViewBag.TotalAsistieron / ViewBag.TotalInscritos * 100, 1)
                : 0;

            return View(registros);
        }


        // GET: Inscripcion/ExportarAsistenciaExcel?idActividad=5
        [HttpGet]
        public async Task<IActionResult> ExportarAsistenciaExcel(int idActividad)
        {
            var actividad = await _context.Actividades.FirstOrDefaultAsync(a => a.IdActividad == idActividad);
            if (actividad == null) return NotFound();

            // 1. Obtener datos crudos
            var datosCrudos = await (from r in _context.Registros
                                     where r.IdActividad == idActividad
                                     join p in _context.Profesores on r.Rfc equals p.Rfc into profGroup
                                     from p in profGroup.DefaultIfEmpty()
                                     select new
                                     {
                                         r.IdRegistro,
                                         r.Rfc,
                                         r.Cct,
                                         r.FechaRegistro,
                                         r.FechaAsistencia,
                                         ProfNombre = p != null ? p.Nombre : null,
                                         ProfApePat = p != null ? p.ApePat : null,
                                         ProfApeMat = p != null ? p.ApeMat : null,
                                         ProfEscuela = p != null ? p.Cct : null
                                     })
                                     .ToListAsync();

            // 2. Mapear y procesar en C#
            var lista = datosCrudos.Select(d => new
            {
                Estatus = d.FechaAsistencia.HasValue ? "ASISTIÓ" : "PENDIENTE",
                Rfc = d.Rfc,
                NombreCompleto = !string.IsNullOrEmpty(d.ProfNombre)
                    ? $"{d.ProfNombre} {d.ProfApePat} {d.ProfApeMat}".Trim()
                    : "No registrado en padrón",
                Cct = d.Cct,
                Escuela = !string.IsNullOrEmpty(d.ProfEscuela) ? d.ProfEscuela : d.Cct,
                FechaInscripcion = d.FechaRegistro.ToString("dd/MM/yyyy HH:mm"),
                FechaAsistencia = d.FechaAsistencia.HasValue ? d.FechaAsistencia.Value.ToString("dd/MM/yyyy HH:mm:ss") : "-"
            })
            .OrderByDescending(x => x.Estatus)
            .ThenBy(x => x.NombreCompleto)
            .ToList();

            // 3. Construir libro Excel con ClosedXML
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Lista de Asistencia");

                // Encabezado principal del reporte
                worksheet.Cell(1, 1).Value = $"Reporte de Asistencia: {actividad.Nombre}";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;

                // Cabeceras de la tabla
                worksheet.Cell(3, 1).Value = "Estatus";
                worksheet.Cell(3, 2).Value = "RFC";
                worksheet.Cell(3, 3).Value = "Nombre del Asistente";
                worksheet.Cell(3, 4).Value = "CCT";
                worksheet.Cell(3, 5).Value = "Escuela / Centro de Trabajo";
                worksheet.Cell(3, 6).Value = "Fecha Inscripción";
                worksheet.Cell(3, 7).Value = "Fecha Acreditación";

                // Estilo de cabecera
                var headerRange = worksheet.Range("A3:G3");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(63, 81, 181); // Indigo
                headerRange.Style.Font.FontColor = XLColor.White;

                // Llenar datos
                int row = 4;
                foreach (var item in lista)
                {
                    worksheet.Cell(row, 1).Value = item.Estatus;
                    worksheet.Cell(row, 2).Value = item.Rfc;
                    worksheet.Cell(row, 3).Value = item.NombreCompleto;
                    worksheet.Cell(row, 4).Value = item.Cct;
                    worksheet.Cell(row, 5).Value = item.Escuela;
                    worksheet.Cell(row, 6).Value = item.FechaInscripcion;
                    worksheet.Cell(row, 7).Value = item.FechaAsistencia;

                    // Color del estatus
                    if (item.Estatus == "ASISTIÓ")
                    {
                        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.Green;
                        worksheet.Cell(row, 1).Style.Font.Bold = true;
                    }
                    else
                    {
                        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.DarkOrange;
                    }

                    row++;
                }

                // Ajustar ancho automático de columnas
                worksheet.Columns().AdjustToContents();

                // Guardar en Stream de memoria para descarga
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string nombreArchivo = $"Asistencia_{actividad.IdActividad}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
                }
            }
        }
    }
}