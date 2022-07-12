using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.RecalculateEarnings;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.RecalculateEarnings.Handlers
{
    [TestFixture]
    public class WhenHandlingRecalculateEarningsCommand
    {
        private RecalculateEarningsCommandHandler _sut;
        private Fixture _fixture;
        private Mock<IApprenticeshipIncentiveDomainRepository> _domainRepository;
        private Mock<ICollectionCalendarService> _collectionCalendarService;
        private List<CollectionCalendarPeriod> _collectionPeriods;
        private Domain.ValueObjects.CollectionCalendar _collectionCalendar;
        private DateTime _collectionPeriod;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _domainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _collectionCalendarService = new Mock<ICollectionCalendarService>();
            _sut = new RecalculateEarningsCommandHandler(_domainRepository.Object, _collectionCalendarService.Object);
            _collectionPeriod = new DateTime(2020, 10, 1);
            _collectionPeriods = new List<CollectionCalendarPeriod>()
            {
                new CollectionCalendarPeriod(new Domain.ValueObjects.CollectionPeriod(1, _fixture.Create<short>()), (byte)_collectionPeriod.AddMonths(-1).Month, (short)_collectionPeriod.AddMonths(-1).Year, _fixture.Create<DateTime>(), _collectionPeriod.AddMonths(1).AddDays(1), true, false)
            };
            for (var i = 1; i <= 12; i++)
            {
                _collectionPeriods.Add(new CollectionCalendarPeriod(new Domain.ValueObjects.CollectionPeriod((byte)i, _fixture.Create<short>()), (byte)_collectionPeriod.AddMonths(i).Month, (short)_collectionPeriod.AddMonths(i).Year, _fixture.Create<DateTime>(), _collectionPeriod.AddMonths(i + 1).AddDays(1), false, false)
                );
            }

            _collectionCalendar = new Domain.ValueObjects.CollectionCalendar(new List<AcademicYear>(), _collectionPeriods);
            
            _collectionCalendarService.Setup(x => x.Get()).ReturnsAsync(_collectionCalendar);
        }

        [Test]
        public async Task Then_the_earnings_are_recalculated_for_the_incentives_identified()
        {
            // Arrange
            var command = new RecalculateEarningsCommand(_fixture.CreateMany<IncentiveLearnerIdentifier>(5));

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
                    .With(x => x.Status, IncentiveStatus.Active)
                    .With(x => x.PreviousStatus, IncentiveStatus.Active)
                    .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                    .Without(x => x.PaymentModels)
                    .Without(x => x.PendingPaymentModels)
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
            var command = new RecalculateEarningsCommand(_fixture.CreateMany<IncentiveLearnerIdentifier>(1));
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
