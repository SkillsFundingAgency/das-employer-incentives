using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ValueObjects
{
    [TestFixture]
    public class WhenHasSignedRequiredAgreementVersion
    {
        private List<IncentivePaymentProfile> _incentivePaymentProfiles;

        [SetUp]
        public void SetUp()
        {
            var paymentProfilesPhase1 = new List<PaymentProfile>
            {
                new PaymentProfile(90, 100, IncentiveType.UnderTwentyFiveIncentive, EarningType.FirstPayment),
                new PaymentProfile(365, 300, IncentiveType.UnderTwentyFiveIncentive, EarningType.SecondPayment),
                new PaymentProfile(90, 200, IncentiveType.TwentyFiveOrOverIncentive, EarningType.FirstPayment),
                new PaymentProfile(365, 400, IncentiveType.TwentyFiveOrOverIncentive, EarningType.SecondPayment),
            };

            var paymentProfilesPhase2 = new List<PaymentProfile>
            {
                new PaymentProfile(90, 100, IncentiveType.UnderTwentyFiveIncentive, EarningType.FirstPayment),
                new PaymentProfile(365, 300, IncentiveType.UnderTwentyFiveIncentive, EarningType.SecondPayment),
                new PaymentProfile(90, 200, IncentiveType.TwentyFiveOrOverIncentive, EarningType.FirstPayment),
                new PaymentProfile(365, 400, IncentiveType.TwentyFiveOrOverIncentive, EarningType.SecondPayment),
            };

            _incentivePaymentProfiles = new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile(
                    4,
                    new DateTime(2020,8,1),
                    new DateTime(2021,5,31),
                    new DateTime(2020,8,1),
                    new DateTime(2021,1,31),
                    paymentProfilesPhase1),
                new IncentivePaymentProfile(
                    5,
                    new DateTime(2020,8,1),
                    new DateTime(2021,5,31),
                    new DateTime(2021,2,1),
                    new DateTime(2021,5,31),
                    paymentProfilesPhase1),
                new IncentivePaymentProfile(
                    6,
                    new DateTime(2021,6,1),
                    new DateTime(2021,11,30),
                    new DateTime(2021,4,1),
                    new DateTime(2021,5,31),
                    paymentProfilesPhase2),
            };
        }

        [TestCase("2020-07-31")]
        [TestCase("2021-12-01")]
        public void Then_returns_true_when_ineligible(DateTime startDate)
        {
            var incentive = new Incentive(DateTime.Now.AddYears(-20), startDate, _incentivePaymentProfiles, 0);

            var result = incentive.IsNewAgreementRequired(10);

            result.Should().BeTrue();
        }

        [TestCase("2020-09-01", 3)]
        [TestCase("2021-02-05", 4)]
        public void Then_returns_true_when_incorrect_agreement_signed(DateTime startDate, int signedAgreementVersion)
        {
            var incentive = new Incentive(DateTime.Now.AddYears(-20), startDate, _incentivePaymentProfiles, 0);

            var result = incentive.IsNewAgreementRequired(signedAgreementVersion);

            result.Should().BeTrue();
        }

        [TestCase("2020-09-01", 4)]
        [TestCase("2021-02-05", 5)]
        [TestCase("2021-02-05", 6)]
        public void Then_returns_false_when_correct_agreement_signed(DateTime startDate, int signedAgreementVersion)
        {
            var incentive = new Incentive(DateTime.Now.AddYears(-20), startDate, _incentivePaymentProfiles, 0);

            var result = incentive.IsNewAgreementRequired(signedAgreementVersion);

            result.Should().BeFalse();
        }
    }
}
