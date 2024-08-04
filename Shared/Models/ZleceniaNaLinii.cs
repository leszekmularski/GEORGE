using System.ComponentModel.DataAnnotations;

namespace GEORGE.Shared.Models
{
    public class ZleceniaNaLinii
    {
        public long Id { get; set; }
        public string? RowId { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string? RowIdLinieProdukcyjne { get; set; }
        [Required]
        public int RowIdZleceniaProdukcyjne { get; set; }
        public string? Uwagi { get; set; }
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
    }
}
