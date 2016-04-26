using System.Web.Mvc;

namespace MVCAndAngularJSClient.Controllers
{
    /// <summary>
    /// Home Controller
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Shows Index page
        /// </summary>
        /// <returns>View of the Index page</returns>
        public ActionResult Index()
        {
            return View();
        }
    }
}