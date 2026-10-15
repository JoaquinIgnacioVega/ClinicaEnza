namespace ClinicaEnza.Application.Common.Interfaces.Services;

public interface IDateTimeService
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}

