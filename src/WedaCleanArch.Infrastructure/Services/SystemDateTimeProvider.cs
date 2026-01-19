using WedaCleanArch.Application.Common.Interfaces;

namespace WedaCleanArch.Infrastructure.Services;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
