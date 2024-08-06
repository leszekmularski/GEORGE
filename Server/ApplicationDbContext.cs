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
        public DbSet<ZleceniaProdukcyjneWew>? ZleceniaProdukcyjneWew { get; set; }
        public DbSet<KantowkaDoZlecen>? KantowkaDoZlecen { get; set; }
        public DbSet<LinieProdukcyjne>? LinieProdukcyjne { get; set; }
        public DbSet<ZleceniaNaLinii>? ZleceniaNaLinii { get; set; }

        public async Task<bool> ZmienUwage(long id, string uwaga)
        {
            var plik = await PlikiZlecenProdukcyjnych.FindAsync(id);
            if (plik == null)
            {
                return false;
            }

            plik.Uwagi = uwaga;
            plik.OstatniaZmiana = "Zmiana: " + DateTime.Now.ToLongDateString();
            await SaveChangesAsync();
            return true;


        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RodzajeKartInstrukcyjnych>()
                .HasIndex(r => r.NumerRodzajuKart)
                .IsUnique();
        }
    }
}