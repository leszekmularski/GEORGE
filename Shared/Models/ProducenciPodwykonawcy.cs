using System.ComponentModel.DataAnnotations.Schema;

namespace GEORGE.Shared.Models
{
    public class ProducenciPodwykonawcy
    {
        public long Id { get; set; }
        public string? RowId { get; set; } = Guid.NewGuid().ToString();
        public string? NazwaProducenta { get; set; }
        public string? Adres { get; set; }
        public string? Miejscowosc { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? NIP { get; set; }
        public string? REGON { get; set; }
        public string? AdresWWW1 { get; set; }
        public string? AdresWWW2 { get; set; }
        public string? Uwagi { get; set; }
        public string? OsobaKontaktowa1 { get; set; }
        public string? EmailOsobyKontaktowej1 { get; set; }
        public string? TelefonOsobyKontaktowej1 { get; set; }
        public string? OsobaKontaktowa2 { get; set; }
        public string? EmailOsobyKontaktowej2 { get; set; }
        public string? TelefonOsobyKontaktowej2 { get; set; }
        public string? OsobaKontaktowa3 { get; set; }
        public string? EmailOsobyKontaktowej3 { get; set; }
        public string? TelefonOsobyKontaktowej3 { get; set; }
        public string? OsobaKontaktowa4 { get; set; }
        public string? EmailOsobyKontaktowej4 { get; set; }
        public string? TelefonOsobyKontaktowej4 { get; set; }
        public string? OsobaKontaktowa5 { get; set; }
        public string? EmailOsobyKontaktowej5 { get; set; }
        public string? TelefonOsobyKontaktowej5 { get; set; }
        public int IloscDniRealizacji { get; set; } = 0;
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; }
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
        public string[]? Tags { get; set; }
        public bool KlienetWymagaProforma { get; set; } = false;
        public string? RodzajProdukcjiWykonywanej { get; set; }

        [NotMapped]
        public string TagsString
        {
            get => Tags == null ? string.Empty : string.Join(", ", Tags);
            set => Tags = string.IsNullOrWhiteSpace(value) ? null : value.Split(',').Select(tag => tag.Trim()).ToArray();
        }
    }
}
