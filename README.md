Sistema de Registro, Acreditación QR y Control de Aforo (SEyD)
Versión: v1.0.0-GA
Plataforma: .NET 9 ASP.NET Core / SQL Server / W3.CSS / HTML5 QR Scanner
Estado CI/CD: 

** Resumen Ejecutivo **
* Descripción del Proyecto
Sistema web integral desarrollado para la Secretaría de Educación y Deporte (SEyD) y los Servicios Educativos del Estado de Chihuahua (SEECH).
El sistema gestiona la logística de congresos y eventos educativos masivos, administrando desde el alta de conferencias hasta la acreditación en puerta mediante códigos QR y el análisis de aforo en tiempo real.

* Problema Identificado
Los eventos institucionales previos presentaban cuellos de botella en los accesos debido a la validación manual en listas impresas, riesgo de suplantación de identidad, falta de control sobre las cuotas de aforo asignadas por Identificador de Centro de Trabajo (CCT) y la ausencia de métricas en tiempo real sobre la asistencia real frente a los inscritos.

* Solución Desarrollada
Una plataforma web desacoplada y responsiva que:
1. Parametriza eventos y restringe el número de registros permitidos por identificador de Clave de Centro de Trabajo.
2. Emite pases de entrada individuales con un Token QR cifrado de un solo uso.
3. Permite la acreditación en sedes mediante escaneo móvil/web en puerta con prevención estricta de accesos duplicados (FechaAsistencia).
4. Genera tableros de control analíticos y reportes de asistencia exportables a Excel (ClosedXML).

* Arquitectura del Sistema
El sistema adopta un patrón MVC Multicapa Desacoplado:
- Capa de Presentación: Vistas Razor responsivas (W3.CSS, Bootstrap 5, FontAwesome, SweetAlert2) con motor cliente JavaScript para lectura de cámara (html5-qrcode).
- Capa Backend / Lógica: API ASP.NET Core en .NET 9 con arquitectura de controladores y servicios de validación de seguridad.
- Capa de Datos: SQL Server gestionado mediante Entity Framework Core y LINQ optimizado para consultas de alta concurrencia (~700,000 alumnos / 32,000 docentes).
- Pipeline CI/CD: GitHub Actions configurado para integración continua, compilación automatizada y ejecución de pruebas unitarias.

Tabla de Contenidos (ToC)
1. Requerimientos del Sistema
2. Instalación y Despliegue
3. Configuración
4. Manual de Uso
5. Guía de Contribución
6. Roadmap y Alcance Futuro

* Requerimientos del Sistema
Entorno de Desarrollo y Servidores
- Sistema Operativo: Windows 10/11, Windows Server 2019+ o Linux (Ubuntu 22.04 LTS).
- Runtime / SDK: .NET 9.0 SDK
- Servidor Web: Kestrel (Integrado) / IIS 10.0 / Nginx como Reverse Proxy.
- Base de Datos: Microsoft SQL Server 2019 o superior (SQL Server Express compatible).
- Navegadores Soportados: Google Chrome, Microsoft Edge, Safari, Mozilla Firefox (con soporte para API de Cámara Web/Móvil).

* Paquetes y Librerías Principales (NuGet & Frontend)
- Microsoft.EntityFrameworkCore.SqlServer (v9.0)
- ClosedXML (v0.102+) — Generación y exportación de reportes Excel.
- OtpNet — Algoritmos de seguridad y tokens temporales.
- html5-qrcode (v2.3.8) — Motor JavaScript de lectura de código QR cliente.
- W3.CSS & FontAwesome 6 — Estilizado y maquetación de interfaz.

* Instalación y Despliegue
A. Instalación del Ambiente de Desarrollo Local
1. Clonar el repositorio:
   git clone https://github.com/al05088714/sistema-eventos-qr-seech
   cd sistema-eventos-qr-seech
2. Restaurar paquetes NuGet:
   dotnet restore
3. Configurar la cadena de conexión de Base de Datos en
   appsettings.Development.json.
4. Aplicar migraciones / Crear base de datos:
   dotnet ef database update
5. Ejecutar en entorno local:
   dotnet run
   Navegar a https://localhost:7123 o http://localhost:5123.

B. Ejecución Manual de Pruebas Unitarias
Para validar las reglas de negocio y los servicios de cifrado de Token QR:
   dotnet test --logger "console;verbosity=detailed"

