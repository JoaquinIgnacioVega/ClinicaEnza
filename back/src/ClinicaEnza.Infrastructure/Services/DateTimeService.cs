using System;
using ClinicaEnza.Application.Common.Interfaces.Services;

namespace ClinicaEnza.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}

