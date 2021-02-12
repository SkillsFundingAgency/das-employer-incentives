using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities;
using SFA.DAS.EmployerIncentives.Commands.Services.AccountApi;
using SFA.DAS.EmployerIncentives.Messages.Events;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using System.Linq;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.RefreshLegalEntities.Handlers
{
    [TestFixture]
    public class WhenHandlingRefreshLegalEntityCommand
    {
        private RefreshLegalEntitiesCommandHandler _sut;
        private Mock<IEventPublisher> _mockEventPublisher;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockEventPublisher = new Mock<IEventPublisher>();

            _mockEventPublisher
                .Setup(m => m.Publish(It.IsAny<List<RefreshLegalEntitiesEvent>>()))
                .Returns(Task.CompletedTask);

            _sut = new RefreshLegalEntitiesCommandHandler(_mockEventPublisher.Object);
        }

        [Test]
        public async Task Then_a_RefreshLegalEntitiesEvent_is_not_published_when_page_number_is_not_1()
        {
            //Arrange
            var accountLegalEntities = _fixture.CreateMany<AccountLegalEntity>(10);
            var command = new RefreshLegalEntitiesCommand(accountLegalEntities, pageNumber: 2, pageSize: 100, totalPages: 3);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockEventPublisher.Verify(m => m.Publish(It.IsAny<List<RefreshLegalEntitiesEvent>>()), Times.Never);
        }

        [Test]
        public async Task Then_a_RefreshLegalEntitiesEvent_is_published_for_each_page_when_page_number_is_1()
        {
            //Arrange
            var accountLegalEntities = _fixture.CreateMany<AccountLegalEntity>(10);
            var command = new RefreshLegalEntitiesCommand(accountLegalEntities, pageNumber: 1, pageSize: 10, totalPages: 3);

            //Act
            await _sut.Handle(command);

            //Assert            
            _mockEventPublisher.Verify(m => m.Publish(It.IsAny<RefreshLegalEntitiesEvent>()), Times.Exactly(2));
            _mockEventPublisher.Verify(m => m.Publish(It.Is<RefreshLegalEntitiesEvent>(e => e.PageNumber == 2)), Times.Once);
            _mockEventPublisher.Verify(m => m.Publish(It.Is<RefreshLegalEntitiesEvent>(e => e.PageNumber == 3)), Times.Once);
        }

        [Test]
        public async Task Then_a_RefreshLegalEntityEvent_is_published_for_each_legal_entity_for_a_page()
        {
            //Arrange
            var accountLegalEntities = _fixture.CreateMany<AccountLegalEntity>(10);
            var command = new RefreshLegalEntitiesCommand(accountLegalEntities, pageNumber: 1, pageSize: 10, totalPages: 1);

            //Act
            await _sut.Handle(command);

            //Assert            
            _mockEventPublisher.Verify(m => m.Publish(It.Is<RefreshLegalEntityEvent>(e => AssertEventIsExpected(e, command.AccountLegalEntities))), Times.Exactly(10));
        }

        private bool AssertEventIsExpected(RefreshLegalEntityEvent e, IEnumerable<AccountLegalEntity> pagedData)
        {
            pagedData.Any(ale =>
                ale.AccountId == e.AccountId &&
                    ale.AccountLegalEntityId == e.AccountLegalEntityId &&
                    ale.LegalEntityId == e.LegalEntityId &&
                    ale.Name == e.OrganisationName).Should().BeTrue();

            return true;
        }
    }
}
