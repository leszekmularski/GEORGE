namespace GEORGE.Shared.Models
{
    public class Schody
    {
        public long Id { get; set; }
        public Guid RowId { get; set; } = Guid.NewGuid();
        public string? Typ { get; set; }
        public string? Uwagi { get; set; }
        public double Wysokosc { get; set; }
        public double Szerokosc { get; set; }
        public double Glebokosc { get; set; }
        public double GlebokoscZabieg1 { get; set; }
        public double GlebokoscZabieg2 { get; set; }
        public double SzerokoscZabieg1 { get; set; }
        public double SzerokoscZabieg2 { get; set; }
        public byte[] RysunekPogladowy { get; set; } = Array.Empty<byte>();
        public Guid RowIdPliku { get; set; } = Guid.Empty;
        public bool Wycofany_z_produkcji { get; set; } = false;
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; } = "NaN";

    }

}
