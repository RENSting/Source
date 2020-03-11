using Cnf.CodeBase.Secure;
using Cnf.Project.Employee.Entity;
using Cnf.Project.Employee.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading.Tasks;
using System;

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

            string credential = string.IsNullOrWhiteSpace(viewModel.Password)? null: CryptoHelper.CreateCredential(viewModel.Password);

            User user = await _userManager.Authenticate(viewModel.UserName, credential);
            if (user == null || user.ActiveStatus == false)
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

        [HttpGet("{controller}/{action}/{id?}")]
        public async Task<IActionResult> ChangePassword(int? id)
        {
            int currentUserId = id.HasValue? id.Value: UserHelper.GetUserID(HttpContext);
            ChangePasswordViewModel model = await _userManager.GetUser(currentUserId);
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var data = new ChangeCredential
                        {
                            UserID = model.UserId,
                            OldCredential = string.IsNullOrWhiteSpace(model.OldPassword)?
                                null: CryptoHelper.CreateCredential(model.OldPassword),
                            NewCredential = string.IsNullOrWhiteSpace(model.NewPassword)?
                                null: CryptoHelper.CreateCredential(model.NewPassword)
                        };
                try
                {
                    await _userManager.ChangeCredential(data);
                    return RedirectToAction(nameof(Index));
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError("", "修改口令没有成功:" + ex.Message);
                }
            }
            return View(model);
        }

        public IActionResult Index()
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
