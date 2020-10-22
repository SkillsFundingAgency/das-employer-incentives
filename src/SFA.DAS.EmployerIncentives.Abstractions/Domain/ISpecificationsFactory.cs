using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Abstractions.Domain
{
    public interface ISpecificationsFactory<T>
    {
        IEnumerable<Specification<T>> Rules { get; }
    }
}
