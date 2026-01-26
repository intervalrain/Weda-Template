using System.Reflection;
using Weda.Core.Infrastructure.Nats.Attributes;
using Weda.Core.Infrastructure.Nats.Enums;

namespace Weda.Core.Infrastructure.Nats.Discovery;

public class EventControllerDiscovery(string defaultConnection = "default")
{
    private readonly string _defaultConnection = defaultConnection;

    public List<EndpointDescriptor> Endpoints { get; } = [];
    public List<EndpointDescriptor> RequestReplyEndpoints { get; } = [];
    public List<EndpointDescriptor> CorePubSubEndpoints { get; } = [];
    public List<EndpointDescriptor> JetStreamConsumeEndpoints { get; } = [];
    public List<EndpointDescriptor> JetStreamFetchEndpoints { get; } = [];

    public void DiscoverControllers(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var controllerTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(EventController)));

            foreach (var controllerType in controllerTypes)
            {
                DiscoverEndpoints(controllerType);
            }
        }
    }

    private void DiscoverEndpoints(Type controllerType)
    {
        var classStreamAttr = controllerType.GetCustomAttribute<StreamAttribute>();
        var classConsumerAttr = controllerType.GetCustomAttribute<ConsumerAttribute>();
        var classConnectionAttr = controllerType.GetCustomAttribute<ConnectionAttribute>();

        var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<SubjectAttribute>() is not null)
            .ToList();

        foreach (var method in methods)
        {
            var subjectAttr = method.GetCustomAttribute<SubjectAttribute>()!;
            var methodConnectionAttr = method.GetCustomAttribute<ConnectionAttribute>();

            var connectionName = methodConnectionAttr?.Name
                ?? classConnectionAttr?.Name
                ?? _defaultConnection;

            var streamTemplate = subjectAttr.Stream ?? classStreamAttr?.Template;
            var consumerTemplate = subjectAttr.Consumer ?? classConsumerAttr?.Name;

            var streamName = streamTemplate is not null
                ? TemplateResolver.Resolve(streamTemplate, controllerType, method.Name)
                : null;
            var consumerName = consumerTemplate is not null
                ? TemplateResolver.Resolve(consumerTemplate, controllerType, method.Name)
                : null;

            var responseType = GetResponseType(method);
            var mode = DetermineEndpointMode(responseType, subjectAttr.DeliveryMode, subjectAttr.ConsumerMode);

            var descriptor = new EndpointDescriptor
            {
                ControllerType = controllerType,
                Method = method,
                SubjectPattern = subjectAttr.Pattern,
                Mode = mode,
                ConnectionName = connectionName,
                StreamName = streamName,
                ConsumerName = consumerName,
                RequestType = GetRequestType(method),
                ResponseType = GetResponseType(method),
            };

            Endpoints.Add(descriptor);
            switch (descriptor.Mode)
            {
                case EndpointMode.RequestReply:
                    RequestReplyEndpoints.Add(descriptor);
                    break;
                case EndpointMode.CorePubSub:
                    CorePubSubEndpoints.Add(descriptor);
                    break;
                case EndpointMode.JetStreamConsume:
                    JetStreamConsumeEndpoints.Add(descriptor);
                    break;
                case EndpointMode.JetStreamFetch:
                    JetStreamFetchEndpoints.Add(descriptor);
                    break;
                default:
                    break;
            }
        }
    }

    private EndpointMode DetermineEndpointMode(Type? responseType, DeliveryMode deliveryMode, ConsumerMode consumerMode)
    {
        if (responseType is not null)
        {
            return EndpointMode.RequestReply;
        }

        if (deliveryMode == DeliveryMode.Core)
        {
            return EndpointMode.CorePubSub;
        }

        return consumerMode == ConsumerMode.Consume ? EndpointMode.JetStreamConsume : EndpointMode.JetStreamFetch;
    }

    private static Type? GetRequestType(MethodInfo method)
    {
        var parameters = method.GetParameters();
        return parameters.Length > 0 ? parameters[0].ParameterType : null;
    }

    private static Type? GetResponseType(MethodInfo method)
    {
        var returnType = method.ReturnType;

        if (returnType == typeof(Task) || returnType == typeof(ValueTask) || returnType == typeof(void))
        {
            return null;
        }

        if (returnType.IsGenericType)
        {
            var genericDef = returnType.GetGenericTypeDefinition();
            if (genericDef == typeof(Task<>) || genericDef == typeof(ValueTask<>))
            {
                return returnType.GetGenericArguments()[0];
            }
        }

        return returnType;
    }
}