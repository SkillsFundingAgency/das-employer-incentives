using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntity;
using SFA.DAS.HashingService;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    [TestFixture]
    public class WhenGetLegalEntityByHashedId
    {
        private AccountQueryController _sut;
        private Mock<IQueryDispatcher> _queryDispatcherMock;
        private Fixture _fixture;
        private string _hashedLegalEntityId;

        [SetUp]
        public void Setup()
        {
            _queryDispatcherMock = new Mock<IQueryDispatcher>();
            _fixture = new Fixture();
            _hashedLegalEntityId = _fixture.Create<string>();
            
            _sut = new AccountQueryController(_queryDispatcherMock.Object);
        }

        [Test]
        public async Task Then_data_is_returned_for_existing_account()
        {
            // Arrange
            var expected = _fixture.Create<GetLegalEntityResponse>();
            _queryDispatcherMock.Setup(x => x.Send<GetLegalEntityByHashedIdRequest, GetLegalEntityResponse>(
                    It.Is<GetLegalEntityByHashedIdRequest>(r => r.HashedLegalEntityId == _hashedLegalEntityId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetEmployerVendorId(_hashedLegalEntityId) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be(expected);
        }

        [Test]
        public async Task Then_error_returned_for_non_existing_account()
        {
            // Arrange
            var expected = new GetLegalEntityResponse();
            _queryDispatcherMock.Setup(x => x.Send<GetLegalEntityByHashedIdRequest, GetLegalEntityResponse>(
                    It.Is<GetLegalEntityByHashedIdRequest>(r => r.HashedLegalEntityId == _hashedLegalEntityId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetEmployerVendorId(_hashedLegalEntityId) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be(expected);
        }
    }
}
