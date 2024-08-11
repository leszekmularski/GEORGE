using System.ComponentModel.DataAnnotations;

namespace GEORGE.Shared.Models
{
    public class RodzajeDzialow
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? RowId { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string? NazwaDzialu { get; set; }
        public string? Uwagi { get; set; }
        public string? Notatka { get; set; }
        public DateTime Datautowrzenia { get; set; }
        public string? Autorzmiany { get; set; }

    }
}
