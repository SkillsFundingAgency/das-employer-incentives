using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Queries.Extensions
{
    internal static class ApprenticeshipDtoExtensions
    {
        internal static Apprenticeship ToApprenticeship(this ApprenticeshipDto dto)
        {
            return new Apprenticeship(dto.UniqueLearnerNumber, dto.FirstName, dto.LastName, dto.DateOfBirth, dto.StartDate, dto.IsApproved);
        }
    }
}
