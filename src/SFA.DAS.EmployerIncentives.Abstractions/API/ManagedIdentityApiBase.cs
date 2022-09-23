using SFA.DAS.Http.Configuration;

namespace SFA.DAS.EmployerIncentives.Abstractions.API
{
    public class ManagedIdentityApiBase : ApiBase, IManagedIdentityClientConfiguration
    {
        public string Identifier { get; set; }
    }
}
