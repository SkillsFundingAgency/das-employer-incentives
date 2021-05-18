using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.RemoveIneligibleApprenticesFromApplication;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.RemoveIneligibleApprenticesFromApplication.Handlers
{
    public class WhenHandlingRemoveIneligibleApprenticesFromApplication
    {
        private RemoveIneligibleApprenticesFromApplicationCommandHandler _sut;
        private Mock<IIncentiveApplicationDomainRepository> _mockDomainRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _mockDomainRepository = new Mock<IIncentiveApplicationDomainRepository>();

            _sut = new RemoveIneligibleApprenticesFromApplicationCommandHandler(_mockDomainRepository.Object);
        }

        [Test]
        public async Task Then_a_valid_application_is_updated_in_the_domain_repository()
        {
            //Arrange
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            var apprenticeships = _fixture.CreateMany<Apprenticeship>(3).ToList();
            var ineligibleApprenticeship = new IncentiveApplicationFactory().CreateApprenticeship(_fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), _fixture.Create<ApprenticeshipEmployerType>(),
                _fixture.Create<long>(), _fixture.Create<string>(), new DateTime(2021, 03, 01));
            apprenticeships.Add(ineligibleApprenticeship);
            incentiveApplication.SetApprenticeships(apprenticeships);

            var command = new RemoveIneligibleApprenticesFromApplicationCommand(incentiveApplication.Id, incentiveApplication.AccountId);

            _mockDomainRepository.Setup(x => x.Find(command.IncentiveApplicationId)).ReturnsAsync(incentiveApplication);

            // Act
            await _sut.Handle(command);

            // Assert
            incentiveApplication.Apprenticeships.Count.Should().Be(3);

            _mockDomainRepository.Verify(m => m.Save(incentiveApplication), Times.Once);
        }

        [Test]
        public void Then_an_application_with_an_invalid_application_id_is_rejected()
        {
            //Arrange
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            var command = new RemoveIneligibleApprenticesFromApplicationCommand(_fixture.Create<Guid>(), incentiveApplication.AccountId);

            IncentiveApplication nullResponse = null;
            _mockDomainRepository.Setup(x => x.Find(command.IncentiveApplicationId)).ReturnsAsync(nullResponse);

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
            var command =
                new RemoveIneligibleApprenticesFromApplicationCommand(incentiveApplication.Id, _fixture.Create<long>());

            _mockDomainRepository.Setup(x => x.Find(command.IncentiveApplicationId)).ReturnsAsync(incentiveApplication);

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            // Assert
            action.Should().Throw<InvalidRequestException>();
            _mockDomainRepository.Verify(m => m.Save(incentiveApplication), Times.Never);
        }
    }
}
