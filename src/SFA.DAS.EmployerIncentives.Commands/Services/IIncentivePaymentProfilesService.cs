using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Commands.Services
{
    public interface IIncentivePaymentProfilesService
    {
        IEnumerable<Domain.ValueObjects.IncentivePaymentProfile> MapToDomainIncentivePaymentProfiles();
    }
}
