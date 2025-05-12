using Microsoft.AspNetCore.Mvc;

namespace RevampAuto.Controllers
{
    public class ShippingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
