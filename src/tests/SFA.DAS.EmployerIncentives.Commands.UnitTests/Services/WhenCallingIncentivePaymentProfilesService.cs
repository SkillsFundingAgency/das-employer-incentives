using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System;
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
            _incentivePaymentProfiles = new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile
                {
                    PaymentProfiles = new List<PaymentProfile>
                        {
                         new PaymentProfile {AmountPayable = 1002, DaysAfterApprenticeshipStart = 365, IncentiveType = IncentiveType.TwentyFiveOrOverIncentive},
                         new PaymentProfile {AmountPayable = 1000, DaysAfterApprenticeshipStart = 91, IncentiveType = IncentiveType.UnderTwentyFiveIncentive},
                         new PaymentProfile {AmountPayable = 1001, DaysAfterApprenticeshipStart = 366, IncentiveType = IncentiveType.UnderTwentyFiveIncentive},
                         new PaymentProfile {AmountPayable = 1003, DaysAfterApprenticeshipStart = 90, IncentiveType = IncentiveType.TwentyFiveOrOverIncentive},
                        },
                    IncentivePhase = IncentivePhase.Phase1_0,
                    EligibleApplicationDates= (new DateTime(2020,8,1), new DateTime(2021,5,31)),
                    EligibleTrainingDates= (new DateTime(2020,7,1), new DateTime(2021,6,30)),
                    MinRequiredAgreementVersion = 4
                },
                new IncentivePaymentProfile
                {
                    PaymentProfiles = new List<PaymentProfile>
                    {new PaymentProfile {AmountPayable = 1200, DaysAfterApprenticeshipStart = 190, IncentiveType = IncentiveType.TwentyFiveOrOverIncentive},
                    new PaymentProfile {AmountPayable = 1201, DaysAfterApprenticeshipStart = 191, IncentiveType = IncentiveType.UnderTwentyFiveIncentive}},
                    IncentivePhase = IncentivePhase.Phase2_0,
                    EligibleApplicationDates= (new DateTime(2020,9,1), new DateTime(2021,6,30)),
                    EligibleTrainingDates= (new DateTime(2021,6,1), new DateTime(2021,7,29)),
                    MinRequiredAgreementVersion = 5
                }
            };

            _mockApplicationSettings = new Mock<IOptions<ApplicationSettings>>();
            _mockApplicationSettings.Setup(x => x.Value).Returns(new ApplicationSettings { IncentivePaymentProfiles = _incentivePaymentProfiles });

            _sut = new IncentivePaymentProfilesService(_mockApplicationSettings.Object);
        }

        [Test]
        public async Task then_config_settings_should_map_to_domain_values()
        {
            // Act
            var config = await _sut.Get();
           
            // Assert
            config.GetPaymentProfiles(IncentiveType.TwentyFiveOrOverIncentive, new DateTime(2020, 8, 1))
                .Single(x => x.EarningType == EarningType.FirstPayment).Should().BeEquivalentTo(_incentivePaymentProfiles[0].PaymentProfiles[3]);
       
            config.GetPaymentProfiles(IncentiveType.TwentyFiveOrOverIncentive, new DateTime(2020, 8, 1))
                .Single(x => x.EarningType == EarningType.SecondPayment).Should().BeEquivalentTo(_incentivePaymentProfiles[0].PaymentProfiles[0]);

            config.GetPaymentProfiles(IncentiveType.UnderTwentyFiveIncentive, new DateTime(2020, 8, 1))
                .Single(x => x.EarningType == EarningType.FirstPayment).Should().BeEquivalentTo(_incentivePaymentProfiles[0].PaymentProfiles[1]);
           
            config.GetPaymentProfiles(IncentiveType.UnderTwentyFiveIncentive, new DateTime(2020, 8, 1))
                .Single(x => x.EarningType == EarningType.SecondPayment).Should().BeEquivalentTo(_incentivePaymentProfiles[0].PaymentProfiles[2]);
           
            config.GetPaymentProfiles(IncentiveType.TwentyFiveOrOverIncentive, new DateTime(2021, 6, 5))
                .Single(x => x.EarningType == EarningType.FirstPayment).Should().BeEquivalentTo(_incentivePaymentProfiles[1].PaymentProfiles[0]);

            config.GetPaymentProfiles(IncentiveType.UnderTwentyFiveIncentive, new DateTime(2021, 6, 5)).
                Single(x => x.EarningType == EarningType.FirstPayment).Should().BeEquivalentTo(_incentivePaymentProfiles[1].PaymentProfiles[1]);

            config.GetPaymentProfiles(IncentiveType.TwentyFiveOrOverIncentive, new DateTime(2021, 6, 5))
                .SingleOrDefault(x => x.EarningType == EarningType.SecondPayment).Should().BeNull();
            
            config.GetPaymentProfiles(IncentiveType.UnderTwentyFiveIncentive, new DateTime(2021, 6, 5))
                .SingleOrDefault(x => x.EarningType == EarningType.SecondPayment).Should().BeNull();
        }
    }
}
