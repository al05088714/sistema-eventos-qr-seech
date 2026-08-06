using Microsoft.EntityFrameworkCore;
using SistemaEventosQR.Data;

var builder = WebApplication.CreateBuilder(args);

// Configurar SQLite en memoria/archivo local
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=congreso.db"));

builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

// Inicializar la Base de Datos al arrancar
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbInitializer.Initialize(dbContext);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();