using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class ChangeOfCircumstance : ValueObject
    {
        public Guid Id { get; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public ChangeOfCircumstanceType Type { get; set; }
        public string PreviousValue { get; set; }
        public string NewValue { get; set; }
        public DateTime ChangedDate { get; set; }
        public CollectionPeriod PreviousCollectionPeriod { get; set; }

        public ChangeOfCircumstance(
            Guid id,
            Guid apprenticeshipIncentiveId,
            ChangeOfCircumstanceType type,
            string previousValue,            
            string newValue,
            DateTime changedDate)
        {
            Id = id;
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            Type = type;
            PreviousValue = previousValue;
            NewValue = newValue;
            ChangedDate = changedDate;
        }

        public void SetPreviousCollectionPeriod(CollectionPeriod previousCollectionPeriod)
        {
            PreviousCollectionPeriod = previousCollectionPeriod;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Id;
            yield return ApprenticeshipIncentiveId;
            yield return Type;
            yield return PreviousValue;
            yield return PreviousCollectionPeriod;
            yield return NewValue;
        }
    }
}
