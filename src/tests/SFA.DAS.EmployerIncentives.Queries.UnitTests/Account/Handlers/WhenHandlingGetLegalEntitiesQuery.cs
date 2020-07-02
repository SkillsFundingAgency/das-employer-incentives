using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Queries.Account;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.Account.Handlers
{
    public class WhenHandlingGetLegalEntitiesQueryInvoked
    {
        private GetLegalEntitiesQueryHandler _sut;
        private Mock<IQueryRepository<LegalEntityDto>> _repositoryMock;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _repositoryMock = new Mock<IQueryRepository<LegalEntityDto>>();
            _sut = new GetLegalEntitiesQueryHandler(_repositoryMock.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_data_repository()
        {
            //Arrange
            var query = _fixture.Create<GetLegalEntitiesRequest>();
            var data = _fixture.CreateMany<LegalEntityDto>().ToList();
            var expected = new GetLegalEntitiesResponse
            {
                LegalEntities = data
            };

            _repositoryMock.Setup(x => x.GetList(dto => dto.AccountId == query.AccountId)).ReturnsAsync(data);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }

    }
}
