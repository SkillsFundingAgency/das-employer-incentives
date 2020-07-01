using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Queries.Account;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.Account.Handlers
{
    public class WhenHandlingGetLegalEntitiesQueryInvoked
    {
        private GetLegalEntitiesQueryHandler _sut;
        private Mock<IReadonlyRepository<Domain.Accounts.Account>> _repositoryMock;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _repositoryMock = new Mock<IReadonlyRepository<Domain.Accounts.Account>>();
            _sut = new GetLegalEntitiesQueryHandler(_repositoryMock.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_data_repository()
        {
            //Arrange
            var query = _fixture.Create<GetLegalEntitiesRequest>();
            var data = _fixture.CreateMany<Domain.Accounts.Account>().ToArray();
            var expected = new GetLegalEntitiesResponse
            {
                LegalEntities = data.ToLegalEntityDto()
            };

            _repositoryMock.Setup(x => x.GetList(x => x.Id == query.AccountId)).ReturnsAsync(data);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }

    }
}
