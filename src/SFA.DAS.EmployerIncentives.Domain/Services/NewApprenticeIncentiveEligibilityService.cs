using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.ValueObjects;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Domain.Services
{
    public class NewApprenticeIncentiveEligibilityService : INewApprenticeIncentiveEligibilityService
    {
        private readonly IUlnValidationService _ulnValidationService;
        private static IIncentivePaymentProfilesService _incentivePaymentProfilesService;

        public NewApprenticeIncentiveEligibilityService(
            IUlnValidationService ulnValidationService,
            IIncentivePaymentProfilesService incentivePaymentProfilesService)
        {
            _ulnValidationService = ulnValidationService;
            _incentivePaymentProfilesService = incentivePaymentProfilesService;
        }

        public async Task<bool> IsApprenticeshipEligible(Apprenticeship apprenticeship)
        {
            if (!apprenticeship.IsApproved || await IsStartDateOutsideSchemeRange(apprenticeship))
            {
                return false;
            }

            if (await _ulnValidationService.UlnAlreadyOnSubmittedIncentiveApplication(apprenticeship.UniqueLearnerNumber))
            {
                return false;
            }

            return true;
        }

        private static async Task<bool> IsStartDateOutsideSchemeRange(Apprenticeship apprenticeship)
        {
            var config = await _incentivePaymentProfilesService.Get();
           
            return !config.IsEligible(apprenticeship.StartDate);
        }
    }
}
