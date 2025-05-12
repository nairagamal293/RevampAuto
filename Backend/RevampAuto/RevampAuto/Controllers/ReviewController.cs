using Microsoft.AspNetCore.Mvc;

namespace RevampAuto.Controllers
{
    public class ReviewController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
