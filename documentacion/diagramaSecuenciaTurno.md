# Diagrama de secuencia - Asignación de turno

El siguiente diagrama representa el proceso de asignación de un turno en Clínica Enza.
El sistema verifica que el médico y el consultorio no tengan un turno superpuesto antes de registrar el nuevo turno. Los turnos tienen una duración fija de 20 minutos.

```mermaid
sequenceDiagram

    actor R as Recepcionista
    participant S as Sistema
    participant H as HorarioAtencion
    participant T as Turno

    R->>S: Selecciona paciente, medico y fecha

    S->>H: Consultar horario del medico para la fecha
    H-->>S: Retorna franja horaria y consultorio

    alt Medico atiende ese dia

        S->>S: Consultar turnos existentes del medico para esa fecha
        S->>S: Calcular horarios disponibles de 20 minutos
        S-->>R: Mostrar horarios disponibles

        R->>S: Selecciona horario disponible

        S->>T: Crear turno con paciente, medico, horaInicio y consultorio
        T->>T: CalcularHoraFin()
        T->>T: Asignar estado Pendiente
        T-->>S: Turno creado

        S->>S: Guardar turno
        S-->>R: Confirmar turno asignado

    else Medico no atiende ese dia

        S-->>R: Informar que el medico no atiende ese dia

    end
```