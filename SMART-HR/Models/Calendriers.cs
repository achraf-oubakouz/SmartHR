using System;

namespace SmartHR.Models
{
    public class Calendriers
    {
        public int Id { get; set; }

        // Titre de l'événement
        public string Titre { get; set; }

        // Type : Férié, Evénement, Congé, Autre...
        public string Type { get; set; }

        // Début d'événement
        public DateTime DateDebut { get; set; }

        // Fin d'événement
        public DateTime DateFin { get; set; }

        // Optionnel : Description ou note
        public string Description { get; set; }

        // Lié à un employé ? (pour afficher les congés approuvés)
        public int? EmployeId { get; set; }
        public Employe Employe { get; set; }
    }
}
