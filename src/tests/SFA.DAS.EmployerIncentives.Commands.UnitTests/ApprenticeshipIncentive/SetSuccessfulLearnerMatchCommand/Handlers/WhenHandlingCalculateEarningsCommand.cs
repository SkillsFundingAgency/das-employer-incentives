using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SetSuccessfulLearnerMatch;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.SetSuccessfulLearnerMatchCommand.Handlers
{
    public class WhenHandlingSetSuccessfulLearnerMatchCommand
    {
        private SetSuccessfulLearnerMatchCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRepository;
        private Mock<ILearnerDomainRepository> _mockLearnerDomainRepository;
        private Fixture _fixture;
        private Learner _learner;
        private bool _succeeded;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _incentive;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockIncentiveDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockLearnerDomainRepository = new Mock<ILearnerDomainRepository>();

            var incentiveModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship,
                    new Apprenticeship(
                        _fixture.Create<long>(),
                        _fixture.Create<string>(),
                        _fixture.Create<string>(),
                        DateTime.Today.AddYears(-26),
                        _fixture.Create<long>(),
                        ApprenticeshipEmployerType.Levy,
                        _fixture.Create<string>(),
                        _fixture.Create<DateTime>(),
                        _fixture.Create<Provider>()
                    ))
                .With(p => p.StartDate, DateTime.Today)
                .With(p => p.Status, Enums.IncentiveStatus.Active)
                .With(p => p.HasPossibleChangeOfCircumstances, true)
                .With(p => p.Phase, new IncentivePhase(Phase.Phase2))
                .With(p => p.MinimumAgreementVersion, new AgreementVersion(_fixture.Create<int>()))
                .With(p => p.PendingPaymentModels, new List<PendingPaymentModel>())
                .Create();

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(incentiveModel.Id, incentiveModel);
            _fixture.Register(() => incentive);
            _incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();
            _succeeded = _fixture.Create<bool>();
           
            _learner = new LearnerFactory().GetExisting(
                _fixture.Build<LearnerModel>()
                    .With(x => x.SuccessfulLearnerMatch, !_succeeded)
                    .With(x => x.ApprenticeshipIncentiveId, _incentive.Id)
                    .Create());

            _mockIncentiveDomainRepository.Setup(x => x.Find(_incentive.Id)).ReturnsAsync(_incentive);
            _mockLearnerDomainRepository.Setup(m => m.GetOrCreate(_incentive)).ReturnsAsync(_learner);

            _sut = new SetSuccessfulLearnerMatchCommandHandler(
                _mockIncentiveDomainRepository.Object,
                _mockLearnerDomainRepository.Object);
        }

        [Test]
        public async Task Then_learner_record_is_updated()
        {
            // Arrange
            var command = new Commands.ApprenticeshipIncentive.SetSuccessfulLearnerMatch.SetSuccessfulLearnerMatchCommand(
                _incentive.Id, _incentive.Apprenticeship.UniqueLearnerNumber, _succeeded);

            _mockIncentiveDomainRepository.Setup(
                    x => x.Find(command.ApprenticeshipIncentiveId))
                .ReturnsAsync(_incentive);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockLearnerDomainRepository.Verify(r => r.Save(It.Is<Learner>(
                l => l.Id == _learner.Id && l.SuccessfulLearnerMatch == _succeeded
            )), Times.Once);
        }
    }
}
