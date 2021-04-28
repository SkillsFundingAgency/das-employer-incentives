using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ValueObjects
{
    [TestFixture]
    public class WhenCreatingTheIncentive
    {
        private List<IncentivePaymentProfile> _incentivePaymentProfiles;

        [SetUp]
        public void SetUp()
        {
            _incentivePaymentProfiles = new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile(
                    4,
                    new DateTime(2020,8,1),
                    new DateTime(2021,5,31),
                    new DateTime(2020,8,1),
                    new DateTime(2021,1,31),

                    new List<PaymentProfile>
                        {
                            new PaymentProfile(89, 1222, IncentiveType.UnderTwentyFiveIncentive, EarningType.FirstPayment), 
                            new PaymentProfile(354, 1333, IncentiveType.UnderTwentyFiveIncentive, EarningType.SecondPayment),
                            new PaymentProfile(90, 1000, IncentiveType.TwentyFiveOrOverIncentive, EarningType.FirstPayment),
                            new PaymentProfile(365, 1100, IncentiveType.TwentyFiveOrOverIncentive, EarningType.SecondPayment)
                        }),
                   };
        }

        [TestCase(25, IncentiveType.TwentyFiveOrOverIncentive, 1000, 90, 1100, 365)]
        [TestCase(24, IncentiveType.UnderTwentyFiveIncentive, 1222, 89, 1333, 354)]
        public void Then_the_properties_are_set_correctly(int age, IncentiveType expectedIncentiveType, decimal expectedAmount1, int expectedDays1, decimal expectedAmount2, int expectedDays2)
        {
            var date = new DateTime(2020, 10, 1);
            
            var result = new Incentive(date.AddYears(-1*age), date, _incentivePaymentProfiles, 0);

            result.IncentiveType.Should().Be(expectedIncentiveType);
            result.IsEligible.Should().BeTrue();            
            var payments = result.Payments.ToList();
            payments.Count.Should().Be(2);
            payments[0].Amount.Should().Be(expectedAmount1);
            payments[0].PaymentDate.Should().Be(date.AddDays(expectedDays1));
            payments[0].EarningType.Should().Be(EarningType.FirstPayment);
            payments[1].Amount.Should().Be(expectedAmount2);
            payments[1].PaymentDate.Should().Be(date.AddDays(expectedDays2));
            payments[1].EarningType.Should().Be(EarningType.SecondPayment);
        }

        [TestCase(25, IncentiveType.TwentyFiveOrOverIncentive, 1000, 90, 1000, 365)]
        [TestCase(24, IncentiveType.UnderTwentyFiveIncentive, 1200, 90, 1200, 365)]
        public void Then_the_due_date_includes_the_break_in_learning(int age, IncentiveType expectedIncentiveType, decimal expectedAmount1, int expectedDays1, decimal expectedAmount2, int expectedDays2)
        {
            var date = new DateTime(2020, 10, 1);
            int breakInLearning = 10;

            var result = new Incentive(date.AddYears(-1 * age), date, _incentivePaymentProfiles, breakInLearning);

            result.IncentiveType.Should().Be(expectedIncentiveType);
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
        public void And_Date_Is_Before_August_Then_the_apprentice_is_not_eligible()
        {
            var date = new DateTime(2020, 07, 31);
            var result = new Incentive(date.AddYears(-1 * 25), date, _incentivePaymentProfiles, 0);

            result.IsEligible.Should().BeFalse();
            var payments = result.Payments.ToList();
            payments.Count.Should().Be(0);
        }

        [Test]
        public void And_Date_Is_After_March_Then_the_apprentice_is_not_eligible()
        {
            var date = new DateTime(2021, 04, 1);
            var result = new Incentive(date.AddYears(-1 * 25), date, _incentivePaymentProfiles, 0);

            result.IsEligible.Should().BeFalse();
            var payments = result.Payments.ToList();
            payments.Count.Should().Be(0);
        }
    }
}