C. Implementación en Producción (Local / IIS / Nginx)
1. Publicar el proyecto compilado:
   dotnet publish -c Release -o ./publish
2. Despliegue en IIS (Windows Server):
   * Crear un nuevo Application Pool en IIS configurado con .NET CLR Version: No Managed Code.
   * Crear un Sitio Web apuntando a la carpeta ./publish.
   * Instalar el .NET Core Hosting Bundle en el servidor.
3. Variables de Entorno: Configurar ASPNETCORE_ENVIRONMENT=Production en el servidor host.

* Configuración
Archivo appsettings.json
El comportamiento central del sistema se ajusta mediante las claves de conexión y configuración:
  - ConnectionStrings:DefaultConnection: Cadena de conexión a SQL Server (Server=localhost;Database=DB_Eventos_SEECH;Trusted_Connection=True;).
  - SistemaConfig:NombreInstitucion: "Servicios Educativos del Estado de Chihuahua".
  - SistemaConfig:MaxReintentosCheckIn: 3.

* Manual de Uso
Referencia para Usuario Final (Docentes / Participantes)
1. Acceso y Registro: Ingresar a la URL del evento o escanear el QR público del cartel promocional.
2. Inscripción: Introducir el RFC y CCT. El sistema validará automáticamente la disponibilidad de cupo asignado a su escuela.
3. Generación de Pase: Al confirmar la inscripción, la pantalla mostrará su Pase Digital con Código QR Cifrado.
4. Presentación en Sede: Guardar la imagen o imprimir el pase para presentarlo al personal de recepción el día del evento.

* Referencia para Usuario Administrador / Operador de Puerta
1. Gestión de Eventos (/Actividades):
   * Crear conferencias definiendo fecha, cupo máximo global y restricción por CCT.
   * Consultar la cuota de aforo consumida.
2. Scanner Check-In en Puerta (/CheckIn):
   * Seleccionar la cámara del dispositivo móvil o laptop.
   * Apuntar al QR del asistente. El sistema emitirá una alerta visual/auditiva:
      - Acceso Concedido: Registro exitoso (primera entrada).
      - Pase Ya Utilizado: Muestra la fecha y hora exacta del primer ingreso para prevenir pases clonados.
      - Código Inválido: El registro no existe en la base de datos.
3. Reporte y Exportación de Asistencia (/Inscripcion/Asistencia/{id}):
   * Visualizar las tarjetas KPI (Total Inscritos, Asistieron, Pendientes, % Cobertura).
   * Utilizar el buscador rápido por RFC o CCT.
   * Hacer clic en Exportar a Excel para descargar el concentrado oficial .xlsx.

* Guía de Contribución
Agradecemos las contribuciones para mantener el sistema robusto. Para colaborar, siga el flujo estricto basado en Gitflow Adaptado:
1. Clonar el repositorio y situarse en develop:
   git clone https://github.com/al05088714/sistema-eventos-qr-seech
   git checkout develop
2. Crear una rama de característica (Feature Branch):
   git checkout -b feature/issue-XX-nombre-descriptivo
3. Realizar cambios y commits estructurados:
   git commit -m "feat(modulo): descripción clara del cambio (Closes #XX)"
4. Enviar rama al repositorio remoto:
   git push origin feature/issue-XX-nombre-descriptivo
5. Crear Pull Request (PR):
   * Abrir el PR en GitHub orientando la base hacia la rama develop.
   * Llenar la plantilla del PR detallando las pruebas realizadas.
   * Esperar a la revisión del pipeline CI/CD y aprobación de los revisores antes del Merge.

* Roadmap (Planes Futuros / v2.0)
Los siguientes requerimientos fueron identificados durante la arquitectura inicial y han sido programados para la versión v2.0.0 (Out of Scope del MVP v1.0):
   - Issue #5 (OUT-01) — Pasarela de Pagos en Línea:
      * Integración con proveedor de cobros (Stripe/MercadoPago) para eventos con cuotas de recuperación. (Etiqueta: deferred).
   - Issue #8 (OUT-02) — Módulo Nativo de Envío Masivo SMTP:
      * Envío automático de pases en PDF y confirmaciones al correo institucional del docente tras su registro. (Etiqueta: deferred).

Módulo de Mapeo Geográfico:

Integración con mapas interactivos (Leaflet/Terra Draw) para trazar polígonos de ubicación de sedes educativas.
