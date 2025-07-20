using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wedding.Model.DB;
using Wedding.Repository.Interfaces;

namespace Wedding.Repository.Services
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly Supabase_WeddingDbContext _context;

        public PhotoRepository(Supabase_WeddingDbContext context)
        {
            _context = context;
        }

        public async Task<List<PhotoDb>> GetAllAsync()
        {
            return await _context.Photos.Where(a=>a.IsActive).OrderByDescending(x => x.created_at).ToListAsync();
        }

        public async Task<PhotoDb> GetByIdAsync(int id)
        {
            return await _context.Photos.Where(a => a.IsActive && a.id == id).OrderByDescending(a=>a.created_at).FirstOrDefaultAsync();
        }

        public async Task AddAsync(PhotoDb photo)
        {
            await _context.Photos.AddAsync(photo);
        }

        public void Add(PhotoDb photo)
        {
            _context.Photos.Add(photo);
        }
        public EntityEntry<PhotoDb> Remove(PhotoDb photo)
        {
            photo.IsActive = false;

            return _context.Photos.Update(photo);
        }

        public async Task SaveAsync()
        {
            var count = await _context.SaveChangesAsync();
            Console.WriteLine($"SaveChangesAsync => {count} satır etkilendi");
        }
    }

}
