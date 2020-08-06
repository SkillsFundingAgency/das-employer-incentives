using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntity;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.Account.Handlers
{
    public class WhenHandlingGetLegalEntityQueryInvoked
    {
        private GetLegalEntityQueryHandler _sut;
        private Mock<IQueryRepository<LegalEntityDto>> _repositoryMock;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _repositoryMock = new Mock<IQueryRepository<LegalEntityDto>>();
            _sut = new GetLegalEntityQueryHandler(_repositoryMock.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_data_repository()
        {
            //Arrange
            var query = _fixture.Create<GetLegalEntityRequest>();
            var data = _fixture.Create<LegalEntityDto>();
            var expected = new GetLegalEntityResponse
            {
                LegalEntity = data
            };

            _repositoryMock.Setup(x => x.Get(dto => dto.AccountId == query.AccountId && dto.AccountLegalEntityId == query.AccountLegalEntityId)).ReturnsAsync(data);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }

    }
}
