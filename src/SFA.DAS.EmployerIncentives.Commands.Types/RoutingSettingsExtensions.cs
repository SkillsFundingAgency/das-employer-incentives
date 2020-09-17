using NServiceBus;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Infrastructure;

namespace SFA.DAS.EmployerIncentives.Commands.Types
{
    public static class RoutingSettingsExtensions
    {
        public static void AddRouting(this RoutingSettings routingSettings)
        {
            routingSettings.RouteToEndpoint(typeof(CreateCommand), QueueNames.ApprenticeshipIncentivesCreate);
            routingSettings.RouteToEndpoint(typeof(CalculateEarningsCommand), QueueNames.ApprenticeshipIncentivesCalculateEarnings);
        }
    }
}
