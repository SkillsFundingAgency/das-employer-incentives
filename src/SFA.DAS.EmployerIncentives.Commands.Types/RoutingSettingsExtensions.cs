using NServiceBus;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Infrastructure;

namespace SFA.DAS.EmployerIncentives.Commands.Types
{
    public static class RoutingSettingsExtensions
    {
        public static void AddRouting(this RoutingSettings routingSettings)
        {
            routingSettings.RouteToEndpoint(typeof(CreateIncentiveCommand), QueueNames.ApprenticeshipIncentivesCreate);
            routingSettings.RouteToEndpoint(typeof(CalculateEarningsCommand), QueueNames.ApprenticeshipIncentivesCalculateEarnings);
            routingSettings.RouteToEndpoint(typeof(CompleteEarningsCalculationCommand), QueueNames.CompleteEarningsCalculation);
            routingSettings.RouteToEndpoint(typeof(AddEmployerVendorIdCommand), QueueNames.AddEmployerVendorId);
        }
    }
}
