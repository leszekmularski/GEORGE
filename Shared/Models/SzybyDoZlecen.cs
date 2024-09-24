using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GEORGE.Shared.Models
{
    public class SzybyDoZlecen
    {
        public long Id { get; set; }

        [Required]
        public string? RowIdZlecenia { get; set; }
        public string? NazwaProduktu { get; set; } = string.Empty;
        public float? Szerokosc { get; set; }
        public float? Wysokosc { get; set; }   
        public string? RodzajSzyby { get; set; } = string.Empty;
        public string? RodzajRamki { get; set; } = string.Empty;
        public int IloscSztuk { get; set; } = 0;
        public string? Uwagi { get; set; } = string.Empty;
        public DateTime DataZamowienia{ get; set; } = DateTime.MinValue;
        public DateTime DataRealizacji { get; set; } = DateTime.Now.AddDays(14);
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; } = "NaN";
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
        public bool CzyKsztalt { get; set; } = false;
        public bool PozDostarczono { get; set; } = false;
        public bool CzyZamowiono { get; set; } = false;
        public DateTime DataDostarczenia { get; set; } = DateTime.MinValue;
    }
}
