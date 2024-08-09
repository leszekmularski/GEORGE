namespace GEORGE.Shared.Models
{
    public class ZleceniaCzasNaLinieProd
    {
        public long Id { get; set; }
        public string? RowId { get; set; } = Guid.NewGuid().ToString();
        public string? RowIdLinieProdukcyjne { get; set; } = "";
        public string? RowIdZleceniaProdukcyjne { get; set; } = "";
        public int CzasNaZlecenie { get; set; } = 0;
        public bool ZlecenieWewnetrzne { get; set; } = false;
        public string? Uwagi { get; set; } = "";
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; } = "Admin";
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
    }
}
