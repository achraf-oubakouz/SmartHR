using System;

namespace SmartHR.Models
{
    public class Calendriers
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public string? Description { get; set; }
        public int? EmployeId { get; set; }
        public Employe? Employe { get; set; }
    }
}
