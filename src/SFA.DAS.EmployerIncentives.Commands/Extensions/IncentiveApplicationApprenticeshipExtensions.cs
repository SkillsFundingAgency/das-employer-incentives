using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Commands.Extensions
{
    public static class IncentiveApplicationApprenticeshipExtensions
    {
        public static IEnumerable<Apprenticeship> ToEntities(this IEnumerable<IncentiveApplicationApprenticeshipDto> dto, IIncentiveApplicationFactory factory)
        {
            return dto.Select(
                apprenticeship => factory.CreateApprenticeship(
                    apprenticeship.ApprenticeshipId, apprenticeship.FirstName, apprenticeship.LastName,
                    apprenticeship.DateOfBirth, apprenticeship.ULN, apprenticeship.PlannedStartDate,
                    apprenticeship.ApprenticeshipEmployerTypeOnApproval,
                    apprenticeship.UKPRN, apprenticeship.CourseName, apprenticeship.EmploymentStartDate)
            );
        }
    }
}