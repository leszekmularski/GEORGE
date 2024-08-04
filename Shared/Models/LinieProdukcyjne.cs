namespace GEORGE.Shared.Models
{
    public class LinieProdukcyjne
    {
        public long Id { get; set; }
        public string? NumerKarty { get; set; }
        public string? RowId { get; set; } = Guid.NewGuid().ToString(); 
        public string? NazwaLiniiProdukcyjnej { get; set; }
        public int DziennaZdolnoscProdukcyjna { get; set; } = 0;
        public string? Uwagi { get; set; }
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; } = "Admin";
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
    }
}
