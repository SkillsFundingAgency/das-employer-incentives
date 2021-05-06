using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.Builders;
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
        private Mock<IQueryRepository<LegalEntityDto>> _legalEntityRepository;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _applicationRepository = new Mock<IQueryRepository<IncentiveApplicationDto>>();
            _legalEntityRepository = new Mock<IQueryRepository<LegalEntityDto>>();

            _sut = new GetApplicationQueryHandler(_applicationRepository.Object, _legalEntityRepository.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_data_repository()
        {
            //Arrange
            var query = _fixture.Create<GetApplicationRequest>();
            var apprenticeship = _fixture.Build<IncentiveApplicationApprenticeshipDto>().With(x => x.Phase, Enums.Phase.Phase1_0).With(x => x.PlannedStartDate, new DateTime(2020, 9,1)).Create();
            var data = _fixture.Build<IncentiveApplicationDto>().With(x => x.Apprenticeships, new List<IncentiveApplicationApprenticeshipDto> { apprenticeship }).Create();
            var expected = new GetApplicationResponse(data);
            var legalEntity = _fixture.Build<LegalEntityDto>().With(x => x.AccountLegalEntityId, data.AccountLegalEntityId).Create();

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
            var apprenticeship = _fixture.Build<IncentiveApplicationApprenticeshipDto>().With(x => x.Phase, Enums.Phase.Phase1_0).With(x => x.PlannedStartDate, new DateTime(2021, 2, 1)).Create();
            var data = _fixture.Build<IncentiveApplicationDto>().With(x => x.Apprenticeships, new List<IncentiveApplicationApprenticeshipDto> { apprenticeship }).Create();
            var legalEntity = _fixture.Build<LegalEntityDto>().With(x => x.AccountLegalEntityId, data.AccountLegalEntityId).With(x => x.SignedAgreementVersion, 4).Create();

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
            var apprenticeship = _fixture.Build<IncentiveApplicationApprenticeshipDto>().With(x => x.Phase, Enums.Phase.Phase1_0).With(x => x.PlannedStartDate, new DateTime(2020, 9, 1)).Create();
            var data = _fixture.Build<IncentiveApplicationDto>().With(x => x.Apprenticeships, new List<IncentiveApplicationApprenticeshipDto> { apprenticeship }).Create();
            var legalEntity = _fixture.Build<LegalEntityDto>().With(x => x.AccountLegalEntityId, data.AccountLegalEntityId).With(x => x.SignedAgreementVersion, 5).Create();

            _applicationRepository.Setup(x => x.Get(dto => dto.Id == query.ApplicationId && dto.AccountId == query.AccountId)).ReturnsAsync(data);
            _legalEntityRepository.Setup(x => x.Get(y => y.AccountLegalEntityId == data.AccountLegalEntityId)).ReturnsAsync(legalEntity);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Application.NewAgreementRequired.Should().BeFalse();
        }
    }
}
