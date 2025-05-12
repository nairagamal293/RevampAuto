using Microsoft.AspNetCore.Mvc;

namespace RevampAuto.Controllers
{
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
