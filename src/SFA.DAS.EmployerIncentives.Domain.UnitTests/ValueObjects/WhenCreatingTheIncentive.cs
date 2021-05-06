using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ValueObjects
{
    [TestFixture]
    public class WhenCreatingTheIncentive
    {
        private Mock<IIncentivePaymentProfilesService> _mockIncentivePaymentProfileService;
        private List<IncentivePaymentProfile> _incentivePaymentProfiles;

        [SetUp]
        public void SetUp()
        {
            _mockIncentivePaymentProfileService = new Mock<IIncentivePaymentProfilesService>();
            _incentivePaymentProfiles = new IncentivePaymentProfileListBuilder()
                .WithIncentivePaymentProfiles(new List<IncentivePaymentProfile>
            {
                 new IncentivePaymentProfile(
                     new IncentivePhase(Phase.Phase1_0),
                        new List<PaymentProfile>
                            {
                                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, 90, 1200),
                                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, 365, 1200),
                                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, 90, 1000),
                                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, 365, 1000)
                            })
                })
                .Build();

            _mockIncentivePaymentProfileService.Setup(m => m.Get()).ReturnsAsync(_incentivePaymentProfiles);
        }        

        [TestCase(25, IncentiveType.TwentyFiveOrOverIncentive, 1000, 90, 1000, 365)]
        [TestCase(24, IncentiveType.UnderTwentyFiveIncentive, 1200, 90, 1200, 365)]
        public async Task Then_the_properties_are_set_correctly(int age, IncentiveType expectedIncentiveType, decimal expectedAmount1, int expectedDays1, decimal expectedAmount2, int expectedDays2)
        {
            var date = new DateTime(2020, 10, 1);

            var apprenticeshipIncentive = new ApprenticeshipIncentiveBuilder()
                .WithStartDate(date)
                .WithBreakInLearningDayCount(0)
                .WithIncentivePhase(new IncentivePhase(Phase.Phase1_0))
                .WithApprenticeship(
                        new ApprenticeshipBuilder()
                        .WithDateOfBirth(date.AddYears(-1 * age))
                        .Build())
                .Build();

            var result = await Incentive.Create(apprenticeshipIncentive, _mockIncentivePaymentProfileService.Object);

            result.IsEligible.Should().BeTrue();            
            var payments = result.Payments.ToList();
            payments.Count().Should().Be(2);
            payments[0].Amount.Should().Be(expectedAmount1);
            payments[0].PaymentDate.Should().Be(date.AddDays(expectedDays1));
            payments[0].EarningType.Should().Be(EarningType.FirstPayment);
            payments[1].Amount.Should().Be(expectedAmount2);
            payments[1].PaymentDate.Should().Be(date.AddDays(expectedDays2));
            payments[1].EarningType.Should().Be(EarningType.SecondPayment);
        }

        [TestCase(25, IncentiveType.TwentyFiveOrOverIncentive, 1000, 90, 1000, 365)]
        [TestCase(24, IncentiveType.UnderTwentyFiveIncentive, 1200, 90, 1200, 365)]
        public async Task Then_the_due_date_includes_the_break_in_learning(int age, IncentiveType expectedIncentiveType, decimal expectedAmount1, int expectedDays1, decimal expectedAmount2, int expectedDays2)
        {
            var date = new DateTime(2020, 10, 1);
            int breakInLearning = 10;

            var apprenticeshipIncentive = new ApprenticeshipIncentiveBuilder()
                .WithStartDate(date)
                .WithBreakInLearningDayCount(breakInLearning)
                .WithIncentivePhase(new IncentivePhase(Phase.Phase1_0))
                .WithApprenticeship(
                        new ApprenticeshipBuilder()
                        .WithDateOfBirth(date.AddYears(-1 * age))
                        .Build())
                .Build();

            var result = await Incentive.Create(apprenticeshipIncentive, _mockIncentivePaymentProfileService.Object);

            result.IsEligible.Should().BeTrue();
            var payments = result.Payments.ToList();
            payments.Count.Should().Be(2);
            payments[0].Amount.Should().Be(expectedAmount1);
            payments[0].PaymentDate.Should().Be(date.AddDays(expectedDays1).AddDays(breakInLearning));
            payments[0].EarningType.Should().Be(EarningType.FirstPayment);
            payments[1].Amount.Should().Be(expectedAmount2);
            payments[1].PaymentDate.Should().Be(date.AddDays(expectedDays2).AddDays(breakInLearning));
            payments[1].EarningType.Should().Be(EarningType.SecondPayment);
        }

        [Test]
        public async Task And_Date_Is_Before_August_Then_the_application_is_not_eligible()
        {
            var date = new DateTime(2020, 07, 31);

            var apprenticeshipIncentive = new ApprenticeshipIncentiveBuilder()
                .WithStartDate(date)
                .WithBreakInLearningDayCount(0)
                .WithIncentivePhase(new IncentivePhase(Phase.Phase1_0))
                .WithApprenticeship(
                        new ApprenticeshipBuilder()
                        .WithDateOfBirth(date.AddYears(-1 * 25))
                        .Build())
                .Build();

            var result = await Incentive.Create(apprenticeshipIncentive, _mockIncentivePaymentProfileService.Object);

            result.IsEligible.Should().BeFalse();
            var payments = result.Payments.ToList();
            payments.Count.Should().Be(0);
        }

        [Test]
        public async Task And_Date_Is_After_May_Then_the_application_is_not_eligible()
        {
            var date = new DateTime(2021, 06, 1);

            var apprenticeshipIncentive = new ApprenticeshipIncentiveBuilder()
                .WithStartDate(date)
                .WithBreakInLearningDayCount(0)
                .WithIncentivePhase(new IncentivePhase(Phase.Phase1_0))
                .WithApprenticeship(
                        new ApprenticeshipBuilder()
                        .WithDateOfBirth(date.AddYears(-1 * 25))
                        .Build())
                .Build();

            var result = await Incentive.Create(apprenticeshipIncentive, _mockIncentivePaymentProfileService.Object);

            result.IsEligible.Should().BeFalse();
            var payments = result.Payments.ToList();
            payments.Count.Should().Be(0);
        }           
    }
}
