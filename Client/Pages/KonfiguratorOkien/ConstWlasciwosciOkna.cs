using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class ConstWlasciwosciOkna
    {
        public int IdZmiany { get; set; }
        public List<XPoint>? Wierzcholki { get; set; }
        public List<XPoint>? WierzcholkiWartosciNominalne { get; set; }
        public List<XPoint>? WierzcholkiInner { get; set; }
        public List<ContourSegment> Kontur { get; set; }
        public List<ContourSegment> KonturInner { get; set; }
        public MVCKonfModele? MVCKonfModelu { get; set; }
        public KonfModele? WybranyModel { get; set; }
        public List<DaneKwadratu>? ListaKwadratow { get; set; }
        public double Szerokosc { get; set; } = 1250;
        public double Wysokosc { get; set; } = 1000;

        // POPRAWIONE: Dodaj inicjalizację
        public List<EditableProperty> EditableProperties { get; set; } = new();

        public string KolorZewnetrzny { get; set; }
        public string KolorWewnetrzny { get; set; }
        public string KolorSzyby { get; set; }
        public string TypSzyby { get; set; }
        public float GruboscPakietu { get; set; }
        public float WspolczynnikPrzepuszczalnosciCieplaSzyby { get; set; }
        public float WspolczynnikPrzepuszczalnosciCieplaOkna { get; set; }
        public string TypKlamki { get; set; }
        public string TypUszczelek { get; set; }
        public string TypOslonPrzeciwsłonecznych { get; set; }
        public string TypMontazu { get; set; }

        public const string DomyslnyKolorZewnetrzny = "Bialy";
        public const string DomyslnyKolorWewnetrzny = "Bialy";
        public const string DomyslnyKolorSzyby = "Przezroczysta";
        public const string DomyslnyTypSzyby = "Standardowa";
        public const string DomyslnyTypKlamki = "Standardowa";
        public const string DomyslnyTypUszczelek = "Standardowe";
        public const string DomyslnyTypOslonPrzeciwsłonecznych = "Brak";
        public const string DomyslnyTypMontazu = "Standard";
        public string Komunikaty { get; set; }

        public ConstWlasciwosciOkna()
        {
            IdZmiany = 0;
            Wierzcholki = new List<XPoint>();
            WierzcholkiWartosciNominalne = new List<XPoint>();
            WierzcholkiInner = new List<XPoint>();
            Kontur = new List<ContourSegment>();
            KonturInner = new List<ContourSegment>();
            MVCKonfModelu = new MVCKonfModele();
            WybranyModel = new KonfModele();
            ListaKwadratow = new List<DaneKwadratu>();
            Szerokosc = 1250;
            Wysokosc = 1000;
            EditableProperties = new List<EditableProperty>();
            KolorZewnetrzny = DomyslnyKolorZewnetrzny;
            KolorWewnetrzny = DomyslnyKolorWewnetrzny;
            KolorSzyby = DomyslnyKolorSzyby;
            TypSzyby = DomyslnyTypSzyby;
            GruboscPakietu = 0;
            WspolczynnikPrzepuszczalnosciCieplaSzyby = 0;
            WspolczynnikPrzepuszczalnosciCieplaOkna = 0;
            TypKlamki = DomyslnyTypKlamki;
            TypUszczelek = DomyslnyTypUszczelek;
            TypOslonPrzeciwsłonecznych = DomyslnyTypOslonPrzeciwsłonecznych;
            TypMontazu = DomyslnyTypMontazu;
            Komunikaty = string.Empty;
        }

        /// <summary>
        /// Pobiera wartość właściwości po indeksie
        /// </summary>
        public double GetPropertyValue(int index)
        {
            if (EditableProperties == null || index < 0 || index >= EditableProperties.Count)
                return 0;

            return EditableProperties[index].Value;
        }

        /// <summary>
        /// Sprawdza czy właściwość jest tylko do odczytu
        /// </summary>
        public bool IsPropertyReadOnly(int index)
        {
            if (EditableProperties == null || index < 0 || index >= EditableProperties.Count)
                return true;

            return EditableProperties[index].IsReadOnly;
        }

        /// <summary>
        /// Pobiera etykietę właściwości
        /// </summary>
        public string GetPropertyLabel(int index)
        {
            if (EditableProperties == null || index < 0 || index >= EditableProperties.Count)
                return "";

            return EditableProperties[index].Label;
        }

        /// <summary>
        /// Aktualizuje wartość edytowalnej właściwości po indeksie
        /// Dla record: zastępuje obiekt w liście nowym
        /// </summary>
        public void UpdateEditableProperty(int index, double value)
        {
            if (EditableProperties == null || index < 0 || index >= EditableProperties.Count)
                return;


            var prop = EditableProperties[index];
            {
                prop.Value = value; // Wywołuje setter -> SetValue(value)

                Console.WriteLine($"UpdateEditableProperty --> index: {index} prop.Label: {prop.Label} value: {value}");

                if (prop.Label.ToLower().StartsWith("szerokość") && prop.gabarytOkna)
                {
                    Szerokosc = value;
                }
                else if (prop.Label.ToLower().StartsWith("wysokość") && prop.gabarytOkna)
                {
                    Wysokosc = value;
                }
                else if (prop.Label.ToLower().StartsWith("promień okna") && prop.gabarytOkna)
                {
                    Wysokosc = value * 2;
                    Szerokosc = value * 2;
                }
                else if (prop.Label.ToLower().StartsWith("wymiar okna kwadratowego") && prop.gabarytOkna)
                {
                    Wysokosc = value;
                    Szerokosc = value;
                }

            }

        }

        /// <summary>
        /// Znajduje i aktualizuje właściwość po nazwie (Label)
        /// </summary>
        public void UpdateEditableProperty(string label, double value)
        {
            var index = EditableProperties?.FindIndex(p => p.Label == label) ?? -1;
            if (index >= 0)
            {
                UpdateEditableProperty(index, value);
            }
        }
    }
}