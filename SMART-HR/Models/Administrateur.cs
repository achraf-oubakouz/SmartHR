using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHR.Core.Entities
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UtilisateurId { get; set; }

        [ForeignKey("UtilisateurId")]
        public virtual Utilisateur Utilisateur { get; set; }

        [Required]
        [MaxLength(100)]
        public string Departement { get; set; } // Exemple : "IT", "Administration"

        [MaxLength(100)]
        public string Poste { get; set; } // Exemple : "Administrateur principal"

        [MaxLength(15)]
        public string Telephone { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string EmailProfessionnel { get; set; }
    }
}
