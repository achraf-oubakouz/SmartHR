using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public CalendriersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Calendrier
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Calendriers.Include(c => c.Employe);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Calendrier/Details/5
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

        // GET: Calendrier/Create
        public IActionResult Create()
        {
            ViewData["EmployeId"] = new SelectList(_context.Employes, "Id", "Departement");
            return View();
        }

        // POST: Calendrier/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titre,Type,DateDebut,DateFin,Description,EmployeId")] Calendriers calendrier)
        {
            if (ModelState.IsValid)
            {
                _context.Add(calendrier);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeId"] = new SelectList(_context.Employes, "Id", "Departement", calendrier.EmployeId);
            return View(calendrier);
        }

        // GET: Calendrier/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calendrier = await _context.Calendriers.FindAsync(id);
            if (calendrier == null)
            {
                return NotFound();
            }
            ViewData["EmployeId"] = new SelectList(_context.Employes, "Id", "Departement", calendrier.EmployeId);
            return View(calendrier);
        }

        // POST: Calendrier/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titre,Type,DateDebut,DateFin,Description,EmployeId")] Calendriers calendrier)
        {
            if (id != calendrier.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
            ViewData["EmployeId"] = new SelectList(_context.Employes, "Id", "Departement", calendrier.EmployeId);
            return View(calendrier);
        }

        // GET: Calendrier/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Calendrier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
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
