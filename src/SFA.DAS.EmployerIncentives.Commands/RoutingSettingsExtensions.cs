using NServiceBus;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.EmployerIncentives.Commands
{
    public static class RoutingSettingsExtensions
    {
        private const string NotificationsMessageHandler = "SFA.DAS.Notifications.MessageHandlers";

        public static void AddRouting(this RoutingSettings routingSettings)
        {
            routingSettings.RouteToEndpoint(typeof(SendEmailCommand), NotificationsMessageHandler);
            routingSettings.RouteToEndpoint(typeof(CreateCommand), QueueNames.ApprenticeshipIncentivesCreate);
            routingSettings.RouteToEndpoint(typeof(CalculateEarningsCommand), QueueNames.ApprenticeshipIncentivesCalculateEarnings);
        }
    }
}
