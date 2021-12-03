using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.EmploymentCheck
{
    public class WhenHandlingUpdateEmploymentCheckCommand
    {
        private UpdateEmploymentCheckCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRespository;
        private Fixture _fixture;
        private Guid _correlationId;
        private ApprenticeshipIncentiveModel _apprenticeshipIncentiveModel;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _incentive;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _correlationId = Guid.NewGuid();

            _apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .Without(a => a.EmploymentCheckModels)
                .Create();

            _incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(_apprenticeshipIncentiveModel.Id, _apprenticeshipIncentiveModel);

            _mockIncentiveDomainRespository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            
            _mockIncentiveDomainRespository
                .Setup(m => m.FindByEmploymentCheckId(_correlationId))
                .ReturnsAsync(_incentive);

            _sut = new UpdateEmploymentCheckCommandHandler(_mockIncentiveDomainRespository.Object);            
        }

        [TestCase("EmployedBeforeSchemeStarted")]
        [TestCase("EmployedAtStartOfApprenticeship")]
        public async Task Then_an_existing_employment_check_result_is_updated(string checkTypeString)
        {
            var checkType = Enum.Parse<EmploymentCheckType>(checkTypeString);
            
            //Arrange
            var existingEmploymentCheck = new EmploymentCheckModel
            {
                Id = Guid.NewGuid(),
                ApprenticeshipIncentiveId = _apprenticeshipIncentiveModel.Id,
                CheckType = checkType,
                CorrelationId = _correlationId,
                MinimumDate = DateTime.Now.AddDays(-30),
                MaximumDate = DateTime.Now.AddDays(-10),
                CreatedDateTime = DateTime.Today.AddDays(-5),
                UpdatedDateTime = null,
                Result = false,
                ResultDateTime = null
            };

            _apprenticeshipIncentiveModel.EmploymentCheckModels.Add(existingEmploymentCheck);

            // Act
            await _sut.Handle(new UpdateEmploymentCheckCommand(existingEmploymentCheck.CorrelationId, EmploymentCheckResultType.Employed, DateTime.Today));

            // Assert
            var savedEmploymentCheck = _incentive.GetModel().EmploymentCheckModels.Single(e=> e.CorrelationId == existingEmploymentCheck.CorrelationId);
            savedEmploymentCheck.Result.Should().BeTrue();
            savedEmploymentCheck.ResultDateTime.Should().Be(DateTime.Today);            

            _mockIncentiveDomainRespository.Verify(m => m.Save(_incentive), Times.Once);
        }

        [Test()]        
        public async Task Then_an_existing_employment_check_result_is_not_updated_when_a_check_with_the_correlationId_does_not_exist()
        {
            //Arrange
            var existingEmploymentCheck = new EmploymentCheckModel
            {
                Id = Guid.NewGuid(),
                ApprenticeshipIncentiveId = _apprenticeshipIncentiveModel.Id,
                CheckType = EmploymentCheckType.EmployedAtStartOfApprenticeship,
                CorrelationId = _correlationId,
                MinimumDate = DateTime.Now.AddDays(-30),
                MaximumDate = DateTime.Now.AddDays(-10),
                CreatedDateTime = DateTime.Today.AddDays(-5),
                Result = false,
                ResultDateTime = DateTime.Today.AddDays(-5),
            };

            _apprenticeshipIncentiveModel.EmploymentCheckModels.Add(existingEmploymentCheck);

            // Act
            await _sut.Handle(new UpdateEmploymentCheckCommand(Guid.NewGuid(), EmploymentCheckResultType.Employed, DateTime.Today));

            // Assert
            var savedEmploymentCheck = _incentive.GetModel().EmploymentCheckModels.Single(e => e.CorrelationId == existingEmploymentCheck.CorrelationId);
            savedEmploymentCheck.Result.Should().BeFalse();
            savedEmploymentCheck.ResultDateTime.Should().Be(DateTime.Today.AddDays(-5));
        }

        [Test()]
        public async Task Then_an_existing_employment_check_result_is_not_updated_for_an_earlier_ResultDateTime_for_the_same_correlation_id()
        {
            //Arrange
            var existingResultDateTime = DateTime.Now.AddDays(-5);
            var existingEmploymentCheck = new EmploymentCheckModel
            {
                Id = Guid.NewGuid(),
                ApprenticeshipIncentiveId = _apprenticeshipIncentiveModel.Id,
                CheckType = EmploymentCheckType.EmployedAtStartOfApprenticeship,
                CorrelationId = _correlationId,
                MinimumDate = DateTime.Now.AddDays(-30),
                MaximumDate = DateTime.Now.AddDays(-10),
                CreatedDateTime = DateTime.Today.AddDays(-5),
                Result = false,
                ResultDateTime = existingResultDateTime
            };

            _apprenticeshipIncentiveModel.EmploymentCheckModels.Add(existingEmploymentCheck);

            // Act
            await _sut.Handle(new UpdateEmploymentCheckCommand(existingEmploymentCheck.CorrelationId, EmploymentCheckResultType.Employed, existingResultDateTime.AddDays(-5).AddMinutes(-1)));

            // Assert
            var savedEmploymentCheck = _incentive.GetModel().EmploymentCheckModels.Single(e => e.CorrelationId == existingEmploymentCheck.CorrelationId);
            savedEmploymentCheck.Result.Should().BeFalse();
            savedEmploymentCheck.ResultDateTime.Should().Be(existingResultDateTime);
        }
    }
}
