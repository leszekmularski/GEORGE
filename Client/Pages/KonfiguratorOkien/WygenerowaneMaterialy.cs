namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class WygenerowaneMaterialy
    {
        public Guid RowIdIndeksu { get; set; }
        public double Dlugosc { get; set; }
        public string Nazwa { get; set; }
        public string Indeks { get; set; }
        public float IloscSztuk { get; set; }
        public string Restrykcja { get; set; }

        public WygenerowaneMaterialy(Guid rowIdIndeksu, double dlugosc, string nazwa, string indeks, float iloscSztuk, string restrykcja)
        {
            RowIdIndeksu = rowIdIndeksu;
            Dlugosc = dlugosc;
            Nazwa = nazwa;
            Indeks = indeks;
            IloscSztuk = iloscSztuk;
            Restrykcja = restrykcja;
        }
    }
}
