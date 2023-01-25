using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspNet.Models;

namespace AspNet.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }


    public IActionResult Index()
    {
        this.ViewBag.Message = "Hello from ViewBag";
        this.ViewData["Message"] = "Hello from ViewData";

        var listOfPeople = new List<Person>
        {
            new Person("Ivan", "Ivanov", "ivan@ivan.ivan"),
            new Person("Gosho", "Georgiev", "gosho@gosho.com")
        };

        return this.View(listOfPeople);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
