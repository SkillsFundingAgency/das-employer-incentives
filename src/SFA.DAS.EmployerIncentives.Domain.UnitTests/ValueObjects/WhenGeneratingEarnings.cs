using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
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
    public class WhenGeneratingEarnings
    {
        private Mock<IIncentivePaymentProfilesService> _mockIncentivePaymentProfileService;
        private List<IncentivePaymentProfile> _incentivePaymentProfiles;
        private static readonly DateTime StartDate = new DateTime(2020, 10, 1);

        [SetUp]
        public void SetUp()
        {
            _mockIncentivePaymentProfileService = new Mock<IIncentivePaymentProfilesService>();
            _incentivePaymentProfiles = new IncentivePaymentProfileListBuilder()
                .WithIncentivePaymentProfiles(new List<IncentivePaymentProfile>
                {
                    new IncentivePaymentProfile(
                        new IncentivePhase(Phase.Phase1),
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

        // ReSharper disable InconsistentNaming
        private static readonly GenerateEarningsTestCase no_breaks =
            new GenerateEarningsTestCase(new List<BreakInLearning>(), 90, 365);

        private static readonly GenerateEarningsTestCase one_break_before_first_payment = new GenerateEarningsTestCase(
            new List<BreakInLearning>
            {
                new BreakInLearning(StartDate.AddDays(1)).SetEndDate(StartDate.AddDays(15))
            },90+14, 365+14);

        private static readonly GenerateEarningsTestCase two_breaks_before_first_payment = new GenerateEarningsTestCase(
            new List<BreakInLearning>
            {
                new BreakInLearning(StartDate.AddDays(1)).SetEndDate(StartDate.AddDays(15)),
                new BreakInLearning(StartDate.AddDays(1)).SetEndDate(StartDate.AddDays(7))
            }, 90 + 14 + 6, 365 + 14 + 6);

        private static readonly GenerateEarningsTestCase one_break_after_first_payment_before_second = new GenerateEarningsTestCase(
            new List<BreakInLearning>
            {
                new BreakInLearning(StartDate.AddDays(91)).SetEndDate(StartDate.AddDays(100)),
            }, 90, 365 + 9);

        private static readonly GenerateEarningsTestCase two_breaks_after_first_payment_before_second = new GenerateEarningsTestCase(
            new List<BreakInLearning>
            {
                new BreakInLearning(StartDate.AddDays(91)).SetEndDate(StartDate.AddDays(100)),
                new BreakInLearning(StartDate.AddDays(200)).SetEndDate(StartDate.AddDays(300)),
            },90, 365 + 9 + 100);

        private static readonly GenerateEarningsTestCase one_break_after_second_payment = new GenerateEarningsTestCase(
            new List<BreakInLearning>
            {
                new BreakInLearning(StartDate.AddDays(366)).SetEndDate(StartDate.AddDays(400))
            }, 90, 365);

        private static readonly GenerateEarningsTestCase one_break_after_first_payment_one_after_second = new GenerateEarningsTestCase(
            new List<BreakInLearning>
            {
                new BreakInLearning(StartDate.AddDays(91)).SetEndDate(StartDate.AddDays(100)),
                new BreakInLearning(StartDate.AddDays(400)).SetEndDate(StartDate.AddDays(444)),
            }, 90, 365 + 9);

        private static readonly GenerateEarningsTestCase two_breaks_after_first_payment_one_after_second = new GenerateEarningsTestCase(
            new List<BreakInLearning>
            {
                new BreakInLearning(StartDate.AddDays(91)).SetEndDate(StartDate.AddDays(100)),
                new BreakInLearning(StartDate.AddDays(222)).SetEndDate(StartDate.AddDays(333)),
                new BreakInLearning(StartDate.AddDays(600)).SetEndDate(StartDate.AddDays(700)),
            }, 90, 365 + 9 + 111);

        private static readonly GenerateEarningsTestCase[] GenerateEarningsTestCases =
        {
            no_breaks,
            one_break_before_first_payment,
            two_breaks_before_first_payment,
            one_break_after_first_payment_before_second,
            two_breaks_after_first_payment_before_second,
            one_break_after_second_payment,
            one_break_after_first_payment_one_after_second,
            two_breaks_after_first_payment_one_after_second,
        };

        [TestCaseSource(nameof(GenerateEarningsTestCases))]

        public void Then_the_payment_due_date_considers_breaks_in_learning(GenerateEarningsTestCase @case)
        {
            var apprenticeshipIncentive = new ApprenticeshipIncentiveBuilder()
                .WithStartDate(StartDate)
                .WithBreaksInLearning(@case.BreaksInLearning)
                .WithIncentivePhase(new IncentivePhase(Phase.Phase1))
                .WithApprenticeship(
                    new ApprenticeshipBuilder()
                        .WithDateOfBirth(StartDate.AddYears(-21))
                        .Build())
                .Build();

            var result = Incentive.Create(apprenticeshipIncentive, _incentivePaymentProfiles);

            result.IsEligible.Should().BeTrue();
            var payments = result.Payments.ToList();
            payments.Count.Should().Be(2);
            payments[0].PaymentDate.Should().Be(StartDate.AddDays(@case.FirstPaymentDays));
            payments[1].PaymentDate.Should().Be(StartDate.AddDays(@case.SecondPaymentDays));
        }


        public class GenerateEarningsTestCase
        {
            public int FirstPaymentDays;
            public int SecondPaymentDays;
            public List<BreakInLearning> BreaksInLearning;

            public GenerateEarningsTestCase(List<BreakInLearning> breaks, int firstPaymentDays, int secondPaymentDays)
            {
                BreaksInLearning = breaks;
                FirstPaymentDays = firstPaymentDays;
                SecondPaymentDays = secondPaymentDays;
            }
        }

        [TestCase(25, 1000,  1000)]
        [TestCase(24, 1200, 1200)]
        public void Then_the_the_payment_amounts_and_earning_type_is_set_regardless_of_breaks_in_learning(int age, decimal expectedAmount1, decimal expectedAmount2)
        {
            var date = new DateTime(2020, 10, 1);
            const int breakInLearning = 10;

            var breaks = new List<BreakInLearning>
            {
                new BreakInLearning(date).SetEndDate(date.AddDays(breakInLearning))
            };
            var apprenticeshipIncentive = new ApprenticeshipIncentiveBuilder()
                .WithStartDate(date)
                .WithBreaksInLearning(breaks)
                .WithIncentivePhase(new IncentivePhase(Phase.Phase1))
                .WithApprenticeship(
                    new ApprenticeshipBuilder()
                        .WithDateOfBirth(date.AddYears(-1 * age))
                        .Build())
                .Build();

            var result = Incentive.Create(apprenticeshipIncentive, _incentivePaymentProfiles);

            result.IsEligible.Should().BeTrue();
            var payments = result.Payments.ToList();
            payments.Count.Should().Be(2);
            payments[0].Amount.Should().Be(expectedAmount1);
            payments[0].EarningType.Should().Be(EarningType.FirstPayment);
            payments[1].Amount.Should().Be(expectedAmount2);
            payments[1].EarningType.Should().Be(EarningType.SecondPayment);
        }
    }
}
