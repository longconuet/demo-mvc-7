using Microsoft.AspNetCore.Mvc;
using WebApplication7.Data;
using WebApplication7.Requests;
using Microsoft.AspNetCore.Identity;
using WebApplication7.Models;
using System.Net.Mail;

namespace WebApplication7.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;

        public AccountController(ApplicationDbContext context, 
            SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (ModelState.IsValid)
            {
                var userName = request.Username;
                if (IsValidEmail(request.Username))
                {
                    var user = await _userManager.FindByEmailAsync(request.Username);
                    if (user != null)
                    {
                        userName = user.UserName;
                    }
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(userName, request.Password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View();
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        public bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (ModelState.IsValid)
            {
                MailAddress address = new MailAddress(request.Email);
                string userName = address.User;
                var user = new ApplicationUser
                {
                    UserName = userName,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName
                };

                await _userStore.SetUserNameAsync(user, request.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, request.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, request.Password);

                if (result.Succeeded)
                {
                    // confirm email
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, code);

                    // login
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
