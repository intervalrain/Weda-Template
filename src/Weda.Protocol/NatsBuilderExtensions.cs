using Microsoft.Extensions.DependencyInjection;
using NATS.Client.Core;
using NATS.Net;
using Weda.Core.Infrastructure.Messaging.Nats.Abstractions;
using Weda.Core.Infrastructure.Messaging.Nats.Configuration;
using Weda.Protocol.Enums;
using Weda.Protocol.Serialization;

namespace Weda.Protocol;

public static class NatsBuilderExtensions
{
    public static NatsBuilder UseWedaProtocol(this NatsBuilder builder)
    {
        builder.Services.AddSingleton<ISubjectResolver, WedaSubjectResolver>();
        return builder;
    }

    /// <summary>
    /// Configures a connection to use Protobuf serialization
    /// </summary>
    public static NatsBuilder UseProtobuf(this NatsBuilder builder, string connectionName)
    {
        return builder.ConfigureConnection(connectionName, opts =>
            opts with { SerializerRegistry = ProtobufSerializerRegistry.Default });
    }

    /// <summary>
    /// Configures the default connection to use Protobuf serialization
    /// </summary>
    public static NatsBuilder UseProtobuf(this NatsBuilder builder)
    {
        return builder.UseProtobuf(builder.DefaultConnection);
    }

    public static NatsBuilder AddConnection(
        this NatsBuilder builder,
        EcoType protocol,
        string name,
        string url)
    {
        INatsSerializerRegistry registry = protocol switch
        {
            EcoType.Json => NatsClientDefaultSerializerRegistry.Default,
            EcoType.Protobuf => ProtobufSerializerRegistry.Default,
            _ => throw new ArgumentOutOfRangeException(nameof(protocol))
        };

        var opts = NatsOpts.Default with
        {
            Url = url,
            SerializerRegistry = registry
        };

        return builder.AddConnection(name, opts);
    }

    /// <summary>
    /// Binds configuration from NatsOptions with protocol-aware serializer selection
    /// </summary>
    public static NatsBuilder BindConfigurationWithProtocol(this NatsBuilder builder, NatsOptions options)
    {
        builder.DefaultConnection = options.DefaultConnection;

        foreach (var (name, config) in options.Connections)
        {
            var registry = GetSerializerRegistry(config.Protocol);
            var opts = CreateNatsOpts(config, registry);
            builder.AddConnection(name, opts);
        }

        return builder;
    }

    private static INatsSerializerRegistry GetSerializerRegistry(string protocol)
    {
        return protocol?.ToLowerInvariant() switch
        {
            "protobuf" => ProtobufSerializerRegistry.Default,
            "json" or null or "" => NatsClientDefaultSerializerRegistry.Default,
            _ => NatsClientDefaultSerializerRegistry.Default
        };
    }

    private static NatsOpts CreateNatsOpts(NatsConnectionConfig config, INatsSerializerRegistry registry)
    {
        return NatsOpts.Default with
        {
            Url = config.Url,
            Name = config.Name,
            SerializerRegistry = registry,
            AuthOpts = NatsAuthOpts.Default with
            {
                Username = config.Username,
                Password = config.Password,
                Token = config.Token,
                Jwt = config.Jwt,
                NKey = config.NKey,
                Seed = config.Seed,
                CredsFile = config.CredsFile,
                NKeyFile = config.NKeyFile
            }
        };
    }
}