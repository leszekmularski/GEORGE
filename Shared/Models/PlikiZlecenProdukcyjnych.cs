using System.ComponentModel.DataAnnotations.Schema;

namespace GEORGE.Shared.Models
{
    public class PlikiZlecenProdukcyjnych
    {
        public long Id { get; set; }
        public string? RowId { get; set; } = Guid.NewGuid().ToString();
        public string? RowIdZleceniaProdukcyjne { get; set; }
        public string? NazwaPliku { get; set; }
        public string? OryginalnaNazwaPliku { get; set; }
        public string? TypPliku { get; set; }
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; }
        public string? Uwagi { get; set; } = " ";
        public bool WidocznyDlaWszystkich { get; set; } = false;
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
        public long IloscPobranPliku { get; set; } = 0;

        [NotMapped]
        public Guid RowGuid
        {
            get
            {
                if (Guid.TryParse(RowId, out var guid))
                    return guid;

                return Guid.Empty;
            }
            set
            {
                RowId = value.ToString();
            }
        }
    }
}
