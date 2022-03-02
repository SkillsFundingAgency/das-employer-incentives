using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.RecalculateEarnings;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.RecalculateEarnings.Handlers
{
    [TestFixture]
    public class WhenHandlingRecalculateEarningsCommand
    {
        private RecalculateEarningsCommandHandler _sut;
        private Fixture _fixture;
        private Mock<IApprenticeshipIncentiveDomainRepository> _domainRepository;
        private Mock<ICollectionCalendarService> _collectionCalendarService;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _domainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _collectionCalendarService = new Mock<ICollectionCalendarService>();
            _sut = new RecalculateEarningsCommandHandler(_domainRepository.Object, _collectionCalendarService.Object);
            var collectionCalendar = _fixture.Create<Domain.ValueObjects.CollectionCalendar>();
            _collectionCalendarService.Setup(x => x.Get()).ReturnsAsync(collectionCalendar);
        }

        [Test]
        public async Task Then_the_earnings_are_recalculated_for_the_incentives_identified()
        {
            // Arrange
            var command = new RecalculateEarningsCommand(_fixture.CreateMany<IncentiveLearnerIdentifierDto>(5));

            foreach(var identifier in command.IncentiveLearnerIdentifiers)
            {
                var account = new Account(_fixture.Create<long>(), identifier.AccountLegalEntityId);
                var apprenticeship = new Apprenticeship(_fixture.Create<long>(), _fixture.Create<string>(),
                    _fixture.Create<string>(), _fixture.Create<DateTime>(), identifier.ULN,
                    ApprenticeshipEmployerType.Levy, _fixture.Create<string>(), _fixture.Create<DateTime>(),
                    new Provider(_fixture.Create<long>()));
                var incentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                    .With(x => x.Account, account)
                    .With(x => x.Apprenticeship, apprenticeship)
                    .Create();
                var incentive =  new ApprenticeshipIncentiveFactory().GetExisting(incentiveModel.Id, incentiveModel);
                _domainRepository
                    .Setup(x => x.FindByUlnWithinAccountLegalEntity(identifier.ULN, identifier.AccountLegalEntityId))
                    .ReturnsAsync(incentive);
            }

            // Act
            await _sut.Handle(command);

            // Assert
            foreach(var identifier in command.IncentiveLearnerIdentifiers)
            {
                _domainRepository.Verify(x => x.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(
                    y => y.Account.AccountLegalEntityId == identifier.AccountLegalEntityId
                    && y.Apprenticeship.UniqueLearnerNumber == identifier.ULN)), Times.Once);
            }
        }

        [Test]
        public void Then_an_exception_is_thrown_if_the_incentive_is_not_found()
        {
            // Arrange
            var command = new RecalculateEarningsCommand(_fixture.CreateMany<IncentiveLearnerIdentifierDto>(1));
            var identifier = command.IncentiveLearnerIdentifiers.ToList()[0];

            Domain.ApprenticeshipIncentives.ApprenticeshipIncentive nullIncentive = null;
            _domainRepository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(identifier.ULN, identifier.AccountLegalEntityId))
                // ReSharper disable once ExpressionIsAlwaysNull
                .ReturnsAsync(nullIncentive);
        
            // Act
            Func<Task> commandAction = async () => await _sut.Handle(command);
            
            // Assert
            commandAction.Should().Throw<ArgumentException>();
        }
    }
}
