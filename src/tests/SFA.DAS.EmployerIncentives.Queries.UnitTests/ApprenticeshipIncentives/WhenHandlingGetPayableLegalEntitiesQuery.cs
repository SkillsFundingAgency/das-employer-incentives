using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPayableLegalEntities;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.ApprenticeshipIncentives
{
    public class WhenHandlingGetPayableLegalEntitiesQuery
    {
        private GetPayableLegalEntitiesQueryHandler _sut;
        private Mock<IPayableLegalEntityQueryRepository> _repositoryMock;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _repositoryMock = new Mock<IPayableLegalEntityQueryRepository>();
            _sut = new GetPayableLegalEntitiesQueryHandler(_repositoryMock.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_query_repository()
        {
            //Arrange
            var query = _fixture.Create<GetPayableLegalEntitiesRequest>();
            var data = _fixture.CreateMany<PayableLegalEntityDto>().ToList();
            var expected = new GetPayableLegalEntitiesResponse(data);

            _repositoryMock.Setup(x => x.GetList(query.CollectionPeriodYear, query.CollectionPeriodMonth)).ReturnsAsync(data);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }

    }
}
