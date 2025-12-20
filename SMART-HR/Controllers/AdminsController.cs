using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using SmartHR.Models;
using SmartHR.ViewModels;
using SmartHR.Services.Interfaces;

namespace SmartHR.Controllers
{
    public class AdminsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUtilisateurService _utilisateurService;

        public AdminsController(ApplicationDbContext context, IUtilisateurService utilisateurService)
        {
            _context = context;
            _utilisateurService = utilisateurService;
        }

        // Authorization check helper
        private bool IsAdmin()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == "Admin";
        }

        private IActionResult CheckAdminAccess()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Accès refusé. Cette page est réservée aux administrateurs.";
                return RedirectToAction("Index", "Home");
            }
            return null;
        }

        // GET: Admin Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Utilisateurs.CountAsync(),
                ActiveUsers = await _context.Utilisateurs.CountAsync(u => u.Actif),
                InactiveUsers = await _context.Utilisateurs.CountAsync(u => !u.Actif),
                AdminCount = await _context.Utilisateurs.CountAsync(u => u.Role == "Admin"),
                HRCount = await _context.Utilisateurs.CountAsync(u => u.Role == "RH"),
                ManagerCount = await _context.Utilisateurs.CountAsync(u => u.Role == "Manager"),
                EmployeeCount = await _context.Utilisateurs.CountAsync(u => u.Role == "Employe"),
                RecentUsers = await _context.Utilisateurs
                    .OrderByDescending(u => u.Id)
                    .Take(5)
                    .Select(u => new UserListItemViewModel
                    {
                        Id = u.Id,
                        FullName = u.Prenom + " " + u.Nom,
                        Email = u.Email,
                        Role = u.Role,
                        Actif = u.Actif
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Users Management - List all users
        public async Task<IActionResult> Users(string searchTerm = null, string roleFilter = null, bool? activeFilter = null)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var query = _context.Utilisateurs.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u => u.Nom.ToLower().Contains(searchTerm) || 
                                        u.Prenom.ToLower().Contains(searchTerm) || 
                                        u.Email.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(roleFilter))
            {
                query = query.Where(u => u.Role == roleFilter);
            }

            if (activeFilter.HasValue)
            {
                query = query.Where(u => u.Actif == activeFilter.Value);
            }

            var users = await query
                .OrderBy(u => u.Nom)
                .Select(u => new UserListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.Prenom + " " + u.Nom,
                    Email = u.Email,
                    Role = u.Role,
                    Actif = u.Actif
                })
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.ActiveFilter = activeFilter;

            return View(users);
        }

        // GET: Create User
        public IActionResult CreateUser()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var managersList = _context.Utilisateurs
                .Where(u => u.Role == "Manager" && u.Actif)
                .Join(_context.Managers,
                    u => u.Id,
                    m => m.UtilisateurId,
                    (u, m) => new { m.Id, Name = u.Prenom + " " + u.Nom })
                .ToList();
            
            var managerSelectList = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Aucun manager --" } };
            managerSelectList.AddRange(managersList.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }));
            ViewBag.Managers = new SelectList(managerSelectList, "Value", "Text");
            
            return View();
        }

        // POST: Create User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (_utilisateurService.EmailExists(model.Email))
                {
                    ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                    var managersForError = await _context.Utilisateurs
                        .Where(u => u.Role == "Manager" && u.Actif)
                        .Join(_context.Managers,
                            u => u.Id,
                            m => m.UtilisateurId,
                            (u, m) => new { m.Id, Name = u.Prenom + " " + u.Nom })
                        .ToListAsync();
                    
                    var managerSelectForError = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Aucun manager --" } };
                    managerSelectForError.AddRange(managersForError.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }));
                    ViewBag.Managers = new SelectList(managerSelectForError, "Value", "Text");
                    return View(model);
                }

                // Create the base user
                var user = new Utilisateur
                {
                    Prenom = model.Prenom,
                    Nom = model.Nom,
                    Email = model.Email,
                    MotDePasse = _utilisateurService.HashPassword(model.MotDePasse),
                    Role = model.Role,
                    Actif = model.Actif
                };

                _context.Utilisateurs.Add(user);
                await _context.SaveChangesAsync();

                // Create role-specific entity
                switch (model.Role)
                {
                    case "Admin":
                        var admin = new Admin
                        {
                            UtilisateurId = user.Id,
                            Departement = model.Departement ?? "Administration",
                            Poste = model.Poste ?? "Administrateur",
                            Telephone = model.Telephone ?? "",
                            EmailProfessionnel = model.EmailProfessionnel ?? model.Email
                        };
                        _context.Admins.Add(admin);
                        break;

                    case "RH":
                        var rh = new RessourceHumaine
                        {
                            UtilisateurId = user.Id,
                            Departement = model.Departement ?? "Ressources Humaines",
                            Poste = model.Poste ?? "Responsable RH",
                            Telephone = model.Telephone ?? "",
                            EmailProfessionnel = model.EmailProfessionnel ?? model.Email
                        };
                        _context.RessourcesHumaines.Add(rh);
                        break;

                    case "Manager":
                        var manager = new Manager
                        {
                            UtilisateurId = user.Id,
                            Departement = model.Departement ?? "Management",
                            Poste = model.Poste ?? "Manager",
                            Telephone = model.Telephone ?? "",
                            EmailProfessionnel = model.EmailProfessionnel ?? model.Email
                        };
                        _context.Managers.Add(manager);
                        break;

                    case "Employe":
                        var employe = new Employe
                        {
                            UtilisateurId = user.Id,
                            Departement = model.Departement ?? "General",
                            Poste = model.Poste ?? "Employé",
                            Telephone = model.Telephone,
                            EmailProfessionnel = model.EmailProfessionnel ?? model.Email,
                            ManagerId = model.ManagerId
                        };
                        _context.Employes.Add(employe);
                        break;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"L'utilisateur {user.Prenom} {user.Nom} a été créé avec succès.";
                return RedirectToAction(nameof(Users));
            }

            var managersList = await _context.Utilisateurs
                .Where(u => u.Role == "Manager" && u.Actif)
                .Join(_context.Managers,
                    u => u.Id,
                    m => m.UtilisateurId,
                    (u, m) => new { m.Id, Name = u.Prenom + " " + u.Nom })
                .ToListAsync();
            
            var managerSelectList = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Aucun manager --" } };
            managerSelectList.AddRange(managersList.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }));
            ViewBag.Managers = new SelectList(managerSelectList, "Value", "Text");
            return View(model);
        }

        // GET: Edit User
        public async Task<IActionResult> EditUser(int? id)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Utilisateurs.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Prenom = user.Prenom,
                Nom = user.Nom,
                Email = user.Email,
                Role = user.Role,
                Actif = user.Actif
            };

            // Load role-specific data
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
                    var manager = await _context.Managers.FirstOrDefaultAsync(m => m.UtilisateurId == user.Id);
                    if (manager != null)
                    {
                        viewModel.Departement = manager.Departement;
                        viewModel.Poste = manager.Poste;
                        viewModel.Telephone = manager.Telephone;
                        viewModel.EmailProfessionnel = manager.EmailProfessionnel;
                    }
                    break;

                case "Employe":
                    var employe = await _context.Employes.FirstOrDefaultAsync(e => e.UtilisateurId == user.Id);
                    if (employe != null)
                    {
                        viewModel.Departement = employe.Departement;
                        viewModel.Poste = employe.Poste;
                        viewModel.Telephone = employe.Telephone;
                        viewModel.EmailProfessionnel = employe.EmailProfessionnel;
                        viewModel.ManagerId = employe.ManagerId;
                    }
                    break;
            }

            var managersList = await _context.Utilisateurs
                .Where(u => u.Role == "Manager" && u.Actif)
                .Join(_context.Managers,
                    u => u.Id,
                    m => m.UtilisateurId,
                    (u, m) => new { m.Id, Name = u.Prenom + " " + u.Nom })
                .ToListAsync();
            
            var managerSelectList = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Aucun manager --", Selected = viewModel.ManagerId == null } };
            managerSelectList.AddRange(managersList.Select(m => new SelectListItem 
            { 
                Value = m.Id.ToString(), 
                Text = m.Name,
                Selected = viewModel.ManagerId.HasValue && viewModel.ManagerId.Value == m.Id
            }));
            ViewBag.Managers = managerSelectList;

            return View(viewModel);
        }

        // POST: Edit User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, EditUserViewModel model)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Utilisateurs.FindAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    // Check if email is changed and already exists
                    if (user.Email != model.Email && _utilisateurService.EmailExists(model.Email))
                    {
                        ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                        var managersForError = await _context.Utilisateurs
                            .Where(u => u.Role == "Manager" && u.Actif)
                            .Join(_context.Managers,
                                u => u.Id,
                                m => m.UtilisateurId,
                                (u, m) => new { m.Id, Name = u.Prenom + " " + u.Nom })
                            .ToListAsync();
                        
                        var managerSelectForError = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Aucun manager --" } };
                        managerSelectForError.AddRange(managersForError.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }));
                        ViewBag.Managers = new SelectList(managerSelectForError, "Value", "Text", model.ManagerId?.ToString() ?? "");
                        return View(model);
                    }

                    // Update base user
                    user.Prenom = model.Prenom;
                    user.Nom = model.Nom;
                    user.Email = model.Email;
                    user.Role = model.Role;
                    user.Actif = model.Actif;

                    _context.Update(user);

                    // Update or create role-specific entity
                    switch (model.Role)
                    {
                        case "Admin":
                            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.UtilisateurId == user.Id);
                            if (admin != null)
                            {
                                admin.Departement = model.Departement ?? "Administration";
                                admin.Poste = model.Poste ?? "Administrateur";
                                admin.Telephone = model.Telephone ?? "";
                                admin.EmailProfessionnel = model.EmailProfessionnel ?? model.Email;
                                _context.Update(admin);
                            }
                            else
                            {
                                admin = new Admin
                                {
                                    UtilisateurId = user.Id,
                                    Departement = model.Departement ?? "Administration",
                                    Poste = model.Poste ?? "Administrateur",
                                    Telephone = model.Telephone ?? "",
                                    EmailProfessionnel = model.EmailProfessionnel ?? model.Email
                                };
                                _context.Admins.Add(admin);
                            }
                            break;

                        case "RH":
                            var rh = await _context.RessourcesHumaines.FirstOrDefaultAsync(r => r.UtilisateurId == user.Id);
                            if (rh != null)
                            {
                                rh.Departement = model.Departement ?? "Ressources Humaines";
                                rh.Poste = model.Poste ?? "Responsable RH";
                                rh.Telephone = model.Telephone ?? "";
                                rh.EmailProfessionnel = model.EmailProfessionnel ?? model.Email;
                                _context.Update(rh);
                            }
                            else
                            {
                                rh = new RessourceHumaine
                                {
                                    UtilisateurId = user.Id,
                                    Departement = model.Departement ?? "Ressources Humaines",
                                    Poste = model.Poste ?? "Responsable RH",
                                    Telephone = model.Telephone ?? "",
                                    EmailProfessionnel = model.EmailProfessionnel ?? model.Email
                                };
                                _context.RessourcesHumaines.Add(rh);
                            }
                            break;

                        case "Manager":
                            var manager = await _context.Managers.FirstOrDefaultAsync(m => m.UtilisateurId == user.Id);
                            if (manager != null)
                            {
                                manager.Departement = model.Departement ?? "Management";
                                manager.Poste = model.Poste ?? "Manager";
                                manager.Telephone = model.Telephone ?? "";
                                manager.EmailProfessionnel = model.EmailProfessionnel ?? model.Email;
                                _context.Update(manager);
                            }
                            else
                            {
                                manager = new Manager
                                {
                                    UtilisateurId = user.Id,
                                    Departement = model.Departement ?? "Management",
                                    Poste = model.Poste ?? "Manager",
                                    Telephone = model.Telephone ?? "",
                                    EmailProfessionnel = model.EmailProfessionnel ?? model.Email
                                };
                                _context.Managers.Add(manager);
                            }
                            break;

                        case "Employe":
                            var employe = await _context.Employes.FirstOrDefaultAsync(e => e.UtilisateurId == user.Id);
                            if (employe != null)
                            {
                                employe.Departement = model.Departement ?? "General";
                                employe.Poste = model.Poste ?? "Employé";
                                employe.Telephone = model.Telephone;
                                employe.EmailProfessionnel = model.EmailProfessionnel ?? model.Email;
                                employe.ManagerId = model.ManagerId;
                                _context.Update(employe);
                            }
                            else
                            {
                                employe = new Employe
                                {
                                    UtilisateurId = user.Id,
                                    Departement = model.Departement ?? "General",
                                    Poste = model.Poste ?? "Employé",
                                    Telephone = model.Telephone,
                                    EmailProfessionnel = model.EmailProfessionnel ?? model.Email,
                                    ManagerId = model.ManagerId,
                                    JoursCongesTotal = 30
                                };
                                _context.Employes.Add(employe);
                            }
                            break;
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"L'utilisateur {user.Prenom} {user.Nom} a été modifié avec succès.";
                    return RedirectToAction(nameof(Users));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var managersList = await _context.Utilisateurs
                .Where(u => u.Role == "Manager" && u.Actif)
                .Join(_context.Managers,
                    u => u.Id,
                    m => m.UtilisateurId,
                    (u, m) => new { m.Id, Name = u.Prenom + " " + u.Nom })
                .ToListAsync();
            
            var managerSelectList = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Aucun manager --", Selected = model.ManagerId == null } };
            managerSelectList.AddRange(managersList.Select(m => new SelectListItem 
            { 
                Value = m.Id.ToString(), 
                Text = m.Name,
                Selected = model.ManagerId.HasValue && model.ManagerId.Value == m.Id
            }));
            ViewBag.Managers = managerSelectList;
            return View(model);
        }

        // GET: User Details
        public async Task<IActionResult> UserDetails(int? id)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Utilisateurs.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserDetailsViewModel
            {
                Id = user.Id,
                Prenom = user.Prenom,
                Nom = user.Nom,
                Email = user.Email,
                Role = user.Role,
                Actif = user.Actif
            };

            // Load role-specific data
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
                            : "Aucun";
                    }
                    break;
            }

            return View(viewModel);
        }

        // POST: Toggle User Active Status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var user = await _context.Utilisateurs.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Actif = !user.Actif;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Le statut de {user.Prenom} {user.Nom} a été modifié : {(user.Actif ? "Actif" : "Inactif")}";
            return RedirectToAction(nameof(Users));
        }

        // GET: Reset Password
        public async Task<IActionResult> ResetPassword(int? id)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Utilisateurs.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new ResetPasswordViewModel
            {
                UserId = user.Id,
                UserName = $"{user.Prenom} {user.Nom}",
                Email = user.Email
            };

            return View(viewModel);
        }

        // POST: Reset Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            if (ModelState.IsValid)
            {
                var user = await _context.Utilisateurs.FindAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                user.MotDePasse = _utilisateurService.HashPassword(model.NewPassword);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Le mot de passe de {user.Prenom} {user.Nom} a été réinitialisé avec succès.";
                return RedirectToAction(nameof(Users));
            }

            return View(model);
        }

        // GET: Delete User
        public async Task<IActionResult> DeleteUser(int? id)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Utilisateurs.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserDetailsViewModel
            {
                Id = user.Id,
                Prenom = user.Prenom,
                Nom = user.Nom,
                Email = user.Email,
                Role = user.Role,
                Actif = user.Actif
            };

            return View(viewModel);
        }

        // POST: Delete User
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var user = await _context.Utilisateurs.FindAsync(id);
            if (user != null)
            {
                // Delete role-specific entity first
                switch (user.Role)
                {
                    case "Admin":
                        var admin = await _context.Admins.FirstOrDefaultAsync(a => a.UtilisateurId == user.Id);
                        if (admin != null) _context.Admins.Remove(admin);
                        break;

                    case "RH":
                        var rh = await _context.RessourcesHumaines.FirstOrDefaultAsync(r => r.UtilisateurId == user.Id);
                        if (rh != null) _context.RessourcesHumaines.Remove(rh);
                        break;

                    case "Manager":
                        var manager = await _context.Managers.FirstOrDefaultAsync(m => m.UtilisateurId == user.Id);
                        if (manager != null) _context.Managers.Remove(manager);
                        break;

                    case "Employe":
                        var employe = await _context.Employes.FirstOrDefaultAsync(e => e.UtilisateurId == user.Id);
                        if (employe != null) _context.Employes.Remove(employe);
                        break;
                }

                _context.Utilisateurs.Remove(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"L'utilisateur {user.Prenom} {user.Nom} a été supprimé avec succès.";
            }

            return RedirectToAction(nameof(Users));
        }

        // System Settings (placeholder for future features)
        public IActionResult Settings()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            return View();
        }

        private bool UserExists(int id)
        {
            return _context.Utilisateurs.Any(e => e.Id == id);
        }

        // ===== Old Admin entity management methods (kept for backward compatibility) =====
        
        // GET: Admins
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Admins.Include(a => a.Utilisateur);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admins/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _context.Admins
                .Include(a => a.Utilisateur)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // GET: Admins/Create
        public IActionResult Create()
        {
            ViewData["UtilisateurId"] = new SelectList(_context.Utilisateurs, "Id", "Email");
            return View();
        }

        // POST: Admins/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UtilisateurId,Departement,Poste,Telephone,EmailProfessionnel")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                _context.Add(admin);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UtilisateurId"] = new SelectList(_context.Utilisateurs, "Id", "Email", admin.UtilisateurId);
            return View(admin);
        }

        // GET: Admins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }
            ViewData["UtilisateurId"] = new SelectList(_context.Utilisateurs, "Id", "Email", admin.UtilisateurId);
            return View(admin);
        }

        // POST: Admins/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UtilisateurId,Departement,Poste,Telephone,EmailProfessionnel")] Admin admin)
        {
            if (id != admin.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(admin);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdminExists(admin.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UtilisateurId"] = new SelectList(_context.Utilisateurs, "Id", "Email", admin.UtilisateurId);
            return View(admin);
        }

        // GET: Admins/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _context.Admins
                .Include(a => a.Utilisateur)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // POST: Admins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin != null)
            {
                _context.Admins.Remove(admin);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdminExists(int id)
        {
            return _context.Admins.Any(e => e.Id == id);
        }
    }
}
