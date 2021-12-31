using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    public class WhenEmploymentCheckDeletedHandler
    {
        private Mock<IApprenticeshipIncentiveArchiveRepository> _mockArchiveDataRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockArchiveDataRepository = new Mock<IApprenticeshipIncentiveArchiveRepository>();
        }

        [Test]
        public async Task Then_an_archive_record_is_persisted_for_an_employment_check()
        {
            //Arrange
            var sut = new EmploymentCheckDeletedHandler(_mockArchiveDataRepository.Object);
            var @event = _fixture.Create<EmploymentCheckDeleted>();

            //Act
            await sut.Handle(@event);

            //Assert
            _mockArchiveDataRepository.Verify(m => m.Archive(@event.Model), Times.Once);
        }
    }
}
