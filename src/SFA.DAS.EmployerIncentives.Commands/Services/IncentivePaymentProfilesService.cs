using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using PaymentProfile = SFA.DAS.EmployerIncentives.Infrastructure.Configuration.PaymentProfile;

namespace SFA.DAS.EmployerIncentives.Commands.Services
{
    public class IncentivePaymentProfilesService : IIncentivePaymentProfilesService
    {
        private readonly ApplicationSettings _applicationSettings;

        public IncentivePaymentProfilesService(IOptions<ApplicationSettings> applicationSettings)
        {
            _applicationSettings = applicationSettings.Value;
        }

        public Task<IncentivesConfiguration> Get()
        {
            var profiles = _applicationSettings.IncentivePaymentProfiles.Select(x =>
                new Domain.ValueObjects.IncentivePaymentProfile(
                    x.IncentivePhase,
                    x.MinRequiredAgreementVersion,
                    x.EligibleApplicationDates.Start,
                    x.EligibleApplicationDates.End,
                    x.EligibleTrainingDates.Start,
                    x.EligibleTrainingDates.End,
                    MapToDomainPaymentProfiles(x.PaymentProfiles).ToList()
                    )).ToList();

            return Task.FromResult(new IncentivesConfiguration(profiles));
        }

        private static IEnumerable<Domain.ValueObjects.PaymentProfile> MapToDomainPaymentProfiles(IReadOnlyCollection<PaymentProfile> paymentProfiles)
        {
            return paymentProfiles == null ? Enumerable.Empty<Domain.ValueObjects.PaymentProfile>() : paymentProfiles.Select(x => new Domain.ValueObjects.PaymentProfile(x.DaysAfterApprenticeshipStart, x.AmountPayable, x.IncentiveType));
        }
    }
}
