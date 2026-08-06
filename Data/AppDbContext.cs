using Microsoft.EntityFrameworkCore;
using SistemaEventosQR.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SistemaEventosQR.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ActividadCongreso> Actividades { get; set; } = null!;
    public DbSet<CuotaAforo> Cuotas { get; set; } = null!;
    public DbSet<RegistroAlumno> Registros { get; set; } = null!;
    public DbSet<Profesor> Profesores { get; set; } = null!;
    public DbSet<UsuarioSistema> Usuarios { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Restricción Única UQ_actividad_rfc
        modelBuilder.Entity<RegistroAlumno>()
            .HasIndex(r => new { r.IdActividad, r.Rfc })
            .IsUnique()
            .HasDatabaseName("UQ_actividad_rfc");
    }
}