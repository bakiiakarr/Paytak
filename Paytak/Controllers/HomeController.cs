using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Paytak.Models;

namespace Paytak.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Landing page - her zaman göster
        return View();
    }

    public IActionResult App()
    {
        // App sayfası - sadece giriş yapan kullanıcılar
        if (!User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Login", "Account");
        }
        return RedirectToAction("Index", "Chat");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Services()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult HomePage()
    {
        return RedirectToAction("Index");
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
