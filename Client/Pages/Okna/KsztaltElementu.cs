using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;


namespace GEORGE.Client.Pages.Okna
{
    public class KsztaltElementu
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string IdRegion { get; set; } = "";

        // Typ ksztaltu: prostokat, trojkat, romb, trapez, kolo itd.
        public string? TypKsztaltu { get; set; }

        // Lista wierzcholkow (w kolejnosci zgodnej z ruchem wskazowek zegara)
        public List<XPoint> Wierzcholki { get; set; } = new();

        // Styl wypelnienia wewnetrznego (np. kolor lub nazwa tekstury)
        public string WypelnienieWewnetrzne { get; set; } = "#FFFFFF";

        // Styl wypelnienia zewnetrznego (np. obrys, tekstura drewna)
        public string WypelnienieZewnetrzne { get; set; } = "#000000";

        // Szerokosc linii obramowania
        public float GruboscObramowania { get; set; } = 2.0f;

        // Czy obiekt zawiera otwor (do fill("evenodd"))
        public bool CzyZawieraOtwor { get; set; } = false;

        // Czy element ma byc widoczny
        public bool Widoczny { get; set; } = true;

        // Opcjonalnie: warstwa rysowania
        public int ZIndex { get; set; } = 0;

        // RoIdElemntu zapisanego w bazie danych
        public Guid RowIdElementu { get; set; }
        public string? IndeksElementu { get; set; } = "BRAK-DANYCH";
        public string? NazwaElementu { get; set; } = "BRAK-DANYCH";
        public float Kat { get; set; } = 0;
        public string? Strona { get; set; }

        // Opcjonalna grupa logiczna (np. "Opis modelu podany przez użytkownika")
        public string? Grupa { get; set; }
        // Opcjonalna grupa logiczna (np. "rama", "szyba", "skrzydlo")
        public string? Typ { get; set; }
        //Informacje o długości elementy
        public float DlogoscElementu { get; set; } = 0.0f;
        // Informacje długość cięcia
        public float DlogoscWidocznaElementu { get; set; } = 0.0f;
        // Informacje o długości widocznej w modelu elementu
        public float KatStronaA { get; set; } = 0.0f;
        // Informacje o kąt cięcia strona A elementu
        public float KatStronaB { get; set; } = 0.0f;
        // Informacje o kąt cięcia strona B elementu

        public float OffsetLewa { get; set; } = 0.0f;
        // Informacje o offset lewa elementu
        public float OffsetPrawa { get; set; } = 0.0f;
        // Informacje o offset prawa elementu
        public float OffsetGora { get; set; } = 0.0f;
        // Informacje o offset Gora elementu
        public float OffsetDol { get; set; } = 0.0f;
        // Informacje o offset Dol elementu

        public BoundingBox? OstatniRegion { get; set; }

        public KsztaltElementu Clone()
        {
            return new KsztaltElementu
            {
                Id = this.Id, 
                IdRegion = this.IdRegion,
                TypKsztaltu = this.TypKsztaltu,
                Wierzcholki = this.Wierzcholki.Select(p => new XPoint(p.X, p.Y)).ToList(),
                WypelnienieWewnetrzne = this.WypelnienieWewnetrzne,
                WypelnienieZewnetrzne = this.WypelnienieZewnetrzne,
                GruboscObramowania = this.GruboscObramowania,
                CzyZawieraOtwor = this.CzyZawieraOtwor,
                Widoczny = this.Widoczny,
                ZIndex = this.ZIndex,
                RowIdElementu = this.RowIdElementu,
                IndeksElementu = this.IndeksElementu,
                NazwaElementu = this.NazwaElementu,
                Kat = this.Kat,
                Strona = this.Strona,
                Grupa = this.Grupa,
                Typ = this.Typ,
                DlogoscElementu = this.DlogoscElementu,
                DlogoscWidocznaElementu = this.DlogoscWidocznaElementu,
                KatStronaA = this.KatStronaA,
                KatStronaB = this.KatStronaB,
                OffsetLewa = this.OffsetLewa,
                OffsetPrawa = this.OffsetPrawa,
                OffsetGora = this.OffsetGora,
                OffsetDol = this.OffsetDol
            };
        }

    }

}
