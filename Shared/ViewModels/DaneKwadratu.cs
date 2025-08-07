
namespace GEORGE.Shared.ViewModels
{
    public class DaneKwadratu
    {
        public List<XPoint>? Wierzcholki { get; set; } //punkty wierzchołków pojedynczej linii
        public Guid RowIdElementu { get; set; } //id elementu w bazie danych
        public Guid RowIdSasiada { get; set; } //id elementu w bazie danych
        public string? RowIdRegionuSasiada { get; set; } //id elementu w bazie danych
        public int KatLinii { get; set; } //kierunek linii w stopniach
        public string? Strona { get; set; } //kierunek linii w stopniach
    }

}
