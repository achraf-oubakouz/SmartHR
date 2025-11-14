using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace SmartHR.Controllers
{
	public class DocumentsController : Controller
	{
		// GET: /Documents?employeId=1
		public IActionResult Index(int employeId)
		{
			ViewBag.EmployeId = employeId;
			return View();
		}

		// GET: /Documents/Download?employeId=1&type=bulletin
		public IActionResult Download(int employeId, string type)
		{
			if (string.IsNullOrWhiteSpace(type))
			{
				return BadRequest();
			}

			// Expected path: wwwroot/docs/{employeId}/{type}.pdf
			var fileName = $"{type}.pdf";
			var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "docs", employeId.ToString(), fileName);
			if (!System.IO.File.Exists(path))
			{
				return NotFound("Document introuvable pour le moment.");
			}

			var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
			return File(stream, "application/pdf", fileName);
		}
	}
}

