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

namespace SmartHR.Controllers
{
    public class DemandeCongesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DemandeCongesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DemandeConges
        public async Task<IActionResult> Index()
        {
            // Rediriger vers MesDemandes pour les employés
            return RedirectToAction(nameof(MesDemandes));
        }

        // GET: DemandeConges/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var demandeConge = await _context.DemandesConges
                .Include(d => d.Employe)
                .Include(d => d.Manager)
                .Include(d => d.TypeConge)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (demandeConge == null)
            {
                return NotFound();
            }

            return View(demandeConge);
        }

        // GET: DemandeConges/MesDemandes
        public async Task<IActionResult> MesDemandes()
        {
            // Récupérer l'ID utilisateur depuis la session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            // Vérifier le rôle de l'utilisateur
            var userRole = HttpContext.Session.GetString("UserRole");
            
            // Si Admin, Manager ou RH - afficher toutes les demandes
            if (userRole == "Admin" || userRole == "Manager" || userRole == "RH")
            {
                var allDemandes = await _context.DemandesConges
                    .Include(d => d.Employe)
                        .ThenInclude(e => e.Utilisateur)
                    .Include(d => d.Manager)
                        .ThenInclude(m => m.Utilisateur)
                    .Include(d => d.TypeConge)
                    .OrderByDescending(d => d.DateDemande)
                    .ToListAsync();

                var allDemandesViewModel = new MesDemandesViewModel
                {
                    JoursTotal = 0,
                    JoursConsommes = 0,
                    JoursEnAttente = 0,
                    EmployeNom = "",
                    EmployePrenom = "",
                    EmployePoste = userRole,
                    ManagerNom = "",
                    IsModerator = true,
                    CurrentRole = userRole,
                    Demandes = allDemandes.Select(d => new DemandeCongeListViewModel
                    {
                        Id = d.Id,
                        EmployeNom = d.Employe?.Utilisateur?.Nom ?? "N/A",
                        EmployePrenom = d.Employe?.Utilisateur?.Prenom ?? "N/A",
                        EmployePoste = d.Employe?.Poste ?? "",
                        ManagerNom = d.Manager?.Utilisateur?.Nom ?? "",
                        ManagerPrenom = d.Manager?.Utilisateur?.Prenom ?? "",
                        ManagerEmail = d.Manager?.EmailProfessionnel ?? "",
                        TypeConge = d.TypeConge?.Nom ?? "N/A",
                        DateDebut = d.DateDebut,
                        DateFin = d.DateFin,
                        NombreJours = (int)(d.DateFin - d.DateDebut).TotalDays + 1,
                        Motif = d.Motif,
                        DateDemande = d.DateDemande,
                        Statut = d.Statut
                    }).ToList()
                };

                return View(allDemandesViewModel);
            }

            // Récupérer l'employé (pour le rôle Employe)
            var employe = await _context.Employes
                .Include(e => e.Utilisateur)
                .Include(e => e.Manager)
                    .ThenInclude(m => m.Utilisateur)
                .FirstOrDefaultAsync(e => e.UtilisateurId == userId.Value);

            // Si l'employé n'existe pas encore, créer un profil par défaut
            if (employe == null)
            {
                var utilisateur = await _context.Utilisateurs.FindAsync(userId.Value);
                if (utilisateur != null)
                {
                    employe = new Employe
                    {
                        UtilisateurId = userId.Value,
                        Departement = "Non défini",
                        Poste = "Employé",
                        Telephone = "",
                        EmailProfessionnel = utilisateur.Email,
                        JoursCongesTotal = 30
                    };
                    _context.Employes.Add(employe);
                    await _context.SaveChangesAsync();
                    
                    // Recharger avec les relations
                    employe = await _context.Employes
                        .Include(e => e.Utilisateur)
                        .Include(e => e.Manager)
                            .ThenInclude(m => m.Utilisateur)
                        .FirstOrDefaultAsync(e => e.Id == employe.Id);
                }
                else
                {
                    TempData["Error"] = "Votre profil utilisateur n'a pas été trouvé.";
                    return RedirectToAction("Index", "Home");
                }
            }

            // Récupérer les demandes de l'employé
            var demandes = await _context.DemandesConges
                .Include(d => d.Employe)
                    .ThenInclude(e => e.Utilisateur)
                .Include(d => d.Manager)
                    .ThenInclude(m => m.Utilisateur)
                .Include(d => d.TypeConge)
                .Where(d => d.EmployeId == employe.Id)
                .OrderByDescending(d => d.DateDemande)
                .ToListAsync();

            // Calculer le solde de congé
            var joursConsommes = demandes
                .Where(d => d.Statut == "Validé")
                .Sum(d => (int)(d.DateFin - d.DateDebut).TotalDays + 1);

            var joursEnAttente = demandes
                .Where(d => d.Statut == "En attente")
                .Sum(d => (int)(d.DateFin - d.DateDebut).TotalDays + 1);

            var viewModel = new MesDemandesViewModel
            {
                JoursTotal = employe.JoursCongesTotal,
                JoursConsommes = joursConsommes,
                JoursEnAttente = joursEnAttente,
                EmployeNom = employe.Utilisateur?.Nom ?? "N/A",
                EmployePrenom = employe.Utilisateur?.Prenom ?? "N/A",
                EmployePoste = employe.Poste ?? "N/A",
                ManagerNom = employe.Manager?.Utilisateur != null 
                    ? $"{employe.Manager.Utilisateur.Prenom} {employe.Manager.Utilisateur.Nom}" 
                    : "N/A",
                IsModerator = false,
                CurrentRole = "Employe",
                Demandes = demandes.Select(d => new DemandeCongeListViewModel
                {
                    Id = d.Id,
                    EmployeNom = d.Employe?.Utilisateur?.Nom ?? "N/A",
                    EmployePrenom = d.Employe?.Utilisateur?.Prenom ?? "N/A",
                    EmployePoste = d.Employe?.Poste ?? "",
                    ManagerNom = d.Manager?.Utilisateur?.Nom ?? "",
                    ManagerPrenom = d.Manager?.Utilisateur?.Prenom ?? "",
                    ManagerEmail = d.Manager?.EmailProfessionnel ?? "",
                    TypeConge = d.TypeConge?.Nom ?? "N/A",
                    DateDebut = d.DateDebut,
                    DateFin = d.DateFin,
                    NombreJours = (int)(d.DateFin - d.DateDebut).TotalDays + 1,
                    Motif = d.Motif,
                    DateDemande = d.DateDemande,
                    Statut = d.Statut
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: DemandeConges/Create
        public async Task<IActionResult> Create()
        {
            // Récupérer l'ID utilisateur depuis la session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            // Récupérer l'employé pour afficher son solde
            var employe = await _context.Employes
                .FirstOrDefaultAsync(e => e.UtilisateurId == userId.Value);

            if (employe != null)
            {
                // Calculer le solde en tenant compte des demandes validées et en attente
                var demandes = await _context.DemandesConges
                    .Where(d => d.EmployeId == employe.Id && (d.Statut == "Validé" || d.Statut == "En attente"))
                    .ToListAsync();

                var joursConsommes = demandes
                    .Where(d => d.Statut == "Validé")
                    .Sum(d => (int)(d.DateFin - d.DateDebut).TotalDays + 1);

                var joursEnAttente = demandes
                    .Where(d => d.Statut == "En attente")
                    .Sum(d => (int)(d.DateFin - d.DateDebut).TotalDays + 1);

                ViewData["JoursRestants"] = Math.Max(0, employe.JoursCongesTotal - joursConsommes - joursEnAttente);
                ViewData["JoursTotal"] = employe.JoursCongesTotal;
            }

            var typesConges = await _context.TypesConges.ToListAsync();
            ViewData["TypeCongeId"] = new SelectList(typesConges, "Id", "Nom");
            ViewData["TypeCongeLimits"] = typesConges.ToDictionary(t => t.Id, t => t.NombreJoursMax);
            return View();
        }


        // POST: DemandeConges/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TypeCongeId,DateDebut,DateFin,Motif")] DemandeConge demandeConge)
        {
            // Récupérer l'utilisateur connecté
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            // Récupérer l'employé lié
            var employe = await _context.Employes
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.UtilisateurId == userId.Value);

            if (employe == null)
            {
                ModelState.AddModelError(string.Empty, "Employé introuvable.");
            }
            else
            {
                // Renseigner les champs non saisis par l'utilisateur et nettoyer la validation
                demandeConge.EmployeId = employe.Id;
                demandeConge.ManagerId = employe.ManagerId;
                ModelState.Remove(nameof(demandeConge.EmployeId));
                ModelState.Remove(nameof(demandeConge.ManagerId));
                ModelState.Remove(nameof(demandeConge.Employe));
                ModelState.Remove(nameof(demandeConge.Manager));
            }

            // Validation des dates
            if (demandeConge.DateFin < demandeConge.DateDebut)
            {
                ModelState.AddModelError("DateFin", "La date de fin doit être après la date de début.");
            }

            // Validation de la durée par rapport au type de congé
            var typeConge = await _context.TypesConges.FirstOrDefaultAsync(t => t.Id == demandeConge.TypeCongeId);
            if (typeConge == null)
            {
                ModelState.AddModelError("TypeCongeId", "Type de congé introuvable.");
            }
            else
            {
                var nbJoursDemandes = (int)(demandeConge.DateFin - demandeConge.DateDebut).TotalDays + 1;
                if (nbJoursDemandes > typeConge.NombreJoursMax)
                {
                    ModelState.AddModelError("DateFin", $"La durée demandée ({nbJoursDemandes} jours) dépasse la limite de {typeConge.NombreJoursMax} jours pour ce type de congé.");
                }
            }

            // Nettoyer la validation sur la navigation TypeConge (seul l'Id est saisi)
            ModelState.Remove(nameof(demandeConge.TypeConge));

            if (!ModelState.IsValid)
            {
                // Recharger les données pour la vue
                if (employe != null)
                {
                    var demandesEmploye = await _context.DemandesConges
                        .Where(d => d.EmployeId == employe.Id && (d.Statut == "Validé" || d.Statut == "En attente"))
                        .ToListAsync();

                    var joursConsommes = demandesEmploye
                        .Where(d => d.Statut == "Validé")
                        .Sum(d => (int)(d.DateFin - d.DateDebut).TotalDays + 1);

                    var joursEnAttente = demandesEmploye
                        .Where(d => d.Statut == "En attente")
                        .Sum(d => (int)(d.DateFin - d.DateDebut).TotalDays + 1);

                    ViewData["JoursRestants"] = Math.Max(0, employe.JoursCongesTotal - joursConsommes - joursEnAttente);
                    ViewData["JoursTotal"] = employe.JoursCongesTotal;
                }

                var typesConges = await _context.TypesConges.ToListAsync();
                ViewData["TypeCongeId"] = new SelectList(typesConges, "Id", "Nom", demandeConge.TypeCongeId);
                ViewData["TypeCongeLimits"] = typesConges.ToDictionary(t => t.Id, t => t.NombreJoursMax);
                return View(demandeConge);
            }

            // Compléter automatiquement
            demandeConge.DateDemande = DateTime.Now;
            demandeConge.Statut = "En attente";

            _context.DemandesConges.Add(demandeConge);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: DemandeConges/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var demandeConge = await _context.DemandesConges.FindAsync(id);
            if (demandeConge == null)
            {
                return NotFound();
            }
            ViewData["EmployeId"] = new SelectList(_context.Employes, "Id", "Departement", demandeConge.EmployeId);
            ViewData["ManagerId"] = new SelectList(_context.Managers, "Id", "Departement", demandeConge.ManagerId);
            ViewData["TypeCongeId"] = new SelectList(_context.TypesConges, "Id", "Description", demandeConge.TypeCongeId);
            return View(demandeConge);
        }

        // POST: DemandeConges/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmployeId,TypeCongeId,DateDebut,DateFin,Motif,DateDemande,Statut,ManagerId")] DemandeConge demandeConge)
        {
            if (id != demandeConge.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(demandeConge);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DemandeCongeExists(demandeConge.Id))
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
            ViewData["EmployeId"] = new SelectList(_context.Employes, "Id", "Departement", demandeConge.EmployeId);
            ViewData["ManagerId"] = new SelectList(_context.Managers, "Id", "Departement", demandeConge.ManagerId);
            ViewData["TypeCongeId"] = new SelectList(_context.TypesConges, "Id", "Description", demandeConge.TypeCongeId);
            return View(demandeConge);
        }

        // GET: DemandeConges/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var demandeConge = await _context.DemandesConges
                .Include(d => d.Employe)
                .Include(d => d.Manager)
                .Include(d => d.TypeConge)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (demandeConge == null)
            {
                return NotFound();
            }

            return View(demandeConge);
        }

        // POST: DemandeConges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var demandeConge = await _context.DemandesConges.FindAsync(id);
            if (demandeConge != null)
            {
                _context.DemandesConges.Remove(demandeConge);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: DemandeConges/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Manager" && role != "RH")
            {
                return Forbid();
            }

            var demande = await _context.DemandesConges.FindAsync(id);
            if (demande == null)
            {
                return NotFound();
            }

            demande.Statut = "Validé";
            _context.Update(demande);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: DemandeConges/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Manager" && role != "RH")
            {
                return Forbid();
            }

            var demande = await _context.DemandesConges.FindAsync(id);
            if (demande == null)
            {
                return NotFound();
            }

            demande.Statut = "Refusé";
            _context.Update(demande);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool DemandeCongeExists(int id)
        {
            return _context.DemandesConges.Any(e => e.Id == id);
        }
    }
}
