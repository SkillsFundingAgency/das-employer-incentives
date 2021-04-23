using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    public class IncentivesConfigurationTests
    {
        private IncentiveProfiles _sut;

        [SetUp]
        public void Arrange()
        {
            var paymentProfilesPhase1 = new List<PaymentProfile>
            {
                new PaymentProfile(1, 10, IncentiveType.UnderTwentyFiveIncentive, EarningType.FirstPayment),
                new PaymentProfile(2, 20, IncentiveType.UnderTwentyFiveIncentive, EarningType.SecondPayment),
                new PaymentProfile(11, 110, IncentiveType.TwentyFiveOrOverIncentive, EarningType.FirstPayment),
                new PaymentProfile(22, 220, IncentiveType.TwentyFiveOrOverIncentive, EarningType.SecondPayment),
            };
            var paymentProfilesPhase1_1 = new List<PaymentProfile>
            {
                new PaymentProfile(5, 50, IncentiveType.UnderTwentyFiveIncentive, EarningType.FirstPayment),
                new PaymentProfile(6, 60, IncentiveType.UnderTwentyFiveIncentive, EarningType.SecondPayment),
                new PaymentProfile(55, 550, IncentiveType.TwentyFiveOrOverIncentive, EarningType.FirstPayment),
                new PaymentProfile(66, 660, IncentiveType.TwentyFiveOrOverIncentive, EarningType.SecondPayment),
            };

            var paymentProfilesPhase2 = new List<PaymentProfile>
            {
                new PaymentProfile(7, 70, IncentiveType.UnderTwentyFiveIncentive, EarningType.FirstPayment),
                new PaymentProfile(8, 80, IncentiveType.UnderTwentyFiveIncentive, EarningType.SecondPayment),
                new PaymentProfile(77, 770, IncentiveType.TwentyFiveOrOverIncentive, EarningType.FirstPayment),
                new PaymentProfile(88, 880, IncentiveType.TwentyFiveOrOverIncentive, EarningType.SecondPayment),
            };

            var paymentProfiles = new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile(
                    IncentivePhase.Phase1_0,
                    4,
                    new DateTime(2020,8,1),
                    new DateTime(2021,5,31),
                    new DateTime(2020,8,1),
                    new DateTime(2021,1,31),
                    paymentProfilesPhase1),
                new IncentivePaymentProfile(
                    IncentivePhase.Phase1_1,
                    5,
                    new DateTime(2020,8,1),
                    new DateTime(2021,5,31),
                    new DateTime(2021,2,2), // Intentional 1-day gap
                    new DateTime(2021,5,31),
                    paymentProfilesPhase1_1),
                new IncentivePaymentProfile(
                    IncentivePhase.Phase2_0,
                    6,
                    new DateTime(2021,6,1),
                    new DateTime(2021,11,30),
                    new DateTime(2021,4,1),
                    new DateTime(2021,11,30),
                    paymentProfilesPhase2),
            };

            _sut = new IncentiveProfiles(paymentProfiles);
        }

        [Test]
        [TestCase("2020-8-1", true)]
        [TestCase("2021-1-1", true)]
        [TestCase("2021-2-2", true)]
        [TestCase("2021-3-31", true)]
        [TestCase("2021-4-1", true)]
        [TestCase("2021-5-1", true)]
        [TestCase("2021-6-1", true)]
        [TestCase("2021-11-30", true)]
        [TestCase("2020-7-29", false)]
        [TestCase("2021-12-30", false)]
        [TestCase("2021-12-30", false)]
        [TestCase("2021-2-1", false)]
        public void IsEligible_returns_result_based_on_training_start_date(DateTime trainingStartDate, bool expected)
        {
            _sut.IsEligible(trainingStartDate).Should().Be(expected, trainingStartDate.ToString("yyyy-M-d"));
        }

        [Test]
        [TestCase("2020-8-1", 4)]
        [TestCase("2021-1-1", 4)]
        [TestCase("2021-2-2", 5)]
        [TestCase("2021-3-31", 5)]
        [TestCase("2021-4-1", 5)]
        [TestCase("2021-5-1", 5)]
        [TestCase("2021-6-1", 6)]
        [TestCase("2021-11-30", 6)]
        public void GetMinimumAgreementVersion_returns_minimum_agreement_version_based_on_training_start_date(DateTime trainingStartDate, byte expectedVersion)
        {
            _sut.GetMinimumAgreementVersion(trainingStartDate).Should().Be(expectedVersion);
        }

        [Test]
        [TestCase("2020-7-29")]
        [TestCase("2021-12-30")]
        [TestCase("2021-12-30")]
        [TestCase("2021-2-1")]
        public void GetMinimumAgreementVersion_throws_exception_for_ineligible_incentives(DateTime trainingStartDate)
        {
            Action act = () => _sut.GetMinimumAgreementVersion(trainingStartDate);
            act.Should().Throw<MissingPaymentProfileException>().WithMessage($"Payment profiles not found for Training Start Date {trainingStartDate}");
        }

        [Test]
        [TestCase("2020-8-1", IncentiveType.UnderTwentyFiveIncentive, 1, 10, 2, 20)]
        [TestCase("2020-8-1", IncentiveType.TwentyFiveOrOverIncentive, 11, 110, 22, 220)]
        [TestCase("2021-3-31", IncentiveType.UnderTwentyFiveIncentive, 5, 50, 6, 60)]
        [TestCase("2021-3-31", IncentiveType.TwentyFiveOrOverIncentive, 55, 550, 66,660)]
        [TestCase("2021-6-1", IncentiveType.UnderTwentyFiveIncentive, 7, 70,8,80)]
        [TestCase("2021-6-1", IncentiveType.TwentyFiveOrOverIncentive, 77, 770,88, 880)]
        public void GetPaymentProfiles_returns_payment_profiles_based_on_training_start_date_and_incentive_type(
            DateTime trainingStartDate, IncentiveType incentiveType, int expectedFirstPaymentDays, decimal expectedFirstPayment,
            int expectedSecondPaymentDays, decimal expectedSecondPayment)
        {
            var actual = _sut.GetPaymentProfiles(incentiveType, trainingStartDate);

            actual[0].AmountPayable.Should().Be(expectedFirstPayment, $"{trainingStartDate:yyyy-M-d} {incentiveType}");
            actual[0].DaysAfterApprenticeshipStart.Should().Be(expectedFirstPaymentDays, $"{trainingStartDate:yyyy-M-d} {incentiveType}");
            actual[1].AmountPayable.Should().Be(expectedSecondPayment, $"{trainingStartDate:yyyy-M-d} {incentiveType}");
            actual[1].DaysAfterApprenticeshipStart.Should().Be(expectedSecondPaymentDays, $"{trainingStartDate:yyyy-M-d} {incentiveType}");
        }
    }
}
