using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GEORGE.Shared.Models
{
    public class KonfModele
    {
        public int Id { get; set; }
        public Guid RowId { get; set; } = Guid.NewGuid();
        public Guid RowIdSystem { get; set; }
        public Guid RowIdElement { get; set; }
        public string? NazwaKonfiguracji { get; set; }
        public string? Typ { get; set; } // Typ- Rama, skrzydło, słupek ruchomy, słupek stały
        public int? KatWystapieniaZakresOdMin { get; set; } = 0;
        public int? KatWystapieniaZakresOdMax { get; set; } = 360;
        public int? PromienWystapieniaZakresOdMin { get; set; } = 0;
        public int? PromienWystapieniaZakresOdMax { get; set; } = 6500;
        public int? KonstrMinSzer { get; set; } = 0;
        public int? KonstrMaxSzer { get; set; } = 6500;
        public int? KonstrMinWys { get; set; } = 0;
        public int? KonstrMaxWys { get; set; } = 6500;
        public string? DodatkowyFiltrWystepowania { get; set; }
        public string? PolaczenieNaroza { get; set; } //Sposob łaczenia narozy czy kat 45;45;45;45 lub 90;90;90;90 lub 0;0;0;0 --> 180-T3;90-T3;0-T3;270-T3
        public bool SposobLaczeniaCzop { get; set; } = false; // W przypadku wybrania tej opcji elemnty nachodzą na siebie
        public int? NaddatekNaZgrzewNaStrone { get; set; } = 0; //Naddatek na zgrzew na stronę
        public int? ZwiekszNaddatekGdyKatInny90 { get; set; } = 0; //Dodaj do długości gdy kąt inny niż 90 stop
        public string? Uwagi { get; set; }
        public bool WidocznaNaLiscie { get; set; } = true; // Czy widoczna na liscie do wyboru w modelach

        // Obrazek DXF w bazie
        public byte[] Rysunek { get; set; } = Array.Empty<byte>();

        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; } = "NaN";
    }

}
