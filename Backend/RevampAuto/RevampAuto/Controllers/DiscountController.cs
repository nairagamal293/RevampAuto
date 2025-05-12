using Microsoft.AspNetCore.Mvc;

namespace RevampAuto.Controllers
{
    public class DiscountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
