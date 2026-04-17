using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Client.Pages.Models;
using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;
using Microsoft.JSInterop;

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
                Console.WriteLine($"❌ Brak KonfiguracjeSystemu lub PowiazanyModel!");
                return false;
            }

            if (EdytowanyModel == null)
            {
                Console.WriteLine($"❌ Brak EdytowanyModel jest nie ustawiony!!!");
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

            List<XPoint> punkty = new List<XPoint>();
            List<ContourSegment> punktyZLukami = new List<ContourSegment>();

            if (region == null && !ElementLiniowy)
            {
                Console.WriteLine($"❌ Nie znaleziono regionu o ID: {regionId} w AddElements - GeneratoryOkienne");
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

                Console.WriteLine($"❌ Region o ID: {regionId} region.Wierzcholki.Count():{region.Wierzcholki.Count()}");

                punkty = region.Wierzcholki;
                punktyZLukami = region.Kontur;
            }

            Wierzcholki = punkty;
            zewnetrznyKonturZLukami = punktyZLukami;

            foreach (var x in punkty)
            {
                Console.WriteLine($"punkty --> x.X: {x.X} / x.Y: {x.Y}");
            }

            foreach (var c in punktyZLukami)
            {
                Console.WriteLine($"punktyFull --> c.Start.X: {c.Start.X} / c.Start.Y: {c.Start.Y} / c.End.X: {c.End.X} / c.End.Y: {c.End.Y} / c.Type: {c.Type}");
            }

            if ((punkty == null || punkty.Count < 3) && !ElementLiniowy)
            {
                Console.WriteLine($"❌ Region o ID: {regionId} ma zbyt mało punktów");
                return false;
            }

            if ((punkty == null || punkty.Count < 2))
            {
                Console.WriteLine($"❌ Region o ID: {regionId} ma zbyt mało punktów! punkty.Count: {punkty.Count}");
                return false;
            }

            Console.WriteLine($"🟩 Generuj okno dla regionu ID {regionId} typu: {region.TypKsztaltu} ElementLiniowy: {ElementLiniowy} punkty.Count: {punkty.Count()}");

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

            przeskalowanePunktyZLukami = BuildClosedContour(bezDuplikatow);

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
                        (string.IsNullOrEmpty(slruchPoLewej) || e.Typ == slruchPoLewej));


            var konfRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa &&
                        (string.IsNullOrEmpty(slruchPoPrawej) || e.Typ == slruchPoPrawej));

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

            float offsetLeft = ObliczRoznicePoziomowSzyba(konfLeft, ElementLiniowy);
            float offsetRight = ObliczRoznicePoziomowSzyba(konfRight, ElementLiniowy);
            float offsetTop = ObliczRoznicePoziomowSzyba(konfTop, ElementLiniowy);
            float offsetBottom = ObliczRoznicePoziomowSzyba(konfBottom, ElementLiniowy);

            if (offsetLeft > 0) offsetLeft = profileLeft - offsetLeft;
            if (offsetRight > 0) offsetRight = profileRight - offsetRight;
            if (offsetTop > 0) offsetTop = profileTop - offsetTop;
            if (offsetBottom > 0) offsetBottom = profileBottom - offsetBottom;

            Console.WriteLine($"🔧 Profile z konfiguracji przed korektą: profileLeft: {profileLeft} profileRight: {profileRight} profileTop: {profileTop} profileBottom: {profileBottom}");

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

                    Console.WriteLine($"🔷 T5-T5 Znaleziono konfigurację przesunięcia dla przypadku poziomego. profileLeft: {profileLeft} profileRight: {profileRight} profileTop: {profileTop} profileBottom: {profileBottom}");
                }
                else
                {
                    Console.WriteLine($"🔷 T5-T5 Nie znaleziono konfiguracji przesunięcia dla przypadku poziomego. Domyślnie ustawiono 0 przesunięć.");
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

                // Przed wywołaniem funkcji, dodaj diagnostykę:
                Console.WriteLine($"=== DANE WEJŚCIOWE KONTURU WEWNĘTRZNEGO ===");
                Console.WriteLine($"Liczba segmentów: {przeskalowanePunktyZLukami.Count}");
                for (int i = 0; i < przeskalowanePunktyZLukami.Count; i++)
                {
                    var seg = przeskalowanePunktyZLukami[i];
                    Console.WriteLine($"  Seg.{i}: {seg.Type} Start({seg.Start.X:F2},{seg.Start.Y:F2}) End({seg.End.X:F2},{seg.End.Y:F2})");
                    if (seg.Type == SegmentType.Arc && seg.Center != null)
                    {
                        Console.WriteLine($"         Center({seg.Center.Value.X:F2},{seg.Center.Value.Y:F2}) R={seg.Radius:F2} CCW={seg.CounterClockwise}");
                    }
                }

                wewnetrznyKonturZLukami = CalculateOffsetPolygonKontur(przeskalowanePunktyZLukami,
                profileLeft, profileRight, profileTop, profileBottom,
                false); // dla modeli z łukami i liniami

                Console.WriteLine($"=== ORYGINALNE SEGMENTY WEWNĘTRZNE ===");
                for (int i = 0; i < wewnetrznyKonturZLukami.Count; i++)
                {
                    var seg = wewnetrznyKonturZLukami[i];
                    Console.WriteLine($"  [{i}] {seg.Type}: ({seg.Start.X:F2},{seg.Start.Y:F2}) -> ({seg.End.X:F2},{seg.End.Y:F2})");
                    if (seg.Type == SegmentType.Arc)
                    {
                        Console.WriteLine($"       Center: ({seg.Center.Value.X:F2},{seg.Center.Value.Y:F2}) R={seg.Radius:F2}");
                    }
                }

                // Napraw punkty startowe jeśli potrzebne
                // wewnetrznyKonturZLukami = FixStartPoints(wewnetrznyKonturZLukami);

                liniaSzkleniaKontur = CalculateOffsetPolygon(
                    przeskalowanePunkty,
                    offsetLeft, offsetRight, offsetTop, offsetBottom,
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

        public List<ContourSegment> BuildClosedContour(List<ContourSegment> input, double tolerance = 0.01)
        {
            if (input == null || input.Count == 0)
                return new List<ContourSegment>();

            var unused = new List<ContourSegment>(input);
            var result = new List<ContourSegment>();

            // 1️⃣ start od pierwszego sensownego segmentu
            var current = unused[0];
            unused.RemoveAt(0);
            result.Add(current);

            while (unused.Count > 0)
            {
                var last = result.Last();
                bool found = false;

                for (int i = 0; i < unused.Count; i++)
                {
                    var candidate = unused[i];

                    // 🔹 CASE 1: normalne połączenie
                    if (PointsAreClose(last.End, candidate.Start, tolerance))
                    {
                        result.Add(candidate);
                        unused.RemoveAt(i);
                        found = true;
                        break;
                    }

                    // 🔹 CASE 2: odwrócony segment (Line lub Arc)
                    if (PointsAreClose(last.End, candidate.End, tolerance))
                    {
                        result.Add(Reverse(candidate));
                        unused.RemoveAt(i);
                        found = true;
                        break;
                    }
                }

                // ❌ jeśli nie znaleziono → STOP (nie zgadujemy)
                if (!found)
                {
                    Console.WriteLine("❌ BuildClosedContour: przerwanie - brak ciągłości segmentów");
                    break;
                }
            }

            // 2️⃣ WALIDACJA ZAMKNIĘCIA (bez sztucznego domykania)
            var firstSeg = result.First();
            var lastSeg = result.Last();

            if (!PointsAreClose(lastSeg.End, firstSeg.Start, tolerance))
            {
                Console.WriteLine("⚠️ Kontur NIE jest domknięty geometrycznie!");
                Console.WriteLine($"   gap = {Distance(lastSeg.End, firstSeg.Start)}");
            }

            return result;
        }
        private static ContourSegment Reverse(ContourSegment s)
        {
            if (s.Type == SegmentType.Arc)
            {
                return new ContourSegment(
                    s.End,
                    s.Start,
                    s.Center,
                    s.Radius,
                    !s.CounterClockwise
                );
            }

            return new ContourSegment(s.End, s.Start);
        }

        private static bool PointsAreClose(XPoint a, XPoint b, double tolerance)
        {
            return Math.Abs(a.X - b.X) <= tolerance &&
                   Math.Abs(a.Y - b.Y) <= tolerance;
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

            for (int i = 0; i < polaczeniaArray.Count(); i++)
            {
                Console.WriteLine($"🔷🔷 polaczeniaArray {i}: Join Kat: {polaczeniaArray[i].kat} Typ: {polaczeniaArray[i].typ}");
            }

            foreach (var test in inner)
            {
                Console.WriteLine($"🔷🔷 inner point X: {test.X} Y: {test.Y}");
            }

            foreach (var test in outer)
            {
                Console.WriteLine($"🔷🔷 outer point X: {test.X} Y: {test.Y}");
            }

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

                Console.WriteLine($"📐 Wzorzec: kąt {kat}° → strona {strona} → typ {typ}");
            }

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

            // Debug: pokażmy zliczone elementy
            foreach (var kv in elementyWedlugStron)
            {
                Console.WriteLine($"📊 Strona {kv.Key}: {kv.Value.Count} elementów - indeksy: [{string.Join(", ", kv.Value)}]");
            }

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
                        Console.WriteLine($"🔗 Połączenie {klucz} (ta sama strona) → typ {typ}");
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
                        Console.WriteLine($"🔗 Połączenie {klucz} (różne strony) → typ {typ} (ze strony {stronaB})");
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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

                }
                else if (leftJoin == "T1" && rightJoin == "T1")
                {
                    //Console.WriteLine($"🔷 T1/T1 element {i + 1} START isAlmostHorizontal: {isAlmostHorizontal} isAlmostVertical: {isAlmostVertical} vertexCount: {vertexCount} angleDegrees: {angleDegrees}");
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

                    wierzcholkiStycznePodLuki = GetWierzcholkiStycznePodLuki(
                                         i,
                                         next,
                                         prev,
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                                wierzcholki,
                                                outerContourSegment,
                                                innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

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
                                         wierzcholki,
                                         outerContourSegment,
                                         innerContourSegment);

                }


                // Budujemy pełny kontur 4-segmentowy
                wierzcholkiZLukami = Build4SegmentContour(wierzcholkiStycznePodLuki, outerContourSegment, innerContourSegment);


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
        List<ContourSegment> innerContour)
        {
            if (wierzcholki == null || wierzcholki.Count != 4)
                throw new ArgumentException("Lista wierzchołków musi zawierać dokładnie 4 punkty.");


            // Sprawdź czy kontury są współśrodkowe
            XPoint? commonCenter = GetCommonCenter(outerContour, innerContour);
            bool isConcentric = commonCenter.HasValue;

            // Znajdujemy najbliższe segmenty zewnętrzne i wewnętrzne
            ContourSegment outerSegment = FindClosestSegment(wierzcholki[0], wierzcholki[1], outerContour);
            ContourSegment innerSegment = null;

            Console.WriteLine($"Segment: Start(wierzcholki[0]: {wierzcholki[0]}, wierzcholki[1]: {wierzcholki[1]})");
            Console.WriteLine($"Segment: Start({outerSegment.Start.X}, {outerSegment.Start.Y}) End({outerSegment.End.X}, {outerSegment.End.Y}) Type: {outerSegment.Type} Center: {(outerSegment.Center.HasValue ? $"({outerSegment.Center.Value.X}, {outerSegment.Center.Value.Y})" : "null")} Radius: {outerSegment.Radius}");

            if (outerSegment.Type == SegmentType.Arc && outerSegment.Center != null)
            {
                XPoint center = outerSegment.Center.Value;

                // Punkty na zewnętrznym łuku (z wierzchołków)
                XPoint outerStart = wierzcholki[0];
                XPoint outerEnd = wierzcholki[1];

                // Oblicz kąty dla punktów zewnętrznych
                double angle1 = Math.Atan2(outerStart.Y - center.Y, outerStart.X - center.X);
                double angle2 = Math.Atan2(outerEnd.Y - center.Y, outerEnd.X - center.X);

                // Znajdź wewnętrzny łuk
                var innerArc = FindMatchingArc(outerSegment, innerContour);

                if (innerArc == null)
                    throw new InvalidOperationException("Nie znaleziono pasującego łuku inner.");

                double innerRadius = innerArc.Radius;
                XPoint innerCenter = innerArc.Center.Value;

                // Oblicz punkty na wewnętrznym okręgu (promieniście)
                XPoint innerStart = new XPoint(
                    innerCenter.X + innerRadius * Math.Cos(angle1),
                    innerCenter.Y + innerRadius * Math.Sin(angle1)
                );

                XPoint innerEnd = new XPoint(
                    innerCenter.X + innerRadius * Math.Cos(angle2),
                    innerCenter.Y + innerRadius * Math.Sin(angle2)
                );

                // 🔥 WAŻNE: Zewnętrzny łuk = CW (zgodny z ruchem wskazówek)
                // 🔥 Wewnętrzny łuk = CCW (przeciwny do ruchu wskazówek)
                outerSegment = new ContourSegment(outerStart, outerEnd, center, outerSegment.Radius, false); // CW
                innerSegment = new ContourSegment(innerEnd, innerStart, innerCenter, innerRadius, true); // CCW (odwrócona kolejność!)
            }
            else
            {
                // Dla linii prostych
                outerSegment = new ContourSegment(wierzcholki[0], wierzcholki[1]);
                innerSegment = new ContourSegment(wierzcholki[2], wierzcholki[3]);
            }

            // 🔥 Linie łączące – zawsze w kierunku od zewnętrznego do wewnętrznego
            ContourSegment line1 = new ContourSegment(outerSegment.End, innerSegment.Start)
            {
                Informacja = "Łącznik prawy (do środka)"
            };

            ContourSegment line2 = new ContourSegment(innerSegment.End, outerSegment.Start)
            {
                Informacja = "Łącznik lewy (na zewnątrz)"
            };

            return new List<ContourSegment>
            {
                outerSegment,  // Łuk CW (zewnętrzny)
                line1,         // Linia do wewnątrz
                innerSegment,  // Łuk CCW (wewnętrzny)
                line2          // Linia na zewnątrz
            };
        }

        private List<XPoint> GetWierzcholkiStycznePodLuki(
        int i,
        int next,
        int prev,
        List<XPoint> wierzcholki,
        List<ContourSegment> outerContourSegment,
        List<ContourSegment> innerContourSegment)
        {
            // 🔒 zabezpieczenie zakresu
            bool outOfRange =
                i >= outerContourSegment.Count ||
                i >= innerContourSegment.Count ||
                next >= outerContourSegment.Count ||
                prev >= outerContourSegment.Count;

            if (outOfRange)
            {
                Console.WriteLine($"❌ Index out of range: i={i}, outerSeg={outerContourSegment.Count}, innerSeg={innerContourSegment.Count}");
                return wierzcholki;
            }

            var seg = outerContourSegment[i];
            var segNext = outerContourSegment[next];
            var segPrev = outerContourSegment[prev];

            // 🔍 sprawdzamy czy wszystko linie
            bool allLines =
                seg.Type == SegmentType.Line &&
                segNext.Type == SegmentType.Line &&
                segPrev.Type == SegmentType.Line;

            if (allLines)
                return wierzcholki;

            // 🔥 przypadek łuku / styczności
            return new List<XPoint>
            {
                seg.Start,
                seg.End,
                wierzcholki[2],
                wierzcholki[3]
            };
        }
        /// <summary>
        /// Sprawdza czy kontury są współśrodkowe (mają ten sam środek)
        /// </summary>
        private XPoint? GetCommonCenter(List<ContourSegment> outerContour, List<ContourSegment> innerContour)
        {
            XPoint? outerCenter = null;
            XPoint? innerCenter = null;

            // Znajdź środek zewnętrznego łuku
            foreach (var seg in outerContour)
            {
                if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    outerCenter = seg.Center.Value;
                    break;
                }
            }

            // Znajdź środek wewnętrznego łuku
            foreach (var seg in innerContour)
            {
                if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    innerCenter = seg.Center.Value;
                    break;
                }
            }

            // Sprawdź czy środki istnieją i są takie same (z tolerancją)
            if (outerCenter.HasValue && innerCenter.HasValue)
            {
                double dx = outerCenter.Value.X - innerCenter.Value.X;
                double dy = outerCenter.Value.Y - innerCenter.Value.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < 0.1) // Tolerancja 0.1 jednostki
                {
                    return outerCenter.Value;
                }
            }

            return null;
        }

        public ContourSegment FindClosestSegment(XPoint p1, XPoint p2, List<ContourSegment> contourSegments)
        {
            if (contourSegments == null || contourSegments.Count == 0)
            {
                Console.WriteLine("FindClosestSegment --> ContourSegments jest null lub pusty. Zwarcam tulko linie p1 --> p2");

                return new ContourSegment(
                        p1,
                        p2
                    );
            }

            ContourSegment closest = null;
            double minTotalDistance = double.MaxValue;

            foreach (var seg in contourSegments)
            {
                double distToP1, distToP2;

                if (seg.Type == SegmentType.Line)
                {
                    distToP1 = DistanceToLine(p1, seg.Start, seg.End);
                    distToP2 = DistanceToLine(p2, seg.Start, seg.End);
                }
                else // Arc
                {
                    distToP1 = DistanceToArc(p1, seg);
                    distToP2 = DistanceToArc(p2, seg);
                }

                double totalDistance = distToP1 + distToP2;

                Console.WriteLine($"Segment {seg.Type}: odległość do p1={distToP1:F2}, do p2={distToP2:F2}, suma={totalDistance:F2}");

                if (totalDistance < minTotalDistance)
                {
                    minTotalDistance = totalDistance;
                    closest = seg;
                }
            }

            return closest ?? contourSegments[0];
        }

        // Odległość między dwoma punktami
        private static double Distance(XPoint a, XPoint b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        // Odległość punktu od linii (odcinek)
        private double DistanceToLine(XPoint point, XPoint lineStart, XPoint lineEnd)
        {
            double dx = lineEnd.X - lineStart.X;
            double dy = lineEnd.Y - lineStart.Y;

            double lineLength = Math.Sqrt(dx * dx + dy * dy);
            if (lineLength == 0) return Distance(point, lineStart);

            double t = ((point.X - lineStart.X) * dx + (point.Y - lineStart.Y) * dy) / (lineLength * lineLength);
            t = Math.Max(0, Math.Min(1, t));

            double closestX = lineStart.X + t * dx;
            double closestY = lineStart.Y + t * dy;

            double distX = point.X - closestX;
            double distY = point.Y - closestY;
            return Math.Sqrt(distX * distX + distY * distY);
        }

        // Sprawdzenie czy punkt leży na łuku
        private bool IsPointOnArc(XPoint point, ContourSegment arc, double tolerance = 1.0)
        {
            if (arc.Center == null) return false;

            double distToCenter = Distance(point, arc.Center.Value);
            if (Math.Abs(distToCenter - arc.Radius) > tolerance)
                return false;

            double angle = Math.Atan2(point.Y - arc.Center.Value.Y,
                                       point.X - arc.Center.Value.X);

            double startAngle = Math.Atan2(arc.Start.Y - arc.Center.Value.Y,
                                            arc.Start.X - arc.Center.Value.X);
            double endAngle = Math.Atan2(arc.End.Y - arc.Center.Value.Y,
                                          arc.End.X - arc.Center.Value.X);

            if (startAngle < 0) startAngle += 2 * Math.PI;
            if (endAngle < 0) endAngle += 2 * Math.PI;
            if (angle < 0) angle += 2 * Math.PI;

            if (!arc.CounterClockwise)
            {
                if (startAngle > endAngle)
                    return angle >= startAngle || angle <= endAngle;
                else
                    return angle >= startAngle && angle <= endAngle;
            }
            else
            {
                if (endAngle > startAngle)
                    return angle >= endAngle || angle <= startAngle;
                else
                    return angle >= endAngle && angle <= startAngle;
            }
        }

        // Odległość punktu od łuku
        private double DistanceToArc(XPoint point, ContourSegment arc)
        {
            if (arc.Center == null) return double.MaxValue;

            double distToCenter = Distance(point, arc.Center.Value);
            double distToCircle = Math.Abs(distToCenter - arc.Radius);

            if (!IsPointOnArc(point, arc))
            {
                double distToStart = Distance(point, arc.Start);
                double distToEnd = Distance(point, arc.End);
                return Math.Min(distToStart, distToEnd);
            }

            return distToCircle;
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

        private ContourSegment FindMatchingArc(ContourSegment outerArc, List<ContourSegment> innerContour)
        {
            if (outerArc.Center == null)
                return null;

            ContourSegment best = null;
            double minDiff = double.MaxValue;

            foreach (var seg in innerContour)
            {
                if (seg.Type != SegmentType.Arc || seg.Center == null)
                    continue;

                double dx = seg.Center.Value.X - outerArc.Center.Value.X;
                double dy = seg.Center.Value.Y - outerArc.Center.Value.Y;
                double dist = dx * dx + dy * dy;

                if (dist < minDiff)
                {
                    minDiff = dist;
                    best = seg;
                }
            }

            return best;
        }
        // Funkcja do obliczania długości łuku
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

            Console.WriteLine($"▶️ GetStartT1: stronaWModelu: {stronaWModelu}, stonaOstanioDodanegoElementu: {stonaOstanioDodanegoElementu}, nk: {nk + 1}, czyParzysta: {czyParzysta} warunek: {warunek}");

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
        //private List<ContourSegment> GetStartT2(ContourSegment inner, ContourSegment outer)
        //{
        //    var segments = new List<ContourSegment>();

        //    // Kopiowanie segmentu inner
        //    if (inner.Type == SegmentType.Line)
        //    {
        //        segments.Add(new ContourSegment(
        //            new XPoint(inner.Start.X, inner.Start.Y),
        //            new XPoint(inner.End.X, inner.End.Y)
        //        ));
        //    }
        //    else if (inner.Type == SegmentType.Arc && inner.Center != null)
        //    {
        //        segments.Add(new ContourSegment(
        //            new XPoint(inner.Start.X, inner.Start.Y),
        //            new XPoint(inner.End.X, inner.End.Y),
        //            new XPoint(inner.Center.Value.X, inner.Center.Value.Y),
        //            inner.Radius,
        //            inner.CounterClockwise
        //        ));
        //    }

        //    // Kopiowanie segmentu outer
        //    if (outer.Type == SegmentType.Line)
        //    {
        //        segments.Add(new ContourSegment(
        //            new XPoint(outer.Start.X, outer.Start.Y),
        //            new XPoint(outer.End.X, outer.End.Y)
        //        ));
        //    }
        //    else if (outer.Type == SegmentType.Arc && outer.Center != null)
        //    {
        //        segments.Add(new ContourSegment(
        //            new XPoint(outer.Start.X, outer.Start.Y),
        //            new XPoint(outer.End.X, outer.End.Y),
        //            new XPoint(outer.Center.Value.X, outer.Center.Value.Y),
        //            outer.Radius,
        //            outer.CounterClockwise
        //        ));
        //    }

        //    return segments;
        //}
        //private List<ContourSegment> GetEndT2(ContourSegment inner, ContourSegment outer)
        //{
        //    var segments = new List<ContourSegment>();

        //    // Kopiowanie segmentu inner
        //    if (inner.Type == SegmentType.Line)
        //    {
        //        segments.Add(new ContourSegment(
        //            new XPoint(inner.Start.X, inner.Start.Y),
        //            new XPoint(inner.End.X, inner.End.Y)
        //        ));
        //    }
        //    else if (inner.Type == SegmentType.Arc && inner.Center != null)
        //    {
        //        segments.Add(new ContourSegment(
        //            new XPoint(inner.Start.X, inner.Start.Y),
        //            new XPoint(inner.End.X, inner.End.Y),
        //            new XPoint(inner.Center.Value.X, inner.Center.Value.Y),
        //            inner.Radius,
        //            inner.CounterClockwise
        //        ));
        //    }

        //    // Kopiowanie segmentu outer
        //    if (outer.Type == SegmentType.Line)
        //    {
        //        segments.Add(new ContourSegment(
        //            new XPoint(outer.Start.X, outer.Start.Y),
        //            new XPoint(outer.End.X, outer.End.Y)
        //        ));
        //    }
        //    else if (outer.Type == SegmentType.Arc && outer.Center != null)
        //    {
        //        segments.Add(new ContourSegment(
        //            new XPoint(outer.Start.X, outer.Start.Y),
        //            new XPoint(outer.End.X, outer.End.Y),
        //            new XPoint(outer.Center.Value.X, outer.Center.Value.Y),
        //            outer.Radius,
        //            outer.CounterClockwise
        //        ));
        //    }

        //    return segments;
        //}

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
                return -1;
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

        // Znajdź pierwsze przecięcie osi z konturem
        public XPoint FindAxisIntersection(
           float value,
           AxisDirection direction,
           List<XPoint> contour,
           AxisPick pick = AxisPick.Nearest,
           float epsilon = 0.001f
       )
        {
            List<XPoint> hits = new();

            for (int i = 0; i < contour.Count; i++)
            {
                int next = (i + 1) % contour.Count;

                var a = contour[i];
                var b = contour[next];

                // 🔒 FILTR: interesują nas tylko krawędzie osiowe
                if (direction == AxisDirection.Horizontal)
                {
                    // tylko krawędzie POZIOME
                    if (Math.Abs(a.Y - b.Y) > epsilon)
                        continue;

                    if (Math.Abs(a.Y - value) > epsilon)
                        continue;

                    hits.Add(new XPoint(
                        Math.Min(a.X, b.X),
                        value
                    ));
                    hits.Add(new XPoint(
                        Math.Max(a.X, b.X),
                        value
                    ));
                }
                else
                {
                    // tylko krawędzie PIONOWE
                    if (Math.Abs(a.X - b.X) > epsilon)
                        continue;

                    if (Math.Abs(a.X - value) > epsilon)
                        continue;

                    hits.Add(new XPoint(
                        value,
                        Math.Min(a.Y, b.Y)
                    ));
                    hits.Add(new XPoint(
                        value,
                        Math.Max(a.Y, b.Y)
                    ));
                }
            }

            if (hits.Count == 0)
                return direction == AxisDirection.Horizontal
                    ? new XPoint(contour[0].X, value)
                    : new XPoint(value, contour[0].Y);

            return pick switch
            {
                AxisPick.Min => direction == AxisDirection.Horizontal
                    ? hits.OrderBy(p => p.X).First()
                    : hits.OrderBy(p => p.Y).First(),

                AxisPick.Max => direction == AxisDirection.Horizontal
                    ? hits.OrderByDescending(p => p.X).First()
                    : hits.OrderByDescending(p => p.Y).First(),

                _ => hits.First()
            };
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

            if (count > 0)
                Console.WriteLine($"🔷CalculateOffsetPolygon Calculating offset polygon for {count} X:{points[0].X} Y:{points[0].Y} elementLiniowy:{elementLiniowy} points with profiles L:{profileLeft}, R:{profileRight}, T:{profileTop}, B:{profileBottom}");

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

                Console.WriteLine($"🔷CalculateOffsetPolygon Element liniowy: strona {side}, offsetX={offsetX}, offsetY={offsetY}");

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

                Console.WriteLine($"🔷CalculateOffsetPolygon Bok {i}: kierunek ({tx:F2}, {ty:F2}), normalna zewnętrzna ({nx:F2}, {ny:F2})");

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

                Console.WriteLine($"🔷CalculateOffsetPolygon Bok {i}: strona {side}, offsetValue={offsetValue}, usePositiveNormal={usePositiveNormal}, offset={offset}");


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

            foreach (var pt in result)
            {
                Console.WriteLine($"🔷CalculateOffsetPolygon Calculated offset polygon point: X={pt.X}, Y={pt.Y}");
            }

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

            var offsetSegments = new List<ContourSegment>();
            var sideInfo = new List<string>();

            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];

                float dx = (float)(seg.End.X - seg.Start.X);
                float dy = (float)(seg.End.Y - seg.Start.Y);
                float length = MathF.Sqrt(dx * dx + dy * dy);

                if (length < 1e-6f)
                    continue;

                float angleDegrees = MathF.Atan2(dy, dx) * 180f / MathF.PI;
                if (angleDegrees < 0) angleDegrees += 360f;

                string side = StronaOknaHelper.OkreslStrone(angleDegrees, i, null);
                sideInfo.Add(side);

                float offsetValue = side switch
                {
                    "Góra" => profileTop,
                    "Dół" => profileBottom,
                    "Lewa" => profileLeft,
                    "Prawa" => profileRight,
                    _ => 0
                };

                // =========================
                // LINIE
                // =========================
                if (seg.Type == SegmentType.Line)
                {
                    float tx = dx / length;
                    float ty = dy / length;

                    float nx = ty;
                    float ny = -tx;

                    float offset = -offsetValue;

                    var newStart = new XPoint(
                        seg.Start.X + nx * offset,
                        seg.Start.Y + ny * offset);

                    var newEnd = new XPoint(
                        seg.End.X + nx * offset,
                        seg.End.Y + ny * offset);

                    offsetSegments.Add(new ContourSegment(newStart, newEnd)
                    {
                        Informacja = side
                    });
                }

                // =========================
                // ŁUKI (POPRAWKA)
                // =========================
                else if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    var center = seg.Center.Value;

                    bool isClockwise = IsContourClockwise(segments);

                    // 🔥 KLUCZOWA ZMIANA:
                    // łuki zawsze używają profileTop
                    float offset = profileTop;

                    float radiusChange = isClockwise ? -offset : offset;

                    float newRadius = (float)seg.Radius + radiusChange;

                    if (newRadius < 0.1f)
                        newRadius = 0.1f;

                    double startAngle = Math.Atan2(
                        seg.Start.Y - center.Y,
                        seg.Start.X - center.X);

                    double endAngle = Math.Atan2(
                        seg.End.Y - center.Y,
                        seg.End.X - center.X);

                    var newStart = new XPoint(
                        center.X + newRadius * Math.Cos(startAngle),
                        center.Y + newRadius * Math.Sin(startAngle));

                    var newEnd = new XPoint(
                        center.X + newRadius * Math.Cos(endAngle),
                        center.Y + newRadius * Math.Sin(endAngle));

                    offsetSegments.Add(new ContourSegment(
                        newStart,
                        newEnd,
                        center,
                        newRadius,
                        seg.CounterClockwise)
                    {
                        Informacja = "ARC_TOP_OFFSET"
                    });
                }
            }

            // =========================
            // KROK 2 - PRZECIĘCIA
            // =========================
            var result = new List<ContourSegment>();

            for (int i = 0; i < offsetSegments.Count; i++)
            {
                var current = offsetSegments[i];
                var previous = offsetSegments[(i - 1 + offsetSegments.Count) % offsetSegments.Count];

                if (current.Type == SegmentType.Line &&
                    previous.Type == SegmentType.Line)
                {
                    var intersection = GetLinesIntersectionK(
                        previous.Start,
                        previous.End,
                        current.Start,
                        current.End);

                    if (!float.IsNaN((float)intersection.X) &&
                        !float.IsNaN((float)intersection.Y))
                    {
                        if (result.Count > 0)
                        {
                            result[result.Count - 1].End = intersection;
                        }

                        result.Add(new ContourSegment(intersection, current.End)
                        {
                            Informacja = current.Informacja
                        });
                    }
                    else
                    {
                        result.Add(current);
                    }
                }
                else
                {
                    result.Add(current);
                }
            }

            // =========================
            // KROK 3 - ZAMKNIĘCIE
            // =========================
            if (result.Count > 0 &&
                !PointsEqualK(result[0].Start, result[^1].End))
            {
                result[^1].End = result[0].Start;
            }

            return result;
        }

        /// <summary>
        /// Sprawdza czy kontur jest zgodny z ruchem wskazówek zegara (CW)
        /// </summary>
        private bool IsContourClockwise(List<ContourSegment> segments)
        {
            if (segments == null || segments.Count == 0)
                return true;

            double sum = 0;
            foreach (var seg in segments)
            {
                // Dla łuków, używamy punktu środkowego łuku dla lepszej dokładności
                if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    // Oblicz punkt środkowy łuku
                    double startAngle = Math.Atan2(seg.Start.Y - seg.Center.Value.Y, seg.Start.X - seg.Center.Value.X);
                    double endAngle = Math.Atan2(seg.End.Y - seg.Center.Value.Y, seg.End.X - seg.Center.Value.X);
                    double midAngle = (startAngle + endAngle) / 2;

                    double midX = seg.Center.Value.X + seg.Radius * Math.Cos(midAngle);
                    double midY = seg.Center.Value.Y + seg.Radius * Math.Sin(midAngle);

                    // Użyj punktu środkowego do obliczenia pola
                    sum += (midX - seg.Start.X) * (midY + seg.Start.Y);
                    sum += (seg.End.X - midX) * (seg.End.Y + midY);
                }
                else
                {
                    // Dla linii
                    sum += (seg.End.X - seg.Start.X) * (seg.End.Y + seg.Start.Y);
                }
            }

            // Jeśli suma > 0 to CCW, jeśli < 0 to CW
            return sum < 0;
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

        // Funkcja pomocnicza do określenia typu połączenia w narożniku

        public enum AxisDirection
        {
            Horizontal,
            Vertical
        }

        public enum AxisPick
        {
            Nearest, // zachowanie jak dotychczas
            Min,     // zewnętrzne / lewe / dolne
            Max      // wewnętrzne / prawe / górne
        }

        // Struktura przechowująca informacje o połączeniach dla każdego boku
        public class PolaczenieBoku
        {
            public string? TypPolaczenia { get; set; }  // T1, T2, T3, T4, T5
            public int Kat { get; set; }                // Kąt pod którym występuje to połączenie
        }

    }
}