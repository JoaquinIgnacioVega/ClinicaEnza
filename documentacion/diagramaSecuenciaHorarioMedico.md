# Diagrama de secuencia - Configuración de horario de atención

El siguiente diagrama representa el proceso de configurar horarios de atención de un mdico en Clínica Enza.
Configurar la agenda del médico: un administrador/recepcionista define cuándo y dónde atiende.

```mermaid
sequenceDiagram

    actor A as Administrador
    participant S as Sistema
    participant H as HorarioAtencion

    A->>S: Selecciona medico
    S-->>A: Mostrar configuracion semanal actual

    A->>S: Selecciona dia de semana
    A->>S: Ingresa hora desde y hora hasta
    A->>S: Selecciona consultorio

    S->>S: Consultar horarios del medico para ese dia
    S->>S: Consultar horarios del consultorio para ese dia

    S->>H: Verificar superposicion
    H-->>S: Resultado

    alt No existe superposicion
        S->>H: Crear horario semanal
        H-->>S: Horario creado
        S-->>A: Mostrar configuracion semanal actualizada
    else Existe superposicion
        S-->>A: Informar conflicto de horario
    end
```