using System;
using System.Collections.Generic;
using SmartHR.Models;

namespace SmartHR.Services.Interfaces
{
	public class LeaveBalanceItem
	{
		public int TypeCongeId { get; set; }
		public string TypeCongeNom { get; set; }
		public int AutoriseJours { get; set; }
		public int UtiliseJours { get; set; }
		public int ResteJours => Math.Max(AutoriseJours - UtiliseJours, 0);
	}

	public interface ICongeService
	{
		IReadOnlyList<LeaveBalanceItem> GetLeaveBalance(int employeId);
		IReadOnlyList<DemandeConge> GetRequests(int employeId);
		IReadOnlyList<DemandeConge> GetHistory(int employeId);
		void RequestLeave(DemandeConge demande);

		// Team calendar = approved leaves for teammates + fixed holidays
		(IReadOnlyList<(DateTime date, string label)> joursFeries, IReadOnlyList<DemandeConge> congesEquipe) GetTeamCalendar(int employeId);
	}
}

