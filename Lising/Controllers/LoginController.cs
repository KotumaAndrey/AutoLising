﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApiUtils.Entities;
using WebApiUtils.BaseApi;

namespace Lising.Controllers
{
    [Route("/login")]
    public class LoginController : Controller
    {
        protected string connectionString => Environment.GetEnvironmentVariable("AuthConnectionString")!;
        protected BaseWithNameRepository<DUser> repository => new BaseWithNameRepository<DUser>(connectionString);

        [HttpGet("/register")]
        public IActionResult Register()
        {
            ViewData["Action"] = "./register";
            ViewData["Title"] = "Register";

            return View("Login");
        }

        [HttpPost("/register")]
        public IActionResult RegisterPost()
        {
            ViewData["Action"] = "./register";
            ViewData["Title"] = "Register";

            var login = Request.Form["login"].ToString();
            ViewData["Login"] = login;
            var password = Request.Form["password"].ToString();
            ViewData["Password"] = password;

            if (login is null || login.Length < 1)
            {
                ViewData["ErrorText"] = "Incorrect login";
                return View("Login");
            }

            if (password is null || password.Length < 1)
            {
                ViewData["ErrorText"] = "Incorrect password";
                return View("Login");
            }

            var dbUser = repository.GetByName(login);
            if (dbUser is not null)
            {
                ViewData["ErrorText"] = $"User with login \"{login}\" already exists";
                return View("Login");
            }

            var user = new DUser
            {
                Name = login,
                PasswordHash = CalculateHash(password),
            };
            var createdUser = repository.Add(user);
            if (createdUser is null)
            {
                ViewData["ErrorText"] = "User was not created";
                return View("Login");
            }

            LoginAsUser(login);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("/login")]
        public IActionResult Login()
        {
            ViewData["Action"] = "./login";
            ViewData["Title"] = "Login";
            ViewData["SubmitText"] = "Login";

            return View("Login");
        }

        [HttpPost("/login")]
        public IActionResult LoginPost()
        {
            ViewData["Action"] = "./login";
            ViewData["Title"] = "Login";
            ViewData["SubmitText"] = "Login";

            var login = Request.Form["login"].ToString();
            var password = Request.Form["password"].ToString();

            var dbUser = repository.GetByName(login);
            if (dbUser is null)
            {
                ViewData["ErrorText"] = $"User with login \"{login}\" not exists";
                return View("Login");
            }

            if (dbUser.PasswordHash != CalculateHash(password))
            {
                ViewData["ErrorText"] = "Incorrect password";
                return View("Login");
            }

            LoginAsUser(login);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        private static int CalculateHash(string s)
        {
            if (s is null || s.Length < 1) return 0;

            int result = 0;
            foreach (var c in s)
            {
                var random = new Random(c);
                var temp = random.Next();
                result ^= temp;
            }

            return result;
        }

        private void LoginAsUser(string login)
        {
            List<Claim> claims = [new(ClaimsIdentity.DefaultNameClaimType, login)];
            ClaimsIdentity identity = new(claims, "Cookies");
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new(identity)).Wait();
        }
    }
}
