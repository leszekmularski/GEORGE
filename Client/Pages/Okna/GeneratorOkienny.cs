using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Client.Pages.Models;
using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;
using Microsoft.JSInterop;
using System.Data;

namespace GEORGE.Client.Pages.Okna
{
    public class Generator : GenerujOkno
    {
        public new List<KsztaltElementu> ElementyRamyRysowane { get; set; } = new();
        public List<KonfSystem> KonfiguracjeSystemu { get; set; } = new();

        public KonfModele? EdytowanyModel;
        public int Zindeks { get; set; }
        public string IdRegionuPonizej { get; set; }

        // Lista wierzcholkow tylko linie (w kolejnosci zgodnej z ruchem wskazowek zegara)
        public List<XPoint> Wierzcholki { get; set; } = new();
        public List<ContourSegment> konturWenetrznyPodRysunek { get; set; } = new();

        // Lista wierzcholkow linie i łuki (w kolejnosci zgodnej z ruchem wskazowek zegara)

        public List<XPoint> wewnetrznyKontur; // przechowuje obliczony wewnętrzny kontur po offsetowaniu

        public List<XPoint> liniaSzkleniaKontur;// przechowuje obliczony kontur linii szklenia (jeśli dotyczy)
        public List<XPoint> wierzcholkiWenetrznePodRysunek { get; set; } = new();
        public List<ContourSegment> zewnetrznyKonturZLukami { get; set; } = new();

        public List<ContourSegment> wewnetrznyKonturZLukami; // przechowuje obliczony wewnętrzny kontur po offsetowaniu

        public List<ContourSegment> liniaSzkleniaKonturZLukami;// przechowuje obliczony kontur linii szklenia (jeśli dotyczy)

        public List<XPoint> liniaOkuciaKontur;// przechowuje obliczony kontur linii szklenia (jeśli dotyczy)

        public List<ContourSegment> liniaOkuciaKonturZLukami;// przechowuje obliczony kontur linii szklenia (jeśli dotyczy)
        public ConstWlasciwosciOkna constWlasciwosciOkna { get; set; } = new(); // przechowuje stałe punkty do wyświetlania właściwości okna (np. w panelu bocznym)
        public List<ShapeRegion> Region { get; set; } = new();
        public string StronaElementu { get; set; } = "";

        private readonly IJSRuntime _jsRuntime;
        public List<string> Komunikaty { get; set; } = new();
        public List<string> BledySystemowe { get; set; } = new();

        public Generator(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            // Inicjalizacja domyślnych wartości
            Szerokosc = 1250;
            Wysokosc = 1000;
            KolorZewnetrzny = "#FFFFFF";
            KolorWewnetrzny = "#FFFFFF";
            Waga = 0;
            TypKsztaltu = "prostokąt";
            GruboscSzyby = 24;
            KolorSzyby = "#ADD8E6";
            KonfiguracjeSystemu = new List<KonfSystem>();
            EdytowanyModel = null;
            Zindeks = -1;
            IdRegionuPonizej = string.Empty;
            //RowIdSystemu = Guid.NewGuid();
            //RowIdModelu = Guid.NewGuid();
            MVCKonfModelu = null; // analizowany model
            RuchomySlupekPoPrawej = false;
            RuchomySlupekPoLewej = false;
            ElementLiniowy = false;
            wewnetrznyKontur = new List<XPoint>();
            liniaSzkleniaKontur = new List<XPoint>();
            Komunikaty = new List<string>();
        }
        public async Task<bool> AddElements(List<ShapeRegion> regions, string regionId, Dictionary<string, GeneratorState> generatorStates, List<ShapeRegion> regionAdd,
            List<DaneKwadratu> daneKwadratu, List<XPoint> punktyRegionuMaster, XPoint mouseClik, bool kasujKonsole = false)
        {
            if (regions == null) return false;

            if (_jsRuntime != null && kasujKonsole)
            {
                await _jsRuntime.InvokeVoidAsync("console.clear");
                await _jsRuntime.InvokeVoidAsync("console.log", "\n\n");
            }

            if (KonfiguracjeSystemu == null || MVCKonfModelu == null)
            {
                //Console.WriteLine($"❌ AddElements Brak KonfiguracjeSystemu lub PowiazanyModel!");
                BledySystemowe.Add("❌ Brak konfiguracji systemu lub powiązanego modelu. Upewnij się, że dane są poprawnie załadowane.");
                return false;
            }

            if (EdytowanyModel == null)
            {
                //Console.WriteLine($"❌ AddElements Brak EdytowanyModel jest nie ustawiony!!!");
                BledySystemowe.Add("❌ Brak edytowanego modelu. Upewnij się, że model jest poprawnie załadowany.");
                return false;
            }

            if (regions == null)
            {
                //Console.WriteLine($"❌ AddElements Brak EdytowanyModel jest nie ustawiony!!!");
                BledySystemowe.Add("❌ Brak wybranego regionu. Sprawdź dane!!!");
                return false;
            }

            //Console.WriteLine($"➡️ AddElements EdytowanyModel.PolaczenieNaroza: {EdytowanyModel.PolaczenieNaroza} daneKwadratu.Count: {(daneKwadratu == null ? "NULL" : daneKwadratu.Count())}");

            //if (punktyRegionuMaster != null)
            //{
            //    Console.WriteLine($"➡️ AddElements punktyRegionuMaster.Count: {punktyRegionuMaster.Count()}");
            //}

            // Console.WriteLine($"📏 AddElements Szerokosc: {Szerokosc}, Wysokosc: {Wysokosc}");

            Region = regionAdd;

            var region = regions.FirstOrDefault(r => r.Id == regionId);


            //if (region != null && daneKwadratu != null && !region.Rama)
            //{
            //   region.Wierzcholki = SortPointsToCCW(region.Wierzcholki);
            //}

            List<XPoint> punkty = new List<XPoint>();
            List<ContourSegment> punktyZLukami = new List<ContourSegment>();

            if (region == null && !ElementLiniowy)
            {
                //Console.WriteLine($"❌ AddElements Nie znaleziono regionu o ID: {regionId} w AddElements - GeneratoryOkienne");
                BledySystemowe.Add($"❌ Nie znaleziono regionu o ID: {regionId}. Upewnij się, że dane regionów są poprawnie załadowane i zawierają wymagany region.");
                return false;
            }
            else if (region != null && !ElementLiniowy)
            {
                punkty = region.Wierzcholki;
                punktyZLukami = region.Kontur;
            }
            else if (ElementLiniowy)
            {
                region = regions.FirstOrDefault(r => r.Id != null);

                Console.WriteLine($"❌ AddElements Region o ID: {regionId} region.Wierzcholki.Count():{region.Wierzcholki.Count()}");

                punkty = region.Wierzcholki;
                punktyZLukami = region.Kontur;
            }

            Wierzcholki = punkty;
            zewnetrznyKonturZLukami = punktyZLukami;

            //foreach (var x in punkty)
            //{
            //    Console.WriteLine($"punkty --> x.X: {x.X} / x.Y: {x.Y}");
            //}

            //foreach (var c in punktyZLukami)
            //{
            //    Console.WriteLine($"punktyFull --> c.Start.X: {c.Start.X} / c.Start.Y: {c.Start.Y} / c.End.X: {c.End.X} / c.End.Y: {c.End.Y} / c.Type: {c.Type}");
            //}

            if ((punkty == null || punkty.Count < 3) && !ElementLiniowy)
            {
                // Console.WriteLine($"❌ AddElements Region o ID: {regionId} ma zbyt mało punktów");
                BledySystemowe.Add($"❌ Region o ID: {regionId} ma zbyt mało punktów. Wymagane jest co najmniej 3 punkty dla regionów nielinowych. Sprawdź dane wejściowe dla tego regionu.");
                return false;
            }

            if ((punkty == null || punkty.Count < 2))
            {
                //Console.WriteLine($"❌ AddElements Region o ID: {regionId} ma zbyt mało punktów! punkty.Count: {punkty.Count}");
                BledySystemowe.Add($"❌ Region o ID: {regionId} ma zbyt mało punktów. Wymagane jest co najmniej 2 punkty dla elementów liniowych. Sprawdź dane wejściowe dla tego regionu.");
                return false;
            }

            //Console.WriteLine($"🟩 AddElements Generuj okno dla regionu ID {regionId} typu: {region.TypKsztaltu} ElementLiniowy: {ElementLiniowy} punkty.Count: {punkty.Count()}");

            // 🧮 Bounding box
            float minX = (float)punkty.Min(p => p.X);
            float maxX = (float)punkty.Max(p => p.X);
            float minY = (float)punkty.Min(p => p.Y);
            float maxY = (float)punkty.Max(p => p.Y);

            float width = maxX - minX;
            float height = maxY - minY;

            // 🔄 Skalowanie do regionu
            // var przeskalowanePunkty = SkalujIPrzesun(punkty, minX, minY, width, height, Szerokosc, Wysokosc);
            var przeskalowanePunkty = new List<XPoint>(punkty); // bez skalowania – prawdziwe dane
                                                                // Zakładam, że punktyFull to List<ContourSegment>
            var przeskalowanePunktyZLukami = new List<ContourSegment>();

            var przeskalowanePunktyZLukamiPodRysynek = new List<ContourSegment>(); // bez skalowania – prawdziwe dane

            var przeskalowanePunktyPodRysynek = new List<XPoint>(punkty); // bez skalowania – prawdziwe dane

            // 1️⃣ Usuń segmenty zerowej długości
            var bezDuplikatow = punktyZLukami
                .Where(s => !PointsAreClose(s.Start, s.End))
                .ToList();

            przeskalowanePunktyZLukami = bezDuplikatow;// BuildClosedContour(bezDuplikatow);

            //Console.WriteLine($"🔹 Segmenty po usunięciu duplikatów: {bezDuplikatow.Count} z {punktyZLukami.Count}");

            // funkcja porównująca punkty
            bool PointsAreClose(XPoint a, XPoint b, double tolerance = 0.001)
            {
                return Math.Abs(a.X - b.X) < tolerance &&
                       Math.Abs(a.Y - b.Y) < tolerance;
            }

            //   Console.WriteLine($"🔹 przeskalowanePunktyZLukami: {przeskalowanePunktyZLukami.Count} w tym linii: {przeskalowanePunktyZLukami.Where(x => x.Type == SegmentType.Line).Count()} i łuki: {przeskalowanePunktyZLukami.Where(x => x.Type == SegmentType.Arc).Count()}");

            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------

            // znajdź indeks punktu o najmniejszym X i Y
            int startIndex = 0;
            double minValue = double.MaxValue;

            for (int i = 0; i < przeskalowanePunkty.Count; i++)
            {
                double value = przeskalowanePunkty[i].X + przeskalowanePunkty[i].Y;

                if (value < minValue)
                {
                    minValue = value;
                    startIndex = i;
                }
            }

            //// rotacja listy
            //var posortowane = przeskalowanePunkty
            //    .Skip(startIndex)
            //    .Concat(przeskalowanePunkty.Take(startIndex))
            //    .ToList();

            //przeskalowanePunkty = posortowane;


            // Console.WriteLine($"📐 Przeskalowane punkty: {string.Join(", ", przeskalowanePunkty.Select(p => $"({p.X:F2}, {p.Y:F2})"))} --------> minX:{minX}");

            string slruchPoPrawej = "";
            string slruchPoLewej = "";
            if (RuchomySlupekPoPrawej) slruchPoPrawej = "Słupek ruchomy";
            if (RuchomySlupekPoLewej) slruchPoLewej = "Słupek ruchomy";

            if (ElementLiniowy)
            {
                slruchPoPrawej = "";//brak słupka dla elementu liniowego
                slruchPoLewej = "";

                Wierzcholki = region.LinieDzielace?
                .SelectMany(l => l.Points)
                .ToList() ?? new List<XPoint>();

                zewnetrznyKonturZLukami = region.LinieDzielace?
                    .SelectMany(l => l.ContourSegments)
                    .ToList() ?? new List<ContourSegment>();

            }

            //foreach (var konf in MVCKonfModelu.KonfSystem)
            //{
            //    Console.WriteLine($"🔧 KonfiguracjeSystemu: {konf.Typ} Nazwa: {konf.Nazwa} W sumie: {MVCKonfModelu.KonfSystem.Count()}");
            //}

            //Console.WriteLine($"slruchPoPrawej = {slruchPoPrawej} slruchPoLewej = {slruchPoLewej}");


            //var konfLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa &&
            //            (string.IsNullOrEmpty(slruchPoLewej) || e.Typ == slruchPoLewej) || (string.IsNullOrEmpty(slruchPoPrawej) || e.Typ == slruchPoPrawej));


            //var konfRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa &&
            //            (string.IsNullOrEmpty(slruchPoPrawej) || e.Typ == slruchPoPrawej) || (string.IsNullOrEmpty(slruchPoLewej) || e.Typ == slruchPoLewej));

            var konfLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa &&
            (string.IsNullOrEmpty(slruchPoLewej) || e.Typ == slruchPoLewej));


            var konfRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa &&
                        (string.IsNullOrEmpty(slruchPoPrawej) || e.Typ == slruchPoPrawej));

