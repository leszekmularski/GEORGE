namespace GEORGE.Shared.Models
{
    public class KonfModeleElementy
    {
        public int Id { get; set; }
        public Guid RowId { get; set; } = Guid.NewGuid();
        public Guid RowIdSystem { get; set; }
        public Guid RowIdElement { get; set; }
        public Guid RowIdKonfModele { get; set; }
        public string? NazwaKonfiguracji { get; set; }
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; } = "NaN";
    }

}
