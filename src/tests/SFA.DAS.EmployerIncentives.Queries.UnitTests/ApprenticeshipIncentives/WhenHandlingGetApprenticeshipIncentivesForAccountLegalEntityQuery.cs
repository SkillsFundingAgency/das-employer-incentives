﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentivesForAccountLegalEntity;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.ApprenticeshipIncentives
{
    public class WhenHandlingGetApprenticeshipIncentivesForAccountLegalEntityQuery
    {
        private GetApprenticeshipIncentivesForAccountLegalEntityQueryHandler _sut;
        private Mock<IApprenticeshipIncentiveQueryRepository> _repositoryMock;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _repositoryMock = new Mock<IApprenticeshipIncentiveQueryRepository>();
            _sut = new GetApprenticeshipIncentivesForAccountLegalEntityQueryHandler(_repositoryMock.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_query_repository()
        {
            //Arrange
            var query = _fixture.Create<GetApprenticeshipIncentivesForAccountLegalEntityRequest>();
            var data = _fixture.CreateMany<ApprenticeshipIncentive>().ToList();
            var expected = new GetApprenticeshipIncentivesResponse(data);

            _repositoryMock.Setup(x => x.GetDtoList(q => q.AccountId == query.AccountId && q.AccountLegalEntityId == query.AccountLegalEntityId && (query.IncludeWithdrawn ? q.Status == Enums.IncentiveStatus.Withdrawn : q.Status != Enums.IncentiveStatus.Withdrawn))).ReturnsAsync(data);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expected, options => options.Excluding(o => o.Log));
        }

    }
}
