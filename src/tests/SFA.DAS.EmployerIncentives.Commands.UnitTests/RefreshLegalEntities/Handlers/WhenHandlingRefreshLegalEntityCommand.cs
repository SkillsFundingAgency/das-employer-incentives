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

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.RefreshLegalEntities.Handlers
{
    public class WhenHandlingRefreshLegalEntityCommand
    {
        private RefreshLegalEntitiesCommandHandler _sut;
        private Mock<IAccountService> _mockAccountService;
        private Mock<IMultiEventPublisher> _mockMultiEventPublisher;

        private Fixture _fixture;
        private PagedModel<AccountLegalEntity> _pagedData;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockAccountService = new Mock<IAccountService>();
            _mockMultiEventPublisher = new Mock<IMultiEventPublisher>();

            _pagedData = _fixture.Create<PagedModel<AccountLegalEntity>>();
            
            _mockMultiEventPublisher
                .Setup(m => m.Publish(It.IsAny<List<RefreshLegalEntitiesEvent>>()))
                .Returns(Task.CompletedTask);

            _mockAccountService
                .Setup(m => m.GetAccountLegalEntitiesByPage(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(_pagedData);

            _sut = new RefreshLegalEntitiesCommandHandler(_mockAccountService.Object, _mockMultiEventPublisher.Object);
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
            _mockMultiEventPublisher.Verify(m => m.Publish(It.IsAny<List<RefreshLegalEntitiesEvent>>()), Times.Never);
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
            _mockMultiEventPublisher.Verify(m => m.Publish(It.Is<List<RefreshLegalEntitiesEvent>>(events => AssertEvents(events, _pagedData))), Times.Once);
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
            _mockMultiEventPublisher.Verify(m => m.Publish(It.Is<List<RefreshLegalEntityEvent>>(events => AssertEvents(events))), Times.Once);
        }

        private bool AssertEvents(List<RefreshLegalEntitiesEvent> events, PagedModel<AccountLegalEntity> pagedData)
        {
            events.Count.Should().Be(pagedData.TotalPages - 1);
            for (int i = 2; i <= pagedData.TotalPages; i++)
            {
                events.Any(le => le.PageNumber == i && le.PageSize == pagedData.TotalPages).Should().BeTrue();
            }

            return true;
        }

        private bool AssertEvents(List<RefreshLegalEntityEvent> events)
        {
            events.Count.Should().Be(3);
            foreach(var legalEntities in _pagedData.Data)
            {
                events.Any(legalEntity =>
                legalEntity.AccountId == legalEntities.AccountId  &&
                legalEntity.AccountLegalEntityId == legalEntities.AccountLegalEntityId &&
                legalEntity.LegalEntityId == legalEntities.LegalEntityId &&
                legalEntity.OrganisationName == legalEntities.Name).Should().BeTrue();
            }

            return true;
        }
    }
}
