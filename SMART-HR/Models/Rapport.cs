using SMART_HR.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHR.Core.Entities
{
    public class Rapport
    {
        public int Id { get; set; }
        public DateTime DateGeneration { get; set; }
        public string Titre { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string FichierPath { get; set; }

        public int? ManagerId { get; set; }      //  ← must have the ?
        public Manager? Manager { get; set; }    //  ← optional navigation
    }
}
