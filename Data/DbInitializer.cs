using SistemaEventosQR.Models;

namespace SistemaEventosQR.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Actividades.Any()) return; // Ya inicializado

        // 1. Usuario Admin
        context.Usuarios.Add(new UsuarioSistema
        {
            Usuario = "admin",
            Password = "admin123",
            Nombre = "Miguel Ángel Rodríguez",
            UsrArc = "99"
        });

        // 2. Profesores de Prueba
        context.Profesores.AddRange(new[]
        {
            new Profesor { Rfc = "RODM840612A10", Nombre = "MIGUEL ANGEL", ApePat = "RODRIGUEZ", ApeMat = "LOPEZ", Cct = "08DPR0001A", Subsistema = "F" },
            new Profesor { Rfc = "GARC850315H12", Nombre = "CARLOS", ApePat = "GARCIA", ApeMat = "HERNANDEZ", Cct = "08EJN0005B", Subsistema = "E" }
        });

        // 3. Evento Demo
        var eventoDemo = new ActividadCongreso
        {
            IdActividad = 1,
            Aannoo = 2026,
            CveArc = "01",
            Actividad = "Taller",
            Codigo = "EV001",
            Nombre = "Congreso Internacional de Educación Básica 2026",
            Ponente = "Dr. Carlos Eduardo Moreno",
            Subsistema = "Federalizado",
            Nivel = "Primaria / Secundaria",
            Lugar = "Auditorio Centro de Convenciones",
            Domicilio = "Av. Tecnológico #1200, Chihuahua, Chih.",
            Aforo = 100,
            Fecha = "2026-08-15",
            Hora = "09:00 - 14:00",
            Staff = 5,
            ResponsableStaff = "Ing. Miguel Ángel Rodríguez",
            GuidActividad = "745aaac9-d010-44b6-9ee3-8fb5529abe61"
        };
        context.Actividades.Add(eventoDemo);

        // 4. Cuotas
        context.Cuotas.AddRange(new[]
        {
            new CuotaAforo { IdActividad = 1, Subsistema = "F", Modalidad = "|DPR||BAS|", Aforo = 50, Registros = 1 },
            new CuotaAforo { IdActividad = 1, Subsistema = "E", Modalidad = "|EJN||BAS|", Aforo = 50, Registros = 0 }
        });

        // 5. Registro Inicial
        context.Registros.Add(new RegistroAlumno
        {
            IdActividad = 1,
            IdCuota = 1,
            Cct = "08DPR0001A",
            Rfc = "RODM840612A10",
            FechaRegistro = DateTime.Now
        });

        context.SaveChanges();
    }
}