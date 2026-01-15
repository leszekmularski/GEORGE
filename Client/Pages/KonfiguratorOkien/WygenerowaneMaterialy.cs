namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class WygenerowaneMaterialy
    {
        public Guid RowIdIndeksu { get; set; }
        public double Dlugosc { get; set; }
        public string Nazwa { get; set; }
        public string IndeksElementu { get; set; }
        public float IloscSztuk { get; set; }
        public string Restrykcja { get; set; }

        public WygenerowaneMaterialy(Guid rowIdIndeksu, double dlugosc, string nazwa, string indeksElementu, float iloscSztuk, string restrykcja)
        {
            RowIdIndeksu = rowIdIndeksu;
            Dlugosc = dlugosc;
            Nazwa = nazwa;
            IndeksElementu = indeksElementu;
            IloscSztuk = iloscSztuk;
            Restrykcja = restrykcja;
        }
    }
}
