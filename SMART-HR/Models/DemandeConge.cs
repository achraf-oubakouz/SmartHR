using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHR.Core.Entities
{
    public class DemandeConge
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeId { get; set; }

        [ForeignKey("EmployeId")]
        public virtual Employe Employe { get; set; }

        [Required]
        public int TypeCongeId { get; set; }

        [ForeignKey("TypeCongeId")]
        public virtual TypeConge TypeConge { get; set; }

        [Required]
        public DateTime DateDebut { get; set; }

        [Required]
        public DateTime DateFin { get; set; }

        [MaxLength(255)]
        public string Motif { get; set; }

        public DateTime DateDemande { get; set; } = DateTime.Now;

        [Required]
        public string Statut { get; set; } = "En attente"; // En attente, Accepté, Refusé

        // 🔹 Relation optionnelle avec le Manager qui valide
        public int? ManagerId { get; set; }

        [ForeignKey("ManagerId")]
        public virtual Manager Manager { get; set; }
    }
}
