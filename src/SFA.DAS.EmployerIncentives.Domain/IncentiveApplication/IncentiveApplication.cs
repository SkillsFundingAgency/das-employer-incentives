using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplication.Models;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplication
{
    public sealed class IncentiveApplication : AggregateRoot<Guid, IncentiveApplicationModel>
    {
        public long AccountId => Model.AccountId;
        public long AccountLegalEntityId => Model.AccountLegalEntityId;
        public DateTime DateCreated => Model.DateCreated;
        public IncentiveApplicationStatus Status => Model.Status;

        private readonly List<Apprenticeship> _apprenticeships = new List<Apprenticeship>();
        public ReadOnlyCollection<Apprenticeship> Apprenticeships => _apprenticeships.AsReadOnly();

        public static IncentiveApplication New(Guid id, long accountId, long accountLegalEntityId)
        {
            return new IncentiveApplication(id, new IncentiveApplicationModel { Id = id, AccountId = accountId, AccountLegalEntityId = accountLegalEntityId, DateCreated = DateTime.Now, Status = IncentiveApplicationStatus.InProgress }, true);
        }

        private IncentiveApplication(Guid id, IncentiveApplicationModel model, bool isNew = false) : base(id, model, isNew)
        {
        }

        public void AddApprenticeship(Apprenticeship apprenticeship)
        {
            _apprenticeships.Add(apprenticeship);
            Model.ApprenticeshipModels.Add(apprenticeship.GetModel());
        }
    }
}
