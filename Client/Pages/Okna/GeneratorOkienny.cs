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

        // Lista wierzcholkow linie i łuki (w kolejnosci zgodnej z ruchem wskazowek zegara)

        public List<XPoint> wewnetrznyKontur; // przechowuje obliczony wewnętrzny kontur po offsetowaniu

        public List<XPoint> liniaSzkleniaKontur;// przechowuje obliczony kontur linii szklenia (jeśli dotyczy)
        public List<ContourSegment> zewnetrznyKonturZLukami { get; set; } = new();

        public List<ContourSegment> wewnetrznyKonturZLukami; // przechowuje obliczony wewnętrzny kontur po offsetowaniu

        public List<ContourSegment> liniaSzkleniaKonturZLukami;// przechowuje obliczony kontur linii szklenia (jeśli dotyczy)

        public List<XPoint> liniaOkuciaKontur;// przechowuje obliczony kontur linii szklenia (jeśli dotyczy)

        public List<ContourSegment> liniaOkuciaKonturZLukami;// przechowuje obliczony kontur linii szklenia (jeśli dotyczy)
        public ConstWlasciwosciOkna constWlasciwosciOkna { get; set; } = new(); // przechowuje stałe punkty do wyświetlania właściwości okna (np. w panelu bocznym)
        public List<ShapeRegion> Region { get; set; } = new();
        public string StronaElementu { get; set; } = "";

        private readonly IJSRuntime _jsRuntime;

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
                Console.WriteLine($"❌ AddElements Brak KonfiguracjeSystemu lub PowiazanyModel!");
                return false;
            }

            if (EdytowanyModel == null)
            {
                Console.WriteLine($"❌ AddElements Brak EdytowanyModel jest nie ustawiony!!!");
                return false;
            }

            Console.WriteLine($"➡️ AddElements EdytowanyModel.PolaczenieNaroza: {EdytowanyModel.PolaczenieNaroza} daneKwadratu.Count: {(daneKwadratu == null ? "NULL" : daneKwadratu.Count())}");

            if (punktyRegionuMaster != null)
            {
                Console.WriteLine($"➡️ AddElements punktyRegionuMaster.Count: {punktyRegionuMaster.Count()}");
            }

            Console.WriteLine($"📏 AddElements Szerokosc: {Szerokosc}, Wysokosc: {Wysokosc}");

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
                Console.WriteLine($"❌ AddElements Nie znaleziono regionu o ID: {regionId} w AddElements - GeneratoryOkienne");
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
                Console.WriteLine($"❌ AddElements Region o ID: {regionId} ma zbyt mało punktów");
                return false;
            }

            if ((punkty == null || punkty.Count < 2))
            {
                Console.WriteLine($"❌ AddElements Region o ID: {regionId} ma zbyt mało punktów! punkty.Count: {punkty.Count}");
                return false;
            }

            Console.WriteLine($"🟩 AddElements Generuj okno dla regionu ID {regionId} typu: {region.TypKsztaltu} ElementLiniowy: {ElementLiniowy} punkty.Count: {punkty.Count()}");

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

            Console.WriteLine($"🔹 przeskalowanePunktyZLukami: {przeskalowanePunktyZLukami.Count} w tym linii: {przeskalowanePunktyZLukami.Where(x => x.Type == SegmentType.Line).Count()} i łuki: {przeskalowanePunktyZLukami.Where(x => x.Type == SegmentType.Arc).Count()}");

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


            var konfLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa &&
                        (string.IsNullOrEmpty(slruchPoLewej) || e.Typ == slruchPoLewej) || (string.IsNullOrEmpty(slruchPoPrawej) || e.Typ == slruchPoPrawej));


            var konfRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa &&
                        (string.IsNullOrEmpty(slruchPoPrawej) || e.Typ == slruchPoPrawej) || (string.IsNullOrEmpty(slruchPoLewej) || e.Typ == slruchPoLewej));

            var konfTop = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeGora);

            var konfBottom = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeDol);

            // 🔧 Profile z konfiguracji
            //float profileLeft = (float)((konfLeft?.PionPrawa ?? 0) - (konfLeft?.PionLewa ?? 0));
            //float profileRight = (float)((konfRight?.PionPrawa ?? 0) - (konfRight?.PionLewa ?? 0));
            //float profileTop = (float)((konfTop?.PionPrawa ?? 0) - (konfTop?.PionLewa ?? 0));
            //float profileBottom = (float)((konfBottom?.PionPrawa ?? 0) - (konfBottom?.PionLewa ?? 0));

            float profileLeft = ObliczRoznicePoziomow(konfLeft, ElementLiniowy);
            float profileRight = ObliczRoznicePoziomow(konfRight, ElementLiniowy);
            float profileTop = ObliczRoznicePoziomow(konfTop, ElementLiniowy);
            float profileBottom = ObliczRoznicePoziomow(konfBottom, ElementLiniowy);

            float offsetGlassLeft = ObliczRoznicePoziomowSzyba(konfLeft, ElementLiniowy);
            float offsetGlassRight = ObliczRoznicePoziomowSzyba(konfRight, ElementLiniowy);
            float offsetGlassTop = ObliczRoznicePoziomowSzyba(konfTop, ElementLiniowy);
            float offsetGlassBottom = ObliczRoznicePoziomowSzyba(konfBottom, ElementLiniowy);

            if (offsetGlassLeft > 0) offsetGlassLeft = profileLeft - offsetGlassLeft;
            if (offsetGlassRight > 0) offsetGlassRight = profileRight - offsetGlassRight;
            if (offsetGlassTop > 0) offsetGlassTop = profileTop - offsetGlassTop;
            if (offsetGlassBottom > 0) offsetGlassBottom = profileBottom - offsetGlassBottom;

            Console.WriteLine($"🔧 Profile z konfiguracji przed korektą: profileLeft: {profileLeft} profileRight: {profileRight} profileTop: {profileTop} profileBottom: {profileBottom}");
            Console.WriteLine($"🔧 Profile z konfiguracji przed korektą: offsetGlassLeft: {offsetGlassLeft} offsetGlassRight: {offsetGlassRight} offsetGlassTop: {offsetGlassTop} offsetGlassBottom: {offsetGlassBottom}");

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

                // Napraw punkty startowe jeśli potrzebne
                //   wewnetrznyKonturZLukami = FixStartPoints(wewnetrznyKonturZLukami);

                punktyRegionuMaster = CalculateOffsetPolygon(punktyRegionuMaster, profileLeft, profileRight, profileTop, profileBottom, false);

                //foreach (var test in punktyRegionuMaster)
                //{
                //    Console.WriteLine($"🔷🔷🔷🔷🔷🔷🔷🔷 punktyRegionuMaster 2 Wierzcholek X: {test.X} Y: {test.Y} / {punktyRegionuMaster.Count}");
                //}
            }
            else
            {
                wewnetrznyKontur = CalculateOffsetPolygon(
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

                wewnetrznyKonturZLukami = CalculateOffsetPolygonKontur(przeskalowanePunktyZLukami,
                profileLeft, profileRight, profileTop, profileBottom,
                false); // dla modeli z łukami i liniami

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

                liniaSzkleniaKontur = CalculateOffsetPolygon(
                    przeskalowanePunkty,
                    offsetGlassLeft, offsetGlassRight, offsetGlassTop, offsetGlassBottom,
                    false);

                //Console.WriteLine($"offsetLeft, offsetRight, offsetTop, offsetBottom, {offsetLeft}, {offsetRight}, {offsetTop}, {offsetBottom}");
                liniaSzkleniaKonturZLukami = CalculateOffsetPolygonKontur(przeskalowanePunktyZLukami,
                    offsetGlassLeft, offsetGlassRight, offsetGlassTop, offsetGlassBottom,
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

            Console.WriteLine($"▶️ Generowanie elementów dla regionu {regionId} z typem kształtu: {typKsztalt} oraz ElementLiniowy: {ElementLiniowy} profileLeft: {profileLeft}, profileRight :{profileRight}");

            // Użyj oryginalnych segmentów (nieposortowanych) - one i tak będą dopasowane przez Build4SegmentContour
            // outerContourSegment i innerContourSegment pozostają BEZ ZMIAN
            float angleDegreesElementLionowy = 0;

            float katGornegoElemntu = GetTopEdgeAngleFromFirstSegment(outer);

            // 🔹 Nowy tryb – jeśli to tylko element liniowy (np. słupek)
            if (ElementLiniowy)
            {
                if (outer == null || outer.Count < 2)
                {
                    Console.WriteLine("▶️ Element: brak wystarczającej liczby punktów (min. 2 wymagane).");
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

                Console.WriteLine($"▶️ Element X wartość X: {szukDaneKwadratu.Wierzcholki[0].X} dotyczy: ElementLiniowy: {ElementLiniowy} rowIdprofileLeft: {rowIdprofileLeft} rowIdprofileRight: {rowIdprofileRight} rowIdprofileTop: {rowIdprofileTop} rowIdprofileBottom: {rowIdprofileBottom}");

                if (szukDaneKwadratu != null)
                {
                    Console.WriteLine($"▶️ Element model.Count:{model.Count()} szukDaneKwadratu.Wierzcholki.Count: {szukDaneKwadratu?.Wierzcholki.Count()} RuchomySlupekPoLewej:{RuchomySlupekPoLewej} RuchomySlupekPoPrawej:{RuchomySlupekPoPrawej}");

                    foreach (var dk in szukDaneKwadratu.Wierzcholki)
                    {
                        Console.WriteLine($"▶️ ElementX:{dk.X} Y:{dk.Y}");
                    }
                }

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
                throw new Exception("Wielokąt musi mieć co najmniej 3 wierzchołki.");

            outer = RemoveDuplicateConsecutivePoints(outer);
            inner = RemoveDuplicateConsecutivePoints(inner);

            Console.WriteLine($"▶️ Generuje elementy z polygon with vertexCount: {vertexCount} vertices and joins: {polaczenia} angleDegreesElementLionowy: {angleDegreesElementLionowy}");

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
                int kat = int.Parse(parts[0]);
                string typ = parts[1];

                string strona = StronaOknaHelper.OkreslStrone(kat);
                wzorzecPolaczen[strona] = typ;

             //   Console.WriteLine($"📐 Wzorzec: kąt {kat}° → strona {strona} → typ {typ}");
            }

            Console.WriteLine($"🔷🔷 Wzorzec połączeń dla stron: outer: {outer.Count} vertexCount:{vertexCount}");

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
                        string typ = wzorzecPolaczen.ContainsKey(stronaA) ? wzorzecPolaczen[stronaA] : "T3";
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
                        string typ = wzorzecPolaczen.ContainsKey(stronaB) ? wzorzecPolaczen[stronaB] : "T3";
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
                string typBiezacej = wzorzecPolaczen.ContainsKey(currentSide) ? wzorzecPolaczen[currentSide] : "T3";
                string typPoprzedniej = wzorzecPolaczen.ContainsKey(prevSide) ? wzorzecPolaczen[prevSide] : "T3";
                string typNastepnej = wzorzecPolaczen.ContainsKey(nextSide) ? wzorzecPolaczen[nextSide] : "T3";

                Console.WriteLine($"▶️🔷🔷▶️ Processing element {i + 1}/{vertexCount} with joins: {leftJoin} - {rightJoin} " +
                                 $"wyliczony kąt: {angleDegrees:F2}° dla i: {i} StronaElementu: {currentSide} " +
                                 $"(prev: {prevSide} [{typPoprzedniej}], next: {nextSide} [{typNastepnej}])");

                Console.WriteLine($"   📍 Narożniki: lewy ({prevSide}-{currentSide}) = {leftJoin}, " +
                                 $"prawy ({currentSide}-{nextSide}) = {rightJoin}");

                bool dodajA = false;
                bool dodajB = false;
                XPoint outerStart = outer[i];
                XPoint outerEnd = outer[next];

                XPoint _innerStart = inner[i];
                XPoint _innerEnd = inner[next];
                float length = MathF.Sqrt(dx * dx + dy * dy);

                StronaElementu = StronaOknaHelper.OkreslStrone(angleDegrees, i, outer);

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
                    angleDegreesStronaA = 360 - angleDegreesStronaA; // Kąt po drugiej stronie
                }

                double crossProductNext = (currentDx * nextDy - currentDy * nextDx);
                if (crossProductNext < 0)
                {
                    angleDegreesStronaB = 360 - angleDegreesStronaB;
                }

                // Teraz możesz użyć angleDegreesStronaA i angleDegreesStronaB
                Console.WriteLine($"Wierzchołek {i}: Kąt z poprzednim = {angleDegreesStronaA:F1}°, Kąt z następnym = {angleDegreesStronaB:F1}°");

                if (angleDegreesStronaB < 20)
                {
                    // Jeśli kąt z następnym jest bardzo mały, traktujemy to jako prawie prostą linię → potencjalnie T1
                    rightJoin = "T2"; // połączone równym kątem
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
                List<XPoint>? wierzcholkiStycznePodLuki;
                List<ContourSegment>? wierzcholkiZLukami;

                Console.WriteLine($"🔷 element --> {i + 1}/{vertexCount} with joins: {leftJoin} - {rightJoin} angleDegrees: {angleDegrees} katGornegoElemntu: {katGornegoElemntu} StronaElementu: {StronaElementu}");

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

                            wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                                i,
                                next,
                                prev,
                                wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                                outerContourSegment,
                                innerContourSegment,
                                leftJoin,     // przekaż typy połączeń do funkcji
                                rightJoin,
                                outer,        // przekaż outer do ew. przeliczeń T3
                                angleDegrees,
                                anglePrev,
                                angleNext,
                                StronaElementu,
                                stonaOstanioDodanegoElementu,
                                vertexCount,
                                firstangleDegrees
                            );

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

                                wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                                    i,
                                    next,
                                    prev,
                                    wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                                    outerContourSegment,
                                    innerContourSegment,
                                    leftJoin,     // przekaż typy połączeń do funkcji
                                    rightJoin,
                                    outer,        // przekaż outer do ew. przeliczeń T3
                                    angleDegrees,
                                    anglePrev,
                                    angleNext,
                                    StronaElementu,
                                    stonaOstanioDodanegoElementu,
                                    vertexCount,
                                    firstangleDegrees
                                );

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
                                        Console.WriteLine($"🔷 Wyliczono dla elementu {i + 1} angleNext: {angleNext} angleDegrees: {angleDegrees} firstangleDegrees: {firstangleDegrees} anglePrev: {anglePrev}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"🔷 Szukanie przecięcia dla elementu {i + 1} z anglePrevDegrees: {anglePrev}");
                                        outerTop = FindFirstEdgeIntersectionByAngle(innerTop, anglePrev, outer);
                                    }

                                }

                                wierzcholki = new List<XPoint> {
                                outerTop, outerBottom, innerBottom, innerTop
                                };

                                wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                                    i,
                                    next,
                                    prev,
                                    wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                                    outerContourSegment,
                                    innerContourSegment,
                                    leftJoin,     // przekaż typy połączeń do funkcji
                                    rightJoin,
                                    outer,        // przekaż outer do ew. przeliczeń T3
                                    angleDegrees,
                                    anglePrev,
                                    angleNext,
                                    StronaElementu,
                                    stonaOstanioDodanegoElementu,
                                    vertexCount,
                                    firstangleDegrees
                                );

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

                            wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                                i,
                                next,
                                prev,
                                wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                                outerContourSegment,
                                innerContourSegment,
                                leftJoin,     // przekaż typy połączeń do funkcji
                                rightJoin,
                                outer,        // przekaż outer do ew. przeliczeń T3
                                angleDegrees,
                                anglePrev,
                                angleNext,
                                StronaElementu,
                                stonaOstanioDodanegoElementu,
                                vertexCount,
                                firstangleDegrees
                            );

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
                                Console.WriteLine($"🔷 Szukanie przecięcia dla elementu {i + 1} z anglePrev: {anglePrev}, zmienna angleDegrees: {angleDegrees} angleNext: {angleNext} anglePrev: {anglePrev}");

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

                            wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                                i,
                                next,
                                prev,
                                wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                                outerContourSegment,
                                innerContourSegment,
                                leftJoin,     // przekaż typy połączeń do funkcji
                                rightJoin,
                                outer,        // przekaż outer do ew. przeliczeń T3
                                angleDegrees,
                                anglePrev,
                                angleNext,
                                StronaElementu,
                                stonaOstanioDodanegoElementu,
                                vertexCount,
                                firstangleDegrees
                            );

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

                    wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                        i,
                        next,
                        prev,
                        wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                        outerContourSegment,
                        innerContourSegment,
                        leftJoin,     // przekaż typy połączeń do funkcji
                        rightJoin,
                        outer,        // przekaż outer do ew. przeliczeń T3
                        angleDegrees,
                        anglePrev,
                        angleNext,
                        StronaElementu,
                        stonaOstanioDodanegoElementu,
                        vertexCount,
                        firstangleDegrees
                    );

                }
                else if (leftJoin == "T1" && rightJoin == "T1")
                {
                    Console.WriteLine($"🔷 T1/T1 element {i + 1} START isAlmostHorizontal: {isAlmostHorizontal} isAlmostVertical: {isAlmostVertical} vertexCount: {vertexCount} angleDegrees: {angleDegrees} firstangleDegrees: {firstangleDegrees}");
                    
                    List<XPoint> getStartT1 = GetStartT1(inner[i], outer[i], outer, angleDegrees, anglePrev, angleNext,
                        StronaElementu, stonaOstanioDodanegoElementu, vertexCount < 6 || (vertexCount == 7 && angleDegrees > 299) ? -1 : i);


                    List<XPoint> getEndT1;
                    var _anglePrev = anglePrev;
                    if (i == vertexCount - 1)
                    {
                        _anglePrev = firstangleDegrees;
                    }
                    getEndT1 = GetEndT1(inner[next], outer[next], outer, angleDegrees, _anglePrev, angleNext, StronaElementu,
                        stonaOstanioDodanegoElementu, vertexCount < 6 || (vertexCount == 7 && angleDegrees > 270) ? -1 : i);

                    wierzcholki = new List<XPoint> {
                            getStartT1[1], getEndT1[1], getEndT1[0], getStartT1[0]
                        };

                    wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                        i,
                        next,
                        prev,
                        wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                        outerContourSegment,
                        innerContourSegment,
                        leftJoin,     // przekaż typy połączeń do funkcji
                        rightJoin,
                        outer,        // przekaż outer do ew. przeliczeń T3
                        angleDegrees,
                        anglePrev,
                        angleNext,
                        StronaElementu,
                        stonaOstanioDodanegoElementu,
                        vertexCount,
                        firstangleDegrees
                    );

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

                    wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                        i,
                        next,
                        prev,
                        wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                        outerContourSegment,
                        innerContourSegment,
                        leftJoin,     // przekaż typy połączeń do funkcji
                        rightJoin,
                        outer,        // przekaż outer do ew. przeliczeń T3
                        angleDegrees,
                        anglePrev,
                        angleNext,
                        StronaElementu,
                        stonaOstanioDodanegoElementu,
                        vertexCount,
                        firstangleDegrees
                    );

                }
                else if (leftJoin == "T2" && rightJoin == "T2")
                {

                    List<XPoint> getStartT2 = GetStartT2(inner[i], outer[i]);
                    List<XPoint> getEndT2 = GetEndT2(inner[next], outer[next]);

                    wierzcholki = new List<XPoint> {
                            getStartT2[1], getEndT2[1], getEndT2[0], getStartT2[0]
                        };

                    wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                        i,
                        next,
                        prev,
                        wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                        outerContourSegment,
                        innerContourSegment,
                        leftJoin,     // przekaż typy połączeń do funkcji
                        rightJoin,
                        outer,        // przekaż outer do ew. przeliczeń T3
                        angleDegrees,
                        anglePrev,
                        angleNext,
                        StronaElementu,
                        stonaOstanioDodanegoElementu,
                        vertexCount,
                        firstangleDegrees
                    );

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


                    wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                        i,
                        next,
                        prev,
                        wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                        outerContourSegment,
                        innerContourSegment,
                        leftJoin,     // przekaż typy połączeń do funkcji
                        rightJoin,
                        outer,        // przekaż outer do ew. przeliczeń T3
                        angleDegrees,
                        anglePrev,
                        angleNext,
                        StronaElementu,
                        stonaOstanioDodanegoElementu,
                        vertexCount,
                        firstangleDegrees
                    );

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


                    wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                        i,
                        next,
                        prev,
                        wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                        outerContourSegment,
                        innerContourSegment,
                        leftJoin,     // przekaż typy połączeń do funkcji
                        rightJoin,
                        outer,        // przekaż outer do ew. przeliczeń T3
                        angleDegrees,
                        anglePrev,
                        angleNext,
                        StronaElementu,
                        stonaOstanioDodanegoElementu,
                        vertexCount,
                        firstangleDegrees
                    );

                }
                else if (leftJoin == "T1" && rightJoin == "T2")
                {
                    Console.WriteLine($"🔷 T1/T2 element {i + 1} - kombinacja czopa (T1) ze ścięciem (T2)");

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
                            getStartT1[1], getEndT2[1], getEndT2[0], getStartT2[0]
                        };


                    wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                        i,
                        next,
                        prev,
                        wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                        outerContourSegment,
                        innerContourSegment,
                        leftJoin,     // przekaż typy połączeń do funkcji
                        rightJoin,
                        outer,        // przekaż outer do ew. przeliczeń T3
                        angleDegrees,
                        anglePrev,
                        angleNext,
                        StronaElementu,
                        stonaOstanioDodanegoElementu,
                        vertexCount,
                        firstangleDegrees
                    );

                }
                else if (leftJoin == "T3" && rightJoin == "T2")
                {
                    Console.WriteLine($"🔷 T3/T2 element {i + 1} - kombinacja pełnego profilu (T3) ze ścięciem (T2)");

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
                            getStartT3[1], getEndT2[1], getEndT2[0], getStartT3[0]
                    };


                    wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                        i,
                        next,
                        prev,
                        wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                        outerContourSegment,
                        innerContourSegment,
                        leftJoin,     // przekaż typy połączeń do funkcji
                        rightJoin,
                        outer,        // przekaż outer do ew. przeliczeń T3
                        angleDegrees,
                        anglePrev,
                        angleNext,
                        StronaElementu,
                        stonaOstanioDodanegoElementu,
                        vertexCount,
                        firstangleDegrees
                    );

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


                    wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                        i,
                        next,
                        prev,
                        wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                        outerContourSegment,
                        innerContourSegment,
                        leftJoin,     // przekaż typy połączeń do funkcji
                        rightJoin,
                        outer,        // przekaż outer do ew. przeliczeń T3
                        angleDegrees,
                        anglePrev,
                        angleNext,
                        StronaElementu,
                        stonaOstanioDodanegoElementu,
                        vertexCount,
                        firstangleDegrees
                    );

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


                    wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                        i,
                        next,
                        prev,
                        wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                        outerContourSegment,
                        innerContourSegment,
                        leftJoin,     // przekaż typy połączeń do funkcji
                        rightJoin,
                        outer,        // przekaż outer do ew. przeliczeń T3
                        angleDegrees,
                        anglePrev,
                        angleNext,
                        StronaElementu,
                        stonaOstanioDodanegoElementu,
                        vertexCount,
                        firstangleDegrees
                    );

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


                    wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                        i,
                        next,
                        prev,
                        wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                        outerContourSegment,
                        innerContourSegment,
                        leftJoin,     // przekaż typy połączeń do funkcji
                        rightJoin,
                        outer,        // przekaż outer do ew. przeliczeń T3
                        angleDegrees,
                        anglePrev,
                        angleNext,
                        StronaElementu,
                        stonaOstanioDodanegoElementu,
                        vertexCount,
                        firstangleDegrees
                    );

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


                    wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                        i,
                        next,
                        prev,
                        wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                        outerContourSegment,
                        innerContourSegment,
                        leftJoin,     // przekaż typy połączeń do funkcji
                        rightJoin,
                        outer,        // przekaż outer do ew. przeliczeń T3
                        angleDegrees,
                        anglePrev,
                        angleNext,
                        StronaElementu,
                        stonaOstanioDodanegoElementu,
                        vertexCount,
                        firstangleDegrees
                    );

                }
                else
                {
                    Console.WriteLine($"🔷 Wartość domyślna T2/T2 {i + 1} połączenia: {leftJoin}-{rightJoin}");

                    List<XPoint> getStartT2 = GetStartT2(inner[i], outer[i]);
                    List<XPoint> getEndT2 = GetEndT2(inner[next], outer[next]);

                    wierzcholki = new List<XPoint> {
                            getStartT2[1], getEndT2[1], getEndT2[0], getStartT2[0]
                        };


                    wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                        i,
                        next,
                        prev,
                        wierzcholki,  // zawsze przekazuj oryginalne wierzcholki
                        outerContourSegment,
                        innerContourSegment,
                        leftJoin,     // przekaż typy połączeń do funkcji
                        rightJoin,
                        outer,        // przekaż outer do ew. przeliczeń T3
                        angleDegrees,
                        anglePrev,
                        angleNext,
                        StronaElementu,
                        stonaOstanioDodanegoElementu,
                        vertexCount,
                        firstangleDegrees
                    );

                }


                // Budujemy pełny kontur 4-segmentowy
                wierzcholkiZLukami = Build4SegmentContour(wierzcholkiStycznePodLuki, outerContourSegment, innerContourSegment, i + 1);

                double regionMinX = wierzcholki.Min(p => p.X);
                double regionMaxX = wierzcholki.Max(p => p.X);
                double regionMinY = wierzcholki.Min(p => p.Y);
                double regionMaxY = wierzcholki.Max(p => p.Y);

                int wartoscX = (int)Math.Round(regionMaxX - regionMinX);
                int wartoscY = (int)Math.Round(regionMaxY - regionMinY);

                // Console.WriteLine($"leftJoin: {leftJoin} rightJoin:{rightJoin} wierzcholki: {wierzcholki.Count()} isAlmostVertical:{isAlmostVertical}");
                float bazowaDlugosc = DlugoscElementu(wierzcholki);

                Console.WriteLine($"▶️ Element Start switch {i + 1}/{vertexCount}: Length: {length}, StronaElementu :{StronaElementu}, angleDegreesElementLionowy:{angleDegreesElementLionowy}, Angle: {angleDegrees}°, Profile: {profile}, Wierzchołki: {wierzcholki.Count}, BazowaDlugosc: {bazowaDlugosc}, wartoscX: {wartoscX}, wartoscY: {wartoscY} ElementLiniowy:{ElementLiniowy} wierzcholki X0: {wierzcholki[0].X} Y0: {wierzcholki[0].Y}");

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

                Console.WriteLine($"▶️▶️▶️▶️ Element {i + 1}/{vertexCount} dodałem do ElementyRamyRysowane. Total elements now: {ElementyRamyRysowane.Count} - > rowIdProfil:{rowIdProfil} Angle: {angleDegrees}° leftJoin:{leftJoin} rightJoin:{rightJoin}");

                if (ElementLiniowy) return true;

            }

            await Task.CompletedTask;

            return true;
        }

        public List<ContourSegment> Build4SegmentContour(
        List<XPoint> wierzcholki,
        List<ContourSegment> outerContour,
        List<ContourSegment> innerContour,
        int nri)
        {
            if (wierzcholki == null || wierzcholki.Count != 4)
            {
                Console.WriteLine("❌ Build4SegmentContour: lista wierzchołków musi zawierać dokładnie 4 punkty");
                // Fallback - zwróć zwykłe linie
                var fallback = new List<ContourSegment>();
                for (int i = 0; i < wierzcholki.Count; i++)
                {
                    int next = (i + 1) % wierzcholki.Count;
                    fallback.Add(new ContourSegment(wierzcholki[i], wierzcholki[next])
                    {
                        Informacja = $"Fallback segment {i}"
                    });
                }
                return fallback;
            }

            // Sprawdź czy któryś z wierzchołków leży na łuku
            // W miejscu gdzie szukasz łuku zewnętrznego, dodaj sprawdzenie czy oba punkty są na łuku
            bool hasArc = false;
            XPoint? arcCenter = null;
            double arcRadius = 0;
            bool arcCW = false;

            // Sprawdź wszystkie pary segmentów, nie tylko (0-1)
            foreach (var seg in outerContour)
            {
                if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    // Sprawdź czy któreś dwa kolejne wierzchołki leżą na tym łuku
                    for (int i = 0; i < wierzcholki.Count; i++)
                    {
                        int next = (i + 1) % wierzcholki.Count;
                        double distToStart = Distance(seg.Start, wierzcholki[i]);
                        double distToEnd = Distance(seg.End, wierzcholki[next]);

                        // Sprawdź też odwrotną kolejność
                        bool match = (distToStart < 1.0 && distToEnd < 1.0) ||
                                    (Distance(seg.Start, wierzcholki[next]) < 1.0 &&
                                     Distance(seg.End, wierzcholki[i]) < 1.0);

                        if (match)
                        {
                            hasArc = true;
                            arcCenter = seg.Center.Value;
                            arcRadius = seg.Radius;
                            arcCW = seg.CounterClockwise;
                            Console.WriteLine($"✅ Znaleziono łuk dla par ({i},{next})");
                            break;
                        }
                    }
                    if (hasArc) break;
                }
            }

            if (hasArc && arcCenter != null)
            {
                // Mamy łuk na zewnętrznej krawędzi
                // Oblicz kąty dla punktów zewnętrznych
                double angle1 = Math.Atan2(wierzcholki[0].Y - arcCenter.Value.Y,
                                            wierzcholki[0].X - arcCenter.Value.X);
                double angle2 = Math.Atan2(wierzcholki[1].Y - arcCenter.Value.Y,
                                            wierzcholki[1].X - arcCenter.Value.X);

                // Znajdź odpowiadający łuk wewnętrzny (przesunięty do środka)
                ContourSegment innerArc = null;
                foreach (var seg in innerContour)
                {
                    if (seg.Type == SegmentType.Arc && seg.Center != null)
                    {
                        double dx = seg.Center.Value.X - arcCenter.Value.X;
                        double dy = seg.Center.Value.Y - arcCenter.Value.Y;
                        double dist = Math.Sqrt(dx * dx + dy * dy);

                        // Jeśli środek jest blisko (współśrodkowy)
                        if (dist < 1.0)
                        {
                            //seg.CounterClockwise = false; // Wewnętrzny łuk ma przeciwną orientację
                            innerArc = seg;
                            Console.WriteLine($"Build4SegmentContour: 🔷 Znaleziono łuk wewnętrzny: Center({seg.Center.Value.X},{seg.Center.Value.Y}) R={seg.Radius}  seg.CounterClockwise:{seg.CounterClockwise}");
                            break;
                        }
                    }
                }

                if (innerArc != null && innerArc.Center != null)
                {
                    double innerRadius = innerArc.Radius;
                    XPoint innerCenter = innerArc.Center.Value;

                    // Oblicz punkty wewnętrzne na tych samych kątach
                    XPoint innerStart = new XPoint(
                        innerCenter.X + innerRadius * Math.Cos(angle1),
                        innerCenter.Y + innerRadius * Math.Sin(angle1));
                    XPoint innerEnd = new XPoint(
                        innerCenter.X + innerRadius * Math.Cos(angle2),
                        innerCenter.Y + innerRadius * Math.Sin(angle2));

                    Console.WriteLine($"Build4SegmentContour: 🔷 innerStart({innerStart.X:F2},{innerStart.Y:F2}) innerEnd({innerEnd.X:F2},{innerEnd.Y:F2})");

                    var outerArcSeg = new ContourSegment(wierzcholki[0], wierzcholki[1],
                        arcCenter.Value, arcRadius, false);  // ← zawsze CCW

                    var innerArcSeg = new ContourSegment(innerEnd, innerStart,
                        innerCenter, innerRadius, true);   // ← zawsze CW

                    // Łączniki - proste linie
                    bool isClosedContour =
                    Distance(wierzcholki[0], wierzcholki[3]) < 1.0 ||
                    outerContour.Count >= 4 && outerContour.All(x => x.Type != SegmentType.Arc);

                    // 👉 jeśli kontur zamknięty - NIE DODAJEMY łączników
                    if (!isClosedContour)
                    {
                        var connector1 = new ContourSegment(wierzcholki[1], innerEnd)
                        {
                            Informacja = "Łącznik prawy"
                        };

                        var connector2 = new ContourSegment(innerStart, wierzcholki[0])
                        {
                            Informacja = "Łącznik lewy"
                        };

                        return new List<ContourSegment>
                        {
                            outerArcSeg,
                            connector1,
                            innerArcSeg,
                            connector2
                        };
                    }
                    else
                    {
                        // ✔ pełny zamknięty kontur
                        return new List<ContourSegment>
                        {
                            outerArcSeg,
                            innerArcSeg
                        };
                    }

                }
            }

            // Brak łuku - zwykłe linie (trapez lub prostokąt)
            var segments = new List<ContourSegment>();
            segments.Add(new ContourSegment(wierzcholki[0], wierzcholki[1]) { Informacja = "Góra" });
            segments.Add(new ContourSegment(wierzcholki[1], wierzcholki[2]) { Informacja = "Prawa" });
            segments.Add(new ContourSegment(wierzcholki[2], wierzcholki[3]) { Informacja = "Dół" });
            segments.Add(new ContourSegment(wierzcholki[3], wierzcholki[0]) { Informacja = "Lewa" });

            return segments;
        }


        private List<XPoint> GetWierzcholkiStycznePodLuki(
       int i,
       int next,
       int prev,
       List<XPoint> wierzcholki,
       List<ContourSegment> outerContourSegment,
       List<ContourSegment> innerContourSegment,
       string leftJoin = null,
       string rightJoin = null,
       List<XPoint> outer = null,
       float angleDegrees = 0,
       float anglePrev = 0,
       float angleNext = 0,
       string stronaElementu = null,
       string stonaOstanioDodanegoElementu = null,
       int vertexCount = 0,
       float firstangleDegrees = 0)
        {
            if (i < 0 || i >= outerContourSegment.Count() || outerContourSegment.Count() < 2)
                return wierzcholki;

            var currentSeg = outerContourSegment[i];
            var prevSeg = outerContourSegment[prev];
            var nextSeg = outerContourSegment[next];

            bool currentIsArc = currentSeg.Type == SegmentType.Arc;
            bool prevIsArc = prevSeg.Type == SegmentType.Arc;
            bool nextIsArc = nextSeg.Type == SegmentType.Arc;

            // Jeśli brak łuków - zwróć oryginalne wierzchołki bez zmian
            if (!currentIsArc && !prevIsArc && !nextIsArc)
            {
                return wierzcholki;
            }

            Console.WriteLine($"🔴 GetWierzcholkiStycznePodLuki[{i}]: łuki - current:{currentIsArc}, prev:{prevIsArc}, next:{nextIsArc}");

            // 🔥 PRZYPADEK 1: Aktualny segment jest łukiem - użyj oryginalnej logiki
            if (currentIsArc && currentSeg.Center != null)
            {
                var innerSeg = innerContourSegment
                    .FirstOrDefault(x =>
                        x.Type == SegmentType.Arc &&
                        x.Center != null &&
                        Distance(x.Center.Value, currentSeg.Center.Value) < 1.0);

                if (innerSeg != null)
                {
                    Console.WriteLine($"🔷 [{i}] ŁUK: używam punktów z konturu (oryginalna logika)");
                    return new List<XPoint>
                    {
                        currentSeg.Start,        // outer start
                        currentSeg.End,          // outer end
                        innerSeg.End,            // inner end
                        innerSeg.Start           // inner start
                    };
                }
            }

            // 🔥 PRZYPADEK 2: Linia styka się z łukiem - użyj punktów z konturu sąsiada
            var result = new List<XPoint>
            {
                new XPoint(wierzcholki[0].X, wierzcholki[0].Y),
                new XPoint(wierzcholki[1].X, wierzcholki[1].Y),
                new XPoint(wierzcholki[2].X, wierzcholki[2].Y),
                new XPoint(wierzcholki[3].X, wierzcholki[3].Y)
            };

            // Lewe połączenie z łukiem
            if (prevIsArc)
            {
                var prevOuter = outerContourSegment[prev];
                var prevInner = innerContourSegment[prev];

                // Sprawdź który koniec łuku jest bliżej naszego punktu
                double distToStart = Distance(prevOuter.Start, result[0]);
                double distToEnd = Distance(prevOuter.End, result[0]);

                if (distToEnd < distToStart)
                {
                    // Koniec łuku łączy się z nami
                    result[0] = new XPoint(prevOuter.End.X, prevOuter.End.Y);
                    if (prevInner.Type == SegmentType.Arc)
                    {
                        // Dla łuku wewnętrznego, koniec odpowiada naszemu inner start
                        result[3] = new XPoint(prevInner.End.X, prevInner.End.Y);
                    }
                }
                else
                {
                    // Początek łuku łączy się z nami
                    result[0] = new XPoint(prevOuter.Start.X, prevOuter.Start.Y);
                    if (prevInner.Type == SegmentType.Arc)
                    {
                        result[3] = new XPoint(prevInner.Start.X, prevInner.Start.Y);
                    }
                }
                Console.WriteLine($"🔷 [{i}] Lewy bok z łuku: outer=[{result[0].X:F2},{result[0].Y:F2}] inner=[{result[3].X:F2},{result[3].Y:F2}]");
            }

            // Prawe połączenie z łukiem
            if (nextIsArc)
            {
                var nextOuter = outerContourSegment[next];
                var nextInner = innerContourSegment[next];

                double distToStart = Distance(nextOuter.Start, result[1]);
                double distToEnd = Distance(nextOuter.End, result[1]);

                if (distToStart < distToEnd)
                {
                    // Początek łuku łączy się z nami
                    result[1] = new XPoint(nextOuter.Start.X, nextOuter.Start.Y);
                    if (nextInner.Type == SegmentType.Arc)
                    {
                        result[2] = new XPoint(nextInner.Start.X, nextInner.Start.Y);
                    }
                }
                else
                {
                    // Koniec łuku łączy się z nami
                    result[1] = new XPoint(nextOuter.End.X, nextOuter.End.Y);
                    if (nextInner.Type == SegmentType.Arc)
                    {
                        result[2] = new XPoint(nextInner.End.X, nextInner.End.Y);
                    }
                }
                Console.WriteLine($"🔷 [{i}] Prawy bok z łuku: outer=[{result[1].X:F2},{result[1].Y:F2}] inner=[{result[2].X:F2},{result[2].Y:F2}]");
            }

            return result;
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
        private float ObliczRoznicePoziomow(KonfSystem? konf, bool slupekStaly)
        {
            if (konf == null)
                return 0;
            if (!slupekStaly)
            {
                float gora = (float)konf.PoziomGora;
                float dol = (float)konf.PoziomDol;

                // Jeśli jedno z pól jest 0, traktuj drugie jako wartość symetryczną
                if (gora == 0 && dol != 0)
                    return Math.Abs(dol);

                if (dol == 0 && gora != 0)
                    return Math.Abs(gora);

                return Math.Abs(gora - dol);
            }
            else
            {
                //Słupki stałe mają zawsze pełną wartość profilu, niezależnie od poziomów pozostałe dane z tabeli KonfPolaczenia
                return 0;
            }

        }

        private float ObliczRoznicePoziomowSzyba(KonfSystem? konf, bool slupekStaly)
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

                return Math.Abs(gora - dol);
            }
            else
            {
                //Słupki stałe mają zawsze pełną wartość profilu, niezależnie od poziomów pozostałe dane z tabeli KonfPolaczenia
                Console.BackgroundColor = ConsoleColor.Green;
                Console.WriteLine("Słupki stałe mają zawsze pełną wartość profilu, niezależnie od poziomów pozostałe dane z tabeli KonfPolaczenia");
                return 0;
            }

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

        public List<XPoint> CalculateOffsetPolygon(
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
                throw new ArgumentException("Figura musi mieć co najmniej 2 punkty.");

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
                throw new ArgumentException("Wielokąt musi mieć co najmniej 3 punkty.");

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

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------


        public List<ContourSegment> CalculateOffsetPolygonKontur(
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
                    Console.WriteLine("⚠️ Element liniowy z niestandardowymi segmentami - zwracam oryginał");
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

            return result;
        }

        public List<ContourSegment> CalculateOffsetPolygonKonturSkrzydlo(
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
                    Console.WriteLine("⚠️ CalculateOffsetPolygonKonturSkrzydlo: Element liniowy z niestandardowymi segmentami - zwracam oryginał");
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

                    Console.WriteLine($"🔷 Segment {i} ({side}): offsetValue={offsetValue}, sign={sign}, da={da:F2}, db={db:F2}");
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

                    Console.WriteLine($"🔷 Łuk {i} ({side}): offsetValue={offsetValue}, oldRadius={seg.Radius:F2}, newRadius={newRadius:F2}");
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

        private bool PointsEqualK(XPoint a, XPoint b, double tolerance = 0.01)
        {
            return Math.Abs(a.X - b.X) < tolerance && Math.Abs(a.Y - b.Y) < tolerance;
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
        /// Calculates the length of an element based on its vertices.
        /// </summary>
        /// <param name="vertices">List of vertices defining the shape.</param>
        /// <returns>The calculated length of the element.</returns>
        public float DlugoscElementu(List<XPoint> vertices)
        {
            return (float)Odleglosc(vertices[0], vertices[1]);
        }

        private double Odleglosc(XPoint p1, XPoint p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Sprawdza czy lista punktów jest zgodna z kierunkiem CCW (Counter-Clockwise)
        /// </summary>
        private bool IsCounterClockwise(List<XPoint> points)
        {
            if (points.Count < 3) return true;

            double sum = 0;
            for (int i = 0; i < points.Count; i++)
            {
                var p1 = points[i];
                var p2 = points[(i + 1) % points.Count];
                sum += (p2.X - p1.X) * (p2.Y + p1.Y);
            }
            return sum < 0; // Dla CCW suma jest ujemna w układzie współrzędnych ekranowych (Y w dół)
        }


        /// <summary>
        /// Znajduje przesunięcie między oryginalną a posortowaną listą punktów
        /// </summary>
        private int FindShiftIndex(List<XPoint> original, List<XPoint> sorted)
        {
            if (original.Count == 0 || sorted.Count == 0) return 0;

            // Znajdź indeks pierwszego punktu z sorted w original
            for (int i = 0; i < original.Count; i++)
            {
                if (PointsEqual(original[i], sorted[0], 0.01))
                {
                    return i;
                }
            }
            return 0;
        }

        /// <summary>
        /// Sortuje listę punktów do CCW (dla układu współrzędnych gdzie Y rośnie w dół)
        /// </summary>
        private List<XPoint> SortPointsToCCW(List<XPoint> points)
        {
            if (points.Count < 3) return points;

            // Oblicz środek ciężkości
            double centerX = points.Average(p => p.X);
            double centerY = points.Average(p => p.Y);

            // Sortuj według kąta względem środka
            var sorted = points.OrderBy(p =>
            {
                double angle = Math.Atan2(p.Y - centerY, p.X - centerX);
                return angle;
            }).ToList();

            // Sprawdź orientację - jeśli jest CW, odwróć
            if (!IsCounterClockwise(sorted))
            {
                sorted.Reverse();
            }

            // Znajdź optymalny punkt startowy (najbardziej lewy-górny)
            int startIndex = FindOptimalStartIndex(sorted);
            if (startIndex > 0)
            {
                sorted = sorted.Skip(startIndex).Concat(sorted.Take(startIndex)).ToList();
            }

            return sorted;
        }

        /// <summary>
        /// Znajduje optymalny punkt startowy (najbardziej lewy-górny)
        /// </summary>
        private int FindOptimalStartIndex(List<XPoint> points)
        {
            if (points.Count == 0) return 0;

            double minX = points.Min(p => p.X);
            var leftmostPoints = points.Where(p => Math.Abs(p.X - minX) < 0.001).ToList();

            if (leftmostPoints.Count > 0)
            {
                double minY = leftmostPoints.Min(p => p.Y);
                var topLeftPoint = leftmostPoints.First(p => Math.Abs(p.Y - minY) < 0.001);
                return points.IndexOf(topLeftPoint);
            }

            return 0;
        }

        /// <summary>
        /// Porównuje dwa punkty z tolerancją
        /// </summary>
        private bool PointsEqual(XPoint a, XPoint b, double tolerance = 0.01)
        {
            return Math.Abs(a.X - b.X) < tolerance && Math.Abs(a.Y - b.Y) < tolerance;
        }


    }
}