using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseDetailsForNewApplications;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Messages.Events;
using SFA.DAS.NServiceBus.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.UpdateVrfCaseDetailsForNewApplications.Handlers
{
    public class WhenHandlingUpdateVrfCaseDetailsForNewApplicationsCommand
    {
        private UpdateVrfCaseDetailsForNewApplicationsCommandHandler _sut;
        private Mock<IQueryRepository<IncentiveApplicationLegalEntityDto>> _mockQueryRepository;
        private Mock<IEventPublisher> _mockEventPublisher;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockQueryRepository = new Mock<IQueryRepository<IncentiveApplicationLegalEntityDto>>();
            _mockEventPublisher = new Mock<IEventPublisher>();

            _sut = new UpdateVrfCaseDetailsForNewApplicationsCommandHandler(_mockQueryRepository.Object, _mockEventPublisher.Object);
        }

        [Test]
        public async Task Then_an_UpdateLegalEntityVrfCaseDetailsEvent_is_published_for_each_legal_entity()
        {
            //Arrange
            var applicationLegalEntities = _fixture.CreateMany<IncentiveApplicationLegalEntityDto>().ToList();
            var command = new UpdateVrfCaseDetailsForNewApplicationsCommand();
            _mockQueryRepository.Setup(x => x.GetList(dto => dto.ApplicationStatus == IncentiveApplicationStatus.Submitted && dto.VrfCaseId == null)).ReturnsAsync(applicationLegalEntities);

            //Act
            await _sut.Handle(command);

            //Assert            
            _mockEventPublisher.Verify(x => x.Publish(It.Is<UpdateLegalEntityVrfCaseDetailsEvent>(x => applicationLegalEntities.Any(y => y.LegalEntityId == x.LegalEntityId))), Times.Exactly(applicationLegalEntities.Count));
        }
    }
}
