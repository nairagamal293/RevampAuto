using Microsoft.AspNetCore.Mvc;

namespace RevampAuto.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
