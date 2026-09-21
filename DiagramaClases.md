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
        -string legajo
        -DateTime fechaInicio
        -string rol
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
        -string especialidad
        -DateTime fechaInicio
        -string matricula
        +ObtenerFichaProfesional() string
        +ObtenerIdentificacion() string
    }

    class Turno {
        -int idTurno
        -DateTime horario
        -string estado
        -int idPaciente
        -int idMedico
        -int idConsultorio
        +Reagendar(DateTime nuevoHorario) void
        +CambiarEstado(string nuevoEstado) void
        +EsSolapadoCon(DateTime horarioComparar) bool
    }

    class HistoriaClinica {
        -int idHistoriaClinica
        -int idPaciente
        +AgregarPrescripcion(PrescripcionMedica prescripcion) void
        +ObtenerPrescripcionesPorMedico(int idMedico) List~PrescripcionMedica~
        +ObtenerMedicamentosRecetados() List~string~
        +BuscarPrimeraPrescripcionDe(string medicamento) PrescripcionMedica
    }

    class PrescripcionMedica {
        -int idPrescripcion
        -string medicamento
        -int idHistoriaClinica
        -int idMedico
        +ObtenerDetalle() string
    }

    class Consultorio {
        -int idConsultorio
        -string numero
        -int piso
        -string sector
        +ObtenerUbicacion() string
        +EstaDisponibleEn(DateTime horario) bool
    }

    Persona <|-- Usuario
    Persona <|-- Paciente
    Persona <|-- Medico

    Paciente "1" *-- "1" HistoriaClinica
    HistoriaClinica "1" *-- "0..*" PrescripcionMedica
    Medico "1" --> "0..*" PrescripcionMedica
    
    Paciente "1" --> "0..*" Turno
    Medico "1" --> "0..*" Turno
    Turno "0..*" --> "1" Consultorio