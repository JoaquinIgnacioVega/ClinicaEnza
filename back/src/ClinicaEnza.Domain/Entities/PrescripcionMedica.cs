using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain;

public class PrescripcionMedica
{
    public int IdPrescripcion { get; set; }
    public string Medicamento { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; } = DateTime.Now;
    public int IdHistoriaClinica { get; private set; }
    public HistoriaClinica HistoriaClinica { get; private set; } = null!;

    public int IdMedico { get; private set; }
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
        Fecha = DateTime.Now;
    }

    public string ObtenerDetalle()
    {
        var medicoNombre = Medico != null ? Medico.ObtenerNombreCompleto() : "Médico no asignado";
        return $"Prescripción: {Medicamento} | Fecha: {Fecha:dd/MM/yyyy} | Emitida por: {medicoNombre}";
    }
}