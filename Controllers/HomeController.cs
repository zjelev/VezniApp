using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspNet.Models;

//using Microsoft.AspNetCore.Mvc.Rendering;

namespace AspNet.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View(MeasuresServices.GetProducts());

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}