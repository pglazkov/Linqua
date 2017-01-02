using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Linqua.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment env;

        public HomeController(IHostingEnvironment env)
        {
            this.env = env;
        }

        public IActionResult Index()
        {
            ViewBag.HashedMain = GetHashedMainDotJs();

            return View();
        }

        public string GetHashedMainDotJs()
        {
            var basePath = env.WebRootPath + "//dist//";
            var info = new System.IO.DirectoryInfo(basePath);
            var file = info.GetFiles().FirstOrDefault(f => f.Name.StartsWith("main.") && !f.Name.EndsWith("bundle.map"));

            return file.Name;
        }

    }
}
