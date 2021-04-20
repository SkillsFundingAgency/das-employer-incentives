﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;

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
                    IncentiveType = IncentiveType.TwentyFiveOrOverIncentive,
                    PaymentProfiles = new List<PaymentProfile>
                        {new PaymentProfile {AmountPayable = 1000, DaysAfterApprenticeshipStart = 90}},
                    IncentivePhase = IncentivePhase.Phase1_0,
                    EligibleApplicationDates= (new DateTime(2020,8,1), new DateTime(2021,5,31)),
                    EligibleTrainingDates= (new DateTime(2020,7,1), new DateTime(2021,6,31)),
                    EligibleEmploymentDates= (new DateTime(2020,6,1), new DateTime(2021,7,31)),
                    MinRequiredAgreementVersion = 4
                },
                new IncentivePaymentProfile
                {
                    IncentiveType = IncentiveType.UnderTwentyFiveIncentive,
                    PaymentProfiles = new List<PaymentProfile>
                        {new PaymentProfile {AmountPayable = 1500, DaysAfterApprenticeshipStart = 90}},
                    IncentivePhase = IncentivePhase.Phase2_0,
                    EligibleApplicationDates= (new DateTime(2020,9,1), new DateTime(2021,6,31)),
                    EligibleTrainingDates= (new DateTime(2020,10,1), new DateTime(2021,7,31)),
                    EligibleEmploymentDates= (new DateTime(2020,11,1), new DateTime(2021,8,31)),
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
            List<Domain.ValueObjects.IncentivePaymentProfile> result = (await _sut.Get()).ToList();


            result.Should().BeEquivalentTo(_incentivePaymentProfiles);

        //    result.Count.Should().Be(2);
        //    result[0].IncentiveType.Should().Be(_incentivePaymentProfiles[0].IncentiveType);
        //    result[0].PaymentProfiles[0].AmountPayable.Should().Be(_incentivePaymentProfiles[0].PaymentProfiles[0].AmountPayable);
        //    result[0].PaymentProfiles[0].DaysAfterApprenticeshipStart.Should().Be(_incentivePaymentProfiles[0].PaymentProfiles[0].DaysAfterApprenticeshipStart);
        //    result[0].IncentivePhase.Should().Be(_incentivePaymentProfiles[0].IncentivePhase);
        //    result[0].MinRequiredAgreementVersion.Should().Be(_incentivePaymentProfiles[0].MinRequiredAgreementVersion);

        //    result[1].IncentiveType.Should().Be(_incentivePaymentProfiles[1].IncentiveType);
        //    result[1].PaymentProfiles[0].AmountPayable.Should().Be(_incentivePaymentProfiles[1].PaymentProfiles[0].AmountPayable);
        //    result[1].PaymentProfiles[0].DaysAfterApprenticeshipStart.Should().Be(_incentivePaymentProfiles[1].PaymentProfiles[0].DaysAfterApprenticeshipStart);
        }
    }
}
