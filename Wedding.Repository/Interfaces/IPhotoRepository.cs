using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wedding.Model.DB;

namespace Wedding.Repository.Interfaces
{
    public interface IPhotoRepository
    {
        Task<List<PhotoDb>> GetAllAsync();
        Task<PhotoDb> GetByIdAsync(int id);
        Task AddAsync(PhotoDb photo);
        void Add(PhotoDb photo);
        Task SaveAsync();
    }
}
