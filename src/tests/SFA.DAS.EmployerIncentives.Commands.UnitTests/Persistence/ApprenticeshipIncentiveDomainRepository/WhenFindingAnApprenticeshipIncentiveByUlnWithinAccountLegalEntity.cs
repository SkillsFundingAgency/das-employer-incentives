using System;
using System.Collections.Generic;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Persistence.ApprenticeshipIncentiveDomainRepository
{
    public class WhenFindingAnApprenticeshipIncentiveByUlnWithinAccountLegalEntity
    {
        private Commands.Persistence.ApprenticeshipIncentiveDomainRepository _sut;
        private Mock<IApprenticeshipIncentiveDataRepository> _mockApprenticeshipIncentiveDataRepository;
        private Mock<IPaymentDataRepository> _mockPaymentDataRepository;
        private Mock<IApprenticeshipIncentiveFactory> _mockApprenticeshipIncentiveFactory;
        private Mock<IDomainEventDispatcher> _mockDomainEventDispatcher;
        private long _accountLegalEntityId;
        private long _uln;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new ApprenticeshipIncentiveCustomization());

            _accountLegalEntityId = _fixture.Create<long>();
            _uln = _fixture.Create<long>();


            _mockApprenticeshipIncentiveDataRepository = new Mock<IApprenticeshipIncentiveDataRepository>();
            _mockPaymentDataRepository = new Mock<IPaymentDataRepository>();
            _mockApprenticeshipIncentiveFactory = new Mock<IApprenticeshipIncentiveFactory>();
            _mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();

            _sut = new Commands.Persistence.ApprenticeshipIncentiveDomainRepository(
                _mockApprenticeshipIncentiveDataRepository.Object,
                _mockPaymentDataRepository.Object,
                _mockApprenticeshipIncentiveFactory.Object, 
                _mockDomainEventDispatcher.Object);
        }

        [Test]
        public async Task Then_when_a_single_match_is_found_an_apprenticeship_incentive_is_returned()
        {
            //Arrange
            var model = _fixture.Create<ApprenticeshipIncentiveModel>();
            var instance = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            _mockApprenticeshipIncentiveDataRepository
                .Setup(x => x.FindApprenticeshipIncentiveByUlnWithinAccountLegalEntity(_uln, _accountLegalEntityId))
                .ReturnsAsync(new List<ApprenticeshipIncentiveModel> {model});

            _mockApprenticeshipIncentiveFactory.Setup(x => x.GetExisting(model.Id, model))
                .Returns(instance);

            //Act
            var apprenticeshipIncentive = await _sut.FindByUlnWithinAccountLegalEntity(_uln, _accountLegalEntityId);

            //Assert
            apprenticeshipIncentive.Should().Be(instance);
        }

        [Test]
        public async Task Then_when_no_matches_are_found_null_is_returned()
        {
            //Arrange
            _mockApprenticeshipIncentiveDataRepository
                .Setup(x => x.FindApprenticeshipIncentiveByUlnWithinAccountLegalEntity(_uln, _accountLegalEntityId))
                .ReturnsAsync(new List<ApprenticeshipIncentiveModel>());

            //Act
            var apprenticeshipIncentive = await _sut.FindByUlnWithinAccountLegalEntity(_uln, _accountLegalEntityId);

            //Assert
            apprenticeshipIncentive.Should().BeNull();
        }

        [Test]
        public async Task Then_when_multiple_matches_exist_it_throws_is_InvalidIncentiveException()
        {
            // Arrange
            _mockApprenticeshipIncentiveDataRepository
                .Setup(x => x.FindApprenticeshipIncentiveByUlnWithinAccountLegalEntity(_uln, _accountLegalEntityId))
                .ReturnsAsync(new List<ApprenticeshipIncentiveModel> {
                    _fixture.Create<ApprenticeshipIncentiveModel>(),
                    _fixture.Create<ApprenticeshipIncentiveModel>()
                });

            // Act
            Func<Task> act = async () => await _sut.FindByUlnWithinAccountLegalEntity(_uln, _accountLegalEntityId);

            // Assert
            act.Should().Throw<InvalidIncentiveException>();
        }        
    }
}
