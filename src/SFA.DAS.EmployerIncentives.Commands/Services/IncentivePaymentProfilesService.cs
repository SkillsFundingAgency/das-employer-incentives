using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;

namespace SFA.DAS.EmployerIncentives.Commands.Services
{
    public class IncentivePaymentProfilesService : IIncentivePaymentProfilesService
    {
        private readonly ApplicationSettings _applicationSettings;

        public IncentivePaymentProfilesService(IOptions<ApplicationSettings> applicationSettings)
        {
            _applicationSettings = applicationSettings.Value;
        }

        public Task<IEnumerable<Domain.ValueObjects.IncentivePaymentProfile>> Get()
        {
            if (_applicationSettings?.IncentivePaymentProfiles == null)
            {
                return Task.FromResult(Enumerable.Empty<Domain.ValueObjects.IncentivePaymentProfile>());
            }
            return Task.FromResult(_applicationSettings.IncentivePaymentProfiles.Select(x =>
                new Domain.ValueObjects.IncentivePaymentProfile(x.IncentiveType,
                    MapToDomainPaymentProfiles(x.PaymentProfiles).ToList())));
        }

        private IEnumerable<Domain.ValueObjects.PaymentProfile> MapToDomainPaymentProfiles(List<Infrastructure.Configuration.PaymentProfile> paymentProfiles)
        {
            if (paymentProfiles == null)
            {
                return Enumerable.Empty<Domain.ValueObjects.PaymentProfile>();
            }
            return paymentProfiles.Select(x => new Domain.ValueObjects.PaymentProfile(x.DaysAfterApprenticeshipStart, x.AmountPayable));
        }
    }
}
