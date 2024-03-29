﻿using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.SubmitIncentiveApplication;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            var command = new SubmitIncentiveApplicationCommand(incentiveApplication.Id, incentiveApplication.AccountId, _fixture.Create<DateTime>(), _fixture.Create<string>(), _fixture.Create<string>());

            _mockUlnValidationService.Setup(x => x.UlnAlreadyOnSubmittedIncentiveApplication(incentiveApplication.Apprenticeships[1].ULN)).ReturnsAsync(true);

            _mockDomainRepository.Setup(x => x.Find(command.IncentiveApplicationId))
                .ReturnsAsync(incentiveApplication);

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            // Assert
            action.Should().Throw<UlnAlreadySubmittedException>();
            _mockDomainRepository.Verify(m => m.Save(incentiveApplication), Times.Never);
        }

        [TestCase("2021-03-31", "2021-04-01", true, Phase.Phase2)]
        [TestCase("2021-04-01", "2021-04-01", false, Phase.Phase2)]
        [TestCase("2021-09-30", "2021-04-01", false, Phase.Phase2)]
        [TestCase("2021-10-01", "2021-04-01", true, Phase.Phase2)]
        [TestCase("2021-09-30", "2021-10-01", true, Phase.Phase3)]
        [TestCase("2021-10-01", "2021-10-01",false, Phase.Phase3)]
        [TestCase("2022-01-31", "2021-10-01", false, Phase.Phase3)]
        [TestCase("2022-02-01", "2021-10-01", true, Phase.Phase3)]
        public async Task Then_application_with_ineligible_employment_start_date_is_removed(DateTime employmentStartdate, DateTime commitmentStartDate, bool isRemoved, Phase phase)
        {
            //Arrange
            var apprentice = new Apprenticeship(Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<string>(),
                _fixture.Create<string>(), _fixture.Create<DateTime>(), _fixture.Create<long>(), commitmentStartDate,
                ApprenticeshipEmployerType.NonLevy, _fixture.Create<long>(), _fixture.Create<string>(), employmentStartdate, phase);

            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            incentiveApplication.SetApprenticeships(new List<Apprenticeship> { apprentice });

            var command = new SubmitIncentiveApplicationCommand(incentiveApplication.Id, incentiveApplication.AccountId, _fixture.Create<DateTime>(), _fixture.Create<string>(), _fixture.Create<string>());

            _mockDomainRepository.Setup(x => x.Find(command.IncentiveApplicationId))
                .ReturnsAsync(incentiveApplication);


            // Act
            await _sut.Handle(command);

            // Assert
            incentiveApplication.Status.Should().Be(IncentiveApplicationStatus.Submitted);            
            _mockDomainRepository.Verify(m => m.Save(incentiveApplication), Times.Once);

            // Assert
            if (isRemoved)
            {
                incentiveApplication.Apprenticeships.Count.Should().Be(0);
            }
            else
            {
                incentiveApplication.Apprenticeships.Count.Should().Be(1);
                incentiveApplication.Apprenticeships.Single().Id.Should().Be(apprentice.Id);
            }

        }
    }
}
