using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartHR.Models;

namespace SmartHR.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Documents
        public async Task<IActionResult> Index()
        {
            // Documents functionality disabled
            return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (!userId.HasValue)
            {
                ViewBag.IsLoggedIn = false;
                return View();
            }

            ViewBag.IsLoggedIn = true;
            ViewBag.UserId = userId.Value;
            ViewBag.UserRole = userRole;

            // Récupérer l'employé connecté
            var employe = await _context.Employes
                .Include(e => e.Utilisateur)
                .FirstOrDefaultAsync(e => e.UtilisateurId == userId);

            if (employe != null)
            {
                ViewBag.Employe = employe;
                ViewBag.EmployeId = employe.Id;
            }

            // Simuler des documents disponibles
            var documents = GetDocumentsDisponibles(employe?.Id ?? 0);
            ViewBag.Documents = documents;

            // Statistiques
            var stats = new DocumentStats
            {
                TotalDocuments = documents.Count,
                DocumentsRecents = documents.Count(d => d.DateCreation >= DateTime.Now.AddMonths(-1)),
                DocumentsNonLus = documents.Count(d => !d.EstLu)
            };
            ViewBag.Stats = stats;

            return View();
        }

        // Simuler les documents disponibles
        private List<DocumentInfo> GetDocumentsDisponibles(int employeId)
        {
            var documents = new List<DocumentInfo>();
            var now = DateTime.Now;

            // Bulletins de paie (12 derniers mois)
            for (int i = 0; i < 12; i++)
            {
                var date = now.AddMonths(-i);
                documents.Add(new DocumentInfo
                {
                    Id = 100 + i,
                    Nom = $"Bulletin de paie - {date:MMMM yyyy}",
                    Type = "Bulletin de paie",
                    DateCreation = new DateTime(date.Year, date.Month, 25),
                    Taille = "45 Ko",
                    Format = "PDF",
                    EstLu = i > 0,
                    Icone = "💰"
                });
            }

            // Contrat
            documents.Add(new DocumentInfo
            {
                Id = 1,
                Nom = "Contrat de travail CDI",
                Type = "Contrat",
                DateCreation = DateTime.Now.AddYears(-2),
                Taille = "120 Ko",
                Format = "PDF",
                EstLu = true,
                Icone = "📝"
            });

            // Avenants
            documents.Add(new DocumentInfo
            {
                Id = 2,
                Nom = "Avenant au contrat - Augmentation",
                Type = "Avenant",
                DateCreation = DateTime.Now.AddMonths(-6),
                Taille = "35 Ko",
                Format = "PDF",
                EstLu = true,
                Icone = "📄"
            });

            // Attestations
            documents.Add(new DocumentInfo
            {
                Id = 3,
                Nom = "Attestation employeur",
                Type = "Attestation",
                DateCreation = DateTime.Now.AddDays(-15),
                Taille = "28 Ko",
                Format = "PDF",
                EstLu = false,
                Icone = "📜"
            });

            // Solde de congés
            documents.Add(new DocumentInfo
            {
                Id = 4,
                Nom = $"Solde de congés - {now:MMMM yyyy}",
                Type = "Solde congés",
                DateCreation = now,
                Taille = "15 Ko",
                Format = "PDF",
                EstLu = false,
                Icone = "🏖️"
            });

            return documents.OrderByDescending(d => d.DateCreation).ToList();
        }

        // GET: /Documents/Download/{id}
        public IActionResult Download(int id, string type)
        {
            // Documents functionality disabled
            return NotFound();
        }

        // GET: /Documents/Demander
        public IActionResult Demander()
        {
            // Documents functionality disabled
            return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.TypesDocuments = new List<string>
            {
                "Attestation de travail",
                "Attestation de salaire",
                "Certificat de travail",
                "Relevé d'heures",
                "Autre"
            };

            return View();
        }

        // POST: /Documents/Demander
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Demander(string typeDocument, string motif)
        {
            // Documents functionality disabled
            return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Home");
            }

            // Dans une vraie application, on enregistrerait la demande en base de données
            TempData["Message"] = $"Votre demande de '{typeDocument}' a été envoyée au service RH.";
            TempData["MessageType"] = "success";

            return RedirectToAction(nameof(Index));
        }
    }

    // Classes auxiliaires
    public class DocumentInfo
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; }
        public string Taille { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public bool EstLu { get; set; }
        public string Icone { get; set; } = "📄";
    }

    public class DocumentStats
    {
        public int TotalDocuments { get; set; }
        public int DocumentsRecents { get; set; }
        public int DocumentsNonLus { get; set; }
    }
}
