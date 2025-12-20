using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartHR.Models;
using SmartHR.ViewModels;

namespace SmartHR.Controllers
{
    public class RapportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private static readonly CultureInfo FrenchCulture = new CultureInfo("fr-FR");

        public RapportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rapports - Dashboard principal
        public async Task<IActionResult> Index(int? annee)
        {
            var currentYear = annee ?? DateTime.Now.Year;
            var dashboard = new RapportDashboardViewModel
            {
                AnneeSelectionnee = currentYear
            };

            // Années disponibles (basées sur les demandes existantes)
            var anneesFromDemandes = await _context.DemandesConges
                .Select(d => d.DateDebut.Year)
                .Distinct()
                .ToListAsync();
            
            dashboard.AnneesDisponibles = anneesFromDemandes
                .Union(new[] { DateTime.Now.Year, DateTime.Now.Year - 1 })
                .Distinct()
                .OrderByDescending(a => a)
                .ToList();

            // 1. Employés par département
            await CalculerEmployesParDepartement(dashboard);

            // 2. Utilisation des types de congés
            await CalculerUtilisationTypesConges(dashboard, currentYear);

            // 3. Périodes pic (12 derniers mois)
            await CalculerPeriodesPic(dashboard);

            // 4. Tendances annuelles
            await CalculerTendancesAnnuelles(dashboard, currentYear);

            // Statistiques globales
            await CalculerStatistiquesGlobales(dashboard, currentYear);

            return View(dashboard);
        }

        private async Task CalculerEmployesParDepartement(RapportDashboardViewModel dashboard)
        {
            var employes = await _context.Employes
                .GroupBy(e => e.Departement)
                .Select(g => new EmployesParDepartementItem
                {
                    Departement = g.Key ?? "Non assigné",
                    NombreEmployes = g.Count()
                })
                .OrderByDescending(x => x.NombreEmployes)
                .ToListAsync();

            dashboard.TotalEmployes = employes.Sum(e => e.NombreEmployes);

            foreach (var item in employes)
            {
                item.Pourcentage = dashboard.TotalEmployes > 0 
                    ? Math.Round((double)item.NombreEmployes / dashboard.TotalEmployes * 100, 1) 
                    : 0;
            }

            dashboard.EmployesParDepartement = employes;
        }

        private async Task CalculerUtilisationTypesConges(RapportDashboardViewModel dashboard, int annee)
        {
            var demandes = await _context.DemandesConges
                .Include(d => d.TypeConge)
                .Where(d => d.DateDebut.Year == annee)
                .ToListAsync();

            var parType = demandes
                .GroupBy(d => d.TypeConge?.Nom ?? "Non spécifié")
                .Select(g => new UtilisationTypeCongeItem
                {
                    TypeConge = g.Key,
                    NombreDemandes = g.Count(),
                    JoursTotaux = g.Sum(d => (int)(d.DateFin - d.DateDebut).TotalDays + 1)
                })
                .OrderByDescending(x => x.NombreDemandes)
                .ToList();

            dashboard.TotalDemandesConges = parType.Sum(t => t.NombreDemandes);

            foreach (var item in parType)
            {
                item.Pourcentage = dashboard.TotalDemandesConges > 0
                    ? Math.Round((double)item.NombreDemandes / dashboard.TotalDemandesConges * 100, 1)
                    : 0;
            }

            dashboard.UtilisationTypesConges = parType;
        }

        private async Task CalculerPeriodesPic(RapportDashboardViewModel dashboard)
        {
            // Période glissante sur 12 mois (mois courant inclus)
            var dateFin = DateTime.Today;
            var dateDebut = dateFin.AddMonths(-11);

            var demandes = await _context.DemandesConges
                .Where(d => d.DateDebut.Date >= dateDebut.Date && d.DateDebut.Date <= dateFin.Date)
                .ToListAsync();

            var parMois = new List<PeriodePicItem>();

            // Construire explicitement les 12 mois, même sans demande, pour l'affichage du graphique
            for (int i = 0; i < 12; i++)
            {
                var moisDate = new DateTime(dateDebut.Year, dateDebut.Month, 1).AddMonths(i);
                var count = demandes.Count(d => d.DateDebut.Year == moisDate.Year && d.DateDebut.Month == moisDate.Month);

                parMois.Add(new PeriodePicItem
                {
                    Annee = moisDate.Year,
                    Mois = moisDate.Month,
                    NomMois = FrenchCulture.DateTimeFormat.GetMonthName(moisDate.Month),
                    NombreDemandes = count
                });
            }

            // Marquer les périodes pic (top 3)
            var maxDemandes = parMois
                .OrderByDescending(p => p.NombreDemandes)
                .Take(3)
                .Select(p => p.NombreDemandes)
                .ToList();

            foreach (var item in parMois)
            {
                item.EstPeriodePic = maxDemandes.Contains(item.NombreDemandes) && item.NombreDemandes > 0;
            }

            // Conserver l'ordre chronologique pour l'affichage (du plus ancien au plus récent)
            dashboard.PeriodesPic = parMois
                .OrderBy(p => p.Annee)
                .ThenBy(p => p.Mois)
                .ToList();
        }

