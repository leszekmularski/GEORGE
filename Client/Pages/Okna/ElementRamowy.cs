namespace GEORGE.Client.Pages.Okna
{
    public class ElementRamowy
    {
        public string? Nazwa { get; set; } // np. "Lewy pion", "Góra", "Dół"
        public float Dlugosc { get; set; }
        public float Grubosc { get; set; }
        public string? Typ { get; set; } // np. "słupek", "rama", "skrzydło"
        public string? Kolor { get; set; } = "#FFFFFF";
    }

}
