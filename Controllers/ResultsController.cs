using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AspNet.Controllers
{
    public class ResultsController : Controller
    {
        public IActionResult Add()
        {
            return View();
        }
    }
}