using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Queries.Account.GetAccountsWithVrfStatus;
using Moq;
using AutoFixture;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.DataTransferObjects;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.Account.Handlers
{
    [TestFixture]
    public class WhenHandlingGetAccountsByVrfCaseStatusQuery
    {
        private GetAccountsWithVrfCaseStatusQueryHandler _sut;
        private Mock<IAccountDataRepository> _repository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _repository = new Mock<IAccountDataRepository>();
            _sut = new GetAccountsWithVrfCaseStatusQueryHandler(_repository.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_data_repository()
        {
            //Arrange
            var query = _fixture.Create<GetAccountsWithVrfCaseStatusRequest>();
            var accountsList = _fixture.CreateMany<DataTransferObjects.Account>().ToList();
            var expectedResponse = new GetAccountsWithVrfCaseStatusResponse
            {
                Accounts = accountsList
            };

            _repository.Setup(x => x.GetByVrfCaseStatus(query.VrfStatus)).ReturnsAsync(accountsList);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
