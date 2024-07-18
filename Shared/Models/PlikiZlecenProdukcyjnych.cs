namespace GEORGE.Shared.Models
{
    public class PlikiZlecenProdukcyjnych
    {
        public long Id { get; set; }
        public string? RowId { get; set; } = Guid.NewGuid().ToString();
        public string? RowIdZleceniaProdukcyjne { get; set; }
        public string? NazwaPliku { get; set; }
        public string? TypPliku { get; set; }
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; }
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
    }
}
