using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Queries.Account.GetApplications;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.Account.Handlers
{
    [TestFixture]
    public class WhenHandlingGetApplicationsQuery
    {
        private GetApplicationsQueryHandler _sut;
        private Mock<IApprenticeApplicationDataRepository> _repository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _repository = new Mock<IApprenticeApplicationDataRepository>();
            _sut = new GetApplicationsQueryHandler(_repository.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_data_repository()
        {
            //Arrange
            var query = _fixture.Create<GetApplicationsRequest>();
            var applicationsList = _fixture.CreateMany<ApprenticeApplicationDto>().ToList();
            var expectedResponse = new GetApplicationsResponse
            {
                ApprenticeApplications = applicationsList
            };

            _repository.Setup(x => x.GetList(query.AccountId)).ReturnsAsync(applicationsList);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
