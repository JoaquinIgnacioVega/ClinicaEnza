# Diagrama Entidad-Relación - Clínica Enza

El siguiente diagrama representa el modelo de persistencia del sistema de Clínica Enza utilizando SQLite.

```mermaid
erDiagram

    PERSONA {
        int idPersona PK
        string nombre
        string apellido
        string dni UK
        datetime fechaNacimiento
        string mail
    }

    USUARIO {
        int idPersona PK, FK
        string legajo UK
        datetime fechaInicio
        string rol
    }

    PACIENTE {
        int idPersona PK, FK
        string obraSocial
        decimal peso
        decimal altura
    }

    MEDICO {
        int idPersona PK, FK
        string matricula UK
        string especialidad
        datetime fechaInicio
    }

    CONSULTORIO {
        int idConsultorio PK
        string numero
        int piso
        string sector
    }

    HORARIO_ATENCION {
        int idHorarioAtencion PK
        int idMedico FK
        int idConsultorio FK
        string diaSemana
        time horaDesde
        time horaHasta
    }

    TURNO {
        int idTurno PK
        int idPaciente FK
        int idMedico FK
        int idConsultorio FK
        datetime horaInicio
        datetime horaFin
        string estado
    }

    HISTORIA_CLINICA {
        int idHistoriaClinica PK
        int idPaciente FK
    }

    PRESCRIPCION_MEDICA {
        int idPrescripcion PK
        int idHistoriaClinica FK
        int idMedico FK
        string medicamento
        datetime fecha
    }

    PERSONA ||--o| USUARIO : es
    PERSONA ||--o| PACIENTE : es
    PERSONA ||--o| MEDICO : es

    PACIENTE ||--|| HISTORIA_CLINICA : posee

    HISTORIA_CLINICA ||--o{ PRESCRIPCION_MEDICA : contiene
    MEDICO ||--o{ PRESCRIPCION_MEDICA : realiza

    PACIENTE ||--o{ TURNO : tiene
    MEDICO ||--o{ TURNO : atiende
    CONSULTORIO ||--o{ TURNO : asignado

    MEDICO ||--o{ HORARIO_ATENCION : posee
    CONSULTORIO ||--o{ HORARIO_ATENCION : asignado
```

## Enumeraciones persistidas

Algunas propiedades del modelo de clases se representan mediante enumeraciones en C#. En SQLite se almacenan como valores simples, pero solamente pueden contener los valores definidos por el dominio.

### Estado del turno

El campo `TURNO.estado` representa el enum `EstadoTurno`.

Valores permitidos:

* `Pendiente`
* `Realizado`
* `Cancelado`

### Rol del usuario

El campo `USUARIO.rol` representa el enum `RolUsuario`.

Valores permitidos:

* `Administrador`
* `Recepcionista`
* `Medico`

### Especialidad del médico

El campo `MEDICO.especialidad` representa el enum `Especialidad`.

Valores permitidos:

* `Clinica`
* `Pediatria`
* `Cardiologia`
* `Dermatologia`

## Restricciones del modelo

* `PERSONA.dni` debe ser único.
* `USUARIO.legajo` debe ser único.
* `MEDICO.matricula` debe ser única.
* Todo turno debe estar asociado a un paciente, un médico y un consultorio existentes.
* Todo horario de atención debe estar asociado a un médico y un consultorio existentes.
* `HORARIO_ATENCION.horaDesde` debe ser anterior a `horaHasta`.
* Los horarios de atención de un mismo médico no pueden superponerse para el mismo día de la semana.
* Un consultorio no puede estar asignado a dos médicos en franjas horarias superpuestas del mismo día.
* Todo turno tiene una duración de 20 minutos.
* Un turno debe encontrarse dentro de un `HorarioAtencion` correspondiente al médico para ese día y horario.
* El consultorio almacenado en `TURNO.idConsultorio` debe coincidir con el consultorio definido por el `HorarioAtencion` utilizado al momento de asignar el turno.
* Un médico no puede tener dos turnos superpuestos.
* `TURNO.estado` solamente puede contener valores definidos por `EstadoTurno`.
* La historia clínica pertenece a un único paciente.
* Una prescripción médica pertenece a una historia clínica y debe estar asociada al médico que la realizó.

Las restricciones de superposición de horarios, disponibilidad y correspondencia entre el turno y el horario de atención se implementan como reglas de negocio en la aplicación y se verifican mediante pruebas.
