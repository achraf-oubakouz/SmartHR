using System.Collections.Generic;
using System.Linq;
using SmartHR.Models;
using SmartHR;
using SmartHR.Services.Interfaces;

namespace SmartHR.Services.Implementations
{
    public class UtilisateurService : IUtilisateurService
    {
        private readonly ApplicationDbContext _context;

        public UtilisateurService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Utilisateur> GetAll()
        {
            return _context.Utilisateurs.ToList();
        }

        public Utilisateur GetById(int id)
        {
            return _context.Utilisateurs.FirstOrDefault(u => u.Id == id);
        }

        public void Create(Utilisateur user)
        {
            _context.Utilisateurs.Add(user);
            _context.SaveChanges();
        }

        public void Update(Utilisateur user)
        {
            _context.Utilisateurs.Update(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = _context.Utilisateurs.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _context.Utilisateurs.Remove(user);
                _context.SaveChanges();
            }
        }
    }
}
