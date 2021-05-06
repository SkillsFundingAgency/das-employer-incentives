using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.Builders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ValueObjects
{
    [TestFixture]
    public class WhenHasSignedRequiredAgreementVersion
    {
        private Fixture _fixture;
        private Mock<IIncentivePaymentProfilesService> _mockIncentivePaymentProfileService;
        private List<IncentivePaymentProfile> _incentivePaymentProfiles;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _mockIncentivePaymentProfileService = new Mock<IIncentivePaymentProfilesService>();
            _incentivePaymentProfiles = new List<IncentivePaymentProfile>
            {
               new IncentivePaymentProfile(
                     new IncentivePhase(Phase.Phase1_0),
                        new List<PaymentProfile>
                            {
                                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, 90, 1000),
                                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, 365, 1000),
                                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, 90, 750),
                                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, 365, 750)
                            }),

                  new IncentivePaymentProfile(
                     new IncentivePhase(Phase.Phase1_1),
                        new List<PaymentProfile>
                            {
                                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, 90, 1000),
                                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, 365, 1000),
                                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, 90, 750),
                                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, 365, 750)
                            }),

                   new IncentivePaymentProfile(
                     new IncentivePhase(Phase.Phase2_0),
                        new List<PaymentProfile>
                            {
                                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, 90, 1500),
                                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, 365, 1500),
                                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, 90, 1500),
                                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, 365, 1500)
                            })
            };

            _mockIncentivePaymentProfileService.Setup(m => m.Get()).ReturnsAsync(_incentivePaymentProfiles);
        }

        [TestCase("2020-07-31")]
        [TestCase("2021-06-01")]
        public void Then_returns_true_when_ineligible(DateTime startDate)
        {
            var apprenticeshipIncentive = _fixture.Build<IncentiveApplicationApprenticeshipDto>()
               .With(p => p.PlannedStartDate, startDate)
               .Create();

            var application = _fixture.Build<IncentiveApplicationDto>()
                .With(p => p.Apprenticeships, new List<IncentiveApplicationApprenticeshipDto> { apprenticeshipIncentive })
                .Create();

            var legalEntityDto = _fixture.Build<LegalEntityDto>()
                .With(p => p.SignedAgreementVersion, 10)
                .With(p => p.AccountLegalEntityId, application.AccountLegalEntityId)
                .Create();

            var result = Incentive.IsNewAgreementRequired(application, legalEntityDto);

            result.Should().BeTrue();
        }

        [TestCase("2020-09-01", 3)]
        [TestCase("2021-02-05", 4)]
        public void Then_returns_true_when_incorrect_agreement_signed(DateTime startDate, int signedAgreementVersion)
        {
            var apprenticeshipIncentive = _fixture.Build<IncentiveApplicationApprenticeshipDto>()
              .With(p => p.PlannedStartDate, startDate)
              .Create();

            var application = _fixture.Build<IncentiveApplicationDto>()
                .With(p => p.Apprenticeships, new List<IncentiveApplicationApprenticeshipDto> { apprenticeshipIncentive })
                .Create();

            var legalEntityDto = _fixture.Build<LegalEntityDto>()
                .With(p => p.SignedAgreementVersion, signedAgreementVersion)
                .With(p => p.AccountLegalEntityId, application.AccountLegalEntityId)
                .Create();

            var result = Incentive.IsNewAgreementRequired(application, legalEntityDto);

            result.Should().BeTrue();
        }

        [TestCase("2020-09-01", 4)]
        [TestCase("2021-02-05", 5)]
        [TestCase("2021-02-05", 6)]
        public void Then_returns_false_when_correct_agreement_signed(DateTime startDate, int signedAgreementVersion)
        {
            var apprenticeshipIncentive = _fixture.Build<IncentiveApplicationApprenticeshipDto>()
              .With(p => p.PlannedStartDate, startDate)
              .Create();

            var application = _fixture.Build<IncentiveApplicationDto>()
                .With(p => p.Apprenticeships, new List<IncentiveApplicationApprenticeshipDto> { apprenticeshipIncentive })
                .Create();

            var legalEntityDto = _fixture.Build<LegalEntityDto>()
                .With(p => p.SignedAgreementVersion, signedAgreementVersion)
                .With(p => p.AccountLegalEntityId, application.AccountLegalEntityId)
                .Create();

            var result = Incentive.IsNewAgreementRequired(application, legalEntityDto);

            result.Should().BeFalse();
        }

        [Test]
        public void Then_throws_ArgumentException_when_legalentity_not_related_to_the_application()
        {
            var apprenticeshipIncentive = _fixture.Build<IncentiveApplicationApprenticeshipDto>()
              .Create();

            var application = _fixture.Build<IncentiveApplicationDto>()
                .With(p => p.Apprenticeships, new List<IncentiveApplicationApprenticeshipDto> { apprenticeshipIncentive })
                .Create();

            var legalEntityDto = _fixture.Build<LegalEntityDto>()
                .With(p => p.AccountLegalEntityId, application.AccountLegalEntityId + 1)
                .Create();

            Action action = () => Incentive.IsNewAgreementRequired(application, legalEntityDto);

            action.Should().Throw<Exception>().WithMessage($"Legal entity {legalEntityDto.AccountLegalEntityId} is not related to the application {application.AccountLegalEntityId} when checking IsNewAgreementRequired");
        }
    }
}