            var konfTop = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeGora);

            var konfBottom = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeDol);

            if (konfLeft == null)
            {
                konfLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa);
                if (!RuchomySlupekPoLewej)
                    BledySystemowe.Add($"⚠️ Uwaga: Nie znaleziono konfiguracji dla lewej strony z typem '{slruchPoLewej}'. Użyto pierwszej dostępnej konfiguracji dla lewej strony: {konfLeft?.Nazwa ?? "BRAK-DANYCH"}. Sprawdź konfigurację systemu.");
            }

            if (konfRight == null)
            {
                konfRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa);
                if (!RuchomySlupekPoPrawej)
                    BledySystemowe.Add($"⚠️ Uwaga: Nie znaleziono konfiguracji dla prawej strony z typem '{slruchPoPrawej}'. Użyto pierwszej dostępnej konfiguracji dla prawej strony: {konfRight?.Nazwa ?? "BRAK-DANYCH"}. Sprawdź konfigurację systemu.");
            }

            // 🔧 Profile z konfiguracji
            //float profileLeft = (float)((konfLeft?.PionPrawa ?? 0) - (konfLeft?.PionLewa ?? 0));
            //float profileRight = (float)((konfRight?.PionPrawa ?? 0) - (konfRight?.PionLewa ?? 0));
            //float profileTop = (float)((konfTop?.PionPrawa ?? 0) - (konfTop?.PionLewa ?? 0));
            //float profileBottom = (float)((konfBottom?.PionPrawa ?? 0) - (konfBottom?.PionLewa ?? 0));

            float profileLeft = await ObliczRoznicePoziomow(konfLeft, ElementLiniowy);
            float profileRight = await ObliczRoznicePoziomow(konfRight, ElementLiniowy);
            float profileTop = await ObliczRoznicePoziomow(konfTop, ElementLiniowy);
            float profileBottom = await ObliczRoznicePoziomow(konfBottom, ElementLiniowy);

            float offsetGlassLeft = await ObliczRoznicePoziomowSzyba(konfLeft, ElementLiniowy);
            float offsetGlassRight = await ObliczRoznicePoziomowSzyba(konfRight, ElementLiniowy);
            float offsetGlassTop = await ObliczRoznicePoziomowSzyba(konfTop, ElementLiniowy);
            float offsetGlassBottom = await ObliczRoznicePoziomowSzyba(konfBottom, ElementLiniowy);

            float offsetKorpusWewnetrznyLeft = await ObliczRoznicePoziomowKorpusWewnetrzny(konfLeft);
            float offsetKorpusWewnetrznyRight = await ObliczRoznicePoziomowKorpusWewnetrzny(konfRight);
            float offsetKorpusWewnetrznyTop = await ObliczRoznicePoziomowKorpusWewnetrzny(konfTop);
            float offsetKorpusWewnetrznyBottom = await ObliczRoznicePoziomowKorpusWewnetrzny(konfBottom);

            if (offsetGlassLeft > 0) offsetGlassLeft = profileLeft - offsetGlassLeft;
            if (offsetGlassRight > 0) offsetGlassRight = profileRight - offsetGlassRight;
            if (offsetGlassTop > 0) offsetGlassTop = profileTop - offsetGlassTop;
            if (offsetGlassBottom > 0) offsetGlassBottom = profileBottom - offsetGlassBottom;

            if (offsetKorpusWewnetrznyLeft > 0 && !regions.FirstOrDefault().Rama) offsetKorpusWewnetrznyLeft = profileLeft - offsetKorpusWewnetrznyLeft;
            if (offsetKorpusWewnetrznyRight > 0 && !regions.FirstOrDefault().Rama) offsetKorpusWewnetrznyRight = profileRight - offsetKorpusWewnetrznyRight;
            if (offsetKorpusWewnetrznyTop > 0 && !regions.FirstOrDefault().Rama) offsetKorpusWewnetrznyTop = profileTop - offsetKorpusWewnetrznyTop;
            if (offsetKorpusWewnetrznyBottom > 0 && !regions.FirstOrDefault().Rama) offsetKorpusWewnetrznyBottom = profileBottom - offsetKorpusWewnetrznyBottom;

            if (profileLeft == 0 || profileRight == 0 || profileTop == 0 || profileBottom == 0)
            {
                BledySystemowe.Add($"⚠️ Uwaga: Jeden lub więcej profili jest równy 0. profileLeft: {profileLeft} profileRight: {profileRight} profileTop: {profileTop} profileBottom: {profileBottom}. Sprawdź konfigurację systemu.");
            }

            if (offsetKorpusWewnetrznyLeft == 0 || offsetKorpusWewnetrznyRight == 0 || offsetKorpusWewnetrznyTop == 0 || offsetKorpusWewnetrznyBottom == 0)
            {
                BledySystemowe.Add($"⚠️ Uwaga: Jeden lub więcej offsetów korpusu wewnętrznego jest równy 0. offsetKorpusWewnetrznyLeft: {offsetKorpusWewnetrznyLeft} offsetKorpusWewnetrznyRight: {offsetKorpusWewnetrznyRight} offsetKorpusWewnetrznyTop: {offsetKorpusWewnetrznyTop} offsetKorpusWewnetrznyBottom: {offsetKorpusWewnetrznyBottom}. Sprawdź konfigurację systemu.");
            }

            // Console.WriteLine($"🔧 Profile z konfiguracji przed korektą: profileLeft: {profileLeft} profileRight: {profileRight} profileTop: {profileTop} profileBottom: {profileBottom}");
            if (offsetGlassLeft == 0 || offsetGlassRight == 0 || offsetGlassTop == 0 || offsetGlassBottom == 0)
            {
                BledySystemowe.Add($"⚠️ Uwaga: Jeden lub więcej offsetów szklenia jest równy 0. offsetGlassLeft: {offsetGlassLeft} offsetGlassRight: {offsetGlassRight} offsetGlassTop: {offsetGlassTop} offsetGlassBottom: {offsetGlassBottom}. Sprawdź konfigurację systemu.");
            }
            // Console.WriteLine($"🔧 Profile z konfiguracji przed korektą: offsetGlassLeft: {offsetGlassLeft} offsetGlassRight: {offsetGlassRight} offsetGlassTop: {offsetGlassTop} offsetGlassBottom: {offsetGlassBottom}");

            Guid RowIdprofileLeft = konfLeft?.RowId ?? Guid.Empty;
            Guid RowIdprofileRight = konfRight?.RowId ?? Guid.Empty;
            Guid RowIdprofileTop = konfTop?.RowId ?? Guid.Empty;
            Guid RowIdprofileBottom = konfBottom?.RowId ?? Guid.Empty;

            string RowIndeksprofileLeft = konfLeft?.IndeksElementu ?? "BRAK-DANYCH";
            string RowIndeksprofileRight = konfRight?.IndeksElementu ?? "BRAK-DANYCH";
            string RowIndeksprofileTop = konfTop?.IndeksElementu ?? "BRAK-DANYCH";
            string RowIndeksprofileBottom = konfBottom?.IndeksElementu ?? "BRAK-DANYCH";

            string RowNazwaprofileLeft = konfLeft?.Nazwa ?? "BRAK-DANYCH";
            string RowNazwaprofileRight = konfRight?.Nazwa ?? "BRAK-DANYCH";
            string RowNazwaprofileTop = konfTop?.Nazwa ?? "BRAK-DANYCH";
            string RowNazwaprofileBottom = konfBottom?.Nazwa ?? "BRAK-DANYCH";

            if (profileLeft == 0)
            {
                slruchPoLewej = "";

                konfLeft = MVCKonfModelu.KonfSystem
                    .FirstOrDefault(e => e.WystepujeLewa);

                profileLeft = (float)((konfLeft?.PionPrawa ?? 0) - (konfLeft?.PionLewa ?? 0));

                RowIdprofileLeft = konfLeft?.RowId ?? Guid.Empty;
                RowIndeksprofileLeft = konfLeft?.IndeksElementu ?? "BRAK-DANYCH";
                RowNazwaprofileLeft = konfLeft?.Nazwa ?? "BRAK-DANYCH";

            }

            if (profileRight == 0)
            {
                slruchPoPrawej = "";

                konfRight = MVCKonfModelu.KonfSystem
                    .FirstOrDefault(e => e.WystepujePrawa);

                profileRight = (float)((konfRight?.PionPrawa ?? 0) - (konfRight?.PionLewa ?? 0));

                RowIdprofileRight = konfRight?.RowId ?? Guid.Empty;
                RowIndeksprofileRight = konfRight?.IndeksElementu ?? "BRAK-DANYCH";
                RowNazwaprofileRight = konfRight?.Nazwa ?? "BRAK-DANYCH";

            }

            string NazwaObiektu = MVCKonfModelu.KonfSystem.First().Nazwa ?? "";
            string TypObiektu = MVCKonfModelu.KonfSystem.First().Typ ?? "";

            Console.WriteLine($"📐Generator ----> region.TypKsztaltu: {region.TypKsztaltu} profileLeft: {profileLeft}, profileRight: {profileRight}, profileTop: {profileTop}, profileBottom: {profileBottom} slruchPoPrawej: {slruchPoPrawej} slruchPoLewej: {slruchPoLewej}");

            // 🔲 Oblicz wewnętrzny kontur

            if (ElementLiniowy)
            {
                var konfPolaczenia = daneKwadratu.FirstOrDefault(s => s.Przesuniecia != null)?.Przesuniecia;

                if (konfPolaczenia != null && konfPolaczenia.Count > 0)
                {
                    var szukPionA = Math.Abs(konfPolaczenia.FirstOrDefault(p => p.Strona.ToLower() == "góra" || p.Strona.ToLower() == "gora")?.PrzesuniecieYStycznej ?? 0);
                    var szukPionB = Math.Abs(konfPolaczenia.FirstOrDefault(p => p.Strona.ToLower() == "dół" || p.Strona.ToLower() == "dol")?.PrzesuniecieYStycznej ?? 0);
                    var szukPoziomA = Math.Abs(konfPolaczenia.FirstOrDefault(p => p.Strona.ToLower() == "lewa")?.PrzesuniecieYStycznej ?? 0);
                    var szukPoziomB = Math.Abs(konfPolaczenia.FirstOrDefault(p => p.Strona.ToLower() == "prawa")?.PrzesuniecieYStycznej ?? 0);
                    profileLeft = (float)szukPoziomA;
                    profileRight = (float)szukPoziomB;
                    profileTop = (float)szukPionA;
                    profileBottom = (float)szukPionB;

                    Console.WriteLine($"🔷 ElementLiniowy Znaleziono konfigurację przesunięcia dla przypadku poziomego. profileLeft: {profileLeft} profileRight: {profileRight} profileTop: {profileTop} profileBottom: {profileBottom}");
                }
                else
                {
                    Console.WriteLine($"🔷 ElementLiniowy Nie znaleziono konfiguracji przesunięcia dla przypadku poziomego. Domyślnie ustawiono 0 przesunięć.");
                    profileLeft = 0;
                    profileRight = 0;
                    profileTop = 0;
                    profileBottom = 0;
                }

                //foreach(var test in punktyRegionuMaster)
                //{
                //    Console.WriteLine($"🔷🔷🔷🔷🔷🔷🔷🔷 punktyRegionuMaster 1 Wierzcholek X: {test.X} Y: {test.Y} / {punktyRegionuMaster.Count}");
                //}

                //    foreach (var w in przeskalowanePunkty)
                //    {
                //        Console.WriteLine($"🔷🔷🔷🔷🔷🔷🔷🔷 przeskalowanePunkty Wierzcholek X: {w.X} Y: {w.Y}");
                //    }

                wewnetrznyKontur = przeskalowanePunkty;

                wewnetrznyKonturZLukami = przeskalowanePunktyZLukami;

                konturWenetrznyPodRysunek = przeskalowanePunktyZLukamiPodRysynek;

                wierzcholkiWenetrznePodRysunek = przeskalowanePunktyPodRysynek;

                // Napraw punkty startowe jeśli potrzebne
                //   wewnetrznyKonturZLukami = FixStartPoints(wewnetrznyKonturZLukami);

                punktyRegionuMaster = await CalculateOffsetPolygon(punktyRegionuMaster, profileLeft, profileRight, profileTop, profileBottom, false);

                //foreach (var test in punktyRegionuMaster)
                //{
                //    Console.WriteLine($"🔷🔷🔷🔷🔷🔷🔷🔷 punktyRegionuMaster 2 Wierzcholek X: {test.X} Y: {test.Y} / {punktyRegionuMaster.Count}");
                //}
            }
            else
            {

                wewnetrznyKontur = await CalculateOffsetPolygon(
                przeskalowanePunkty,
                profileLeft, profileRight, profileTop, profileBottom,
                false);

                //// Przed wywołaniem funkcji, dodaj diagnostykę:
                //Console.WriteLine($"===!===  DANE WEJŚCIOWE KONTURU WEWNĘTRZNEGO ===");
                //Console.WriteLine($"===!=== Liczba segmentów: {przeskalowanePunktyZLukami.Count}");
                //for (int i = 0; i < przeskalowanePunktyZLukami.Count; i++)
                //{
                //    var seg = przeskalowanePunktyZLukami[i];
                //    Console.WriteLine($" ===!===  Seg.{i}: {seg.Type} Start({seg.Start.X:F2};{seg.Start.Y:F2}) End({seg.End.X:F2};{seg.End.Y:F2})");
                //    if (seg.Type == SegmentType.Arc && seg.Center != null)
                //    {
                //        Console.WriteLine($"         Center({seg.Center.Value.X:F2};{seg.Center.Value.Y:F2}) R={seg.Radius:F2} CCW={seg.CounterClockwise}");
                //    }
                //}

                //Console.WriteLine($"===!=== ORYGINALNE SEGMENTY WEWNĘTRZNE ===");
                //for (int i = 0; i < wewnetrznyKonturZLukami.Count; i++)
                //{
                //    var seg = wewnetrznyKonturZLukami[i];
                //    Console.WriteLine($"===!===  [{i}] {seg.Type}: ({seg.Start.X:F2};{seg.Start.Y:F2}) -> ({seg.End.X:F2};{seg.End.Y:F2})");
                //    if (seg.Type == SegmentType.Arc)
                //    {
                //        Console.WriteLine($"===!===       Center: ({seg.Center.Value.X:F2};{seg.Center.Value.Y:F2}) R={seg.Radius:F2}");
                //    }
                //}

                // Napraw punkty startowe jeśli potrzebne
                // wewnetrznyKonturZLukami = FixStartPoints(wewnetrznyKonturZLukami);

                wewnetrznyKonturZLukami = await CalculateOffsetPolygonKontur(przeskalowanePunktyZLukami,
                    profileLeft, profileRight, profileTop, profileBottom,
                    false); // dla modeli z łukami i liniami

                liniaSzkleniaKontur = await CalculateOffsetPolygon(
                    przeskalowanePunkty,
                    offsetGlassLeft, offsetGlassRight, offsetGlassTop, offsetGlassBottom,
                    false);

                wierzcholkiWenetrznePodRysunek = await CalculateOffsetPolygon(
                    przeskalowanePunktyPodRysynek,
                    offsetKorpusWewnetrznyLeft, offsetKorpusWewnetrznyRight, offsetKorpusWewnetrznyTop, offsetKorpusWewnetrznyBottom,
                    false);

                //Console.WriteLine($"offsetLeft, offsetRight, offsetTop, offsetBottom, {offsetLeft}, {offsetRight}, {offsetTop}, {offsetBottom}");
                liniaSzkleniaKonturZLukami = await CalculateOffsetPolygonKontur(przeskalowanePunktyZLukami,
                    offsetGlassLeft, offsetGlassRight, offsetGlassTop, offsetGlassBottom,
                    false);

                konturWenetrznyPodRysunek = await CalculateOffsetPolygonKontur(
                    przeskalowanePunktyZLukami,
                    offsetKorpusWewnetrznyLeft, offsetKorpusWewnetrznyRight, offsetKorpusWewnetrznyTop, offsetKorpusWewnetrznyBottom,
                    false);
            }

            if (wewnetrznyKonturZLukami == null)
            {
                Console.WriteLine($"❌ Generowanie elementów zakończone niepowodzeniem dla regionu {regionId} wewnetrznyKonturZLukami == null");
                return false;
            }

            if (liniaSzkleniaKontur == null)
            {
                Console.WriteLine($"❌ Generowanie elementów zakończone niepowodzeniem dla regionu {regionId} liniaSzkleniaKontur == null");
                return false;
            }

            var okLine = await GenerateGenericElementsWithJoins(
                     przeskalowanePunkty,
                     wewnetrznyKontur,
                     przeskalowanePunktyZLukami,
                     wewnetrznyKonturZLukami,
                     profileLeft, profileRight, profileTop, profileBottom,
                     region.TypKsztaltu,
                     EdytowanyModel.PolaczenieNaroza,
                     EdytowanyModel.SposobLaczeniaCzop,
                     KonfiguracjeSystemu,
                     regionId,
                     RowIdprofileLeft, RowIdprofileRight, RowIdprofileTop, RowIdprofileBottom,
                     RowIndeksprofileLeft, RowIndeksprofileRight, RowIndeksprofileTop, RowIndeksprofileBottom,
                     RowNazwaprofileLeft, RowNazwaprofileRight, RowNazwaprofileTop, RowNazwaprofileBottom,
                     NazwaObiektu,
                     TypObiektu,
                     daneKwadratu,
                     punktyRegionuMaster,
                     mouseClik
                 );

            if (okLine)
            {
                Console.WriteLine($"✅ Generowanie elementów zakończone sukcesem dla regionu {regionId}");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ Generowanie elementów zakończone niepowodzeniem dla regionu {regionId} okLine: {okLine}");
                return false;
            }

        }

        public async Task<bool> GenerateGenericElementsWithJoins(
            List<XPoint> outer, List<XPoint> inner,
            List<ContourSegment> outerContourSegment, List<ContourSegment> innerContourSegment,
            float profileLeft, float profileRight, float profileTop, float profileBottom,
            string typKsztalt, string polaczenia, bool sposobLaczeniaCzop, List<KonfSystem> model, string regionId,
            Guid rowIdprofileLeft, Guid rowIdprofileRight, Guid rowIdprofileTop, Guid rowIdprofileBottom,
            string rowIndeksprofileLeft, string rowIndeksprofileRight, string rowIndeksprofileTop, string rowIndeksprofileBottom,
            string rowNazwaprofileLeft, string rowNazwaprofileRight, string rowNazwaprofileTop, string rowNazwaprofileBottom,
            string NazwaObiektu, string TypObiektu, List<DaneKwadratu> daneKwadratu, List<XPoint> punktyRegionuMaster, XPoint mouseClik)
        {

            await Task.Yield(); // wymuszenie asynchroniczności, aby uniknąć blokowania wątków UI

            // Console.WriteLine($"▶️ Generowanie elementów dla regionu {regionId} z typem kształtu: {typKsztalt} oraz ElementLiniowy: {ElementLiniowy} profileLeft: {profileLeft}, profileRight :{profileRight}");

            // Użyj oryginalnych segmentów (nieposortowanych) - one i tak będą dopasowane przez Build4SegmentContour
            // outerContourSegment i innerContourSegment pozostają BEZ ZMIAN
            float angleDegreesElementLionowy = 0;

            float katGornegoElemntu = GetTopEdgeAngleFromFirstSegment(outer);

            // 🔹 Nowy tryb – jeśli to tylko element liniowy (np. słupek)
            if (ElementLiniowy)
            {
                if (outer == null || outer.Count < 2)
                {
                    BledySystemowe.Add($"❌ Element: brak wystarczającej liczby punktów (min. 2 wymagane) dla regionu {regionId}.");
                    return false;
                }

                var szukDaneKwadratu = daneKwadratu
                 .Where(x => x.Wierzcholki.Count == 2 && x.BoolElementLinia)
                 .DistinctBy(x => (
                     Math.Round(x.Wierzcholki[0].X, 2),
                     Math.Round(x.Wierzcholki[0].Y, 2),
                     Math.Round(x.Wierzcholki[1].X, 2),
                     Math.Round(x.Wierzcholki[1].Y, 2)
                 ))
                 .LastOrDefault();


                //var szukDaneKwadratu = daneKwadratu
                // .Where(x => x.Wierzcholki.Count == 2)
                //   .LastOrDefault();

                //   Console.WriteLine($"▶️ Element X wartość X: {szukDaneKwadratu.Wierzcholki[0].X} dotyczy: ElementLiniowy: {ElementLiniowy} rowIdprofileLeft: {rowIdprofileLeft} rowIdprofileRight: {rowIdprofileRight} rowIdprofileTop: {rowIdprofileTop} rowIdprofileBottom: {rowIdprofileBottom}");

                //if (szukDaneKwadratu != null)
                //{
                //    Console.WriteLine($"▶️ Element model.Count:{model.Count()} szukDaneKwadratu.Wierzcholki.Count: {szukDaneKwadratu?.Wierzcholki.Count()} RuchomySlupekPoLewej:{RuchomySlupekPoLewej} RuchomySlupekPoPrawej:{RuchomySlupekPoPrawej}");

                //    foreach (var dk in szukDaneKwadratu.Wierzcholki)
                //    {
                //        Console.WriteLine($"▶️ ElementX:{dk.X} Y:{dk.Y}");
                //    }
                //}

                XPoint outerStart = szukDaneKwadratu.Wierzcholki[0];
                XPoint outerEnd = szukDaneKwadratu.Wierzcholki[1];

                XPoint _innerStart = szukDaneKwadratu.Wierzcholki[0];
                XPoint _innerEnd = szukDaneKwadratu.Wierzcholki[1];

                _innerStart.X = _innerStart.X + profileLeft; //Słupek prawy lewy zawsze to samo
                _innerEnd.X = _innerEnd.X + profileLeft;

                float dx = (float)(outerEnd.X - outerStart.X);
                float dy = (float)(outerEnd.Y - outerStart.Y);
                float length = MathF.Sqrt(dx * dx + dy * dy);

                float angleRadians = MathF.Atan2(dy, dx); // kąt w radianach
                angleDegreesElementLionowy = angleRadians * (180f / MathF.PI); // kąt w stopniach

                // Przekształć do zakresu 0–360°, jeśli potrzebujesz
                if (angleDegreesElementLionowy < 0)
                    angleDegreesElementLionowy += 360f;

                // outer = new List<XPoint> { outerStart, outerEnd }; // chyba do wywalenia
                // inner = new List<XPoint> { _innerStart, _innerEnd }; // chyba do wywalenia

            }

            // 🔹 Standardowy tryb wielokąta zamkniętego
            int vertexCount = outer.Count;

            if (vertexCount < 3 && !ElementLiniowy)
            {
                BledySystemowe.Add($"❌ Wielokąt musi mieć co najmniej 3 wierzchołki. Obecnie ma {vertexCount} wierzchołków. Sprawdź dane wejściowe dla regionu {regionId}.");
                return false;
            }
            //throw new Exception("Wielokąt musi mieć co najmniej 3 wierzchołki.");

            outer = RemoveDuplicateConsecutivePoints(outer);
            inner = RemoveDuplicateConsecutivePoints(inner);

            // Console.WriteLine($"▶️ Generuje elementy z polygon with vertexCount: {vertexCount} vertices and joins: {polaczenia} angleDegreesElementLionowy: {angleDegreesElementLionowy}");

            var parsedConnections = polaczenia.Split(';')
                .Select(p => p.Split('-'))
                .Where(parts => parts.Length == 2)
                .Select(parts => (kat: int.Parse(parts[0]), typ: parts[1].Trim()))
                .ToList();

            // Jeśli mniej połączeń niż narożników — powielaj ostatnie
            while (parsedConnections.Count < vertexCount)
            {
                parsedConnections.Add(parsedConnections.Last());
            }

            // Jeśli więcej — przytnij
            if (parsedConnections.Count > vertexCount)
            {
                parsedConnections = parsedConnections.Take(vertexCount).ToList();
            }

            var polaczeniaArray = parsedConnections.ToArray();

            //for (int i = 0; i < polaczeniaArray.Count(); i++)
            //{
            //    Console.WriteLine($"🔷🔷 polaczeniaArray {i}: Join Kat: {polaczeniaArray[i].kat} Typ: {polaczeniaArray[i].typ}");
            //}

            //foreach (var test in inner)
            //{
            //    Console.WriteLine($"🔷🔷 inner point X: {test.X} Y: {test.Y}");
            //}

            //foreach (var test in outer)
            //{
            //    Console.WriteLine($"🔷🔷 outer point X: {test.X} Y: {test.Y}");
            //}

            // =============================
            // 1️⃣ Stwórz tablicę połączeń dla wszystkich boków
            // =============================

            // Najpierw parsujemy dane wzorca (kwadrat)
            var wzorzecPolaczen = new Dictionary<string, string>(); // klucz: strona, wartość: typ

            // Najpierw parsujemy dane wzorca (kwadrat)

            foreach (var pair in polaczenia.Split(';'))
            {
                var parts = pair.Split('-');
                double kat = double.Parse(parts[0]);
                string typ = parts[1];

                string strona = StronaOknaHelper.OkreslStroneNaPodstawieKataLinii(kat);
                wzorzecPolaczen[strona] = typ;

                //   Console.WriteLine($"📐 Wzorzec: kąt {kat}° → strona {strona} → typ {typ}");
            }

            //Console.WriteLine($"🔷🔷 Wzorzec połączeń dla stron: outer: {outer.Count} vertexCount:{vertexCount}");

            // =============================
            // 1️⃣ Zliczamy elementy według stron
            // =============================
            var elementyWedlugStron = new Dictionary<string, List<int>>(); // strona -> lista indeksów

            for (int i = 0; i < vertexCount; i++)
            {
                int next = (i + 1) % vertexCount;

                float dx = (float)(outer[next].X - outer[i].X);
                float dy = (float)(outer[next].Y - outer[i].Y);
                float angleRadians = MathF.Atan2(dy, dx);
                float angleDegrees = angleRadians * (180f / MathF.PI);
                if (angleDegrees < 0) angleDegrees += 360f;

                string strona = StronaOknaHelper.OkreslStrone(angleDegrees, i, outer);

                if (!elementyWedlugStron.ContainsKey(strona))
                    elementyWedlugStron[strona] = new List<int>();

                elementyWedlugStron[strona].Add(i);
            }

            //// Debug: pokażmy zliczone elementy
            //foreach (var kv in elementyWedlugStron)
            //{
            //    Console.WriteLine($"📊 Strona {kv.Key}: {kv.Value.Count} elementów - indeksy: [{string.Join(", ", kv.Value)}]");
            //}

            // =============================
            // 2️⃣ Tworzymy mapowanie typów dla narożników
            // =============================
            var typyNaroznikow = new Dictionary<string, string>(); // klucz: "stronaA-stronaB", wartość: typ

            // Dla każdej pary stron, określamy typ połączenia
            foreach (var stronaA in elementyWedlugStron.Keys)
            {
                foreach (var stronaB in elementyWedlugStron.Keys)
                {
                    string klucz = $"{stronaA}-{stronaB}";

                    if (stronaA == stronaB)
                    {
                        // Połączenie tej samej strony z samą sobą
                        // Używamy typu z pierwszego elementu tej strony
                        string typ = wzorzecPolaczen.ContainsKey(stronaA) ? wzorzecPolaczen[stronaA] : "T2";
                        typyNaroznikow[klucz] = typ;
                        // Console.WriteLine($"🔗 Połączenie {klucz} (ta sama strona) → typ {typ}");
                    }
                    else
                    {
                        // 🔑 POPRAWKA: Dla różnych stron, typ pochodzi z PIERWSZEJ strony w kolejności (zgodnie z ruchem wskazówek zegara)
                        // Ale musimy ustalić, która strona jest "pierwsza" w danym narożniku

                        // W Twoim przypadku, dla narożnika Lewa-Góra, typ powinien być z Góry (T1), a nie z Lewej (T4)
                        // To sugeruje, że typ pochodzi z DRUGIEJ strony w nazwie narożnika?

                        // Spróbujmy: typ pochodzi z DRUGIEJ strony (stronaB)
                        string typ = wzorzecPolaczen.ContainsKey(stronaB) ? wzorzecPolaczen[stronaB] : "T2";
                        typyNaroznikow[klucz] = typ;
                        //  Console.WriteLine($"🔗 Połączenie {klucz} (różne strony) → typ {typ} (ze strony {stronaB})");
                    }
                }
            }

            // =============================
            // 3️⃣ Główna pętla – leftJoin / rightJoin
            // =============================
            float firstangleDegrees = -1;//Kąt pierwszego boku, do porównania z innymi, aby wykryć pełny obrót

            string stonaOstanioDodanegoElementu = ""; // Typ połączenia na końcu prawej strony (dla ostatnio dodanego elementu)

            //foreach (var test in innerKontur)
            //{
            //    Console.WriteLine($"🔷🔷 innerKontur point Start.X: {test.Start.X} Start.Y: {test.Start.Y} End.X: {test.End.X} End.Y: {test.End.Y}");
            //}

            for (int i = 0; i < vertexCount; i++)
            {
                int next = (i + 1) % vertexCount;
                int prev = (i - 1 + vertexCount) % vertexCount;

                // Oblicz kąt bieżącego boku
                float dx = (float)(outer[next].X - outer[i].X);
                float dy = (float)(outer[next].Y - outer[i].Y);
                float angleRadians = MathF.Atan2(dy, dx);
                float angleDegrees = angleRadians * (180f / MathF.PI);
                if (angleDegrees < 0) angleDegrees += 360f;

                if (firstangleDegrees == -1) firstangleDegrees = angleDegrees;

                // OKREŚLENIE STRONY BIEŻĄCEGO ELEMENTU
                string currentSide = StronaOknaHelper.OkreslStrone(angleDegrees, i, outer);

                // OKREŚLENIE STRONY POPRZEDNIEJ
                float dxPrev = (float)(outer[i].X - outer[prev].X);
                float dyPrev = (float)(outer[i].Y - outer[prev].Y);
                float anglePrev = MathF.Atan2(dyPrev, dxPrev) * 180f / MathF.PI;
                if (anglePrev < 0) anglePrev += 360f;
                string prevSide = StronaOknaHelper.OkreslStrone(anglePrev, prev, outer);

                // OKREŚLENIE STRONY NASTĘPNEJ
                float dxNext = (float)(outer[(next + 1) % vertexCount].X - outer[next].X);
                float dyNext = (float)(outer[(next + 1) % vertexCount].Y - outer[next].Y);
                float angleNext = MathF.Atan2(dyNext, dxNext) * 180f / MathF.PI;
                if (angleNext < 0) angleNext += 360f;
                string nextSide = StronaOknaHelper.OkreslStrone(angleNext, next, outer);

                // 🔑 Pobieramy typy połączeń z mapy narożników
                string leftJoin = typyNaroznikow[$"{prevSide}-{currentSide}"];  // lewy narożnik: poprzednia-bieżąca
                string rightJoin = typyNaroznikow[$"{currentSide}-{nextSide}"]; // prawy narożnik: bieżąca-następna

                // Mapujemy strony na typy ze wzorca (tylko dla debugowania)
                string typBiezacej = wzorzecPolaczen.ContainsKey(currentSide) ? wzorzecPolaczen[currentSide] : "T2";
                string typPoprzedniej = wzorzecPolaczen.ContainsKey(prevSide) ? wzorzecPolaczen[prevSide] : "T2";
                string typNastepnej = wzorzecPolaczen.ContainsKey(nextSide) ? wzorzecPolaczen[nextSide] : "T2";

                //Console.WriteLine($"▶️🔷🔷▶️ Processing element {i + 1}/{vertexCount} with joins: {leftJoin} - {rightJoin} " +
                //                 $"wyliczony kąt: {angleDegrees:F2}° dla i: {i} StronaElementu: {currentSide} " +
                //                 $"(prev: {prevSide} [{typPoprzedniej}], next: {nextSide} [{typNastepnej}])");

                //Console.WriteLine($"   📍 Narożniki: lewy ({prevSide}-{currentSide}) = {leftJoin}, " +
                //                 $"prawy ({currentSide}-{nextSide}) = {rightJoin}");

                bool dodajA = false;
                bool dodajB = false;
                XPoint outerStart = outer[i];
                XPoint outerEnd = outer[next];

                XPoint _innerStart = inner[i];
                XPoint _innerEnd = inner[next];
                float length = MathF.Sqrt(dx * dx + dy * dy);

                StronaElementu = currentSide;

                //  Console.WriteLine($"▶️ Processing element {i + 1}/{vertexCount} with joins: {leftJoin} - {rightJoin} wyliczony kąt: {angleDegrees} dla i: {i} StronaElementu: {StronaElementu} length: {length} polaczenia: {polaczenia}");

                if (length < 0.001f) continue;

                float tx = dx / length;
                float ty = dy / length;
                float nx = -ty;
                float ny = tx;

                float profile = Math.Abs(dx) > Math.Abs(dy)
                    ? (ny > 0 ? profileTop : profileBottom)
                    : (nx > 0 ? profileRight : profileLeft);

                float profileA = Math.Abs(dx) > Math.Abs(dy) ? profileTop : profileRight;

                float profileB = Math.Abs(dx) > Math.Abs(dy) ? profileBottom : profileLeft;

                bool isAlmostHorizontal = Math.Abs(dy) < 1e-2;
                bool isAlmostVertical = Math.Abs(dx) < 1e-2;


                //-------------------------------------------------------------------------------------------------------------------------------
                // USTAWIENIA AUTOMATYCZNEGO WYBORU T2
                //-------------------------------------------------------------------------------------------------------------------------------

                // Maksymalny kąt, który traktujemy jako OSTRY narożnik.
                // Przykład:
                // 45°  -> każdy narożnik <= 45° będzie automatycznie T2
                // 30°  -> tylko bardzo ostre narożniki będą T2
                // 60°  -> również łagodniejsze narożniki będą T2
                double katOstryT2 = 46.0;


                //-------------------------------------------------------------------------------------------------------------------------------
                // OBLICZANIE KĄTÓW LEWEJ I PRAWEJ STRONY
                //-------------------------------------------------------------------------------------------------------------------------------

                // Wektor bieżącego segmentu
                double currentDx = outerEnd.X - outerStart.X;
                double currentDy = outerEnd.Y - outerStart.Y;

                // Wektor poprzedniego segmentu
                XPoint outerPrev = outer[prev];

                double prevDx = outerStart.X - outerPrev.X;
                double prevDy = outerStart.Y - outerPrev.Y;

                // Wektor następnego segmentu
                int next2 = (next + 1) % vertexCount;

                XPoint outerNext2 = outer[next2];

                double nextDx = outerNext2.X - outerEnd.X;
                double nextDy = outerNext2.Y - outerEnd.Y;


                //-------------------------------------------------------------------------------------------------------------------------------
                // DŁUGOŚCI WEKTORÓW
                //-------------------------------------------------------------------------------------------------------------------------------

                double magCurrent =
                    Math.Sqrt(
                        currentDx * currentDx +
                        currentDy * currentDy);

                double magPrev =
                    Math.Sqrt(
                        prevDx * prevDx +
                        prevDy * prevDy);

                double magNext =
                    Math.Sqrt(
                        nextDx * nextDx +
                        nextDy * nextDy);


                //-------------------------------------------------------------------------------------------------------------------------------
                // KĄT BIEŻĄCY <-> POPRZEDNI
                //
                // Strona A = LEWA
                //-------------------------------------------------------------------------------------------------------------------------------

                float angleDegreesStronaA = 0;

                if (magCurrent > 0.000001 &&
                    magPrev > 0.000001)
                {
                    double dot =
                        currentDx * prevDx +
                        currentDy * prevDy;

                    double cos =
                        dot / (magCurrent * magPrev);

                    cos = Math.Max(-1.0, Math.Min(1.0, cos));

                    angleDegreesStronaA =
                        (float)(
                            Math.Acos(cos) *
                            180.0 /
                            Math.PI);
                }


                //-------------------------------------------------------------------------------------------------------------------------------
                // KĄT BIEŻĄCY <-> NASTĘPNY
                //
                // Strona B = PRAWA
                //-------------------------------------------------------------------------------------------------------------------------------

                float angleDegreesStronaB = 0;

                if (magCurrent > 0.000001 &&
                    magNext > 0.000001)
                {
                    double dot =
                        currentDx * nextDx +
                        currentDy * nextDy;

                    double cos =
                        dot / (magCurrent * magNext);

                    cos = Math.Max(-1.0, Math.Min(1.0, cos));

                    angleDegreesStronaB =
                        (float)(
                            Math.Acos(cos) *
                            180.0 /
                            Math.PI);
                }


                //-------------------------------------------------------------------------------------------------------------------------------
                // KĄTY KIERUNKOWE
                //-------------------------------------------------------------------------------------------------------------------------------

                double currentDirection =
                    Math.Atan2(currentDy, currentDx) *
                    180.0 /
                    Math.PI;

                double prevDirection =
                    Math.Atan2(prevDy, prevDx) *
                    180.0 /
                    Math.PI;

                double nextDirection =
                    Math.Atan2(nextDy, nextDx) *
                    180.0 /
                    Math.PI;


                // Normalizacja 0..360
                currentDirection =
                    (currentDirection + 360.0) % 360.0;

                prevDirection =
                    (prevDirection + 360.0) % 360.0;

                nextDirection =
                    (nextDirection + 360.0) % 360.0;


                //-------------------------------------------------------------------------------------------------------------------------------
                // RÓŻNICA KIERUNKÓW
                //-------------------------------------------------------------------------------------------------------------------------------

                double diffPrev =
                    Math.Abs(
                        currentDirection -
                        prevDirection);

                if (diffPrev > 180.0)
                    diffPrev = 360.0 - diffPrev;


                double diffNext =
                    Math.Abs(
                        currentDirection -
                        nextDirection);

                if (diffNext > 180.0)
                    diffNext = 360.0 - diffNext;


                ////-------------------------------------------------------------------------------------------------------------------------------
                //// DEBUG
                ////-------------------------------------------------------------------------------------------------------------------------------

                //Console.WriteLine(
                //    $"🔹🔹🔹🔹🔹🔹 Wierzchołek {i + 1} | " +
                //    $"Kąt elementu={angleDegrees:F2}° | " +
                //    $"A={angleDegreesStronaA:F2}° | " +
                //    $"B={angleDegreesStronaB:F2}° | " +
                //    $"Próg T2={katOstryT2:F2}° | " +
                //    $"DiffA={diffPrev:F2}° | " +
                //    $"DiffB={diffNext:F2}°");


                //-------------------------------------------------------------------------------------------------------------------------------
                // AUTOMATYCZNE T2 - LEWA STRONA
                //
                // Jeżeli rzeczywisty kąt po lewej stronie jest ostry,
                // automatycznie ustawiamy T2.
                //
                // <= katOstryT2
                //-------------------------------------------------------------------------------------------------------------------------------

                bool lewyKatOstry =
                    angleDegreesStronaA > 0.001 &&
                    angleDegreesStronaA <= katOstryT2;


                //-------------------------------------------------------------------------------------------------------------------------------
                // AUTOMATYCZNE T2 - PRAWA STRONA
                //
                // Dokładnie ta sama zasada jak po lewej.
                // Dzięki temu obie strony są traktowane symetrycznie.
                //-------------------------------------------------------------------------------------------------------------------------------

                bool prawyKatOstry =
                    angleDegreesStronaB > 0.001 &&
                    angleDegreesStronaB <= katOstryT2;


                //-------------------------------------------------------------------------------------------------------------------------------
                // T2 - LEWA STRONA
                //-------------------------------------------------------------------------------------------------------------------------------

                if (lewyKatOstry)
                {
                    leftJoin = "T2";

                    BledySystemowe.Add(
                        $"⚠️ Wierzchołek element nr: {i + 1}: " +
                        $"ostry kąt po lewej stronie = " +
                        $"{angleDegreesStronaA:F1}° " +
                        $"(próg T2 = {katOstryT2:F1}°). " +
                        $"Automatycznie ustawiono T2 dla lewego narożnika.");
                }


                //-------------------------------------------------------------------------------------------------------------------------------
                // T2 - PRAWA STRONA
                //-------------------------------------------------------------------------------------------------------------------------------

                if (prawyKatOstry)
                {
                    rightJoin = "T2";

                    BledySystemowe.Add(
                        $"⚠️ Wierzchołek element nr: {i + 1}: " +
                        $"ostry kąt po prawej stronie = " +
                        $"{angleDegreesStronaB:F1}° " +
                        $"(próg T2 = {katOstryT2:F1}°). " +
                        $"Automatycznie ustawiono T2 dla prawego narożnika.");
                }


                if (sposobLaczeniaCzop)
                {

                    if (leftJoin == "T1" && isAlmostVertical)
                    {
                        dodajA = true;
                    }
                    if (rightJoin == "T1" && isAlmostVertical)
                    {
                        dodajB = true;
                    }
                    if (leftJoin == "T3" && isAlmostHorizontal)
                    {
                        dodajA = true;
                    }
                    if (rightJoin == "T3" && isAlmostHorizontal)
                    {
                        dodajB = true;
                    }
                    if (leftJoin == "T5")
                    {
                        dodajA = true;
                    }
                    if (rightJoin == "T5")
                    {
                        dodajB = true;
                    }
                    if (leftJoin == "T2")
                    {
                        dodajA = true;
                    }
                    if (rightJoin == "T2")
                    {
                        dodajB = true;
                    }
                }

                if (!isAlmostHorizontal && !isAlmostVertical && vertexCount > 4)
                {
                    if (leftJoin == "T1" && rightJoin == "T1")
                    {
                        isAlmostHorizontal = true;
                    }
                    else if (leftJoin == "T3" && rightJoin == "T3")
                    {
                        isAlmostVertical = true;
                    }
                }

                List<XPoint>? wierzcholki;
                List<ContourSegment>? wierzcholkiZLukami;

                //Console.WriteLine($"🔷 element --> {i + 1}/{vertexCount} with joins: {leftJoin} - {rightJoin} angleDegrees: {angleDegrees} katGornegoElemntu: {katGornegoElemntu} StronaElementu: {StronaElementu}");

                if ((leftJoin == "T1" && rightJoin == "T4") ||
                (leftJoin == "T4" && rightJoin == "T1"))
                {
                    // ============================================================
                    // T1/T4 lub T4/T1
                    // ============================================================

                    bool isTriangle = vertexCount == 3;

                    // ============================================================
                    // PRZYPADEK: T4 -> T1
                    // ============================================================
                    if (leftJoin == "T4" && rightJoin == "T1")
                    {
                        // --------------------------------------------------------
                        // TRÓJKĄT
                        // --------------------------------------------------------
                        if (isTriangle)
                        {
                            // Dla trójkąta zachowujemy logikę T1,
                            // ale punkt końcowy wyznaczamy specjalną funkcją.
                            //
                            // GetStartT1Triangle oraz GetEndT1Triangle
                            // mają za zadanie znaleźć odpowiednie przecięcia
                            // bez zmieniania pozostałej logiki T1/T4.

                            List<XPoint> getStartT1 = GetStartT1Triangle(
                                inner[i],
                                outer[i],
                                outer,
                                angleDegrees,
                                anglePrev,
                                angleNext,
                                StronaElementu,
                                stonaOstanioDodanegoElementu,
                                i, next, prev);

                            var _anglePrev = anglePrev;

                            if (i == vertexCount - 1)
                            {
                                _anglePrev = firstangleDegrees;
                            }

                            List<XPoint> getEndT1 = GetEndT1Triangle(
                                inner[next],
                                outer[next],
                                outer,
                                angleDegrees,
                                _anglePrev,
                                angleNext,
                                StronaElementu,
                                stonaOstanioDodanegoElementu,
                                i, next, prev
                                );

                            wierzcholki = new List<XPoint>
                            {
                                getStartT1[1],
                                getEndT1[1],
                                getEndT1[0],
                                getStartT1[0]
                            };
                        }

                        // --------------------------------------------------------
                        // NORMALNA FIGURA
                        // --------------------------------------------------------
                        else if (isAlmostHorizontal)
                        {
                            // Przecięcia z konturem na bazie normalnej

                            var outerVecStart =
                                FindFirstEdgeIntersection(
                                    outerStart,
                                    nx,
                                    ny,
                                    outer);

                            var outerVecEnd =
                                FindFirstEdgeIntersection(
                                    outerEnd,
                                    nx,
                                    ny,
                                    outer);

                            var innerVecStart =
                                FindFirstEdgeIntersection(
                                    new XPoint(
                                        outerVecStart.X + nx * profile,
                                        outerVecStart.Y + ny * profile),
                                    tx,
                                    ty,
                                    outer);

                            var innerVecEnd =
                                FindFirstEdgeIntersection(
                                    new XPoint(
                                        outerVecEnd.X + nx * profile,
                                        outerVecEnd.Y + ny * profile),
                                    tx,
                                    ty,
                                    outer);

                            wierzcholki = new List<XPoint>
                            {
                                outerVecStart,
                                outerVecEnd,
                                innerVecEnd,
                                innerVecStart
                            };
                        }
                        else
                        {
                            // ----------------------------------------------------
                            // PRZYPADEK PIONOWY
                            // ----------------------------------------------------

                            // Jeżeli kiedyś pojawi się T4/T4 tutaj,
                            // pozostawiamy zabezpieczenie z poprzedniej logiki.
                            if (leftJoin == "T4" &&
                                rightJoin == "T4" &&
                                vertexCount > 4)
                            {
                                var topY =
                                    Math.Min(inner[i].Y, inner[next].Y);

                                var bottomY =
                                    Math.Max(inner[i].Y, inner[next].Y);

                                var outerTop =
                                    GetHorizontalIntersection(
                                        _innerStart,
                                        _innerEnd,
                                        (float)topY);

                                var outerBottom =
                                    GetHorizontalIntersection(
                                        _innerStart,
                                        _innerEnd,
                                        (float)bottomY);

                                var innerTop =
                                    GetHorizontalIntersection(
                                        outer[i],
                                        outer[next],
                                        (float)topY);

                                var innerBottom =
                                    GetHorizontalIntersection(
                                        outer[i],
                                        outer[next],
                                        (float)bottomY);

                                wierzcholki = new List<XPoint>
                                {
                                    outerTop,
                                    outerBottom,
                                    innerBottom,
                                    innerTop
                                };
                            }
                            else
                            {
                                var topY =
                                    Math.Min(inner[i].Y, inner[next].Y);

                                var bottomY =
                                    Math.Max(inner[i].Y, inner[next].Y);

                                var outerBottom =
                                    GetHorizontalIntersection(
                                        outerStart,
                                        outerEnd,
                                        (float)bottomY);

                                var innerTop =
                                    GetHorizontalIntersection(
                                        inner[i],
                                        inner[next],
                                        (float)topY);

                                var innerBottom =
                                    GetHorizontalIntersection(
                                        inner[i],
                                        inner[next],
                                        (float)bottomY);

                                XPoint outerTop;

                                if (i == vertexCount - 1)
                                {
                                    outerTop =
                                        FindFirstEdgeIntersectionByAngle(
                                            innerTop,
                                            firstangleDegrees - 180,
                                            outer);
                                }
                                else
                                {
                                    if (angleDegrees == 270)
                                    {
                                        outerTop =
                                            FindFirstEdgeIntersectionByAngle(
                                                innerTop,
                                                180 + angleNext,
                                                outer);
                                    }
                                    else
                                    {
                                        outerTop =
                                            FindFirstEdgeIntersectionByAngle(
                                                innerTop,
                                                anglePrev,
                                                outer);
                                    }
                                }

                                wierzcholki = new List<XPoint>
                                {
                                    outerTop,
                                    outerBottom,
                                    innerBottom,
                                    innerTop
                                };
                            }
                        }
                    }

                    // ============================================================
                    // PRZYPADEK: T1 -> T4
                    // ============================================================
                    else // leftJoin == "T1" && rightJoin == "T4"
                    {
                        // --------------------------------------------------------
                        // TRÓJKĄT
                        // --------------------------------------------------------
                        if (isTriangle)
                        {
                            // Tutaj również wykorzystujemy specjalną obsługę
                            // trójkąta, ale odwracamy rolę start/end.
                            //
                            // Dzięki temu geometria trójkąta nie korzysta
                            // z normalnej logiki dla czworokątów.

                            List<XPoint> getStartT1 = GetStartT1Triangle(
                                inner[i],
                                outer[i],
                                outer,
                                angleDegrees,
                                anglePrev,
                                angleNext,
                                StronaElementu,
                                stonaOstanioDodanegoElementu,
                                i, next, prev);

                            var _anglePrev = anglePrev;

                            if (i == vertexCount - 1)
                            {
                                _anglePrev = firstangleDegrees;
                            }

                            List<XPoint> getEndT1 = GetEndT1Triangle(
                                inner[next],
                                outer[next],
                                outer,
                                angleDegrees,
                                _anglePrev,
                                angleNext,
                                StronaElementu,
                                stonaOstanioDodanegoElementu,
                                i, next, prev
                                );

                            wierzcholki = new List<XPoint>
                            {
                                getStartT1[1],
                                getEndT1[1],
                                getEndT1[0],
                                getStartT1[0]
                            };
                        }

                        // --------------------------------------------------------
                        // NORMALNA FIGURA - POZIOMA
                        // --------------------------------------------------------
                        else if (isAlmostHorizontal)
                        {
                            var outerVecStart =
                                FindFirstEdgeIntersection(
                                    outerStart,
                                    nx,
                                    ny,
                                    outer);

                            var outerVecEnd =
                                FindFirstEdgeIntersection(
                                    outerEnd,
                                    nx,
                                    ny,
                                    outer);

                            var innerVecStart =
                                FindFirstEdgeIntersection(
                                    new XPoint(
                                        outerVecStart.X + nx * profile,
                                        outerVecStart.Y + ny * profile),
                                    tx,
                                    ty,
                                    outer);

                            var innerVecEnd =
                                FindFirstEdgeIntersection(
                                    new XPoint(
                                        outerVecEnd.X + nx * profile,
                                        outerVecEnd.Y + ny * profile),
                                    tx,
                                    ty,
                                    outer);

                            wierzcholki = new List<XPoint>
                            {
                                outerVecStart,
                                outerVecEnd,
                                innerVecEnd,
                                innerVecStart
                            };
                        }

                        // --------------------------------------------------------
                        // NORMALNA FIGURA - PIONOWA
                        // --------------------------------------------------------
                        else
                        {
                            var topY =
                                Math.Min(inner[i].Y, inner[next].Y);

                            var bottomY =
                                Math.Max(inner[i].Y, inner[next].Y);

                            var outerBottom =
                                GetHorizontalIntersection(
                                    outerStart,
                                    outerEnd,
                                    (float)bottomY);

                            var innerTop =
                                GetHorizontalIntersection(
                                    inner[i],
                                    inner[next],
                                    (float)topY);

                            var innerBottom =
                                GetHorizontalIntersection(
                                    inner[i],
                                    inner[next],
                                    (float)bottomY);

                            XPoint outerTop;

                            if (i == vertexCount - 1)
                            {
                                outerTop =
                                    FindFirstEdgeIntersectionByAngle(
                                        innerTop,
                                        firstangleDegrees - 180,
                                        outer);
                            }
                            else
                            {
                                if (anglePrev == -1 && vertexCount < 4)
                                {
                                    innerTop = inner[i];

                                    outerTop =
                                        FindFirstEdgeIntersectionByAngle(
                                            innerTop,
                                            anglePrev,
                                            outer);
                                }
                                else
                                {
                                    outerTop =
                                        FindFirstEdgeIntersectionByAngle(
                                            innerTop,
                                            anglePrev,
                                            outer);
                                }
                            }

                            // Specjalna korekta dla małych figur,
                            // zachowana dokładnie z poprzedniej wersji.
                            if (vertexCount < 4 && anglePrev != -1)
                            {
                                innerTop =
                                    FindFirstEdgeIntersectionByAngle(
                                        innerTop,
                                        angleDegrees - 180,
                                        outer);

                                outerTop =
                                    FindFirstEdgeIntersectionByAngle(
                                        outerTop,
                                        angleDegrees - 180,
                                        outer);
                            }

                            wierzcholki = new List<XPoint>
                            {
                                outerTop,
                                outerBottom,
                                innerBottom,
                                innerTop
                            };
                        }
                    }
                }
                else if (leftJoin == "T4" && rightJoin == "T4")
                {
                    List<XPoint> getStartT4 = GetStartT4(inner[i]);
                    List<XPoint> getEndT4 = GetEndT4(inner[next]);

                    wierzcholki = new List<XPoint> {
                            getStartT4[1], getEndT4[1], getEndT4[0], getStartT4[0]
                        };


                }
                else if (leftJoin == "T1" && rightJoin == "T1")
                {
                    if (vertexCount == 3)
                    {
                        // ============================================================
                        // T1 / T1 - TRÓJKĄT
                        // ============================================================

                        List<XPoint> startT1 = GetStartT1Triangle(
                            inner[i],
                            outer[i],
                            outer,
                            angleDegrees,
                            anglePrev,
                            angleNext,
                            StronaElementu,
                            stonaOstanioDodanegoElementu,
                            i, next, prev);

                        int nextTriangle = (i + 1) % vertexCount;

                        List<XPoint> endT1 = GetEndT1Triangle(
                            inner[nextTriangle],
                            outer[nextTriangle],
                            outer,
                            angleDegrees,
                            anglePrev,
                            angleNext,
                            StronaElementu,
                            stonaOstanioDodanegoElementu,
                            i, next, prev);

                        wierzcholki = new List<XPoint>
                        {
                            startT1[1],
                            endT1[1],
                            endT1[0],
                            startT1[0]
                        };
                    }
                    else
                    {
                        // ============================================================
                        // T1 / T1 - DOTYCHCZASOWA LOGIKA
                        // ============================================================

                        List<XPoint> getStartT1 = GetStartT1(
                            inner[i],
                            outer[i],
                            outer,
                            angleDegrees,
                            anglePrev,
                            angleNext,
                            StronaElementu,
                            stonaOstanioDodanegoElementu,
                            vertexCount < 6 ? -1 : i);

                        var _anglePrev = anglePrev;

                        if (i == vertexCount - 1)
                        {
                            _anglePrev = firstangleDegrees;
                        }

                        List<XPoint> getEndT1 = GetEndT1(
                            inner[next],
                            outer[next],
                            outer,
                            angleDegrees,
                            _anglePrev,
                            angleNext,
                            StronaElementu,
                            stonaOstanioDodanegoElementu,
                            vertexCount < 6 ? -1 : i);

                        wierzcholki = new List<XPoint>
                        {
                            getStartT1[1],
                            getEndT1[1],
                            getEndT1[0],
                            getStartT1[0]
                        };
                    }
                }
                else if (leftJoin == "T3" && rightJoin == "T3")
                {
                    if (vertexCount == 3)
                    {
                        List<XPoint> getStartT3 = GetStartT3Triangle(
                                       inner[i],
                                       outer[i],
                                       outer,
                                       angleDegrees,
                                       anglePrev,
                                       angleNext,
                                       StronaElementu,
                                       stonaOstanioDodanegoElementu,
                                       i,
                                       next, prev);

                        List<XPoint> getEndT3;

                        var _anglePrev = anglePrev;

                        if (i == vertexCount - 1)
                        {
                            _anglePrev = firstangleDegrees;
                        }

                        getEndT3 = GetEndT3Triangle(
                            inner[next],
                            outer[next],
                            outer,
                            angleDegrees,
                            _anglePrev,
                            angleNext,
                            StronaElementu,
                            stonaOstanioDodanegoElementu,
                            i, next, prev);

                        wierzcholki = new List<XPoint>
                            {
                                getStartT3[1],
                                getEndT3[1],
                                getEndT3[0],
                                getStartT3[0]
                            };
                    }
                    else
                    {
                        List<XPoint> getStartT3 = GetStartT3(
                            inner[i],
                            outer[i],
                            outer,
                            angleDegrees,
                            anglePrev,
                            angleNext,
                            StronaElementu,
                            stonaOstanioDodanegoElementu,
                            vertexCount < 6 ? -1 : i);

                        List<XPoint> getEndT3;

                        var _anglePrev = anglePrev;

                        if (i == vertexCount - 1)
                        {
                            _anglePrev = firstangleDegrees;
                        }

                        getEndT3 = GetEndT3(
                            inner[next],
                            outer[next],
                            outer,
                            angleDegrees,
                            _anglePrev,
                            angleNext,
                            StronaElementu,
                            stonaOstanioDodanegoElementu,
                            vertexCount < 6 ? -1 : i);

                        wierzcholki = new List<XPoint>
                            {
                                getStartT3[1],
                                getEndT3[1],
                                getEndT3[0],
                                getStartT3[0]
                            };
                    }
                }
                else if (leftJoin == "T2" && rightJoin == "T2")
                {

                    List<XPoint> getStartT2 = GetStartT2(inner[i], outer[i]);
                    List<XPoint> getEndT2 = GetEndT2(inner[next], outer[next]);

                    wierzcholki = new List<XPoint> {
                            getStartT2[1], getEndT2[1], getEndT2[0], getStartT2[0]
                        };


                }
                else if (leftJoin == "T5" && rightJoin == "T5")
                {
                    Console.WriteLine($"🔷 T5-T5 case for element {i + 1}. isAlmostHorizontal:{isAlmostHorizontal}, isAlmostVertical:{isAlmostVertical}, daneKwadratu.Count:{daneKwadratu.Count}");

                    double? SzerokoscSlupka = 0;
                    float PionOsSymetrii = 0;

                    if (daneKwadratu != null && daneKwadratu.Count > 0)
                    {
                        var szerSlupka = KonfiguracjeSystemu.FirstOrDefault(x => x.RowId == rowIdprofileLeft); // w słupku stałym rowIdprofileLeft to samo jest we wszystkich pozycjach!!!!

                        if (szerSlupka != null)
                        {
                            PionOsSymetrii = (float)Math.Abs((float)szerSlupka.PionOsSymetrii);
                            SzerokoscSlupka = szerSlupka.PionPrawa - szerSlupka.PionLewa;
                        }
                    }

                    // Najpierw oblicz wektor kierunkowy linii
                    XPoint TopXT5 = new XPoint { X = inner[0].X, Y = inner[0].Y };
                    XPoint BottomXT5 = new XPoint { X = inner[1].X, Y = inner[1].Y };

                    XPoint tmpTopST5 = new XPoint { };
                    XPoint tmpTopLT5 = new XPoint { };
                    XPoint tmpTopRT5 = new XPoint { };

                    // Najpierw oblicz wektor kierunkowy linii
                    double dxT5 = BottomXT5.X - TopXT5.X;
                    double dyT5 = BottomXT5.Y - TopXT5.Y;

                    // Oblicz długość odcinka
                    double dlugosc = Math.Sqrt(dxT5 * dxT5 + dyT5 * dyT5);

                    // Zabezpieczenie przed dzieleniem przez zero
                    if (dlugosc < 0.001)
                    {
                        // Odcinek jest punktem - użyj TopXT5 jako punktu środkowego
                        tmpTopST5 = new XPoint { X = TopXT5.X, Y = TopXT5.Y };
                        tmpTopLT5 = new XPoint { X = TopXT5.X, Y = TopXT5.Y };
                        tmpTopRT5 = new XPoint { X = TopXT5.X, Y = TopXT5.Y };
                        return false; // Dodaj return, bo dalsze obliczenia nie mają sensu
                    }

                    // Wektor jednostkowy wzdłuż linii
                    double uxT5 = dxT5 / dlugosc;
                    double uyT5 = dyT5 / dlugosc;

                    // Wektor prostopadły (obrócony o 90 stopni)
                    // Dla linii pionowej: u = (0, 1), v = (-1, 0) czyli w lewo
                    double vxT5 = -uyT5; // Wektor prostopadły w lewo
                    double vyT5 = uxT5;

                    // Znajdź parametr t (0-1) dla punktu na linii najbliższego kliknięciu myszy
                    double txT5 = mouseClik.X - TopXT5.X;
                    double tyT5 = mouseClik.Y - TopXT5.Y;

                    // Parametr t
                    double t = (txT5 * uxT5 + tyT5 * uyT5) / dlugosc;

                    // Ogranicz t do zakresu [0, 1]
                    t = Math.Max(0, Math.Min(1, t));

                    // Punkt osi symetrii na linii (najbliższy kliknięciu myszy)
                    tmpTopST5 = new XPoint
                    {
                        X = TopXT5.X + uxT5 * (t * dlugosc),
                        Y = TopXT5.Y + uyT5 * (t * dlugosc)
                    };

                    // Oblicz połowę szerokości słupka
                    double polowaSzerokosci = SzerokoscSlupka.HasValue ? SzerokoscSlupka.Value / 2.0 : 0;

                    // Prawidłowe przypisanie punktów lewy/prawy:
                    // - Punkt LEWY (LT5) to punkt przesunięty w lewo od osi (przeciwnie do wektora prostopadłego)
                    // - Punkt PRAWY (RT5) to punkt przesunięty w prawo od osi (zgodnie z wektorem prostopadłym)
                    tmpTopLT5 = new XPoint
                    {
                        X = tmpTopST5.X - vxT5 * polowaSzerokosci,
                        Y = tmpTopST5.Y - vyT5 * polowaSzerokosci
                    };

                    tmpTopRT5 = new XPoint
                    {
                        X = tmpTopST5.X + vxT5 * polowaSzerokosci,
                        Y = tmpTopST5.Y + vyT5 * polowaSzerokosci
                    };

                    // Teraz znajdź przecięcia z konturem
                    XPoint leftTopIntersection = FindFirstEdgeIntersectionByVector(tmpTopLT5, TopXT5, BottomXT5, punktyRegionuMaster, forward: false);
                    XPoint midTopIntersection = FindFirstEdgeIntersectionByVector(tmpTopST5, TopXT5, BottomXT5, punktyRegionuMaster, forward: false);
                    XPoint rightTopIntersection = FindFirstEdgeIntersectionByVector(tmpTopRT5, TopXT5, BottomXT5, punktyRegionuMaster, forward: false);

                    XPoint leftBottomIntersection = FindFirstEdgeIntersectionByVector(tmpTopLT5, TopXT5, BottomXT5, punktyRegionuMaster, forward: true);
                    XPoint midBottomIntersection = FindFirstEdgeIntersectionByVector(tmpTopST5, TopXT5, BottomXT5, punktyRegionuMaster, forward: true);
                    XPoint rightBottomIntersection = FindFirstEdgeIntersectionByVector(tmpTopRT5, TopXT5, BottomXT5, punktyRegionuMaster, forward: true);

                    // Prawidłowe przypisanie nazw (poprawione!)
                    var TopLT5 = leftTopIntersection;      // Lewy górny
                    var TopST5 = midTopIntersection;       // Środkowy górny
                    var TopRT5 = rightTopIntersection;     // Prawy górny

                    var BottomLT5 = leftBottomIntersection;    // Lewy dolny
                    var BottomSTT5 = midBottomIntersection;    // Środkowy dolny
                    var BottomRT5 = rightBottomIntersection;   // Prawy dolny

                    //Console.WriteLine($"🔷 🔷🔷 T5-T5 TopXT5.X/Y: {TopXT5.X}/{TopXT5.Y}");
                    //Console.WriteLine($"🔷 🔷🔷 T5-T5 BottomXT5.X/Y: {BottomXT5.X}/{BottomXT5.Y}");
                    //Console.WriteLine($"🔷 🔷🔷 T5-T5 tmpTopST5.X/Y: {tmpTopST5.X}/{tmpTopST5.Y}");
                    //Console.WriteLine($"🔷 🔷🔷 T5-T5 tmpTopLT5.X/Y: {tmpTopLT5.X}/{tmpTopLT5.Y}");
                    //Console.WriteLine($"🔷 🔷🔷 T5-T5 tmpTopRT5.X/Y: {tmpTopRT5.X}/{tmpTopRT5.Y}");
                    //Console.WriteLine($"🔷 🔷🔷 T5-T5 midTopIntersection.X/Y: {midTopIntersection.X}/{midTopIntersection.Y}");
                    //Console.WriteLine($"🔷 🔷🔷 T5-T5 midBottomIntersection.X/Y: {midBottomIntersection.X}/{midBottomIntersection.Y}");
                    // Kontur słupka jest czworokątem. Punkty osi (TopST5 /
                    // BottomSTT5) służą wyłącznie do wyznaczenia położenia
                    // słupka; nie są punktami jego obwiedni. Włączenie ich do
                    // listy powodowało, że po obrocie Build4SegmentContour
                    // pobierał połowę krawędzi i tworzył przekątną.
                    //
                    // Boki TopRT5->BottomRT5 i BottomLT5->TopLT5 są zawsze
                    // równoległe, bo powstają przez przesunięcie tej samej osi
                    // o połowę szerokości słupka.
                    wierzcholki = new List<XPoint>
                    {
                        TopRT5,
                        TopLT5,
                        BottomLT5,
                        BottomRT5
                    };

                    Console.WriteLine($"🔷 T5-T5 -> czworokąt słupka: {wierzcholki.Count} punktów");
                }
                else if (leftJoin == "T2" && rightJoin == "T1")
                {
                    Console.WriteLine($"🔷 T2/T1 element {i + 1} - kombinacja ścięcia (T2) z czopem (T1)");

                    if (vertexCount == 3)
                    {
                        List<XPoint> getStartT2 = GetStartT2(inner[i], outer[i]);
                        List<XPoint> getEndT2 = GetEndT2(inner[next], outer[next]);

                        List<XPoint> getStartT1 = GetStartT1(inner[i], outer[i], outer, angleDegrees, anglePrev, angleNext,
                            StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);

                        List<XPoint> getEndT1;

                        var _anglePrev = anglePrev;
                        if (i == vertexCount - 1)
                        {
                            _anglePrev = firstangleDegrees;
                        }

                        int nextTriangle = (i + 1) % vertexCount;

                        getEndT1 = GetEndT1Triangle(
                            inner[nextTriangle],
                            outer[nextTriangle],
                            outer,
                            angleDegrees,
                            anglePrev,
                            angleNext,
                            StronaElementu,
                            stonaOstanioDodanegoElementu,
                             i, next, prev);

                        wierzcholki = new List<XPoint> {
                            getStartT2[1], getEndT2[1], getEndT1[0], getStartT2[0]
                        };
                    }
                    else
                    {
                        List<XPoint> getStartT2 = GetStartT2(inner[i], outer[i]);
                        List<XPoint> getEndT2 = GetEndT2(inner[next], outer[next]);

                        List<XPoint> getStartT1 = GetStartT1(inner[i], outer[i], outer, angleDegrees, anglePrev, angleNext,
                            StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);
                        List<XPoint> getEndT1;

                        var _anglePrev = anglePrev;
                        if (i == vertexCount - 1)
                        {
                            _anglePrev = firstangleDegrees;
                        }
                        getEndT1 = GetEndT1(inner[next], outer[next], outer, angleDegrees, _anglePrev, angleNext, StronaElementu,
                            stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);

                        wierzcholki = new List<XPoint> {
                            getStartT2[1], getEndT2[1], getEndT1[0], getStartT2[0]
                        };
                    }

                }
                else if (leftJoin == "T1" && rightJoin == "T2")
                {
                    Console.WriteLine($"🔷 T1/T2 element {i + 1} START isAlmostHorizontal: {isAlmostHorizontal} isAlmostVertical: {isAlmostVertical} vertexCount: {vertexCount} angleDegrees: {angleDegrees} firstangleDegrees: {firstangleDegrees} angleDegreesStronaA:{angleDegreesStronaA:F1}° angleDegreesStronaB:{angleDegreesStronaB:F1}° anglePrev: {anglePrev:F1}°");

                    if (vertexCount == 3)
                    {
                        List<XPoint> getStartT2 = GetStartT2(inner[i], outer[i]);
                        List<XPoint> getEndT2 = GetEndT2(inner[next], outer[next]);

                        List<XPoint> getStartT1 = GetStartT1Triangle(
                            inner[i],
                            outer[i],
                            outer,
                            angleDegrees,
                            anglePrev,
                            angleNext,
                            StronaElementu,
                            stonaOstanioDodanegoElementu,
                            i, next, prev);

                        wierzcholki = new List<XPoint> {
                            getStartT1[1], getEndT2[1], getEndT2[0], getStartT1[0]
                        };
                    }
                    else
                    {
                        List<XPoint> getStartT2 = GetStartT2(inner[i], outer[i]);
                        List<XPoint> getEndT2 = GetEndT2(inner[next], outer[next]);

                        List<XPoint> getStartT1 = GetStartT1(inner[i], outer[i], outer, angleDegrees, anglePrev, angleNext,
                         StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);

                        wierzcholki = new List<XPoint> {
                            getStartT1[1], getEndT2[1], getEndT2[0], getStartT1[0]
                        };
                    }

                }
                else if (leftJoin == "T3" && rightJoin == "T2")
                {
                    Console.WriteLine($"🔷 T3/T2 element {i + 1} - kombinacja pełnego profilu (T3) ze ścięciem (T2)");

                    List<XPoint> getStartT3 = GetStartT3(inner[i], outer[i], outer, angleDegrees, anglePrev, angleNext,
                        StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 || anglePrev == 270 ? -1 : i);
                    List<XPoint> getEndT3;

                    var _anglePrev = anglePrev;
                    if (i == vertexCount - 1)
                    {
                        _anglePrev = firstangleDegrees;
                    }

                    getEndT3 = GetEndT3(inner[next], outer[next], outer, angleDegrees, _anglePrev, angleNext,
                        StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);

                    List<XPoint> getStartT2 = GetStartT2(inner[i], outer[i]);
                    List<XPoint> getEndT2 = GetEndT2(inner[next], outer[next]);

                    wierzcholki = new List<XPoint> {
                            getStartT3[1], getEndT2[1], getEndT2[0], getStartT3[0]
                    };


                }
                else if (leftJoin == "T2" && rightJoin == "T3")
                {
                    Console.WriteLine($"🔷 T2/T3 element {i + 1} - kombinacja ścięcia (T2) z pełnym profilem (T3)");

                    List<XPoint> getStartT3 = GetStartT3(inner[i], outer[i], outer, angleDegrees, anglePrev, angleNext,
                        StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);
                    List<XPoint> getEndT3;
                    var _anglePrev = anglePrev;
                    if (i == vertexCount - 1)
                    {
                        _anglePrev = firstangleDegrees;
                    }
                    getEndT3 = GetEndT3(inner[next], outer[next], outer, angleDegrees, _anglePrev, angleNext,
                        StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);

                    List<XPoint> getStartT2 = GetStartT2(inner[i], outer[i]);
                    List<XPoint> getEndT2 = GetEndT2(inner[next], outer[next]);

                    wierzcholki = new List<XPoint> {
                            getStartT2[1], getEndT3[1], getEndT3[0], getStartT2[0]
                    };


                }
                else if (leftJoin == "T3" && rightJoin == "T1")
                {
                    List<XPoint> getStartT1 = GetStartT1(inner[i], outer[i], outer, angleDegrees, anglePrev, angleNext,
                    StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);
                    List<XPoint> getEndT1;
                    var _anglePrev = anglePrev;
                    if (i == vertexCount - 1)
                    {
                        _anglePrev = firstangleDegrees;
                    }

                    if (vertexCount == 3)
                    {
                        int nextTriangle = (i + 1) % vertexCount;

                        getEndT1 = GetEndT1Triangle(
                            inner[nextTriangle],
                            outer[nextTriangle],
                            outer,
                            angleDegrees,
                            anglePrev,
                            angleNext,
                            StronaElementu,
                            stonaOstanioDodanegoElementu,
                            i, next, prev);
                    }
                    else
                    {
                        getEndT1 = GetEndT1(inner[next], outer[next], outer, angleDegrees, _anglePrev, angleNext, StronaElementu,
                        stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);
                    }


                    List<XPoint> getStartT3 = GetStartT3(inner[i], outer[i], outer, angleDegrees, anglePrev, angleNext,
                     StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);
                    List<XPoint> getEndT3;

                    getEndT3 = GetEndT3(inner[next], outer[next], outer, angleDegrees, _anglePrev, angleNext,
                        StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);

                    wierzcholki = new List<XPoint> {
                            getStartT3[1], getEndT1[1], getEndT1[0], getStartT3[0]
                        };

                }
                else if (leftJoin == "T4" && rightJoin == "T3")
                {
                    Console.WriteLine($"🔷 T4/T3 element {i + 1} - kombinacja wcięcia (T4) z pełnym profilem (T3)");
                    List<XPoint> getStartT3 = GetStartT3(inner[i], outer[i], outer, angleDegrees, anglePrev, angleNext,
                        StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);
                    List<XPoint> getEndT3;
                    var _anglePrev = anglePrev;
                    if (i == vertexCount - 1)
                    {
                        _anglePrev = firstangleDegrees;
                    }
                    getEndT3 = GetEndT3(inner[next], outer[next], outer, angleDegrees, _anglePrev, angleNext,
                        StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);

                    List<XPoint> getStartT4 = GetStartT4(inner[i]);
                    List<XPoint> getEndT4 = GetEndT4(inner[next]);

                    var _getStartT3 = FindFirstEdgeIntersectionByAngle(inner[i], anglePrev, outer);

                    wierzcholki = new List<XPoint> {
                            _getStartT3, getEndT3[1], getEndT3[0], getStartT4[0]
                        };


                }
                else if (leftJoin == "T3" && rightJoin == "T4")
                {
                    Console.WriteLine($"🔷 T3/T4 element {i + 1} - kombinacja pełnego profilu (T3) z wcięciem (T4)");

                    List<XPoint> getStartT3 = GetStartT3(inner[i], outer[i], outer, angleDegrees, anglePrev, angleNext,
                        StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 && vertexCount > 3 ? -1 : i);

                    List<XPoint> getEndT3;

                    var _anglePrev = anglePrev;

                    if (i == vertexCount - 1)
                    {
                        _anglePrev = firstangleDegrees;
                    }

                    getEndT3 = GetEndT3(inner[next], outer[next], outer, angleDegrees, _anglePrev, angleNext,
                        StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);

                    List<XPoint> getStartT4 = GetStartT4(inner[i]);
                    List<XPoint> getEndT4 = GetEndT4(inner[next]);

                    var _getStartT4 = FindFirstEdgeIntersectionByAngle(getEndT4[0], angleNext - 180, outer);

                    wierzcholki = new List<XPoint> {
                            getStartT3[1], _getStartT4, getEndT4[0], getStartT3[0]
                        };


                }
                else
                {
                    Console.WriteLine($"🔷 Wartość domyślna T2/T2 {i + 1} połączenia: {leftJoin}-{rightJoin}");

                    List<XPoint> getStartT2 = GetStartT2(inner[i], outer[i]);
                    List<XPoint> getEndT2 = GetEndT2(inner[next], outer[next]);

                    wierzcholki = new List<XPoint> {
                            getStartT2[1], getEndT2[1], getEndT2[0], getStartT2[0]
                        };

                }

                wierzcholki = RotateContourSegments(wierzcholki, Corner.BottomLeft, clockwise: true);

                // Budujemy pełny kontur 4-segmentowy
                wierzcholkiZLukami = Build4SegmentContour(wierzcholki, outerContourSegment, innerContourSegment, i + 1, StronaElementu, wierzcholki, leftJoin, rightJoin, angleDegrees, angleNext, anglePrev);

                double regionMinX = wierzcholki.Min(p => p.X);
                double regionMaxX = wierzcholki.Max(p => p.X);
                double regionMinY = wierzcholki.Min(p => p.Y);
                double regionMaxY = wierzcholki.Max(p => p.Y);

                int wartoscX = (int)Math.Round(regionMaxX - regionMinX);
                int wartoscY = (int)Math.Round(regionMaxY - regionMinY);

                // Console.WriteLine($"leftJoin: {leftJoin} rightJoin:{rightJoin} wierzcholki: {wierzcholki.Count()} isAlmostVertical:{isAlmostVertical}");
                float bazowaDlugosc = DlugoscElementu(wierzcholki);

                //Console.WriteLine($"▶️ Element Start switch {i + 1}/{vertexCount}: Length: {length}, StronaElementu :{StronaElementu}, angleDegreesElementLionowy:{angleDegreesElementLionowy}, Angle: {angleDegrees}°, Profile: {profile}, Wierzchołki: {wierzcholki.Count}, BazowaDlugosc: {bazowaDlugosc}, wartoscX: {wartoscX}, wartoscY: {wartoscY} ElementLiniowy:{ElementLiniowy} wierzcholki X0: {wierzcholki[0].X} Y0: {wierzcholki[0].Y}");

                Guid rowIdProfil;
                string nazwaElemntu;
                string indeksElementu;

                switch (StronaElementu)
                {
                    case "Lewa":
                        rowIdProfil = rowIdprofileLeft;
                        nazwaElemntu = rowNazwaprofileLeft;
                        indeksElementu = rowIndeksprofileLeft;
                        break;
                    case "Prawa":
                        rowIdProfil = rowIdprofileRight;
                        nazwaElemntu = rowNazwaprofileRight;
                        indeksElementu = rowIndeksprofileRight;
                        break;
                    case "Góra":
                        rowIdProfil = rowIdprofileTop;
                        nazwaElemntu = rowNazwaprofileTop;
                        indeksElementu = rowIndeksprofileTop;
                        break;
                    case "Dół":
                        rowIdProfil = rowIdprofileBottom;
                        nazwaElemntu = rowNazwaprofileBottom;
                        indeksElementu = rowIndeksprofileBottom;
                        break;
                    default:
                        rowIdProfil = rowIdprofileLeft;
                        nazwaElemntu = rowNazwaprofileLeft;
                        indeksElementu = rowIndeksprofileLeft;
                        break;
                }

                if (angleDegreesElementLionowy != angleDegrees && ElementLiniowy) break;

                if (rowIdprofileLeft != Guid.Empty)
                    ElementyRamyRysowane.Add(new KsztaltElementu
                    {
                        NrPozWModelu = i + 1,
                        TypKsztaltu = typKsztalt,
                        Wierzcholki = wierzcholki,
                        WierzcholkiZLukami = wierzcholkiZLukami,
                        WypelnienieZewnetrzne = "wood-pattern",
                        WypelnienieWewnetrzne = KolorSzyby,
                        Grupa = NazwaObiektu + $" {StronaElementu}-{i + 1} {wartoscX}/{wartoscY}",
                        Typ = TypObiektu,
                        ZIndex = Zindeks,
                        RowIdElementu = rowIdProfil,
                        IdRegion = regionId,
                        Kat = (float)angleDegrees,
                        KatStronaA = (float)angleDegreesStronaA,
                        KatStronaB = (float)angleDegreesStronaB,
                        OffsetLewa = StronaElementu == "Lewa" ? profileLeft : 0,
                        OffsetPrawa = StronaElementu == "Prawa" ? profileRight : 0,
                        OffsetDol = StronaElementu == "Dól" ? profileBottom : 0,
                        OffsetGora = StronaElementu == "Góra" ? profileTop : 0,
                        Strona = StronaElementu,
                        IndeksElementu = indeksElementu,
                        NazwaElementu = nazwaElemntu,
                        DlogoscElementu = bazowaDlugosc + ((dodajA ? profileA : 0) + (dodajB ? profileB : 0)),
                        DlogoscWidocznaElementu = bazowaDlugosc,
                        DlugoscCzopaA = dodajA ? profileA : -1,
                        DlugoscCzopaB = dodajB ? profileB : -1,
                        RodzajpolaczenAiB = $"{leftJoin}/{rightJoin}",
                        PolaczenieStronaA = leftJoin,
                        PolaczenieStronaB = rightJoin
                    });

                stonaOstanioDodanegoElementu = StronaElementu;

                // Console.WriteLine($"▶️▶️▶️▶️ Element {i + 1}/{vertexCount} dodałem do ElementyRamyRysowane. Total elements now: {ElementyRamyRysowane.Count} - > rowIdProfil:{rowIdProfil} Angle: {angleDegrees}° leftJoin:{leftJoin} rightJoin:{rightJoin}");

                if (ElementLiniowy) return true;

            }

            await Task.CompletedTask;

            return true;
        }

        /// <summary>
        /// Obraca listę segmentów tak, aby pierwszy segment rozpoczynał się
        /// w wybranym narożniku.
        /// Nie zmienia geometrii ani współrzędnych.
        /// </summary>
        private List<XPoint> RotateContourSegments(
            List<XPoint> points,
            Corner startCorner = Corner.TopLeft,
            bool clockwise = true)
        {
            if (points == null || points.Count < 2)
                return points;

            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            XPoint startPoint = startCorner switch
            {
                Corner.TopLeft =>
                    points.OrderBy(p => Distance(p, new XPoint(minX, minY))).First(),

                Corner.TopRight =>
                    points.OrderBy(p => Distance(p, new XPoint(maxX, minY))).First(),

                Corner.BottomRight =>
                    points.OrderBy(p => Distance(p, new XPoint(maxX, maxY))).First(),

                Corner.BottomLeft =>
                    points.OrderBy(p => Distance(p, new XPoint(minX, maxY))).First(),

                _ =>
                    points.OrderBy(p => Distance(p, new XPoint(minX, minY))).First()
            };

            int startIndex = points.FindIndex(p => Distance(p, startPoint) < 0.001);

            if (startIndex < 0)
                return points;

            var rotated = new List<XPoint>(points.Count);

            for (int i = 0; i < points.Count; i++)
            {
                rotated.Add(points[(startIndex + i) % points.Count]);
            }

            // Jeśli chcemy przeciwny kierunek obiegu
            if (!clockwise)
            {
                var first = rotated[0];

                rotated = rotated
                    .Skip(1)
                    .Reverse()
                    .ToList();

                rotated.Insert(0, first);
            }

            return rotated;
        }

        /// <summary>
        /// Enum reprezentujący narożniki prostokąta
        /// </summary>
        public enum Corner
        {
            TopLeft,
            TopRight,
            BottomRight,
            BottomLeft
        }



        public List<ContourSegment> Build4SegmentContour(
                List<XPoint> wierzcholki,
                List<ContourSegment> outerContour,
                List<ContourSegment> innerContour,
                int numerElemntu,
                string _stronaElementu,
                List<XPoint> wierzcholkiLinieProste,
                string leftJoin,
                string rightJoin,
                double angleDegrees,
                double nextangleDegrees,
                double prevangleDegrees)
        {
            // ============================================================
            // Dla każdego segmentu wybieramy odpowiednie fragmenty konturów
            // ============================================================

            var filteredOuter = GetSegmentsForSide(outerContour, _stronaElementu);
            var filteredInner = GetSegmentsForSide(innerContour, _stronaElementu);

            Console.WriteLine($"\n🔷 Build4SegmentContour ELEMENT {numerElemntu} _stronaElementu: {_stronaElementu} - START");
            Console.WriteLine($"🔷 Build4SegmentContour    Strona: {_stronaElementu}, filteredOuter -> segmenty: {filteredOuter?.Count ?? 0}, łuki: {filteredOuter?.Count(s => s.Type == SegmentType.Arc) ?? 0}");
            Console.WriteLine($"🔷 Build4SegmentContour    Wierzchołki: [0]({wierzcholki[0].X:F2},{wierzcholki[0].Y:F2}) [1]({wierzcholki[1].X:F2},{wierzcholki[1].Y:F2}) [2]({wierzcholki[2].X:F2},{wierzcholki[2].Y:F2}) [3]({wierzcholki[3].X:F2},{wierzcholki[3].Y:F2})");

            int sourceIndex = numerElemntu - 1;
            int previousIndex = (sourceIndex - 1 + outerContour.Count) % outerContour.Count;
            int nextIndex = (sourceIndex + 1) % outerContour.Count;

            // ============================================================
            // PRZYPADEK 1: T1/T3 dla strony Góra oraz obsługa pionowa (Lewa/Prawa)
            // ============================================================
            if (outerContour != null && innerContour != null && sourceIndex >= 0)
            {
                var outerSegment = outerContour[sourceIndex];
                var innerSegment = innerContour[sourceIndex];
                var outerSegmentLeft = outerContour[previousIndex];
                var outerSegmentRight = outerContour[nextIndex];

                // -------------------------------
                // 1a. Pionowe boki (Lewa / Prawa) – T1, T3
                // -------------------------------
                bool rightJoinIsT1orT3 = (rightJoin == "T1" || rightJoin == "T3");
                bool leftJoinIsT1orT3 = (leftJoin == "T1" || leftJoin == "T3");

                if (_stronaElementu == "Lewa" && rightJoinIsT1orT3)
                {
                    var adjustedVerticesX = new List<XPoint>(wierzcholki);

                    if (rightJoin == "T1" && outerContour.Count() > 3)
                        adjustedVerticesX[2] = FindIntersectionWithContourByAngle(adjustedVerticesX[2], angleDegrees, innerContour);

                    if (rightJoin == "T3" && outerContour.Count() > 3)
                        adjustedVerticesX[2] = FindIntersectionWithContourByAngle(adjustedVerticesX[2], angleDegrees, outerContour);

                    // Segmenty zewnętrzny i wewnętrzny – już poprawne
                    var segZewnetrznyX = BuildSegmentWithArc(adjustedVerticesX[0], adjustedVerticesX[1], filteredOuter);
                    var segWewnetrznyX = BuildSegmentWithArc(adjustedVerticesX[2], adjustedVerticesX[3], filteredInner);

                    // Dla wewnętrznego łuku odwracamy kierunek, jeśli jest łukiem
                    if (segWewnetrznyX.Type == SegmentType.Arc && segWewnetrznyX.Center.HasValue)
                    {
                        segWewnetrznyX = new ContourSegment(
                            segWewnetrznyX.End,
                            segWewnetrznyX.Start,
                            segWewnetrznyX.Center,
                            segWewnetrznyX.Radius,
                            !segWewnetrznyX.CounterClockwise
                        );
                    }

                    // ---------- Dla T3 używamy outerContour, dla T1 innerContour ----------
                    List<ContourSegment> contourForSide = (rightJoin == "T3") ? filteredOuter : filteredInner;

                    // Segment boczny 1 (prawy) – od adjustedVerticesX[1] do adjustedVerticesX[2]
                    var segBoczny1 = BuildSegmentWithArc(adjustedVerticesX[1], adjustedVerticesX[2], contourForSide);

                    // Segment boczny 2 (lewy) – od adjustedVerticesX[3] do adjustedVerticesX[0]
                    var segBoczny2 = BuildSegmentWithArc(adjustedVerticesX[3], adjustedVerticesX[0], contourForSide);

                    Console.WriteLine($"\n🔷 Build4SegmentContour ELEMENT {numerElemntu} _stronaElementu: {_stronaElementu} - RETURN 1");

                    return new List<ContourSegment>
                        {
                            segZewnetrznyX,
                            segBoczny1,
                            segWewnetrznyX,
                            segBoczny2
                        };
                }

                // Analogicznie dla prawej strony
                if (_stronaElementu == "Prawa" && leftJoinIsT1orT3)
                {
                    var adjustedVerticesX = new List<XPoint>(wierzcholki);

                    if (leftJoin == "T1")
                        adjustedVerticesX[1] = FindIntersectionWithContourByAngle(adjustedVerticesX[1], angleDegrees - 180, innerContour);

                    if (leftJoin == "T3")
                        adjustedVerticesX[1] = FindIntersectionWithContourByAngle(adjustedVerticesX[1], angleDegrees - 180, outerContour);


                    var segZewnetrznyX = BuildSegmentWithArc(adjustedVerticesX[0], adjustedVerticesX[1], filteredOuter);
                    var segWewnetrznyX = BuildSegmentWithArc(adjustedVerticesX[2], adjustedVerticesX[3], filteredInner);

                    if (segWewnetrznyX.Type == SegmentType.Arc && segWewnetrznyX.Center.HasValue)
                    {
                        segWewnetrznyX = new ContourSegment(
                            segWewnetrznyX.End,
                            segWewnetrznyX.Start,
                            segWewnetrznyX.Center,
                            segWewnetrznyX.Radius,
                            !segWewnetrznyX.CounterClockwise
                        );
                    }

                    if (rightJoin == "T2")
                    {

                        segZewnetrznyX = BuildSegmentWithArc(wierzcholki[0], wierzcholki[1], innerContour);
                        segWewnetrznyX = BuildSegmentWithArc(wierzcholki[2], wierzcholki[3], innerContour);

                        return new List<ContourSegment>
                            {
                                segZewnetrznyX,
                                new ContourSegment(wierzcholki[1], wierzcholki[2]),
                                segWewnetrznyX,
                                new ContourSegment(wierzcholki[3], wierzcholki[0])
                            };

                    }
                    else
                    {

                        List<ContourSegment> contourForSide = (leftJoin == "T3") ? filteredOuter : filteredInner;

                        var segBoczny1 = BuildSegmentWithArc(adjustedVerticesX[1], adjustedVerticesX[2], contourForSide);
                        var segBoczny2 = BuildSegmentWithArc(adjustedVerticesX[3], adjustedVerticesX[0], contourForSide);

                        Console.WriteLine($"\n🔷 Build4SegmentContour ELEMENT {numerElemntu} _stronaElementu: {_stronaElementu} - RETURN 3");

                        return new List<ContourSegment>
                        {
                            segZewnetrznyX,
                            segBoczny1,
                            segWewnetrznyX,
                            segBoczny2
                        };
                    }

                }


                // -------------------------------
                // 1b. Górne boki – gdy outerSegment i innerSegment są łukami
                // -------------------------------
                if (outerSegment.Type == SegmentType.Arc && outerSegment.Center.HasValue &&
                    innerSegment.Type == SegmentType.Arc && innerSegment.Center.HasValue)
                {
                    bool leftT1Bevel = _stronaElementu == "Góra" && leftJoin == "T1" &&
                        outerContour[previousIndex].Type == SegmentType.Line;
                    bool rightT1Bevel = _stronaElementu == "Góra" && rightJoin == "T1" &&
                        outerContour[nextIndex].Type == SegmentType.Line;

                    bool leftT2Bevel = (_stronaElementu == "Góra") && leftJoin == "T2" &&
                        outerContour[previousIndex].Type == SegmentType.Line;
                    bool rightT2Bevel = (_stronaElementu == "Góra") && rightJoin == "T2" &&
                        outerContour[nextIndex].Type == SegmentType.Line;

                    bool leftT3Bevel = (_stronaElementu == "Góra") && leftJoin == "T3" &&
                        outerContour[previousIndex].Type == SegmentType.Line;
                    bool rightT3Bevel = (_stronaElementu == "Góra") && rightJoin == "T3" &&
                        outerContour[nextIndex].Type == SegmentType.Line;


                    var result = new List<ContourSegment>();

                    int idWar = 0;
                    int idWarSC = 0;


                    // ---------- Prawa strona ----------
                    if (rightT1Bevel)
                    {
                        result = new List<ContourSegment>
                        {
                            new ContourSegment(outerSegment.Start, outerSegment.End,
                                outerSegment.Center, outerSegment.Radius, false)
                        };


                        XPoint bevel = GetT1BevelPoint(outerSegment.End, innerSegment.End, innerSegment, outerContour);
                        result.Add(new ContourSegment(outerSegment.End, bevel));
                        result.Add(new ContourSegment(bevel, innerSegment.End));
                        idWar += 1;
                    }
                    else if (rightT2Bevel)
                    {

                        result = new List<ContourSegment>
                        {
                            new ContourSegment(outerSegment.Start, outerSegment.End,
                                outerSegment.Center, outerSegment.Radius, false)
                        };

                        // T2: pozostaje bez zmian (szukamy wewnętrznego łuku)
                        var innerArc = FindArcBetweenPoints(innerContour, outerSegment.End, outerSegment.End, 0.1);
                        if (innerArc != null && innerArc.Center.HasValue)
                        {
                            XPoint innerPointOnArc = GetPointOnArcAtAngle(innerArc, outerSegment.End);
                            XPoint bevel = GetT3BevelPoint(outerSegment.End, innerPointOnArc, outerSegment, innerContour);
                            result.Add(new ContourSegment(outerSegment.End, bevel));
                            result.Add(new ContourSegment(bevel, innerPointOnArc));
                        }
                        else
                        {
                            result.Add(new ContourSegment(outerSegment.End, innerSegment.End));
                        }

                        idWar += 10;
                    }

                    // ---------- Prawa strona (rightT3Bevel) ----------
                    else if (rightT3Bevel)
                    {
                        var (pointOnOuter, segment) = FindIntersectionByAngleWithSegments(
                            innerSegment.End,
                            nextangleDegrees - 180,
                            outerContour,
                            1000.0
                        );

                        // Zewnętrzny łuk – zachowujemy oryginalny kierunek (CW)
                        var outerArc = BuildSegmentWithArc(outerSegment.Start, pointOnOuter, outerContour);

                        // Prawy bok – łączy pointOnOuter z innerSegment.End
                        var rightSide = BuildSegmentWithArc(pointOnOuter, innerSegment.End, outerContour);

                        // Wewnętrzny łuk – BEZ ODWRACANIA (zachowujemy kierunek z innerContour, czyli CCW)
                        var innerArc = BuildSegmentWithArc(innerSegment.End, innerSegment.Start, innerContour);

                        // Lewy bok
                        var leftSide = BuildSegmentWithArc(innerSegment.Start, outerSegment.Start, innerContour);

                        Console.WriteLine($"\n🔷 rightT3Bevel ELEMENT {numerElemntu} - RETURN");
                        return new List<ContourSegment>
                        {
                            outerArc,
                            rightSide,
                            innerArc,
                            leftSide
                        };
                    }

                    // ---------- Lewa strona (leftT3Bevel) ----------
                    if (leftT3Bevel)
                    {
                        var (pointOnOuter, segment) = FindIntersectionByAngleWithSegments(
                            innerSegment.Start,
                            prevangleDegrees,
                            outerContour,
                            1000.0
                        );

                        // Zewnętrzny łuk – od pointOnOuter do outerSegment.End
                        var outerArc = BuildSegmentWithArc(pointOnOuter, outerSegment.End, outerContour);

                        // Prawy bok
                        var rightSide = BuildSegmentWithArc(outerSegment.End, innerSegment.End, outerContour);

                        // Wewnętrzny łuk – BEZ ODWRACANIA
                        var innerArc = BuildSegmentWithArc(innerSegment.End, innerSegment.Start, innerContour);

                        // Lewy bok – od innerSegment.Start do pointOnOuter
                        var leftSide = BuildSegmentWithArc(innerSegment.Start, pointOnOuter, outerContour);

                        Console.WriteLine($"\n🔷 leftT3Bevel ELEMENT {numerElemntu} - RETURN");
                        return new List<ContourSegment>
                        {
                            outerArc,
                            rightSide,
                            innerArc,
                            leftSide
                        };
                    }
                    else if (!rightT1Bevel)
                    {
                        result = new List<ContourSegment>
                        {
                            new ContourSegment(outerSegment.Start, outerSegment.End,
                                outerSegment.Center, outerSegment.Radius, false)
                        };

                        result.Add(new ContourSegment(outerSegment.End, innerSegment.End));

                        idWarSC += 1000;
                    }

                    result.Add(new ContourSegment(innerSegment.End, innerSegment.Start,
                        innerSegment.Center, innerSegment.Radius, true));

                    // ---------- Lewa strona ----------
                    if (leftT1Bevel)
                    {
                        XPoint bevel = GetT1BevelPoint(outerSegment.Start, innerSegment.Start, innerSegment, outerContour);
                        result.Add(new ContourSegment(innerSegment.Start, bevel));
                        result.Add(new ContourSegment(bevel, outerSegment.Start));

                        idWar += 1000;
                    }
                    else if (leftT2Bevel)
                    {
                        // T2: pozostaje bez zmian
                        var innerArc = FindArcBetweenPoints(innerContour, outerSegment.Start, outerSegment.Start, 0.1);
                        if (innerArc != null && innerArc.Center.HasValue)
                        {
                            XPoint innerPointOnArc = GetPointOnArcAtAngle(innerArc, outerSegment.Start);
                            XPoint bevel = GetT3BevelPoint(outerSegment.Start, innerPointOnArc, outerSegment, innerContour);
                            result.Add(new ContourSegment(innerPointOnArc, bevel));
                            result.Add(new ContourSegment(bevel, outerSegment.Start));
                        }
                        else
                        {
                            result.Add(new ContourSegment(innerSegment.Start, outerSegment.Start));
                        }
                        idWar += 10000;
                    }
                    else if (leftT3Bevel)
                    {
                        // T3: przedłużamy lewy pionowy element do outerContour
                        var (pointOnOuter, segment) = FindIntersectionWithOuterContour(
                            outerSegment.Start,
                            innerSegment.Start,
                            outerContour,
                            true);

                        if (segment != null)
                        {
                            result.Add(new ContourSegment(innerSegment.Start, pointOnOuter));
                            result.Add(new ContourSegment(pointOnOuter, outerSegment.Start));
                        }
                        else if (Distance(pointOnOuter, outerSegment.Start) > 0.1 && pointOnOuter.Y < outerSegment.Start.Y)
                        {
                            result.Add(new ContourSegment(innerSegment.Start, pointOnOuter));
                            result.Add(new ContourSegment(pointOnOuter, outerSegment.Start));
                        }
                        else
                        {
                            result.Add(new ContourSegment(innerSegment.Start, outerSegment.Start));
                        }

                        idWar += 100000;
                    }
                    else
                    {
                        result.Add(new ContourSegment(innerSegment.Start, outerSegment.Start));
                    }

                    Console.WriteLine($"\n🔷 Build4SegmentContour ELEMENT {numerElemntu} /{idWar}+{idWarSC} _stronaElementu: {_stronaElementu} - RETURN 6");

                    return result;
                }


            }

            // ============================================================
            // PRZYPADEK 2: Standardowa ścieżka dla linii
            // ============================================================
            var adjustedVertices = new List<XPoint>(wierzcholki);

            // Wyjątek: gdy oba końce mają tylko linie – zwróć 4 segmenty liniowe
            if (CzyObaKonceWspolnyFragmentTylkoLinie(adjustedVertices, outerContour, innerContour, sourceIndex))
            {
                Console.WriteLine($"🟢 Build4SegmentContour ELEMENT PRZYPADEK 02 {numerElemntu}: oba końce mają wspólny fragment wyłącznie LINIOWY - zwracam 4 segmenty Line.");
                return new List<ContourSegment>
                    {
                        new ContourSegment(adjustedVertices[0], adjustedVertices[1]),
                        new ContourSegment(adjustedVertices[1], adjustedVertices[2]),
                        new ContourSegment(adjustedVertices[2], adjustedVertices[3]),
                        new ContourSegment(adjustedVertices[3], adjustedVertices[0])
                    };
            }

            if (outerContour != null && innerContour != null &&
                outerContour.Count == innerContour.Count &&
                sourceIndex >= 0 && sourceIndex < outerContour.Count &&
                outerContour[sourceIndex].Type == SegmentType.Line)
            {
                if (outerContour[previousIndex].Type == SegmentType.Arc &&
                    innerContour[previousIndex].Type == SegmentType.Arc)
                {
                    ReplaceNearestPoint(adjustedVertices, outerContour[previousIndex].End);
                    ReplaceNearestPoint(adjustedVertices, innerContour[previousIndex].End);
                }

                if (outerContour[nextIndex].Type == SegmentType.Arc &&
                    innerContour[nextIndex].Type == SegmentType.Arc)
                {
                    ReplaceNearestPoint(adjustedVertices, outerContour[nextIndex].Start);
                    ReplaceNearestPoint(adjustedVertices, innerContour[nextIndex].Start);
                }
            }

            RotateContourSegments(adjustedVertices, Corner.BottomLeft, clockwise: true);

            var segZewnetrzny = BuildSegmentWithArc(adjustedVertices[0], adjustedVertices[1], filteredOuter);
            var segWewnetrzny = BuildSegmentWithArc(adjustedVertices[2], adjustedVertices[3], filteredInner);

            // ============================================================
            // PRZYPADEK 2.4: T5 — słupek stały
            // ============================================================
            bool isT5 = leftJoin == "T5" || rightJoin == "T5";
            if (isT5 && adjustedVertices.Count == 4)
            {
                var contoursForT5 = new List<ContourSegment>(outerContour.Count + innerContour.Count);
                contoursForT5.AddRange(outerContour);
                contoursForT5.AddRange(innerContour);

                var firstEnd = BuildSegmentWithArc(adjustedVertices[0], adjustedVertices[1], contoursForT5);
                var secondEnd = BuildSegmentWithArc(adjustedVertices[2], adjustedVertices[3], contoursForT5);

                Console.WriteLine($"\n🔷 Build4SegmentContour ELEMENT {numerElemntu} _stronaElementu: {_stronaElementu} - RETURN 7");

                return new List<ContourSegment>
                    {
                        firstEnd,
                        new ContourSegment(adjustedVertices[1], adjustedVertices[2]),
                        secondEnd,
                        new ContourSegment(adjustedVertices[3], adjustedVertices[0])
                    };
            }

            // ============================================================
            // PRZYPADEK 2.5: T3 dla skrzydła (strony Lewa/Prawa)
            // ============================================================
            bool isSkrzydloPion = (_stronaElementu == "Lewa" || _stronaElementu == "Prawa") &&
                                  (leftJoin == "T3" || rightJoin == "T3");

            if (isSkrzydloPion)
            {

                var result = new List<ContourSegment>();

                result.Add(new ContourSegment(adjustedVertices[0], adjustedVertices[1]));

                if (leftJoin == "T3" && _stronaElementu == "Prawa")
                {
                    var (intersection, segment) = FindIntersectionWithOuterContour(
                        adjustedVertices[0], adjustedVertices[1], outerContour, true);

                    if (segment != null)
                    {
                        result.Add(new ContourSegment(intersection, adjustedVertices[2],
                            segment.Center, segment.Radius, segment.CounterClockwise));
                    }
                    else if (Distance(intersection, adjustedVertices[1]) > 0.1)
                    {
                        result.Add(new ContourSegment(adjustedVertices[1], adjustedVertices[2]));
                    }
                    else
                    {
                        result.Add(new ContourSegment(adjustedVertices[1], adjustedVertices[2]));
                    }
                }
                else if (rightJoin == "T3" && _stronaElementu == "Lewa")
                {
                    var (intersection, segment) = FindIntersectionWithOuterContour(
                        adjustedVertices[3], adjustedVertices[2], outerContour, true);

                    if (segment != null)
                    {
                        result.Add(new ContourSegment(adjustedVertices[2], intersection,
                            segment.Center, segment.Radius, segment.CounterClockwise));
                    }
                    else if (Distance(intersection, adjustedVertices[2]) > 0.1)
                    {
                        result.Add(new ContourSegment(adjustedVertices[1], adjustedVertices[2]));
                    }
                    else
                    {
                        result.Add(new ContourSegment(adjustedVertices[1], adjustedVertices[2]));
                    }
                }
                else
                {
                    result.Add(new ContourSegment(adjustedVertices[1], adjustedVertices[2]));
                }

                result.Add(new ContourSegment(adjustedVertices[2], adjustedVertices[3]));
                result.Add(new ContourSegment(adjustedVertices[3], adjustedVertices[0]));

                Console.WriteLine($"\n🔷 Build4SegmentContour ELEMENT {numerElemntu} _stronaElementu: {_stronaElementu} - RETURN 8");

                Console.WriteLine($"   adjustedVertices[0]: ({adjustedVertices[0].X:F2}, {adjustedVertices[0].Y:F2})");
                Console.WriteLine($"   adjustedVertices[1]: ({adjustedVertices[1].X:F2}, {adjustedVertices[1].Y:F2})");
                Console.WriteLine($"   adjustedVertices[2]: ({adjustedVertices[2].X:F2}, {adjustedVertices[2].Y:F2})");
                Console.WriteLine($"   adjustedVertices[3]: ({adjustedVertices[3].X:F2}, {adjustedVertices[3].Y:F2})");

                return result;
            }

            // ============================================================
            // PRZYPADEK 3: T1 z łukiem (t1AfterArc / t1BeforeArc)
            // ============================================================
            bool isBottomElement = _stronaElementu == "Dół";

            bool t1AfterArc =
                !isBottomElement &&
                sourceIndex >= 0 &&
                outerContour != null &&
                sourceIndex < outerContour.Count &&
                outerContour.Count > 0 &&
                leftJoin == "T1" &&
                outerContour[(sourceIndex - 1 + outerContour.Count) % outerContour.Count].Type == SegmentType.Arc;

            bool t1BeforeArc =
                !isBottomElement &&
                sourceIndex >= 0 &&
                outerContour != null &&
                sourceIndex < outerContour.Count &&
                outerContour.Count > 0 &&
                rightJoin == "T1" &&
                outerContour[(sourceIndex + 1) % outerContour.Count].Type == SegmentType.Arc;

            if (t1AfterArc)
            {
                var outerArcSegment = outerContour[previousIndex];
                XPoint bevel = GetT1BevelPoint(adjustedVertices[2], adjustedVertices[1], outerArcSegment, outerContour);

                Console.WriteLine($"\n🔷 Build4SegmentContour ELEMENT {numerElemntu} _stronaElementu: {_stronaElementu} - RETURN 9");

                return new List<ContourSegment>
                        {
                            segZewnetrzny,
                            new ContourSegment(adjustedVertices[1], bevel),
                            new ContourSegment(bevel, adjustedVertices[2]),
                            segWewnetrzny,
                            new ContourSegment(adjustedVertices[3], adjustedVertices[0])
                        };
            }

            if (t1BeforeArc)
            {
                var outerArcSegment = outerContour[nextIndex];
                XPoint bevel = GetT1BevelPoint(adjustedVertices[1], adjustedVertices[2], outerArcSegment, outerContour);

                Console.WriteLine($"\n🔷 Build4SegmentContour ELEMENT {numerElemntu} _stronaElementu: {_stronaElementu} - RETURN 10");

                return new List<ContourSegment>
                        {
                            segZewnetrzny,
                            new ContourSegment(adjustedVertices[1], bevel),
                            new ContourSegment(bevel, adjustedVertices[2]),
                            segWewnetrzny,
                            new ContourSegment(adjustedVertices[3], adjustedVertices[0])
                        };
            }

            Console.WriteLine($"\n🔷 Build4SegmentContour ELEMENT {numerElemntu} _stronaElementu: {_stronaElementu} - RETURN 11");

            // ============================================================
            // PRZYPADEK 4: Standardowy kontur 4-segmentowy
            // ============================================================
            return new List<ContourSegment>
                    {
                        segZewnetrzny,
                        new ContourSegment(adjustedVertices[1], adjustedVertices[2]),
                        segWewnetrzny,
                        new ContourSegment(adjustedVertices[3], adjustedVertices[0])
                    };
        }


        private static bool CzyObaKonceWspolnyFragmentTylkoLinie(
        List<XPoint> wierzcholki,
        List<ContourSegment> outerContour,
        List<ContourSegment> innerContour,
        int sourceIndex)
        {
            if (wierzcholki == null || wierzcholki.Count < 4)
                return false;

            if (outerContour == null || innerContour == null)
                return false;

            if (sourceIndex < 0 ||
                sourceIndex >= outerContour.Count ||
                sourceIndex >= innerContour.Count)
                return false;

            int previousIndex =
                (sourceIndex - 1 + outerContour.Count) % outerContour.Count;

            int nextIndex =
                (sourceIndex + 1) % outerContour.Count;

            // Interesuje nas sytuacja, gdy oba sąsiednie
            // fragmenty konturu są liniami.
            bool outerPreviousLine =
                outerContour[previousIndex].Type == SegmentType.Line;

            bool outerNextLine =
                outerContour[nextIndex].Type == SegmentType.Line;

            bool innerPreviousLine =
                innerContour[previousIndex].Type == SegmentType.Line;

            bool innerNextLine =
                innerContour[nextIndex].Type == SegmentType.Line;

            return outerPreviousLine &&
                   outerNextLine &&
                   innerPreviousLine &&
                   innerNextLine;
        }

        private static void ReplaceNearestPoint(List<XPoint> points, XPoint replacement)
        {
            if (points == null || points.Count == 0)
                return;

            int nearestIndex = 0;
            double nearestDistance = Distance(points[0], replacement);
            for (int i = 1; i < points.Count; i++)
            {
                double distance = Distance(points[i], replacement);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }

            points[nearestIndex] = replacement;
        }

        private static XPoint GetT1BevelPoint(XPoint outerPoint, XPoint innerPoint,
        ContourSegment innerSegment, List<ContourSegment> outerContour)
        {
            // Sprawdź czy innerSegment to łuk czy linia
            if (innerSegment.Type == SegmentType.Arc && innerSegment.Center != null)
            {
                // ============================================================
                // PRZYPADEK: innerSegment to ŁUK
                // Oblicz punkt na przedłużeniu stycznej do łuku
                // ============================================================

                // Oblicz wektor od środka łuku do punktu wewnętrznego (innerPoint)
                double dx = innerPoint.X - innerSegment.Center.Value.X;
                double dy = innerPoint.Y - innerSegment.Center.Value.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);

                if (length > 0.001)
                {
                    // Wektor styczny do łuku (zależny od kierunku CCW/CW)
                    double tx, ty;
                    if (innerSegment.CounterClockwise)
                    {
                        // Dla CCW: styczna = (-dy, dx) / długość
                        tx = -dy / length;
                        ty = dx / length;
                    }
                    else
                    {
                        // Dla CW: styczna = (dy, -dx) / długość
                        tx = dy / length;
                        ty = -dx / length;
                    }

                    // Sprawdź który kierunek stycznej jest bliższy outerPoint
                    // Sprawdzamy dwa kierunki: +styczna i -styczna
                    XPoint tangentPlus = new XPoint(
                        innerPoint.X + tx * 100.0,
                        innerPoint.Y + ty * 100.0
                    );
                    XPoint tangentMinus = new XPoint(
                        innerPoint.X - tx * 100.0,
                        innerPoint.Y - ty * 100.0
                    );

                    // Wybierz kierunek stycznej który jest bliższy outerPoint
                    double distPlus = Distance(outerPoint, tangentPlus);
                    double distMinus = Distance(outerPoint, tangentMinus);

                    // Użyj właściwego kierunku stycznej
                    double finalTx = (distPlus < distMinus) ? tx : -tx;
                    double finalTy = (distPlus < distMinus) ? ty : -ty;

                    // Znajdź przecięcie stycznej z konturem zewnętrznym
                    XPoint intersection = FindIntersectionWithContourT1i3(
                        innerPoint,
                        new XPoint(innerPoint.X + finalTx * 1000.0, innerPoint.Y + finalTy * 1000.0),
                        outerContour
                    );

                    // Jeśli znaleziono przecięcie i jest w rozsądnej odległości, użyj go
                    double distToIntersection = Distance(innerPoint, intersection);
                    double distToOuter = Distance(innerPoint, outerPoint);

                    if (distToIntersection > 1.0 && distToIntersection < distToOuter * 2.0)
                    {
                        return intersection;
                    }

                    // Fallback: użyj przedłużenia stycznej
                    double bevelLength = Math.Max(
                        Distance(innerPoint, outerPoint) * 0.3,
                        10.0
                    );

                    return new XPoint(
                        innerPoint.X + finalTx * bevelLength,
                        innerPoint.Y + finalTy * bevelLength
                    );
                }
            }
            else if (innerSegment.Type == SegmentType.Line)
            {
                // ============================================================
                // PRZYPADEK: innerSegment to LINIA
                // Przedłuż linię w kierunku outerPoint
                // ============================================================

                // Wektor kierunkowy linii
                double dx = innerSegment.End.X - innerSegment.Start.X;
                double dy = innerSegment.End.Y - innerSegment.Start.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);

                if (length > 0.001)
                {
                    double tx = dx / length;
                    double ty = dy / length;

                    // Sprawdź który kierunek jest bliższy outerPoint
                    XPoint forward = new XPoint(
                        innerPoint.X + tx * 100.0,
                        innerPoint.Y + ty * 100.0
                    );
                    XPoint backward = new XPoint(
                        innerPoint.X - tx * 100.0,
                        innerPoint.Y - ty * 100.0
                    );

                    double distForward = Distance(outerPoint, forward);
                    double distBackward = Distance(outerPoint, backward);

                    double finalTx = (distForward < distBackward) ? tx : -tx;
                    double finalTy = (distForward < distBackward) ? ty : -ty;

                    // Znajdź przecięcie z konturem zewnętrznym
                    XPoint intersection = FindIntersectionWithContourT1i3(
                        innerPoint,
                        new XPoint(innerPoint.X + finalTx * 1000.0, innerPoint.Y + finalTy * 1000.0),
                        outerContour
                    );

                    double distToIntersection = Distance(innerPoint, intersection);
                    double distToOuter = Distance(innerPoint, outerPoint);

                    if (distToIntersection > 1.0 && distToIntersection < distToOuter * 2.0)
                    {
                        return intersection;
                    }

                    // Fallback
                    double bevelLength = Math.Max(
                        Distance(innerPoint, outerPoint) * 0.3,
                        10.0
                    );

                    return new XPoint(
                        innerPoint.X + finalTx * bevelLength,
                        innerPoint.Y + finalTy * bevelLength
                    );
                }
            }

            // ============================================================
            // FALLBACK: oryginalna metoda (gdy nie udało się wyliczyć)
            // ============================================================
            return new XPoint(
                outerPoint.X,
                outerPoint.Y + (innerPoint.Y - outerPoint.Y) * 0.5
            );
        }

        /// <summary>
        /// Pomocnicza funkcja do znajdowania przecięcia z konturem
        /// </summary>
        private static XPoint FindIntersectionWithContourT1i3(
            XPoint startPoint,
            XPoint endPoint,
            List<ContourSegment> contour)
        {
            if (contour == null || contour.Count == 0)
                return endPoint;


            XPoint closestIntersection = endPoint;
            double minDistance = double.MaxValue;

            foreach (var seg in contour)
            {
                if (seg.Type == SegmentType.Line)
                {
                    var intersection = GetLinesIntersectionNullableT1i3(startPoint, endPoint, seg.Start, seg.End);

                    if (intersection.HasValue)
                    {
                        double dist = Distance(startPoint, intersection.Value);
                        if (dist > 0.01 && dist < minDistance)
                        {
                            minDistance = dist;
                            closestIntersection = intersection.Value;
                        }
                    }
                }
                else if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    // Dla łuku - sprawdź przecięcie z okręgiem
                    var intersections = GetLineCircleIntersectionsT1i3(startPoint, endPoint, seg.Center.Value, seg.Radius);

                    foreach (var pt in intersections)
                    {
                        if (IsPointOnArcT1i3(pt, seg, 0.1))
                        {
                            double dist = Distance(startPoint, pt);
                            if (dist > 0.01 && dist < minDistance)
                            {
                                minDistance = dist;
                                closestIntersection = pt;
                            }
                        }
                    }
                }
            }

            return closestIntersection;
        }

        /// <summary>
        /// Pomocnicza funkcja do znajdowania przecięcia dwóch linii
        /// </summary>
        private static XPoint? GetLinesIntersectionNullableT1i3(XPoint a1, XPoint a2, XPoint b1, XPoint b2)
        {
            double dx1 = a2.X - a1.X;
            double dy1 = a2.Y - a1.Y;
            double dx2 = b2.X - b1.X;
            double dy2 = b2.Y - b1.Y;

            double det = dx1 * dy2 - dy1 * dx2;

            if (Math.Abs(det) < 1e-6)
                return null;

            double t = ((b1.X - a1.X) * dy2 - (b1.Y - a1.Y) * dx2) / det;

            return new XPoint(
                a1.X + t * dx1,
                a1.Y + t * dy1
            );
        }

        private static bool IsPointOnArcT1i3(XPoint point, ContourSegment arc, double tolerance = 0.1)
        {
            if (arc.Center == null)
                return false;

            double distToCenter = Distance(point, arc.Center.Value);
            double radiusDiff = Math.Abs(distToCenter - arc.Radius);

            // Sprawdź czy punkt leży na okręgu
            if (radiusDiff > tolerance)
                return false;

            // Sprawdź czy punkt leży na łuku (między kątami start i end)
            double angle = Math.Atan2(point.Y - arc.Center.Value.Y, point.X - arc.Center.Value.X);
            double startAngle = Math.Atan2(arc.Start.Y - arc.Center.Value.Y, arc.Start.X - arc.Center.Value.X);
            double endAngle = Math.Atan2(arc.End.Y - arc.Center.Value.Y, arc.End.X - arc.Center.Value.X);

            // Normalizacja kątów do [0, 2π)
            angle = (angle + 2 * Math.PI) % (2 * Math.PI);
            startAngle = (startAngle + 2 * Math.PI) % (2 * Math.PI);
            endAngle = (endAngle + 2 * Math.PI) % (2 * Math.PI);

            // Sprawdź czy punkt leży między kątami start i end
            if (arc.CounterClockwise)
            {
                // Łuk CCW: od startAngle do endAngle (przeciwnie do wskazówek)
                if (startAngle <= endAngle)
                    return angle >= startAngle - tolerance && angle <= endAngle + tolerance;
                else
                    return angle >= startAngle - tolerance || angle <= endAngle + tolerance;
            }
            else
            {
                // Łuk CW: od startAngle do endAngle (zgodnie z wskazówkami)
                if (endAngle <= startAngle)
                    return angle <= startAngle + tolerance && angle >= endAngle - tolerance;
                else
                    return angle <= startAngle + tolerance || angle >= endAngle - tolerance;
            }
        }

        /// <summary>
        /// Pomocnicza funkcja do znajdowania przecięć linii z okręgiem
        /// </summary>
        private static List<XPoint> GetLineCircleIntersectionsT1i3(
            XPoint p1, XPoint p2,
            XPoint center, double radius)
        {
            var result = new List<XPoint>();

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double fx = p1.X - center.X;
            double fy = p1.Y - center.Y;

            double a = dx * dx + dy * dy;
            double b = 2 * (fx * dx + fy * dy);
            double c = fx * fx + fy * fy - radius * radius;

            double discriminant = b * b - 4 * a * c;

            if (discriminant < -1e-9)
                return result;

            discriminant = Math.Max(0, discriminant);
            double sqrtD = Math.Sqrt(discriminant);

            double t1 = (-b + sqrtD) / (2 * a);
            double t2 = (-b - sqrtD) / (2 * a);

            if (t1 >= 0 && t1 <= 1)
                result.Add(new XPoint(p1.X + t1 * dx, p1.Y + t1 * dy));

            if (t2 >= 0 && t2 <= 1 && Math.Abs(t1 - t2) > 0.0001)
                result.Add(new XPoint(p1.X + t2 * dx, p1.Y + t2 * dy));

            return result;
        }

        private static XPoint GetT3BevelPoint(
        XPoint start,
        XPoint end,
        ContourSegment innerSegment,
        List<ContourSegment> outerContour)
        {
            if (outerContour == null || outerContour.Count == 0)
                return new XPoint(start.X + (end.X - start.X) * 0.5, end.Y);


            // ============================================================
            // OKREŚL KIERUNEK NA PODSTAWIE TYPU SEGMENTU
            // ============================================================
            double tx, ty;

            if (innerSegment.Type == SegmentType.Arc && innerSegment.Center != null)
            {
                // Dla łuku - użyj stycznej do łuku w punkcie start
                double dx = start.X - innerSegment.Center.Value.X;
                double dy = start.Y - innerSegment.Center.Value.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);

                if (length > 0.001)
                {
                    if (innerSegment.CounterClockwise)
                    {
                        tx = -dy / length;
                        ty = dx / length;
                    }
                    else
                    {
                        tx = dy / length;
                        ty = -dx / length;
                    }

                    // Sprawdź czy kierunek stycznej jest zgodny z kierunkiem start->end
                    double dot = tx * (end.X - start.X) + ty * (end.Y - start.Y);
                    if (dot < 0)
                    {
                        tx = -tx;
                        ty = -ty;
                    }
                }
                else
                {
                    // Fallback: kierunek od start do end
                    double dx2 = end.X - start.X;
                    double dy2 = end.Y - start.Y;
                    double length2 = Math.Sqrt(dx2 * dx2 + dy2 * dy2);
                    if (length2 > 0.001)
                    {
                        tx = dx2 / length2;
                        ty = dy2 / length2;
                    }
                    else
                    {
                        tx = 0;
                        ty = -1;
                    }
                }
            }
            else
            {
                // Dla linii - kierunek od start do end
                double dx = end.X - start.X;
                double dy = end.Y - start.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);

                if (length > 0.001)
                {
                    tx = dx / length;
                    ty = dy / length;
                }
                else
                {
                    tx = 0;
                    ty = -1;
                }
            }

            // ============================================================
            // PRZEDŁUŻENIE O 500 JEDNOSTEK
            // ============================================================
            double extensionLength = 500.0;
            XPoint extendedPoint = new XPoint(
                start.X + tx * extensionLength,
                start.Y + ty * extensionLength
            );

            // ============================================================
            // ZNAJDŹ PRZECIĘCIE Z outerContour
            // ============================================================
            XPoint intersection = FindIntersectionWithContourT1i3(
                start,
                extendedPoint,
                outerContour
            );

            Console.WriteLine($"   GetT3BevelPoint: start=({start.X:F2},{start.Y:F2}) end=({end.X:F2},{end.Y:F2}) intersection=({intersection.X:F2},{intersection.Y:F2})");

            double distToIntersection = Distance(start, intersection);

            if (distToIntersection > 0.1 && distToIntersection < extensionLength)
            {
                return intersection;
            }

            // Fallback
            double fallbackLength = Distance(start, end) * 1.5;
            return new XPoint(
                start.X + tx * fallbackLength,
                start.Y + ty * fallbackLength
            );
        }

        /// <summary>
        /// Znajduje przecięcie linii wyznaczonej przez dwa punkty z outerContour
        /// Zwraca punkt przecięcia oraz dane do utworzenia segmentu (linia lub łuk)
        /// </summary>
        private static (XPoint intersectionPoint, ContourSegment? segment) FindIntersectionWithOuterContour(
            XPoint point1,
            XPoint point2,
            List<ContourSegment> outerContour,
            bool goUp)
        {
            if (outerContour == null || outerContour.Count == 0)
                return (point2, null);

            // Oblicz wektor kierunkowy linii
            double dx = point2.X - point1.X;
            double dy = point2.Y - point1.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);

            if (length < 0.001)
                return (point2, null);

            // Wektor jednostkowy
            double tx = dx / length;
            double ty = dy / length;

            // Przedłużenie linii
            double extensionLength = 1000.0;
            XPoint extendedPoint = new XPoint(
                point1.X + tx * extensionLength,
                point1.Y + ty * extensionLength
            );

            XPoint bestPoint = point2;
            ContourSegment? bestSegment = null;
            double minDistance = double.MaxValue;

            // Przeszukujemy wszystkie segmenty konturu
            for (int idx = 0; idx < outerContour.Count; idx++)
            {
                var seg = outerContour[idx];

                if (seg.Type == SegmentType.Line)
                {
                    // Przecięcie z linią
                    var intersection = GetLinesIntersectionNullable(point1, extendedPoint, seg.Start, seg.End);

                    if (intersection.HasValue)
                    {
                        double dist = Distance(point1, intersection.Value);

                        // Sprawdź czy punkt jest w odpowiednim kierunku
                        double dot = (intersection.Value.X - point1.X) * tx + (intersection.Value.Y - point1.Y) * ty;
                        bool isForward = dot > 0;

                        if (isForward && dist > 0.1 && dist < minDistance)
                        {
                            // Sprawdź czy punkt leży na segmencie
                            if (IsPointOnSegment(intersection.Value, seg.Start, seg.End))
                            {
                                minDistance = dist;
                                bestPoint = intersection.Value;
                                bestSegment = seg;
                            }
                        }
                    }
                }
                else if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    // Przecięcie z łukiem (okręgiem)
                    var intersections = GetLineCircleIntersections(point1, extendedPoint, seg.Center.Value, seg.Radius);

                    foreach (var pt in intersections)
                    {
                        // Sprawdź czy punkt leży na łuku
                        if (IsPointOnArc(pt, seg, 0.1))
                        {
                            double dist = Distance(point1, pt);

                            // Sprawdź czy punkt jest w odpowiednim kierunku
                            double dot = (pt.X - point1.X) * tx + (pt.Y - point1.Y) * ty;
                            bool isForward = dot > 0;

                            if (isForward && dist > 0.1 && dist < minDistance)
                            {
                                minDistance = dist;
                                bestPoint = pt;
                                bestSegment = seg;
                            }
                        }
                    }
                }
            }

            // Jeśli znaleziono przecięcie z łukiem, zwróć dane do utworzenia łuku
            if (bestSegment != null && bestSegment.Type == SegmentType.Arc && bestSegment.Center != null)
            {
                // Oblicz kąty dla łuku
                double startAngle = Math.Atan2(bestPoint.Y - bestSegment.Center.Value.Y, bestPoint.X - bestSegment.Center.Value.X);
                double endAngle = Math.Atan2(point1.Y - bestSegment.Center.Value.Y, point1.X - bestSegment.Center.Value.X);

                // Normalizacja dla CCW/CW
                if (bestSegment.CounterClockwise)
                {
                    while (endAngle <= startAngle) endAngle += 2 * Math.PI;
                }
                else
                {
                    while (endAngle >= startAngle) endAngle -= 2 * Math.PI;
                }

                // Utwórz segment łuku od bestPoint do point1
                ContourSegment arcSegment = new ContourSegment(
                    bestPoint,
                    point1,
                    bestSegment.Center.Value,
                    bestSegment.Radius,
                    bestSegment.CounterClockwise
                );

                return (bestPoint, arcSegment);
            }

            // Jeśli znaleziono przecięcie z linią lub nie znaleziono żadnego
            return (bestPoint, null);
        }

        /// <summary>
        /// Sprawdza czy punkt leży na odcinku
        /// </summary>
        private static bool IsPointOnSegment(XPoint point, XPoint start, XPoint end, double tolerance = 0.1)
        {
            double cross = (point.X - start.X) * (end.Y - start.Y) - (point.Y - start.Y) * (end.X - start.X);
            if (Math.Abs(cross) > tolerance)
                return false;

            double dot = (point.X - start.X) * (end.X - start.X) + (point.Y - start.Y) * (end.Y - start.Y);
            if (dot < 0)
                return false;

            double squaredLength = (end.X - start.X) * (end.X - start.X) + (end.Y - start.Y) * (end.Y - start.Y);
            if (dot > squaredLength)
                return false;

            return true;
        }

        /// <summary>
        /// Znajduje przecięcie dwóch linii
        /// </summary>
        private static XPoint? GetLinesIntersectionNullable(XPoint a1, XPoint a2, XPoint b1, XPoint b2)
        {
            double dx1 = a2.X - a1.X;
            double dy1 = a2.Y - a1.Y;
            double dx2 = b2.X - b1.X;
            double dy2 = b2.Y - b1.Y;

            double det = dx1 * dy2 - dy1 * dx2;

            if (Math.Abs(det) < 1e-6)
                return null;

            double t = ((b1.X - a1.X) * dy2 - (b1.Y - a1.Y) * dx2) / det;

            return new XPoint(
                a1.X + t * dx1,
                a1.Y + t * dy1
            );
        }

        /// <summary>
        /// Znajduje przecięcia linii z okręgiem
        /// </summary>
        private static List<XPoint> GetLineCircleIntersections(
            XPoint p1,
            XPoint p2,
            XPoint center,
            double radius)
        {
            var result = new List<XPoint>();

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double fx = p1.X - center.X;
            double fy = p1.Y - center.Y;

            double a = dx * dx + dy * dy;
            double b = 2 * (fx * dx + fy * dy);
            double c = fx * fx + fy * fy - radius * radius;

            double discriminant = b * b - 4 * a * c;

            if (discriminant < -1e-9)
                return result;

            discriminant = Math.Max(0, discriminant);
            double sqrtD = Math.Sqrt(discriminant);

            double t1 = (-b + sqrtD) / (2 * a);
            double t2 = (-b - sqrtD) / (2 * a);

            if (t1 >= 0 && t1 <= 1)
                result.Add(new XPoint(p1.X + t1 * dx, p1.Y + t1 * dy));

            if (t2 >= 0 && t2 <= 1 && Math.Abs(t1 - t2) > 0.0001)
                result.Add(new XPoint(p1.X + t2 * dx, p1.Y + t2 * dy));

            return result;
        }

        /// <summary>
        /// Sprawdza czy punkt leży na łuku
        /// </summary>
        private static bool IsPointOnArc(XPoint point, ContourSegment arc, double tolerance = 1)
        {
            if (arc.Center == null)
                return false;

            double distToCenter = Distance(point, arc.Center.Value);
            double radiusDiff = Math.Abs(distToCenter - arc.Radius);

            if (radiusDiff > tolerance)
                return false;

            double angle = Math.Atan2(point.Y - arc.Center.Value.Y, point.X - arc.Center.Value.X);
            double startAngle = Math.Atan2(arc.Start.Y - arc.Center.Value.Y, arc.Start.X - arc.Center.Value.X);
            double endAngle = Math.Atan2(arc.End.Y - arc.Center.Value.Y, arc.End.X - arc.Center.Value.X);

            angle = (angle + 2 * Math.PI) % (2 * Math.PI);
            startAngle = (startAngle + 2 * Math.PI) % (2 * Math.PI);
            endAngle = (endAngle + 2 * Math.PI) % (2 * Math.PI);

            if (arc.CounterClockwise)
            {
                if (startAngle <= endAngle)
                    return angle >= startAngle - tolerance && angle <= endAngle + tolerance;
                else
                    return angle >= startAngle - tolerance || angle <= endAngle + tolerance;
            }
            else
            {
                if (endAngle <= startAngle)
                    return angle <= startAngle + tolerance && angle >= endAngle - tolerance;
                else
                    return angle <= startAngle + tolerance || angle >= endAngle - tolerance;
            }
        }


        /// <summary>
        /// Filtruje segmenty konturu dla danej strony na podstawie kąta linii
        /// </summary>
        private List<ContourSegment> GetSegmentsForSide(List<ContourSegment> contour, string strona)
        {
            if (contour == null || contour.Count == 0)
                return contour;

            var result = new List<ContourSegment>();

            foreach (var seg in contour)
            {
                // Oblicz kąt linii między Start a End
                double dx = seg.End.X - seg.Start.X;
                double dy = seg.End.Y - seg.Start.Y;
                double angleRad = Math.Atan2(dy, dx);
                double angleDeg = angleRad * 180.0 / Math.PI;
                if (angleDeg < 0) angleDeg += 360.0;

                // Określ stronę dla tego segmentu
                string segmentSide = StronaOknaHelper.OkreslStroneNaPodstawieKataLinii(angleDeg);

                // Jeśli segment pasuje do żądanej strony – dodaj go
                if (segmentSide == strona)
                    result.Add(seg);
            }

            // Jeśli nie znaleziono żadnego segmentu dla danej strony, zwróć cały kontur (fallback)
            return result.Count > 0 ? result : contour.ToList();
        }
        /// <summary>
        /// Znajduje punkt przecięcia półprostej (punkt + kierunek) z konturem.
        /// </summary>
        /// <param name="point">Punkt startowy</param>
        /// <param name="angleDegrees">Kąt kierunku w stopniach (0° = w prawo, 90° = w dół)</param>
        /// <param name="contour">Kontur (outerContour lub innerContour)</param>
        /// <param name="goForward">true = szukamy w kierunku kąta, false = przeciwnie</param>
        /// <param name="maxDistance">Maksymalna odległość szukania</param>
        /// <returns>Punkt na konturze lub oryginalny, jeśli nie znaleziono</returns>
        private XPoint FindIntersectionWithContourByAngle(
            XPoint point,
            double angleDegrees,
            List<ContourSegment> contour,
            bool goForward = true,
            double maxDistance = 1000.0)
        {
            if (contour == null || contour.Count == 0)
                return point;

            // Konwersja kąta na radiany i wektor kierunkowy
            double angleRad = angleDegrees * Math.PI / 180.0;
            double dx = Math.Cos(angleRad);
            double dy = Math.Sin(angleRad);

            if (!goForward)
            {
                dx = -dx;
                dy = -dy;
            }

            // Przedłużenie punktu w zadanym kierunku
            XPoint extendedPoint = new XPoint(
                point.X + dx * maxDistance,
                point.Y + dy * maxDistance
            );

            // Znajdź przecięcie z konturem (używamy istniejącej funkcji)
            XPoint intersection = FindIntersectionWithContourT1i3(point, extendedPoint, contour);

            // Sprawdź, czy przecięcie jest w odpowiednim kierunku
            double dot = (intersection.X - point.X) * dx + (intersection.Y - point.Y) * dy;
            if (dot > 0.1 && Distance(point, intersection) > 0.1 && Distance(point, intersection) < maxDistance)
            {
                return intersection;
            }

            // Jeśli nie znaleziono przecięcia, spróbuj znaleźć najbliższy punkt na konturze
            return FindClosestPointOnContour(point, contour);
        }

        /// <summary>
        /// Znajduje najbliższy punkt na konturze (dla przypadku, gdy nie ma przecięcia).
        /// </summary>
        private XPoint FindClosestPointOnContour(XPoint point, List<ContourSegment> contour)
        {
            if (contour == null || contour.Count == 0)
                return point;

            XPoint bestPoint = point;
            double minDist = double.MaxValue;

            foreach (var seg in contour)
            {
                if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    // Dla łuku – rzutuj punkt na okrąg
                    double dx = point.X - seg.Center.Value.X;
                    double dy = point.Y - seg.Center.Value.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist > 0.01)
                    {
                        XPoint candidate = new XPoint(
                            seg.Center.Value.X + seg.Radius * (dx / dist),
                            seg.Center.Value.Y + seg.Radius * (dy / dist)
                        );
                        double d = Distance(point, candidate);
                        if (d < minDist)
                        {
                            minDist = d;
                            bestPoint = candidate;
                        }
                    }
                }
                else if (seg.Type == SegmentType.Line)
                {
                    // Dla linii – rzutuj punkt na odcinek
                    XPoint candidate = ProjectPointOnSegment(point, seg.Start, seg.End);
                    double d = Distance(point, candidate);
                    if (d < minDist)
                    {
                        minDist = d;
                        bestPoint = candidate;
                    }
                }
            }

            return bestPoint;
        }

        /// <summary>
        /// Rzutuje punkt na odcinek.
        /// </summary>
        private XPoint ProjectPointOnSegment(XPoint point, XPoint a, XPoint b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double lenSq = dx * dx + dy * dy;
            if (lenSq < 0.0001) return a;

            double t = ((point.X - a.X) * dx + (point.Y - a.Y) * dy) / lenSq;
            t = Math.Max(0, Math.Min(1, t));

            return new XPoint(a.X + t * dx, a.Y + t * dy);
        }

        private ContourSegment BuildSegmentWithArc(
            XPoint start,
            XPoint end,
            List<ContourSegment> contourToSearch)
        {
            const double tolerance = 5;
            var arc = FindArcBetweenPoints(contourToSearch, start, end, tolerance);
            if (arc == null || !arc.Center.HasValue)
                return new ContourSegment(start, end);

            // Start i End konturu elementu są zawsze zachowane. Dla granicy
            // wewnętrznej przechodzimy zwykle po tym samym łuku w przeciwną stronę.
            bool followsSourceDirection = IsPointOnAngularSweep(
                end, start, arc.End, arc.Center.Value,
                arc.CounterClockwise, arc.Radius, tolerance);

            return new ContourSegment(
                start,
                end,
                arc.Center,
                arc.Radius,
                followsSourceDirection ? arc.CounterClockwise : !arc.CounterClockwise);
        }

        private static bool IsPointOnAngularSweep(
            XPoint point,
            XPoint start,
            XPoint end,
            XPoint center,
            bool increasingAngle,
            double radius,
            double pointTolerance)
        {
            double pointAngle = Math.Atan2(point.Y - center.Y, point.X - center.X);
            double startAngle = Math.Atan2(start.Y - center.Y, start.X - center.X);
            double endAngle = Math.Atan2(end.Y - center.Y, end.X - center.X);

            double sweep = increasingAngle
                ? PositiveAngleDelta(startAngle, endAngle)
                : PositiveAngleDelta(endAngle, startAngle);
            double progress = increasingAngle
                ? PositiveAngleDelta(startAngle, pointAngle)
                : PositiveAngleDelta(pointAngle, startAngle);

            // Tolerancja 0,1 mm odpowiada maksymalnie małemu błędowi kątowemu;
            // stałe minimum zabezpiecza małe promienie.
            double angularTolerance = Math.Max(1e-9, pointTolerance / Math.Max(radius, 1e-9));
            return progress <= sweep + angularTolerance;
        }

        private static double PositiveAngleDelta(double from, double to)
        {
            double result = (to - from) % (2 * Math.PI);
            return result < 0 ? result + 2 * Math.PI : result;
        }

        private XPoint GetPointOnArcAtAngle(ContourSegment arc, XPoint referencePoint)
        {
            if (!arc.Center.HasValue)
                return referencePoint;

            // Oblicz kąt punktu odniesienia względem środka łuku
            double angle = Math.Atan2(referencePoint.Y - arc.Center.Value.Y,
                                      referencePoint.X - arc.Center.Value.X);

            // Znajdź punkt na łuku o tym samym kącie
            return new XPoint(
                arc.Center.Value.X + arc.Radius * Math.Cos(angle),
                arc.Center.Value.Y + arc.Radius * Math.Sin(angle)
            );
        }


        private ContourSegment FindArcBetweenPoints(
         List<ContourSegment> contour,
         XPoint point1,
         XPoint point2,
         double tolerance = 1.0) // domyślnie 1.0
        {
            foreach (var seg in contour)
            {
                if (seg.Type != SegmentType.Arc || !seg.Center.HasValue)
                    continue;
                if (IsPointOnArc(point1, seg, tolerance) && IsPointOnArc(point2, seg, tolerance))
                    return seg;
            }
            return null;
        }

        // Odległość między dwoma punktami
        private static double Distance(XPoint a, XPoint b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public float ObliczDlugoscKonturu(List<ContourSegment> kontur)
        {
            if (kontur == null || kontur.Count == 0)
                return 0;

            double sumaDlugosci = 0;

            foreach (var segment in kontur)
            {
                if (segment.Type == SegmentType.Arc)
                {
                    // Dla łuku - oblicz długość łuku
                    sumaDlugosci += DlugoscLukuKontur(segment);
                }
                else
                {
                    // Dla linii - odległość między punktami
                    sumaDlugosci += OdlegloscKontur(segment.Start, segment.End);
                }
            }

            return (float)sumaDlugosci;
        }
        private double DlugoscLukuKontur(ContourSegment arc)
        {
            if (arc.Type != SegmentType.Arc || !arc.Center.HasValue)
                return 0;

            // Oblicz kąt środkowy łuku
            double startAngle = Math.Atan2(arc.Start.Y - arc.Center.Value.Y, arc.Start.X - arc.Center.Value.X);
            double endAngle = Math.Atan2(arc.End.Y - arc.Center.Value.Y, arc.End.X - arc.Center.Value.X);

            // Normalizacja kątów
            if (arc.CounterClockwise)
            {
                if (endAngle < startAngle)
                    endAngle += 2 * Math.PI;
            }
            else
            {
                if (endAngle > startAngle)
                    endAngle -= 2 * Math.PI;
            }

            double angleDelta = Math.Abs(endAngle - startAngle);

            // Długość łuku = promień * kąt (w radianach)
            return arc.Radius * angleDelta;
        }

        private double OdlegloscKontur(XPoint p1, XPoint p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private List<XPoint> GetStartT1Triangle(
        XPoint _innerP,
        XPoint _outerP,
        List<XPoint> _outer,
        float angleDegrees,
        float prevangleDegrees,
        float nextangleDegrees,
        string stronaWModelu,
        string stonaOstanioDodanegoElementu,
        int nk,
        int next,
        int prev)
        {
            List<XPoint> intersections = new List<XPoint>();

            bool czyParzysta = (nk + 1) % 2 == 0;

            string prevSide = StronaOknaHelper.OkreslStrone(prevangleDegrees, prev, _outer);
            string nextSide = StronaOknaHelper.OkreslStrone(nextangleDegrees, next, _outer);

            // ============================================================
            // GÓRA / DÓŁ
            //
            // Te elementy muszą być DŁUŻSZE.
            //
            // Dlatego ich końce mają dochodzić do zewnętrznego konturu.
            // ============================================================

            bool poziomy =
                stronaWModelu == "Góra" ||
                stronaWModelu == "Dół";

            if (stronaWModelu == "Prawa" && prevSide == "Lewa")
            {
                poziomy = false;
                czyParzysta = false;
            }

            if (stronaWModelu == "Prawa" && prevSide == "Góra")
            {
                poziomy = false;
                czyParzysta = false;
            }

            if ((nextSide == "Góra" || prevSide == "Góra") && stronaWModelu == "Dół")
            {
                poziomy = true;
                czyParzysta = false;
            }

            if ((nextSide == "Dół" || prevSide == "Dół") && stronaWModelu == "Góra")
            {
                poziomy = false;
                czyParzysta = false;
            }

            //Console.WriteLine(
            //    $"🔺 GetStartT1Triangle " +
            //    $"strona={stronaWModelu} " +
            //    $"nk={nk} " +
            //    $"czyParzysta={czyParzysta} " +
            //    $"poziomy={poziomy}");

            if (poziomy)
            {
                // Dla GÓRA/DÓŁ zawsze zaczynamy od zewnętrznego punktu.
                XPoint startT1 = FindTriangleEdgeIntersectionByAngle(
                    _innerP,
                    angleDegrees - 180.0,
                    _outer);

                intersections.Add(new XPoint(
                    startT1.X,
                    startT1.Y));

                intersections.Add(new XPoint(
                    _outerP.X,
                    _outerP.Y));

                //Console.WriteLine(
                //    $"🔺 GÓRA/DÓŁ START LONG " +
                //    $"P1=({startT1.X:F3},{startT1.Y:F3}) " +
                //    $"P2=({_outerP.X:F3},{_outerP.Y:F3})");
            }
            else
            {
                // ========================================================
                // LEWA / PRAWA
                //
                // Zachowujemy dotychczasową logikę.
                // ========================================================

                if (czyParzysta)
                {
                    XPoint startT1 =
                        FindTriangleEdgeIntersectionByAngle(
                            _innerP,
                            angleDegrees - 180.0,
                            _outer);

                    intersections.Add(new XPoint(
                        startT1.X,
                        startT1.Y));

                    intersections.Add(new XPoint(
                        _outerP.X,
                        _outerP.Y));
                }
                else
                {
                    XPoint startT1 =
                        FindTriangleEdgeIntersectionByAngle(
                            _innerP,
                            prevangleDegrees,
                            _outer);

                    intersections.Add(new XPoint(
                        _innerP.X,
                        _innerP.Y));

                    intersections.Add(new XPoint(
                        startT1.X,
                        startT1.Y));
                }
            }

            return intersections;
        }

        private List<XPoint> GetEndT1Triangle(
        XPoint _innerP,
        XPoint _outerP,
        List<XPoint> _outer,
        float angleDegrees,
        float prevangleDegrees,
        float nextangleDegrees,
        string stronaWModelu,
        string stonaOstanioDodanegoElementu,
        int nk,
        int next,
        int prev)
        {
            List<XPoint> intersections = new List<XPoint>();

            string nextSide = StronaOknaHelper.OkreslStrone(nextangleDegrees, next, _outer);
            string prevSide = StronaOknaHelper.OkreslStrone(prevangleDegrees, prev, _outer);

            bool czyParzysta = (nk + 1) % 2 == 0;
            czyParzysta = false;

            bool poziome =
                stronaWModelu == "Góra" ||
                stronaWModelu == "Dół";

            if (stronaWModelu == "Góra" && nextSide == "Dół")
            {
                poziome = true;
                czyParzysta = false;
            }


            if (stronaWModelu == "Lewa" && nextSide == "Prawa" && nextangleDegrees < 180.0)
                poziome = true;

            if (stronaWModelu == "Lewa" && nextSide == "Góra")
            {
                poziome = false;
                czyParzysta = true;
            }

            if (stronaWModelu == "Góra" && nextSide == "Prawa")
            {
                poziome = false;
                czyParzysta = true;
            }

            if (stronaWModelu == "Góra" && prevSide == "Dół")
            {
                poziome = true;
                czyParzysta = false;
            }


            //Console.WriteLine(
            //    $"🔺 GetEndT1Triangle " +
            //    $"strona={stronaWModelu} " +
            //    $"nk={nk} " +
            //    $"warunek={czyParzysta} " +
            //    $"poziome={poziome}");

            // ============================================================
            // GÓRA / DÓŁ
            //
            // ZAWSZE DŁUŻSZE
            // ============================================================

            if (poziome)
            {
                XPoint endT1 =
                    FindTriangleEdgeIntersectionByAngle(
                        _innerP,
                        angleDegrees,
                        _outer);

                intersections.Add(new XPoint(
                    endT1.X,
                    endT1.Y));

                intersections.Add(new XPoint(
                    _outerP.X,
                    _outerP.Y));

                //Console.WriteLine(
                //    $"🔺 GÓRA/DÓŁ END LONG " +
                //    $"P1=({endT1.X:F3},{endT1.Y:F3}) " +
                //    $"P2=({_outerP.X:F3},{_outerP.Y:F3})");
            }
            else
            {
                // ========================================================
                // LEWA / PRAWA
                // Dotychczasowa logika
                // ========================================================

                if (czyParzysta)
                {
                    XPoint endT1 =
                        FindTriangleEdgeIntersectionByAngle(
                            _innerP,
                            angleDegrees,
                            _outer);

                    intersections.Add(new XPoint(
                        endT1.X,
                        endT1.Y));

                    intersections.Add(new XPoint(
                        _outerP.X,
                        _outerP.Y));
                }
                else
                {
                    XPoint endT1 =
                        FindTriangleEdgeIntersectionByAngle(
                            _innerP,
                            nextangleDegrees - 180.0,
                            _outer);

                    intersections.Add(new XPoint(
                        _innerP.X,
                        _innerP.Y));

                    intersections.Add(new XPoint(
                        endT1.X,
                        endT1.Y));
                }
            }

            return intersections;
        }


        private XPoint FindTriangleEdgeIntersectionByAngle(
        XPoint start,
        double angleDegrees,
        List<XPoint> outer)
        {
            if (outer == null || outer.Count < 3)
            {
                Console.WriteLine(
                    "⚠️ FindTriangleEdgeIntersectionByAngle: " +
                    "kontur ma mniej niż 3 punkty.");

                return start;
            }

            double angleRad = angleDegrees * Math.PI / 180.0;

            double dx = Math.Cos(angleRad);
            double dy = Math.Sin(angleRad);

            double bestT = double.MaxValue;

            XPoint bestPoint = start;

            // ============================================================
            // PRZECHODZIMY PO WSZYSTKICH BOKACH KONTURU
            // ============================================================

            for (int i = 0; i < outer.Count; i++)
            {
                XPoint a = outer[i];

                XPoint b = outer[(i + 1) % outer.Count];

                double sx = b.X - a.X;
                double sy = b.Y - a.Y;

                // Iloczyn wektorowy promienia i boku
                double denominator =
                    dx * sy -
                    dy * sx;

                // Równoległe
                if (Math.Abs(denominator) < 0.000001)
                    continue;

                double ax = a.X - start.X;
                double ay = a.Y - start.Y;

                double t =
                    (ax * sy - ay * sx) /
                    denominator;

                double u =
                    (ax * dy - ay * dx) /
                    denominator;

                // ========================================================
                // t > 0
                //
                // Punkt musi leżeć W KIERUNKU promienia.
                // ========================================================

                if (t <= 0.000001)
                    continue;

                // ========================================================
                // u musi należeć do odcinka [0,1]
                // ========================================================

                if (u < -0.000001 || u > 1.000001)
                    continue;

                // ========================================================
                // Bierzemy NAJBLIŻSZE przecięcie.
                // ========================================================

                if (t < bestT)
                {
                    bestT = t;

                    bestPoint = new XPoint(
                        start.X + t * dx,
                        start.Y + t * dy);
                }
            }

            if (bestT == double.MaxValue)
            {
                //Console.WriteLine(
                //    $"⚠️ Brak przecięcia trójkąta: " +
                //    $"start=({start.X:F3},{start.Y:F3}), " +
                //    $"angle={angleDegrees:F3}°");

                return start;
            }

            //Console.WriteLine(
            //    $"🔺 Intersection: " +
            //    $"start=({start.X:F3},{start.Y:F3}) " +
            //    $"angle={angleDegrees:F3}° " +
            //    $"→ ({bestPoint.X:F3},{bestPoint.Y:F3}) " +
            //    $"t={bestT:F3}");

            return bestPoint;
        }

        private List<XPoint> GetStartT1(XPoint _innerP, XPoint _outerP, List<XPoint> _outer, float angleDegrees,
        float prevangleDegrees, float nextangleDegrees, string stronaWModelu,
        string stonaOstanioDodanegoElementu, int nk)
        {
            List<XPoint> intersections = new List<XPoint>();

            bool czyParzysta = (nk + 1) % 2 == 0;

            bool warunek = false;

            if (nk <= 0)
            {
                warunek =
                    (stronaWModelu == "Dół" && stonaOstanioDodanegoElementu != "Góra")
                    || (stronaWModelu == "Góra" && ElementyRamyRysowane.Count == 0)
                    || (stronaWModelu == "Góra"
                        && stonaOstanioDodanegoElementu != "Góra"
                        && stonaOstanioDodanegoElementu != "Dół");
            }
            else if (nk > 0)
            {
                warunek = czyParzysta;
            }

            Console.WriteLine($"▶️ GetStartT1: stronaWModelu: {stronaWModelu}, stonaOstanioDodanegoElementu: {stonaOstanioDodanegoElementu}, nk: {nk}, czyParzysta: {czyParzysta} warunek: {warunek}");

            if (warunek)
            {
                var startT1 = FindFirstEdgeIntersectionByAngle(_innerP, angleDegrees - 180, _outer);

                XPoint p1 = startT1;
                XPoint p2 = _outerP;

                intersections.Add(new XPoint(p1.X, p1.Y));
                intersections.Add(new XPoint(p2.X, p2.Y));

                Console.WriteLine($"▶️ GetStartT1: OK");
            }
            else
            {
                var startT1 = FindFirstEdgeIntersectionByAngle(_innerP, prevangleDegrees, _outer);

                XPoint p1 = _innerP;
                XPoint p2 = startT1;
                intersections.Add(new XPoint(p1.X, p1.Y));
                intersections.Add(new XPoint(p2.X, p2.Y));

                Console.WriteLine($"▶️ GetStartT1: NOK");
            }

            return intersections;
        }
        private List<XPoint> GetEndT1(XPoint _innerP, XPoint _outerP, List<XPoint> _outer, float angleDegrees, float prevangleDegrees,
            float nextangleDegrees, string stronaWModelu,
            string stonaOstanioDodanegoElementu, int nk)
        {
            List<XPoint> intersections = new List<XPoint>();

            bool czyParzysta = (nk + 1) % 2 == 0;

            bool warunek = false;

            if (nk < 0)
            {
                warunek =
                 (stronaWModelu == "Góra" && ElementyRamyRysowane.Count == 0) || (stronaWModelu == "Góra" && _outer.Count() == 4) || stronaWModelu == "Dół"
                 || (stronaWModelu == "Lewa" && (ElementyRamyRysowane.Count > 0 && _outer.Count() < 4) && ElementyRamyRysowane[0].Strona == "Prawa")
                 || (stronaWModelu == "Góra" && ElementyRamyRysowane.Count > 0 && (ElementyRamyRysowane[0].Strona != "Dół" || _outer.Count() == 3));
            }
            else if (nk > 0)
            {
                warunek = czyParzysta;
            }

            Console.WriteLine($"▶️ GetEndT1: stronaWModelu: {stronaWModelu}, stonaOstanioDodanegoElementu: {stonaOstanioDodanegoElementu}, nk: {nk}, czyParzysta: {czyParzysta} warunek: {warunek}");

            if (warunek)
            {
                var startT1 = FindFirstEdgeIntersectionByAngle(_innerP, angleDegrees, _outer);

                XPoint p1 = startT1;
                XPoint p2 = _outerP;

                intersections.Add(new XPoint(p1.X, p1.Y));
                intersections.Add(new XPoint(p2.X, p2.Y));
            }
            else
            {
                var startT1 = FindFirstEdgeIntersectionByAngle(_innerP, nextangleDegrees - 180, _outer);

                XPoint p1 = _innerP;
                XPoint p2 = startT1;
                intersections.Add(new XPoint(p1.X, p1.Y));
                intersections.Add(new XPoint(p2.X, p2.Y));
            }

            return intersections;
        }

        private List<XPoint> GetStartT3(XPoint _innerP, XPoint _outerP, List<XPoint> _outer, float angleDegrees,
             float prevangleDegrees, float nextangleDegrees, string stronaWModelu,
             string stonaOstanioDodanegoElementu, int nk)
        {
            List<XPoint> intersections = new List<XPoint>();

            bool czyParzysta = (nk + 1) % 2 == 0;

            bool warunek = false;

            if (nk < 0)
            {
                warunek =
                (stronaWModelu == "Dół" && stonaOstanioDodanegoElementu != "Góra") || (stronaWModelu == "Góra" && ElementyRamyRysowane.Count == 0)
                || (stronaWModelu == "Góra" && stonaOstanioDodanegoElementu != "Góra" && stonaOstanioDodanegoElementu != "Dół");
            }
            else if (nk > 0)
            {
                warunek = czyParzysta;
            }

            if (warunek)
            {
                var startT1 = FindFirstEdgeIntersectionByAngle(_innerP, prevangleDegrees, _outer);

                XPoint p1 = _innerP;
                XPoint p2 = startT1;
                intersections.Add(new XPoint(p1.X, p1.Y));
                intersections.Add(new XPoint(p2.X, p2.Y));
            }
            else
            {
                var startT1 = FindFirstEdgeIntersectionByAngle(_innerP, angleDegrees - 180, _outer);

                XPoint p1 = startT1;
                XPoint p2 = _outerP;

                intersections.Add(new XPoint(p1.X, p1.Y));
                intersections.Add(new XPoint(p2.X, p2.Y));
            }

            return intersections;
        }
        private List<XPoint> GetEndT3(XPoint _innerP, XPoint _outerP, List<XPoint> _outer, float angleDegrees, float prevangleDegrees,
            float nextangleDegrees, string stronaWModelu,
            string stonaOstanioDodanegoElementu, int nk)
        {
            List<XPoint> intersections = new List<XPoint>();

            bool czyParzysta = (nk + 1) % 2 == 0;

            bool warunek = false;

            if (nk < 0)
            {
                warunek =
                (stronaWModelu == "Góra" && ElementyRamyRysowane.Count == 0) || stronaWModelu == "Dół"
                || (stronaWModelu == "Lewa" && ElementyRamyRysowane.Count > 0 && ElementyRamyRysowane[0].Strona == "Prawa")
                || (stronaWModelu == "Góra" && ElementyRamyRysowane.Count > 0 && ElementyRamyRysowane[0].Strona != "Dół");
            }
            else if (nk > 0)
            {
                warunek = czyParzysta;
            }

            if (warunek)
            {
                var startT1 = FindFirstEdgeIntersectionByAngle(_innerP, nextangleDegrees - 180, _outer);

                XPoint p1 = _innerP;
                XPoint p2 = startT1;
                intersections.Add(new XPoint(p1.X, p1.Y));
                intersections.Add(new XPoint(p2.X, p2.Y));
            }
            else
            {

                var startT1 = FindFirstEdgeIntersectionByAngle(_innerP, angleDegrees, _outer);

                XPoint p1 = startT1;
                XPoint p2 = _outerP;

                intersections.Add(new XPoint(p1.X, p1.Y));
                intersections.Add(new XPoint(p2.X, p2.Y));
            }

            return intersections;
        }


        private List<XPoint> GetStartT3Triangle(
        XPoint _innerP,
        XPoint _outerP,
        List<XPoint> _outer,
        float angleDegrees,
        float prevangleDegrees,
        float nextangleDegrees,
        string stronaWModelu,
        string stonaOstanioDodanegoElementu,
        int nk,
        int next,
        int prev)
        {
            List<XPoint> intersections = new List<XPoint>();

            bool czyParzysta = (nk + 1) % 2 == 0;

            string prevSide = StronaOknaHelper.OkreslStrone(
                prevangleDegrees,
                prev,
                _outer);

            string nextSide = StronaOknaHelper.OkreslStrone(
                nextangleDegrees,
                next,
                _outer);

            // ============================================================
            // T3
            //
            // W T3 DŁUŻSZE SĄ ELEMENTY PIONOWE:
            //
            //      LEWA       PRAWA
            //        │          │
            //        │          │
            //        │          │
            //
            // Natomiast GÓRA / DÓŁ pozostają elementami krótszymi,
            // znajdującymi się wewnątrz.
            // ============================================================

            bool pionowe =
                stronaWModelu == "Lewa" ||
                stronaWModelu == "Prawa";

            // ============================================================
            // Specjalne przypadki narożników
            //
            // Zachowujemy analogię do GetStartT1Triangle(),
            // ale odwracamy kierunek logiki.
            // ============================================================

            if (stronaWModelu == "Góra" && prevSide == "Lewa")
            {
                pionowe = false;
                czyParzysta = false;
            }

            if (stronaWModelu == "Dół" && prevSide == "Lewa")
            {
                pionowe = false;
                czyParzysta = false;
            }

            if (stronaWModelu == "Góra" && prevSide == "Dół")
            {
                pionowe = false;
                czyParzysta = true;
            }

            if (stronaWModelu == "Góra" && nextSide == "Góra")
            {
                czyParzysta = true;
            }

            // ============================================================
            // LEWA / PRAWA
            //
            // W T3 to właśnie te elementy mają być DŁUŻSZE.
            //
            // Dlatego ich końce dochodzą do zewnętrznego konturu.
            // ============================================================

            if (pionowe)
            {
                // --------------------------------------------------------
                // Punkt wewnętrzny -> szukamy przecięcia z outerContour
                //
                // Dla pionowego elementu kierunek wyznaczamy na podstawie
                // kąta elementu.
                // --------------------------------------------------------

                XPoint startT3 = FindTriangleEdgeIntersectionByAngle(
                    _innerP,
                    angleDegrees - 180.0,
                    _outer);

                intersections.Add(new XPoint(
                    startT3.X,
                    startT3.Y));

                // Drugi koniec już znajduje się na zewnętrznym konturze.
                intersections.Add(new XPoint(
                    _outerP.X,
                    _outerP.Y));

                return intersections;
            }

            // ============================================================
            // GÓRA / DÓŁ
            //
            // W T3 są to elementy KRÓTSZE.
            //
            // Nie wydłużamy ich do zewnętrznego konturu tak jak
            // elementów Lewa/Prawa.
            // ============================================================

            if (czyParzysta)
            {
                XPoint startT3 =
                    FindTriangleEdgeIntersectionByAngle(
                        _innerP,
                        angleDegrees - 180.0,
                        _outer);

                intersections.Add(new XPoint(
                    startT3.X,
                    startT3.Y));

                intersections.Add(new XPoint(
                    _outerP.X,
                    _outerP.Y));
            }
            else
            {
                XPoint startT3 =
                    FindTriangleEdgeIntersectionByAngle(
                        _innerP,
                        prevangleDegrees,
                        _outer);

                intersections.Add(new XPoint(
                    _innerP.X,
                    _innerP.Y));

                intersections.Add(new XPoint(
                    startT3.X,
                    startT3.Y));
            }

            return intersections;
        }

        private List<XPoint> GetEndT3Triangle(
         XPoint _innerP,
         XPoint _outerP,
         List<XPoint> _outer,
         float angleDegrees,
         float prevangleDegrees,
         float nextangleDegrees,
         string stronaWModelu,
         string stonaOstanioDodanegoElementu,
         int nk,
         int next,
        int prev)
        {
            List<XPoint> intersections = new List<XPoint>();

            string nextSide = StronaOknaHelper.OkreslStrone(
                nextangleDegrees,
                next,
                _outer);

            bool czyParzysta = (nk + 1) % 2 == 0;

            // Tak jak w GetEndT1Triangle:
            // obecnie logika parzystości jest wymuszona na false.
            czyParzysta = false;

            // ============================================================
            // T3
            //
            // W T3 DŁUŻSZE SĄ ELEMENTY PIONOWE:
            //
            //      LEWA       PRAWA
            //        |          |
            //        |          |
            //        |          |
            //
            // GÓRA / DÓŁ są krótsze.
            // ============================================================

            bool pionowe =
                stronaWModelu == "Lewa" ||
                stronaWModelu == "Prawa";

            // ============================================================
            // Dodatkowe wyjątki zachowane analogicznie do T1
            // ============================================================

            if (stronaWModelu == "Góra" &&
                nextSide == "Dół")
            {
                pionowe = false;
                czyParzysta = true;
            }

            if (stronaWModelu == "Lewa" &&
                nextSide == "Prawa" &&
                nextangleDegrees < 180.0)
            {
                pionowe = true;
            }


            //Console.WriteLine(
            //    $"🔺 GetEndT3Triangle " +
            //    $"strona={stronaWModelu} " +
            //    $"nk={nk} " +
            //    $"nextSide={nextSide} " +
            //    $"warunek={czyParzysta} " +
            //    $"pionowe={pionowe}");

            // ============================================================
            // LEWA / PRAWA
            //
            // T3 -> ZAWSZE DŁUŻSZE
            //
            // Koniec elementu musi dojść do OUTER CONTOUR.
            // ============================================================

            if (pionowe)
            {
                XPoint endT3 =
                    FindTriangleEdgeIntersectionByAngle(
                        _innerP,
                        angleDegrees,
                        _outer);

                intersections.Add(new XPoint(
                    endT3.X,
                    endT3.Y));

                intersections.Add(new XPoint(
                    _outerP.X,
                    _outerP.Y));

                Console.WriteLine(
                    $"🔺 T3 LEWA/PRAWA END LONG " +
                    $"P1=({endT3.X:F3},{endT3.Y:F3}) " +
                    $"P2=({_outerP.X:F3},{_outerP.Y:F3})");

                return intersections;
            }

            // ============================================================
            // GÓRA / DÓŁ
            //
            // T3 -> KRÓTSZE
            //
            // Nie wydłużamy ich specjalnie do outerContour.
            // Zachowujemy dotychczasową logikę drugiej gałęzi T1.
            // ============================================================

            if (czyParzysta)
            {
                XPoint endT3 =
                    FindTriangleEdgeIntersectionByAngle(
                        _innerP,
                        angleDegrees,
                        _outer);

                intersections.Add(new XPoint(
                    endT3.X,
                    endT3.Y));

                intersections.Add(new XPoint(
                    _outerP.X,
                    _outerP.Y));
            }
            else
            {
                XPoint endT3 =
                    FindTriangleEdgeIntersectionByAngle(
                        _innerP,
                        nextangleDegrees - 180.0,
                        _outer);

                intersections.Add(new XPoint(
                    _innerP.X,
                    _innerP.Y));

                intersections.Add(new XPoint(
                    endT3.X,
                    endT3.Y));
            }

            return intersections;
        }


        private List<XPoint> GetStartT2(XPoint _inner, XPoint _outer)
        {
            List<XPoint> intersections = new List<XPoint>();
            XPoint p1 = _inner;
            XPoint p2 = _outer;

            intersections.Add(new XPoint(p1.X, p1.Y));
            intersections.Add(new XPoint(p2.X, p2.Y));

            return intersections;
        }
        private List<XPoint> GetEndT2(XPoint inner, XPoint outer)
        {
            List<XPoint> intersections = new List<XPoint>();
            XPoint p1 = inner;
            XPoint p2 = outer;

            intersections.Add(new XPoint(p1.X, p1.Y));
            intersections.Add(new XPoint(p2.X, p2.Y));

            return intersections;
        }

        private List<XPoint> GetStartT4(XPoint _inner)
        {
            List<XPoint> intersections = new List<XPoint>();
            XPoint p1 = _inner;

            intersections.Add(new XPoint(p1.X, p1.Y));
            intersections.Add(new XPoint(p1.X, p1.Y));

            return intersections;
        }
        private List<XPoint> GetEndT4(XPoint _inner)
        {
            List<XPoint> intersections = new List<XPoint>();
            XPoint p1 = _inner;

            intersections.Add(new XPoint(p1.X, p1.Y));
            intersections.Add(new XPoint(p1.X, p1.Y));

            return intersections;
        }
        private async Task<float> ObliczRoznicePoziomow(KonfSystem? konf, bool slupekStaly)
        {
            if (konf == null)
            {
                BledySystemowe.Add($"Konfiguracja systemu jest pusta. Dotyczy funkcji ObliczRoznicePoziomow");
                return 0;
            }

            if (!slupekStaly)
            {
                float gora = (float)konf.PoziomGora;
                float dol = (float)konf.PoziomDol;

                // Jeśli jedno z pól jest 0, traktuj drugie jako wartość symetryczną
                if (gora == 0 && dol != 0)
                    return Math.Abs(dol);

                if (dol == 0 && gora != 0)
                    return Math.Abs(gora);

                await Task.CompletedTask;

                return Math.Abs(gora - dol);
            }
            else
            {
                //Słupki stałe mają zawsze pełną wartość profilu, niezależnie od poziomów pozostałe dane z tabeli KonfPolaczenia
                BledySystemowe.Add($"Słupki stałe mają zawsze pełną wartość profilu, niezależnie od poziomów pozostałe dane z tabeli KonfPolaczenia");
                await Task.CompletedTask;
                return 0;
            }

        }

        private async Task<float> ObliczRoznicePoziomowSzyba(KonfSystem? konf, bool slupekStaly)
        {
            if (konf == null || !konf.CzyMozeBycFix)
                return 0;

            if (!slupekStaly)
            {
                float gora = (float)konf.PoziomLiniaSzkla;
                float dol = (float)konf.PoziomDol;

                // Jeśli jedno z pól jest 0, traktuj drugie jako wartość symetryczną
                if (gora == 0 && dol != 0)
                    return Math.Abs(dol);

                if (dol == 0 && gora != 0)
                    return Math.Abs(gora);

                await Task.CompletedTask;

                return Math.Abs(gora - dol);
            }
            else
            {
                //Słupki stałe mają zawsze pełną wartość profilu, niezależnie od poziomów pozostałe dane z tabeli KonfPolaczenia
                //Console.BackgroundColor = ConsoleColor.Green;
                //Console.WriteLine("Słupki stałe mają zawsze pełną wartość profilu, niezależnie od poziomów pozostałe dane z tabeli KonfPolaczenia");
                BledySystemowe.Add($"Dla słupków stałych nie wyszukano w tabeli KonfPolaczenia wartość LINIA SZKLENIA");
                await Task.CompletedTask;
                return 0;
            }

        }

        private async Task<float> ObliczRoznicePoziomowKorpusWewnetrzny(KonfSystem? konf)
        {
            if (konf == null)
            {
                BledySystemowe.Add($"Konfiguracja systemu jest pusta. Dotyczy funkcji ObliczRoznicePoziomowKorpusWewnetrzny");
                return 0;
            }


            float gora = (float)konf.PoziomKorpus;
            float dol = (float)konf.PoziomDol;

            //Console.WriteLine($"ObliczRoznicePoziomowKorpusWewnetrzny   gora: {gora} dol:{dol}");
            // Jeśli jedno z pól jest 0, traktuj drugie jako wartość symetryczną
            if (gora == 0 && dol != 0)
                return Math.Abs(dol);

            if (dol == 0 && gora != 0)
                return Math.Abs(gora);

            await Task.CompletedTask;

            return Math.Abs(gora - dol);

        }

        /// <summary>
        /// Zwraca kąt górnej krawędzi w stopniach (0 = poziomo, 90 = pionowo)
        /// </summary>
        public static float GetTopEdgeAngleFromFirstSegment(List<XPoint> outer)
        {
            if (outer == null || outer.Count < 2)
                throw new ArgumentException("Lista punktów musi mieć co najmniej 2 elementy.");

            var p1 = outer[0]; // lewy
            var p2 = outer[1]; // prawy

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;

            if (angle < 0) angle += 360;

            return (float)angle;
        }

        private List<XPoint> RemoveDuplicateConsecutivePoints(List<XPoint> points)
        {
            var unique = new List<XPoint>();
            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0 || !ArePointsEqual(points[i], points[i - 1]))
                {
                    unique.Add(points[i]);
                }
            }

            // Jeśli pierwszy == ostatni — zamknięcie konturu — usuń ostatni
            if (unique.Count > 2 && ArePointsEqual(unique.First(), unique.Last()))
            {
                unique.RemoveAt(unique.Count - 1);
            }

            return unique;
        }
        private bool ArePointsEqual(XPoint p1, XPoint p2)
        {
            return Math.Abs(p1.X - p2.X) < 0.1 && Math.Abs(p1.Y - p2.Y) < 0.1;
        }

        /// <summary>
        /// Znajduje pierwsze przecięcie linii (wyznaczonej przez wektor direction) z wielokątem contour,
        /// startując od point basePoint.
        /// </summary>
        private XPoint FindFirstEdgeIntersectionByVector(
         XPoint basePoint,
         XPoint dirStart,
         XPoint dirEnd,
         List<XPoint> polygon,
         bool forward = true,
         double tolerance = 0.01)
        {
            double dx = dirEnd.X - dirStart.X;
            double dy = dirEnd.Y - dirStart.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 1e-8)
            {
                // Jeśli linia jest punktem, użyj kierunku pionowego
                dx = 0;
                dy = forward ? 1 : -1;
            }
            else
            {
                dx /= len;
                dy /= len;

                if (!forward)
                {
                    dx = -dx;
                    dy = -dy;
                }
            }

            XPoint? closest = null;
            double minDistSq = double.MaxValue;

            for (int i = 0; i < polygon.Count; i++)
            {
                int next = (i + 1) % polygon.Count;

                var inter = GetLinesIntersectionNullable(
                    basePoint,
                    new XPoint(basePoint.X + dx * 10000.0, basePoint.Y + dy * 10000.0),
                    polygon[i],
                    polygon[next]
                );

                if (!inter.HasValue)
                {
                    // Sprawdź punkty końcowe krawędzi
                    foreach (var pt in new[] { polygon[i], polygon[next] })
                    {
                        // Sprawdź czy punkt leży na linii prostej (w przybliżeniu)
                        double cross = Math.Abs((pt.X - basePoint.X) * dy - (pt.Y - basePoint.Y) * dx);
                        if (cross > tolerance) continue;

                        // Sprawdź czy punkt jest w odpowiednim kierunku
                        double dot = (pt.X - basePoint.X) * dx + (pt.Y - basePoint.Y) * dy;
                        if (dot >= -tolerance)
                        {
                            double distSq = (pt.X - basePoint.X) * (pt.X - basePoint.X) +
                                            (pt.Y - basePoint.Y) * (pt.Y - basePoint.Y);
                            if (distSq < minDistSq && distSq > tolerance)
                            {
                                minDistSq = distSq;
                                closest = pt;
                            }
                        }
                    }
                    continue;
                }

                var p = inter.Value;

                // Sprawdź czy punkt przecięcia leży w odpowiednim kierunku
                double dotInter = (p.X - basePoint.X) * dx + (p.Y - basePoint.Y) * dy;
                if (dotInter < -tolerance) continue;

                // Oblicz odległość
                double distSqInter = (p.X - basePoint.X) * (p.X - basePoint.X) +
                                     (p.Y - basePoint.Y) * (p.Y - basePoint.Y);

                // Ignoruj punkty zbyt blisko basePoint
                if (distSqInter < tolerance) continue;

                if (distSqInter < minDistSq)
                {
                    minDistSq = distSqInter;
                    closest = p;
                }
            }

            if (!closest.HasValue)
            {
                // Zwróć punkt z informacją, że nie znaleziono przecięcia
                return new XPoint { X = -1, Y = -1 };
            }

            return closest.Value;
        }
        // Tworzy offset zamkniętego konturu do środka o zadaną wartość

        private XPoint FindFirstEdgeIntersectionByAngle(
        XPoint origin,
        float angleDegrees,
        List<XPoint> contour)
        {
            double angleRad = angleDegrees * Math.PI / 180.0;

            float dx = (float)Math.Cos(angleRad);
            float dy = (float)Math.Sin(angleRad);

            XPoint? closest = null;
            float minDistSq = float.MaxValue;

            for (int i = 0; i < contour.Count; i++)
            {
                int next = (i + 1) % contour.Count;

                XPoint? inter = GetLinesIntersectionNullable(
                    origin,
                    new XPoint(origin.X + dx * 10000f, origin.Y + dy * 10000f),
                    contour[i],
                    contour[next]
                );

                if (!inter.HasValue)
                    continue;

                var p = inter.Value;

                // 🔥 KLUCZOWE — sprawdź czy punkt jest w kierunku promienia
                double dot = (p.X - origin.X) * dx + (p.Y - origin.Y) * dy;

                if (dot <= 0) // punkt za plecami
                    continue;

                double distSq =
                    (p.X - origin.X) * (p.X - origin.X) +
                    (p.Y - origin.Y) * (p.Y - origin.Y);

                if (distSq < minDistSq)
                {
                    minDistSq = (float)distSq;
                    closest = p;
                }
            }

            return closest ?? origin;
        }

        private List<XPoint> GetRayCircleIntersections(XPoint origin, double dx, double dy, XPoint center, double radius)
        {
            var result = new List<XPoint>();
            double fx = origin.X - center.X;
            double fy = origin.Y - center.Y;
            double a = dx * dx + dy * dy; // = 1
            double b = 2 * (fx * dx + fy * dy);
            double c = fx * fx + fy * fy - radius * radius;
            double disc = b * b - 4 * a * c;
            if (disc < -1e-9) return result;
            disc = Math.Max(0, disc);
            double sqrtDisc = Math.Sqrt(disc);
            double t1 = (-b - sqrtDisc) / (2 * a);
            double t2 = (-b + sqrtDisc) / (2 * a);
            if (t1 >= 0) result.Add(new XPoint(origin.X + t1 * dx, origin.Y + t1 * dy));
            if (t2 >= 0 && Math.Abs(t2 - t1) > 1e-9) result.Add(new XPoint(origin.X + t2 * dx, origin.Y + t2 * dy));
            return result;
        }

        /// <summary>
        /// Znajduje pierwsze przecięcie promienia (punkt + kąt) z konturem złożonym z segmentów (linie i łuki).
        /// Zwraca punkt przecięcia oraz segment (jeśli przecięcie jest z łukiem, zwraca ten łuk; dla linii zwraca null).
        /// </summary>
        private (XPoint intersection, ContourSegment? segment) FindIntersectionByAngleWithSegments(
            XPoint origin,
            double angleDegrees,
            List<ContourSegment> contour,
            double maxDistance = 10000.0)
        {
            if (contour == null || contour.Count == 0)
                return (origin, null);

            double angleRad = angleDegrees * Math.PI / 180.0;
            double dx = Math.Cos(angleRad);
            double dy = Math.Sin(angleRad);

            XPoint? closestPoint = null;
            ContourSegment? closestSegment = null;
            double minDistSq = double.MaxValue;

            // Przygotuj punkt końcowy promienia
            XPoint endPoint = new XPoint(origin.X + dx * maxDistance, origin.Y + dy * maxDistance);

            foreach (var seg in contour)
            {
                if (seg.Type == SegmentType.Line)
                {
                    var inter = GetLinesIntersectionNullable(origin, endPoint, seg.Start, seg.End);
                    if (!inter.HasValue)
                        continue;

                    var p = inter.Value;
                    double dot = (p.X - origin.X) * dx + (p.Y - origin.Y) * dy;
                    if (dot <= 0) continue; // za plecami

                    double distSq = (p.X - origin.X) * (p.X - origin.X) + (p.Y - origin.Y) * (p.Y - origin.Y);
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        closestPoint = p;
                        closestSegment = null; // linia, więc segment = null
                    }
                }
                else if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    // Przecięcie promienia z okręgiem
                    var intersections = GetRayCircleIntersections(origin, dx, dy, seg.Center.Value, seg.Radius);
                    foreach (var p in intersections)
                    {
                        // Sprawdź, czy punkt leży na łuku (nie tylko na okręgu)
                        if (!IsPointOnArc(p, seg, 0.1))
                            continue;

                        double dot = (p.X - origin.X) * dx + (p.Y - origin.Y) * dy;
                        if (dot <= 0) continue;

                        double distSq = (p.X - origin.X) * (p.X - origin.X) + (p.Y - origin.Y) * (p.Y - origin.Y);
                        if (distSq < minDistSq)
                        {
                            minDistSq = distSq;
                            closestPoint = p;
                            closestSegment = seg; // zapamiętujemy segment łuku
                        }
                    }
                }
            }

            return (closestPoint ?? origin, closestSegment);
        }

        private XPoint FindFirstEdgeIntersection(XPoint origin, float dx, float dy, List<XPoint> contour)
        {
            XPoint? closest = null;
            float minDist = float.MaxValue;

            // Console.WriteLine($"🔷 Finding first edge intersection from origin X:{origin.X} Y:{origin.Y} with direction dx:{dx}, dy:{dy}");

            for (int i = 0; i < contour.Count; i++)
            {
                int next = (i + 1) % contour.Count;

                XPoint? inter = GetLinesIntersectionNullable(
                    origin,
                    new XPoint(origin.X + dx * 10000, origin.Y + dy * 10000),
                    contour[i], contour[next]);

                if (!inter.HasValue) continue;

                float distSq = (float)((inter.Value.X - origin.X) * (inter.Value.X - origin.X) +
                                       (inter.Value.Y - origin.Y) * (inter.Value.Y - origin.Y));
                if (distSq < minDist)
                {
                    minDist = distSq;
                    closest = inter;
                }
            }

            return closest ?? origin;
        }

        private XPoint GetHorizontalIntersection(XPoint a, XPoint b, float y)
        {
            if (Math.Abs(a.Y - b.Y) < 1e-3f)
                return new XPoint(a.X, y);

            float t = (y - (float)a.Y) / ((float)b.Y - (float)a.Y);
            float x = (float)a.X + t * ((float)b.X - (float)a.X);
            return new XPoint(x, y);
        }

        public async Task<List<XPoint>> CalculateOffsetPolygon(
        List<XPoint> points,
        float profileLeft,
        float profileRight,
        float profileTop,
        float profileBottom,
        bool elementLiniowy)
        {
            int count = points.Count;

            //if (count > 0)
            //    Console.WriteLine($"🔷CalculateOffsetPolygon Calculating offset polygon for {count} X:{points[0].X} Y:{points[0].Y} elementLiniowy:{elementLiniowy} points with profiles L:{profileLeft}, R:{profileRight}, T:{profileTop}, B:{profileBottom}");

            if (count < 2)
            {
                Komunikaty.Add("Figura musi mieć co najmniej 2 punkty.");
                return points; // Nie można utworzyć wielokąta z mniej niż 2 punktów, zwróć oryginalne punkty
            }
            //  throw new ArgumentException("Figura musi mieć co najmniej 2 punkty.");

            // 🟢 OBSŁUGA ELEMENTÓW LINIOWYCH (np. słupków)
            if (elementLiniowy)
            {
                var p1 = points[0];
                var p2 = points[1];

                float dx = (float)(p2.X - p1.X);
                float dy = (float)(p2.Y - p1.Y);
                double length = Math.Sqrt(dx * dx + dy * dy);
                if (length < 1e-6f) return points;

                // jednostkowy wektor kierunku i normalna
                float tx = (float)(dx / length);
                float ty = (float)(dy / length);
                float nx = -ty;
                float ny = tx;

                // 🔥 OKREŚLENIE STRONY NA PODSTAWIE KĄTA
                float angleRadians = MathF.Atan2(dy, dx);
                float angleDegrees = angleRadians * (180f / MathF.PI);
                if (angleDegrees < 0) angleDegrees += 360f;

                string side = StronaOknaHelper.OkreslStrone(angleDegrees, 0, points);

                float offsetX = 0f;
                float offsetY = 0f;

                // 🔥 POPRAWIONE: Przesunięcie DO WNĘTRZA
                // Dla elementów liniowych przesuwamy cały odcinek
                switch (side)
                {
                    case "Góra":
                        // Góra przesuwa się w dół (ujemny Y)
                        offsetY = -profileTop;
                        break;
                    case "Dół":
                        // Dół przesuwa się w górę (dodatni Y)
                        offsetY = profileBottom;
                        break;
                    case "Lewa":
                        // Lewa przesuwa się w prawo (dodatni X)
                        offsetX = profileLeft;
                        break;
                    case "Prawa":
                        // Prawa przesuwa się w lewo (ujemny X)
                        offsetX = -profileRight;
                        break;
                }

                // Console.WriteLine($"🔷CalculateOffsetPolygon Element liniowy: strona {side}, offsetX={offsetX}, offsetY={offsetY}");

                var p1Offset = new XPoint(p1.X + offsetX, p1.Y + offsetY);
                var p2Offset = new XPoint(p2.X + offsetX, p2.Y + offsetY);

                return new List<XPoint> { p1Offset, p2Offset };
            }

            // 🟢 OBSŁUGA WIELOKĄTA
            if (count < 3)
            {
                Komunikaty.Add("Wielokąt musi mieć co najmniej 3 punkty.");
                return points; // Nie można utworzyć wielokąta z mniej niż 2 punktów, zwróć oryginalne punkty
            }
            //throw new ArgumentException("Wielokąt musi mieć co najmniej 3 punkty.");

            // Krok 1: Dla każdego boku określamy jego stronę i odpowiedni profil
            var offsetLines = new List<(XPoint p1, XPoint p2, string side, float offset)>();

            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                var p1 = points[i];
                var p2 = points[next];

                float dx = (float)(p2.X - p1.X);
                float dy = (float)(p2.Y - p1.Y);
                float length = MathF.Sqrt(dx * dx + dy * dy);
                if (length < 1e-6f) continue;

                // 🔥 OKREŚLENIE STRONY NA PODSTAWIE KĄTA
                float angleRadians = MathF.Atan2(dy, dx);
                float angleDegrees = angleRadians * (180f / MathF.PI);
                if (angleDegrees < 0) angleDegrees += 360f;

                string side = StronaOknaHelper.OkreslStrone(angleDegrees, i, points);

                // Standardowa normalna (obrót o -90 stopni)
                // Oblicz wektor kierunku i normalną
                float tx = dx / length;
                float ty = dy / length;

                // 🔥 POPRAWIONA NORMALNA dla wielokąta CW (zgodnego z ruchem wskazówek zegara)
                // Dla CW: normalna zewnętrzna = (ty, -tx)
                float nx = ty;
                float ny = -tx;

                // Console.WriteLine($"🔷CalculateOffsetPolygon Bok {i}: kierunek ({tx:F2}, {ty:F2}), normalna zewnętrzna ({nx:F2}, {ny:F2})");

                // Określenie znaku offsetu
                float offsetValue = 0f;
                bool usePositiveNormal = true; // true = używamy normalnej, false = używamy przeciwnej

                switch (side)
                {
                    case "Góra":
                        // Góra - chcemy przesunąć w dół (zgodnie z normalną zewnętrzną)
                        offsetValue = profileTop;
                        usePositiveNormal = false; // używamy normalnej (w dół)
                        break;
                    case "Dół":
                        // Dół - chcemy przesunąć w górę (przeciwnie do normalnej zewnętrznej)
                        offsetValue = profileBottom;
                        usePositiveNormal = false; // używamy przeciwnej normalnej (w górę)
                        break;
                    case "Lewa":
                        // Lewa - chcemy przesunąć w prawo (przeciwnie do normalnej zewnętrznej)
                        offsetValue = profileLeft;
                        usePositiveNormal = false; // normalna zewnętrzna dla lewej to w lewo, więc do środka potrzeba przeciwną
                        break;
                    case "Prawa":
                        // Prawa - chcemy przesunąć w lewo (zgodnie z normalną zewnętrzną)
                        offsetValue = profileRight;
                        usePositiveNormal = false; // normalna zewnętrzna dla prawej to w lewo
                        break;
                }

                float offset = usePositiveNormal ? offsetValue : -offsetValue;

                // Przesunięcie boku
                var p1Offset = new XPoint(p1.X + nx * offset, p1.Y + ny * offset);
                var p2Offset = new XPoint(p2.X + nx * offset, p2.Y + ny * offset);

                // Console.WriteLine($"🔷CalculateOffsetPolygon Bok {i}: strona {side}, offsetValue={offsetValue}, usePositiveNormal={usePositiveNormal}, offset={offset}");


                offsetLines.Add((p1Offset, p2Offset, side, offset));
            }

            // Krok 2: Znajdź przecięcia przesuniętych boków
            var result = new List<XPoint>();
            for (int i = 0; i < offsetLines.Count; i++)
            {
                var (a1, a2, sideA, offsetA) = offsetLines[i];
                var (b1, b2, sideB, offsetB) = offsetLines[(i - 1 + offsetLines.Count) % offsetLines.Count];

                var intersection = GetLinesIntersection(a1, a2, b1, b2);

                if (float.IsNaN((float)intersection.X) || float.IsNaN((float)intersection.Y))
                {
                    // Jeśli nie ma przecięcia, weź środek odcinka między punktami
                    intersection = new XPoint((a1.X + b1.X) / 2f, (a1.Y + b1.Y) / 2f);
                }

                result.Add(intersection);
            }

            // Krok 3: Sprawdź czy wielokąt jest odwrócony (opcjonalnie)
            // Jeśli powstały wielokąt ma większe pole niż oryginał, znaczy że offset poszedł na zewnątrz
            // Można dodać logikę odwracania znaków jeśli to konieczne

            //foreach (var pt in result)
            //{
            //    Console.WriteLine($"🔷CalculateOffsetPolygon Calculated offset polygon point: X={pt.X}, Y={pt.Y}");
            //}

            await Task.CompletedTask; // symulacja asynchroniczności

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------


        public async Task<List<ContourSegment>> CalculateOffsetPolygonKontur(
        List<ContourSegment> segments,
        float profileLeft,
        float profileRight,
        float profileTop,
        float profileBottom,
        bool elementLiniowy)
        {
            if (segments == null || segments.Count == 0)
                return new List<ContourSegment>();

            const double EPS = 1e-6;
            const double TOLERANCJA = 0.01;

            // 🟢 OBSŁUGA ELEMENTÓW LINIOWYCH (np. słupków)
            if (elementLiniowy && segments.Count == 2)
            {
                var seg1 = segments[0];
                var seg2 = segments[1];

                // Zakładamy, że element liniowy składa się z dwóch segmentów liniowych
                if (seg1.Type == SegmentType.Line && seg2.Type == SegmentType.Line)
                {
                    // Weź punkty początkowe obu segmentów (lub początek pierwszego i koniec drugiego)
                    var p1 = seg1.Start;
                    var p2 = seg2.End; // lub seg1.End i seg2.Start, zależnie od struktury

                    double dx = p2.X - p1.X;
                    double dy = p2.Y - p1.Y;
                    double length = Math.Sqrt(dx * dx + dy * dy);

                    if (length > EPS)
                    {
                        // Określenie kąta i strony
                        double angleRadians = Math.Atan2(dy, dx);
                        double angleDegrees = angleRadians * (180.0 / Math.PI);
                        if (angleDegrees < 0) angleDegrees += 360.0;

                        string side = StronaOknaHelper.OkreslStrone((float)angleDegrees, 0, null);

                        double offsetX = 0;
                        double offsetY = 0;

                        // Przesunięcie DO WNĘTRZA - analogicznie jak w CalculateOffsetPolygon
                        switch (side)
                        {
                            case "Góra":
                                // Góra przesuwa się w dół (ujemny Y)
                                offsetY = -profileTop;
                                break;
                            case "Dół":
                                // Dół przesuwa się w górę (dodatni Y)
                                offsetY = profileBottom;
                                break;
                            case "Lewa":
                                // Lewa przesuwa się w prawo (dodatni X)
                                offsetX = profileLeft;
                                break;
                            case "Prawa":
                                // Prawa przesuwa się w lewo (ujemny X)
                                offsetX = -profileRight;
                                break;
                        }

                        // Tworzymy przesunięte segmenty
                        var newSeg1Start = new XPoint(seg1.Start.X + offsetX, seg1.Start.Y + offsetY);
                        var newSeg1End = new XPoint(seg1.End.X + offsetX, seg1.End.Y + offsetY);
                        var newSeg2Start = new XPoint(seg2.Start.X + offsetX, seg2.Start.Y + offsetY);
                        var newSeg2End = new XPoint(seg2.End.X + offsetX, seg2.End.Y + offsetY);

                        var resultX = new List<ContourSegment>
                {
                    new ContourSegment(newSeg1Start, newSeg1End)
                    {
                        Informacja = seg1.Informacja ?? side
                    },
                    new ContourSegment(newSeg2Start, newSeg2End)
                    {
                        Informacja = seg2.Informacja ?? side
                    }
                };

                        return resultX;
                    }
                }
                else
                {
                    // Jeśli segmenty nie są liniowe, zwróć oryginał
                    //Console.WriteLine("⚠️ Element liniowy z niestandardowymi segmentami - zwracam oryginał");
                    BledySystemowe.Add("Element liniowy z niestandardowymi segmentami - zwracam oryginał");
                    return segments;
                }
            }

            var offsetSegments = new List<ContourSegment>();
            var arcRadiusCache = new Dictionary<string, float>();

            bool isFullCircle = segments.All(s => s.Type == SegmentType.Arc);

            var bboxCenter = new XPoint(
                segments.Average(s => (s.Start.X + s.End.X) / 2.0),
                segments.Average(s => (s.Start.Y + s.End.Y) / 2.0)
            );

            // OFFSET SEGMENTÓW
            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];

                double dx = seg.End.X - seg.Start.X;
                double dy = seg.End.Y - seg.Start.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);
                if (length < EPS) continue;

                float angleDegrees = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                if (angleDegrees < 0) angleDegrees += 360f;

                string side = StronaOknaHelper.OkreslStrone(angleDegrees, i, null);

                float offsetValue = side switch
                {
                    "Góra" => profileTop,
                    "Dół" => profileBottom,
                    "Lewa" => profileLeft,
                    "Prawa" => profileRight,
                    _ => 0
                };

                if (seg.Type == SegmentType.Line)
                {
                    double tx = dx / length;
                    double ty = dy / length;
                    double nx = ty;
                    double ny = -tx;

                    var midpoint = new XPoint(
                        (seg.Start.X + seg.End.X) / 2.0,
                        (seg.Start.Y + seg.End.Y) / 2.0
                    );

                    var testA = new XPoint(midpoint.X + nx * offsetValue, midpoint.Y + ny * offsetValue);
                    var testB = new XPoint(midpoint.X - nx * offsetValue, midpoint.Y - ny * offsetValue);

                    double da = DistanceSquared(testA, bboxCenter);
                    double db = DistanceSquared(testB, bboxCenter);
                    double sign = da < db ? 1 : -1;

                    var p1 = new XPoint(seg.Start.X + nx * offsetValue * sign, seg.Start.Y + ny * offsetValue * sign);
                    var p2 = new XPoint(seg.End.X + nx * offsetValue * sign, seg.End.Y + ny * offsetValue * sign);

                    p1 = SnapPoint(p1);
                    p2 = SnapPoint(p2);

                    offsetSegments.Add(new ContourSegment(p1, p2)
                    {
                        Informacja = seg.Informacja ?? side
                    });
                }
                else if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    var center = seg.Center.Value;

                    string arcKey = $"{Math.Round(center.X, 3)}_{Math.Round(center.Y, 3)}_{Math.Round(seg.Radius, 3)}";

                    if (!arcRadiusCache.ContainsKey(arcKey))
                    {
                        arcRadiusCache[arcKey] = (float)(seg.Radius - offsetValue);
                    }

                    float newRadius = arcRadiusCache[arcKey];
                    if (newRadius < 0.1f) newRadius = 0.1f;

                    double startAngle = Math.Atan2(seg.Start.Y - center.Y, seg.Start.X - center.X);
                    double endAngle = Math.Atan2(seg.End.Y - center.Y, seg.End.X - center.X);

                    var newStart = new XPoint(
                        center.X + newRadius * Math.Cos(startAngle),
                        center.Y + newRadius * Math.Sin(startAngle)
                    );

                    var newEnd = new XPoint(
                        center.X + newRadius * Math.Cos(endAngle),
                        center.Y + newRadius * Math.Sin(endAngle)
                    );

                    newStart = SnapPoint(newStart);
                    newEnd = SnapPoint(newEnd);

                    // 🔑 ZAWSZE USTAW CCW = TRUE
                    offsetSegments.Add(new ContourSegment(newStart, newEnd, center, newRadius, true)
                    {
                        Informacja = seg.Informacja ?? (isFullCircle ? "ARC_FULL_CIRCLE" : side)
                    });
                }
            }

            // PRZECIĘCIA
            var result = new List<ContourSegment>();

            for (int i = 0; i < offsetSegments.Count; i++)
            {
                var current = offsetSegments[i];
                var previous = offsetSegments[(i - 1 + offsetSegments.Count) % offsetSegments.Count];

                XPoint? intersection = null;

                if (current.Type == SegmentType.Line && previous.Type == SegmentType.Line)
                {
                    intersection = GetLinesIntersectionK(previous.Start, previous.End, current.Start, current.End);
                }
                else if (previous.Type == SegmentType.Line && current.Type == SegmentType.Arc && current.Center != null)
                {
                    var pts = GetLineCircleIntersections(previous.Start, previous.End, current.Center.Value, current.Radius);
                    intersection = ChooseClosestTo(pts, current.Start);
                }
                else if (previous.Type == SegmentType.Arc && previous.Center != null && current.Type == SegmentType.Line)
                {
                    var pts = GetLineCircleIntersections(current.Start, current.End, previous.Center.Value, previous.Radius);
                    intersection = ChooseClosestTo(pts, current.Start);
                }
                else if (previous.Type == SegmentType.Arc && current.Type == SegmentType.Arc &&
                         previous.Center != null && current.Center != null)
                {
                    var pts = GetCircleCircleIntersections(previous.Center.Value, previous.Radius, current.Center.Value, current.Radius);
                    intersection = ChooseClosestTo(pts, current.Start);
                }

                if (intersection != null && !double.IsNaN(intersection.Value.X))
                {
                    if (result.Count > 0)
                    {
                        result[^1].End = intersection.Value;
                    }

                    if (current.Type == SegmentType.Arc && current.Center != null)
                    {
                        // 🔑 ZAWSZE CCW = TRUE
                        result.Add(new ContourSegment(intersection.Value, current.End, current.Center, current.Radius, true)
                        {
                            Informacja = current.Informacja
                        });
                    }
                    else
                    {
                        result.Add(new ContourSegment(intersection.Value, current.End)
                        {
                            Informacja = current.Informacja
                        });
                    }
                }
                else
                {
                    if (result.Count > 0)
                    {
                        var srodek = new XPoint(
                            (result[^1].End.X + current.Start.X) / 2.0,
                            (result[^1].End.Y + current.Start.Y) / 2.0
                        );
                        result[^1].End = srodek;
                        current.Start = srodek;
                    }
                    result.Add(current);
                }
            }

            // 🔑 SPRAWDŹ CZY CAŁY KONTUR JEST CCW
            if (result.Count > 0)
            {
                // Oblicz pole konturu (dodatnie = CCW, ujemne = CW)
                double pole = 0;
                for (int i = 0; i < result.Count; i++)
                {
                    var current = result[i];
                    var next = result[(i + 1) % result.Count];

                    // Użyj punktów Start każdego segmentu
                    pole += (current.Start.X * next.Start.Y) - (next.Start.X * current.Start.Y);
                }
                pole /= 2.0;

                // Jeśli kontur jest CW (pole ujemne), odwróć wszystkie segmenty
                if (pole < 0)
                {
                    // Odwróć kolejność segmentów
                    result.Reverse();

                    // Dla każdego segmentu zamień Start z End
                    for (int i = 0; i < result.Count; i++)
                    {
                        var temp = result[i].Start;
                        result[i].Start = result[i].End;
                        result[i].End = temp;

                        // Dla łuków - zachowaj CCW = true (już jest ustawione)
                    }
                }
            }

            // ZAMKNIĘCIE
            if (result.Count > 0)
            {
                var firstStart = result[0].Start;
                var lastEnd = result[^1].End;

                double odleglosc = Math.Sqrt(Math.Pow(lastEnd.X - firstStart.X, 2) +
                                             Math.Pow(lastEnd.Y - firstStart.Y, 2));

                if (odleglosc > TOLERANCJA)
                {
                    result[^1].End = result[0].Start;
                }
            }

            await Task.CompletedTask; // symulacja asynchroniczności

            return result;
        }

        public async Task<List<ContourSegment>> CalculateOffsetPolygonKonturSkrzydlo(
           List<ContourSegment> segments,
           float profileLeft,
           float profileRight,
           float profileTop,
           float profileBottom,
           bool elementLiniowy)
        {
            if (segments == null || segments.Count == 0)
                return new List<ContourSegment>();

            //Console.WriteLine($"🔷CalculateOffsetPolygonKonturSkrzydlo Calculating offset for {segments.Count} segments elementLiniowy:{elementLiniowy} with profiles L:{profileLeft}, R:{profileRight}, T:{profileTop}, B:{profileBottom}");

            const double EPS = 1e-6;
            const double TOLERANCJA = 0.01;

            // 🟢 OBSŁUGA ELEMENTÓW LINIOWYCH (np. słupków)
            if (elementLiniowy && segments.Count == 2)
            {
                var seg1 = segments[0];
                var seg2 = segments[1];

                if (seg1.Type == SegmentType.Line && seg2.Type == SegmentType.Line)
                {
                    var p1 = seg1.Start;
                    var p2 = seg2.End;

                    double dx = p2.X - p1.X;
                    double dy = p2.Y - p1.Y;
                    double length = Math.Sqrt(dx * dx + dy * dy);

                    if (length > EPS)
                    {
                        double angleRadians = Math.Atan2(dy, dx);
                        double angleDegrees = angleRadians * (180.0 / Math.PI);
                        if (angleDegrees < 0) angleDegrees += 360.0;

                        string side = StronaOknaHelper.OkreslStrone((float)angleDegrees, 0, null);

                        double offsetX = 0;
                        double offsetY = 0;

                        // Przesunięcie z uwzględnieniem znaku profilu
                        // Ujemny profil = offset na zewnątrz (odwrotny kierunek)
                        switch (side)
                        {
                            case "Góra":
                                offsetY = -profileTop; // Ujemny profileTop da dodatni offsetY (w górę)
                                break;
                            case "Dół":
                                offsetY = profileBottom; // Ujemny profileBottom da ujemny offsetY (w dół)
                                break;
                            case "Lewa":
                                offsetX = profileLeft; // Ujemny profileLeft da ujemny offsetX (w lewo)
                                break;
                            case "Prawa":
                                offsetX = -profileRight; // Ujemny profileRight da dodatni offsetX (w prawo)
                                break;
                        }

                        //Console.WriteLine($"🔷CalculateOffsetPolygonKonturSkrzydlo Element liniowy: strona {side}, offsetX={offsetX}, offsetY={offsetY}");

                        var newSeg1Start = new XPoint(seg1.Start.X + offsetX, seg1.Start.Y + offsetY);
                        var newSeg1End = new XPoint(seg1.End.X + offsetX, seg1.End.Y + offsetY);
                        var newSeg2Start = new XPoint(seg2.Start.X + offsetX, seg2.Start.Y + offsetY);
                        var newSeg2End = new XPoint(seg2.End.X + offsetX, seg2.End.Y + offsetY);

                        var resultX = new List<ContourSegment>
                {
                    new ContourSegment(newSeg1Start, newSeg1End)
                    {
                        Informacja = seg1.Informacja ?? side
                    },
                    new ContourSegment(newSeg2Start, newSeg2End)
                    {
                        Informacja = seg2.Informacja ?? side
                    }
                };

                        return resultX;
                    }
                }
                else
                {
                    //Console.WriteLine("⚠️ CalculateOffsetPolygonKonturSkrzydlo: Element liniowy z niestandardowymi segmentami - zwracam oryginał");
                    BledySystemowe.Add("CalculateOffsetPolygonKonturSkrzydlo: Element liniowy z niestandardowymi segmentami - zwracam oryginał");
                    return segments;
                }
            }

            var offsetSegments = new List<ContourSegment>();
            var arcRadiusCache = new Dictionary<string, float>();

            bool isFullCircle = segments.All(s => s.Type == SegmentType.Arc);

            var bboxCenter = new XPoint(
                segments.Average(s => (s.Start.X + s.End.X) / 2.0),
                segments.Average(s => (s.Start.Y + s.End.Y) / 2.0)
            );

            // OFFSET SEGMENTÓW
            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];

                double dx = seg.End.X - seg.Start.X;
                double dy = seg.End.Y - seg.Start.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);
                if (length < EPS) continue;

                float angleDegrees = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                if (angleDegrees < 0) angleDegrees += 360f;

                string side = StronaOknaHelper.OkreslStrone(angleDegrees, i, null);

                float offsetValue = side switch
                {
                    "Góra" => profileTop,
                    "Dół" => profileBottom,
                    "Lewa" => profileLeft,
                    "Prawa" => profileRight,
                    _ => 0
                };

                if (seg.Type == SegmentType.Line)
                {
                    double tx = dx / length;
                    double ty = dy / length;
                    double nx = ty;
                    double ny = -tx;

                    var midpoint = new XPoint(
                        (seg.Start.X + seg.End.X) / 2.0,
                        (seg.Start.Y + seg.End.Y) / 2.0
                    );

                    // Testowe punkty dla określenia kierunku
                    var testA = new XPoint(midpoint.X + nx * offsetValue, midpoint.Y + ny * offsetValue);
                    var testB = new XPoint(midpoint.X - nx * offsetValue, midpoint.Y - ny * offsetValue);

                    double da = DistanceSquared(testA, bboxCenter);
                    double db = DistanceSquared(testB, bboxCenter);

                    // 🔑 KLUCZOWA ZMIANA: Określenie znaku na podstawie wartości offsetu
                    // Dla dodatniego offsetu: idziemy do środka (bliżej centrum)
                    // Dla ujemnego offsetu: idziemy na zewnątrz (dalej od centrum)
                    double sign;
                    if (offsetValue >= 0)
                    {
                        // Dodatni offset - do środka
                        sign = da < db ? 1 : -1;
                    }
                    else
                    {
                        // Ujemny offset - na zewnątrz
                        sign = da < db ? -1 : 1;
                    }

                    var p1 = new XPoint(seg.Start.X + nx * offsetValue * sign, seg.Start.Y + ny * offsetValue * sign);
                    var p2 = new XPoint(seg.End.X + nx * offsetValue * sign, seg.End.Y + ny * offsetValue * sign);

                    p1 = SnapPoint(p1);
                    p2 = SnapPoint(p2);

                    offsetSegments.Add(new ContourSegment(p1, p2)
                    {
                        Informacja = seg.Informacja ?? side
                    });

                    //Console.WriteLine($"🔷 Segment {i} ({side}): offsetValue={offsetValue}, sign={sign}, da={da:F2}, db={db:F2}");
                }
                else if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    var center = seg.Center.Value;

                    string arcKey = $"{Math.Round(center.X, 3)}_{Math.Round(center.Y, 3)}_{Math.Round(seg.Radius, 3)}";

                    if (!arcRadiusCache.ContainsKey(arcKey))
                    {
                        // 🔑 Dla łuków: dodatni offset zmniejsza promień (do środka), ujemny zwiększa (na zewnątrz)
                        arcRadiusCache[arcKey] = (float)(seg.Radius - offsetValue);
                    }

                    float newRadius = arcRadiusCache[arcKey];
                    if (newRadius < 0.1f) newRadius = 0.1f;

                    double startAngle = Math.Atan2(seg.Start.Y - center.Y, seg.Start.X - center.X);
                    double endAngle = Math.Atan2(seg.End.Y - center.Y, seg.End.X - center.X);

                    var newStart = new XPoint(
                        center.X + newRadius * Math.Cos(startAngle),
                        center.Y + newRadius * Math.Sin(startAngle)
                    );

                    var newEnd = new XPoint(
                        center.X + newRadius * Math.Cos(endAngle),
                        center.Y + newRadius * Math.Sin(endAngle)
                    );

                    newStart = SnapPoint(newStart);
                    newEnd = SnapPoint(newEnd);

                    offsetSegments.Add(new ContourSegment(newStart, newEnd, center, newRadius, true)
                    {
                        Informacja = seg.Informacja ?? (isFullCircle ? "ARC_FULL_CIRCLE" : side)
                    });

                    //Console.WriteLine($"🔷 Łuk {i} ({side}): offsetValue={offsetValue}, oldRadius={seg.Radius:F2}, newRadius={newRadius:F2}");
                }
            }

            // PRZECIĘCIA
            var result = new List<ContourSegment>();

            for (int i = 0; i < offsetSegments.Count; i++)
            {
                var current = offsetSegments[i];
                var previous = offsetSegments[(i - 1 + offsetSegments.Count) % offsetSegments.Count];

                XPoint? intersection = null;

                if (current.Type == SegmentType.Line && previous.Type == SegmentType.Line)
                {
                    intersection = GetLinesIntersectionK(previous.Start, previous.End, current.Start, current.End);
                }
                else if (previous.Type == SegmentType.Line && current.Type == SegmentType.Arc && current.Center != null)
                {
                    var pts = GetLineCircleIntersections(previous.Start, previous.End, current.Center.Value, current.Radius);
                    intersection = ChooseClosestTo(pts, current.Start);
                }
                else if (previous.Type == SegmentType.Arc && previous.Center != null && current.Type == SegmentType.Line)
                {
                    var pts = GetLineCircleIntersections(current.Start, current.End, previous.Center.Value, previous.Radius);
                    intersection = ChooseClosestTo(pts, current.Start);
                }
                else if (previous.Type == SegmentType.Arc && current.Type == SegmentType.Arc &&
                         previous.Center != null && current.Center != null)
                {
                    var pts = GetCircleCircleIntersections(previous.Center.Value, previous.Radius, current.Center.Value, current.Radius);
                    intersection = ChooseClosestTo(pts, current.Start);
                }

                if (intersection != null && !double.IsNaN(intersection.Value.X))
                {
                    if (result.Count > 0)
                    {
                        result[^1].End = intersection.Value;
                    }

                    if (current.Type == SegmentType.Arc && current.Center != null)
                    {
                        result.Add(new ContourSegment(intersection.Value, current.End, current.Center, current.Radius, true)
                        {
                            Informacja = current.Informacja
                        });
                    }
                    else
                    {
                        result.Add(new ContourSegment(intersection.Value, current.End)
                        {
                            Informacja = current.Informacja
                        });
                    }
                }
                else
                {
                    if (result.Count > 0)
                    {
                        var srodek = new XPoint(
                            (result[^1].End.X + current.Start.X) / 2.0,
                            (result[^1].End.Y + current.Start.Y) / 2.0
                        );
                        result[^1].End = srodek;
                        current.Start = srodek;
                    }
                    result.Add(current);
                }
            }

            // 🔑 SPRAWDŹ CZY CAŁY KONTUR JEST CCW
            if (result.Count > 0)
            {
                double pole = 0;
                for (int i = 0; i < result.Count; i++)
                {
                    var current = result[i];
                    var next = result[(i + 1) % result.Count];
                    pole += (current.Start.X * next.Start.Y) - (next.Start.X * current.Start.Y);
                }
                pole /= 2.0;

                if (pole < 0)
                {
                    result.Reverse();
                    for (int i = 0; i < result.Count; i++)
                    {
                        var temp = result[i].Start;
                        result[i].Start = result[i].End;
                        result[i].End = temp;
                    }
                }
            }

            // ZAMKNIĘCIE
            if (result.Count > 0)
            {
                var firstStart = result[0].Start;
                var lastEnd = result[^1].End;

                double odleglosc = Math.Sqrt(Math.Pow(lastEnd.X - firstStart.X, 2) +
                                             Math.Pow(lastEnd.Y - firstStart.Y, 2));

                if (odleglosc > TOLERANCJA)
                {
                    result[^1].End = result[0].Start;
                }
            }

            await Task.CompletedTask; // symulacja asynchroniczności

            return result;
        }

        private static XPoint SnapPoint(XPoint p, double precision = 0.001)
        {
            return new XPoint(
                Math.Round(p.X / precision) * precision,
                Math.Round(p.Y / precision) * precision
            );
        }

        private static double DistanceSquared(XPoint a, XPoint b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        private static XPoint? ChooseClosestTo(List<XPoint> pts, XPoint reference)
        {
            if (pts == null || pts.Count == 0) return null;
            XPoint best = pts[0];
            double bestD = DistanceSquared(best, reference);
            for (int i = 1; i < pts.Count; i++)
            {
                double d = DistanceSquared(pts[i], reference);
                if (d < bestD) { best = pts[i]; bestD = d; }
            }
            return best;
        }

        private List<XPoint> GetCircleCircleIntersections(XPoint c0, double r0, XPoint c1, double r1)
        {
            var results = new List<XPoint>();
            double dx = c1.X - c0.X;
            double dy = c1.Y - c0.Y;
            double d = Math.Sqrt(dx * dx + dy * dy);
            if (d < 1e-9) return results; // współśrodkowe lub bardzo blisko

            // Warunek istnienia przecięć
            if (d > r0 + r1 + 1e-9) return results; // za daleko
            if (d < Math.Abs(r0 - r1) - 1e-9) return results; // jedno zawiera drugie

            double a = (r0 * r0 - r1 * r1 + d * d) / (2 * d);
            double h = Math.Sqrt(Math.Max(0, r0 * r0 - a * a));

            double xm = c0.X + a * (dx) / d;
            double ym = c0.Y + a * (dy) / d;

            double rx = -dy * (h / d);
            double ry = dx * (h / d);

            var p1 = new XPoint(xm + rx, ym + ry);
            var p2 = new XPoint(xm - rx, ym - ry);
            results.Add(p1);
            if (DistanceSquared(p1, p2) > 1e-12) results.Add(p2);

            return results;
        }

        private XPoint GetLinesIntersectionK(XPoint p1, XPoint p2, XPoint p3, XPoint p4)
        {
            double d = (p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X);
            if (Math.Abs(d) < 1e-10) return new XPoint(float.NaN, float.NaN);

            double pre = (p1.X * p2.Y - p1.Y * p2.X);
            double post = (p3.X * p4.Y - p3.Y * p4.X);

            double x = (pre * (p3.X - p4.X) - (p1.X - p2.X) * post) / d;
            double y = (pre * (p3.Y - p4.Y) - (p1.Y - p2.Y) * post) / d;

            return new XPoint(x, y);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private XPoint GetLinesIntersection(XPoint a1, XPoint a2, XPoint b1, XPoint b2)
        {
            float dx1 = (float)(a2.X - a1.X);
            float dy1 = (float)(a2.Y - a1.Y);
            float dx2 = (float)(b2.X - b1.X);
            float dy2 = (float)(b2.Y - b1.Y);

            float determinant = dx1 * dy2 - dy1 * dx2;
            if (Math.Abs(determinant) < 1e-6f)
            {
                // Linie równoległe
                return new XPoint((a1.X + b1.X) / 2, (a1.Y + b1.Y) / 2);
            }

            float t = (float)((b1.X - a1.X) * dy2 - (b1.Y - a1.Y) * dx2) / determinant;

            return new XPoint(
                a1.X + t * dx1,
                a1.Y + t * dy1
            );
        }

        /// <summary>
        /// Zwraca największą odległość pomiędzy dowolnymi dwoma punktami elementu.
        /// </summary>
        public float DlugoscElementu(List<XPoint> vertices)
        {
            if (vertices == null || vertices.Count < 2)
                return 0;

            double minX = vertices.Min(p => p.X);
            double maxX = vertices.Max(p => p.X);

            double minY = vertices.Min(p => p.Y);
            double maxY = vertices.Max(p => p.Y);

            double width = maxX - minX;
            double height = maxY - minY;

            return (float)Math.Round(Math.Max(width, height), 2);
        }
        //private double Odleglosc(XPoint p1, XPoint p2)
        //{
        //    double dx = p2.X - p1.X;
        //    double dy = p2.Y - p1.Y;
        //    return Math.Sqrt(dx * dx + dy * dy);
        //}


    }
}
