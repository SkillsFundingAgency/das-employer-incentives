using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPayableLegalEntities;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.ApprenticeshipIncentives
{
    public class WhenHandlingGetPayableLegalEntitiesQuery
    {
        private GetPayableLegalEntitiesQueryHandler _sut;
        private Mock<IPaymentsQueryRepository> _repositoryMock;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _repositoryMock = new Mock<IPaymentsQueryRepository>();
            _sut = new GetPayableLegalEntitiesQueryHandler(_repositoryMock.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_query_repository()
        {
            //Arrange
            var query = _fixture.Create<GetPayableLegalEntitiesRequest>();
            var data = _fixture.CreateMany<PayableLegalEntity>().ToList();
            var expected = new GetPayableLegalEntitiesResponse(data);

            _repositoryMock.Setup(x => x.GetPayableLegalEntities(query.CollectionPeriod.AcademicYear, query.CollectionPeriod.PeriodNumber)).ReturnsAsync(data);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expected, options => options.Excluding(o => o.Log));
        }
    }
}
