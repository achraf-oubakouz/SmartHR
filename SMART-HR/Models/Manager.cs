using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHR.Models
{
    public class Manager
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
        public virtual ICollection<Employe> Employes { get; set; } // Liste des employés sous sa responsabilité
        public virtual ICollection<Rapport> Rapports { get; set; } // Rapports générés par le manager
    }
}
