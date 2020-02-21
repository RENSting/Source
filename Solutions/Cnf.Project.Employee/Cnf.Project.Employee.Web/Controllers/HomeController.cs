using Cnf.CodeBase.Secure;
using Cnf.Project.Employee.Entity;
using Cnf.Project.Employee.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Cnf.Project.Employee.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        readonly Api.WebConnectorSettings _webConnectorSettings;
        readonly IUserManager _userManager;

        public HomeController(ILogger<HomeController> logger, IOptionsSnapshot<Api.WebConnectorSettings> apiOption,
            IUserManager userManager)
        {
            _logger = logger;
            _webConnectorSettings = apiOption.Value;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            UserHelper.Signout(HttpContext);

            LoginViewModel loginViewModel = new LoginViewModel
            {
                HasChecked = false
            };

            return View(loginViewModel);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel viewModel)
        {
            if(!ModelState.IsValid)
            {
                return View(viewModel);
            }

            User user = await _userManager.Authenticate(viewModel.UserName, CryptoHelper.CreateCredential(viewModel.Password));
            if (user == null)
            {
                viewModel.HasChecked = true;
                return View(viewModel);
            }
            else
            {
                await UserHelper.Signin(user, HttpContext);

                return RedirectToAction("Index");
            }
        }

        public ActionResult Logout()
        {
            UserHelper.Signout(HttpContext);
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public ActionResult Denied()
        {
            return Content("当前用户不具有需要的权限访问指定的资源");
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
}
