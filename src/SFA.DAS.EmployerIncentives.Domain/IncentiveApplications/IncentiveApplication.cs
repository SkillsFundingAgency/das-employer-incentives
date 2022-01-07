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
using SFA.DAS.EmployerIncentives.Domain.Accounts;

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

        public ReadOnlyCollection<Apprenticeship> Apprenticeships => Map(Model.ApprenticeshipModels).AsReadOnly();        

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
        }

        public void Submit(DateTime submittedAt, string submittedByEmail, string submittedByName)
        {
            Model.Status = IncentiveApplicationStatus.Submitted;
            Model.DateSubmitted = submittedAt;
            Model.SubmittedByEmail = submittedByEmail;
            Model.SubmittedByName = submittedByName;

            Model.ApprenticeshipModels = FilterEligibleApprenticeships(Model.ApprenticeshipModels);

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
            Model.ApprenticeshipModels.Clear();
            foreach (var a in apprenticeships)
            {
                AddApprenticeship(a);
            }
        }

        public void EmployerWithdrawal(Apprenticeship apprenticeship, LegalEntity legalEntity, string withdrawnByEmailAddress, ServiceRequest serviceRequest)
        {
            var apprenticeToWithdraw = Apprenticeships.Single(m => m.Id == apprenticeship.Id);
            apprenticeToWithdraw.Withdraw(IncentiveApplicationStatus.EmployerWithdrawn);
            
            AddEvent(new EmployerWithdrawn(
                Model.AccountId,
                Model.AccountLegalEntityId,
                legalEntity.Name,
                withdrawnByEmailAddress,
                apprenticeToWithdraw.GetModel(),
                serviceRequest));
        }

        public void ComplianceWithdrawal(Apprenticeship apprenticeship, ServiceRequest serviceRequest)
        {
            var apprenticeToWithdraw = Apprenticeships.Single(m => m.Id == apprenticeship.Id);
            apprenticeToWithdraw.Withdraw(IncentiveApplicationStatus.ComplianceWithdrawn);

            AddEvent(new ComplianceWithdrawn(
                Model.AccountId,
                Model.AccountLegalEntityId,
                apprenticeToWithdraw.GetModel(),
                serviceRequest));
        }

        public void EarningsCalculated(Guid apprenticeshipId)
        {
            var apprenticeship = Apprenticeships.Single(a => a.Id == apprenticeshipId);
            apprenticeship.SetEarningsCalculated(true);
        }

        private void AddApprenticeship(Apprenticeship apprenticeship)
        {
            var endOfStartMonth = new DateTime(apprenticeship.PlannedStartDate.Year, apprenticeship.PlannedStartDate.Month, DateTime.DaysInMonth(apprenticeship.PlannedStartDate.Year, apprenticeship.PlannedStartDate.Month));
            apprenticeship.SetPlannedStartDate(endOfStartMonth);
                        
            Model.ApprenticeshipModels.Add(apprenticeship.GetModel());
        }

        private static List<ApprenticeshipModel> FilterEligibleApprenticeships(ICollection<ApprenticeshipModel> apprenticeshipModels)
        {
            return new List<ApprenticeshipModel>(apprenticeshipModels.Where(a => a.StartDatesAreEligible));
        }

        private static List<Apprenticeship> Map(ICollection<ApprenticeshipModel> apprenticeshipModels)
        {
            var apprenticeships = new List<Apprenticeship>();
            foreach (var apprenticeshipModel in apprenticeshipModels.ToList())
            {
                apprenticeships.Add(Apprenticeship.Create(apprenticeshipModel));
            }

            return apprenticeships;
        }

    }
}
