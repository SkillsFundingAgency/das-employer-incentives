using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.LearnerChangeOfCircumstance;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.LearnerChangeOfCircumstance
{
    public class WhenHandlingLearnerChangeOfCircumstanceCommand
    {
        private LearnerChangeOfCircumstanceCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRespository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockIncentiveDomainRespository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            
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
            
            incentive.Apprenticeship.SetProvider(_fixture.Create<Provider>());

            _fixture.Register(() => incentive);

            _sut = new LearnerChangeOfCircumstanceCommandHandler(_mockIncentiveDomainRespository.Object);
        }

        [Test]
        public async Task Then_the_start_date_is_updated()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var command = new LearnerChangeOfCircumstanceCommand(incentive.Id, _fixture.Create<DateTime>());

            _mockIncentiveDomainRespository.Setup(x => x
            .Find(command.ApprenticeshipIncentiveId))
                .ReturnsAsync(incentive);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.ActualStartDate.Should().Be(command.StartDate);
        }

        [Test]
        public async Task Then_the_changes_are_persisted()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var command = new LearnerChangeOfCircumstanceCommand(incentive.Id, _fixture.Create<DateTime>());

            _mockIncentiveDomainRespository.Setup(x => x
            .Find(command.ApprenticeshipIncentiveId))
                .ReturnsAsync(incentive);

            int itemsPersisted = 0;
            _mockIncentiveDomainRespository.Setup(m => m.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>( a => a.Id == command.ApprenticeshipIncentiveId)))
                                            .Callback(() =>
                                            {
                                                itemsPersisted++;
                                            });


            // Act
            await _sut.Handle(command);

            // Assert
            itemsPersisted.Should().Be(1);
        }
    }
}
