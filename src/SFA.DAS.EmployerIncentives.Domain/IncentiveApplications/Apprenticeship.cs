using System;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.ValueObjects;

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

        private readonly NewApprenticeIncentive _incentive;

        internal static Apprenticeship Create(ApprenticeshipModel model)
        {
            return new Apprenticeship(model.Id, model, false);
        }

        private Apprenticeship(Guid id, ApprenticeshipModel model, bool isNew) : base(id, model, isNew)
        {
            _incentive = new NewApprenticeIncentive();
        }

        public double TotalIncentiveAmount => _incentive.CalculateTotalIncentiveAmount(DateOfBirth, PlannedStartDate);
    }
}
