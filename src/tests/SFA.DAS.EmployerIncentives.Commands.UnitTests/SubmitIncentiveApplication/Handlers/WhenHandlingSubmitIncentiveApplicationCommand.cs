using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.SubmitIncentiveApplication;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using IncentiveApplication = SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.IncentiveApplication;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.SubmitIncentiveApplication.Handlers
{
    public class WhenHandlingSubmitIncentiveApplicationCommand
    {
        private SubmitIncentiveApplicationCommandHandler _sut;
        private Mock<IIncentiveApplicationDomainRepository> _mockDomainRepository;
        private Mock<IUlnValidationService> _mockUlnValidationService;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _mockDomainRepository = new Mock<IIncentiveApplicationDomainRepository>();
            _mockUlnValidationService = new Mock<IUlnValidationService>();
            _mockUlnValidationService.Setup(x => x.UlnAlreadyOnSubmittedIncentiveApplication(It.IsAny<long>())).ReturnsAsync(false);

            _sut = new SubmitIncentiveApplicationCommandHandler(_mockDomainRepository.Object, _mockUlnValidationService.Object);
        }

        [Test]
        public async Task Then_a_valid_application_is_updated_in_the_domain_repository()
        {
            //Arrange
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            var command = new SubmitIncentiveApplicationCommand(incentiveApplication.Id, incentiveApplication.AccountId, _fixture.Create<DateTime>(), _fixture.Create<string>(), _fixture.Create<string>());

            _mockDomainRepository.Setup(x => x.Find(command.IncentiveApplicationId))
                .ReturnsAsync(incentiveApplication);

            // Act
            await _sut.Handle(command);

            // Assert
            incentiveApplication.Status.Should().Be(IncentiveApplicationStatus.Submitted);
            incentiveApplication.DateSubmitted.Should().Be(command.DateSubmitted);
            incentiveApplication.SubmittedByEmail.Should().Be(command.SubmittedByEmail);
            incentiveApplication.SubmittedByName.Should().Be(command.SubmittedByName);            

            _mockDomainRepository.Verify(m => m.Save(incentiveApplication), Times.Once);
        }

        [Test]
        public void Then_an_application_with_an_invalid_application_id_is_rejected()
        {
            //Arrange
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            var command = new SubmitIncentiveApplicationCommand(_fixture.Create<Guid>(), incentiveApplication.AccountId, _fixture.Create<DateTime>(), _fixture.Create<string>(), _fixture.Create<string>());

            IncentiveApplication nullResponse = null;
            _mockDomainRepository.Setup(x => x.Find(command.IncentiveApplicationId))
                .ReturnsAsync(nullResponse);

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            // Assert
            action.Should().Throw<InvalidRequestException>();
            _mockDomainRepository.Verify(m => m.Save(incentiveApplication), Times.Never);
        }

        [Test]
        public void Then_an_application_with_an_invalid_account_id_is_rejected()
        {
            //Arrange
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            var command = new SubmitIncentiveApplicationCommand(incentiveApplication.Id, _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<string>(), _fixture.Create<string>());

            _mockDomainRepository.Setup(x => x.Find(command.IncentiveApplicationId))
                .ReturnsAsync(incentiveApplication);

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            // Assert
            action.Should().Throw<InvalidRequestException>();
            _mockDomainRepository.Verify(m => m.Save(incentiveApplication), Times.Never);
        }

        [Test]
        public void Then_an_application_with_an_already_submitted_uln_is_rejected()
        {
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            incentiveApplication.SetApprenticeships(_fixture.CreateMany<Apprenticeship>(3));
            var command = new SubmitIncentiveApplicationCommand(incentiveApplication.Id, _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<string>(), _fixture.Create<string>());

            _mockDomainRepository.Setup(x => x.Find(command.IncentiveApplicationId))
                .ReturnsAsync(incentiveApplication);

            _mockUlnValidationService.Setup(x => x.UlnAlreadyOnSubmittedIncentiveApplication(incentiveApplication.Apprenticeships[1].ULN)).ReturnsAsync(true);

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            // Assert
            action.Should().Throw<InvalidRequestException>();
            _mockDomainRepository.Verify(m => m.Save(incentiveApplication), Times.Never);
        }
    }
}
