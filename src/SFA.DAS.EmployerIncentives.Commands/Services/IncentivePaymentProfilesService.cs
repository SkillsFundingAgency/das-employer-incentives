using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;

namespace SFA.DAS.EmployerIncentives.Commands.Services
{
    public class IncentivePaymentProfilesService : IIncentivePaymentProfilesService
    {
        private ApplicationSettings _applicationSettings;
        public IncentivePaymentProfilesService(IOptions<ApplicationSettings> applicationSettings)
        {
            _applicationSettings = applicationSettings.Value;
        }

        public IEnumerable<Domain.ValueObjects.IncentivePaymentProfile> MapToDomainIncentivePaymentProfiles()
        {
            if (_applicationSettings?.IncentivePaymentProfiles == null)
            {
                return Enumerable.Empty<Domain.ValueObjects.IncentivePaymentProfile>();
            }
            return _applicationSettings.IncentivePaymentProfiles.Select(x =>
                new Domain.ValueObjects.IncentivePaymentProfile(x.IncentiveType,
                    MapToDomainPaymentProfiles(x.PaymentProfiles).ToList()));
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
