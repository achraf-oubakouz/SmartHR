using System.Collections.Generic;
using SmartHR.Models;
namespace SmartHR.Services.Interfaces
{
    public interface IUtilisateurService
    {
        List<Utilisateur> GetAll();
        Utilisateur GetById(int id);
        void Create(Utilisateur user);
        void Update(Utilisateur user);
        void Delete(int id);
    }
}