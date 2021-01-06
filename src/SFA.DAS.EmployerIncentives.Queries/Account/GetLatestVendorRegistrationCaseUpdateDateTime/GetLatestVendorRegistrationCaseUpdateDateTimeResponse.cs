using SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntity;
using System;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetLatestVendorRegistrationCaseUpdateDateTime
{
    public class GetLatestVendorRegistrationCaseUpdateDateTimeResponse : GetLegalEntityResponse
    {
        public DateTime? LastUpdateDateTime { get; set; }
    }
}