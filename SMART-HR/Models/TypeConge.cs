using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartHR.Models
{
    public class TypeConge
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nom { get; set; } // Exemple : "Congé annuel", "Congé maladie", "Maternité"

        [Required]
        [MaxLength(255)]
        public string Description { get; set; } // Détails du type de congé

        [Required]
        public int NombreJoursMax { get; set; } // Exemple : 30 pour congé annuel

        public bool EstRenumere { get; set; } // True si le congé est payé

        // 🔹 Relation avec DemandeConge
        public virtual ICollection<DemandeConge> DemandesConges { get; set; }
    }
}
