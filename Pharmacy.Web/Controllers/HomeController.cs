using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Business;

namespace Pharmacy.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            clsDashboard dashboard = clsDashboard.Load();
            return View(dashboard);
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}
