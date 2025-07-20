using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wedding.Model.DB
{
    public class PhotoDb
    {
        public int id { get; set; }
        public string PublicId { get; set; }
        public string? Ip { get; set; }
        public string? Device { get; set; }
        public string Extension { get; set; } = "jpg"; // varsayılan
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public bool IsActive {  get; set; } = true;
    }
}
