using Microsoft.AspNetCore.Mvc;
using Netstr.RelayInformation;
using Netstr.ViewModels;

namespace Netstr.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        private readonly IRelayInformationService service;
        private readonly IHostEnvironment environment;

        public HomeController(IRelayInformationService service, IHostEnvironment environment)
        {
            this.service = service;
            this.environment = environment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (Request.Headers["Accept"] == "application/nostr+json")
            {
                return Ok(this.service.GetDocument());
            }
            else
            {
                var vm = new HomeViewModel(this.service.GetDocument(), $"wss://{Request.Host}", this.environment.EnvironmentName);
                return View(vm);
            }
        }
    }
}
