using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentive.Builders;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ValueObjects
{
    [TestFixture]
    public class WhenHasSignedRequiredAgreementVersion
    {
        private Mock<IIncentivePaymentProfilesService> _mockIncentivePaymentProfileService;
        private List<IncentivePaymentProfile> _incentivePaymentProfiles;

        [SetUp]
        public void SetUp()
        {
            _mockIncentivePaymentProfileService = new Mock<IIncentivePaymentProfilesService>();
            _incentivePaymentProfiles = new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile(IncentiveType.TwentyFiveOrOverIncentive,
                    new List<PaymentProfile>
                        {new PaymentProfile(90, 1000), new PaymentProfile(365, 1000)}),

                new IncentivePaymentProfile(IncentiveType.UnderTwentyFiveIncentive,
                    new List<PaymentProfile>
                        {new PaymentProfile(90, 1200), new PaymentProfile(365, 1200)})
            };

            _mockIncentivePaymentProfileService.Setup(m => m.Get()).ReturnsAsync(_incentivePaymentProfiles);
        }

        [TestCase("2020-07-31")]
        [TestCase("2021-06-01")]
        public async Task Then_returns_true_when_ineligible(DateTime startDate)
        {
            var apprenticeshipIncentive = new ApprenticeshipIncentiveBuilder()
                .WithStartDate(startDate)
                .WithBreakInLearningDayCount(0)
                .WithApprenticeship(
                        new ApprenticeshipBuilder()
                        .WithDateOfBirth(DateTime.Now.AddYears(-20))
                        .Build())
                .Build();

            var incentive = await Incentive.Create(apprenticeshipIncentive, _mockIncentivePaymentProfileService.Object);

            var result = incentive.IsNewAgreementRequired(10);

            result.Should().BeTrue();
        }

        [TestCase("2020-09-01", 3)]
        [TestCase("2021-02-05", 4)]
        public async Task Then_returns_true_when_incorrect_agreement_signed(DateTime startDate, int signedAgreementVersion)
        {
            var apprenticeshipIncentive = new ApprenticeshipIncentiveBuilder()
                .WithStartDate(startDate)
                .WithBreakInLearningDayCount(0)
                .WithApprenticeship(
                        new ApprenticeshipBuilder()
                        .WithDateOfBirth(DateTime.Now.AddYears(-20))
                        .Build())
                .Build();

            var incentive = await Incentive.Create(apprenticeshipIncentive, _mockIncentivePaymentProfileService.Object);

            var result = incentive.IsNewAgreementRequired(signedAgreementVersion);

            result.Should().BeTrue();
        }

        [TestCase("2020-09-01", 4)]
        [TestCase("2021-02-05", 5)]
        [TestCase("2021-02-05", 6)]
        public async Task Then_returns_false_when_correct_agreement_signed(DateTime startDate, int signedAgreementVersion)
        {
            var apprenticeshipIncentive = new ApprenticeshipIncentiveBuilder()
                .WithStartDate(startDate)
                .WithBreakInLearningDayCount(0)
                .WithApprenticeship(
                        new ApprenticeshipBuilder()
                        .WithDateOfBirth(DateTime.Now.AddYears(-20))
                        .Build())
                .Build();

            var incentive = await Incentive.Create(apprenticeshipIncentive, _mockIncentivePaymentProfileService.Object);

            var result = incentive.IsNewAgreementRequired(signedAgreementVersion);

            result.Should().BeFalse();
        }
    }
}
