using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.DataTransferObjects;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.NewApprenticeIncentive.Handlers
{
    public class WhenHandlingGetApplicationQuery
    {
        private GetApplicationQueryHandler _sut;
        private Mock<IIncentiveApplicationQueryRepository> _applicationRepository;
        private Mock<IQueryRepository<LegalEntity>> _legalEntityQueryRepository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _applicationRepository = new Mock<IIncentiveApplicationQueryRepository>();
            _legalEntityQueryRepository = new Mock<IQueryRepository<LegalEntity>>();

            _sut = new GetApplicationQueryHandler(_applicationRepository.Object, _legalEntityQueryRepository.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_data_repository()
        {
            //Arrange
            var query = _fixture.Create<GetApplicationRequest>();
            var apprenticeship = _fixture.Build<IncentiveApplicationApprenticeship>().With(x => x.Phase, Enums.Phase.Phase1).With(x => x.PlannedStartDate, new DateTime(2020, 9,1)).Create();
            var data = _fixture.Build<IncentiveApplication>().With(x => x.Apprenticeships, new List<IncentiveApplicationApprenticeship> { apprenticeship }).Create();
            var accountData = _fixture.Build<LegalEntity>().Create();
            var expected = new GetApplicationResponse(data);

            _applicationRepository.Setup(x => x.Get(dto => dto.Id == query.ApplicationId && dto.AccountId == query.AccountId)).ReturnsAsync(data);
            _legalEntityQueryRepository.Setup(x => x.Get(dto => dto.AccountLegalEntityId == data.AccountLegalEntityId)).ReturnsAsync(accountData);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }
    }
}
