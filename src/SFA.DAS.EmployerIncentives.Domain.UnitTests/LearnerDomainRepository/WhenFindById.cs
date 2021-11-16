using AutoFixture;
using FluentAssertions;
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
    public class WhenFindById
    {
        private ILearnerDomainRepository _sut;
        private Fixture _fixture;

        private Mock<ILearnerFactory> _learnerFactoryMock;
        private Mock<ILearnerDataRepository> _learnerDataRepositoryMock;
        private Mock<IDomainEventDispatcher> _domainEventDispatcherMock;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize<LearnerModel>(c => c.Without(x => x.LearningPeriods));
          
            _learnerDataRepositoryMock = new Mock<ILearnerDataRepository>();
            _learnerFactoryMock = new Mock<ILearnerFactory>();
            _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();
            ILearnerFactory learnerFactory = new LearnerFactory();

            _sut = new Commands.Persistence.LearnerDomainRepository(_learnerDataRepositoryMock.Object, learnerFactory,
                _domainEventDispatcherMock.Object);
        }

        [Test]
        public async Task Then_returns_learner_domain_model_if_found()
        {
            // Arrange
            var model = _fixture.Create<LearnerModel>();
            _learnerDataRepositoryMock.Setup(r => r.Get(model.Id)).ReturnsAsync(model);

            // Act
            var result = await _sut.Find(model.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(model.Id);
        }


        [Test]
        public async Task Then_returns_null_if_not_found()
        {
            // Arrange
            var model = _fixture.Create<LearnerModel>();
            _learnerDataRepositoryMock.Setup(r => r.Get(model.Id)).ReturnsAsync((LearnerModel)null);

            // Act
            var result = await _sut.Find(model.Id);

            // Assert
            result.Should().BeNull();
        }

    }
}
