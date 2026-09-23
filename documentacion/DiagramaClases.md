# Diagrama de clases - Clínica Enza

Este diagrama representa las principales clases del sistema de gestión de turnos e historias clínicas de Clínica Enza.

```mermaid
classDiagram

    class Persona {
        <<abstract>>
        -int idPersona
        -string nombre
        -string apellido
        -string dni
        -DateTime fechaNacimiento
        -string mail
        +ObtenerNombreCompleto() string
        +CalcularEdad() int
        +ObtenerIdentificacion()* string
    }

    class Usuario {
        -int idUsuario
        -string legajo
        -DateTime fechaInicio
        -RolUsuario rol
        +EsAdministrador() bool
        +ObtenerIdentificacion() string
    }

    class Paciente {
        -string obraSocial
        -decimal peso
        -decimal altura
        +ActualizarMedidas(decimal peso, decimal altura) void
        +ObtenerIdentificacion() string
    }

    class Medico {
        -Especialidad especialidad
        -DateTime fechaInicio
        -string matricula
        +ObtenerFichaProfesional() string
        +ObtenerIdentificacion() string
    }

    class Turno {
        -int idTurno
        -DateTime horaInicio
        -DateTime horaFin
        -EstadoTurno estado
        -Paciente paciente
        -Medico medico
        -Consultorio consultorio
        +Reagendar(DateTime nuevaHoraInicio) void
        +CambiarEstado(EstadoTurno nuevoEstado) void
        +EsSolapadoCon(Turno otroTurno) bool
        -CalcularHoraFin() void
    }

    class HorarioAtencion {
        -int idHorarioAtencion
        -DayOfWeek diaSemana
        -TimeSpan horaDesde
        -TimeSpan horaHasta
        -Medico medico
        -Consultorio consultorio
        +IncluyeHorario(DateTime horario) bool
        +EsSolapadoCon(HorarioAtencion otroHorario) bool
    }

    class HistoriaClinica {
        -int idHistoriaClinica
        +AgregarPrescripcion(PrescripcionMedica prescripcion) void
        +ObtenerPrescripcionesPorMedico(Medico medico) List~PrescripcionMedica~
        +ObtenerMedicamentosRecetados() List~string~
        +BuscarPrimeraPrescripcionDe(string medicamento) PrescripcionMedica
    }

    class PrescripcionMedica {
        -int idPrescripcion
        -string medicamento
        -DateTime fecha
        -Medico medico
        +ObtenerDetalle() string
    }

    class Consultorio {
        -int idConsultorio
        -string numero
        -int piso
        -string sector
        +ObtenerUbicacion() string
    }

    class EstadoTurno {
        <<enumeration>>
        Pendiente
        Realizado
        Cancelado
    }

    class RolUsuario {
        <<enumeration>>
        Administrador
        Recepcionista
        Medico
    }

    class Especialidad {
        <<enumeration>>
        Clinica
        Pediatria
        Cardiologia
        Dermatologia
    }

    Persona <|-- Usuario
    Persona <|-- Paciente
    Persona <|-- Medico

    Paciente "1" *-- "1" HistoriaClinica
    HistoriaClinica "1" *-- "0..*" PrescripcionMedica
    Medico "1" --> "0..*" PrescripcionMedica

    Paciente "1" --> "0..*" Turno
    Medico "1" --> "0..*" Turno
    Consultorio "1" --> "0..*" Turno

    Medico "1" --> "0..*" HorarioAtencion
    Consultorio "1" --> "0..*" HorarioAtencion

    Turno --> EstadoTurno
    Usuario --> RolUsuario
    Medico --> Especialidad
```

## Enumeraciones

### EstadoTurno

Representa el estado actual de un turno:

* `Pendiente`: el turno fue agendado y todavía no fue atendido.
* `Realizado`: el paciente fue atendido.
* `Cancelado`: el turno fue cancelado.

### RolUsuario

Define el rol de un usuario dentro del sistema:

* `Administrador`
* `Recepcionista`
* `Medico`

### Especialidad

Define la especialidad de un médico:

* `Clinica`
* `Pediatria`
* `Cardiologia`
* `Dermatologia`

## Consideraciones del modelo

Los turnos tienen una duración fija de 20 minutos. Al establecer `horaInicio`, el sistema calcula automáticamente `horaFin`.

`HorarioAtencion` representa la configuración semanal de atención de un médico. Cada horario establece el día de la semana, la franja horaria y el consultorio asignado.

Al crear un turno, el consultorio no es seleccionado manualmente por recepción. El sistema obtiene el consultorio correspondiente a partir del `HorarioAtencion` del médico y lo almacena también en el `Turno`.

Esto permite conservar el consultorio asignado al turno incluso si posteriormente se modifica la configuración semanal del médico.
