using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Spike.Api.Client;
using SFA.DAS.EmployerIncentives.Spike.Api.Client.Types;

namespace SFA.DAS.EmployerIncentives.Web.Controllers
{
    [Route("test-api")]
    public class TestApiController : Controller
    {
        private readonly ISimpleApiClient _apiClient;

        public TestApiController(ISimpleApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Details()
        {
            var response = await _apiClient.GetSimple();

            return View(Map(response));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(SimpleViewModel viewModel)
        {
            try
            {
                var response = await _apiClient.PostSimple(new SimpleRequest { Name = viewModel.Name});

                return View(Map(response));

            }
            catch (ApiDomainException e)
            {
                foreach (var error in e.Errors)
                {
                    
                }

                throw e;
            }
        }

        private SimpleViewModel Map(SimpleResponse response) => new SimpleViewModel {SayHelloName = response.SayHelloName};
    }



    public class SimpleViewModel
    {
        public string SayHelloName { get; set; }
        public string Name { get; set; }
    }



}