using GEORGE.Shared.Models;

namespace GEORGE.Client.Pages.Okna
{
    public abstract class GenerujOkno
    {
        public string IdOkna { get; set; } = Guid.NewGuid().ToString();
        // 📐 Wymiary okna
        public float Szerokosc { get; set; }
        public float Wysokosc { get; set; }
        // ⚙️ Grubości poszczególnych ram
        public float GruboscLewo { get; set; }
        public float GruboscPrawo { get; set; }
        public float GruboscGora { get; set; }
        public float GruboscDol { get; set; }
        // 🎨 Kolory
        public string KolorZewnetrzny { get; set; } = "#FFFFFF";
        public string KolorWewnetrzny { get; set; } = "#FFFFFF";
        // ⚖️ Waga okna (kg)
        public float Waga { get; set; }
        // 🧩 Typ kształtu ramy: prostokąt, koło, trójkąt, trapez, romb
        public string? TypKsztaltu { get; set; } = "prostokąt";
        // 📏 Informacja o profilach
        public MVCKonfModele? PowiazanyModel;
        // 📏 Informacja o profilach
        public List<KsztaltElementu> ElementyRamyRysowane { get; set; } = new();
        // 🪟 Informacje o szybie
        public float GruboscSzyby { get; set; }
        public string KolorSzyby { get; set; } = "#ADD8E6";
        // 🧪 Możliwość przechowywania danych dodatkowych
        public Dictionary<string, string> WlasciwosciDodatkowe { get; set; } = new();
        public Guid RowIdSystemu { get; set; }
        public Guid RowIdModelu { get; set; }
        public bool RuchomySlupekPoPrawej { get; set; } = false;
        public bool RuchomySlupekPoLewej { get; set; } = false;
    }
}
