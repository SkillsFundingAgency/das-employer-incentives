using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.LearnerChangeOfCircumstance;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.LearnerChangeOfCircumstance
{
    public class WhenHandlingLearnerChangeOfCircumstanceCommand
    {
        private LearnerChangeOfCircumstanceCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRespository;
        private Fixture _fixture;
        private Mock<ILearnerDomainRepository> _mockLearnerDomainRespository;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _incentive;
        private Learner _learner;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockIncentiveDomainRespository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockLearnerDomainRespository = new Mock<ILearnerDomainRepository>();

            var incentive = new ApprenticeshipIncentiveFactory()
                    .CreateNew(_fixture.Create<Guid>(),
                    _fixture.Create<Guid>(),
                    _fixture.Create<Account>(),
                    new Apprenticeship(
                        _fixture.Create<long>(),
                        _fixture.Create<string>(),
                        _fixture.Create<string>(),
                        DateTime.Today.AddYears(-26),
                        _fixture.Create<long>(),
                        ApprenticeshipEmployerType.Levy
                        ),
                    DateTime.Today);
            incentive.SetHasPossibleChangeOfCircumstances(true);

            incentive.Apprenticeship.SetProvider(_fixture.Create<Provider>());

            _fixture.Register(() => incentive);

            _sut = new LearnerChangeOfCircumstanceCommandHandler(_mockIncentiveDomainRespository.Object, _mockLearnerDomainRespository.Object);

            _incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();
            _learner = new LearnerFactory().GetExisting(
                _fixture.Build<LearnerModel>()
                .With(x => x.SubmissionData, _fixture.Create<SubmissionData>())
                .With(x=> x.ApprenticeshipIncentiveId, _incentive.Id)
                .Create());

            _mockIncentiveDomainRespository.Setup(x => x.Find(incentive.Id)).ReturnsAsync(_incentive);
            _mockLearnerDomainRespository.Setup(m => m.GetOrCreate(incentive)).ReturnsAsync(_learner);
        }

        [Test]
        public async Task Then_the_start_date_is_updated()
        {
            //Arrange
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);
            _learner.SubmissionData.SetSubmissionDate(_fixture.Create<DateTime>());
            _learner.SubmissionData.SetLearningData(new LearningData(true));
            _learner.SubmissionData.LearningData.SetStartDate(_fixture.Create<DateTime>());

            // Act
            await _sut.Handle(command);

            // Assert
            _incentive.StartDate.Should().Be(_learner.SubmissionData.LearningData.StartDate.Value);
        }

        [Test]
        public async Task Then_the_changes_are_persisted()
        {
            //Arrange
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);
            _learner.SubmissionData.LearningData.SetStartDate(_fixture.Create<DateTime>());

            int itemsPersisted = 0;
            _mockIncentiveDomainRespository.Setup(m => m.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(a => a.Id == command.ApprenticeshipIncentiveId)))
                .Callback(() =>
                {
                    itemsPersisted++;
                });


            // Act
            await _sut.Handle(command);

            // Assert
            itemsPersisted.Should().Be(1);
        }

        [Test]
        public async Task Then_the_start_date_is_not_updated_when_submission_data_was_not_found()
        {
            //Arrange
            var originalStartDate = _incentive.StartDate;
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);
            _learner.SetSubmissionData(null);

            // Act
            await _sut.Handle(command);

            // Assert
            _incentive.StartDate.Should().Be(originalStartDate);
        }

        [Test]
        public async Task Then_the_start_date_is_not_updated_when_submission_has_no_start_date()
        {
            //Arrange
            var originalStartDate = _incentive.StartDate;
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);
            _learner.SubmissionData.LearningData.SetStartDate(null);

            // Act
            await _sut.Handle(command);

            // Assert
            _incentive.StartDate.Should().Be(originalStartDate);
        }

        [Test]
        public async Task Then_HasPossibleChangeOfCircumstances_set_to_false_when_change_of_circs_complete()
        {
            //Arrange
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);

            // Act
            await _sut.Handle(command);

            // Assert
            _incentive.HasPossibleChangeOfCircumstances.Should().BeFalse();
        }

        [Test]
        public async Task Then_process_should_exit_if_no_possible_change_of_circumstances()
        {
            //Arrange
            _incentive.SetHasPossibleChangeOfCircumstances(false);
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockLearnerDomainRespository.Verify(x => x.GetOrCreate(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()), Times.Never);
            _mockIncentiveDomainRespository.Verify(x => x.Save(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()), Times.Never);
        }
    }
}
