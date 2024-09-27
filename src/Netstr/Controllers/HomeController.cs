using Microsoft.AspNetCore.Mvc;
using Netstr.RelayInformation;
using Netstr.ViewModels;

namespace Netstr.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        private readonly IRelayInformationService service;

        public HomeController(IRelayInformationService service)
        {
            this.service = service;
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
                var vm = new HomeViewModel(this.service.GetDocument(), $"wss://{Request.Host}");
                return View(vm);
            }
        }
    }
}
