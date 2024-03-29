﻿using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface ICollectionPeriodDataRepository
    {
        Task<IEnumerable<CollectionCalendarPeriod>> GetAll();
        Task Save(IEnumerable<CollectionCalendarPeriod> collectionPeriods);
    }
}
