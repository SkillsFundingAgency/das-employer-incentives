using NServiceBus;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.EmployerIncentives.Commands
{
    public static class RoutingSettingsExtensions
    {
        private const string NotificationsMessageHandler = "SFA.DAS.Notifications.MessageHandlers";
        private const string DomainMessageHandler = "SFA.DAS.EmployerIncentives.MessageHandlers";

        public static void AddRouting(this RoutingSettings routingSettings)
        {
            routingSettings.RouteToEndpoint(typeof(SendEmailCommand), NotificationsMessageHandler);
            routingSettings.RouteToEndpoint(typeof(CalculateClaimCommand), DomainMessageHandler);
        }
    }
}
