using AutoFixture;
using Azure.Core;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Infrastructure.SqlAzureIdentityAuthentication;
using System;
using System.Threading;

namespace SFA.DAS.EmployerIncentives.Infrastructure.UnitTests.SqlAzureIdentityTokenProviderTests
{
    [TestFixture]
    public class WhenGetAccessTokenIsCalled
    {
        private SqlAzureIdentityTokenProvider _sut;
        private Mock<IAzureCredential> _mockAzureCredential;
        private Mock<TokenCredential> _mockTokenCredential;

        private string _token;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _token = _fixture.Create<string>();
            
            _mockTokenCredential = new Mock<TokenCredential>();

            _mockTokenCredential
                .Setup(m => m.GetToken(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
                .Returns(new AccessToken(_token, new DateTimeOffset()));

            _mockAzureCredential = new Mock<IAzureCredential>();
            _mockAzureCredential.Setup(m => m.Get()).Returns(_mockTokenCredential.Object);

            _sut = new SqlAzureIdentityTokenProvider(_mockAzureCredential.Object);
        }

        [Test]
        public void Then_the_token_is_returned()
        {
            //Arrange

            //Act
            var result = _sut.GetAccessToken();

            //Assert
            result.Should().Be(_token);
        }
    }
}
