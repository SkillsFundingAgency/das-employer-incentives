using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.EarningsResilienceCheck.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications
{
    public sealed class IncentiveApplication : AggregateRoot<Guid, IncentiveApplicationModel>
    {
        public long AccountId => Model.AccountId;
        public long AccountLegalEntityId => Model.AccountLegalEntityId;
        public DateTime DateCreated => Model.DateCreated;
        public IncentiveApplicationStatus Status => Model.Status;
        public DateTime? DateSubmitted => Model.DateSubmitted;
        public string SubmittedByEmail => Model.SubmittedByEmail;
        public string SubmittedByName => Model.SubmittedByName;

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

        public void Submit(DateTime submittedAt, string submittedByEmail, string submittedByName)
        {
            Model.Status = IncentiveApplicationStatus.Submitted;
            Model.DateSubmitted = submittedAt;
            Model.SubmittedByEmail = submittedByEmail;
            Model.SubmittedByName = submittedByName;

            AddEvent(new Submitted(Model));
        }

        public void SetApprenticeships(IEnumerable<Apprenticeship> apprenticeships)
        {
            _apprenticeships.Clear();
            Model.ApprenticeshipModels.Clear();
            foreach (var a in apprenticeships)
            {
                AddApprenticeship(a);
            }
        }

        public void EarningsCalculated(Guid apprenticeshipId)
        {
            var apprenticeship = _apprenticeships.Single(a => a.Id == apprenticeshipId);
            apprenticeship.SetEarningsCalculated(true);
        }

        private void AddApprenticeship(Apprenticeship apprenticeship)
        {
            _apprenticeships.Add(apprenticeship);
            Model.ApprenticeshipModels.Add(apprenticeship.GetModel());
        }

        public void CalculateEarnings()
        {
            AddEvent(new EarningsCalculationRequired(Model));
        }
    }
}
