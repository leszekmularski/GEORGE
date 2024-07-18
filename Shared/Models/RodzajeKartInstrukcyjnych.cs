namespace GEORGE.Shared.Models
{
    public class RodzajeKartInstrukcyjnych
    {
        public long Id { get; set; }
        public string? NumerRodzajuKart { get; set; }
        public string? OpisRodzajuKart { get; set; }
        public string? NazwaProduktu { get; set; }
        public string? NazwaProduktu2 { get; set; }
        public string? KodProduktu { get; set; }
        public string? Uwagi { get; set; }
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; }
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
    }
}
