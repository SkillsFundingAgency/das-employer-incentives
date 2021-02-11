using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLatestVendorRegistrationCaseUpdateDateTime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.Account.Handlers
{
    [TestFixture]
    public class WhenHandlingGetLatestVendorRegistrationCaseUpdateDateTimeQuery
    {
        private GetLatestVendorRegistrationCaseUpdateDateTimeQueryHandler _sut;
        private Mock<IAccountDataRepository> _repository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _repository = new Mock<IAccountDataRepository>();
            _sut = new GetLatestVendorRegistrationCaseUpdateDateTimeQueryHandler(_repository.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_data_repository()
        {
            //Arrange
            var query = _fixture.Create<GetLatestVendorRegistrationCaseUpdateDateTimeRequest>();
            var expected = _fixture.Create<DateTime>();
            var expectedResponse = new GetLatestVendorRegistrationCaseUpdateDateTimeResponse { LastUpdateDateTime = expected };

            _repository.Setup(x => x.GetLatestVendorRegistrationCaseUpdateDateTime()).ReturnsAsync(expected);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
