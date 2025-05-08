using GEORGE.Client.Pages.Models;

namespace GEORGE.Client.Pages.Okna
{
    public class KsztaltElementu
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

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

        // Opcjonalna grupa logiczna (np. "rama", "szyba", "skrzydlo")
        public string? Grupa { get; set; }
    }

}
