using System.ComponentModel.DataAnnotations;

namespace GEORGE.Shared.Models
{
    public class Uprawnieniapracownika
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string? RowId { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string? RowIdPracownicy { get; set; }
        [Required]
        public string? RowIdRejestrejestrow { get; set; }
        [Required]
        public string? TableName { get; set; }
        public bool Odczyt { get; set; }
        public bool Zapis { get; set; }
        public bool Zmiana { get; set; }
        public bool Usuniecie { get; set; }
        public bool Administrator { get; set; }
        public string? Uwagi { get; set; }
        public DateTime Datautowrzenia { get; set; }
        public string? Autorzmiany { get; set; }

    }
}
