using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEventosQR.Data;
using SistemaEventosQR.Models;

namespace SistemaEventosQR.Controllers;

public class ActividadesController : Controller
{
    private readonly AppDbContext _context;

    public ActividadesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Actividades/Index
    public IActionResult Index()
    {
        return View();
    }

    // GET: Actividades/Listar (Reemplaza a congreso-data.asp?a=listar)
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var actividades = await _context.Actividades
            .Include(a => a.Registros)
            .Select(a => new
            {
                idactividad = a.IdActividad,
                codigo = a.Codigo ?? "",
                nombre = a.Nombre ?? "",
                ponente = a.Ponente ?? "",
                lugar = a.Lugar ?? "",
                fecha = a.Fecha ?? "",
                hora = a.Hora ?? "",
                aforo = a.Aforo ?? 0,
                ocupados = a.Registros.Count,
                nivel = a.Nivel ?? "",
                actividad = a.Actividad ?? "",
                cvearc = a.CveArc ?? "01",
                guid = a.GuidActividad ?? ""
            })
            .ToListAsync();

        var stats = new
        {
            totalEventos = actividades.Count,
            aforoTotal = actividades.Sum(x => x.aforo),
            totalRegistrados = actividades.Sum(x => x.ocupados),
            sedesActivas = actividades.Select(x => x.lugar).Where(l => !string.IsNullOrEmpty(l)).Distinct().Count()
        };

