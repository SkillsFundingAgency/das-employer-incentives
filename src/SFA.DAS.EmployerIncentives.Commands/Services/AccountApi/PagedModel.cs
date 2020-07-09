using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Commands.Services.AccountApi
{
    public class PagedModel<T>
    {
        public List<T> Data { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }
}
