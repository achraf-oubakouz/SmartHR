using SMART_HR.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace SmartHR.Core.Entities
{
    public class Utilisateur
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nom { get; set; }

        [Required]
        [MaxLength(50)]
        public string Prenom { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(100)]
        public string MotDePasse { get; set; }

        [Required]
        public string Role { get; set; }  

        public bool Actif { get; set; } = true;

        // 🔹 Relations 1-1 avec d'autres entités
        public virtual Employe Employe { get; set; }
        public virtual Admin Administrateur { get; set; }
        public virtual Manager Manager { get; set; }
        public virtual RessourceHumaine RH{ get; set; }
    }
}