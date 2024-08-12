using System.ComponentModel.DataAnnotations.Schema;

namespace GEORGE.Shared.Models
{
    public class ZleceniaProdukcyjneWew
    {
        public long Id { get; set; }
        public string? RowId { get; set; } = Guid.NewGuid().ToString();
        public string? TypZamowienia { get; set; }
        public string? NumerZamowienia { get; set; }
        public string? NumerUmowy { get; set; }
        public DateTime DataProdukcji { get; set; } = DateTime.Now.AddDays(56);
        public DateTime DataWysylki { get; set; } = DateTime.Now.AddDays(88);
        public DateTime DataMontazu { get; set; } = DateTime.Now.AddDays(85);
        public string? Klient { get; set; }
        public string? Adres { get; set; }
        public string? Miejscowosc { get; set; } = "Marcinkowice";
        public string? Telefon { get; set; }
        public string? NazwaProduktu { get; set; }
        public string? NazwaProduktu2 { get; set; }
        public string? KodProduktu { get; set; }
        public int Ilosc { get; set; }
        public Single Wartosc { get; set; }
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public DateTime DataGotowosci { get; set; } = DateTime.MinValue;
        public string? KtoZapisal { get; set; } //= User.Identity.Name;
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
        public string[]? Tags { get; set; }
        public int JednostkiNaZlecenie { get; set; } = 0;
        public bool ZlecZrealizowane { get; set; } = false;

        [NotMapped]
        public string TagsString
        {
            get => Tags == null ? string.Empty : string.Join(", ", Tags);
            set => Tags = string.IsNullOrWhiteSpace(value) ? null : value.Split(',').Select(tag => tag.Trim()).ToArray();
        }
    }
}
