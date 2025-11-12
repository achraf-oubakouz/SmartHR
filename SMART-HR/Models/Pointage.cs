using SMART_HR.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHR.Core.Entities
{
    public class Pointage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeId { get; set; }

        [ForeignKey("EmployeId")]
        public virtual Employe Employe { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public TimeSpan? HeureArrivee { get; set; }

        public TimeSpan? HeureDepart { get; set; }

        public bool EstPresent { get; set; }

        public string Commentaire { get; set; }
    }
}
