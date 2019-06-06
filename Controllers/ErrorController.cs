using Microsoft.AspNetCore.Mvc;

namespace TauManager.Controllers
{
    [Route("Error")]
    public class ErrorController: Controller
    {
        [Route("404")]
        public IActionResult NotFoundView()
        {
            return View();
        }

        [Route("409")]
        public IActionResult ConflictView()
        {
            return View();
        }
    }
}