        private async Task CalculerTendancesAnnuelles(RapportDashboardViewModel dashboard, int annee)
        {
            var demandes = await _context.DemandesConges
                .Where(d => d.DateDebut.Year == annee)
                .ToListAsync();

            var tendances = new List<TendanceAnnuelleItem>();
            int? moisPrecedent = null;

            for (int mois = 1; mois <= 12; mois++)
            {
                var demandesMois = demandes.Where(d => d.DateDebut.Month == mois).ToList();
                var nombreDemandes = demandesMois.Count;
                var joursConges = demandesMois.Sum(d => (int)(d.DateFin - d.DateDebut).TotalDays + 1);

                double variation = 0;
                string tendance = "stable";

                if (moisPrecedent.HasValue && moisPrecedent.Value > 0)
                {
                    variation = Math.Round(((double)nombreDemandes - moisPrecedent.Value) / moisPrecedent.Value * 100, 1);
                    tendance = variation > 10 ? "hausse" : (variation < -10 ? "baisse" : "stable");
                }
                else if (moisPrecedent.HasValue && moisPrecedent.Value == 0 && nombreDemandes > 0)
                {
                    variation = 100;
                    tendance = "hausse";
                }

                tendances.Add(new TendanceAnnuelleItem
                {
                    Mois = mois,
                    NomMois = FrenchCulture.DateTimeFormat.GetMonthName(mois),
                    NombreDemandes = nombreDemandes,
                    JoursConges = joursConges,
                    VariationPourcentage = variation,
                    Tendance = tendance
                });

                moisPrecedent = nombreDemandes;
            }

            dashboard.TendancesAnnuelles = tendances;
        }

        private async Task CalculerStatistiquesGlobales(RapportDashboardViewModel dashboard, int annee)
        {
            var demandes = await _context.DemandesConges
                .Where(d => d.DateDebut.Year == annee)
                .ToListAsync();

            // Harmonisation avec les statuts réellement utilisés dans l'application
            // "En attente"  : demande en cours de validation
            // "Validé"      : congé approuvé (flux DemandeCongesController)
            // "Accepté"     : congé approuvé (ancien flux / CongeService)
            // "Refusé"      : congé refusé
            dashboard.CongesEnAttente = demandes.Count(d => d.Statut == "En attente");
            dashboard.CongesApprouves = demandes.Count(d => d.Statut == "Validé" || d.Statut == "Accepté");
            dashboard.CongesRefuses = demandes.Count(d => d.Statut == "Refusé");

            var totalTraites = dashboard.CongesApprouves + dashboard.CongesRefuses;
            dashboard.TauxApprobation = totalTraites > 0
                ? Math.Round((double)dashboard.CongesApprouves / totalTraites * 100, 1)
                : 0;
        }

        // GET: Rapports/Historique - Liste des rapports sauvegardés
        public async Task<IActionResult> Historique()
        {
            var rapports = await _context.Rapports
                .OrderByDescending(r => r.DateGeneration)
                .ToListAsync();
            return View(rapports);
        }

        // GET: Rapports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rapport = await _context.Rapports
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rapport == null)
            {
                return NotFound();
            }

            return View(rapport);
        }

        // GET: Rapports/Create
        public IActionResult Create()
        {
            ViewData["TypesRapport"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "EmployesParDepartement", Text = "Employés par département" },
                new SelectListItem { Value = "UtilisationConges", Text = "Utilisation des types de congés" },
                new SelectListItem { Value = "PeriodesPic", Text = "Périodes de pic de demandes" },
                new SelectListItem { Value = "TendancesAnnuelles", Text = "Tendances annuelles" },
                new SelectListItem { Value = "RapportComplet", Text = "Rapport complet" }
            };
            return View();
        }

        // POST: Rapports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DateGeneration,Titre,Description,Type")] Rapport rapport)
        {
            if (ModelState.IsValid)
            {
                rapport.DateGeneration = DateTime.Now;
                _context.Add(rapport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Historique));
            }
            return View(rapport);
        }

        // GET: Rapports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rapport = await _context.Rapports.FindAsync(id);
            if (rapport == null)
            {
                return NotFound();
            }
            return View(rapport);
        }

        // POST: Rapports/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DateGeneration,Titre,Description,Type")] Rapport rapport)
        {
            if (id != rapport.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rapport);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RapportExists(rapport.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Historique));
            }
            return View(rapport);
        }

        // GET: Rapports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rapport = await _context.Rapports
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rapport == null)
            {
                return NotFound();
            }

            return View(rapport);
        }

        // POST: Rapports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rapport = await _context.Rapports.FindAsync(id);
            if (rapport != null)
            {
                _context.Rapports.Remove(rapport);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Historique));
        }

        private bool RapportExists(int id)
        {
            return _context.Rapports.Any(e => e.Id == id);
        }
    }
}
