using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Web.Models
{
    public class GoogleAnalyticsData
    {
        public string DataLoaded { get; set; } = "dataLoaded";
        public string UserId { get; set; }
        public string Acc { get; set; }
        public IDictionary<string, string> Extras { get; set; } = new Dictionary<string, string>();
    }
}
