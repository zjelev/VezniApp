using Microsoft.AspNetCore.Mvc;
using EFUtils.Data.Models.Persons;

namespace AspNet.Controllers
{
    public class ResultsController : Controller
    {
        public IActionResult ReadPeople()
        {
            var db = new PersonsContext();
            var result = EFUtils.Program.SelectNFirst(db);

            return this.View(result);
        }
    }
}