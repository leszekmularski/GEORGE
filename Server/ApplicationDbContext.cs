using Microsoft.EntityFrameworkCore;
using GEORGE.Shared;
using System.Collections.Generic;
using GEORGE.Shared.Models;

namespace GEORGE.Server
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ZleceniaProdukcyjne>? ZleceniaProdukcyjne { get; set; }
        public DbSet<KartyInstrukcyjne> KartyInstrukcyjne { get; set; }
        public DbSet<RodzajeKartInstrukcyjnych> RodzajeKartInstrukcyjnych { get; set; }
        public DbSet<PlikiZlecenProdukcyjnych> PlikiZlecenProdukcyjnych { get; set; }
        
    }
}
