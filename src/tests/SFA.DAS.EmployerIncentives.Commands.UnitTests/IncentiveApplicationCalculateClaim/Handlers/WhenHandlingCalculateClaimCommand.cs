using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Extensions;
using SFA.DAS.EmployerIncentives.Commands.IncentiveApplicationCalculateClaim;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;
using SFA.DAS.EmployerIncentives.Domain.Factories;
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
        private Mock<IIncentivePaymentProfilesService> _mockPaymentProfilesService;
        private List<Domain.ValueObjects.IncentivePaymentProfile> _incentivePaymentProfiles;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());
            _incentivePaymentProfiles = new List<Domain.ValueObjects.IncentivePaymentProfile>
            {
                new Domain.ValueObjects.IncentivePaymentProfile(IncentiveType.TwentyFiveOrOverIncentive,
                    new List<Domain.ValueObjects.PaymentProfile> {new Domain.ValueObjects.PaymentProfile(90, 750)}),
                new Domain.ValueObjects.IncentivePaymentProfile(IncentiveType.UnderTwentyFiveIncentive,
                    new List<Domain.ValueObjects.PaymentProfile> {new Domain.ValueObjects.PaymentProfile(90, 1000)})
            };

            _mockDomainRespository = new Mock<IIncentiveApplicationDomainRepository>();
            _mockPaymentProfilesService = new Mock<IIncentivePaymentProfilesService>();
            _mockPaymentProfilesService.Setup(x => x.MapToDomainIncentivePaymentProfiles())
                .Returns(_incentivePaymentProfiles);

            _sut = new CalculateClaimCommandHandler(_mockDomainRespository.Object, _mockPaymentProfilesService.Object);
        }

        [Test]
        public async Task Then_the_EarningsCalculationRequestedEvents_are_raised_for_each_apprenticeship_on_application()
        {
            //Arrange
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            var command = new CalculateClaimCommand(incentiveApplication.AccountId, _fixture.Create<Guid>());

            _mockDomainRespository.Setup(x => x.Find(command.IncentiveClaimApplicationId)).ReturnsAsync(incentiveApplication);

            // Act
            await _sut.Handle(command);

            // Assert
            var events = incentiveApplication.FlushEvents().OfType<EarningsCalculationRequested>().ToList();
            events.Count.Should().Be(incentiveApplication.Apprenticeships.Count);
        }

        [TestCase(24, IncentiveType.UnderTwentyFiveIncentive)]
        [TestCase(25, IncentiveType.TwentyFiveOrOverIncentive)]
        public async Task And_there_is_one_X_years_old_on_application_Then_the_EarningsCalculationRequestedEvent_is_raised_for_this_apprenticeship_with_expected_incentive_type(int age, IncentiveType expected)
        {
            //Arrange
            var application = CreateTestApplication(age);
            var command = new CalculateClaimCommand(application.AccountId, _fixture.Create<Guid>());
            _mockDomainRespository.Setup(x => x.Find(command.IncentiveClaimApplicationId)).ReturnsAsync(application);

            // Act
            await _sut.Handle(command);

            // Assert
            var events = application.FlushEvents().OfType<EarningsCalculationRequested>().ToList();
            events.Count.Should().Be(1);
            events[0].IncentiveType.Should().Be(expected);
            events[0].ApprenticeshipId.Should().Be(application.Apprenticeships[0].ApprenticeshipId);
            events[0].AccountId.Should().Be(application.AccountId);
            events[0].ApprenticeshipStartDate.Should().Be(application.Apprenticeships[0].PlannedStartDate);
            events[0].IncentiveClaimApprenticeshipId.Should().Be(application.Apprenticeships[0].Id);
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

        private IncentiveApplication CreateTestApplication(int age)
        {
            var today = DateTime.Today;

            IncentiveApplicationApprenticeshipDto apprentice = _fixture.Create<IncentiveApplicationApprenticeshipDto>();
            apprentice.DateOfBirth = today.AddYears(-1 * age);
            apprentice.PlannedStartDate = today.AddDays(1);
            var list = new List<IncentiveApplicationApprenticeshipDto>();
            list.Add(apprentice);

            var factory = new IncentiveApplicationFactory();
            var application = factory.CreateNew(_fixture.Create<Guid>(), _fixture.Create<long>(), _fixture.Create<long>());
            application.SetApprenticeships(list.ToEntities(factory));

            return application;
        }
    }
}
