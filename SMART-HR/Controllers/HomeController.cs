using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SMART_HR.Models;
using SmartHR.Models;
using SmartHR.ViewModels;
using SmartHR.Services.Interfaces;

namespace SMART_HR.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUtilisateurService _utilisateurService;

        public HomeController(IUtilisateurService utilisateurService)
        {
            _utilisateurService = utilisateurService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // If already logged in, redirect to home
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                ViewData["Error"] = "Veuillez remplir tous les champs correctement.";
                return View(model);
            }

            var user = _utilisateurService.Authenticate(model.Email, model.Password);
            
            if (user == null)
            {
                ViewData["Error"] = "Email ou mot de passe incorrect.";
                return View(model);
            }

            // Store user info in session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", $"{user.Prenom} {user.Nom}");
            HttpContext.Session.SetString("UserRole", user.Role);

            // Redirect based on role or return URL
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Register()
        {
            // If already logged in, redirect to home
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Error"] = "Veuillez remplir tous les champs correctement.";
                return View(model);
            }

            // Check if email already exists
            if (_utilisateurService.EmailExists(model.Email))
            {
                ViewData["Error"] = "Cet email est déjà utilisé.";
                return View(model);
            }

            // Create new user
            var user = new Utilisateur
            {
                Prenom = model.FirstName,
                Nom = model.LastName,
                Email = model.Email,
                MotDePasse = _utilisateurService.HashPassword(model.Password),
                Role = "Employe",
                Actif = true
            };

            try
            {
                _utilisateurService.Create(user);

                // Auto-login after registration
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserName", $"{user.Prenom} {user.Nom}");
                HttpContext.Session.SetString("UserRole", user.Role);

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewData["Error"] = "Une erreur est survenue lors de l'inscription. Veuillez réessayer.";
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
