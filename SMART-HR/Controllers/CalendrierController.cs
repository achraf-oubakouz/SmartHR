using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartHR;
using SmartHR.Models;

namespace SmartHR.Controllers
{
    public class CalendriersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private static readonly CultureInfo FrenchCulture = new CultureInfo("fr-FR");

        public CalendriersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Calendriers - Vue principale du calendrier
        public async Task<IActionResult> Index(int? mois, int? annee)
        {
            var currentDate = DateTime.Now;
            var selectedMonth = mois ?? currentDate.Month;
            var selectedYear = annee ?? currentDate.Year;

            // Récupérer l'utilisateur connecté
            var userId = HttpContext.Session.GetInt32("UserId");

            // Récupérer les événements du calendrier (globaux)
            var calendrierEvents = await _context.Calendriers
                .Include(c => c.Employe)
                .Where(c => (c.DateDebut.Month == selectedMonth && c.DateDebut.Year == selectedYear) ||
                           (c.DateFin.Month == selectedMonth && c.DateFin.Year == selectedYear))
                .ToListAsync();

            // Récupérer les congés approuvés pour ce mois, uniquement pour l'utilisateur connecté si possible
            IQueryable<DemandeConge> congesQuery = _context.DemandesConges
                .Include(d => d.Employe)
                .Include(d => d.TypeConge)
                .Where(d => d.Statut == "Validé" &&
                           ((d.DateDebut.Month == selectedMonth && d.DateDebut.Year == selectedYear) ||
                            (d.DateFin.Month == selectedMonth && d.DateFin.Year == selectedYear)));

            if (userId.HasValue)
            {
                var employe = await _context.Employes.FirstOrDefaultAsync(e => e.UtilisateurId == userId.Value);
                if (employe != null)
                {
                    congesQuery = congesQuery.Where(d => d.EmployeId == employe.Id);
                }
                else
                {
                    // Aucun profil employé associé : pas de congés affichés pour ce user
                    congesQuery = congesQuery.Where(d => false);
                }
            }

            var congesApprouves = await congesQuery.ToListAsync();

            // Jours fériés français (statiques pour l'exemple)
            var joursFeries = GetJoursFeries(selectedYear);

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.MonthName = FrenchCulture.DateTimeFormat.GetMonthName(selectedMonth);
            ViewBag.DaysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);
            ViewBag.FirstDayOfMonth = new DateTime(selectedYear, selectedMonth, 1).DayOfWeek;
            ViewBag.CongesApprouves = congesApprouves;
            ViewBag.JoursFeries = joursFeries;
            ViewBag.Today = currentDate;

            return View(calendrierEvents);
        }

        // Jours fériés français
        private List<(DateTime date, string nom)> GetJoursFeries(int annee)
        {
            var paques = CalculerPaques(annee);
            return new List<(DateTime, string)>
            {
                (new DateTime(annee, 1, 1), "Jour de l'An"),
                (paques.AddDays(1), "Lundi de Pâques"),
                (new DateTime(annee, 5, 1), "Fête du Travail"),
                (new DateTime(annee, 5, 8), "Victoire 1945"),
                (paques.AddDays(39), "Ascension"),
                (paques.AddDays(50), "Lundi de Pentecôte"),
                (new DateTime(annee, 7, 14), "Fête Nationale"),
                (new DateTime(annee, 8, 15), "Assomption"),
                (new DateTime(annee, 11, 1), "Toussaint"),
                (new DateTime(annee, 11, 11), "Armistice"),
                (new DateTime(annee, 12, 25), "Noël")
            };
        }

        private DateTime CalculerPaques(int annee)
        {
            int a = annee % 19;
            int b = annee / 100;
            int c = annee % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int month = (h + l - 7 * m + 114) / 31;
            int day = ((h + l - 7 * m + 114) % 31) + 1;
            return new DateTime(annee, month, day);
        }

        // GET: Calendriers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calendrier = await _context.Calendriers
                .Include(c => c.Employe)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calendrier == null)
            {
                return NotFound();
            }

            return View(calendrier);
        }

        // GET: Calendriers/Create
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Employe" || string.IsNullOrEmpty(role))
            {
                return Forbid();
            }

            ViewData["Types"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "Férié", Text = "Jour Férié" },
                new SelectListItem { Value = "Événement", Text = "Événement" },
                new SelectListItem { Value = "Réunion", Text = "Réunion" },
                new SelectListItem { Value = "Formation", Text = "Formation" },
                new SelectListItem { Value = "Autre", Text = "Autre" }
            };
            return View();
        }

        // POST: Calendriers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titre,Type,DateDebut,DateFin,Description")] Calendriers calendrier)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Employe" || string.IsNullOrEmpty(role))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                _context.Add(calendrier);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(calendrier);
        }

        // GET: Calendriers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Employe" || string.IsNullOrEmpty(role))
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var calendrier = await _context.Calendriers.FindAsync(id);
            if (calendrier == null)
            {
                return NotFound();
            }
            ViewData["Types"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "Férié", Text = "Jour Férié" },
                new SelectListItem { Value = "Événement", Text = "Événement" },
                new SelectListItem { Value = "Réunion", Text = "Réunion" },
                new SelectListItem { Value = "Formation", Text = "Formation" },
                new SelectListItem { Value = "Autre", Text = "Autre" }
            };
            return View(calendrier);
        }

        // POST: Calendriers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titre,Type,DateDebut,DateFin,Description,EmployeId")] Calendriers calendrier)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Employe" || string.IsNullOrEmpty(role))
            {
                return Forbid();
            }

            if (id != calendrier.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Préserver l'EmployeId existant (non modifié depuis l'UI)
                    _context.Entry(calendrier).Property(c => c.EmployeId).IsModified = false;
                    _context.Update(calendrier);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CalendrierExists(calendrier.Id))
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
            return View(calendrier);
        }

        // GET: Calendriers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Employe" || string.IsNullOrEmpty(role))
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var calendrier = await _context.Calendriers
                .Include(c => c.Employe)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calendrier == null)
            {
                return NotFound();
            }

            return View(calendrier);
        }

        // POST: Calendriers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Employe" || string.IsNullOrEmpty(role))
            {
                return Forbid();
            }

            var calendrier = await _context.Calendriers.FindAsync(id);
            if (calendrier != null)
            {
                _context.Calendriers.Remove(calendrier);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CalendrierExists(int id)
        {
            return _context.Calendriers.Any(e => e.Id == id);
        }
    }
}
