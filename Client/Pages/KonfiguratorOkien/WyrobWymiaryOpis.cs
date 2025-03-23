namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class WyrobWymiaryOpis
    {
        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }
        public string RodzajObiektu { get; set; }
        public string Restrykcja { get; set; }

        public WyrobWymiaryOpis(double szerokosc, double wysokosc, string rodzajObiektu, string restrykcja)
        {
            Szerokosc = szerokosc;
            Wysokosc = wysokosc;
            RodzajObiektu = rodzajObiektu;
            Restrykcja = restrykcja;
        }
    }
}
