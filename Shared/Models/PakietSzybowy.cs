using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GEORGE.Shared.Models
{
    public enum TypWarstwy { Szyba, Ramka }

    public enum RodzajSzyby
    {
        Float, Hartowane, Laminowane, Niskoemisyjne,
        Refleksyjne, Barwione, Dźwiękochłonne, Antysloneczne, Inne
    }

    public enum RodzajRamki { Aluminiowa, Stalowa, WarmEdge, TGI, Inna }

    public enum GazWRamce { Powietrze, Argon, Krypton, Ksenon }

    [Table("PakietySzybowe")]
    public class PakietSzybowy
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(200)]
        public string Nazwa { get; set; } = "";

        [MaxLength(1000)]
        public string Opis { get; set; } = "";

        public bool Aktywny { get; set; } = true;

        // Parametry cieplno-optyczne
        public double Ug { get; set; }
        public double Lt { get; set; }
        public double g { get; set; }
        public double Rw { get; set; }
        public double CenaZaM2 { get; set; }

        public DateTime DataUtworzenia { get; set; } = DateTime.UtcNow;
        public DateTime DataModyfikacji { get; set; } = DateTime.UtcNow;
        public Guid? SystemOkienId { get; set; } = Guid.Empty;

        public List<WarstwaSzyby> Warstwy { get; set; } = new();

        [NotMapped]
        public double GruboscCalkowita => Warstwy?.Sum(w => w.Grubosc) ?? 0;

        [NotMapped]
        public int LiczbaSzyb => Warstwy?.Count(w => w.Typ == TypWarstwy.Szyba) ?? 0;

        [NotMapped]
        public string Etykieta =>
            Warstwy == null || !Warstwy.Any()
                ? "(pusty)"
                : string.Join("/", Warstwy.OrderBy(w => w.Kolejnosc)
                                          .Select(w => w.Grubosc.ToString("0.#")));

        public PakietSzybowy Clone()
        {
            var clone = (PakietSzybowy)MemberwiseClone();
            clone.Warstwy = Warstwy?.Select(w => w.Clone()).ToList() ?? new();
            return clone;
        }

        public PakietSzybowy CloneWithNewId()
        {
            var c = Clone();
            c.Id = Guid.NewGuid();
            c.Nazwa = $"{Nazwa} (kopia)";
            foreach (var w in c.Warstwy) { w.Id = Guid.NewGuid(); w.PakietSzybowyId = c.Id; }
            return c;
        }
    }

    [Table("WarstwySzyby")]
    public class WarstwaSzyby
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PakietSzybowyId { get; set; }

        public int Kolejnosc { get; set; }

        public TypWarstwy Typ { get; set; } = TypWarstwy.Szyba;
        public double Grubosc { get; set; } = 4.0;

        public RodzajSzyby RodzajSzyby { get; set; } = RodzajSzyby.Float;
        [MaxLength(100)] public string? Powloka { get; set; }
        [MaxLength(50)] public string? Kolor { get; set; }

        public RodzajRamki RodzajRamki { get; set; } = RodzajRamki.Aluminiowa;
        public GazWRamce Gaz { get; set; } = GazWRamce.Argon;

        [MaxLength(100)] public string? Producent { get; set; }
        [MaxLength(500)] public string? Uwagi { get; set; }

        public WarstwaSzyby Clone() => (WarstwaSzyby)MemberwiseClone();
    }
}