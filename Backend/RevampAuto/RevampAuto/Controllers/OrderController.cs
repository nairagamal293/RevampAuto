using Microsoft.AspNetCore.Mvc;

namespace RevampAuto.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
