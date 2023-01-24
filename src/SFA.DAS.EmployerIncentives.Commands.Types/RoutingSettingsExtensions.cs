using System.Diagnostics.CodeAnalysis;
using NServiceBus;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Commands.Types.PausePayments;
using SFA.DAS.EmployerIncentives.Infrastructure;

namespace SFA.DAS.EmployerIncentives.Commands.Types
{
    [ExcludeFromCodeCoverage]
    public static class RoutingSettingsExtensions
    {
        public static void AddRouting(this RoutingSettings routingSettings)
        {
            routingSettings.RouteToEndpoint(typeof(CreateIncentiveCommand), QueueNames.ApprenticeshipIncentivesCreate);
            routingSettings.RouteToEndpoint(typeof(CalculateEarningsCommand), QueueNames.ApprenticeshipIncentivesCalculateEarnings);
            routingSettings.RouteToEndpoint(typeof(CompleteEarningsCalculationCommand), QueueNames.CompleteEarningsCalculation);
            routingSettings.RouteToEndpoint(typeof(AddEmployerVendorIdCommand), QueueNames.AddEmployerVendorId);
            routingSettings.RouteToEndpoint(typeof(WithdrawCommand), QueueNames.ApprenticeshipIncentivesWithdraw);
            routingSettings.RouteToEndpoint(typeof(ReinstateApprenticeshipIncentiveCommand), QueueNames.ApprenticeshipIncentivesReinstate);
            routingSettings.RouteToEndpoint(typeof(EmployerWithdrawalCommand), QueueNames.EmployerWithdrawal);
            routingSettings.RouteToEndpoint(typeof(ComplianceWithdrawalCommand), QueueNames.ComplianceWithdrawal);
            routingSettings.RouteToEndpoint(typeof(ReinstateWithdrawalCommand), QueueNames.ReinstateWithdrawal);
            routingSettings.RouteToEndpoint(typeof(UpdateVendorRegistrationCaseStatusForAccountCommand), QueueNames.UpdateVendorRegistrationCaseStatus);
            routingSettings.RouteToEndpoint(typeof(SendEmploymentCheckRequestsCommand), QueueNames.SendEmploymentCheckRequests);
            routingSettings.RouteToEndpoint(typeof(UpdateEmploymentCheckCommand), QueueNames.UpdateEmploymentCheck);
            routingSettings.RouteToEndpoint(typeof(RefreshEmploymentCheckCommand), QueueNames.RefreshEmploymentCheckCommand);
            routingSettings.RouteToEndpoint(typeof(RecalculateEarningsCommand), QueueNames.RecalculateEarningsCommand);
            routingSettings.RouteToEndpoint(typeof(ValidationOverrideCommand), QueueNames.ValidationOverride);
            routingSettings.RouteToEndpoint(typeof(PausePaymentsCommand), QueueNames.PausePaymentsCommand);
        }
    }
}
