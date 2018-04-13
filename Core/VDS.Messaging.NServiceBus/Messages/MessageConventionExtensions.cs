using System;
using NServiceBus;

namespace VDS.Messaging.NServiceBus.Messages
{
    public static class MessageConventionExtensions
    {
        public static void UseNServiceBusMessageConventions(this EndpointConfiguration endpointConfiguration)
        {
            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(type => type.Name != null && type.Name.EndsWith("Command"));

            conventions.DefiningEventsAs(type => type.Name != null && type.Name.EndsWith("Event"));

            conventions.DefiningTimeToBeReceivedAs(type => TimeSpan.MaxValue);
        }
    }
}