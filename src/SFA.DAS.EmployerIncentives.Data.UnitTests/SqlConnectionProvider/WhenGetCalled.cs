using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Infrastructure.SqlAzureIdentityAuthentication;
using System.Threading;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.SqlConnectionProviderTests
{
    public class WhenGetCalled
    {
        private SqlConnectionProvider _sut;
        private Fixture _fixture;

        private string _token;
        private Mock<ISqlAzureIdentityTokenProvider> _sqlAzureIdentityTokenProvider;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _token = _fixture.Create<string>();

            _sqlAzureIdentityTokenProvider = new Mock<ISqlAzureIdentityTokenProvider>();

            _sqlAzureIdentityTokenProvider
                .Setup(m => m.GetAccessToken(It.IsAny<CancellationToken>()))
                .Returns(_token);

            _sut = new SqlConnectionProvider(_sqlAzureIdentityTokenProvider.Object);
        }

        [TestCase("Integrated Security=True")]
        [TestCase("Integrated Security=true")]
        [TestCase("Integrated Security=  true")]
        [TestCase("Integrated Security=SSPI")]
        [TestCase("Integrated Security=SSpI")]
        [TestCase("Integrated Security= SSPI")]
        public void Then_the_token_provider_is_not_called_when_the_connection_string_is_integrated_security(string integratedSecurity)
        {
            // Arrange
            var connectionString = $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=model;{integratedSecurity};MultipleActiveResultSets=True;Pooling=False;Connect Timeout=30;";

            // Act
            var result = _sut.Get(connectionString);

            // Assert
            result.AccessToken.Should().BeNull();
            result.ConnectionString.Should().Be(connectionString);
            _sqlAzureIdentityTokenProvider.Verify(m => m.GetAccessToken(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void Then_the_token_provider_is_called_when_the_connection_string_is_not_integrated_security()
        {
            // Arrange
            var connectionString = $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=model;MultipleActiveResultSets=True;Pooling=False;Connect Timeout=30;";

            // Act
            var result = _sut.Get(connectionString);

            // Assert
            result.AccessToken.Should().Be(_token);
            result.ConnectionString.Should().Be(connectionString);
            _sqlAzureIdentityTokenProvider.Verify(m => m.GetAccessToken(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
