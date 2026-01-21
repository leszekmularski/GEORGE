
namespace GEORGE.Shared.ViewModels
{
    public class DaneKwadratu
    {
        public List<XPoint>? Wierzcholki { get; set; } //punkty wierzchołków pojedynczej linii
        public Guid RowIdElementu { get; set; } //id elementu w bazie danych
        public Guid RowIdSasiada { get; set; } //id elementu w bazie danych sąsiad równoległy
        public Guid RowIdSasiadaStronaA { get; set; } = Guid.Empty; //id elementu w bazie danych sąsiad strona A przecinający (góra/lewa)
        public Guid RowIdSasiadaStronaB { get; set; } = Guid.Empty; //id elementu w bazie danych sąsiad strona B przecinający (dół/prawa)
        public string? RowIdRegionuSasiada { get; set; } //id elementu w bazie danych
        public float KatLinii { get; set; } //kierunek linii w stopniach
        public string? Strona { get; set; } //kierunek linii w stopniach
        public double OffsetTop { get; set; } = 0; //odległość linii od górnej krawędzi elementu pobrana z bazy danych
        public double OffsetBottom { get; set; } = 0;
        public double OffsetLeft { get; set; } = 0;
        public double OffsetRight { get; set; } = 0;
        public bool BoolElementLinia { get; set; } = false; //czy element jest linią

        // Dodaj te właściwości jeśli są potrzebne:
        public string? TypKsztaltu { get; set; }
        public string? WypelnienieWewnetrzne { get; set; }
        public string? WypelnienieZewnetrzne { get; set; }
        public float GruboscObramowania { get; set; } = 2.0f;
        public int ZIndex { get; set; } = 0;
        public bool CzyLiniaPionowa()
        {
            return KatLinii == 90 || KatLinii == 270;
        }
        public bool CzyLiniaPozioma()
        {
            return KatLinii == 0 || KatLinii == 180;
        }
        public DaneKwadratu Clone()
        {
            return new DaneKwadratu
            {
                Wierzcholki = this.Wierzcholki != null
                    ? this.Wierzcholki.Select(p => new XPoint(p.X, p.Y)).ToList()
                    : null,

                RowIdElementu = this.RowIdElementu,
                RowIdSasiada = this.RowIdSasiada,
                RowIdSasiadaStronaA = this.RowIdSasiadaStronaA,
                RowIdSasiadaStronaB = this.RowIdSasiadaStronaB,
                RowIdRegionuSasiada = this.RowIdRegionuSasiada,

                KatLinii = this.KatLinii,
                Strona = this.Strona,

                OffsetTop = this.OffsetTop,
                OffsetBottom = this.OffsetBottom,
                OffsetLeft = this.OffsetLeft,
                OffsetRight = this.OffsetRight,

                BoolElementLinia = this.BoolElementLinia
            };
        }

    }

}
