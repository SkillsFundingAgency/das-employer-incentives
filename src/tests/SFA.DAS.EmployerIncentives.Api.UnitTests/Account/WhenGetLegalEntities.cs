using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Queries.Account;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    public class WhenGetLegalEntities
    {
        private AccountQueryController _sut;
        private Mock<IQueryDispatcher> _queryDispatcherMock;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _queryDispatcherMock = new Mock<IQueryDispatcher>();
            _fixture = new Fixture();
            _sut = new AccountQueryController(_queryDispatcherMock.Object);
        }

        [Test]
        public async Task Then_data_is_returned_for_existing_account()
        {
            // Arrange
            const long accountId = 7;
            var expected = new GetLegalEntitiesResponse { LegalEntities = _fixture.CreateMany<LegalEntityDto>() };

            _queryDispatcherMock.Setup(x => x.Send<GetLegalEntitiesRequest, GetLegalEntitiesResponse>(
                    It.Is<GetLegalEntitiesRequest>(r => r.AccountId == accountId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetLegalEntities(accountId);

            // Assert
            actual.Should().Be(expected);
        }

        [Test]
        public void Then_error_returned_for_non_existing_account()
        {
            // Arrange
            const long accountId = 7;
            var expected = new GetLegalEntitiesResponse();

            _queryDispatcherMock.Setup(x => x.Send<GetLegalEntitiesRequest, GetLegalEntitiesResponse>(
                    It.Is<GetLegalEntitiesRequest>(r => r.AccountId == accountId)))
                .ReturnsAsync(expected);

            // Act
             Action act = () =>  _sut.GetLegalEntities(accountId);

            // Assert
            act.Should().Throw<HttpResponseException>();//.Where(x => x.Response.StatusCode == HttpStatusCode.NotFound);
        }

    }
}