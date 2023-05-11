using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services.SlackApi;
using SFA.DAS.EmployerIncentives.Commands.Types.Notification;
using SFA.DAS.EmployerIncentives.UnitTests.Shared;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services.SlackApiTests
{
    public class WhenSend
    {
        private SlackNotificationService _sut;
        private TestHttpClient _httpClient;
        private string _webhookUrl;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _httpClient = new TestHttpClient();
            _webhookUrl = $"{_httpClient.BaseAddress.AbsoluteUri}webhook";
            
            _sut = new SlackNotificationService(_httpClient, _webhookUrl);
        }

        [TestCase(HttpStatusCode.Accepted)]
        [TestCase(HttpStatusCode.OK)]
        public async Task Then_the_message_is_posted_successfully(HttpStatusCode statusCode)
        {
            //Arrange
            _httpClient.SetUpPostAsAsync(statusCode);
            var message = _fixture.Create<SlackMessage>();

            //Act
            await _sut.Send(message);

            // Assert
            _httpClient.VerifyPostAsAsync($"webhook", Times.Once());
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        public void Then_an_error_is_raised_when_the_slack_api_returns_an_invalid_status_code(HttpStatusCode statusCode)
        {
            //Arrange
            _httpClient.SetUpPostAsAsync(statusCode);
            var payment = _fixture.Create<SlackMessage>();

            //Act
            Func<Task> act = async () => await _sut.Send(payment);

            // Assert
            act.Should().Throw<HttpRequestException>();
        }
    }
}
