using System.ComponentModel.DataAnnotations;

namespace GEORGE.Shared.Models
{
    public class Pracownicy
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string? RowId { get; set; } = Guid.NewGuid().ToString();
        public string? RowIdDzialu { get; set; } = "";
        public string? Kodkreskowy { get; set; }
        public string? Imie { get; set; }
        public string? Nazwisko { get; set; }
        public string? Stanowisko { get; set; }
        public string? StanowiskoSystem { get; set; }
        public string? Dzial { get; set; }
        public string? UzytkownikSQL { get; set; }
        public string? HasloSQL { get; set; }
        public string? Telefon { get; set; }
        public string? Uwagi { get; set; }
        public string? Notatka { get; set; }
        public string? Email { get; set; }
        public string? HasloDoPoczty { get; set; } = "---";
        public DateTime Datautowrzenia { get; set; } = System.DateTime.Now;
        public string? Autorzmiany { get; set; }
        public bool Nieaktywny { get; set; }

    }
}
