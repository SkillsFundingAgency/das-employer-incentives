using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications
{
    public sealed class IncentiveApplication : AggregateRoot<Guid, IncentiveApplicationModel>
    {
        public long AccountId => Model.AccountId;
        public long AccountLegalEntityId => Model.AccountLegalEntityId;
        public DateTime DateCreated => Model.DateCreated;
        public IncentiveApplicationStatus Status => Model.Status;
        public DateTime? DateSubmitted => Model.DateSubmitted;
        public string SubmittedBy => Model.SubmittedBy;

        private readonly List<Apprenticeship> _apprenticeships = new List<Apprenticeship>();
        public ReadOnlyCollection<Apprenticeship> Apprenticeships => _apprenticeships.AsReadOnly();

        internal static IncentiveApplication New(Guid id, long accountId, long accountLegalEntityId)
        {
            return new IncentiveApplication(id, new IncentiveApplicationModel { Id = id, AccountId = accountId, AccountLegalEntityId = accountLegalEntityId, DateCreated = DateTime.Now, Status = IncentiveApplicationStatus.InProgress }, true);
        }

        internal static IncentiveApplication Get(Guid id, IncentiveApplicationModel model)
        {
            return new IncentiveApplication(id, model);
        }

        private IncentiveApplication(Guid id, IncentiveApplicationModel model, bool isNew = false) : base(id, model, isNew)
        {
            foreach (var apprenticeshipModel in model.ApprenticeshipModels.ToList())
            {
                _apprenticeships.Add(Apprenticeship.Create(apprenticeshipModel));
            }
        }

        public void AddApprenticeship(Apprenticeship apprenticeship)
        {
            _apprenticeships.Add(apprenticeship);
            Model.ApprenticeshipModels.Add(apprenticeship.GetModel());
        }

        public void RemoveApprenticeship(Apprenticeship apprenticeship)
        {
            _apprenticeships.Remove(apprenticeship);
            Model.ApprenticeshipModels.Remove(apprenticeship.GetModel());
        }

        public void Submit(DateTime submittedAt, string submittedBy)
        {
            Model.Status = IncentiveApplicationStatus.Submitted;
            Model.DateSubmitted = submittedAt;
            Model.SubmittedBy = submittedBy;
        }

    }
}
