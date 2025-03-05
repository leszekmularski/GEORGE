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
        public string? Uwagi { get; set; }

        // Obrazek DXF w bazie
        public byte[] Rysunek { get; set; } = Array.Empty<byte>();

        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; } = "NaN";
    }

}
