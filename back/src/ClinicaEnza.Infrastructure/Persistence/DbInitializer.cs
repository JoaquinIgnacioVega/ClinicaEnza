using System.Globalization;
using System.Reflection;
using ClinicaEnza.Application.Common.Interfaces.Services;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using ClinicaEnza.Infrastructure.Persistence.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicaEnza.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("DbInitializer");

        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? "Data Source=clinica_enza.db";

        var sqliteBuilder = new SqliteConnectionStringBuilder(connectionString);
        if (!string.IsNullOrWhiteSpace(sqliteBuilder.DataSource) &&
            !string.Equals(sqliteBuilder.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase))
        {
            var fullDbPath = Path.GetFullPath(sqliteBuilder.DataSource);
            var directory = Path.GetDirectoryName(fullDbPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();

        logger?.LogInformation("Aplicando migraciones de Entity Framework Core sobre SQLite ({DataSource})...", sqliteBuilder.DataSource);
        await context.Database.MigrateAsync();
        context.EnsureSqliteSchemaCompatibility();

        if (await context.Personas.AnyAsync())
        {
            logger?.LogInformation("La base de datos ya contiene datos iniciales. Se omite el seeding.");
            return;
        }

        logger?.LogInformation("Base de datos vacía. Insertando datos iniciales...");

        await using var transaction = await context.Database.BeginTransactionAsync();

        // 1. Consultorios y Personas (respetando IDs y claves foráneas originales)
        var consultorio1 = CrearConsultorio(1, "101", 1, "Ala Norte", true);
        var consultorio2 = CrearConsultorio(2, "102", 1, "Ala Norte", true);
        var consultorio3 = CrearConsultorio(3, "201", 2, "Ala Sur", true);

        await context.Consultorios.AddRangeAsync(consultorio1, consultorio2, consultorio3);

        var medico2 = CrearMedico(
            idPersona: 2,
            nombre: "Mariana Andrea",
            apellido: "Pérez Gómez",
            dni: "30111222",
            fechaNacimiento: new DateTime(1985, 4, 12),
            mail: "mariana.perez@clinicaenza.com",
            especialidad: Especialidad.Pediatria,
            matricula: "MP-54321",
            fechaInicio: new DateTime(2020, 3, 1));

        var medico3 = CrearMedico(
            idPersona: 3,
            nombre: "Juan",
            apellido: "Pérez",
            dni: "40111222",
            fechaNacimiento: new DateTime(1992, 6, 20),
            mail: "juan.perez@clinicaenza.com",
            especialidad: Especialidad.Clinica,
            matricula: "MP-99999",
            fechaInicio: new DateTime(2026, 1, 1));

        var paciente4 = CrearPaciente(
            idPersona: 4,
            nombre: "joaco",
            apellido: "vega",
            dni: "45573520",
            fechaNacimiento: DateTime.Parse("2004-10-31T18:52:26.058", CultureInfo.InvariantCulture),
            mail: "vegajoa@gmail.com",
            obraSocial: "Galeno",
            peso: 100.0m,
            altura: 170.0m,
            passwordHash: passwordHasher.HashPassword("1234"),
            debeCambiarPassword: false);

        var usuario1 = CrearUsuario(
            idPersona: 1,
            nombre: "Carlos",
            apellido: "González",
            dni: "35123456",
            fechaNacimiento: new DateTime(1990, 5, 15),
            mail: "carlos.admin@clinicaenza.com",
            legajo: "LEG-001",
            passwordHash: "CKpe/Duuk5Tgjvw54y61WQ==.hCKEnRll+54o+ekykbEo9S3ZZrkf56auyHA0VkAnDCc=",
            rol: RolUsuario.Administrador,
            idMedico: null,
            fechaInicio: DateTime.Parse("2026-10-28T18:16:37.897655", CultureInfo.InvariantCulture),
            activo: true);

        var usuario5 = CrearUsuario(
            idPersona: 5,
            nombre: "Juan",
            apellido: "Perez",
            dni: "28111222",
            fechaNacimiento: new DateTime(1980, 5, 10),
            mail: "dr.perez@clinicaenza.com",
            legajo: "MED-001",
            passwordHash: "xgsRFqonxkrHCOppU2/YwA==./6Wr6o8Gp8tMqTjK9Bxq0usaJyOBYhic4J240yjnLFg=",
            rol: RolUsuario.Medico,
            idMedico: 3,
            fechaInicio: DateTime.Parse("2026-11-06T19:26:59.7145967", CultureInfo.InvariantCulture),
            activo: true);

        var usuario6 = CrearUsuario(
            idPersona: 6,
            nombre: "Ana",
            apellido: "Gomez",
            dni: "32444555",
            fechaNacimiento: new DateTime(1988, 8, 20),
            mail: "ana.recepcion@clinicaenza.com",
            legajo: "REC-001",
            passwordHash: "L0CkR4OU6hCHGyQTVuuQvg==.Q7ewhTfOw8Wyd+4A/rIWDLyG2/1Hlu4EErgb2JiIdZw=",
            rol: RolUsuario.Recepcionista,
            idMedico: null,
            fechaInicio: DateTime.Parse("2026-11-06T19:27:37.8003295", CultureInfo.InvariantCulture),
            activo: true);

        await context.Medicos.AddRangeAsync(medico2, medico3);
        await context.Pacientes.AddAsync(paciente4);
        await context.SaveChangesAsync();

        await context.Usuarios.AddRangeAsync(usuario1, usuario5, usuario6);
        await context.SaveChangesAsync();

        // 2. HorariosAtencion (6 registros)
        var horarios = new[]
        {
            CrearHorario(1, DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0), 2, 2),
            CrearHorario(2, DayOfWeek.Monday, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0), 2, 1),
            CrearHorario(3, DayOfWeek.Monday, new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0), 3, 1),
            CrearHorario(4, DayOfWeek.Tuesday, new TimeSpan(9, 0, 0), new TimeSpan(15, 0, 0), 3, 2),
            CrearHorario(5, DayOfWeek.Wednesday, new TimeSpan(8, 0, 0), new TimeSpan(14, 0, 0), 3, 1),
            CrearHorario(6, DayOfWeek.Thursday, new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0), 3, 1)
        };
        await context.HorariosAtencion.AddRangeAsync(horarios);
        await context.SaveChangesAsync();

        // 3. Turnos (8 registros)
        var turnos = new[]
        {
            CrearTurno(1, "2026-11-16T09:41:38.371", "2026-11-16T10:01:38.371", EstadoTurno.Realizado, 4, 3, 1),
            CrearTurno(2, "2026-11-16T11:41:38.371", "2026-11-16T12:01:38.371", EstadoTurno.Cancelado, 4, 3, 1),
            CrearTurno(3, "2026-11-16T11:41:38.371", "2026-11-16T12:01:38.371", EstadoTurno.Realizado, 4, 3, 1),
            CrearTurno(4, "2026-11-10T11:41:38.371", "2026-11-10T12:01:38.371", EstadoTurno.Cancelado, 4, 3, 2),
            CrearTurno(5, "2026-11-18T09:30:00", "2026-11-18T09:50:00", EstadoTurno.Cancelado, 4, 3, 1),
            CrearTurno(6, "2026-11-20T10:30:00", "2026-11-20T10:50:00", EstadoTurno.Cancelado, 4, 2, 2),
            CrearTurno(7, "2026-11-16T11:00:00", "2026-11-16T11:20:00", EstadoTurno.Cancelado, 4, 3, 1),
            CrearTurno(8, "2026-11-16T10:20:00", "2026-11-16T10:40:00", EstadoTurno.Cancelado, 4, 3, 1)
        };
        await context.Turnos.AddRangeAsync(turnos);
        await context.SaveChangesAsync();

        // 4. HistoriasClinicas (1 registro)
        var historiaClinica1 = CrearHistoriaClinica(1, 4);
        await context.HistoriasClinicas.AddAsync(historiaClinica1);
        await context.SaveChangesAsync();

        // 5. PrescripcionesMedicas (5 registros)
        var prescripciones = new[]
        {
            CrearPrescripcion(1, "Amoxicilina 500mg cada 8hs por 7 días", 1, 2, "2026-10-31T15:54:54.8003854"),
            CrearPrescripcion(2, "blablabla", 1, 3, "2026-11-09T15:49:08.754685"),
            CrearPrescripcion(3, "hola", 1, 3, "2026-11-09T15:51:54.2254447"),
            CrearPrescripcion(4, "Prueba de prescripción", 1, 3, "2026-11-09T16:41:51.6957871"),
            CrearPrescripcion(5, "dfsfsdgfghdf", 1, 3, "2026-11-09T17:59:25.091296")
        };
        await context.PrescripcionesMedicas.AddRangeAsync(prescripciones);
        await context.SaveChangesAsync();

        await transaction.CommitAsync();
        logger?.LogInformation("Seeding inicial completado: 6 personas, 3 consultorios, 6 horarios, 8 turnos, 1 historia clínica y 5 prescripciones.");
    }

    private static Consultorio CrearConsultorio(int id, string numero, int piso, string sector, bool activo)
    {
        var entity = CrearInstancia<Consultorio>();
        SetProp(entity, nameof(Consultorio.IdConsultorio), id);
        SetProp(entity, nameof(Consultorio.Numero), numero);
        SetProp(entity, nameof(Consultorio.Piso), piso);
        SetProp(entity, nameof(Consultorio.Sector), sector);
        SetProp(entity, nameof(Consultorio.Activo), activo);
        return entity;
    }

    private static Medico CrearMedico(
        int idPersona,
        string nombre,
        string apellido,
        string dni,
        DateTime fechaNacimiento,
        string mail,
        Especialidad especialidad,
        string matricula,
        DateTime fechaInicio)
    {
        var entity = new Medico(nombre, apellido, dni, fechaNacimiento, mail, especialidad, matricula, fechaInicio);
        entity.IdPersona = idPersona;
        return entity;
    }

    private static Paciente CrearPaciente(
        int idPersona,
        string nombre,
        string apellido,
        string dni,
        DateTime fechaNacimiento,
        string mail,
        string obraSocial,
        decimal peso,
        decimal altura,
        string passwordHash,
        bool debeCambiarPassword)
    {
        var entity = CrearInstancia<Paciente>();
        SetProp(entity, nameof(Persona.IdPersona), idPersona);
        SetProp(entity, nameof(Persona.Nombre), nombre);
        SetProp(entity, nameof(Persona.Apellido), apellido);
        SetProp(entity, nameof(Persona.Dni), dni);
        SetProp(entity, nameof(Persona.FechaNacimiento), fechaNacimiento);
        SetProp(entity, nameof(Persona.Mail), mail);
        SetProp(entity, nameof(Paciente.ObraSocial), obraSocial);
        SetProp(entity, nameof(Paciente.Peso), peso);
        SetProp(entity, nameof(Paciente.Altura), altura);
        SetProp(entity, nameof(Paciente.PasswordHash), passwordHash);
        SetProp(entity, nameof(Paciente.DebeCambiarPassword), debeCambiarPassword);
        return entity;
    }

    private static Usuario CrearUsuario(
        int idPersona,
        string nombre,
        string apellido,
        string dni,
        DateTime fechaNacimiento,
        string mail,
        string legajo,
        string passwordHash,
        RolUsuario rol,
        int? idMedico,
        DateTime fechaInicio,
        bool activo)
    {
        var entity = CrearInstancia<Usuario>();
        SetProp(entity, nameof(Persona.IdPersona), idPersona);
        SetProp(entity, nameof(Persona.Nombre), nombre);
        SetProp(entity, nameof(Persona.Apellido), apellido);
        SetProp(entity, nameof(Persona.Dni), dni);
        SetProp(entity, nameof(Persona.FechaNacimiento), fechaNacimiento);
        SetProp(entity, nameof(Persona.Mail), mail);
        SetProp(entity, nameof(Usuario.Legajo), legajo);
        SetProp(entity, nameof(Usuario.PasswordHash), passwordHash);
        SetProp(entity, nameof(Usuario.Rol), rol);
        SetProp(entity, nameof(Usuario.IdMedico), idMedico);
        SetProp(entity, nameof(Usuario.FechaInicio), fechaInicio);
        SetProp(entity, nameof(Usuario.Activo), activo);
        return entity;
    }

    private static HorarioAtencion CrearHorario(
        int idHorario,
        DayOfWeek diaSemana,
        TimeSpan horaDesde,
        TimeSpan horaHasta,
        int idMedico,
        int idConsultorio)
    {
        var entity = new HorarioAtencion(diaSemana, horaDesde, horaHasta, idMedico, idConsultorio)
        {
            IdHorarioAtencion = idHorario
        };
        return entity;
    }

    private static Turno CrearTurno(
        int idTurno,
        string horaInicioIso,
        string horaFinIso,
        EstadoTurno estado,
        int idPaciente,
        int idMedico,
        int idConsultorio)
    {
        var entity = CrearInstancia<Turno>();
        SetProp(entity, nameof(Turno.IdTurno), idTurno);
        SetProp(entity, nameof(Turno.HoraInicio), DateTime.Parse(horaInicioIso, CultureInfo.InvariantCulture));
        SetProp(entity, nameof(Turno.HoraFin), DateTime.Parse(horaFinIso, CultureInfo.InvariantCulture));
        SetProp(entity, nameof(Turno.Estado), estado);
        SetProp(entity, nameof(Turno.IdPaciente), idPaciente);
        SetProp(entity, nameof(Turno.IdMedico), idMedico);
        SetProp(entity, nameof(Turno.IdConsultorio), idConsultorio);
        return entity;
    }

    private static HistoriaClinica CrearHistoriaClinica(int idHistoriaClinica, int idPaciente)
    {
        var entity = CrearInstancia<HistoriaClinica>();
        SetProp(entity, nameof(HistoriaClinica.IdHistoriaClinica), idHistoriaClinica);
        SetProp(entity, nameof(HistoriaClinica.IdPaciente), idPaciente);
        return entity;
    }

    private static PrescripcionMedica CrearPrescripcion(
        int idPrescripcion,
        string medicamento,
        int idHistoriaClinica,
        int idMedico,
        string fechaIso)
    {
        var entity = CrearInstancia<PrescripcionMedica>();
        SetProp(entity, nameof(PrescripcionMedica.IdPrescripcion), idPrescripcion);
        SetProp(entity, nameof(PrescripcionMedica.Medicamento), medicamento);
        SetProp(entity, nameof(PrescripcionMedica.IdHistoriaClinica), idHistoriaClinica);
        SetProp(entity, nameof(PrescripcionMedica.IdMedico), idMedico);
        SetProp(entity, nameof(PrescripcionMedica.Fecha), DateTime.Parse(fechaIso, CultureInfo.InvariantCulture));
        return entity;
    }

    private static T CrearInstancia<T>() where T : class =>
        (T)Activator.CreateInstance(typeof(T), nonPublic: true)!;

    private static void SetProp<T>(T target, string propertyName, object? value) where T : class
    {
        for (Type? current = typeof(T); current != null; current = current.BaseType)
        {
            var prop = current.GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            if (prop != null && prop.SetMethod != null)
            {
                prop.SetValue(target, value);
                return;
            }
        }

        throw new InvalidOperationException($"No se encontró el setter de la propiedad '{propertyName}' en {typeof(T).Name}.");
    }
}
