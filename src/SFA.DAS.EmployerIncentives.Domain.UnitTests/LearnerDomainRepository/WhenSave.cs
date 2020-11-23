using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.LearnerDomainRepository
{
    public class WhenSave
    {
        private ILearnerDomainRepository _sut;
        private Fixture _fixture;

        private Mock<ILearnerFactory> _learnerFactoryMock;
        private Mock<ILearnerDataRepository> _learnerDataRepositoryMock;
        private Mock<IDomainEventDispatcher> _domainEventDispatcherMock;
        private LearnerFactory _learnerFactory;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _learnerDataRepositoryMock = new Mock<ILearnerDataRepository>();
            _learnerFactoryMock = new Mock<ILearnerFactory>();
            _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();
            _learnerFactory = new LearnerFactory();

            _sut = new Commands.Persistence.LearnerDomainRepository(_learnerDataRepositoryMock.Object, _learnerFactory,
                _domainEventDispatcherMock.Object);
        }

        [Test]
        public async Task Then_adds_if_new()
        {
            // Arrange
            var model = _fixture.Create<LearnerModel>();
            var aggregate = _learnerFactory.CreateNew(model.Id, model.ApprenticeshipIncentiveId,
                model.ApprenticeshipId, model.Ukprn, model.UniqueLearnerNumber, model.CreatedDate);

            // Act
            await _sut.Save(aggregate);

            // Assert
            _learnerDataRepositoryMock.Verify(r => r.Add(aggregate.GetModel()), Times.Once);
        }

        [Test]
        public async Task Then_updates_if_existing()
        {
            // Arrange
            var model = _fixture.Create<LearnerModel>();
            var aggregate = _learnerFactory.GetExisting(model);

            // Act
            await _sut.Save(aggregate);

            // Assert
            _learnerDataRepositoryMock.Verify(r => r.Update(aggregate.GetModel()), Times.Once);
        }

    }
}
