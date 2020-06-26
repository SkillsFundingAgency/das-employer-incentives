using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Spike.Api.Client.Types;

namespace SFA.DAS.EmployerIncentives.Spike.Api.Client
{
    public interface ISimpleApiClient
    {
        Task<SimpleResponse> GetSimple();
        Task<SimpleResponse> PostSimple(SimpleRequest request);

    }
}
