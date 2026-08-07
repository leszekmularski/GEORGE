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

                //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

                // Obliczanie kątów między liniami
                float angleDegreesStronaA = 0; // Kąt między bieżącą linią a poprzednią
                float angleDegreesStronaB = 0; // Kąt między bieżącą linią a następną

                // Wektor bieżącej linii (od i do next)
                double currentDx = outerEnd.X - outerStart.X;
                double currentDy = outerEnd.Y - outerStart.Y;

                // Wektor poprzedniej linii (od prev do i)
                XPoint outerPrev = outer[prev];
                double prevDx = outerStart.X - outerPrev.X;
                double prevDy = outerStart.Y - outerPrev.Y;

                // Wektor następnej linii (od next do next+1)
                int next2 = (next + 1) % vertexCount;
                XPoint outerNext2 = outer[next2];
                double nextDx = outerNext2.X - outerEnd.X;
                double nextDy = outerNext2.Y - outerEnd.Y;

                // Oblicz kąt między bieżącą a poprzednią (StronaA)
                double dotProductPrev = (currentDx * prevDx + currentDy * prevDy);
                double magCurrent = Math.Sqrt(currentDx * currentDx + currentDy * currentDy);
                double magPrev = Math.Sqrt(prevDx * prevDx + prevDy * prevDy);

                if (magCurrent > 0 && magPrev > 0)
                {
                    double cosAnglePrev = dotProductPrev / (magCurrent * magPrev);
                    cosAnglePrev = Math.Max(-1.0, Math.Min(1.0, cosAnglePrev)); // Zabezpieczenie przed błędami zaokrągleń
                    double angleRadPrev = Math.Acos(cosAnglePrev);
                    angleDegreesStronaA = (float)(angleRadPrev * 180.0 / Math.PI);
                }

                // Oblicz kąt między bieżącą a następną (StronaB)
                double dotProductNext = (currentDx * nextDx + currentDy * nextDy);
                double magNext = Math.Sqrt(nextDx * nextDx + nextDy * nextDy);

                if (magCurrent > 0 && magNext > 0)
                {
                    double cosAngleNext = dotProductNext / (magCurrent * magNext);
                    cosAngleNext = Math.Max(-1.0, Math.Min(1.0, cosAngleNext)); // Zabezpieczenie
                    double angleRadNext = Math.Acos(cosAngleNext);
                    angleDegreesStronaB = (float)(angleRadNext * 180.0 / Math.PI);
                }

                // Opcjonalnie: określenie strony kąta (wewnętrzny/zewnętrzny)
                // Możesz użyć iloczynu wektorowego do określenia orientacji
                double crossProductPrev = (currentDx * prevDy - currentDy * prevDx);
                if (crossProductPrev < 0)
                {
                    angleDegreesStronaA = Math.Abs(angleDegrees - angleDegreesStronaA); // Kąt po drugiej stronie
                }

                double crossProductNext = (currentDx * nextDy - currentDy * nextDx);
                if (crossProductNext < 0)
                {
                    angleDegreesStronaB = 360 - angleDegreesStronaB;
                }

                // Teraz możesz użyć angleDegreesStronaA i angleDegreesStronaB
                //Console.WriteLine($"⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️ Wierzchołek {i}: Kąt elementu: {angleDegrees:F1}° Kąt z poprzednim = {angleDegreesStronaA:F1}°, Kąt z następnym = {angleDegreesStronaB:F1}°");
                //Console.WriteLine($"⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️ Wierzchołek {i}: {(Math.Abs((int)angleDegrees - (int)angleDegreesStronaA))} < 20 && {(int)angleDegrees - (int)angleDegreesStronaA} != 0)");

                if ((angleDegreesStronaA < 20 && angleDegrees != 90 && angleDegreesStronaA != 270)
                    || (Math.Abs((int)angleDegrees - (int)angleDegreesStronaA) < 46 && (int)angleDegrees - (int)angleDegreesStronaA != 0))
                {
                    // Jeśli kąt z następnym jest bardzo mały, traktujemy to jako prawie prostą linię → potencjalnie T1
                    leftJoin = "T2"; // połączone równym kątem
                                     // Console.WriteLine($"⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️⚠️ Wierzchołek {i}: Kąt elementu: {angleDegrees:F1}° Kąt z poprzednim = {angleDegreesStronaA:F1}°, Kąt z następnym = {angleDegreesStronaB:F1}°");
                    BledySystemowe.Add($"⚠️ Wierzchołek element nr: {i + 1}: Kąt z poprzednim elementem = {angleDegrees} a {angleDegreesStronaA:F1}° jest bardzo mały. Zmieniono typ połączenia na T2 dla lewego narożnika.");
                }

                if (angleDegreesStronaB < 45 && angleDegrees != 90)
                {
                    // Jeśli kąt z następnym jest bardzo mały, traktujemy to jako prawie prostą linię → potencjalnie T1
                    rightJoin = "T2"; // połączone równym kątem
                    BledySystemowe.Add($"⚠️ Wierzchołek element nr: {i + 1}: Kąt z następnym elementem = {angleDegrees} a {angleDegreesStronaB:F1}° jest bardzo mały. Zmieniono typ połączenia na T2 dla prawego narożnika.");
                }

                //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

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

                if (leftJoin == "T1" && rightJoin == "T4" || leftJoin == "T4" && rightJoin == "T1")
                {
                    if (leftJoin == "T4" && rightJoin == "T1")
                    {
                        if (isAlmostHorizontal)
                        {
                            // Przecięcia z konturem na bazie normalnej

                            var outerVecStart = FindFirstEdgeIntersection(outerStart, nx, ny, outer);
                            var outerVecEnd = FindFirstEdgeIntersection(outerEnd, nx, ny, outer);

                            var innerVecStart = FindFirstEdgeIntersection(
                                new XPoint(outerVecStart.X + nx * profile, outerVecStart.Y + ny * profile),
                                tx, ty, outer);

                            var innerVecEnd = FindFirstEdgeIntersection(
                                new XPoint(outerVecEnd.X + nx * profile, outerVecEnd.Y + ny * profile),
                                tx, ty, outer);

                            wierzcholki = new List<XPoint> {
                            outerVecStart, outerVecEnd, innerVecEnd, innerVecStart
                            };


                        }
                        else
                        {
                            //Console.WriteLine($"🔷 Vertical case for element {i + 1} isAlmostHorizontal: {isAlmostHorizontal} isAlmostVertical: {isAlmostVertical}");

                            if (leftJoin == "T4" && rightJoin == "T4" && vertexCount > 4)
                            {
                                var topY = Math.Min(inner[i].Y, inner[next].Y);
                                var bottomY = Math.Max(inner[i].Y, inner[next].Y);

                                // Start liczymy względem punktu przecięcia z inner[i] (czyli skrócony)
                                var outerTop = GetHorizontalIntersection(_innerStart, _innerEnd, (float)topY);
                                var outerBottom = GetHorizontalIntersection(_innerStart, _innerEnd, (float)bottomY);

                                // Normalne punkty wewnętrzne
                                var innerTop = GetHorizontalIntersection(outer[i], outer[next], (float)topY);
                                var innerBottom = GetHorizontalIntersection(outer[i], outer[next], (float)bottomY);

                                wierzcholki = new List<XPoint> {
                                outerTop, outerBottom, innerBottom, innerTop
                                };


                            }
                            else
                            {
                                // Pionowy przypadek (np. boczne elementy w trapezie)
                                var topY = Math.Min(inner[i].Y, inner[next].Y);
                                var bottomY = Math.Max(inner[i].Y, inner[next].Y);
                                //             if(angleDegrees )

                                //var outerTop = GetHorizontalIntersection(outerStart, outerEnd, (float)topY);
                                var outerBottom = GetHorizontalIntersection(outerStart, outerEnd, (float)bottomY);

                                var innerTop = GetHorizontalIntersection(inner[i], inner[next], (float)topY);
                                var innerBottom = GetHorizontalIntersection(inner[i], inner[next], (float)bottomY);

                                XPoint outerTop = new(); // = FindFirstEdgeIntersectionByAngle(innerTop, firstangleDegrees - 180, outer);

                                if (i == vertexCount - 1)
                                {
                                    outerTop = FindFirstEdgeIntersectionByAngle(innerTop, firstangleDegrees - 180, outer);
                                }
                                else
                                {
                                    if (angleDegrees == 270)
                                    {

                                        outerTop = FindFirstEdgeIntersectionByAngle(innerTop, 180 + angleNext, outer);
                                        // Console.WriteLine($"🔷 Wyliczono dla elementu {i + 1} angleNext: {angleNext} angleDegrees: {angleDegrees} firstangleDegrees: {firstangleDegrees} anglePrev: {anglePrev}");
                                    }
                                    else
                                    {
                                        //   Console.WriteLine($"🔷 Szukanie przecięcia dla elementu {i + 1} z anglePrevDegrees: {anglePrev}");
                                        outerTop = FindFirstEdgeIntersectionByAngle(innerTop, anglePrev, outer);
                                    }

                                }

                                wierzcholki = new List<XPoint> {
                                outerTop, outerBottom, innerBottom, innerTop
                                };


                            }
                        }
                    }
                    else//--> tylko ten warunek if (leftJoin == "T1" && rightJoin == "T4")
                    {
                        if (isAlmostHorizontal)
                        {
                            // Przecięcia z konturem na bazie normalnej

                            var outerVecStart = FindFirstEdgeIntersection(outerStart, nx, ny, outer);
                            var outerVecEnd = FindFirstEdgeIntersection(outerEnd, nx, ny, outer);

                            var innerVecStart = FindFirstEdgeIntersection(
                                new XPoint(outerVecStart.X + nx * profile, outerVecStart.Y + ny * profile),
                                tx, ty, outer);

                            var innerVecEnd = FindFirstEdgeIntersection(
                                new XPoint(outerVecEnd.X + nx * profile, outerVecEnd.Y + ny * profile),
                                tx, ty, outer);

                            wierzcholki = new List<XPoint> {
                            outerVecStart, outerVecEnd, innerVecEnd, innerVecStart
                            };


                        }
                        else
                        {

                            // Pionowy przypadek (np. boczne elementy w trapezie)
                            var topY = Math.Min(inner[i].Y, inner[next].Y);
                            var bottomY = Math.Max(inner[i].Y, inner[next].Y);
                            //             if(angleDegrees )

                            //var outerTop = GetHorizontalIntersection(outerStart, outerEnd, (float)topY);
                            var outerBottom = GetHorizontalIntersection(outerStart, outerEnd, (float)bottomY);

                            var innerTop = GetHorizontalIntersection(inner[i], inner[next], (float)topY);
                            var innerBottom = GetHorizontalIntersection(inner[i], inner[next], (float)bottomY);

                            XPoint outerTop; // = FindFirstEdgeIntersectionByAngle(innerTop, firstangleDegrees - 180, outer);
                            if (i == vertexCount - 1)
                            {
                                outerTop = FindFirstEdgeIntersectionByAngle(innerTop, firstangleDegrees - 180, outer);
                            }
                            else
                            {
                                // Console.WriteLine($"🔷 Szukanie przecięcia dla elementu {i + 1} z anglePrev: {anglePrev}, zmienna angleDegrees: {angleDegrees} angleNext: {angleNext} anglePrev: {anglePrev}");

                                if (anglePrev == -1 && vertexCount < 4)
                                {
                                    innerTop = inner[i];
                                    outerTop = FindFirstEdgeIntersectionByAngle(innerTop, anglePrev, outer);
                                }
                                else
                                {
                                    outerTop = FindFirstEdgeIntersectionByAngle(innerTop, anglePrev, outer);
                                }

                            }

                            if (vertexCount < 4 && anglePrev != -1)
                            {
                                innerTop = FindFirstEdgeIntersectionByAngle(innerTop, angleDegrees - 180, outer);
                                outerTop = FindFirstEdgeIntersectionByAngle(outerTop, angleDegrees - 180, outer);
                            }

                            wierzcholki = new List<XPoint> {
                            outerTop, outerBottom, innerBottom, innerTop
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
                    //Console.WriteLine($"🔷 T1/T1 element {i + 1} START isAlmostHorizontal: {isAlmostHorizontal} isAlmostVertical: {isAlmostVertical} vertexCount: {vertexCount} angleDegrees: {angleDegrees} firstangleDegrees: {firstangleDegrees}");

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
                            getStartT1[1], getEndT1[1], getEndT1[0], getStartT1[0]
                        };


                }
                else if (leftJoin == "T3" && rightJoin == "T3")
                {
                    //Console.WriteLine($"🔷 T1/T1 element {i + 1} START isAlmostHorizontal: {isAlmostHorizontal} isAlmostVertical: {isAlmostVertical} vertexCount: {vertexCount} angleDegrees: {angleDegrees}");
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

                    wierzcholki = new List<XPoint> {
                            getStartT3[1], getEndT3[1], getEndT3[0], getStartT3[0]
                        };


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
                    // Zbierz punkty w kolejności
                    wierzcholki = new List<XPoint>
                     {
                        TopRT5,
                        TopST5,
                        TopLT5,
                        BottomLT5,
                        BottomSTT5,
                        BottomRT5,

                     };


                    Console.WriteLine($"🔷 T5-T5 -> wierzcholki: {wierzcholki.Count} new List<XPoint>");
                }
                else if (leftJoin == "T2" && rightJoin == "T1")
                {
                    Console.WriteLine($"🔷 T2/T1 element {i + 1} - kombinacja ścięcia (T2) z czopem (T1)");

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
                else if (leftJoin == "T1" && rightJoin == "T2")
                {
                    //Console.WriteLine($"🔷 T1/T2 element {i + 1} - kombinacja czopa (T1) ze ścięciem (T2)");
                    Console.WriteLine($"🔷 T1/T2 element {i + 1} START isAlmostHorizontal: {isAlmostHorizontal} isAlmostVertical: {isAlmostVertical} vertexCount: {vertexCount} angleDegrees: {angleDegrees} firstangleDegrees: {firstangleDegrees} angleDegreesStronaA:{angleDegreesStronaA:F1}° angleDegreesStronaB:{angleDegreesStronaB:F1}° anglePrev: {anglePrev:F1}°");


                    List<XPoint> getStartT2 = GetStartT2(inner[i], outer[i]);
                    List<XPoint> getEndT2 = GetEndT2(inner[next], outer[next]);

                    //List<XPoint> getStartT1 = GetStartT1(inner[i], outer[i], outer, angleDegrees, anglePrev, angleNext,
                    //    StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);




                    List<XPoint> getStartT1 = GetStartT1(inner[i], outer[i], outer, angleDegrees, anglePrev, angleNext,
                    StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);

                    if (anglePrev == 270)
                    {
                        getStartT1[1] = outer[i];
                    }

                    //var _anglePrev = anglePrev;
                    //if (i == vertexCount - 1)
                    //{
                    //    _anglePrev = firstangleDegrees;
                    //}
                    //getEndT1 = GetEndT1(inner[next], outer[next], outer, angleDegrees, _anglePrev, angleNext, StronaElementu,
                    //    stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);

                    // List<XPoint> getEndT1;

                    if (vertexCount > 6 && angleDegrees > 270 && angleDegreesStronaA < 271 || anglePrev == 270)
                    {
                        getStartT2[0] = FindFirstEdgeIntersectionByAngle(getStartT1[0], angleDegrees - 180, outer);
                    }

                    wierzcholki = new List<XPoint> {
                            getStartT1[1], getEndT2[1], getEndT2[0], getStartT2[0]
                        };

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
                    getEndT1 = GetEndT1(inner[next], outer[next], outer, angleDegrees, _anglePrev, angleNext, StronaElementu,
                        stonaOstanioDodanegoElementu, vertexCount < 6 ? -1 : i);

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

                    var _getStartT4 = FindFirstEdgeIntersectionByAngle(getStartT4[1], _anglePrev - 180, outer);

                    wierzcholki = new List<XPoint> {
                            _getStartT4, getEndT3[1], getEndT3[0], getStartT4[0]
                        };


                }
                else if (leftJoin == "T3" && rightJoin == "T4")
                {
                    Console.WriteLine($"🔷 T3/T4 element {i + 1} - kombinacja pełnego profilu (T3) z wcięciem (T4)");

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
                wierzcholkiZLukami = Build4SegmentContour(wierzcholki, outerContourSegment, innerContourSegment, i + 1, StronaElementu, wierzcholki, leftJoin, rightJoin, angleDegrees);

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
        List<XPoint> wierzcholkiLineProste,
        string leftJoin,
        string rightJoin,
        double angleDegrees)
        {
            // ============================================================
            // Dla każdego segmentu wybieramy odpowiednie fragmenty konturów
            // ============================================================

            var filteredOuter = GetSegmentsForSide(outerContour, _stronaElementu);
            var filteredInner = GetSegmentsForSide(innerContour, _stronaElementu);


            Console.WriteLine($"\n🔷 Build4SegmentContour ELEMENT {numerElemntu} _stronaElementu: {_stronaElementu} - START");
            Console.WriteLine($"   Strona: {_stronaElementu}");
            Console.WriteLine($"   Wierzchołki: [0]({wierzcholki[0].X:F2},{wierzcholki[0].Y:F2}) [1]({wierzcholki[1].X:F2},{wierzcholki[1].Y:F2}) [2]({wierzcholki[2].X:F2},{wierzcholki[2].Y:F2}) [3]({wierzcholki[3].X:F2},{wierzcholki[3].Y:F2})");

            int sourceIndex = numerElemntu - 1;

            // ============================================================
            // PRZYPADEK 1: T1 dla strony Góra (oryginalny kod)
            // ============================================================
            if (outerContour != null && innerContour != null &&
                sourceIndex >= 0 && sourceIndex < outerContour.Count && sourceIndex < innerContour.Count)
            {
                var outerSegment = outerContour[sourceIndex];
                var innerSegment = innerContour[sourceIndex];


                if (outerSegment.Type == SegmentType.Arc && outerSegment.Center.HasValue &&
                    innerSegment.Type == SegmentType.Arc && innerSegment.Center.HasValue)
                {
                    int previousIndex = (sourceIndex - 1 + outerContour.Count) % outerContour.Count;
                    int nextIndex = (sourceIndex + 1) % outerContour.Count;

                    // T1 dla Góry
                    bool leftT1Bevel = _stronaElementu == "Góra" && leftJoin == "T1" &&
                        outerContour[previousIndex].Type == SegmentType.Line;
                    bool rightT1Bevel = _stronaElementu == "Góra" && rightJoin == "T1" &&
                        outerContour[nextIndex].Type == SegmentType.Line;

                    // T3 dla Lewej i Prawej (analogicznie do T1)
                    bool leftT3Bevel = (_stronaElementu == "Góra") && leftJoin == "T3" &&
                        outerContour[previousIndex].Type == SegmentType.Line;
                    bool rightT3Bevel = (_stronaElementu == "Góra") && rightJoin == "T3" &&
                        outerContour[nextIndex].Type == SegmentType.Line;

                    // T3 dla Lewej i Prawej (analogicznie do T1)
                    bool leftT3BevelPion = (_stronaElementu == "Lewa") && leftJoin == "T3" &&
                        outerContour[previousIndex].Type == SegmentType.Line;
                    bool rightT3BevelPion = (_stronaElementu == "Prawa") && rightJoin == "T3" &&
                        outerContour[nextIndex].Type == SegmentType.Line;


                    Console.WriteLine($"🔷 Build4SegmentContour ELEMENT {numerElemntu} _stronaElementu: {_stronaElementu} - leftT1Bevel: {leftT1Bevel}, rightT1Bevel: {rightT1Bevel}, leftT3Bevel: {leftT3Bevel}, rightT3Bevel: {rightT3Bevel}, leftT3BevelPion: {leftT3BevelPion}, rightT3BevelPion: {rightT3BevelPion}");


                    var result = new List<ContourSegment>
                        {
                            new ContourSegment(outerSegment.Start, outerSegment.End,
                                outerSegment.Center, outerSegment.Radius, false)
                        };

                    // ============================================================
                    // PRAWA STRONA (rightJoin)
                    // ============================================================
                    if (rightT1Bevel)
                    {
                        // T1 dla Góry - prawa strona
                        XPoint bevel = GetT1BevelPoint(outerSegment.End, innerSegment.End, innerSegment, outerContour);
                        result.Add(new ContourSegment(outerSegment.End, bevel));
                        result.Add(new ContourSegment(bevel, innerSegment.End));
                    }
                    else if (rightT3Bevel)
                    {
                        // T3 dla Góra - prawa strona
                        XPoint bevel = GetT3BevelPoint(outerSegment.End, innerSegment.End, outerSegment, innerContour);
                        result.Add(new ContourSegment(outerSegment.End, bevel));
                        result.Add(new ContourSegment(bevel, innerSegment.End));
                    }
                    else
                    {
                        result.Add(new ContourSegment(outerSegment.End, innerSegment.End));
                    }

                    result.Add(new ContourSegment(innerSegment.End, innerSegment.Start,
                        innerSegment.Center, innerSegment.Radius, true));

                    // ============================================================
                    // LEWA STRONA (leftJoin)
                    // ============================================================
                    if (leftT1Bevel)
                    {
                        // T1 dla Góry - lewa strona
                        XPoint bevel = GetT1BevelPoint(outerSegment.Start, innerSegment.Start, innerSegment, outerContour);
                        result.Add(new ContourSegment(innerSegment.Start, bevel));
                        result.Add(new ContourSegment(bevel, outerSegment.Start));
                    }
                    else if (leftT3Bevel)
                    {
                        // T3 dla Góra- lewa strona
                        XPoint bevel = GetT3BevelPoint(outerSegment.Start, innerSegment.Start, outerSegment, innerContour);
                        result.Add(new ContourSegment(innerSegment.Start, bevel));
                        result.Add(new ContourSegment(bevel, outerSegment.Start));
                    }
                    else
                    {
                        result.Add(new ContourSegment(innerSegment.Start, outerSegment.Start));
                    }

                    return result;
                }
            }




            // ============================================================
            // PRZYPADEK 2: Standardowa ścieżka dla linii - z obsługą T3 dla skrzydła
            // ============================================================
            var adjustedVertices = new List<XPoint>(wierzcholki);
            if (outerContour != null && innerContour != null &&
                outerContour.Count == innerContour.Count &&
                sourceIndex >= 0 && sourceIndex < outerContour.Count &&
                outerContour[sourceIndex].Type == SegmentType.Line)
            {
                int previousIndex = (sourceIndex - 1 + outerContour.Count) % outerContour.Count;
                int nextIndex = (sourceIndex + 1) % outerContour.Count;

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

            var segZewnetrzny = BuildSegmentWithArc(
                adjustedVertices[0], adjustedVertices[1], filteredOuter);
            var segWewnetrzny = BuildSegmentWithArc(
                adjustedVertices[2], adjustedVertices[3], filteredInner);
             

            // ============================================================
            // PRZYPADEK 2.5: T3 dla skrzydła (strony Lewa/Prawa)
            // ============================================================
            bool isSkrzydloPion = (_stronaElementu == "Lewa" || _stronaElementu == "Prawa") &&
                                  (leftJoin == "T3" || rightJoin == "T3");

            if (isSkrzydloPion)
            {
                Console.WriteLine($"🔷 T3 SKRZYDŁO {_stronaElementu} element={numerElemntu} - CZYSTY KSZTAŁT");

                var result = new List<ContourSegment>();

                // ============================================================
                // CZYSTY KSZTAŁT - bez żadnych przesunięć i modyfikacji
                // Używamy tylko punktów z adjustedVertices
                // ============================================================

                // Kolejność segmentów dla zamkniętego konturu:
                // [0] Górny: adjustedVertices[0] -> adjustedVertices[1]
                // [1] Prawy: adjustedVertices[1] -> adjustedVertices[2]
                // [2] Dolny: adjustedVertices[2] -> adjustedVertices[3]
                // [3] Lewy:  adjustedVertices[3] -> adjustedVertices[0]

                if (rightJoin == "T3" && _stronaElementu == "Prawa")
                    adjustedVertices[1] = new XPoint(adjustedVertices[1].X, adjustedVertices[1].Y - 250);
                if (rightJoin == "T3" && _stronaElementu == "Lewa")
                    adjustedVertices[0] = new XPoint(adjustedVertices[0].X, adjustedVertices[0].Y - 250);

                    // Segment górny (zewnętrzny)
                    result.Add(new ContourSegment(adjustedVertices[0], adjustedVertices[1]));

                // Segment prawy
                result.Add(new ContourSegment(adjustedVertices[1], adjustedVertices[2]));

                // Segment dolny (wewnętrzny)
                result.Add(new ContourSegment(adjustedVertices[2], adjustedVertices[3]));

                // Segment lewy
                result.Add(new ContourSegment(adjustedVertices[3], adjustedVertices[0]));

                //prawa strona 1 w osi Y

                Console.WriteLine($"   adjustedVertices[0]: ({adjustedVertices[0].X:F2}, {adjustedVertices[0].Y:F2})");
                Console.WriteLine($"   adjustedVertices[1]: ({adjustedVertices[1].X:F2}, {adjustedVertices[1].Y:F2})");
                Console.WriteLine($"   adjustedVertices[2]: ({adjustedVertices[2].X:F2}, {adjustedVertices[2].Y:F2})");
                Console.WriteLine($"   adjustedVertices[3]: ({adjustedVertices[3].X:F2}, {adjustedVertices[3].Y:F2})");

                return result;
            }

            //if (isSkrzydloPion)
            //{
            //    Console.WriteLine($"🔷 T3 SKRZYDŁO {_stronaElementu} element={numerElemntu}");

            //    var result = new List<ContourSegment>();

            //    // Zewnętrzny segment (górny)
            //    result.Add(segZewnetrzny);

            //    // ============================================================
            //    // PRAWA STRONA - przedłużamy do outerContour
            //    // ============================================================
            //    if (rightJoin == "T3" && _stronaElementu == "Prawa")
            //    {
            //        // Znajdź punkt na outerContour dla prawej strony
            //        // Szukamy punktu o tej samej współrzędnej X co adjustedVertices[1]
            //        XPoint pointOnContour = FindPointOnOuterContourForT3(
            //            adjustedVertices[3],  // punkt na elemencie pionowym
            //            adjustedVertices[2],  // punkt wewnętrzny
            //            outerContour,
            //            true); // szukamy w górę

            //        // Sprawdź czy znaleziono punkt na outerContour
            //        if (Distance(pointOnContour, adjustedVertices[1]) > 0.1 &&
            //            pointOnContour.Y < adjustedVertices[1].Y)
            //        {
            //            // Używamy znalezionego punktu jako bevel
            //            XPoint bevel = pointOnContour;

            //            // Ścięcie: bevel -> adjustedVertices[1] (idziemy w dół do punktu na elemencie)
            //            result.Add(new ContourSegment(bevel, adjustedVertices[1]));
            //            // Pion: adjustedVertices[1] -> adjustedVertices[2]
            //            result.Add(new ContourSegment(adjustedVertices[1], adjustedVertices[2]));
            //        }
            //        else
            //        {
            //            // Fallback: bezpośrednie połączenie
            //            result.Add(new ContourSegment(adjustedVertices[1], adjustedVertices[2]));
            //        }
            //    }
            //    else
            //    {
            //        result.Add(new ContourSegment(adjustedVertices[1], adjustedVertices[2]));
            //    }

            //    // Wewnętrzny segment (dolny)
            //    result.Add(segWewnetrzny);

            //    // ============================================================
            //    // LEWA STRONA - przedłużamy do outerContour
            //    // ============================================================
            //    if (leftJoin == "T3" && _stronaElementu == "Lewa")
            //    {
            //        // Znajdź punkt na outerContour dla prawej strony
            //        // Szukamy punktu o tej samej współrzędnej X co adjustedVertices[1]
            //        XPoint pointOnContour = FindPointOnOuterContourForT3(
            //            adjustedVertices[0],  // punkt na elemencie pionowym
            //            adjustedVertices[3],  // punkt wewnętrzny
            //            outerContour,
            //            true); // szukamy w górę

            //        // Sprawdź czy znaleziono punkt na outerContour
            //        if (Distance(pointOnContour, adjustedVertices[1]) > 0.1 &&
            //            pointOnContour.Y < adjustedVertices[1].Y)
            //        {
            //            // Używamy znalezionego punktu jako bevel
            //            XPoint bevel = pointOnContour;

            //         //   adjustedVertices[2] = new XPoint(adjustedVertices[2].X, adjustedVertices[2].Y - 250);

            //            // Ścięcie: bevel -> adjustedVertices[1] (idziemy w dół do punktu na elemencie)
            //            result.Add(new ContourSegment(adjustedVertices[1], adjustedVertices[0]));
            //            // Pion: adjustedVertices[1] -> adjustedVertices[2]
            //            result.Add(new ContourSegment(adjustedVertices[2], adjustedVertices[3]));
            //        }
            //        else
            //        {
            //            // Fallback: bezpośrednie połączenie
            //            result.Add(new ContourSegment(adjustedVertices[1], adjustedVertices[2]));
            //        }
            //    }
            //    else
            //    {
            //        result.Add(new ContourSegment(adjustedVertices[0], adjustedVertices[3]));
            //    }

            //    return result;
            //}





            // ============================================================
            // PRZYPADEK 2: Standardowa ścieżka dla linii
            // ============================================================
            //var adjustedVertices = new List<XPoint>(wierzcholki);
            if (outerContour != null && innerContour != null &&
                outerContour.Count == innerContour.Count &&
                sourceIndex >= 0 && sourceIndex < outerContour.Count &&
                outerContour[sourceIndex].Type == SegmentType.Line)
            {
                int previousIndex = (sourceIndex - 1 + outerContour.Count) % outerContour.Count;
                int nextIndex = (sourceIndex + 1) % outerContour.Count;

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

            //var segZewnetrzny = BuildSegmentWithArc(
            //    adjustedVertices[0], adjustedVertices[1], filteredOuter);
            //var segWewnetrzny = BuildSegmentWithArc(
            //    adjustedVertices[2], adjustedVertices[3], filteredInner);

            // ============================================================
            // PRZYPADEK 3: T1 z łukiem (t1AfterArc / t1BeforeArc)
            // ============================================================
            bool t1AfterArc = sourceIndex >= 0 && outerContour != null &&
                sourceIndex < outerContour.Count && outerContour.Count > 0 && leftJoin == "T1" &&
                outerContour[(sourceIndex - 1 + outerContour.Count) % outerContour.Count].Type == SegmentType.Arc;
            bool t1BeforeArc = sourceIndex >= 0 && outerContour != null &&
                sourceIndex < outerContour.Count && outerContour.Count > 0 && rightJoin == "T1" &&
                outerContour[(sourceIndex + 1) % outerContour.Count].Type == SegmentType.Arc;

            if (t1AfterArc)
            {
                int previousIndex = (sourceIndex - 1 + outerContour.Count) % outerContour.Count;
                var outerArcSegment = outerContour[previousIndex];

                XPoint bevel = GetT1BevelPoint(
                    adjustedVertices[2], adjustedVertices[1],
                    outerArcSegment, outerContour);

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
                int nextIndex = (sourceIndex + 1) % outerContour.Count;
                var outerArcSegment = outerContour[nextIndex];

                XPoint bevel = GetT1BevelPoint(
                    adjustedVertices[1], adjustedVertices[2],
                    outerArcSegment, outerContour);

                return new List<ContourSegment>
                {
                    segZewnetrzny,
                    new ContourSegment(adjustedVertices[1], bevel),
                    new ContourSegment(bevel, adjustedVertices[2]),
                    segWewnetrzny,
                    new ContourSegment(adjustedVertices[3], adjustedVertices[0])
                };
            }


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

        private ContourSegment FindPreviousArc(
        List<ContourSegment> contour,
        int index)
        {
            if (contour == null || contour.Count == 0)
                return null;


            for (int i = 1; i <= contour.Count; i++)
            {
                int id = (index - i + contour.Count) % contour.Count;

                if (contour[id].Type == SegmentType.Arc)
                    return contour[id];
            }

            return null;
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
       XPoint outerPoint,
       XPoint innerPoint,
       ContourSegment arcOuter,
       ContourSegment arcInner)
        {
            // kierunek pionowy
            bool vertical =
                Math.Abs(outerPoint.X - innerPoint.X) <
                Math.Abs(outerPoint.Y - innerPoint.Y);


            if (vertical)
            {
                // T3 pion:
                // zachowujemy poziom (Y)
                double y = outerPoint.Y;

                double x;

                // wybieramy stronę
                if (Math.Abs(outerPoint.X) > Math.Abs(innerPoint.X))
                {
                    x = outerPoint.X;
                }
                else
                {
                    x = innerPoint.X;
                }


                return new XPoint(x, y);
            }


            // standard dla pozostałych przypadków
            return new XPoint(
                (outerPoint.X + innerPoint.X) / 2.0,
                (outerPoint.Y + innerPoint.Y) / 2.0);
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
        /// Znajduje punkt na górnym konturze zewnętrznym (indeksy 0-3) o tej samej współrzędnej X
        /// </summary>
        private static XPoint FindPointOnOuterContour(XPoint point, List<ContourSegment> outerContour)
        {
            if (outerContour == null || outerContour.Count == 0)
                return point;

            XPoint bestPoint = point;
            double minDistance = double.MaxValue;

            // Szukamy tylko na górnych segmentach (indeksy 0-3)
            int maxIndex = Math.Min(outerContour.Count, 4);

            for (int idx = 0; idx < maxIndex; idx++)
            {
                var seg = outerContour[idx];

                if (seg.Type == SegmentType.Line)
                {
                    double minX = Math.Min(seg.Start.X, seg.End.X);
                    double maxX = Math.Max(seg.Start.X, seg.End.X);

                    if (point.X >= minX - 0.5 && point.X <= maxX + 0.5)
                    {
                        double t = (seg.End.X - seg.Start.X) != 0
                            ? (point.X - seg.Start.X) / (seg.End.X - seg.Start.X)
                            : 0;
                        double y = seg.Start.Y + t * (seg.End.Y - seg.Start.Y);

                        XPoint candidate = new XPoint(point.X, y);

                        // Szukamy punktu wyżej (mniejsza Y) i bliżej
                        if (candidate.Y < point.Y - 0.1)
                        {
                            double dist = Math.Abs(point.Y - candidate.Y);
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                bestPoint = candidate;
                            }
                        }
                    }
                }
                else if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    int steps = 50;
                    for (int i = 0; i <= steps; i++)
                    {
                        double t = (double)i / steps;
                        double startAngle = Math.Atan2(seg.Start.Y - seg.Center.Value.Y, seg.Start.X - seg.Center.Value.X);
                        double endAngle = Math.Atan2(seg.End.Y - seg.Center.Value.Y, seg.End.X - seg.Center.Value.X);

                        if (seg.CounterClockwise)
                        {
                            while (endAngle <= startAngle) endAngle += 2 * Math.PI;
                        }
                        else
                        {
                            while (endAngle >= startAngle) endAngle -= 2 * Math.PI;
                        }

                        double angle = startAngle + t * (endAngle - startAngle);

                        XPoint candidate = new XPoint(
                            seg.Center.Value.X + seg.Radius * Math.Cos(angle),
                            seg.Center.Value.Y + seg.Radius * Math.Sin(angle)
                        );

                        if (Math.Abs(candidate.X - point.X) < 0.5)
                        {
                            if (candidate.Y < point.Y - 0.1)
                            {
                                double dist = Math.Abs(point.Y - candidate.Y);
                                if (dist < minDistance)
                                {
                                    minDistance = dist;
                                    bestPoint = candidate;
                                }
                            }
                        }
                    }
                }
            }

            return bestPoint;
        }

       private static XPoint FindPointOnOuterContourForT3(
      XPoint outerPoint,
      XPoint innerPoint,
      List<ContourSegment> outerContour,
      bool goUp)
        {
            if (outerContour == null || outerContour.Count == 0)
                return outerPoint;

            DisplayLineAngle(outerPoint, innerPoint, "FindPointOnOuterContourForT3 !!!!!");

            XPoint bestPoint = outerPoint;
            double minDistance = double.MaxValue;

            // Przeszukujemy wszystkie segmenty konturu
            for (int idx = 0; idx < outerContour.Count; idx++)
            {
                var seg = outerContour[idx];

                if (seg.Type == SegmentType.Line)
                {
                    // Dla linii - znajdź punkt przecięcia z pionową linią
                    double minX = Math.Min(seg.Start.X, seg.End.X);
                    double maxX = Math.Max(seg.Start.X, seg.End.X);

                    if (outerPoint.X >= minX - 0.5 && outerPoint.X <= maxX + 0.5)
                    {
                        double t = (seg.End.X - seg.Start.X) != 0
                            ? (outerPoint.X - seg.Start.X) / (seg.End.X - seg.Start.X)
                            : 0;
                        double y = seg.Start.Y + t * (seg.End.Y - seg.Start.Y);

                        XPoint candidate = new XPoint(outerPoint.X, y);

                        if (goUp && candidate.Y < outerPoint.Y - 0.1)
                        {
                            double dist = Math.Abs(outerPoint.Y - candidate.Y);
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                bestPoint = candidate;
                            }
                        }
                        else if (!goUp && candidate.Y > outerPoint.Y + 0.1)
                        {
                            double dist = Math.Abs(outerPoint.Y - candidate.Y);
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                bestPoint = candidate;
                            }
                        }
                    }
                }
                else if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    // Dla łuku - znajdź punkt na łuku o tej samej współrzędnej X
                    // Zwiększamy liczbę kroków dla większej dokładności
                    int steps = 100;

                    // Oblicz kąty start i end
                    double startAngle = Math.Atan2(seg.Start.Y - seg.Center.Value.Y, seg.Start.X - seg.Center.Value.X);
                    double endAngle = Math.Atan2(seg.End.Y - seg.Center.Value.Y, seg.End.X - seg.Center.Value.X);

                    // Normalizacja kątów dla CCW/CW
                    if (seg.CounterClockwise)
                    {
                        while (endAngle <= startAngle) endAngle += 2 * Math.PI;
                    }
                    else
                    {
                        while (endAngle >= startAngle) endAngle -= 2 * Math.PI;
                    }

                    // Sprawdź czy X jest w zakresie łuku
                    double minX = Math.Min(seg.Start.X, seg.End.X);
                    double maxX = Math.Max(seg.Start.X, seg.End.X);

                    // Jeśli outerPoint.X jest poza zakresem X łuku, pomiń
                    if (outerPoint.X < minX - 0.5 || outerPoint.X > maxX + 0.5)
                        continue;

                    for (int i = 0; i <= steps; i++)
                    {
                        double t = (double)i / steps;
                        double angle = startAngle + t * (endAngle - startAngle);

                        XPoint candidate = new XPoint(
                            seg.Center.Value.X + seg.Radius * Math.Cos(angle),
                            seg.Center.Value.Y + seg.Radius * Math.Sin(angle)
                        );

                        // Sprawdź czy X jest zbliżone (zwiększona tolerancja)
                        if (Math.Abs(candidate.X - outerPoint.X) < 0.5)
                        {
                            if (goUp && candidate.Y < outerPoint.Y - 0.1)
                            {
                                double dist = Math.Abs(outerPoint.Y - candidate.Y);
                                if (dist < minDistance)
                                {
                                    minDistance = dist;
                                    bestPoint = candidate;
                                }
                            }
                            else if (!goUp && candidate.Y > outerPoint.Y + 0.1)
                            {
                                double dist = Math.Abs(outerPoint.Y - candidate.Y);
                                if (dist < minDistance)
                                {
                                    minDistance = dist;
                                    bestPoint = candidate;
                                }
                            }
                        }
                    }
                }
            }

            // Jeśli znaleziono punkt na outerContour
            if (minDistance < double.MaxValue)
            {
                return bestPoint;
            }

            // Fallback: przedłużenie o 30% odległości do innerPoint
            double distToInner = Distance(outerPoint, innerPoint);
            double extension = Math.Max(distToInner * 0.3, 10.0);

            return new XPoint(
                outerPoint.X,
                goUp ? outerPoint.Y - extension : outerPoint.Y + extension
            );
        }



        private static void DisplayLineAngle(XPoint outerPoint, XPoint innerPoint, string nazwa = "")
        {
            // Oblicz wektor między punktami
            double dx = innerPoint.X - outerPoint.X;
            double dy = innerPoint.Y - outerPoint.Y;

            // Oblicz długość
            double length = Math.Sqrt(dx * dx + dy * dy);

            if (length < 0.001)
            {
            
                Console.WriteLine($"⚠️ {nazwa}: Punkty są identyczne lub bardzo blisko siebie!");
                Console.WriteLine($"   outerPoint: ({outerPoint.X:F2}, {outerPoint.Y:F2})");
                Console.WriteLine($"   innerPoint: ({innerPoint.X:F2}, {innerPoint.Y:F2})");
                return;
            }

            // Oblicz kąt w radianach
            double angleRad = Math.Atan2(dy, dx);

            // Konwersja na stopnie
            double angleDeg = angleRad * 180.0 / Math.PI;

            // Normalizacja do zakresu 0-360
            if (angleDeg < 0) angleDeg += 360.0;

            // Oblicz wektor jednostkowy
            double tx = dx / length;
            double ty = dy / length;

            Console.WriteLine($"📐 {nazwa} KĄT LINII:");
            Console.WriteLine($"   outerPoint: ({outerPoint.X:F2}, {outerPoint.Y:F2})");
            Console.WriteLine($"   innerPoint: ({innerPoint.X:F2}, {innerPoint.Y:F2})");
            Console.WriteLine($"   Wektor: dx={dx:F2}, dy={dy:F2}");
            Console.WriteLine($"   Długość: {length:F2}");
            Console.WriteLine($"   Kąt: {angleDeg:F2}° (rad: {angleRad:F4})");
            Console.WriteLine($"   Wektor jednostkowy: ({tx:F4}, {ty:F4})");
            Console.WriteLine($"   Kierunek: {(dy > 0 ? "w dół" : "w górę")}");

            // Określ kierunek kardynalny
            string direction = "";
            if (angleDeg >= 337.5 || angleDeg < 22.5) direction = "→ w prawo (poziomo)";
            else if (angleDeg >= 22.5 && angleDeg < 67.5) direction = "↘ w prawo w dół";
            else if (angleDeg >= 67.5 && angleDeg < 112.5) direction = "↓ w dół (pionowo)";
            else if (angleDeg >= 112.5 && angleDeg < 157.5) direction = "↙ w lewo w dół";
            else if (angleDeg >= 157.5 && angleDeg < 202.5) direction = "← w lewo (poziomo)";
            else if (angleDeg >= 202.5 && angleDeg < 247.5) direction = "↖ w lewo w górę";
            else if (angleDeg >= 247.5 && angleDeg < 292.5) direction = "↑ w górę (pionowo)";
            else if (angleDeg >= 292.5 && angleDeg < 337.5) direction = "↗ w prawo w górę";

            Console.WriteLine($"   Kierunek: {direction}");
            Console.WriteLine();
        }


        private static XPoint ExtendInnerPointToOuterContour(
        XPoint innerPoint,
        XPoint outerPoint,
        XPoint otherOuterPoint,
        List<ContourSegment> outerContour)
        {
            if (outerContour == null || outerContour.Count == 0)
                return innerPoint;

            // Szukamy punktu na outerContour o tej samej współrzędnej X co outerPoint
            XPoint bestPoint = innerPoint;
            double minDistance = double.MaxValue;

            foreach (var seg in outerContour)
            {
                if (seg.Type == SegmentType.Line)
                {
                    double minX = Math.Min(seg.Start.X, seg.End.X);
                    double maxX = Math.Max(seg.Start.X, seg.End.X);

                    if (outerPoint.X >= minX - 0.5 && outerPoint.X <= maxX + 0.5)
                    {
                        double t = (seg.End.X - seg.Start.X) != 0
                            ? (outerPoint.X - seg.Start.X) / (seg.End.X - seg.Start.X)
                            : 0;
                        double y = seg.Start.Y + t * (seg.End.Y - seg.Start.Y);

                        XPoint candidate = new XPoint(outerPoint.X, y);

                        // Sprawdź czy punkt jest wyżej (mniejsza Y)
                        if (candidate.Y < outerPoint.Y)
                        {
                            double dist = Distance(outerPoint, candidate);
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                bestPoint = candidate;
                            }
                        }
                    }
                }
                else if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    // Sprawdź punkty na łuku
                    int steps = 50;
                    for (int i = 0; i <= steps; i++)
                    {
                        double t = (double)i / steps;
                        double startAngle = Math.Atan2(seg.Start.Y - seg.Center.Value.Y, seg.Start.X - seg.Center.Value.X);
                        double endAngle = Math.Atan2(seg.End.Y - seg.Center.Value.Y, seg.End.X - seg.Center.Value.X);

                        if (seg.CounterClockwise)
                        {
                            while (endAngle <= startAngle) endAngle += 2 * Math.PI;
                        }
                        else
                        {
                            while (endAngle >= startAngle) endAngle -= 2 * Math.PI;
                        }

                        double angle = startAngle + t * (endAngle - startAngle);

                        XPoint candidate = new XPoint(
                            seg.Center.Value.X + seg.Radius * Math.Cos(angle),
                            seg.Center.Value.Y + seg.Radius * Math.Sin(angle)
                        );

                        if (Math.Abs(candidate.X - outerPoint.X) < 0.5)
                        {
                            if (candidate.Y < outerPoint.Y)
                            {
                                double dist = Distance(outerPoint, candidate);
                                if (dist < minDistance)
                                {
                                    minDistance = dist;
                                    bestPoint = candidate;
                                }
                            }
                        }
                    }
                }
            }

            // Jeśli znaleziono punkt, użyj go
            if (minDistance < double.MaxValue && Distance(bestPoint, innerPoint) > 0.01)
            {
                return bestPoint;
            }

            // Fallback - przedłużenie o 30% w górę
            double dy = innerPoint.Y - outerPoint.Y;
            return new XPoint(outerPoint.X, outerPoint.Y + dy * 1.3);
        }

        private static XPoint? FindRayArcIntersection(
        XPoint rayStart,
        XPoint rayEnd,
        ContourSegment arc)
        {
            if (!arc.Center.HasValue)
                return null;


            double dx = rayEnd.X - rayStart.X;
            double dy = rayEnd.Y - rayStart.Y;


            double fx = rayStart.X - arc.Center.Value.X;
            double fy = rayStart.Y - arc.Center.Value.Y;


            double a = dx * dx + dy * dy;

            if (a < 0.000001)
                return null;


            double b = 2 * (fx * dx + fy * dy);

            double c = fx * fx + fy * fy -
                       arc.Radius * arc.Radius;


            double delta = b * b - 4 * a * c;


            if (delta < 0)
                return null;


            double sqrtDelta = Math.Sqrt(delta);


            double t1 = (-b - sqrtDelta) / (2 * a);
            double t2 = (-b + sqrtDelta) / (2 * a);


            List<double> candidates = new();


            // tylko punkty w kierunku promienia
            if (t1 >= 0 && t1 <= 1)
                candidates.Add(t1);


            if (t2 >= 0 && t2 <= 1)
                candidates.Add(t2);


            if (candidates.Count == 0)
                return null;


            // bierzemy najbliższy punkt
            double t = candidates.Min();


            XPoint p = new XPoint(
                rayStart.X + dx * t,
                rayStart.Y + dy * t);



            // sprawdzamy czy faktycznie jest na fragmencie łuku
            if (!IsPointOnArc(p, arc, 1.0))
                return null;


            return p;
        }

        private static XPoint? LineIntersection(
        XPoint p1,
        XPoint p2,
        XPoint p3,
        XPoint p4)
        {
            double den =
                (p1.X - p2.X) * (p3.Y - p4.Y) -
                (p1.Y - p2.Y) * (p3.X - p4.X);


            if (Math.Abs(den) < 0.00001)
                return null;


            double x =
                ((p1.X * p2.Y - p1.Y * p2.X) * (p3.X - p4.X) -
                (p1.X - p2.X) * (p3.X * p4.Y - p3.Y * p4.X))
                / den;


            double y =
                ((p1.X * p2.Y - p1.Y * p2.X) * (p3.Y - p4.Y) -
                (p1.Y - p2.Y) * (p3.X * p4.Y - p3.Y * p4.X))
                / den;


            return new XPoint(x, y);
        }

        /// <summary>
        /// Filtruje segmenty konturu dla danej strony
        /// </summary>
        private List<ContourSegment> GetSegmentsForSide(List<ContourSegment> contour, string strona)
        {
            if (contour == null || contour.Count == 0)
                return contour;

            // Dla konturu z 7 segmentami:
            // Indeksy: 0-3 = górne łuki, 4 = prawa linia, 5 = dolna linia, 6 = lewa linia

            switch (strona)
            {
                case "Góra":
                    // Zwróć tylko segmenty z łukami (indeksy 0-3)
                    return contour.Take(4).ToList();

                case "Prawa":
                    // Zwróć tylko segmenty prawej strony (indeks 4)
                    return contour.Skip(4).Take(1).ToList();

                case "Dół":
                    // Zwróć tylko segmenty dolnej strony (indeks 5)
                    return contour.Skip(5).Take(1).ToList();

                case "Lewa":
                    // Zwróć tylko segmenty lewej strony (indeks 6)
                    return contour.Skip(6).Take(1).ToList();

                default:
                    return contour;
            }
        }

        private ContourSegment BuildSegmentWithArc(
            XPoint start,
            XPoint end,
            List<ContourSegment> contourToSearch)
        {
            const double tolerance = 0.1;
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


        /// <summary>
        /// Znajduje łuk między dwoma punktami w konturze - ZWIĘKSZONA TOLERANCJA
        /// </summary>
        private ContourSegment FindArcBetweenPoints(
            List<ContourSegment> contour,
            XPoint point1,
            XPoint point2,
            double tolerance = 0.1)  // ← ZWIĘKSZONA TOLERANCJA
        {
            if (contour == null || contour.Count == 0)
                return null;

            foreach (var seg in contour)
            {
                if (seg.Type != SegmentType.Arc || seg.Center == null)
                    continue;

                // Sprawdź czy oba punkty leżą na tym łuku
                bool pt1OnArc = IsPointOnArc(point1, seg, tolerance);
                bool pt2OnArc = IsPointOnArc(point2, seg, tolerance);

                if (pt1OnArc && pt2OnArc)
                {
                    return seg;
                }
            }

            return null;
        }

        /// <summary>
        /// Sprawdza czy punkt leży na łuku - ZWIĘKSZONA TOLERANCJA
        /// </summary>
        private static bool IsPointOnArc(XPoint point, ContourSegment arc, double tolerance = 0.1)
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

        private List<XPoint> GetStartT1(XPoint _innerP, XPoint _outerP, List<XPoint> _outer, float angleDegrees,
        float prevangleDegrees, float nextangleDegrees, string stronaWModelu,
        string stonaOstanioDodanegoElementu, int nk)
        {
            List<XPoint> intersections = new List<XPoint>();

            bool czyParzysta = (nk + 1) % 2 == 0;

            bool warunek = false;

            if (nk < 0)
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

        private XPoint? GetLinesIntersectionNullable(XPoint a1, XPoint a2, XPoint b1, XPoint b2)
        {
            float dx1 = (float)(a2.X - a1.X);
            float dy1 = (float)(a2.Y - a1.Y);
            float dx2 = (float)(b2.X - b1.X);
            float dy2 = (float)(b2.Y - b1.Y);

            float det = dx1 * dy2 - dy1 * dx2;

            if (Math.Abs(det) < 1e-6f)
            {
                return null; // linie są równoległe
            }

            float t = ((float)(b1.X - a1.X) * dy2 - (float)(b1.Y - a1.Y) * dx2) / det;

            return new XPoint(
                a1.X + t * dx1,
                a1.Y + t * dy1
            );
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

        private List<XPoint> GetLineCircleIntersections(XPoint p1, XPoint p2, XPoint center, double radius)
        {
            // parametry prostej p = p1 + t*(p2-p1), t dowolne
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            double fx = p1.X - center.X;
            double fy = p1.Y - center.Y;

            double a = dx * dx + dy * dy;
            double b = 2 * (fx * dx + fy * dy);
            double c = fx * fx + fy * fy - radius * radius;

            double discriminant = b * b - 4 * a * c;
            var results = new List<XPoint>();
            if (discriminant < -1e-9) return results;

            discriminant = Math.Max(0, discriminant);
            double sqrtD = Math.Sqrt(discriminant);

            double t1 = (-b + sqrtD) / (2 * a);
            double t2 = (-b - sqrtD) / (2 * a);

            // tu rozważamy dowolne przecięcia (wielokąt offset tworzy linie nieskończone), ale preferujemy punkty na odcinku
            var pA = new XPoint(p1.X + t1 * dx, p1.Y + t1 * dy);
            var pB = new XPoint(p1.X + t2 * dx, p1.Y + t2 * dy);
            results.Add(pA);
            if (discriminant > 1e-12) results.Add(pB);

            return results;
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
