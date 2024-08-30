using System.ComponentModel.DataAnnotations;

namespace GEORGE.Shared.Models
{
    public class PozDoZlecen
    {
        public long Id { get; set; }

        [Required]
        public string? RowId { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string? RowIdZlecenia { get; set; } = "";
        [Required]
        public string? RowIdLiniiProdukcyjnej { get; set; } = "";
        public float Nr { get; set; } = 1;
        public int IloscOkien { get; set; } = 1;
        public float JednostkiOkienDoPoz { get; set; } = 0;
        public float JednostkiOkienSumaDoPoz { get; set; } = 0;
        public float JednostkiOkienDoPozZrobione { get; set; } = 0;
        public float Szerokosc { get; set; } = 0;
        public float Wysokosc { get; set; } = 0;
        public string? System { get; set; } = "";
        public string? Technologia { get; set; } = "";
        public string? Kolor { get; set; } = "";
        public float Ciezar1Sztuki { get; set; } = 0;
        public int Iloscskrzydel { get; set; } = 0;
        public string? Szyba { get; set; } = "";
        public string? Uwagi { get; set; } = "";
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; } = "NaN";
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
    }
}
