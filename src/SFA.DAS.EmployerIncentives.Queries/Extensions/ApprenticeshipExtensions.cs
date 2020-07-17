using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Queries.Extensions
{
    internal static class ApprenticeshipExtensions
    {
        internal static ApprenticeshipDto ToApprenticeshipDto(this Apprenticeship valueObject)
        {
            return new ApprenticeshipDto
            {
                StartDate = valueObject.StartDate,
                IsApproved = valueObject.IsApproved,
                DateOfBirth = valueObject.DateOfBirth,
                UniqueLearnerNumber = valueObject.UniqueLearnerNumber,
                FirstName = valueObject.FirstName,
                LastName = valueObject.LastName
            };
        }
    }
}
