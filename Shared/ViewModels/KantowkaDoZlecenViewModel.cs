using GEORGE.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GEORGE.Shared.ViewModels
{
    public class KantowkaDoZlecenViewModel
    {
        public long Id { get; set; }
        public string? RowIdZlecenia { get; set; }
        public string? GatunekKantowki { get; set; }
        public string? Przekroj { get; set; }
        public string? NazwaProduktu { get; set; }
        public string? KodProduktu { get; set; }
        public string? Uwagi { get; set; }
        public int DlugoscZamawiana { get; set; } = 0;
        public int DlugoscNaGotowo { get; set; } = 0;
        public string DlugoscNaGotowoGrupa { get; set; } = "";
        public int IloscSztuk { get; set; } = 0;
        public DateTime DataZamowienia{ get; set; } = DateTime.MinValue;
        public DateTime DataRealizacji { get; set; } = DateTime.Now.AddDays(14);
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; }
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
        public bool MaterialZeStanMagazyn { get; set; } = false;
        public bool PozDostarczono { get; set; } = false;
        public DateTime DataDostarczenia { get; set; } = DateTime.MinValue;
        public bool WyslanoDoZamowien { get; set; } = false;
        public string? RowIdPliku { get; set; }
        public string? TylkoKlient { get; set; }
        public ZleceniaProdukcyjne? ZleceniaProdukcyjne { get; set; }
    }
}
