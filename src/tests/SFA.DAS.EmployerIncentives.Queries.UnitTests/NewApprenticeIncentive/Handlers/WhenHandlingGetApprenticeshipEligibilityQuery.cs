using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.DataTransferObjects;
using SFA.DAS.EmployerIncentives.Domain.Services;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApprenticeshipEligibility;
using SFA.DAS.EmployerIncentives.ValueObjects;
using Apprenticeship = SFA.DAS.EmployerIncentives.DataTransferObjects.Apprenticeship;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.NewApprenticeIncentive.Handlers
{
    public class WhenHandlingGetApprenticeshipEligibilityQuery
    {
        private GetApprenticeshipEligibilityQueryHandler _sut;
        private Mock<INewApprenticeIncentiveEligibilityService> _eligibilityService;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _eligibilityService = new Mock<INewApprenticeIncentiveEligibilityService>();
            _sut = new GetApprenticeshipEligibilityQueryHandler(_eligibilityService.Object);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Then_apprenticeship_eligibility_is_returned(bool isEligible)
        {
            //Arrange
            var query = _fixture.Create<GetApprenticeshipEligibilityRequest>();
            
            _eligibilityService.Setup(x => x.IsApprenticeshipEligible(It.Is<ValueObjects.Apprenticeship>(y => ApprenticeshipMatchesDto(query.Apprenticeship, y)))).ReturnsAsync(isEligible);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.IsEligible.Should().Be(isEligible);
        }

        private bool ApprenticeshipMatchesDto(Apprenticeship dto, ValueObjects.Apprenticeship valueObject)
        {
            return dto.IsApproved == valueObject.IsApproved &&
                   dto.StartDate == valueObject.StartDate &&
                   dto.UniqueLearnerNumber == valueObject.UniqueLearnerNumber;
        }
    }
}
