using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.LearnerDomainRepository
{
    public class WhenGetByApprenticeshipIncentive
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

            _learnerDataRepositoryMock = new Mock<ILearnerDataRepository>();
            _learnerFactoryMock = new Mock<ILearnerFactory>();
            _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();
            ILearnerFactory learnerFactory = new LearnerFactory();

            _sut = new Commands.Persistence.LearnerDomainRepository(_learnerDataRepositoryMock.Object, learnerFactory,
                _domainEventDispatcherMock.Object);
        }

        [Test]
        public async Task Then_the_existing_learner_is_returned()
        {
            // Arrange
            var incentiveModel = _fixture.Create<ApprenticeshipIncentiveModel>();
            var payments = _fixture.Build<PendingPaymentModel>()
                .With(pp => pp.ApprenticeshipIncentiveId, incentiveModel.ApplicationApprenticeshipId)
                .Without(pp => pp.PaymentMadeDate) // null for "not paid"
                .CreateMany(5).ToArray();

            payments[0].DueDate = DateTime.Parse("01/09/2020", new CultureInfo("en-GB"));
            payments[0].PaymentMadeDate = DateTime.Parse("30/09/2020", new CultureInfo("en-GB"));
            payments[1].DueDate = DateTime.Parse("01/10/2020", new CultureInfo("en-GB")); // next pending payment
            payments[2].DueDate = DateTime.Parse("01/11/2020", new CultureInfo("en-GB"));
            payments[3].DueDate = DateTime.Parse("01/12/2020", new CultureInfo("en-GB"));
            payments[4].DueDate = DateTime.Parse("01/01/2021", new CultureInfo("en-GB"));
            incentiveModel.PendingPaymentModels = payments;

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(incentiveModel.ApplicationApprenticeshipId, incentiveModel);
            incentive.Apprenticeship.SetProvider(_fixture.Create<Provider>());

            var learner = _fixture.Create<LearnerModel>();
            _learnerDataRepositoryMock.Setup(r => r.GetByApprenticeshipIncentiveId(incentive.Id)).ReturnsAsync(learner);

            // Act
            var result = await _sut.GetOrCreate(incentive);

            // Assert
            result.Id.Should().Be(learner.Id, "should return existing");
        }


        [Test]
        public async Task Then_a_new_learner_is_returned()
        {
            // Arrange
            var incentiveModel = _fixture.Create<ApprenticeshipIncentiveModel>();
            var payments = _fixture.Build<PendingPaymentModel>()
                .With(pp => pp.ApprenticeshipIncentiveId, incentiveModel.ApplicationApprenticeshipId)
                .Without(pp => pp.PaymentMadeDate) // null for "not paid"
                .CreateMany(2).ToArray();

            payments[0].DueDate = DateTime.Parse("01/09/2020", new CultureInfo("en-GB")); // next pending payment
            payments[1].DueDate = DateTime.Parse("01/10/2020", new CultureInfo("en-GB"));
            incentiveModel.PendingPaymentModels = payments;
            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(incentiveModel.ApplicationApprenticeshipId, incentiveModel);
            incentive.Apprenticeship.SetProvider(_fixture.Create<Provider>());

            _learnerDataRepositoryMock.Setup(r => r.GetByApprenticeshipIncentiveId(incentive.Id)).ReturnsAsync((LearnerModel)null);

            // Act
            var result = await _sut.GetOrCreate(incentive);

            // Assert
            result.Id.Should().NotBeEmpty("should set a new Id");
            result.ApprenticeshipIncentiveId.Should().Be(incentive.Id);
            result.ApprenticeshipId.Should().Be(incentive.Apprenticeship.Id);
            result.SubmissionData.SubmissionFound.Should().Be(false);
            result.Ukprn.Should().Be(incentive.Apprenticeship.Provider.Ukprn);
            result.UniqueLearnerNumber.Should().Be(incentive.Apprenticeship.UniqueLearnerNumber);
        }
    }
}
