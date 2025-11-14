using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartHR.Models;
using SmartHR.Services.Interfaces;

namespace SmartHR.Services.Implementations
{
	public class CongeService : ICongeService
	{
		private readonly ApplicationDbContext _context;

		public CongeService(ApplicationDbContext context)
		{
			_context = context;
		}

		public IReadOnlyList<LeaveBalanceItem> GetLeaveBalance(int employeId)
		{
			var employe = _context.Employes.FirstOrDefault(e => e.Id == employeId);
			if (employe == null) return Array.Empty<LeaveBalanceItem>();

			var usedByType = _context.DemandesConges
				.Where(d => d.EmployeId == employeId && d.Statut == "Accepté")
				.AsEnumerable() // compute duration in memory
				.GroupBy(d => d.TypeCongeId)
				.ToDictionary(
					g => g.Key,
					g => g.Sum(d => (int)(d.DateFin.Date - d.DateDebut.Date).TotalDays + 1)
				);

			var items = _context.TypesConges
				.Select(tc => new LeaveBalanceItem
				{
					TypeCongeId = tc.Id,
					TypeCongeNom = tc.Nom,
					AutoriseJours = tc.NombreJoursMax,
					UtiliseJours = usedByType.ContainsKey(tc.Id) ? usedByType[tc.Id] : 0
				})
				.OrderBy(i => i.TypeCongeNom)
				.ToList();

			return items;
		}

		public IReadOnlyList<DemandeConge> GetRequests(int employeId)
		{
			return _context.DemandesConges
				.Include(d => d.TypeConge)
				.Where(d => d.EmployeId == employeId)
				.OrderByDescending(d => d.DateDemande)
				.ToList();
		}

		public IReadOnlyList<DemandeConge> GetHistory(int employeId)
		{
			return _context.DemandesConges
				.Include(d => d.TypeConge)
				.Where(d => d.EmployeId == employeId)
				.OrderByDescending(d => d.DateDebut)
				.ToList();
		}

		public void RequestLeave(DemandeConge demande)
		{
			if (demande.DateFin.Date < demande.DateDebut.Date)
			{
				throw new InvalidOperationException("La date de fin doit être postérieure à la date de début.");
			}

			var type = _context.TypesConges.FirstOrDefault(t => t.Id == demande.TypeCongeId);
			if (type == null) throw new InvalidOperationException("Type de congé introuvable.");

			var joursDemandes = (int)(demande.DateFin.Date - demande.DateDebut.Date).TotalDays + 1;
			if (joursDemandes <= 0) throw new InvalidOperationException("La durée de congé doit être positive.");

			// Verify remaining balance
			var balance = GetLeaveBalance(demande.EmployeId)
				.FirstOrDefault(b => b.TypeCongeId == demande.TypeCongeId);
			if (balance != null && joursDemandes > balance.ResteJours)
			{
				throw new InvalidOperationException("Solde de congé insuffisant pour ce type.");
			}

			// Basic overlap check with approved leaves
			var overlap = _context.DemandesConges.Any(d =>
				d.EmployeId == demande.EmployeId &&
				d.Statut == "Accepté" &&
				d.DateDebut.Date <= demande.DateFin.Date &&
				d.DateFin.Date >= demande.DateDebut.Date);
			if (overlap)
			{
				throw new InvalidOperationException("Chevauchement avec un congé approuvé existant.");
			}

			demande.DateDemande = DateTime.Now;
			demande.Statut = "En attente";

			_context.DemandesConges.Add(demande);
			_context.SaveChanges();
		}

		public (IReadOnlyList<(DateTime date, string label)> joursFeries, IReadOnlyList<DemandeConge> congesEquipe)
			GetTeamCalendar(int employeId)
		{
			var employe = _context.Employes.FirstOrDefault(e => e.Id == employeId);
			if (employe == null) return (Array.Empty<(DateTime, string)>(), Array.Empty<DemandeConge>());

			var managerId = employe.ManagerId;
			var teammateIds = _context.Employes
				.Where(e => e.ManagerId == managerId)
				.Select(e => e.Id)
				.ToList();

			var upcoming = DateTime.Today.AddMonths(3);
			var congesEquipe = _context.DemandesConges
				.Include(d => d.TypeConge)
				.Where(d =>
					teammateIds.Contains(d.EmployeId) &&
					d.Statut == "Accepté" &&
					d.DateFin >= DateTime.Today &&
					d.DateDebut <= upcoming)
				.OrderBy(d => d.DateDebut)
				.ToList();

			// Simple static holiday list (replace with DB later if needed)
			var annee = DateTime.Today.Year;
			var joursFeries = new List<(DateTime, string)>
			{
				(new DateTime(annee, 1, 1), "Jour de l'An"),
				(new DateTime(annee, 5, 1), "Fête du Travail"),
				(new DateTime(annee, 7, 30), "Fête du Trône"),
				(new DateTime(annee, 11, 18), "Fête de l'Indépendance")
			};

			return (joursFeries, congesEquipe);
		}
	}
}

