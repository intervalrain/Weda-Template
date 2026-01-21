namespace Weda.Template.Contracts.Employees.Events;

/// <summary>
/// NATS Subject 格式: {protoVer}.{groupId}.{employeeId}.{requestor}.{responder}.{command}.{subCommand}
/// </summary>
public static class EmployeeNatsSubjects
{
    public const string ProtoVer = "eco1j";
    public const string GroupId = "weda";
    public const string ServiceName = "emp";

    // Subject patterns (使用 * 作為 wildcard)
    // eco1j.weda.{id}.*.emp.get
    public const string GetPattern = $"{ProtoVer}.{GroupId}.*.*.{ServiceName}.get";

    // eco1j.weda.{id}.*.emp.get.status
    public const string GetStatusPattern = $"{ProtoVer}.{GroupId}.*.*.{ServiceName}.get.status";

    // eco1j.weda.0.*.emp.list
    public const string ListPattern = $"{ProtoVer}.{GroupId}.0.*.{ServiceName}.list";

    // eco1j.weda.0.*.emp.list.idle
    public const string ListIdlePattern = $"{ProtoVer}.{GroupId}.0.*.{ServiceName}.list.idle";

    // eco1j.weda.{id}.emp.*.event.created
    public const string CreatedEvent = $"{ProtoVer}.{GroupId}.*.{ServiceName}.*.event.created";

    // eco1j.weda.{id}.emp.*.event.updated
    public const string UpdatedEvent = $"{ProtoVer}.{GroupId}.*.{ServiceName}.*.event.updated";

    public static string BuildGetSubject(int employeeId, string requestor) =>
        $"{ProtoVer}.{GroupId}.{employeeId}.{requestor}.{ServiceName}.get";

    public static string BuildCreatedEventSubject(int employeeId, string subscriber) =>
        $"{ProtoVer}.{GroupId}.{employeeId}.{ServiceName}.{subscriber}.event.created";
}
