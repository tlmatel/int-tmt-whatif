using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreWhatIf.Controllers;

[Authorize(Roles = "admin")]
public class ToolsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
