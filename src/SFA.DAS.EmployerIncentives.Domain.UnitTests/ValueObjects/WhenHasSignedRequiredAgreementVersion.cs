using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ValueObjects
{
    [TestFixture]
    public class WhenHasSignedRequiredAgreementVersion
    {
        private List<IncentivePaymentProfile> _incentivePaymentProfiles;

        [SetUp]
        public void SetUp()
        {
            _incentivePaymentProfiles = new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile(IncentiveType.TwentyFiveOrOverIncentive,
                    new List<PaymentProfile>
                        {new PaymentProfile(90, 1000), new PaymentProfile(365, 1000)}),

                new IncentivePaymentProfile(IncentiveType.UnderTwentyFiveIncentive,
                    new List<PaymentProfile>
                        {new PaymentProfile(90, 1200), new PaymentProfile(365, 1200)})
            };
        }

        [TestCase("2020-07-31")]
        [TestCase("2021-06-01")]
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
