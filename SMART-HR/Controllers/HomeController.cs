using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SMART_HR.Models;
using SmartHR.Models;
using SmartHR.ViewModels;
using SmartHR.Services.Interfaces;

namespace SMART_HR.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUtilisateurService _utilisateurService;
        private readonly ApplicationDbContext _context;

        public HomeController(IUtilisateurService utilisateurService, ApplicationDbContext context)
        {
            _utilisateurService = utilisateurService;
            _context = context;
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

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Utilisateurs.FindAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Logout");
            }

            var viewModel = new ProfileViewModel
            {
                Id = user.Id,
                Prenom = user.Prenom,
                Nom = user.Nom,
                Email = user.Email,
                Role = user.Role,
                Actif = user.Actif
            };

            // Load role-specific information
            switch (user.Role)
            {
                case "Admin":
                    var admin = await _context.Admins.FirstOrDefaultAsync(a => a.UtilisateurId == user.Id);
                    if (admin != null)
                    {
                        viewModel.Departement = admin.Departement;
                        viewModel.Poste = admin.Poste;
                        viewModel.Telephone = admin.Telephone;
                        viewModel.EmailProfessionnel = admin.EmailProfessionnel;
                    }
                    break;

                case "RH":
                    var rh = await _context.RessourcesHumaines.FirstOrDefaultAsync(r => r.UtilisateurId == user.Id);
                    if (rh != null)
                    {
                        viewModel.Departement = rh.Departement;
                        viewModel.Poste = rh.Poste;
                        viewModel.Telephone = rh.Telephone;
                        viewModel.EmailProfessionnel = rh.EmailProfessionnel;
                    }
                    break;

                case "Manager":
                    var manager = await _context.Managers
                        .Include(m => m.Employes)
                        .FirstOrDefaultAsync(m => m.UtilisateurId == user.Id);
                    if (manager != null)
                    {
                        viewModel.Departement = manager.Departement;
                        viewModel.Poste = manager.Poste;
                        viewModel.Telephone = manager.Telephone;
                        viewModel.EmailProfessionnel = manager.EmailProfessionnel;
                        viewModel.EmployeeCount = manager.Employes?.Count ?? 0;
                    }
                    break;

                case "Employe":
                    var employe = await _context.Employes
                        .Include(e => e.Manager)
                        .ThenInclude(m => m.Utilisateur)
                        .FirstOrDefaultAsync(e => e.UtilisateurId == user.Id);
                    if (employe != null)
                    {
                        viewModel.Departement = employe.Departement;
                        viewModel.Poste = employe.Poste;
                        viewModel.Telephone = employe.Telephone;
                        viewModel.EmailProfessionnel = employe.EmailProfessionnel;
                        viewModel.ManagerId = employe.ManagerId;
                        viewModel.ManagerName = employe.Manager != null 
                            ? $"{employe.Manager.Utilisateur.Prenom} {employe.Manager.Utilisateur.Nom}" 
                            : null;
                    }
                    break;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Veuillez remplir tous les champs correctement.";
                return RedirectToAction("Profile");
            }

            var user = await _context.Utilisateurs.FindAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Logout");
            }

            // Verify current password
            if (!_utilisateurService.VerifyPassword(model.CurrentPassword, user.MotDePasse))
            {
                TempData["Error"] = "Le mot de passe actuel est incorrect.";
                return RedirectToAction("Profile");
            }

            // Update password
            user.MotDePasse = _utilisateurService.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Votre mot de passe a été modifié avec succès.";
            return RedirectToAction("Profile");
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
