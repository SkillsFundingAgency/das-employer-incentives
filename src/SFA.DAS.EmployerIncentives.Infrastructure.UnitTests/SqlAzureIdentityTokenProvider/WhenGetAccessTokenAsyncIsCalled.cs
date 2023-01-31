using AutoFixture;
using Azure.Core;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Infrastructure.SqlAzureIdentityAuthentication;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Infrastructure.UnitTests.SqlAzureIdentityTokenProviderTests
{
    [TestFixture]
    public class WhenGetAccessTokenAsyncIsCalled
    {
        private SqlAzureIdentityTokenProvider _sut;
        private Mock<IAzureCredential> _mockAzureCredential;
        private Mock<TokenCredential> _mockTokenCredential;
        private IMemoryCache _memoryCache;

        private string _token;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _token = _fixture.Create<string>();

            _mockTokenCredential = new Mock<TokenCredential>();
            _memoryCache = new TestMemoryCache("SqlAzureIdentityAuthenticationKey", null);

            _mockTokenCredential
                .Setup(m => m.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccessToken(_token, new DateTimeOffset()));

            _mockAzureCredential = new Mock<IAzureCredential>();
            _mockAzureCredential.Setup(m => m.Get()).Returns(_mockTokenCredential.Object);

            _sut = new SqlAzureIdentityTokenProvider(_mockAzureCredential.Object, _memoryCache);
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
