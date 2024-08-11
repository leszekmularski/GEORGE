using System.ComponentModel.DataAnnotations;

namespace GEORGE.Shared.Models
{
    public class Logowania
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string? Uzytkownik { get; set; }
        public string? RodzajPrzegladarki { get; set; }
        public DateTime Datalogowania { get; set; }

    }
}