        return Json(new { stats, eventos = actividades });
    }

    // POST: Actividades/Guardar (Reemplaza a congreso-data.asp?a=guardar)
    [HttpPost]
    public async Task<IActionResult> Guardar(ActividadCongreso model)
    {
        try
        {
            if (model.IdActividad == 0)
            {
                // Generar ID máximo manualmente o asignarlo para SQLite
                int maxId = await _context.Actividades.MaxAsync(a => (int?)a.IdActividad) ?? 0;
                model.IdActividad = maxId + 1;
                model.GuidActividad = Guid.NewGuid().ToString();
                model.Aannoo = (short)DateTime.Now.Year;

                _context.Actividades.Add(model);
            }
            else
            {
                var existente = await _context.Actividades.FindAsync(model.IdActividad);
                if (existente == null)
                    return Json(new { success = false, message = "La actividad no existe." });

                existente.Codigo = model.Codigo;
                existente.CveArc = model.CveArc;
                existente.Actividad = model.Actividad;
                existente.Nombre = model.Nombre;
                existente.Ponente = model.Ponente;
                existente.Aforo = model.Aforo;
                existente.Duracion = model.Duracion;
                existente.Subsistema = model.Subsistema;
                existente.Nivel = model.Nivel;
                existente.Modalidad = model.Modalidad;
                existente.Lugar = model.Lugar;
                existente.Domicilio = model.Domicilio;
                existente.Fecha = model.Fecha;
                existente.Hora = model.Hora;
                existente.Federal = model.Federal;
                existente.Estatal = model.Estatal;
                existente.Staff = model.Staff;
                existente.ResponsableStaff = model.ResponsableStaff;
                existente.Materiales = model.Materiales;
                existente.Curriculum = model.Curriculum;

                _context.Actividades.Update(existente);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Actividad guardada correctamente." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error al guardar: " + ex.Message });
        }
    }

    // POST: Actividades/Detalles
    [HttpPost]
    public async Task<IActionResult> Detalles([FromForm] int id)
    {
        var a = await _context.Actividades.FindAsync(id);
        if (a == null)
            return Json(new { success = false, message = "Actividad no encontrada." });

        return Json(new
        {
            success = true,
            idactividad = a.IdActividad,
            codigo = a.Codigo,
            nombre = a.Nombre,
            ponente = a.Ponente,
            aforo = a.Aforo,
            nivel = a.Nivel,
            actividad = a.Actividad,
            cvearc = a.CveArc,
            lugar = a.Lugar,
            fecha = a.Fecha,
            hora = a.Hora,
            duracion = a.Duracion,
            subsistema = a.Subsistema,
            modalidad = a.Modalidad,
            domicilio = a.Domicilio,
            federal = a.Federal,
            estatal = a.Estatal,
            staff = a.Staff,
            responsablestaff = a.ResponsableStaff,
            materiales = a.Materiales,
            curriculum = a.Curriculum
        });
    }

    // POST: Actividades/Eliminar (Reemplaza a congreso-data.asp?a=eliminar)
    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var actividad = await _context.Actividades.FindAsync(id);
        if (actividad == null)
            return Json(new { success = false, message = "La actividad no existe." });

        _context.Actividades.Remove(actividad);
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Actividad eliminada correctamente." });
    }

    // GET: Actividades/ObtenerDetalleEvento?id=1
    [HttpGet]
    public async Task<IActionResult> ObtenerDetalleEvento(int id)
    {
        var actividad = await _context.Actividades
            .Include(a => a.Cuotas)
            .Include(a => a.Registros)
            .FirstOrDefaultAsync(a => a.IdActividad == id);

        if (actividad == null)
            return NotFound("Actividad no encontrada.");

        // Cargar información de profesores asociados a los registros
        var rfcs = actividad.Registros.Select(r => r.Rfc).ToList();
        var profesores = await _context.Profesores
            .Where(p => rfcs.Contains(p.Rfc))
            .ToDictionaryAsync(p => p.Rfc);

        ViewBag.Profesores = profesores;
        return PartialView("_DetalleEventoPartial", actividad);
    }

    // POST: Actividades/ActualizarCuota
    [HttpPost]
    public async Task<IActionResult> ActualizarCuota(int idCuota, string modalidad, short aforo)
    {
        var cuota = await _context.Cuotas.FindAsync(idCuota);
        if (cuota == null)
            return Json(new { status = "error", message = "Cuota no encontrada." });

        cuota.Modalidad = modalidad;
        cuota.Aforo = aforo;

        await _context.SaveChangesAsync();

        // Recalcular aforo global de la actividad
        var aforoGlobal = await _context.Cuotas
            .Where(c => c.IdActividad == cuota.IdActividad)
            .SumAsync(c => (int)c.Aforo);

        var actividad = await _context.Actividades.FindAsync(cuota.IdActividad);
        if (actividad != null)
        {
            actividad.Aforo = aforoGlobal;
            await _context.SaveChangesAsync();
        }

        int porcentaje = aforo > 0 ? (int)Math.Round((double)cuota.Registros / aforo * 100) : 0;

        return Json(new
        {
            status = "success",
            idactividad = cuota.IdActividad,
            registros = cuota.Registros,
            porcentaje,
            aforoGlobal
        });
    }

    // POST: Actividades/AgregarCuota
    [HttpPost]
    public async Task<IActionResult> AgregarCuota(int idActividad, string subsistema, string modalidad, short aforo)
    {
        var actividad = await _context.Actividades.FindAsync(idActividad);
        if (actividad == null)
            return Json(new { status = "error", message = "Actividad no encontrada." });

        if (string.IsNullOrWhiteSpace(modalidad))
            modalidad = "General";

        var nuevaCuota = new CuotaAforo
        {
            IdActividad = idActividad,
            Subsistema = subsistema,
            Modalidad = modalidad,
            Aforo = aforo,
            Registros = 0
        };

        _context.Cuotas.Add(nuevaCuota);
        await _context.SaveChangesAsync();

        // Recalcular el aforo global del evento sumando todas sus cuotas
        var aforoGlobal = await _context.Cuotas
            .Where(c => c.IdActividad == idActividad)
            .SumAsync(c => (int)c.Aforo);

        actividad.Aforo = aforoGlobal;
        await _context.SaveChangesAsync();

        return Json(new
        {
            status = "success",
            message = "Cuota agregada exitosamente.",
            idActividad
        });
    }

    // POST: Actividades/EliminarCuota
    [HttpPost]
    public async Task<IActionResult> EliminarCuota(int idCuota)
    {
        var cuota = await _context.Cuotas.FirstOrDefaultAsync(c => c.IdCuota == idCuota);

        if (cuota == null)
            return Json(new { status = "error", message = "La cuota especificada no existe." });

        // Regla defensiva: Evitar eliminar cuotas con participantes ya inscritos
        if (cuota.Registros > 0)
        {
            return Json(new
            {
                status = "error",
                message = $"No se puede eliminar la cuota porque tiene {cuota.Registros} participante(s) registrado(s)."
            });
        }

        int idActividad = cuota.IdActividad;

        _context.Cuotas.Remove(cuota);
        await _context.SaveChangesAsync();

        // Recalcular el aforo global del evento
        var aforoGlobal = await _context.Cuotas
            .Where(c => c.IdActividad == idActividad)
            .SumAsync(c => (int)c.Aforo);

        var actividad = await _context.Actividades.FindAsync(idActividad);
        if (actividad != null)
        {
            actividad.Aforo = aforoGlobal;
            await _context.SaveChangesAsync();
        }

        return Json(new
        {
            status = "success",
            message = "Cuota eliminada correctamente.",
            idActividad
        });
    }

}