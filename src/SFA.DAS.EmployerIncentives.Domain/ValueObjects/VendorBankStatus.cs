using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class VendorBankStatus : ValueObject
    {
        public VendorBankStatus(string vendorId, VendorCase vendorCase)
        {
            VendorId = vendorId;
            Status = GetBankDetailsStatus(vendorId, vendorCase);
            BankDetailsRequired = GetBankDetailsRequired(vendorId, vendorCase);
        }

        public string VendorId { get; }
        public BankDetailsStatus Status { get; }
        public bool BankDetailsRequired { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return VendorId;
            yield return Status;
            yield return BankDetailsRequired;
        }

        private static bool HasVendorId(string vendorId)
        {
            return !string.IsNullOrEmpty(vendorId) && vendorId != "000000";
        }

        private static bool GetBankDetailsRequired(string vrfVendorId, VendorCase vendorCase)
        {
            if (HasVendorId(vrfVendorId))
            {
                return false;
            }

            return (string.IsNullOrWhiteSpace(vendorCase.Status)
                || vendorCase.Status.Equals(LegalEntityVrfCaseStatus.RejectedDataValidation, StringComparison.InvariantCultureIgnoreCase)
                || vendorCase.Status.Equals(LegalEntityVrfCaseStatus.RejectedVer1, StringComparison.InvariantCultureIgnoreCase)
                || vendorCase.Status.Equals(LegalEntityVrfCaseStatus.RejectedVerification, StringComparison.InvariantCultureIgnoreCase));
        }

        private static BankDetailsStatus GetBankDetailsStatus(string vendorId, VendorCase vendorCase)
        {
            if (HasVendorId(vendorId))
            {
                return BankDetailsStatus.Completed;
            }

            if (string.IsNullOrWhiteSpace(vendorCase.Status))
            {
                return BankDetailsStatus.NotSupplied;
            }

            if (vendorCase.Status.Equals(LegalEntityVrfCaseStatus.RejectedDataValidation, StringComparison.InvariantCultureIgnoreCase)
                 || vendorCase.Status.Equals(LegalEntityVrfCaseStatus.RejectedVer1, StringComparison.InvariantCultureIgnoreCase)
                 || vendorCase.Status.Equals(LegalEntityVrfCaseStatus.RejectedVerification, StringComparison.InvariantCultureIgnoreCase))
            {
                return BankDetailsStatus.Rejected;
            }

            if (vendorCase.Status.Equals(LegalEntityVrfCaseStatus.Completed, StringComparison.InvariantCultureIgnoreCase))
            {
                return BankDetailsStatus.Completed;
            }

            return BankDetailsStatus.InProgress;
        }
    }
}