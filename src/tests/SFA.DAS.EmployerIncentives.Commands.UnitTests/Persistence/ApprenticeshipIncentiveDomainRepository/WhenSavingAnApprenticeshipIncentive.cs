using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Persistence.ApprenticeshipIncentiveDomainRepository
{
    public class WhenSavingAnApprenticeshipIncentive
    {
        private Commands.Persistence.ApprenticeshipIncentiveDomainRepository _sut;
        private Mock<IApprenticeshipIncentiveDataRepository> _mockApprenticeshipIncentiveDataRepository;
        private Mock<IPaymentDataRepository> _mockPaymentDataRepository;
        private Mock<IApprenticeshipIncentiveFactory> _mockApprenticeshipIncentiveFactory;
        private Mock<IDomainEventDispatcher> _mockDomainEventDispatcher;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new ApprenticeshipIncentiveCustomization());

            _mockApprenticeshipIncentiveDataRepository = new Mock<IApprenticeshipIncentiveDataRepository>();
            _mockPaymentDataRepository = new Mock<IPaymentDataRepository>();
            _mockApprenticeshipIncentiveFactory = new Mock<IApprenticeshipIncentiveFactory>();
            _mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();

            _sut = new Commands.Persistence.ApprenticeshipIncentiveDomainRepository(
                _mockApprenticeshipIncentiveDataRepository.Object,
                _mockPaymentDataRepository.Object,
                _mockApprenticeshipIncentiveFactory.Object, 
                _mockDomainEventDispatcher.Object);
        }

        [Test]
        public async Task Then_a_new_entity_is_persisted_by_the_data_layer()
        {
            //Arrange
            var entity = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            //Act
            await _sut.Save(entity);

            //Assert
            _mockApprenticeshipIncentiveDataRepository.Verify(m => m.Add(entity.GetModel()), Times.Once);
        }
    }
}
