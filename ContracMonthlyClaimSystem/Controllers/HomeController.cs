using System.Diagnostics;
using ContracMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContracMonthlyClaimSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
    }
}
