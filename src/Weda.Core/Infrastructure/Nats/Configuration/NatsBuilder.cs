using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NATS.Client.Core;
using NATS.Client.Serializers.Json;
using NATS.Net;

namespace Weda.Core.Infrastructure.Nats.Configuration;

public class NatsBuilder(IServiceCollection services)
{
    private readonly IServiceCollection _services = services;
    private readonly Dictionary<string, NatsOpts> _connections = [];
    private string _defaultConnection = "default";

    public string DefaultConnection
    {
        get => _defaultConnection;
        set => _defaultConnection = value;
    }

    public NatsBuilder AddConnection(string name, string url, Action<NatsOpts>? configure = null)
    {
        var opts = NatsOpts.Default with
        {
            Url = url,
            SerializerRegistry = NatsClientDefaultSerializerRegistry.Default
        };
        configure?.Invoke(opts);
        _connections[name] = opts;
        return this;
    }

    public NatsBuilder AddConnection(string name, NatsOpts opts)
    {
        _connections[name] = opts;
        return this;
    }

    internal void Build()
    {
        _services.TryAddSingleton<INatsConnectionProvider>(sp => new NatsConnectionProvider(_connections, _defaultConnection));
        _services.TryAddSingleton(sp => sp.GetRequiredService<INatsConnectionProvider>().GetConnection());
        _services.TryAddSingleton(sp => sp.GetRequiredService<INatsConnectionProvider>().GetJetStreamContext());
    }
}