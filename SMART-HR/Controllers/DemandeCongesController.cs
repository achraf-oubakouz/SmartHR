using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartHR.Models;

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
            var applicationDbContext = _context.DemandesConges.Include(d => d.Employe).Include(d => d.Manager).Include(d => d.TypeConge);
            return View(await applicationDbContext.ToListAsync());
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

        // GET: DemandeConges/Create
        public IActionResult Create()
        {
            ViewData["TypeCongeId"] = new SelectList(_context.TypesConges, "Id", "Description");
            return View();
        }


        // POST: DemandeConges/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TypeCongeId,DateDebut,DateFin,Motif")] DemandeConge demandeConge)
        {
            if (!ModelState.IsValid)
                return View(demandeConge);

            // 🔹 1. Récupérer l’employé connecté
            var employe = await _context.Employes
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.UtilisateurId == 1); // Remplace 1 par l'ID utilisateur connecté

            if (employe == null)
                return BadRequest("Employé non trouvé.");

            // 🔹 2. Compléter automatiquement
            demandeConge.EmployeId = employe.Id;
            demandeConge.ManagerId = employe.ManagerId;
            demandeConge.DateDemande = DateTime.Now;
            demandeConge.Statut = "En attente";

            // 🔹 3. Ajouter à la base
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

        private bool DemandeCongeExists(int id)
        {
            return _context.DemandesConges.Any(e => e.Id == id);
        }
    }
}
