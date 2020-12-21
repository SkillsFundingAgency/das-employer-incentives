using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Queries.Account.GetVendorId;
using SFA.DAS.HashingService;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    [TestFixture]
    public class WhenGetVendorIdForAnAccount
    {
        private AccountQueryController _sut;
        private Mock<IQueryDispatcher> _queryDispatcherMock;
        private Mock<IHashingService> _hashingServiceMock;
        private Fixture _fixture;
        private string _hashedLegalEntityId;
        private long _unhashedLegalEntityId;

        [SetUp]
        public void Setup()
        {
            _queryDispatcherMock = new Mock<IQueryDispatcher>();
            _hashingServiceMock = new Mock<IHashingService>();
            _fixture = new Fixture();
            _hashedLegalEntityId = _fixture.Create<string>();
            _unhashedLegalEntityId = _fixture.Create<long>();
            _hashingServiceMock.Setup(x => x.DecodeValue(_hashedLegalEntityId)).Returns(_unhashedLegalEntityId);
            
            _sut = new AccountQueryController(_queryDispatcherMock.Object, _hashingServiceMock.Object);
        }

        [Test]
        public async Task Then_data_is_returned_for_existing_account()
        {
            // Arrange
            var expected = new GetVendorIdResponse { VendorId = _fixture.Create<string>() };
            _queryDispatcherMock.Setup(x => x.Send<GetVendorIdRequest, GetVendorIdResponse>(
                    It.Is<GetVendorIdRequest>(r => r.LegalEntityId == _unhashedLegalEntityId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetEmployerVendorId(_hashedLegalEntityId) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be(expected.VendorId);
        }

        [Test]
        public async Task Then_error_returned_for_non_existing_account()
        {
            // Arrange
            var expected = new GetVendorIdResponse();
            _queryDispatcherMock.Setup(x => x.Send<GetVendorIdRequest, GetVendorIdResponse>(
                    It.Is<GetVendorIdRequest>(r => r.LegalEntityId == _unhashedLegalEntityId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetEmployerVendorId(_hashedLegalEntityId) as NotFoundResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}
