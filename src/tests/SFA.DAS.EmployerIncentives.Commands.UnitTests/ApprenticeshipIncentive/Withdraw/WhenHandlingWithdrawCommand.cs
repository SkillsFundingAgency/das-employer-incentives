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
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.Withdraw
{
    public class WhenHandlingWithdrawCommand
    {
        private WithdrawCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRepository;
        private Fixture _fixture;
     
        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockIncentiveDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();

            _fixture.Register(ApprenticeshipIncentiveCreator);

            _sut = new WithdrawCommandHandler(_mockIncentiveDomainRepository.Object);
        }

        [Test]
        public async Task Then_the_incentive_is_deleted()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var command = new WithdrawCommand(incentive.Account.Id, incentive.Id);

            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(incentive);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.IsDeleted.Should().BeTrue();
        }

        [Test]
        public async Task Then_the_incentive_is_persisted()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var command = new WithdrawCommand(incentive.Account.Id, incentive.Id);

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

            var command = new WithdrawCommand(incentive.Account.Id, incentive.Id);

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
                        ApprenticeshipEmployerType.Levy
                    ),
                    DateTime.Today);

            return incentive;
        }
    }
}
