using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications
{
    public class Apprenticeship : Entity<Guid, ApprenticeshipModel>
    {
        private const decimal EmployerIncentivesTotalPaymentAmount = 3000;

        public long ApprenticeshipId => Model.ApprenticeshipId;
        public string FirstName => Model.FirstName;
        public string LastName => Model.LastName;
        public DateTime DateOfBirth => Model.DateOfBirth;
        public long ULN => Model.ULN;
        public DateTime PlannedStartDate => Model.PlannedStartDate;
        public ApprenticeshipEmployerType ApprenticeshipEmployerTypeOnApproval => Model.ApprenticeshipEmployerTypeOnApproval;
        public decimal TotalIncentiveAmount => Model.TotalIncentiveAmount;
        public long? UKPRN => Model.UKPRN;
        public bool EarningsCalculated => Model.EarningsCalculated;
        public bool WithdrawnByEmployer => Model.WithdrawnByEmployer;
        public bool WithdrawnByCompliance => Model.WithdrawnByCompliance;
        public string CourseName => Model.CourseName;
        public bool HasEligibleEmploymentStartDate => Model.HasEligibleEmploymentStartDate;
        public DateTime? EmploymentStartDate => Model.EmploymentStartDate;
        public Phase Phase => Model.Phase;

        public static Apprenticeship Create(ApprenticeshipModel model)
        {
            return new Apprenticeship(model.Id, model, false);
        }

        internal Apprenticeship(Guid id, long apprenticeshipId, string firstName, string lastName, DateTime dateOfBirth, long uln, DateTime plannedStartDate, ApprenticeshipEmployerType apprenticeshipEmployerTypeOnApproval, long? ukprn, string courseName, DateTime? employmentStartDate, Phase phase)
        {
            IsNew = false;
            Model = new ApprenticeshipModel
            {
                Id = id,
                ApprenticeshipId = apprenticeshipId,
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                ULN = uln,
                PlannedStartDate = plannedStartDate,
                ApprenticeshipEmployerTypeOnApproval = apprenticeshipEmployerTypeOnApproval,
                TotalIncentiveAmount = EmployerIncentivesTotalPaymentAmount,
                UKPRN = ukprn,
                CourseName = courseName,
                EmploymentStartDate = employmentStartDate,
                Phase = phase
            };

            Model.HasEligibleEmploymentStartDate = Incentive.EmployerStartDateIsEligible(this);
        }

        public void SetEarningsCalculated(bool isCalculated = true)
        {
            Model.EarningsCalculated = isCalculated;
        }

        public void Withdraw(IncentiveApplicationStatus incentiveApplicationStatus)
        {
            switch (incentiveApplicationStatus)
            {
                case IncentiveApplicationStatus.EmployerWithdrawn:
                    Model.WithdrawnByEmployer = true;
                    break;

                case IncentiveApplicationStatus.ComplianceWithdrawn:
                    Model.WithdrawnByCompliance = true;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported IncentiveApplicationStatus:{incentiveApplicationStatus} for withdrawal");
            }
        }

        public void SetPlannedStartDate(DateTime plannedStartDate)
        {
            Model.PlannedStartDate = plannedStartDate;
        }           

        private Apprenticeship(Guid id, ApprenticeshipModel model, bool isNew) : base(id, model, isNew)
        {
        }
    }
}
