using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Commands.Services.AccountApi;
using SFA.DAS.EmployerIncentives.Messages.Events;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using System.Linq;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.RefreshLegalEntities.Handlers
{
    public class WhenHandlingRefreshLegalEntityCommand
    {
        private RefreshLegalEntitiesCommandHandler _sut;
        private Mock<IAccountService> _mockAccountService;
        private Mock<IEventPublisher> _mockEventPublisher;

        private Fixture _fixture;
        private PagedModel<AccountLegalEntity> _pagedData;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockAccountService = new Mock<IAccountService>();
            _mockEventPublisher = new Mock<IEventPublisher>();

            _pagedData = _fixture.Create<PagedModel<AccountLegalEntity>>();
            
            _mockEventPublisher
                .Setup(m => m.Publish(It.IsAny<List<RefreshLegalEntitiesEvent>>()))
                .Returns(Task.CompletedTask);

            _mockAccountService
                .Setup(m => m.GetAccountLegalEntitiesByPage(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(_pagedData);

            _sut = new RefreshLegalEntitiesCommandHandler(_mockAccountService.Object, _mockEventPublisher.Object);
        }

        [Test]
        public async Task Then_the_current_legal_entities_are_retrieved_from_the_account_service()
        {
            //Arrange
            var command = _fixture.Create<RefreshLegalEntitiesCommand>();

            //Act
            await _sut.Handle(command);

            //Assert
            _mockAccountService.Verify(m => m.GetAccountLegalEntitiesByPage(command.PageNumber, command.PageSize), Times.Once);
        }
        
        [Test]
        public async Task Then_a_RefreshLegalEntitiesEvent_is_not_published_when_page_number_is_not_1()
        {
            //Arrange
            var command = new RefreshLegalEntitiesCommand(2);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockEventPublisher.Verify(m => m.Publish(It.IsAny<List<RefreshLegalEntitiesEvent>>()), Times.Never);
        }

        [Test]
        public async Task Then_a_RefreshLegalEntitiesEvent_is_published_for_each_page_when_page_number_is_1()
        {
            //Arrange
            _pagedData.Page = 1;
            _pagedData.TotalPages = 10;
            var command = new RefreshLegalEntitiesCommand(_pagedData.Page, _pagedData.TotalPages);

            //Act
            await _sut.Handle(command);

            //Assert            
            _mockEventPublisher.Verify(m => m.Publish(It.IsAny<RefreshLegalEntitiesEvent>()), Times.Exactly(9));
            _mockEventPublisher.Verify(m => m.Publish(It.Is<RefreshLegalEntitiesEvent>(e=>e.PageNumber == 2)), Times.Once);
            _mockEventPublisher.Verify(m => m.Publish(It.Is<RefreshLegalEntitiesEvent>(e=>e.PageNumber == 5)), Times.Once);
            _mockEventPublisher.Verify(m => m.Publish(It.Is<RefreshLegalEntitiesEvent>(e=>e.PageNumber == 10)), Times.Once);
        }

        [Test]
        public async Task Then_a_RefreshLegalEntityEvent_is_published_for_each_legal_entity_for_a_page()
        {
            //Arrange
            _pagedData.Page = _fixture.Create<int>() + 1;
            _pagedData.TotalPages = 1;
            var command = new RefreshLegalEntitiesCommand(_pagedData.Page, _pagedData.TotalPages);

            //Act
            await _sut.Handle(command);

            //Assert            
            _mockEventPublisher.Verify(m => m.Publish(It.Is<RefreshLegalEntityEvent>(e=> AssertEventIsExpected(e, _pagedData))), Times.Exactly(_pagedData.Data.Count));
        }

        private bool AssertEventIsExpected(RefreshLegalEntityEvent e, PagedModel<AccountLegalEntity> pagedData) 
        {
            pagedData.Data.Any(ale =>
                ale.AccountId == e.AccountId &&
                    ale.AccountLegalEntityId == e.AccountLegalEntityId &&
                    ale.LegalEntityId == e.LegalEntityId &&
                    ale.Name == e.OrganisationName).Should().BeTrue();

            return true;
        }
    }
}
