namespace GEORGE.Shared.Models
{
    public class KartyInstrukcyjne
    {
        public long Id { get; set; }
        public string? NumerKarty { get; set; }
        public string? OpisKarty { get; set; }
        public string? NazwaProduktu { get; set; }
        public string? NazwaProduktu2 { get; set; }
        public string? KodProduktu { get; set; }
        public string? LinkDoKartyNaSerwerze { get; set; }
        public string? Uwagi { get; set; }
        public int IloscStron { get; set; }
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; }
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
    }
}
