using System;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Persistence.IncentiveApplicationDomainRepository
{
    public class WhenSavingAnIncentiveApplication
    {
        private Commands.Persistence.IncentiveApplicationDomainRepository _sut;
        private Mock<IIncentiveApplicationDataRepository> _mockIncentiveApplicationDataRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Register(() => new IncentiveApplicationFactory().CreateNew(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>()));
            
            _mockIncentiveApplicationDataRepository = new Mock<IIncentiveApplicationDataRepository>();

            _sut = new Commands.Persistence.IncentiveApplicationDomainRepository(_mockIncentiveApplicationDataRepository.Object);
        }

        [Test]
        public async Task Then_a_new_entity_is_persisted_by_the_data_layer()
        {
            //Arrange
            var entity = _fixture.Create<IncentiveApplication>();

            //Act
            await _sut.Save(entity);

            //Assert
            _mockIncentiveApplicationDataRepository.Verify(m => m.Add(entity.GetModel()), Times.Once);
        }
    }
}
