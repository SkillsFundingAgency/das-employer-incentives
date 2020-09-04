using NServiceBus;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;
using SFA.DAS.EmployerIncentives.Commands.Types.Apprenticeship;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.EmployerIncentives.Commands
{
    public static class RoutingSettingsExtensions
    {
        private const string NotificationsMessageHandler = "SFA.DAS.Notifications.MessageHandlers";
        private const string DomainMessageHandlers = "SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers";

        public static void AddRouting(this RoutingSettings routingSettings)
        {
            routingSettings.RouteToEndpoint(typeof(SendEmailCommand), NotificationsMessageHandler);
            routingSettings.RouteToEndpoint(typeof(CalculateClaimCommand), DomainMessageHandlers);
            routingSettings.RouteToEndpoint(typeof(CalculateEarningsCommand), DomainMessageHandlers);            
        }
    }
}
