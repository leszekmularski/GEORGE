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
        public double Szerokosc { get; set; } = 0;
        public double Wysokosc { get; set; } = 0;

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
        /// Sprawdza, czy właściwość jest "pozycją podziału" linii (pionowej lub poziomej).
        /// Zwraca false dla:
        /// - właściwości tylko do odczytu
        /// - linii skośnych (IsSkosna == true)
        /// - właściwości X2, Y2, Kąt (zwraca true tylko dla X1 i Y1)
        /// </summary>
        public bool GetLinia(int index)
        {
            if (EditableProperties == null || index < 0 || index >= EditableProperties.Count)
                return false;
            if (EditableProperties[index].IsReadOnly)
                return false;

            var prop = EditableProperties[index];

            // Wyklucz skośne
            if (prop.IsSkosna) return false;

            // Sprawdź, czy to linia
            if (string.IsNullOrEmpty(prop.NazwaObiektu) ||
                !prop.NazwaObiektu.ToLower().Contains("linia"))
                return false;

            var label = prop.Label?.ToLower() ?? "";

            // ⭐ Dla PIONOWEJ — zwróć true tylko dla X1
            if (prop.IsPionowa)
                return label.StartsWith("x1") || label.Contains("w osi x1");

            // ⭐ Dla POZIOMEJ — zwróć true tylko dla Y1
            if (prop.IsPozioma)
                return label.StartsWith("y1") || label.Contains("w osi y1");

            return false;
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

        public bool UpdateEditableProperty(int index, double value)
        {
            if (EditableProperties == null || index < 0 || index >= EditableProperties.Count)
                return false;

            bool jestZmiana = false;
            var prop = EditableProperties[index];

            // ⭐ Zapamiętaj starą wartość
            double staraWartosc = prop.Value;

            prop.Value = value;

            // ⭐ Propaguj ruch linii (X1/X2/Y1/Y2) na inne linie
            if (Math.Abs(staraWartosc - value) > 0.01
                && !prop.gabarytOkna
                && prop.ShapeId != null)
            {
                PropagujRuchLinii(prop, staraWartosc, value);
            }

            // ─── Reszta (szerokość, wysokość itd.) ───
            if (prop.Label.ToLower().StartsWith("szerokość") && prop.gabarytOkna)
            {
                jestZmiana = Szerokosc != value;
                Szerokosc = value;
            }
            else if (prop.Label.ToLower().StartsWith("wysokość") && prop.gabarytOkna)
            {
                jestZmiana = Wysokosc != value;
                Wysokosc = value;
            }
            else if (prop.Label.ToLower().StartsWith("promień okna") && prop.gabarytOkna)
            {
                Wysokosc = value * 2;
                Szerokosc = value * 2;
                jestZmiana = true;
            }
            else if (prop.Label.ToLower().StartsWith("wymiar okna kwadratowego") && prop.gabarytOkna)
            {
                Wysokosc = value;
                Szerokosc = value;
                jestZmiana = true;
            }

            return jestZmiana;
        }

        /// <summary>
        /// Po zmianie właściwości linii (X1, X2, Y1, Y2) – przesuwa końce innych linii,
        /// które miały koniec w starym punkcie.
        /// </summary>
        /// <summary>
        /// Po edycji X1/X2/Y1/Y2 linii – przesuwa końce innych linii, które leżały
        /// NA starej pozycji edytowanej linii (na jej osi i w zakresie).
        /// </summary>
        private void PropagujRuchLinii(
        EditableProperty zmienionaProp,
        double staraWartosc,
        double nowaWartosc,
        double tolerancja = 1.0)
        {
            var label = zmienionaProp.Label?.ToLower() ?? "";

            bool toX1 = label.StartsWith("x1") || label.Contains("w osi x1");
            bool toX2 = label.StartsWith("x2") || label.Contains("w osi x2");
            bool toY1 = label.StartsWith("y1") || label.Contains("w osi y1");
            bool toY2 = label.StartsWith("y2") || label.Contains("w osi y2");

            if (!toX1 && !toX2 && !toY1 && !toY2) return;

            bool liniaPozioma = (toY1 || toY2);
            bool liniaPionowa = (toX1 || toX2);

            double staraOs = staraWartosc;
            double nowaOs = nowaWartosc;
            double minZakres, maxZakres;

            if (liniaPozioma)
            {
                var x1Prop = EditableProperties.FirstOrDefault(p =>
                    p.ShapeId == zmienionaProp.ShapeId && (p.Label?.ToLower().StartsWith("x1") == true));
                var x2Prop = EditableProperties.FirstOrDefault(p =>
                    p.ShapeId == zmienionaProp.ShapeId && (p.Label?.ToLower().StartsWith("x2") == true));

                double lineX1 = x1Prop?.Value ?? 0;
                double lineX2 = x2Prop?.Value ?? 0;
                minZakres = Math.Min(lineX1, lineX2) - tolerancja;
                maxZakres = Math.Max(lineX1, lineX2) + tolerancja;
            }
            else
            {
                var y1Prop = EditableProperties.FirstOrDefault(p =>
                    p.ShapeId == zmienionaProp.ShapeId && (p.Label?.ToLower().StartsWith("y1") == true));
                var y2Prop = EditableProperties.FirstOrDefault(p =>
                    p.ShapeId == zmienionaProp.ShapeId && (p.Label?.ToLower().StartsWith("y2") == true));

                double lineY1 = y1Prop?.Value ?? 0;
                double lineY2 = y2Prop?.Value ?? 0;
                minZakres = Math.Min(lineY1, lineY2) - tolerancja;
                maxZakres = Math.Max(lineY1, lineY2) + tolerancja;
            }

            Console.WriteLine(
                $"🔗 PropagujRuchLinii: '{(liniaPozioma ? "POZIOMA" : "PIONOWA")}' " +
                $"os: {staraOs:F1} → {nowaOs:F1}, zakres: [{minZakres:F1}..{maxZakres:F1}]");

            int propagowane = 0;

            for (int i = 0; i < EditableProperties.Count; i++)
            {
                var other = EditableProperties[i];
                if (other.ShapeId == zmienionaProp.ShapeId) continue;
                if (other.gabarytOkna) continue;
                if (string.IsNullOrEmpty(other.NazwaObiektu) ||
                    !other.NazwaObiektu.ToLower().Contains("linia"))
                    continue;

                var otherLabel = other.Label?.ToLower() ?? "";
                bool otherIsX1 = otherLabel.StartsWith("x1") || otherLabel.Contains("w osi x1");
                bool otherIsX2 = otherLabel.StartsWith("x2") || otherLabel.Contains("w osi x2");
                bool otherIsY1 = otherLabel.StartsWith("y1") || otherLabel.Contains("w osi y1");
                bool otherIsY2 = otherLabel.StartsWith("y2") || otherLabel.Contains("w osi y2");

                if (!otherIsX1 && !otherIsX2 && !otherIsY1 && !otherIsY2) continue;

                // Znajdź parę (drugą współrzędną)
                string otherSzukany = otherIsX1 ? "y1" : otherIsX2 ? "y2" : otherIsY1 ? "x1" : "x2";

                var otherPara = EditableProperties.FirstOrDefault(p =>
                    p.ShapeId == other.ShapeId &&
                    (p.Label?.ToLower().StartsWith(otherSzukany) == true ||
                     p.Label?.ToLower().Contains($"w osi {otherSzukany}") == true));

                if (otherPara == null) continue;

                double ptX = (otherIsX1 || otherIsX2) ? other.Value : otherPara.Value;
                double ptY = (otherIsY1 || otherIsY2) ? other.Value : otherPara.Value;

                if (liniaPozioma)
                {
                    if (Math.Abs(ptY - staraOs) < tolerancja &&
                        ptX >= minZakres && ptX <= maxZakres)
                    {
                        if (otherIsY1 || otherIsY2)
                        {
                            // ⭐ KLUCZOWA POPRAWKA: SetValue zamiast Value
                            other.SetValue(nowaOs);
                            propagowane++;

                            Console.WriteLine(
                                $"   ✅ Y: '{other.Label}' ({other.ShapeId?.Substring(0, 8)}) " +
                                $"({ptX:F1},{ptY:F1}) → Y={nowaOs:F1}");
                        }
                    }
                }
                else
                {
                    if (Math.Abs(ptX - staraOs) < tolerancja &&
                        ptY >= minZakres && ptY <= maxZakres)
                    {
                        if (otherIsX1 || otherIsX2)
                        {
                            // ⭐ SetValue zamiast Value
                            other.SetValue(nowaOs);
                            propagowane++;

                            Console.WriteLine(
                                $"   ✅ X: '{other.Label}' ({other.ShapeId?.Substring(0, 8)}) " +
                                $"({ptX:F1},{ptY:F1}) → X={nowaOs:F1}");
                        }
                    }
                }
            }

            Console.WriteLine($"   🔗 propagowano {propagowane} właściwości");
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