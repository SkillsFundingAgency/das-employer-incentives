using SFA.DAS.EmployerIncentives.DataTransferObjects;
using SFA.DAS.EmployerIncentives.ValueObjects;
using Apprenticeship = SFA.DAS.EmployerIncentives.DataTransferObjects.Apprenticeship;

namespace SFA.DAS.EmployerIncentives.Queries.Extensions
{
    internal static class ApprenticeshipDtoExtensions
    {
        internal static ValueObjects.Apprenticeship ToApprenticeship(this Apprenticeship dto)
        {
            return new ValueObjects.Apprenticeship(dto.UniqueLearnerNumber, dto.StartDate, dto.IsApproved);
        }
    }
}
