using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

            _paymentProfileService.Setup(x => x.Get()).ReturnsAsync(new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile(IncentiveType.TwentyFiveOrOverIncentive,
                    new List<PaymentProfile>
                        {new PaymentProfile(90, 1000), new PaymentProfile(365, 1000)}),

                new IncentivePaymentProfile(IncentiveType.UnderTwentyFiveIncentive,
                    new List<PaymentProfile>
                        {new PaymentProfile(90, 1200), new PaymentProfile(365, 1200)})
            });

            _sut = new GetApplicationQueryHandler(_applicationRepository.Object, _legalEntityRepository.Object, _paymentProfileService.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_data_repository()
        {
            //Arrange
            var query = _fixture.Create<GetApplicationRequest>();
            var apprenticeship = _fixture.Build<IncentiveApplicationApprenticeshipDto>().With(x => x.PlannedStartDate, new DateTime(2020, 9,1)).Create();
            var data = _fixture.Build<IncentiveApplicationDto>().With(x => x.Apprenticeships, new List<IncentiveApplicationApprenticeshipDto> { apprenticeship }).Create();
            var expected = new GetApplicationResponse(data, false);
            var legalEntity = _fixture.Create<LegalEntityDto>();

            _applicationRepository.Setup(x => x.Get(dto => dto.Id == query.ApplicationId && dto.AccountId == query.AccountId)).ReturnsAsync(data);
            _legalEntityRepository.Setup(x => x.Get(y => y.AccountLegalEntityId == data.AccountLegalEntityId)).ReturnsAsync(legalEntity);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task Then_a_new_agreement_is_required_when_employer_has_not_signed_required_agreement_version()
        {
            //Arrange
            var query = _fixture.Create<GetApplicationRequest>();
            var apprenticeship = _fixture.Build<IncentiveApplicationApprenticeshipDto>().With(x => x.PlannedStartDate, new DateTime(2021, 2, 1)).Create();
            var data = _fixture.Build<IncentiveApplicationDto>().With(x => x.Apprenticeships, new List<IncentiveApplicationApprenticeshipDto> { apprenticeship }).Create();
            var expected = new GetApplicationResponse(data, false);
            var legalEntity = _fixture.Create<LegalEntityDto>();

            _applicationRepository.Setup(x => x.Get(dto => dto.Id == query.ApplicationId && dto.AccountId == query.AccountId)).ReturnsAsync(data);
            _legalEntityRepository.Setup(x => x.Get(y => y.AccountLegalEntityId == data.AccountLegalEntityId)).ReturnsAsync(legalEntity);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.NewAgreementRequired.Should().BeTrue();
        }

        [Test]
        public async Task Then_a_new_agreement_is_not_required_when_employer_has_signed_required_agreement_version()
        {
            //Arrange
            var query = _fixture.Create<GetApplicationRequest>();
            var apprenticeship = _fixture.Build<IncentiveApplicationApprenticeshipDto>().With(x => x.PlannedStartDate, new DateTime(2020, 9, 1)).Create();
            var data = _fixture.Build<IncentiveApplicationDto>().With(x => x.Apprenticeships, new List<IncentiveApplicationApprenticeshipDto> { apprenticeship }).Create();
            var expected = new GetApplicationResponse(data, false);
            var legalEntity = _fixture.Create<LegalEntityDto>();

            _applicationRepository.Setup(x => x.Get(dto => dto.Id == query.ApplicationId && dto.AccountId == query.AccountId)).ReturnsAsync(data);
            _legalEntityRepository.Setup(x => x.Get(y => y.AccountLegalEntityId == data.AccountLegalEntityId)).ReturnsAsync(legalEntity);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.NewAgreementRequired.Should().BeFalse();
        }
    }
}
