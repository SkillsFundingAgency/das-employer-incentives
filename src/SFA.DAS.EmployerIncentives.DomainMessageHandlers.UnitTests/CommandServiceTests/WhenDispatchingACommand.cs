using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers;
using System;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.UnitTests.Shared;
using System.Text.Json;
using System.Net;
using Newtonsoft.Json;

namespace SFA.DAS.EmployerIncentives.DomainMessageHandlers.UnitTests.CommandServiceTests
{
    [TestFixture]
    public class WhenDispatchingACommand
    {
        private CommandService _sut;
        private TestHttpClient _client;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _client = new TestHttpClient();
            _client.SetUpPostAsAsync(HttpStatusCode.OK);
            _sut = new CommandService(_client);
        }

        [Test]
        public void Then_an_exception_is_thrown_when_the_command_type_is_not_valid()
        {
            // arrange
            var invalidCommand = new InvalidCommand();

            // act
            Func<Task> result = async () => await _sut.Dispatch(invalidCommand);

            // assert
            result.Should().Throw<ArgumentException>().WithMessage($"Invalid command type {invalidCommand.GetType().FullName}");
        }

        [Test]
        public async Task Then_the_command_is_dispatched_when_the_command_is_valid()
        {
            // arrange
            var validCommand = _fixture.Create<CreateIncentiveCommand>();

            // act
            await _sut.Dispatch(validCommand);

            // assert
            _client.VerifyPostAsAsync($"commands/ApprenticeshipIncentive.CreateIncentiveCommand", JsonConvert.SerializeObject(validCommand, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), Times.Once());
        }

        [Test]
        public void Then_an_exception_is_thrown_when_the_call_to_the_api_fails()
        {
            // arrange
            var validCommand = _fixture.Create<CreateIncentiveCommand>();

            _client.SetUpPostAsAsync(HttpStatusCode.InternalServerError);

            // act
            Func<Task> result = async () => await _sut.Dispatch(validCommand);

            // assert
            result.Should().Throw<HttpRequestException>().WithMessage("Response status code does not indicate success: 500 (Internal Server Error).");
        }

        public class InvalidCommand : DomainCommand
        {

        }
    }
}
