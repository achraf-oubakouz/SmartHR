using SMART_HR.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHR.Core.Entities
{
    public class Employe
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UtilisateurId { get; set; }

        [ForeignKey("UtilisateurId")]
        public virtual Utilisateur Utilisateur { get; set; }

        [Required]
        [MaxLength(100)]
        public string Departement { get; set; }

        [MaxLength(100)]
        public string Poste { get; set; }

        [MaxLength(15)]
        public string Telephone { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string EmailProfessionnel { get; set; }

        // 🔹 Relations
        public int? ManagerId { get; set; }  // Le manager qui supervise cet employé
        [ForeignKey("ManagerId")]
        public virtual Manager Manager { get; set; }

        public virtual ICollection<DemandeConge> DemandesConges { get; set; }
        public virtual ICollection<Pointage> Pointages { get; set; }
    }
}
