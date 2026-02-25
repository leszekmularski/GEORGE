namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class WygenerowaneMaterialy
    {
        public Guid RowIdIndeksu { get; set; }
        public double Dlugosc { get; set; }
        public float KatWModelu { get; set; }
        public float KatStroanA { get; set; }
        public float KatStroanB { get; set; }
        public float DlugoscCzopaA { get; set; }
        public float DlugoscCzopaB { get; set; }
        public string IdentyfikatorStrony { get; set; }
        public string Nazwa { get; set; }
        public string IndeksElementu { get; set; }
        public float IloscSztuk { get; set; }
        public string Restrykcja { get; set; }
        public string RodzajpolaczenAiB { get; set; }

        public WygenerowaneMaterialy(Guid rowIdIndeksu, double dlugosc, string nazwa, string indeksElementu, float iloscSztuk, string restrykcja, 
            float katWModelu, float katStroanA, float katStroanB, float dlugoscCzopaA, float dlugoscCzopaB, string identyfikatorStrony, string rodzajpolaczenAiB)
        {
            RowIdIndeksu = rowIdIndeksu;
            Dlugosc = dlugosc;
            Nazwa = nazwa;
            IndeksElementu = indeksElementu;
            IloscSztuk = iloscSztuk;
            Restrykcja = restrykcja;
            KatWModelu = katWModelu;
            KatStroanA = katStroanA;
            KatStroanB = katStroanB;
            DlugoscCzopaA = dlugoscCzopaA;
            DlugoscCzopaB = dlugoscCzopaB;
            IdentyfikatorStrony = identyfikatorStrony;
            RodzajpolaczenAiB = rodzajpolaczenAiB;
        }
    }
}
