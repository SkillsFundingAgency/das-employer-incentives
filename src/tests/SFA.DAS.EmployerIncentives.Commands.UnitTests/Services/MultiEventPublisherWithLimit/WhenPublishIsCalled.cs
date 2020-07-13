using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.NServiceBus.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.Commands.Services
{
    public class WhenPublishIsCalled
    {
        private MultiEventPublisherWithLimit _sut;
        private Mock<IEventPublisher> _mockEventPublisher;
        private Mock<IOptions<PolicySettings>> _mockPolicySettings;
        private Policies _policies;

        private Fixture _fixture;

        public class TestEvent { }

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockEventPublisher = new Mock<IEventPublisher>();

            _mockPolicySettings = new Mock<IOptions<PolicySettings>>();
            _policies = new Policies(_mockPolicySettings.Object);

            _sut = new MultiEventPublisherWithLimit(_mockEventPublisher.Object, _policies);
        }

        [Test]
        public async Task Then_each_event_is_published_to_the_EventPublisher()
        {
            //Arrange
            var testEvents = _fixture.Create<List<TestEvent>>();

            //Act
            await _sut.Publish(testEvents);

            //Assert
            foreach(var testEvent in testEvents)
            {
                _mockEventPublisher.Verify(m => m.Publish(It.Is<TestEvent>(e => e == testEvent)), Times.Exactly(1));
            }            
        }

        [Test]
        public void Then_an_event_that_errors_is_returned_in_an_aggregate_exception()
        {
            //Arrange
            var testEvents = _fixture.Create<List<TestEvent>>();
            var errorEvent = _fixture.Create<TestEvent>();
            testEvents.Add(errorEvent);
            _mockEventPublisher
                .Setup(m => m.Publish(errorEvent))
                .ThrowsAsync(new Exception("Test Message"));

            // Act
            Func<Task> action = async () => await _sut.Publish(testEvents);

            // Assert
            action.Should().Throw<AggregateException>().Where(e => e.InnerExceptions.Count == 1);
            _mockEventPublisher.Verify(m => m.Publish(It.IsAny<TestEvent>()), Times.Exactly(4));
        }
    }
}
