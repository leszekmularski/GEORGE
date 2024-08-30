
namespace GEORGE.Shared.ViewModels
{
    public class ZleceniaProdukcyjneDto
    {
        public long Id { get; set; }
        public string? RowId { get; set; }
        public string? TypZamowienia { get; set; }
        public string? NumerZamowienia { get; set; }
        public string? NumerUmowy { get; set; }
        public DateTime DataProdukcji { get; set; }
        public DateTime DataWysylki { get; set; }
        public DateTime DataMontazu { get; set; }
        public string? Klient { get; set; }
        public string? Adres { get; set; }
        public string? Miejscowosc { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? NazwaProduktu { get; set; }
        public string? NazwaProduktu2 { get; set; }
        public string? KodProduktu { get; set; }
        public int Ilosc { get; set; }
        public Single Wartosc { get; set; }
        public DateTime DataZapisu { get; set; }
        public string? KtoZapisal { get; set; }
        public string? OstatniaZmiana { get; set; }
        public string[]? Tags { get; set; }
        public float JednostkiNaZlecenie { get; set; }
        public bool ZlecZrealizowane { get; set; }
        public bool ZlecenieWewnetrzne { get; set; } = false;

    }
}
