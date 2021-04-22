using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.NewApprenticeIncentive.Handlers
{
    public class WhenHandlingGetApplicationQuery
    {
        private GetApplicationQueryHandler _sut;
        private Mock<IQueryRepository<IncentiveApplicationDto>> _applicationRepository;
        private Fixture _fixture;
        private Mock<IIncentivePaymentProfilesService> _paymentProfileService;
        private Mock<IQueryRepository<LegalEntityDto>> _legalEntityRepository;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _applicationRepository = new Mock<IQueryRepository<IncentiveApplicationDto>>();
            _legalEntityRepository = new Mock<IQueryRepository<LegalEntityDto>>();
            _paymentProfileService = new Mock<IIncentivePaymentProfilesService>();

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
                    new DateTime(2021,2,1),
                    new DateTime(2021,5,31),
                    paymentProfilesPhase1),
                new IncentivePaymentProfile(
                    IncentivePhase.Phase2_0,
                    6,
                    new DateTime(2021,6,1),
                    new DateTime(2021,11,30),
                    new DateTime(2021,4,1),
                    new DateTime(2021,5,31),
                    paymentProfilesPhase2),
            };

            var config = new IncentivesConfiguration(paymentProfiles);

            _paymentProfileService.Setup(x => x.Get()).ReturnsAsync(config);

            _sut = new GetApplicationQueryHandler(_applicationRepository.Object, _legalEntityRepository.Object, _paymentProfileService.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_data_repository()
        {
            //Arrange
            var query = _fixture.Create<GetApplicationRequest>();
            var apprenticeship = _fixture.Build<IncentiveApplicationApprenticeshipDto>().With(x => x.PlannedStartDate, new DateTime(2020, 9,1)).Create();
            var data = _fixture.Build<IncentiveApplicationDto>().With(x => x.Apprenticeships, new List<IncentiveApplicationApprenticeshipDto> { apprenticeship }).Create();
            var expected = new GetApplicationResponse(data);
            var legalEntity = _fixture.Create<LegalEntityDto>();

            _applicationRepository.Setup(x => x.Get(dto => dto.Id == query.ApplicationId && dto.AccountId == query.AccountId)).ReturnsAsync(data);
            _legalEntityRepository.Setup(x => x.Get(y => y.AccountLegalEntityId == data.AccountLegalEntityId)).ReturnsAsync(legalEntity);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expected, opts => opts.Excluding(x => x.Application.NewAgreementRequired));
        }

        [Test]
        public async Task Then_a_new_agreement_is_required_when_employer_has_not_signed_required_agreement_version()
        {
            //Arrange
            var query = _fixture.Create<GetApplicationRequest>();
            var apprenticeship = _fixture.Build<IncentiveApplicationApprenticeshipDto>().With(x => x.PlannedStartDate, new DateTime(2021, 2, 1)).Create();
            var data = _fixture.Build<IncentiveApplicationDto>().With(x => x.Apprenticeships, new List<IncentiveApplicationApprenticeshipDto> { apprenticeship }).Create();
            var legalEntity = _fixture.Build<LegalEntityDto>().With(x => x.SignedAgreementVersion, 4).Create();

            _applicationRepository.Setup(x => x.Get(dto => dto.Id == query.ApplicationId && dto.AccountId == query.AccountId)).ReturnsAsync(data);
            _legalEntityRepository.Setup(x => x.Get(y => y.AccountLegalEntityId == data.AccountLegalEntityId)).ReturnsAsync(legalEntity);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Application.NewAgreementRequired.Should().BeTrue();
        }

        [Test]
        public async Task Then_a_new_agreement_is_not_required_when_employer_has_signed_required_agreement_version()
        {
            //Arrange
            var query = _fixture.Create<GetApplicationRequest>();
            var apprenticeship = _fixture.Build<IncentiveApplicationApprenticeshipDto>().With(x => x.PlannedStartDate, new DateTime(2020, 9, 1)).Create();
            var data = _fixture.Build<IncentiveApplicationDto>().With(x => x.Apprenticeships, new List<IncentiveApplicationApprenticeshipDto> { apprenticeship }).Create();
            var legalEntity = _fixture.Build<LegalEntityDto>().With(x => x.SignedAgreementVersion, 5).Create();

            _applicationRepository.Setup(x => x.Get(dto => dto.Id == query.ApplicationId && dto.AccountId == query.AccountId)).ReturnsAsync(data);
            _legalEntityRepository.Setup(x => x.Get(y => y.AccountLegalEntityId == data.AccountLegalEntityId)).ReturnsAsync(legalEntity);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Application.NewAgreementRequired.Should().BeFalse();
        }
    }
}
