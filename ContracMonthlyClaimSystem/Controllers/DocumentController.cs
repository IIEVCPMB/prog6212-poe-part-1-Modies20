using Microsoft.AspNetCore.Mvc;

namespace ContracMonthlyClaimSystem.Controllers
{
    public class DocumentController : Controller
    {
        public IActionResult Upload()
        {
            return View();
        }
    }
}
