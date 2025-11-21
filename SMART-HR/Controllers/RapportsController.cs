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
    public class RapportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RapportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rapports
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Rapports.Include(r => r.Manager);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Rapports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rapport = await _context.Rapports
                .Include(r => r.Manager)
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
            ViewData["ManagerId"] = new SelectList(_context.Managers, "Id", "Departement");
            return View();
        }

        // POST: Rapports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DateGeneration,Titre,Description,Type,FichierPath,ManagerId")] Rapport rapport)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rapport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ManagerId"] = new SelectList(_context.Managers, "Id", "Departement", rapport.ManagerId);
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
            ViewData["ManagerId"] = new SelectList(_context.Managers, "Id", "Departement", rapport.ManagerId);
            return View(rapport);
        }

        // POST: Rapports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DateGeneration,Titre,Description,Type,FichierPath,ManagerId")] Rapport rapport)
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["ManagerId"] = new SelectList(_context.Managers, "Id", "Departement", rapport.ManagerId);
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
                .Include(r => r.Manager)
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
            return RedirectToAction(nameof(Index));
        }

        private bool RapportExists(int id)
        {
            return _context.Rapports.Any(e => e.Id == id);
        }
    }
}
