using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain;

public class PrescripcionMedica
{
    public int IdPrescripcion { get; set; }
    public string Medicamento { get; set; } = string.Empty;

    // Claves foráneas
    public int IdHistoriaClinica { get; set; }
    public HistoriaClinica HistoriaClinica { get; private set; } = null!;

    public int IdMedico { get; set; }
    public Medico Medico { get; private set; } = null!;

    protected PrescripcionMedica() { }

    public PrescripcionMedica(string medicamento, HistoriaClinica historiaClinica, Medico medico)
    {
        if (string.IsNullOrWhiteSpace(medicamento))
            throw new ArgumentException("El nombre del medicamento es obligatorio.", nameof(medicamento));

        Medicamento = medicamento;
        HistoriaClinica = historiaClinica ?? throw new ArgumentNullException(nameof(historiaClinica));
        Medico = medico ?? throw new ArgumentNullException(nameof(medico));

        IdHistoriaClinica = historiaClinica.IdHistoriaClinica;
        IdMedico = medico.IdPersona;
    }

    public string ObtenerDetalle()
    {
        var medicoNombre = Medico != null ? Medico.ObtenerNombreCompleto() : "Médico no asignado";
        return $"Prescripción: {Medicamento} | Emitida por: {medicoNombre}";
    }
}
