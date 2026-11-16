using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Web.Controllers;

// Ana sayfa kontrolcüsü
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // Ana sayfa
    public IActionResult Index()
    {
        return View();
    }

    // Hakkımızda sayfası
    public IActionResult About()
    {
        return View();
    }

    // Gizlilik politikası
    public IActionResult Privacy()
    {
        return View();
    }

    // Hata sayfası
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
