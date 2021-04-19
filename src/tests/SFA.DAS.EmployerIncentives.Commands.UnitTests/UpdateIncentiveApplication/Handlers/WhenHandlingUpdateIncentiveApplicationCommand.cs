using System;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.UpdateIncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

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
            var commandApprenticeships = new List<IncentiveApplicationApprenticeshipDto>();
            for (int i = 0; i < 3; i++)
            {
                var apprentice = _fixture.Create<IncentiveApplicationApprenticeshipDto>();
                apprentice.ULN = 1000 + i;
                apprentice.Selected = true;
                commandApprenticeships.Add(apprentice);
            }

            var command = new UpdateIncentiveApplicationCommand(Guid.NewGuid(), _fixture.Create<long>(), commandApprenticeships);

            var existingApplication = _fixture.Create<IncentiveApplication>();
            var existingApprenticeships = new List<Apprenticeship>();
            for(int i = 0; i < 3; i++)
            {
                var model = _fixture.Create<ApprenticeshipModel>();
                model.ULN = commandApprenticeships[i].ULN;
                var apprenticeship = Apprenticeship.Create(model);
                existingApprenticeships.Add(apprenticeship);
            }
            existingApplication.SetApprenticeships(existingApprenticeships);

            _mockDomainRepository.Setup(x => x.Find(command.IncentiveApplicationId)).ReturnsAsync(existingApplication);

            var expectedApprentices = new List<Apprenticeship>();
            foreach (var apprenticeship in command.Apprenticeships)
            {
                var model = _fixture.Create<ApprenticeshipModel>();
                model.ULN = apprenticeship.ULN;
                var apprentice = Apprenticeship.Create(model);
                _mockDomainFactory.Setup(x => x.CreateApprenticeship(
                    apprenticeship.ApprenticeshipId, apprenticeship.FirstName, apprenticeship.LastName,
                    apprenticeship.DateOfBirth, apprenticeship.ULN, apprenticeship.PlannedStartDate,
                    apprenticeship.ApprenticeshipEmployerTypeOnApproval, apprenticeship.UKPRN.Value, apprenticeship.CourseName)).Returns(apprentice);
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

        [Test]
        public async Task Then_the_previous_selection_is_modified_and_new_apprenticeships_selected()
        {
            // Arrange
            var commandApprenticeships = new List<IncentiveApplicationApprenticeshipDto>();
            for (int i = 0; i < 5; i++)
            {
                var apprentice = _fixture.Create<IncentiveApplicationApprenticeshipDto>();
                apprentice.ULN = 1000 + i;
                apprentice.Selected = true;
                commandApprenticeships.Add(apprentice);
            }
            commandApprenticeships[1].Selected = false;

            var command = new UpdateIncentiveApplicationCommand(Guid.NewGuid(), _fixture.Create<long>(), commandApprenticeships);

            var existingApplication = _fixture.Create<IncentiveApplication>();
            var existingApprenticeships = new List<Apprenticeship>();
            for (int i = 0; i < 3; i++)
            {
                var model = _fixture.Create<ApprenticeshipModel>();
                model.ULN = commandApprenticeships[i].ULN;
                var apprenticeship = Apprenticeship.Create(model);
                existingApprenticeships.Add(apprenticeship);
            }
            existingApplication.SetApprenticeships(existingApprenticeships);

            _mockDomainRepository.Setup(x => x.Find(command.IncentiveApplicationId)).ReturnsAsync(existingApplication);

            var expectedApprentices = new List<Apprenticeship>();
            foreach (var apprenticeship in command.Apprenticeships)
            {
                var model = _fixture.Create<ApprenticeshipModel>();
                model.ULN = apprenticeship.ULN;
                var apprentice = Apprenticeship.Create(model);
                _mockDomainFactory.Setup(x => x.CreateApprenticeship(
                    apprenticeship.ApprenticeshipId, apprenticeship.FirstName, apprenticeship.LastName,
                    apprenticeship.DateOfBirth, apprenticeship.ULN, apprenticeship.PlannedStartDate,
                    apprenticeship.ApprenticeshipEmployerTypeOnApproval, apprenticeship.UKPRN.Value, apprenticeship.CourseName)).Returns(apprentice);
                expectedApprentices.Add(apprentice);
            }

            // Act
            await _sut.Handle(command);

            // Assert
            existingApplication.Apprenticeships.Count.Should().Be(4);
            existingApplication.Apprenticeships.FirstOrDefault(x => x.ULN == 1001).Should().BeNull();
            existingApplication.Apprenticeships.FirstOrDefault(x => x.ULN == 1003).Should().NotBeNull();
            existingApplication.Apprenticeships.FirstOrDefault(x => x.ULN == 1004).Should().NotBeNull();
            _mockDomainRepository.Verify(m => m.Save(existingApplication), Times.Once);
            _mockDomainRepository.VerifyAll();
        }

        [Test]
        public async Task Then_the_previous_selection_is_retained_and_new_apprenticeships_selected()
        {
            // Arrange
            var commandApprenticeships = new List<IncentiveApplicationApprenticeshipDto>();
            for (int i = 0; i < 3; i++)
            {
                var apprentice = _fixture.Create<IncentiveApplicationApprenticeshipDto>();
                apprentice.ULN = 2000 + i;
                apprentice.Selected = true;
                commandApprenticeships.Add(apprentice);
            }

            var command = new UpdateIncentiveApplicationCommand(Guid.NewGuid(), _fixture.Create<long>(), commandApprenticeships);

            var existingApplication = _fixture.Create<IncentiveApplication>();
            var existingApprenticeships = new List<Apprenticeship>();
            for (int i = 0; i < 3; i++)
            {
                var model = _fixture.Create<ApprenticeshipModel>();
                model.ULN = 1000 + i;
                var apprenticeship = Apprenticeship.Create(model);
                existingApprenticeships.Add(apprenticeship);
            }
            existingApplication.SetApprenticeships(existingApprenticeships);

            _mockDomainRepository.Setup(x => x.Find(command.IncentiveApplicationId)).ReturnsAsync(existingApplication);

            var expectedApprentices = new List<Apprenticeship>();
            foreach (var apprenticeship in command.Apprenticeships)
            {
                var model = _fixture.Create<ApprenticeshipModel>();
                model.ULN = apprenticeship.ULN;
                var apprentice = Apprenticeship.Create(model);
                _mockDomainFactory.Setup(x => x.CreateApprenticeship(
                    apprenticeship.ApprenticeshipId, apprenticeship.FirstName, apprenticeship.LastName,
                    apprenticeship.DateOfBirth, apprenticeship.ULN, apprenticeship.PlannedStartDate,
                    apprenticeship.ApprenticeshipEmployerTypeOnApproval, apprenticeship.UKPRN.Value, apprenticeship.CourseName)).Returns(apprentice);
                expectedApprentices.Add(apprentice);
            }

            // Act
            await _sut.Handle(command);

            // Assert
            existingApplication.Apprenticeships.Count.Should().Be(6);
            existingApplication.Apprenticeships.FirstOrDefault(x => x.ULN == 1000).Should().NotBeNull();
            existingApplication.Apprenticeships.FirstOrDefault(x => x.ULN == 1001).Should().NotBeNull();
            existingApplication.Apprenticeships.FirstOrDefault(x => x.ULN == 1002).Should().NotBeNull();
            existingApplication.Apprenticeships.FirstOrDefault(x => x.ULN == 2000).Should().NotBeNull();
            existingApplication.Apprenticeships.FirstOrDefault(x => x.ULN == 2001).Should().NotBeNull();
            existingApplication.Apprenticeships.FirstOrDefault(x => x.ULN == 2002).Should().NotBeNull();
            _mockDomainRepository.Verify(m => m.Save(existingApplication), Times.Once);
            _mockDomainRepository.VerifyAll();
        }
    }
}

