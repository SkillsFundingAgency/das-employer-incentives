using System;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications
{
    public class Apprenticeship : Entity<Guid, ApprenticeshipModel>
    {
        public long ApprenticeshipId => Model.ApprenticeshipId;
        public string FirstName => Model.FirstName;
        public string LastName => Model.LastName;
        public DateTime DateOfBirth => Model.DateOfBirth;
        public long Uln => Model.Uln;
        public DateTime PlannedStartDate => Model.PlannedStartDate;
        public ApprenticeshipEmployerType ApprenticeshipEmployerTypeOnApproval => Model.ApprenticeshipEmployerTypeOnApproval;
        public decimal TotalIncentiveAmount => Model.TotalIncentiveAmount;
        public bool EarningsCalculated => Model.EarningsCalculated;

        internal static Apprenticeship Create(ApprenticeshipModel model)
        {
            return new Apprenticeship(model.Id, model, false);
        }

        internal Apprenticeship(Guid id, long apprenticeshipId, string firstName, string lastName, DateTime dateOfBirth, long uln, DateTime plannedStartDate, ApprenticeshipEmployerType apprenticeshipEmployerTypeOnApproval)
        {
            IsNew = false;
            Model = new ApprenticeshipModel
            {
                Id = id,
                ApprenticeshipId = apprenticeshipId,
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Uln = uln,
                PlannedStartDate = plannedStartDate,
                ApprenticeshipEmployerTypeOnApproval = apprenticeshipEmployerTypeOnApproval,
                TotalIncentiveAmount = new NewApprenticeIncentive().CalculateTotalIncentiveAmount(dateOfBirth, plannedStartDate)
            };
        }

        public void SetEarningsCalculated(bool isCalculated = true)
        {
            Model.EarningsCalculated = isCalculated;
        }

        private Apprenticeship(Guid id, ApprenticeshipModel model, bool isNew) : base(id, model, isNew)
        {
        }
    }
}
