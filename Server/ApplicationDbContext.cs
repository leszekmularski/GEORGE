﻿using Microsoft.EntityFrameworkCore;
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
        public DbSet<ZleceniaProdukcyjneWew>? ZleceniaProdukcyjneWew { get; set; }
        public DbSet<KartyInstrukcyjne> KartyInstrukcyjne { get; set; }
        public DbSet<RodzajeKartInstrukcyjnych> RodzajeKartInstrukcyjnych { get; set; }
        public DbSet<PlikiZlecenProdukcyjnych> PlikiZlecenProdukcyjnych { get; set; }   
        public DbSet<KantowkaDoZlecen>? KantowkaDoZlecen { get; set; }
        public DbSet<LinieProdukcyjne>? LinieProdukcyjne { get; set; }
        public DbSet<ZleceniaNaLinii>? ZleceniaNaLinii { get; set; }
        public DbSet<ZleceniaCzasNaLinieProd>? ZleceniaCzasNaLinieProd { get; set; }
        

        //***********************************************************************************************************************************************************************************************

        public async Task<bool> ZmienDateProdukcji(string rowid, DateTime nowaDataProdukcji)
        {
            // Używamy FirstOrDefaultAsync do wyszukiwania po kolumnie RowId
            var zlec = await ZleceniaProdukcyjne
                .FirstOrDefaultAsync(z => z.RowId == rowid);

            if (zlec == null)
            {
                return false;
            }

            // Aktualizowanie danych
            zlec.DataProdukcji = nowaDataProdukcji;
            zlec.OstatniaZmiana = zlec.OstatniaZmiana + "ZP:[" + DateTime.Now.ToLongDateString() + "]";

            // Zapisanie zmian
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> ZmienDateProdukcjiWew(string rowid, DateTime nowaDataProdukcji)
        {
            // Używamy FirstOrDefaultAsync do wyszukiwania po kolumnie RowId
            var zlec = await ZleceniaProdukcyjneWew
                .FirstOrDefaultAsync(z => z.RowId == rowid);

            if (zlec == null)
            {
                return false;
            }

            // Aktualizowanie danych
            zlec.DataProdukcji = nowaDataProdukcji;
            zlec.OstatniaZmiana = zlec.OstatniaZmiana + "ZP:[" + DateTime.Now.ToLongDateString() + "]";

            // Zapisanie zmian
            await SaveChangesAsync();
            return true;
        }
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