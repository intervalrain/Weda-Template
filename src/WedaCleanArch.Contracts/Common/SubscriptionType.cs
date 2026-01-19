using System.Text.Json.Serialization;

namespace WedaCleanArch.Contracts.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubscriptionType
{
    Basic,
    Pro,
}