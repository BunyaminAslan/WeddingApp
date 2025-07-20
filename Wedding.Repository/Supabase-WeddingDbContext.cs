using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wedding.Model.DB;

namespace Wedding.Repository
{
    public class Supabase_WeddingDbContext : DbContext
    {
        public Supabase_WeddingDbContext(DbContextOptions<Supabase_WeddingDbContext> options) : base(options) { }

        public DbSet<PhotoDb> Photos { get; set; }
    }
}
