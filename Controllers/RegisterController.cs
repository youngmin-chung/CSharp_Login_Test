using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login_process_test.Data;
using Login_process_test.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Login_process_test.Controllers
{
    public class RegisterController : Controller
    {
        UserManager<ApplicationUser> _usrMgr;
        SignInManager<ApplicationUser> _signInMgr;
        private readonly IEmailSender _emailSender;
        public RegisterController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender emailSender)
        {
            _usrMgr = userManager;
            _signInMgr = signInManager;
            _emailSender = emailSender;
        }
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        //[BindProperty]
        //public RegisterViewModel Input { get; set; }

        // POST:/Register/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var appUser = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Firstname = model.Firstname,
                    Address1 = model.Address1,
                    City = model.City,
                    Lastname = model.Lastname,
                    Age = model.Age,
                    Country = model.Country,
                    Region = model.Region,
                    Postalcode = model.Postalcode
                };
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var addUserResult = await _usrMgr.CreateAsync(user, model.Password);
                if (addUserResult.Succeeded)
                {
                    //_logger.LogInformation("User created a new account with password.");

                    var code = await _usrMgr.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                    //await _signInMgr.SignInAsync(appUser, isPersistent: false);
                    HttpContext.Session.SetString("loginstatus", model.Firstname + " is logged in");
                    HttpContext.Session.SetString("message", "Registered, logged on as " + model.Email);
                }
                else
                {
                    ViewBag.message = "registration failed - " + addUserResult.Errors.First().Description;
                    return View("Index");
                }
            }
            return Redirect("/Home");
        }


    }
}