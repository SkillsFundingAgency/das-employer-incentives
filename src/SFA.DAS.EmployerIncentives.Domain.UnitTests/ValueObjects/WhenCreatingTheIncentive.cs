using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.Builders;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ValueObjects
{
    [TestFixture]
    public class WhenCreatingTheIncentive
    {

        [TestCase(25, IncentiveType.TwentyFiveOrOverIncentive, 750, 89, 750, 364)]
        [TestCase(24, IncentiveType.UnderTwentyFiveIncentive, 1000, 89, 1000, 364)]
        public async Task Then_the_properties_are_set_correctly_for_phase1(int age, IncentiveType expectedIncentiveType, decimal expectedAmount1, int expectedDays1, decimal expectedAmount2, int expectedDays2)
        {
            var date = new DateTime(2020, 10, 1);

            var apprenticeshipIncentive = new ApprenticeshipIncentiveBuilder()
                .WithStartDate(date)
                .WithIncentivePhase(new IncentivePhase(Phase.Phase1))
                .WithApprenticeship(
                        new ApprenticeshipBuilder()
                        .WithDateOfBirth(date.AddYears(-1 * age))
                        .Build())
                .Build();

            var result = await Incentive.Create(apprenticeshipIncentive);

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

        [TestCase(25, IncentiveType.TwentyFiveOrOverIncentive, 1500, 89, 1500, 364)]
        [TestCase(24, IncentiveType.UnderTwentyFiveIncentive, 1500, 89, 1500, 364)]
        public async Task Then_the_properties_are_set_correctly_for_phase2(int age, IncentiveType expectedIncentiveType, decimal expectedAmount1, int expectedDays1, decimal expectedAmount2, int expectedDays2)
        {
            var date = new DateTime(2021, 4, 1);

            var apprenticeshipIncentive = new ApprenticeshipIncentiveBuilder()
                .WithStartDate(date)
                .WithIncentivePhase(new IncentivePhase(Phase.Phase2))
                .WithApprenticeship(
                    new ApprenticeshipBuilder()
                        .WithDateOfBirth(date.AddYears(-1 * age))
                        .Build())
                .Build();

            var result = await Incentive.Create(apprenticeshipIncentive);

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

        [Test]
        public async Task And_Date_Is_Before_August_Then_the_application_is_not_eligible()
        {
            var date = new DateTime(2020, 07, 31);

            var apprenticeshipIncentive = new ApprenticeshipIncentiveBuilder()
                .WithStartDate(date)
                .WithIncentivePhase(new IncentivePhase(Phase.Phase1))
                .WithApprenticeship(
                        new ApprenticeshipBuilder()
                        .WithDateOfBirth(date.AddYears(-1 * 25))
                        .Build())
                .Build();

            var result = await Incentive.Create(apprenticeshipIncentive);

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
                .WithIncentivePhase(new IncentivePhase(Phase.Phase1))
                .WithApprenticeship(
                        new ApprenticeshipBuilder()
                        .WithDateOfBirth(date.AddYears(-1 * 25))
                        .Build())
                .Build();

            var result = await Incentive.Create(apprenticeshipIncentive);

            result.IsEligible.Should().BeFalse();
            var payments = result.Payments.ToList();
            payments.Count.Should().Be(0);
        }           
    }
}
