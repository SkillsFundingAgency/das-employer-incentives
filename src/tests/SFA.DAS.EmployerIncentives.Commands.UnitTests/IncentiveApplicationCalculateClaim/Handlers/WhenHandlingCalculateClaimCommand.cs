using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.IncentiveApplicationCalculateClaim;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.SubmitIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.IncentiveApplicationCalculateClaim.Handlers
{
    public class WhenHandlingCalculateClaimCommand
    {
        private CalculateClaimCommandHandler _sut;
        private Mock<IIncentiveApplicationDomainRepository> _mockDomainRespository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _mockDomainRespository = new Mock<IIncentiveApplicationDomainRepository>();

            _sut = new CalculateClaimCommandHandler(_mockDomainRespository.Object);
        }

        [Test]
        public async Task Then_the_EarningsCalculationRequestedEvents_are_raised_for_each_apprenticeship_on_application()
        {
            //Arrange
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            var command = new CalculateClaimCommand(incentiveApplication.AccountId, _fixture.Create<Guid>()); ;

            _mockDomainRespository.Setup(x => x.Find(command.IncentiveClaimApplicationId)).ReturnsAsync(incentiveApplication);

            // Act
            await _sut.Handle(command);

            // Assert
            var events = incentiveApplication.FlushEvents().OfType<EarningsCalculationRequestedEvent>().ToList();
            events.Count.Should().Be(incentiveApplication.Apprenticeships.Count);
            events.Should().AllBeEquivalentTo(incentiveApplication.Apprenticeships.Select(x =>
                new 
                {
                    incentiveApplication.AccountId, 
                    IncentiveClaimApplicationId = incentiveApplication.Id,
                    ApprenticeshipId = x.Id
                }));
        }

        [Test]
        public void Then_an_application_with_an_invalid_application_id_is_rejected()
        {
            //Arrange
            var command = _fixture.Create<CalculateClaimCommand>();

            _mockDomainRespository.Setup(x => x.Find(command.IncentiveClaimApplicationId))
                .ReturnsAsync((IncentiveApplication)null);

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            // Assert
            action.Should().Throw<InvalidRequestException>();
        }

        [Test]
        public void Then_an_application_with_an_non_matching_account_id_is_rejected()
        {
            //Arrange
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            var command = new CalculateClaimCommand(incentiveApplication.AccountId + 100, _fixture.Create<Guid>()); ;

            _mockDomainRespository.Setup(x => x.Find(command.IncentiveClaimApplicationId))
                .ReturnsAsync(incentiveApplication);

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            // Assert
            action.Should().Throw<InvalidRequestException>();
        }
    }
}
