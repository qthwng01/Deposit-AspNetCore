using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Recharge_Test.Repository;
using Recharge_Test.Repository.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Recharge_Test.Controllers
{
    public class AuthController : Controller
    {
        private readonly EVDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public AuthController(ILogger<HomeController> logger, EVDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Customers.FirstOrDefault(u => u.Username == model.UserName && u.Password == model.Password);
                if (user != null)
                {
                    //Tạo cookies
                    HttpContext.Response.Cookies.Append("Username", user.Username);
                    HttpContext.Response.Cookies.Append("UserId", user.CustomerId.ToString());
                    //Tạo identity
                    var userClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.CustomerName),
                        new Claim("CustomerId", user.CustomerId.ToString())
                    };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(userClaims, "login");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    //_notyfySerice.Success("Đăng nhập thành công", 3);
                    await HttpContext.SignInAsync(claimsPrincipal);
                    return RedirectToAction("Deposit", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            Response.Cookies.Delete("EVUserCookie");
            return RedirectToAction("Login", "Auth");
        }

    }
}
