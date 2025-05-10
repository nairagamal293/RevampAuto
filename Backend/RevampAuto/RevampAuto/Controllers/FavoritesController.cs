using Microsoft.AspNetCore.Mvc;

namespace RevampAuto.Controllers
{
    public class FavoritesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
