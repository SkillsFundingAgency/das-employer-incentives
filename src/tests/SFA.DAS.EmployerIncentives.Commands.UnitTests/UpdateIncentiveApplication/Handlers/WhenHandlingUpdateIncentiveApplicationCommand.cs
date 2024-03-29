﻿using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.UpdateIncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.UpdateIncentiveApplication.Handlers
{
    public class WhenHandlingUpdateIncentiveApplicationCommand
    {
        private UpdateIncentiveApplicationCommandHandler _sut;
        private Mock<IIncentiveApplicationFactory> _mockDomainFactory;
        private Mock<IIncentiveApplicationDomainRepository> _mockDomainRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _mockDomainFactory = new Mock<IIncentiveApplicationFactory>();
            _mockDomainRepository = new Mock<IIncentiveApplicationDomainRepository>();

            _sut = new UpdateIncentiveApplicationCommandHandler(_mockDomainFactory.Object, _mockDomainRepository.Object);
        }

        [Test]
        public async Task Then_the_existing_application_is_updated_in_the_domain_repository()
        {
            // Arrange
            var command = _fixture.Create<UpdateIncentiveApplicationCommand>();
            var existingApplicationModel = _fixture.Build<IncentiveApplicationModel>().With(p => p.Status, Enums.IncentiveApplicationStatus.InProgress).Create();
            var existingApplication = IncentiveApplication.Get(existingApplicationModel.Id, existingApplicationModel);
            existingApplication.SetApprenticeships(_fixture.CreateMany<Apprenticeship>());

            _mockDomainRepository.Setup(x => x.Find(command.IncentiveApplicationId)).ReturnsAsync(existingApplication);

            var expectedApprentices = new List<Apprenticeship>();
            foreach (var apprenticeship in command.Apprenticeships)
            {
                var apprentice = _fixture.Create<Apprenticeship>();
                _mockDomainFactory.Setup(x => x.CreateApprenticeship(
                    apprenticeship.ApprenticeshipId, apprenticeship.FirstName, apprenticeship.LastName,
                    apprenticeship.DateOfBirth, apprenticeship.ULN, apprenticeship.PlannedStartDate,
                    apprenticeship.ApprenticeshipEmployerTypeOnApproval, apprenticeship.UKPRN.Value, apprenticeship.CourseName, apprenticeship.EmploymentStartDate.Value)).Returns(apprentice);
                expectedApprentices.Add(apprentice);
            }

            // Act
            await _sut.Handle(command);

            // Assert
            existingApplication.Apprenticeships.Count.Should().Be(command.Apprenticeships.Count());
            existingApplication.Apprenticeships.Should().BeEquivalentTo(expectedApprentices);
            _mockDomainRepository.Verify(m => m.Save(existingApplication), Times.Once);
            _mockDomainRepository.VerifyAll();
        }

        [TestCase("Submitted")]
        [TestCase("EmployerWithdrawn")]
        [TestCase("ComplianceWithdrawn")]
        [TestCase("PaymentsPaused")]
        [TestCase("PaymentsResumed")]
        public async Task Then_the_existing_application_is_not_updated_when_status_is_not_inProgress(string statusText)
        {
            // Arrange
            var command = _fixture.Create<UpdateIncentiveApplicationCommand>();
            var existingApplicationModel = _fixture.Build<IncentiveApplicationModel>().With(p => p.Status, Enum.Parse<Enums.IncentiveApplicationStatus>(statusText)).Create();
            var existingApplication = IncentiveApplication.Get(existingApplicationModel.Id, existingApplicationModel);
            existingApplication.SetApprenticeships(_fixture.CreateMany<Apprenticeship>());

            _mockDomainRepository.Setup(x => x.Find(command.IncentiveApplicationId)).ReturnsAsync(existingApplication);

            // Act
            await _sut.Handle(command);

            // Assert
            existingApplication.Apprenticeships.Count.Should().Be(command.Apprenticeships.Count());
            _mockDomainRepository.Verify(m => m.Save(existingApplication), Times.Never);
            _mockDomainRepository.VerifyAll();
        }
    }
}
