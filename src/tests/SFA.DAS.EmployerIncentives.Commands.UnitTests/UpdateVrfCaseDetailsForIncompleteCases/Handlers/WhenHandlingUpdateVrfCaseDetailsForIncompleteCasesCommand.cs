using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseDetailsForIncompleteCases;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Messages.Events;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.UpdateVrfCaseDetailsForIncompleteCases.Handlers
{
    public class WhenHandlingUpdateVrfCaseDetailsForIncompleteCasesCommand
    {
        private UpdateVrfCaseDetailsForIncompleteCasesCommandHandler _sut;
        private Mock<IQueryRepository<LegalEntityVendorRegistrationFormDto>> _mockQueryRepository;
        private Mock<IEventPublisher> _mockEventPublisher;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockQueryRepository = new Mock<IQueryRepository<LegalEntityVendorRegistrationFormDto>>();
            _mockEventPublisher = new Mock<IEventPublisher>();

            _sut = new UpdateVrfCaseDetailsForIncompleteCasesCommandHandler(_mockQueryRepository.Object, _mockEventPublisher.Object);
        }

        [Test]
        public async Task Then_an_UpdateLegalEntityVrfCaseStatusEvent_is_published_for_each_legal_entity()
        {
            //Arrange
            var legalEntities = _fixture.CreateMany<LegalEntityVendorRegistrationFormDto>(3).ToList();
            var command = new UpdateVrfCaseDetailsForIncompleteCasesCommand();
            _mockQueryRepository.Setup(x => x.GetList(dto => dto.VrfCaseId != null && dto.VrfCaseStatus != "Case Request completed")).ReturnsAsync(legalEntities);

            //Act
            await _sut.Handle(command);

            //Assert            
            _mockEventPublisher.Verify(x => x.Publish(It.Is<UpdateLegalEntityVrfCaseStatusEvent>(x => x.LegalEntityId == legalEntities[0].LegalEntityId && x.VrfCaseId == legalEntities[0].VrfCaseId && x.VrfVendorId == legalEntities[0].VrfVendorId)), Times.Once);
            _mockEventPublisher.Verify(x => x.Publish(It.Is<UpdateLegalEntityVrfCaseStatusEvent>(x => x.LegalEntityId == legalEntities[1].LegalEntityId && x.VrfCaseId == legalEntities[1].VrfCaseId && x.VrfVendorId == legalEntities[1].VrfVendorId)), Times.Once);
            _mockEventPublisher.Verify(x => x.Publish(It.Is<UpdateLegalEntityVrfCaseStatusEvent>(x => x.LegalEntityId == legalEntities[2].LegalEntityId && x.VrfCaseId == legalEntities[2].VrfCaseId && x.VrfVendorId == legalEntities[2].VrfVendorId)), Times.Once);
        }
    }
}
