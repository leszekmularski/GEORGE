using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GEORGE.Shared.Models
{
    public class WzorceKompletacji
    {
        public long Id { get; set; }

        [Required]
        public Guid? RowId { get; set; } = Guid.NewGuid();
        public string? RowIdProducent { get; set; }

        [Required]
        public string? NazwaWzorca { get; set; }
        [Required]
        public Guid? RowIdWzorca { get; set; }
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
        public double Ilosc { get; set; } = 0;
        public double CzasRealizacjiZamowienia { get; set; } = 7;//domyślnie 7 dni
        public string? Uwagi { get; set; } = "";
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; } = "NaN";
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
        public string? RowIdPliku_1 { get; set; }
        public string? RowIdPliku_2 { get; set; }

        [NotMapped]
        public NestedData[] NestedData { get; set; }


    }
    public class NestedData
    {
        public long Id { get; set; }
        public Guid? RowId { get; set; } = Guid.NewGuid();
        public string? RowIdProducent { get; set; }
        public string? NazwaWzorca { get; set; }
        public Guid? RowIdWzorca { get; set; }
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
        public double Ilosc { get; set; } = 0;
        public double CzasRealizacjiZamowienia { get; set; } = 7;//domyślnie 7 dni
        public string? Uwagi { get; set; } = "";
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; } = "NaN";
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
        public string? RowIdPliku_1 { get; set; }
        public string? RowIdPliku_2 { get; set; }
    }
}
