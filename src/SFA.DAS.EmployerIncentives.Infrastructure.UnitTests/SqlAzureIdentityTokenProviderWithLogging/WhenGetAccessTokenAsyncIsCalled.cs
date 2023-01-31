using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Infrastructure.SqlAzureIdentityAuthentication;
using SFA.DAS.EmployerIncentives.UnitTests.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Infrastructure.UnitTests.SqlAzureIdentityTokenProviderWithLoggingTests
{
    [TestFixture]
    public class WhenGetAccessTokenAsyncIsCalled
    {
        private SqlAzureIdentityTokenProviderWithLogging _sut;
        private Mock<ILogger<SqlAzureIdentityTokenProvider>> _mockLogger;
        private Mock<ISqlAzureIdentityTokenProvider> _mockSqlAzureIdentityTokenProvider;

        private string _token;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _token = _fixture.Create<string>();

            _mockLogger = new Mock<ILogger<SqlAzureIdentityTokenProvider>>();
            _mockSqlAzureIdentityTokenProvider = new Mock<ISqlAzureIdentityTokenProvider>();

            _mockSqlAzureIdentityTokenProvider.Setup(
                m => m.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_token);

            _sut = new SqlAzureIdentityTokenProviderWithLogging(_mockLogger.Object, _mockSqlAzureIdentityTokenProvider.Object);
        }

        [Test]
        public async Task Then_the_call_is_logged()
        {
            //Arrange
           
            //Act
            await _sut.GetAccessTokenAsync();

            //Assert
            _mockLogger.VerifyLog(LogLevel.Information, Times.Once(), $"Generated SQL AccessToken");
        }

        [Test]
        public async Task Then_the_SqlAzureIdentityTokenProvider_is_called()
        {
            //Arrange

            //Act
            await _sut.GetAccessTokenAsync();

            //Assert
            _mockSqlAzureIdentityTokenProvider.Verify(m => m.GetAccessTokenAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Then_the_token_is_returned()
        {
            //Arrange

            //Act
            var result = await _sut.GetAccessTokenAsync();

            //Assert
            result.Should().Be(_token);
        }
    }
}
