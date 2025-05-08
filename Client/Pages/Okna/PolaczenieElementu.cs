namespace GEORGE.Client.Pages.Okna
{
    public class PolaczenieElementu
    {
        public int NaroznikId { get; set; } // 0 = lewy-górny, 1 = prawy-górny, 2 = prawy-dolny, 3 = lewy-dolny
        public string? TypPolaczenia { get; set; } // np. "T1", "T2", "T3"
        public string? Opis { get; set; } // np. "Połączenie górne z lewym pionem"
    }

}
