using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SMART_HR.Models;

namespace SMART_HR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public IActionResult Index()
        {
            var news = LoadCompanyNews()
                .OrderByDescending(n => n.Date)
                .ToList();

            return View(news);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private IEnumerable<CompanyNewsItem> LoadCompanyNews()
        {
            try
            {
                var dataPath = Path.Combine(_environment.ContentRootPath, "App_Data", "companyNews.json");
                if (!System.IO.File.Exists(dataPath))
                {
                    _logger.LogWarning("Le fichier d'actualités {Path} est introuvable.", dataPath);
                    return Enumerable.Empty<CompanyNewsItem>();
                }

                var json = System.IO.File.ReadAllText(dataPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var items = JsonSerializer.Deserialize<List<CompanyNewsItem>>(json, options);
                return items ?? Enumerable.Empty<CompanyNewsItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des actualités de l'entreprise.");
                return Enumerable.Empty<CompanyNewsItem>();
            }
        }
    }
}
