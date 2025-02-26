namespace GEORGE.Client.Pages.Wyroby
{
    public class WymiaryOpis
    {
        public double SzerokoscSzyby { get; set; }
        public double WysokoscSzyby { get; set; }
        public double SzerokoscSkrzydla { get; set; }
        public double WysokoscSkrzydla { get; set; }
        public double WymiarWewSzeSkrzydla { get; set; }
        public double WymiarWewWyskrzydla { get; set; }
        private string Id_Okna { get; set; }

        public WymiaryOpis(double szerokoscSzyby, double wysokoscSzyby, double szerokoscSkrzydla, double wysokoscSkrzydla,
            double wymiarWewSzeSkrzydla, double wymiarWewWyskrzydla,
            string id_Okna)
        {
            SzerokoscSzyby = szerokoscSzyby;
            WysokoscSzyby = wysokoscSzyby;
            SzerokoscSkrzydla = szerokoscSkrzydla;
            WysokoscSkrzydla = wysokoscSkrzydla;
            WymiarWewSzeSkrzydla = wymiarWewSzeSkrzydla;
            WymiarWewWyskrzydla = wymiarWewWyskrzydla;
            Id_Okna = id_Okna;
        }
    }
}
