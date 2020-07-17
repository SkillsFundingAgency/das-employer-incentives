using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Domain.Services;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetEligibleApprenticeships;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.NewApprenticeIncentive.Handlers
{
    public class WhenHandlingGetGetEligibleApprenticeships
    {
        private GetEligibleApprenticeshipsQueryHandler _sut;
        private Mock<INewApprenticeIncentiveEligibilityService> _eligibilityService;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _eligibilityService = new Mock<INewApprenticeIncentiveEligibilityService>();
            _sut = new GetEligibleApprenticeshipsQueryHandler(_eligibilityService.Object);
        }

        [Test]
        public async Task Then_eligible_apprentices_are_returned()
        {
            //Arrange
            var query = _fixture.Create<GetEligibleApprenticeshipsRequest>();
            var data = _fixture.CreateMany<Apprenticeship>().ToList();
            
            _eligibilityService.Setup(x => x.GetEligibileApprenticeships(It.Is<IEnumerable<Apprenticeship>>(y => ApprenticeshipsMatchDtos(new List<ApprenticeshipDto>(query.Apprenticeships), new List<Apprenticeship>(y))))).Returns(data);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.EligibleApprenticeships.Should().BeEquivalentTo(data);
        }

        private bool ApprenticeshipsMatchDtos(List<ApprenticeshipDto> dtos, List<Apprenticeship> valueObjects)
        {
            if (dtos.Count() != valueObjects.Count())
            {
                return false;
            }

            return dtos.All(x => ApprenticeshipMatchesDto(x, valueObjects[dtos.IndexOf(x)]));
        }

        private bool ApprenticeshipMatchesDto(ApprenticeshipDto dto, Apprenticeship valueObject)
        {
            return dto.IsApproved == valueObject.IsApproved &&
                   dto.DateOfBirth == valueObject.DateOfBirth &&
                   dto.FirstName == valueObject.FirstName &&
                   dto.LastName == valueObject.LastName &&
                   dto.StartDate == valueObject.StartDate &&
                   dto.UniqueLearnerNumber == valueObject.UniqueLearnerNumber;
        }
    }
}
