using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WorkoutPlanner.Models;
using WorkoutPlanner.Repositories;
using WorkoutPlanner.Interfaces;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace WorkoutPlanner.Controllers
{
    /// <summary>
    /// Controller handling all user-related operations including authentication and account management
    /// </summary>
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes controller with user repository
        /// </summary>
        /// <param name="userRepository">Repository for user data operations</param>
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Displays user registration form
        /// </summary>
        /// <returns>Registration view</returns>
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Processes user registration form submission
        /// </summary>
        /// <param name="user">User registration data</param>
        /// <param name="password">User's plain text password</param>
        /// <returns>Redirects to home page on success, returns to form if validation failed</returns>
        [HttpPost]
        public async Task<IActionResult> Register(User user, string password)
        {
            if (ModelState.IsValid)
            {
                // Hash password and set creation timestamp
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                user.CreatedAt = DateTime.Now;

                await _userRepository.CreateUserAsync(user);

                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password); // Hash password with BCrypt
        }

        /// <summary>
        /// Display Login form
        /// </summary>
        /// <returns>Login view</returns>
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }


        /// <summary>
        /// Processes user login attempt
        /// </summary>
        /// <param name="model">Login credentials</param>
        /// <returns>Redirects to home page on success, returns to with an error on failure</returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userRepository.GetUserByUsernameAsync(model.Username);
                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    // Create claims for the user
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties();

                    // Sign in the user
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Set session data
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Username", user.Username);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
                }
            }
            return View(model);
        }

        /// <summary>
        /// Processes user Logout request
        /// </summary>
        /// <returns>Redirects to home page after signing out</returns>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Clear authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //Clear session
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

    }
}
