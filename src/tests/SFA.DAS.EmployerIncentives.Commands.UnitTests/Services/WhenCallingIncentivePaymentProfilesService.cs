using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.Builders.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services
{
    [TestFixture]
    public class WhenCallingIncentivePaymentProfilesService
    {
        private IncentivePaymentProfilesService _sut;
        private Mock<IOptions<ApplicationSettings>> _mockApplicationSettings;
        private List<IncentivePaymentProfile> _incentivePaymentProfiles;

        [SetUp]
        public void Arrange()
        {
            _incentivePaymentProfiles = new IncentivePaymentProfileListBuilder().Build();

            _mockApplicationSettings = new Mock<IOptions<ApplicationSettings>>();
            _mockApplicationSettings.Setup(x => x.Value).Returns(new ApplicationSettings { IncentivePaymentProfiles = _incentivePaymentProfiles });

            _sut = new IncentivePaymentProfilesService(_mockApplicationSettings.Object);
        }

        [Test]
        public async Task then_config_settings_should_map_to_domain_values()
        {
            var result = (await _sut.Get()).ToList();

            result.Count.Should().Be(2);
            result[0].IncentivePhase.Identifier.Should().Be(_incentivePaymentProfiles[0].Phase);
            result[0].PaymentProfiles[0].IncentiveType.Should().Be(_incentivePaymentProfiles[0].PaymentProfiles[0].IncentiveType);
            result[0].PaymentProfiles[0].AmountPayable.Should().Be(_incentivePaymentProfiles[0].PaymentProfiles[0].AmountPayable);
            result[0].PaymentProfiles[0].DaysAfterApprenticeshipStart.Should().Be(_incentivePaymentProfiles[0].PaymentProfiles[0].DaysAfterApprenticeshipStart);
            result[0].PaymentProfiles[1].IncentiveType.Should().Be(_incentivePaymentProfiles[0].PaymentProfiles[1].IncentiveType);
            result[0].PaymentProfiles[1].AmountPayable.Should().Be(_incentivePaymentProfiles[0].PaymentProfiles[1].AmountPayable);
            result[0].PaymentProfiles[1].DaysAfterApprenticeshipStart.Should().Be(_incentivePaymentProfiles[0].PaymentProfiles[1].DaysAfterApprenticeshipStart);

            result[1].IncentivePhase.Identifier.Should().Be(_incentivePaymentProfiles[1].Phase);
            result[1].PaymentProfiles[0].IncentiveType.Should().Be(_incentivePaymentProfiles[1].PaymentProfiles[0].IncentiveType);
            result[1].PaymentProfiles[0].AmountPayable.Should().Be(_incentivePaymentProfiles[1].PaymentProfiles[0].AmountPayable);
            result[1].PaymentProfiles[0].DaysAfterApprenticeshipStart.Should().Be(_incentivePaymentProfiles[1].PaymentProfiles[0].DaysAfterApprenticeshipStart);
            result[1].PaymentProfiles[1].IncentiveType.Should().Be(_incentivePaymentProfiles[1].PaymentProfiles[1].IncentiveType);
            result[1].PaymentProfiles[1].AmountPayable.Should().Be(_incentivePaymentProfiles[1].PaymentProfiles[1].AmountPayable);
            result[1].PaymentProfiles[1].DaysAfterApprenticeshipStart.Should().Be(_incentivePaymentProfiles[1].PaymentProfiles[1].DaysAfterApprenticeshipStart);
        }
    }
}
