using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SFA.DAS.EmployerIncentives.Commands.UnitTests")]
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

            Model.ApprenticeshipModels.ToList().ForEach(m => m.Phase = IncentivePhase.Create().Identifier);

            // remove ineligible ones
            Model.ApprenticeshipModels = new List<ApprenticeshipModel>(Model.ApprenticeshipModels.Where(a => a.HasEligibleEmploymentStartDate));

            _apprenticeships.Clear();
            foreach (var apprenticeshipModel in Model.ApprenticeshipModels.ToList())
            {
                _apprenticeships.Add(Apprenticeship.Create(apprenticeshipModel));
            }

            AddEvent(new Submitted(Model));
        }

        public void Resubmit()
        {
            if (Model.Status != IncentiveApplicationStatus.Submitted)
            {
                return;
            }

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

        public void EmployerWithdrawal(Apprenticeship apprenticeship, ServiceRequest serviceRequest)
        {
            var apprenticeToWithdraw = _apprenticeships.Single(m => m.Id == apprenticeship.Id);
            apprenticeToWithdraw.Withdraw(IncentiveApplicationStatus.EmployerWithdrawn);
            
            AddEvent(new EmployerWithdrawn(
                Model.AccountId,
                Model.AccountLegalEntityId, 
                apprenticeToWithdraw.GetModel(),
                serviceRequest));
        }

        public void ComplianceWithdrawal(Apprenticeship apprenticeship, ServiceRequest serviceRequest)
        {
            var apprenticeToWithdraw = _apprenticeships.Single(m => m.Id == apprenticeship.Id);
            apprenticeToWithdraw.Withdraw(IncentiveApplicationStatus.ComplianceWithdrawn);

            AddEvent(new ComplianceWithdrawn(
                Model.AccountId,
                Model.AccountLegalEntityId,
                apprenticeToWithdraw.GetModel(),
                serviceRequest));
        }

        public void EarningsCalculated(Guid apprenticeshipId)
        {
            var apprenticeship = _apprenticeships.Single(a => a.Id == apprenticeshipId);
            apprenticeship.SetEarningsCalculated(true);
        }

        private void AddApprenticeship(Apprenticeship apprenticeship)
        {
            var endOfStartMonth = new DateTime(apprenticeship.PlannedStartDate.Year, apprenticeship.PlannedStartDate.Month, DateTime.DaysInMonth(apprenticeship.PlannedStartDate.Year, apprenticeship.PlannedStartDate.Month));
            apprenticeship.SetPlannedStartDate(endOfStartMonth);

            _apprenticeships.Add(apprenticeship);
            Model.ApprenticeshipModels.Add(apprenticeship.GetModel());
        }
    }
}
