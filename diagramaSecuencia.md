# Diagrama de secuencia - Asignación de turno

El siguiente diagrama representa el proceso de asignación de un turno en Clínica Enza.
El sistema verifica que el médico y el consultorio no tengan un turno superpuesto antes de registrar el nuevo turno. Los turnos tienen una duración fija de 20 minutos.

```mermaid
sequenceDiagram
    actor R as Recepcionista
    participant S as Sistema
    participant M as Medico
    participant C as Consultorio
    participant T as Turno

    R->>S: Selecciona paciente, medico, consultorio y horaInicio
    S->>S: Calcular horaFin = horaInicio + 20 minutos

    S->>M: Consultar turnos del medico
    M-->>S: Retorna turnos existentes

    S->>C: Consultar turnos del consultorio
    C-->>S: Retorna turnos existentes

    S->>T: Verificar EsSolapadoCon(turnos existentes)
    T-->>S: Resultado de la validacion

    alt No existe superposicion
        S->>T: Crear turno
        T->>T: CalcularHoraFin()
        T-->>S: Turno creado
        S-->>R: Mostrar turno asignado correctamente
    else Existe superposicion
        S-->>R: Informar horario no disponible
    end
```