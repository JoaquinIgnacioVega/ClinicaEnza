namespace ClinicaEnza.Application.Features.Pacientes.DTOs;

public class AutenticarPacienteResponseDto
{
    public string Token { get; set; } = string.Empty;
    public PacienteDto Paciente { get; set; } = null!;
    public bool RequiereCambioPassword { get; set; }
}
