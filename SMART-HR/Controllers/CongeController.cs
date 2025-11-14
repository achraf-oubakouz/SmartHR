using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHR.Models;
using SmartHR.Services.Interfaces;

namespace SmartHR.Controllers
{
	public class CongeController : Controller
	{
		private readonly ICongeService _congeService;
		private readonly ApplicationDbContext _context;

		public CongeController(ICongeService congeService, ApplicationDbContext context)
		{
			_congeService = congeService;
			_context = context;
		}

		// GET: /Conge/Solde?employeId=1
		public IActionResult Solde(int employeId)
		{
			var balance = _congeService.GetLeaveBalance(employeId);
			ViewBag.EmployeId = employeId;
			return View(balance);
		}

		// GET: /Conge/Demander?employeId=1
		public IActionResult Demander(int employeId)
		{
			ViewBag.Types = _context.TypesConges.AsNoTracking().OrderBy(t => t.Nom).ToList();
			ViewBag.EmployeId = employeId;
			return View(new DemandeConge { EmployeId = employeId, DateDebut = DateTime.Today, DateFin = DateTime.Today });
		}

		// POST: /Conge/Demander
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Demander(DemandeConge model)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Types = _context.TypesConges.AsNoTracking().OrderBy(t => t.Nom).ToList();
				ViewBag.EmployeId = model.EmployeId;
				return View(model);
			}

			try
			{
				_congeService.RequestLeave(model);
				return RedirectToAction(nameof(Suivi), new { employeId = model.EmployeId });
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				ViewBag.Types = _context.TypesConges.AsNoTracking().OrderBy(t => t.Nom).ToList();
				ViewBag.EmployeId = model.EmployeId;
				return View(model);
			}
		}

		// GET: /Conge/Suivi?employeId=1
		public IActionResult Suivi(int employeId)
		{
			var demandes = _congeService.GetRequests(employeId);
			ViewBag.EmployeId = employeId;
			return View(demandes);
		}

		// GET: /Conge/Historique?employeId=1
		public IActionResult Historique(int employeId)
		{
			var demandes = _congeService.GetHistory(employeId);
			ViewBag.EmployeId = employeId;
			return View(demandes);
		}
	}
}

