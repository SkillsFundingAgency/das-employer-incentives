using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.UnitTests.Builders.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.Services.NewApprenticeIncentiveEligibilityService
{
    [TestFixture]
    public class WhenEligibleApprenticeshipsAreRequested
    {
        private Mock<IUlnValidationService> _ulnValidationServiceMock;
        private Domain.Services.NewApprenticeIncentiveEligibilityService _sut;

        [SetUp]
        public void SetUp()
        {
            _ulnValidationServiceMock = new Mock<IUlnValidationService>();
            var paymentProfilesPhase1 = new List<PaymentProfile>
            {
                new PaymentProfile(90, 100, IncentiveType.UnderTwentyFiveIncentive, EarningType.FirstPayment),
                new PaymentProfile(365, 300, IncentiveType.UnderTwentyFiveIncentive, EarningType.SecondPayment),
                new PaymentProfile(90, 200, IncentiveType.TwentyFiveOrOverIncentive, EarningType.FirstPayment),
                new PaymentProfile(365, 400, IncentiveType.TwentyFiveOrOverIncentive, EarningType.SecondPayment),
            };

            var paymentProfiles = new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile(
                    4,
                    new DateTime(2020, 8, 1),
                    new DateTime(2021, 5, 31),
                    new DateTime(2020, 8, 1),
                    new DateTime(2021, 1, 31),
                    paymentProfilesPhase1)
            };

            var mockPaymentProfilesService = new Mock<IIncentivePaymentProfilesService>();
            mockPaymentProfilesService.Setup(m => m.Get()).ReturnsAsync(paymentProfiles);

            _sut = new Domain.Services.NewApprenticeIncentiveEligibilityService(_ulnValidationServiceMock.Object);
        }

        [Test]
        public async Task Then_an_unapproved_apprenticeship_returns_false()
        {
            var apprenticeship = new ApprenticeshipBuilder()
                .WithIsApproved(false)
                .Build();

            var result = await _sut.IsApprenticeshipEligible(apprenticeship);

            result.Should().BeFalse();
        }

        [Test]
        public async Task Then_false_is_returned_when_the_apprenticeship_start_date_is_after_the_scheme_cut_off()
        {
            var apprenticeship = new ApprenticeshipBuilder()
                .WithStartDate(new DateTime(2021, 7, 1))
                .Build();

            var result = await _sut.IsApprenticeshipEligible(apprenticeship);

            result.Should().BeFalse();
        }

        [Test]
        public async Task Then_false_is_returned_when_the_apprenticeship_start_date_is_before_the_scheme_start_date()
        {
            var apprenticeship = new ApprenticeshipBuilder()
                .WithStartDate(new DateTime(2020, 7, 31))
                .Build();

            var result = await _sut.IsApprenticeshipEligible(apprenticeship);

            result.Should().BeFalse();
        }

        [Test]
        public async Task Then_an_eligible_apprenticeship_where_uln_not_applied_previously_returns_true()
        {
            var apprenticeship = new ApprenticeshipBuilder().WithValidIncentiveProperties().Build();

            var result = await _sut.IsApprenticeshipEligible(apprenticeship);

            result.Should().BeTrue();
        }

        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        public async Task Then_an_eligible_apprenticeship_returns_expected_result(bool isApproved, bool ulnPreviouslyApplied, bool expected)
        {
            _ulnValidationServiceMock.Setup(x => x.UlnAlreadyOnSubmittedIncentiveApplication(It.IsAny<long>()))
                .ReturnsAsync(ulnPreviouslyApplied);

            var apprenticeship = new ApprenticeshipBuilder().WithValidIncentiveProperties().WithIsApproved(isApproved).Build();

            var result = await _sut.IsApprenticeshipEligible(apprenticeship);

            result.Should().Be(expected);
        }
    }
}
