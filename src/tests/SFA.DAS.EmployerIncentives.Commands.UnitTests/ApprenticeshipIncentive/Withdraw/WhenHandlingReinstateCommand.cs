using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.Withdraw;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.Withdraw
{
    public class WhenHandlingReinstateCommand
    {
        private ReinstateApprenticeshipIncentiveCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRepository;
        private Mock<ICollectionCalendarService> _mockCollectionCalendarService;
        private CollectionCalendarPeriod _activePeriod;

        private Fixture _fixture;
        private Domain.ValueObjects.CollectionCalendar _collectionCalendar;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockIncentiveDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();

            _activePeriod = CollectionPeriod(2, 2020);
            _activePeriod.SetActive(true);

            var collectionPeriods = new List<Domain.ValueObjects.CollectionCalendarPeriod>()
            {
                CollectionPeriod(1, 2020),
                _activePeriod,
                CollectionPeriod(3, 2020)
            };
            _collectionCalendar = new Domain.ValueObjects.CollectionCalendar(new List<AcademicYear>(), collectionPeriods);

            _mockCollectionCalendarService.Setup(m => m.Get()).ReturnsAsync(_collectionCalendar);

            _fixture.Register(ApprenticeshipIncentiveCreator);

            _sut = new ReinstateApprenticeshipIncentiveCommandHandler(_mockIncentiveDomainRepository.Object, _mockCollectionCalendarService.Object);
        }

        [Test]
        public async Task Then_the_incentive_is_marked_as_active()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var command = new ReinstateApprenticeshipIncentiveCommand(incentive.Account.Id, incentive.Id);

            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(incentive);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.Status.Should().Be(IncentiveStatus.Active);
            incentive.WithdrawnBy.Should().BeNull();
        }
        
        [Test]
        public async Task Then_the_earnings_are_recreated()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var command = new ReinstateApprenticeshipIncentiveCommand(incentive.Account.Id, incentive.Id);

            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(incentive);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.PendingPayments.Count.Should().Be(2)
        }

        [Test]
        public async Task Then_the_incentive_is_persisted()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var command = new ReinstateApprenticeshipIncentiveCommand(incentive.Account.Id, incentive.Id);

            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(incentive);
                        
            // Act
            await _sut.Handle(command);

            // Assert
            _mockIncentiveDomainRepository
                .Verify(m => m.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(i =>
               i.Id == command.IncentiveApplicationApprenticeshipId)),
                Times.Once);
        }

        [Test]
        public async Task Then_the_incentive_is_not_persisted_if_it_does_not_exist()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var command = new ReinstateApprenticeshipIncentiveCommand(incentive.Account.Id, incentive.Id);

            _mockIncentiveDomainRepository
                .Setup(x => x.FindByApprenticeshipId(
                    command.IncentiveApplicationApprenticeshipId))
                .ReturnsAsync(null as Domain.ApprenticeshipIncentives.ApprenticeshipIncentive);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockIncentiveDomainRepository
                .Verify(m => m.Save(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()),
                Times.Never);
        }

        private CollectionCalendarPeriod CollectionPeriod(byte periodNumber, short academicYear)
        {
            return new CollectionCalendarPeriod(new Domain.ValueObjects.CollectionPeriod(periodNumber, academicYear), 1, academicYear, _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), false, false);
        }

        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive ApprenticeshipIncentiveCreator()
        {
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
                        ApprenticeshipEmployerType.Levy,
                        _fixture.Create<string>(),
                        _fixture.Create<DateTime>(),
                        _fixture.Create<Provider>()
                    ),
                    DateTime.Today,
                    _fixture.Create<DateTime>(),
                    _fixture.Create<string>(),
                    new AgreementVersion(_fixture.Create<int>()),
                    new IncentivePhase(Phase.Phase1));

            incentive.Withdraw(WithdrawnBy.Compliance, _collectionCalendar);

            return incentive;
        }
    }
}
