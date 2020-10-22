using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public class Payment : Entity<Guid, PaymentModel>
    {
        public Account Account => Model.Account;
        public decimal Amount => Model.Amount;
        public byte PaymentPeriod => Model.PaymentPeriod;
        public short PaymentYear => Model.PaymentYear;
        public DateTime? PaidDate => Model.PaidDate;
        public SubnominalCode SubnominalCode => Model.SubnominalCode;

        internal static Payment Get(PaymentModel model)
        {
            return new Payment(model);
        }

        private Payment(PaymentModel model, bool isNew = false) : base(model.Id, model, isNew)
        {
        }
    }
}
