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
        public DbSet<ZleceniaProdukcyjneWew>? ZleceniaProdukcyjneWew { get; set; }
        public DbSet<KartyInstrukcyjne> KartyInstrukcyjne { get; set; }
        public DbSet<RodzajeKartInstrukcyjnych> RodzajeKartInstrukcyjnych { get; set; }
        public DbSet<PlikiZlecenProdukcyjnych> PlikiZlecenProdukcyjnych { get; set; }   
        public DbSet<KantowkaDoZlecen>? KantowkaDoZlecen { get; set; }
        public DbSet<SzybyDoZlecen>? SzybyDoZlecen { get; set; }
        public DbSet<LinieProdukcyjne>? LinieProdukcyjne { get; set; }
        public DbSet<ZleceniaNaLinii>? ZleceniaNaLinii { get; set; }
        public DbSet<ZleceniaCzasNaLinieProd>? ZleceniaCzasNaLinieProd { get; set; }
        public DbSet<Pracownicy>? Pracownicy { get; set; }
        public DbSet<Logowania> Logowania => Set<Logowania>();
        public DbSet<RodzajeDzialow> RodzajeDzialow => Set<RodzajeDzialow>();
        public DbSet<Uprawnieniapracownika> Uprawnieniapracownika => Set<Uprawnieniapracownika>();
        public DbSet<PozDoZlecen> PozDoZlecen => Set<PozDoZlecen>();
        public DbSet<ElemetZamDoZlecen> ElemetZamDoZlecen => Set<ElemetZamDoZlecen>();
        public DbSet<ProducenciPodwykonawcy> ProducenciPodwykonawcy => Set<ProducenciPodwykonawcy>();
        public DbSet<ZleceniaProdukcyjneZmianyStatusu> ZleceniaProdukcyjneZmianyStatusu => Set<ZleceniaProdukcyjneZmianyStatusu>();
        public DbSet<KonfSystem> KonfSystem => Set<KonfSystem>();
        public DbSet<SystemyOkienne> SystemyOkienne => Set<SystemyOkienne>();
        public DbSet<KonfModele> KonfModele => Set<KonfModele>();
        public DbSet<KonfModeleElementy> KonfModeleElementy => Set<KonfModeleElementy>();
        public DbSet<KonfPolaczenie> KonfPolaczenie => Set<KonfPolaczenie>();
        public DbSet<WzorceKompletacji> WzorceKompltacji => Set<WzorceKompletacji>(); //Wersja shared: 1.0.1.5 Wersja server:1.1.2.0

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

        public async Task<bool> ZmienDateRozpoczeciaProdukcji(string rowid, string rowidlinia, DateTime nowaDataProdukcji)
        {
            // Używamy FirstOrDefaultAsync do wyszukiwania po kolumnie RowId
            var zlec = await ZleceniaProdukcyjne
                .FirstOrDefaultAsync(z => z.RowId == rowid);

            if (zlec == null)
            {
                return false;
            }

           var zlecNaLinii = await ZleceniaNaLinii
          .FirstOrDefaultAsync(z => z.RowIdZleceniaProdukcyjne == rowid && z.RowIdLinieProdukcyjne == rowidlinia);

            if (zlecNaLinii != null)
            {

                // Aktualizowanie danych
                if (zlec.DataRozpProdukcji == DateTime.MinValue || (nowaDataProdukcji == DateTime.MinValue && zlecNaLinii.DataRozpProdukcjiNaLinii == zlec.DataRozpProdukcji))
                {
                    zlec.DataRozpProdukcji = nowaDataProdukcji;
                    zlec.OstatniaZmiana = zlec.OstatniaZmiana + "ZP/RZOP.P.:[" + DateTime.Now.ToLongDateString() + "]";
                    zlec.ProcentWykonania = nowaDataProdukcji != DateTime.MinValue ? 10 : 0;
                }

                zlecNaLinii.DataRozpProdukcjiNaLinii = nowaDataProdukcji;
                zlecNaLinii.OstatniaZmiana = "Ostatnia zmiana [Data rozpocz. produkcji]: " + DateTime.Now.ToLongDateString();
            }

            // Zapisanie zmian
            await SaveChangesAsync();

            return true;
        }

        public async Task<bool> ZmienDateRozpoczeciaProdukcjiWew(string rowid, string rowidlinia, DateTime nowaDataProdukcji)
        {
            // Używamy FirstOrDefaultAsync do wyszukiwania po kolumnie RowId
            var zlec = await ZleceniaProdukcyjneWew
                .FirstOrDefaultAsync(z => z.RowId != rowid);

            if (zlec == null)
            {
                return false;
            }

            // Aktualizowanie danych

            var zlecNaLinii = await ZleceniaNaLinii
            .FirstOrDefaultAsync(z => z.RowIdZleceniaProdukcyjne == rowid && z.RowIdLinieProdukcyjne == rowidlinia);

            if (zlecNaLinii != null)
            {
                
                // Aktualizowanie danych
                if (zlec.DataRozpProdukcji == DateTime.MinValue || (nowaDataProdukcji == DateTime.MinValue && zlecNaLinii.DataRozpProdukcjiNaLinii == zlec.DataRozpProdukcji))
                {
                    zlec.DataRozpProdukcji = nowaDataProdukcji;
                    zlec.OstatniaZmiana = zlec.OstatniaZmiana + "ZP/RZOP.P.:[" + DateTime.Now.ToLongDateString() + "]";
                    zlec.ProcentWykonania = nowaDataProdukcji != DateTime.MinValue ? 10 : 0;
                }

                zlecNaLinii.DataRozpProdukcjiNaLinii = nowaDataProdukcji;
            }

            // Zapisanie zmian
            await SaveChangesAsync();
            return true;
        }
        public async Task<bool> ZmienNazwePliku(long id, string nazwaPliku)
        {
            var plik = await PlikiZlecenProdukcyjnych.FindAsync(id);
            if (plik == null)
            {
                return false;
            }

            plik.OryginalnaNazwaPliku = nazwaPliku;
            plik.DataZapisu = DateTime.Now;
            plik.IloscPobranPliku = 0; // Zeruje ilość pobrań po zmianie nazwy pliku
            plik.OstatniaZmiana = "Zmiana. Uwagi: " + DateTime.Now.ToLongDateString();
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
            plik.OstatniaZmiana = "Zmiana. Uwagi: " + DateTime.Now.ToLongDateString();
            await SaveChangesAsync();
            return true;
        }
        public async Task<bool> ZmienWidocznosc(long id, bool widocznosc)
        {
            var plik = await PlikiZlecenProdukcyjnych.FindAsync(id);
            if (plik == null)
            {
                return false;
            }

            plik.WidocznyDlaWszystkich = widocznosc;
            plik.OstatniaZmiana = "Zmiana. Widoczności pliku: " + DateTime.Now.ToLongDateString();
            await SaveChangesAsync();
            return true;
        }
        public async Task<bool> ZwiekszLicznikPobranPliku(long id)
        {
            var plik = await PlikiZlecenProdukcyjnych.FindAsync(id);
            if (plik == null)
            {
                return false;
            }

            plik.IloscPobranPliku = plik.IloscPobranPliku + 1;
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