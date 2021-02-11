using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.ApprenticeshipIncentiveHasPossibleChangeOrCircs;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.ApprenticeshipIncentives
{
    public class WhenHandlingApprenticeshipIncentiveHasPossibleChangeOrCircsQuery
    {
        private ApprenticeshipIncentiveHasPossibleChangeOrCircsQueryHandler _sut;
        private Mock<IApprenticeshipIncentiveQueryRepository> _repositoryMock;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _repositoryMock = new Mock<IApprenticeshipIncentiveQueryRepository>();
            _sut = new ApprenticeshipIncentiveHasPossibleChangeOrCircsQueryHandler(_repositoryMock.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_query_repository()
        {
            //Arrange
            var query = _fixture.Create<ApprenticeshipIncentiveHasPossibleChangeOrCircsRequest>();
            var data = _fixture.Create<Data.ApprenticeshipIncentives.Models.ApprenticeshipIncentive>();
            var expected = new ApprenticeshipIncentiveHasPossibleChangeOrCircsResponse(data.HasPossibleChangeOfCircumstances);

            _repositoryMock.Setup(x => x.Get(x => x.Id == query.ApprenticeshipIncentiveId, false)).ReturnsAsync(data);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }

    }
}
