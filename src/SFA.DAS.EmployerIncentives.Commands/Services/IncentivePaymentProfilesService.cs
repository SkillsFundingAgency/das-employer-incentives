using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using PaymentProfile = SFA.DAS.EmployerIncentives.Infrastructure.Configuration.PaymentProfile;

namespace SFA.DAS.EmployerIncentives.Commands.Services
{
    public class IncentivePaymentProfilesService : IIncentivePaymentProfilesService
    {
        private readonly ApplicationSettings _applicationSettings;
        private static readonly EarningType[] EarningTypes = { EarningType.FirstPayment, EarningType.SecondPayment };

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

        private static IEnumerable<Domain.ValueObjects.PaymentProfile> MapToDomainPaymentProfiles(IList<PaymentProfile> profiles)
        {
            var result = new List<Domain.ValueObjects.PaymentProfile>();
            if (profiles == null) return result;

            var incentives = profiles.OrderBy(x => x.DaysAfterApprenticeshipStart).GroupBy(x => x.IncentiveType);

            foreach (var incentive in incentives)
            {
                result.AddRange(
                    incentive.Select((profile, index) =>
                        new Domain.ValueObjects.PaymentProfile(
                            profile.DaysAfterApprenticeshipStart,
                            profile.AmountPayable,
                            profile.IncentiveType,
                            EarningTypes[index])
                    ));
            }

            return result;
        }
    }
}
