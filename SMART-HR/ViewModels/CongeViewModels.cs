using System;
using System.ComponentModel.DataAnnotations;

namespace SmartHR.ViewModels
{
    public class DemandeCongeListViewModel
    {
        public int Id { get; set; }
        
        // Informations Employé
        public string EmployeNom { get; set; }
        public string EmployePrenom { get; set; }
        public string EmployePoste { get; set; }
        
        // Informations Manager
        public string ManagerNom { get; set; }
        public string ManagerPrenom { get; set; }
        public string ManagerEmail { get; set; }
        
        // Informations Congé
        public string TypeConge { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int NombreJours { get; set; }
        public string Motif { get; set; }
        
        // Informations Demande
        public DateTime DateDemande { get; set; }
        public string Statut { get; set; } // En attente, Validé, Refusé
        
        // Helper pour le CSS
        public string StatutClass => Statut switch
        {
            "Validé" => "status-approved",
            "Refusé" => "status-rejected",
            _ => "status-pending"
        };
        
        public string StatutIcon => Statut switch
        {
            "Validé" => "✓",
            "Refusé" => "✗",
            _ => "⏱"
        };
    }
    
    public class CreateDemandeCongeViewModel
    {
        [Required(ErrorMessage = "Le type de congé est requis")]
        [Display(Name = "Type de congé")]
        public int TypeCongeId { get; set; }
        
        [Required(ErrorMessage = "La date de début est requise")]
        [Display(Name = "Date de début")]
        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }
        
        [Required(ErrorMessage = "La date de fin est requise")]
        [Display(Name = "Date de fin")]
        [DataType(DataType.Date)]
        public DateTime DateFin { get; set; }
        
        [MaxLength(500, ErrorMessage = "Le motif ne peut pas dépasser 500 caractères")]
        [Display(Name = "Motif (optionnel)")]
        public string Motif { get; set; }
    }
    
    public class CongeDetailsViewModel
    {
        public int Id { get; set; }
        
        // Employé
        public string EmployeNomComplet { get; set; }
        public string EmployePoste { get; set; }
        public string EmployeDepartement { get; set; }
        public string EmployeEmail { get; set; }
        public string EmployeTelephone { get; set; }
        
        // Manager
        public string ManagerNomComplet { get; set; }
        public string ManagerEmail { get; set; }
        
        // Congé
        public string TypeConge { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int NombreJours { get; set; }
        public string Motif { get; set; }
        
        // Demande
        public DateTime DateDemande { get; set; }
        public string Statut { get; set; }
        public DateTime? DateTraitement { get; set; }
        
        public bool PeutValider { get; set; } // Si l'utilisateur est manager/admin
    }

    public class MesDemandesViewModel
    {
        // Solde de congé
        public int JoursTotal { get; set; }
        public int JoursConsommes { get; set; }
        public int JoursRestants => JoursTotal - JoursConsommes;
        public int JoursEnAttente { get; set; }
        
        // Informations employé
        public string EmployeNom { get; set; }
        public string EmployePrenom { get; set; }
        public string EmployePoste { get; set; }
        public string ManagerNom { get; set; }

        // Contexte utilisateur connecté
        public bool IsModerator { get; set; } // Admin, Manager, RH
        public string CurrentRole { get; set; }
        
        // Liste des demandes
        public List<DemandeCongeListViewModel> Demandes { get; set; }
    }
}

