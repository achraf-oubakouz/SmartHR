using Microsoft.AspNetCore.Mvc;
using SmartHR.Services.Interfaces;
using SmartHR.ViewModels;

namespace SmartHR.Controllers
{
	public class CalendrierController : Controller
	{
		private readonly ICongeService _congeService;

		public CalendrierController(ICongeService congeService)
		{
			_congeService = congeService;
		}

		// GET: /Calendrier/Equipe?employeId=1
		public IActionResult Equipe(int employeId)
		{
			var (joursFeries, congesEquipe) = _congeService.GetTeamCalendar(employeId);
			var vm = new TeamCalendarViewModel
			{
				EmployeId = employeId,
				JoursFeries = joursFeries,
				CongesEquipe = congesEquipe
			};
			return View(vm);
		}
	}
}

