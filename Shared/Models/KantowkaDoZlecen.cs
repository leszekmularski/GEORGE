using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GEORGE.Shared.Models
{
    public class KantowkaDoZlecen
    {
        public long Id { get; set; }

        [Required]
        public string? RowIdZlecenia { get; set; }
        public string? GatunekKantowki { get; set; }
        public string? Przekroj { get; set; }
        public string? NazwaProduktu { get; set; }
        public string? KodProduktu { get; set; }
        public string? Uwagi { get; set; }
        public int DlugoscZamawiana { get; set; } = 0;
        public int DlugoscNaGotowo { get; set; } = 0;
        public int IloscSztuk { get; set; } = 0;
        public DateTime DataZamowienia{ get; set; } = DateTime.Now;
        public DateTime DataRealizacji { get; set; } = DateTime.Now.AddDays(14);
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; }
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
        public bool MaterialZeStanMagazyn { get; set; } = false;
    }
}
