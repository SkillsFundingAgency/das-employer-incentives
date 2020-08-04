using System;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications
{
    public class Apprenticeship : Entity<Guid, ApprenticeshipModel>
    {
        public int ApprenticeshipId => Model.ApprenticeshipId;
        public string FirstName => Model.FirstName;
        public string LastName => Model.LastName;
        public DateTime DateOfBirth => Model.DateOfBirth;
        public long Uln => Model.Uln;
        public DateTime PlannedStartDate => Model.PlannedStartDate;
        public ApprenticeshipEmployerType ApprenticeshipEmployerTypeOnApproval => Model.ApprenticeshipEmployerTypeOnApproval;

        internal static Apprenticeship Create(ApprenticeshipModel model)
        {
            return new Apprenticeship(model.Id, model, false);
        }

        internal Apprenticeship(Guid id, int apprenticeshipId, string firstName, string lastName, DateTime dateOfBirth, long uln, DateTime plannedStartDate, ApprenticeshipEmployerType apprenticeshipEmployerTypeOnApproval)
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
                ApprenticeshipEmployerTypeOnApproval = apprenticeshipEmployerTypeOnApproval
            };
        }

        private Apprenticeship(Guid id, ApprenticeshipModel model, bool isNew) : base(id, model, isNew)
        {
        }
    }
}
