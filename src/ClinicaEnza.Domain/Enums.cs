using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain
{
    public enum EstadoTurno
    {
        Pendiente,
        Realizado,
        Cancelado
    }

    public enum RolUsuario
    {
        Administrador,
        Recepcionista,
        Medico
    }

    public enum Especialidad
    {
        Clinica,
        Pediatria,
        Cardiologia,
        Dermatologia
    }
}
