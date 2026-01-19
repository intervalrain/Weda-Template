using Weda.Template.Application.Common.Interfaces;

namespace Weda.Template.Infrastructure.Services;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
