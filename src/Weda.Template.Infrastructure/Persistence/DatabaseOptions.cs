namespace Weda.Template.Infrastructure.Persistence;

public class DatabaseOptions
{
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.Sqlite;
    public string ConnectionString { get; set; } = "Data Source=Weda.Template.sqlite";
    public string DatabaseName { get; set; } = "WedaTemplate";
}
