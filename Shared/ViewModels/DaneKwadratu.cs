
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
        public double OffsetTop { get; set; } = 0; //odległość linii od górnej krawędzi elementu pobrana z bazy danych
        public double OffsetBottom { get; set; } = 0;
        public double OffsetLeft { get; set; } = 0;
        public double OffsetRight { get; set; } = 0;
        public bool BoolElementLinia { get; set; } = false; //czy element jest linią
        public bool CzyLiniaPionowa()
        {
            return KatLinii == 90 || KatLinii == 270;
        }
        public bool CzyLiniaPozioma()
        {
            return KatLinii == 0 || KatLinii == 180;
        }
    }

}
