using Weda.Core.Application.Interfaces;

namespace Weda.Template.Infrastructure.Services;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
