using System;
using System.Collections.Generic;
using SmartHR.Models;
using SmartHR.Services.Interfaces;

namespace SmartHR.ViewModels
{
	public class TeamCalendarViewModel
	{
		public int EmployeId { get; set; }
		public IReadOnlyList<(DateTime date, string label)> JoursFeries { get; set; }
		public IReadOnlyList<DemandeConge> CongesEquipe { get; set; }
	}
}

