using System.ComponentModel.DataAnnotations;

namespace GEORGE.Shared.Models
{
    public class ElemetZamDoZlecen
    {
        public long Id { get; set; }

        [Required]
        public string? RowIdZlecenia { get; set; }
        public string? RowIdProducent { get; set; }
        public decimal? Szerokosc { get; set; } = 0;
        public decimal? Wysokosc { get; set; } = 0;
        public decimal? Dlugosc { get; set; } = 0;
        public decimal? Waga { get; set; } = 0;
        public decimal? Powierzchnia { get; set; } = 0;
        public decimal? Objetosc { get; set; } = 0;
        public decimal? CenaNetto { get; set; } = 0;
        public string? Jednostka { get; set; } = "";
        public string? Typ { get; set; } = "";
        public string? NumerKatalogowy { get; set; } = "";
        public string? NazwaProduktu { get; set; }
        public string? Opis { get; set; } = "";
        public string? Kolor { get; set; } = "";
        public double IloscSztuk { get; set; } = 0;
        public string? Uwagi { get; set; } = "";
        public DateTime DataZamowienia{ get; set; } = DateTime.MinValue;
        public DateTime DataRealizacji { get; set; } = DateTime.MinValue;
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; } = "NaN";
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
        public bool CzyZamowiono { get; set; } = false;
        public bool PozDostarczono { get; set; } = false;
        public DateTime DataDostarczenia { get; set; } = DateTime.MinValue;
        public string? RowIdPliku { get; set; }
        public string? RowIdPlikuDodatkowy { get; set; }
    }
}
