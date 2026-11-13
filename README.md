# Clínica Enza

Sistema web para la gestión de una clínica, desarrollado como trabajo integrador de Metodologías de Programación II.

La aplicación permite administrar pacientes, médicos, turnos, horarios de atención, consultorios, historias clínicas y prescripciones médicas.

## Roles del sistema

- **Administrador:** gestiona usuarios y configuración general.
- **Recepcionista:** administra pacientes, médicos, turnos, horarios y consultorios.
- **Médico:** consulta su agenda, historias clínicas y registra prescripciones.
- **Paciente:** consulta y cancela sus propios turnos.

## Tecnologías y requisitos

- C# y .NET 8 Web API.
- React, TypeScript, Vite y Tailwind CSS.
- Entity Framework Core 8 con SQLite.
- JWT Bearer y contraseñas hasheadas.
- Git, Node.js 20 o superior, npm y .NET SDK 8.
- Docker y Docker Compose, opcionales.

## Ejecutar el backend

Desde la raíz del repositorio:

```bash
cd back/src/ClinicaEnza.Api
dotnet restore
dotnet run
```

La API queda disponible en `http://localhost:5207`.

- Swagger: `http://localhost:5207/swagger`
- Health check: `http://localhost:5207/api/health`

## Ejecutar el frontend

En otra terminal:

```bash
cd front
npm install
npm run dev
```

El frontend queda disponible normalmente en `http://localhost:5173`. Por defecto utiliza la API en `http://localhost:5207`. Para cambiarla:

```bash
VITE_API_URL=http://localhost:5207 npm run dev
```

En Windows PowerShell:

```powershell
$env:VITE_API_URL = "http://localhost:5207"
npm run dev
```

## Base de datos y migraciones

La aplicación utiliza SQLite. En una ejecución local, el archivo `clinica_enza.db` se crea en el directorio de trabajo de la API.

Para aplicar las migraciones desde la raíz:

```bash
dotnet ef database update \
  --project back/src/ClinicaEnza.Infrastructure \
  --startup-project back/src/ClinicaEnza.Api
```

Las migraciones se encuentran en `back/src/ClinicaEnza.Infrastructure/Persistence/Migrations`.

La cadena de conexión se configura en `back/src/ClinicaEnza.Api/appsettings.json` o mediante `ConnectionStrings__DefaultConnection`.

## Ejecutar las pruebas

```bash
dotnet test test/ClinicaEnza.Tests/ClinicaEnza.Tests.csproj
```

Para compilar el frontend:

```bash
cd front
npm run build
```

## Cuentas de demostración

| Rol | Usuario | Contraseña |
|---|---|---|
| Administrador | `carlos.admin@clinicaenza.com` | `Password123!` |
| Recepcionista | `ana.recepcion@clinicaenza.com` | `Password123!` |
| Médico | `dr.perez@clinicaenza.com` | `Password123!` |

El acceso de Paciente requiere un usuario con rol `Paciente` asociado a un registro mediante `IdPaciente`. La migración agrega la estructura necesaria, pero la cuenta debe configurarse en el entorno de demostración. No se deben utilizar credenciales reales en un entorno público.

## Ejecutar con Docker Compose

```bash
docker compose up --build
```

- Frontend: `http://localhost:3000`
- Backend: `http://localhost:5000`
- Base SQLite: volumen `clinica_data`

Para detener los contenedores:

```bash
docker compose down
```

## Estructura del repositorio

```text
back/                  Backend .NET y capas de la aplicación
front/                 Frontend React y TypeScript
test/                  Pruebas automatizadas
documentacion/         Diagramas y documentación técnica
docker-compose.yml     Entorno contenerizado
```

## Metodología y colaboración

El equipo trabajó con Scrum y organizó el desarrollo en dos sprints. Trello se utilizó para administrar el backlog, asignar responsables, registrar estados y documentar los cierres de sprint. GitHub se utilizó para versionar el código mediante ramas, commits descriptivos y pull requests.

## Convención de commits

Los commits siguen Conventional Commits:

```text
feat(turnos): agregar consulta de turnos por médico
fix(consultorios): permitir deshabilitación lógica
docs(readme): completar instrucciones de instalación
test(turnos): cubrir validaciones de solapamiento
```

## Licencia

Proyecto académico desarrollado para el trabajo integrador de Metodologías de Programación II.
