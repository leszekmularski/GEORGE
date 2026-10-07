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
        // ============================================================
        // POLA PUBLICZNE
        // ============================================================
        public new List<KsztaltElementu> ElementyRamyRysowane { get; set; } = new();
        public List<KonfSystem> KonfiguracjeSystemu { get; set; } = new();

        public KonfModele? EdytowanyModel;
        public int Zindeks { get; set; }
        public string IdRegionuPonizej { get; set; }

        public List<XPoint> Wierzcholki { get; set; } = new();
        public List<ContourSegment> konturWenetrznyPodRysunek { get; set; } = new();

        public List<XPoint> wewnetrznyKontur;
        public List<XPoint> liniaSzkleniaKontur;
        public List<XPoint> wierzcholkiWenetrznePodRysunek { get; set; } = new();
        public List<ContourSegment> zewnetrznyKonturZLukami { get; set; } = new();
        public List<ContourSegment> wewnetrznyKonturZLukami;
        public List<ContourSegment> liniaSzkleniaKonturZLukami;

        public List<XPoint> liniaOkuciaKontur;
        public List<ContourSegment> liniaOkuciaKonturZLukami;

        public ConstWlasciwosciOkna constWlasciwosciOkna { get; set; } = new();
        public List<ShapeRegion> Region { get; set; } = new();
        public string StronaElementu { get; set; } = "";

        private readonly IJSRuntime _jsRuntime;
        public List<string> Komunikaty { get; set; } = new();
        public List<string> BledySystemowe { get; set; } = new();

        // ⭐ NOWE POLA – do stabilnego dopasowania po zmianie wymiarów
        public string? IdMaster { get; set; }
        public string? TypKsztaltu { get; set; }

        // ============================================================
        // KONSTRUKTOR
        // ============================================================
        public Generator(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
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
            MVCKonfModelu = null;
            RuchomySlupekPoPrawej = false;
            RuchomySlupekPoLewej = false;
            ElementLiniowy = false;
            wewnetrznyKontur = new List<XPoint>();
            liniaSzkleniaKontur = new List<XPoint>();
            Komunikaty = new List<string>();
        }

        // ============================================================
        // TYPY POMOCNICZE
        // ============================================================
        private record WynikWalidacji(bool Ok, string? Blad);

        private class WynikWierzcholkow
        {
            public bool Ok;
            public string? Blad;
            public List<XPoint>? Wierzcholki;
            public List<ContourSegment>? WierzcholkiZLukami;
            public bool DodajA;
            public bool DodajB;
        }

        private class GeometriaBoku
        {
            public float Dx, Dy, AngleDegrees;
            public float AnglePrev, AngleNext;
            public string CurrentSide = "", PrevSide = "", NextSide = "";
            public float Length;
            public float Tx, Ty, Nx, Ny;
            public float Profile, ProfileA, ProfileB;

            public float ProfileLeft, ProfileRight, ProfileTop, ProfileBottom;
            public Guid RowIdprofileLeft, RowIdprofileRight, RowIdprofileTop, RowIdprofileBottom;

            public bool IsAlmostHorizontal, IsAlmostVertical;
            public float AngleDegreesStronaA, AngleDegreesStronaB;
        }

        // ============================================================
        // AddElements — orkiestrator przygotowania
        // ============================================================
        public async Task<string> AddElements(
            List<ShapeRegion> regions,
            string regionId,
            Dictionary<string, GeneratorState> generatorStates,
            List<ShapeRegion> regionAdd,
            List<DaneKwadratu> daneKwadratu,
            List<XPoint> punktyRegionuMaster,
            XPoint mouseClik,
            bool kasujKonsole = false,
            Guid? rowIdSlupka = null,
            string? oryginalnyRegionId = null)
        {
            if (regions == null) return "Brak regionu";

            if (regions == null) return "Brak regionu";

            // ⭐ ZABEZPIECZENIE
            daneKwadratu ??= new List<DaneKwadratu>();
            punktyRegionuMaster ??= new List<XPoint>();

            Guid callId = Guid.NewGuid();
            Console.WriteLine($"▶ START AddElements: {callId}, regionId={regionId}");

            if (_jsRuntime != null && kasujKonsole)
            {
                await _jsRuntime.InvokeVoidAsync("console.clear");
                await _jsRuntime.InvokeVoidAsync("console.log", "\n\n");
            }

            if (KonfiguracjeSystemu == null || MVCKonfModelu == null)
            {
                BledySystemowe.Add("❌ Brak konfiguracji systemu lub powiązanego modelu.");
                return "❌ Brak konfiguracji systemu lub powiązanego modelu.";
            }

            if (EdytowanyModel == null)
            {
                BledySystemowe.Add("❌ Brak edytowanego modelu.");
                return "❌ Brak edytowanego modelu.";
            }

            Region = regionAdd;

            Console.WriteLine($"🔍 AddElements: szukam regionu '{regionId}'");
            Console.WriteLine($"   regions: {regions.Count}");
            foreach (var r in regions.Take(20))
            {
                Console.WriteLine($"      Id='{r.Id}' Typ={r.TypKsztaltu} Rama={r.Rama}");
            }

            var region = regions.FirstOrDefault(r => r.Id == regionId);

            List<XPoint> punkty = new List<XPoint>();
            List<ContourSegment> punktyZLukami = new List<ContourSegment>();

            if (region == null && !ElementLiniowy)
            {
                BledySystemowe.Add($"❌ Nie znaleziono regionu o ID: {regionId}.");
                return $"❌ Nie znaleziono regionu o ID: {regionId}.";
            }
            else if (region != null && !ElementLiniowy)
            {
                punkty = region.Wierzcholki;
                punktyZLukami = region.Kontur;
            }
            else if (ElementLiniowy)
            {
                region = regions.FirstOrDefault(r => r.Id != null);
                Console.WriteLine($"✅ AddElements Region o ID: {regionId} region.Wierzcholki.Count():{region.Wierzcholki.Count()}");
                punkty = region.Wierzcholki;
                punktyZLukami = region.Kontur;
            }

            Wierzcholki = punkty;
            zewnetrznyKonturZLukami = punktyZLukami;

            if ((punkty == null || punkty.Count < 3) && !ElementLiniowy)
            {
                BledySystemowe.Add($"❌ Region o ID: {regionId} ma zbyt mało punktów (≥3).");
                return $"❌ Region o ID: {regionId} ma zbyt mało punktów (≥3).";
            }

            if ((punkty == null || punkty.Count < 2))
            {
                BledySystemowe.Add($"❌ Region o ID: {regionId} ma zbyt mało punktów (≥2).");
                return $"❌ Region o ID: {regionId} ma zbyt mało punktów (≥2).";
            }

            bool PointsAreClose(XPoint a, XPoint b, double tolerance = 0.001)
                => Math.Abs(a.X - b.X) < tolerance && Math.Abs(a.Y - b.Y) < tolerance;

            string slruchPoPrawej = RuchomySlupekPoPrawej ? "Słupek ruchomy" : "";
            string slruchPoLewej = RuchomySlupekPoLewej ? "Słupek ruchomy" : "";

            if (ElementLiniowy)
            {
                slruchPoPrawej = "";
                slruchPoLewej = "";

                // Element liniowy reprezentuje jeden, kliknięty odcinek dzielący.
                // Nie wolno przekazywać konturu całego regionu ani łączyć wszystkich
                // linii dzielących: po podziale regionu kontur regionu może być pusty,
                // a połączone odcinki nie tworzą poprawnego wielokąta.
                var wybranaLinia = region.LinieDzielace?
                    .Where(l => l?.Points?.Count >= 2 && l.DlugoscLinii > 0.001)
                    .Select(l => new
                    {
                        Linia = l,
                        Odleglosc = OdlegloscPunktuOdOdcinka(
                            mouseClik.X, mouseClik.Y, l.Points[0], l.Points[1])
                    })
                    .OrderBy(x => x.Odleglosc)
                    .FirstOrDefault()
                    ?.Linia;

                if (wybranaLinia != null)
                {
                    punkty = wybranaLinia.Points
                        .Select(p => new XPoint(p.X, p.Y))
                        .ToList();
                    punktyZLukami = wybranaLinia.ContourSegments
                        .Select(s => s.Clone())
                        .ToList();
                }
                else
                {
                    BledySystemowe.Add(
                        $"⚠️ Brak poprawnej linii dzielącej dla elementu liniowego w regionie {regionId}. " +
                        "Użyto geometrii regionu.");
                }

                Wierzcholki = new List<XPoint>(punkty);
                zewnetrznyKonturZLukami = punktyZLukami.Select(s => s.Clone()).ToList();
            }

            var przeskalowanePunkty = new List<XPoint>(punkty);
            var przeskalowanePunktyZLukami = punktyZLukami
                .Where(s => !PointsAreClose(s.Start, s.End))
                .ToList();
            var przeskalowanePunktyZLukamiPodRysynek = new List<ContourSegment>();
            var przeskalowanePunktyPodRysynek = new List<XPoint>(punkty);

            var konfLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa &&
                (string.IsNullOrEmpty(slruchPoLewej) || e.Typ == slruchPoLewej));
            var konfRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa &&
                (string.IsNullOrEmpty(slruchPoPrawej) || e.Typ == slruchPoPrawej));
            var konfTop = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeGora);
            var konfBottom = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeDol);

            bool czymozbycFIX = MVCKonfModelu.KonfSystem.Where(e => e.CzyMozeBycFix).Any();

            if (konfLeft == null)
            {
                konfLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa);
                if (!RuchomySlupekPoLewej)
                    BledySystemowe.Add($"⚠️ Brak konfiguracji lewej z typem '{slruchPoLewej}'. Użyto: {konfLeft?.Nazwa ?? "BRAK"}.");
            }

            if (konfRight == null)
            {
                konfRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa);
                if (!RuchomySlupekPoPrawej)
                    BledySystemowe.Add($"⚠️ Brak konfiguracji prawej z typem '{slruchPoPrawej}'. Użyto: {konfRight?.Nazwa ?? "BRAK"}.");
            }

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

            if ((profileLeft == 0 || profileRight == 0 || profileTop == 0 || profileBottom == 0) && (punkty.Count() != 2))
                BledySystemowe.Add($"⚠️ Profil równy 0. L={profileLeft} R={profileRight} T={profileTop} B={profileBottom}.");

            if (offsetKorpusWewnetrznyLeft == 0 || offsetKorpusWewnetrznyRight == 0 ||
                offsetKorpusWewnetrznyTop == 0 || offsetKorpusWewnetrznyBottom == 0)
                BledySystemowe.Add($"⚠️ Offset korpusu = 0.");

            if ((offsetGlassLeft == 0 || offsetGlassRight == 0 || offsetGlassTop == 0 || offsetGlassBottom == 0) && czymozbycFIX)
                BledySystemowe.Add($"⚠️ Offset szklenia = 0.");

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

            if (profileLeft == 0 && ElementLiniowy)
            {
                slruchPoLewej = "";
                konfLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa);
                profileLeft = (float)((konfLeft?.PionPrawa ?? 0) - (konfLeft?.PionLewa ?? 0));
                RowIdprofileLeft = konfLeft?.RowId ?? Guid.Empty;
                RowIndeksprofileLeft = konfLeft?.IndeksElementu ?? "BRAK-DANYCH";
                RowNazwaprofileLeft = konfLeft?.Nazwa ?? "BRAK-DANYCH";
            }

            if (profileRight == 0 && ElementLiniowy)
            {
                slruchPoPrawej = "";
                konfRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa);
                profileRight = (float)((konfRight?.PionPrawa ?? 0) - (konfRight?.PionLewa ?? 0));
                RowIdprofileRight = konfRight?.RowId ?? Guid.Empty;
                RowIndeksprofileRight = konfRight?.IndeksElementu ?? "BRAK-DANYCH";
                RowNazwaprofileRight = konfRight?.Nazwa ?? "BRAK-DANYCH";
            }

            if (profileTop == 0 && ElementLiniowy)
            {
                konfTop = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeGora);
                profileTop = (float)((konfTop?.PoziomDol ?? 0) - (konfTop?.PoziomGora ?? 0));
                RowIdprofileTop = konfTop?.RowId ?? Guid.Empty;
                RowIndeksprofileTop = konfTop?.IndeksElementu ?? "BRAK-DANYCH";
                RowNazwaprofileTop = konfTop?.Nazwa ?? "BRAK-DANYCH";
            }

            if (profileBottom == 0 && ElementLiniowy)
            {
                konfBottom = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeDol);
                profileBottom = (float)((konfBottom?.PoziomDol ?? 0) - (konfBottom?.PoziomGora ?? 0));
                RowIdprofileBottom = konfBottom?.RowId ?? Guid.Empty;
                RowIndeksprofileBottom = konfBottom?.IndeksElementu ?? "BRAK-DANYCH";
                RowNazwaprofileBottom = konfBottom?.Nazwa ?? "BRAK-DANYCH";
            }

            string NazwaObiektu = MVCKonfModelu.KonfSystem.First().Nazwa ?? "";
            string TypObiektu = MVCKonfModelu.KonfSystem.First().Typ ?? "";

            Console.WriteLine($"📐Generator ----> region.TypKsztaltu: {region.TypKsztaltu} " +
                $"profileLeft: {profileLeft}, profileRight: {profileRight}, " +
                $"profileTop: {profileTop}, profileBottom: {profileBottom} " +
                $"slruchPoPrawej: {slruchPoPrawej} slruchPoLewej: {slruchPoLewej}");

            // ═══════════════════════════════════════════════════════════
            // OBLICZ WEWNĘTRZNY KONTUR
            // ═══════════════════════════════════════════════════════════
            bool stronaA = false;
            bool stronaB = false;

            if (ElementLiniowy)
            {
                var p1 = punkty[0];
                var p2 = punkty[1];
                double dx = Math.Abs(p2.X - p1.X);
                double dy = Math.Abs(p2.Y - p1.Y);

                bool liniaPionowa = dx < 0.01 && dy > 0.01;
                bool liniaPozioma = dy < 0.01 && dx > 0.01;

                Console.WriteLine($"🔷 ElementLiniowy: orientacja = {(liniaPionowa ? "PIONOWA" : liniaPozioma ? "POZIOMA" : "SKOŚNA")}");

                var kandydaci = daneKwadratu
                    .Where(s => s.Przesuniecia != null && s.Przesuniecia.Count > 0 &&
                                s.Wierzcholki != null && s.Wierzcholki.Count > 1)
                    .Select(s => new { Wpis = s, Odleglosc = OdlegloscPunktuOdOdcinka(mouseClik.X, mouseClik.Y, s.Wierzcholki![0], s.Wierzcholki[1]) })
                    .OrderBy(x => x.Odleglosc)
                    .ToList();

                Console.WriteLine($"🔷 ElementLiniowy: znaleziono {kandydaci.Count} wpisów z Przesunieciami:");

                foreach (var k in kandydaci)
                {
                    Console.WriteLine($"   {k.Wpis.RowIdElementu} (strona={k.Wpis.Strona}) odl={k.Odleglosc:F2} region: {k.Wpis.RowIdRegionu}");
                }

                var konfPolaczenia = kandydaci.FirstOrDefault()?.Wpis.Przesuniecia;

                if (konfPolaczenia == null)
                {
                    Console.WriteLine($"🔷 ElementLiniowy: brak kandydatów — fallback");

                    konfPolaczenia = daneKwadratu
                        .Where(s => s.Przesuniecia != null && s.Przesuniecia.Count > 0 &&
                                    s.Wierzcholki != null && s.Wierzcholki.Count == 2)
                        .Select(s => new { Wpis = s, Odleglosc = OdlegloscPunktuOdOdcinka(mouseClik.X, mouseClik.Y, s.Wierzcholki![0], s.Wierzcholki[1]) })
                        .OrderBy(x => x.Odleglosc)
                        .FirstOrDefault()
                        ?.Wpis.Przesuniecia;
                }

                konfPolaczenia = konfPolaczenia?
                    .Where(x => x.ElementWewnetrznyId == RowIdprofileTop || x.ElementZewnetrznyId == RowIdprofileTop)
                    .GroupBy(x => new { x.ElementWewnetrznyId, x.ElementZewnetrznyId, x.Strona })
                    .Select(g => g.First())
                    .ToList() ?? new List<PrzesuniecieDto>();

                stronaA = daneKwadratu.FirstOrDefault(x => x.SasiadSlupekStronaA) != null;
                stronaB = daneKwadratu.FirstOrDefault(x => x.SasiadSlupekStronaB) != null;

                if (konfPolaczenia.Count > 0)
                {
                    var szukPionA = Math.Abs(konfPolaczenia.FirstOrDefault(p =>
                        p.Strona.Equals(stronaA ? "Dół" : "Góra", StringComparison.OrdinalIgnoreCase)
                        && p.ElementWewnetrznyToSlupek
                        && p.ElementZewnetrznyToSlupek == stronaA)?.PrzesuniecieYStycznej ?? 0);

                    var szukPionB = Math.Abs(konfPolaczenia.FirstOrDefault(p =>
                        p.Strona.Equals(stronaB ? "Góra" : "Dół", StringComparison.OrdinalIgnoreCase)
                        && p.ElementWewnetrznyToSlupek
                        && p.ElementZewnetrznyToSlupek == stronaB)?.PrzesuniecieYStycznej ?? 0);

                    if (szukPionA == 0)
                        szukPionA = Math.Abs(konfPolaczenia.FirstOrDefault(p => p.Strona.Equals("Góra", StringComparison.OrdinalIgnoreCase))?.PrzesuniecieYStycznej ?? 0);

                    if (szukPionB == 0)
                        szukPionB = Math.Abs(konfPolaczenia.FirstOrDefault(p => p.Strona.Equals("Dół", StringComparison.OrdinalIgnoreCase))?.PrzesuniecieYStycznej ?? 0);

                    var szukPoziomA = Math.Abs(konfPolaczenia.FirstOrDefault(p => p.Strona.Equals("Lewa", StringComparison.OrdinalIgnoreCase))?.PrzesuniecieYStycznej ?? 0);
                    var szukPoziomB = Math.Abs(konfPolaczenia.FirstOrDefault(p => p.Strona.Equals("Prawa", StringComparison.OrdinalIgnoreCase))?.PrzesuniecieYStycznej ?? 0);

                    profileLeft = (float)szukPoziomA;
                    profileRight = (float)szukPoziomB;
                    profileTop = (float)szukPionA;
                    profileBottom = (float)szukPionB;

                    Console.WriteLine($"🔷 ElementLiniowy ({(liniaPionowa ? "PION" : "POZIOM")}) — dopasowane: {konfPolaczenia.Count}, " +
                        $"L={profileLeft} R={profileRight} T={profileTop} B={profileBottom}");
                }
                else
                {
                    BledySystemowe.Add($"❌ Brak konfiguracji przesunięcia dla elementu liniowego.");
                    Console.WriteLine($"❌ Brak konfiguracji przesunięcia dla elementu liniowego [AddElemnts]. konfPolaczenia.Count = {konfPolaczenia.Count}");
                }

                wewnetrznyKontur = przeskalowanePunkty;
                wewnetrznyKonturZLukami = przeskalowanePunktyZLukami;
                konturWenetrznyPodRysunek = przeskalowanePunktyZLukamiPodRysynek;
                wierzcholkiWenetrznePodRysunek = przeskalowanePunktyPodRysynek;

                punktyRegionuMaster = await CalculateOffsetPolygon(
                    punktyRegionuMaster,
                    profileLeft, profileRight, profileTop, profileBottom, stronaA, stronaB);
            }
            else
            {
                wewnetrznyKontur = await CalculateOffsetPolygon(
                    przeskalowanePunkty, profileLeft, profileRight, profileTop, profileBottom);

                wewnetrznyKonturZLukami = await CalculateOffsetPolygonKontur(
                    przeskalowanePunktyZLukami, profileLeft, profileRight, profileTop, profileBottom);

                liniaSzkleniaKontur = await CalculateOffsetPolygon(
                    przeskalowanePunkty, offsetGlassLeft, offsetGlassRight, offsetGlassTop, offsetGlassBottom);

                wierzcholkiWenetrznePodRysunek = await CalculateOffsetPolygon(
                    przeskalowanePunktyPodRysynek,
                    offsetKorpusWewnetrznyLeft, offsetKorpusWewnetrznyRight,
                    offsetKorpusWewnetrznyTop, offsetKorpusWewnetrznyBottom);

                liniaSzkleniaKonturZLukami = await CalculateOffsetPolygonKontur(
                    przeskalowanePunktyZLukami, offsetGlassLeft, offsetGlassRight, offsetGlassTop, offsetGlassBottom);

                konturWenetrznyPodRysunek = await CalculateOffsetPolygonKontur(
                    przeskalowanePunktyZLukami,
                    offsetKorpusWewnetrznyLeft, offsetKorpusWewnetrznyRight,
                    offsetKorpusWewnetrznyTop, offsetKorpusWewnetrznyBottom);
            }

            if (wewnetrznyKonturZLukami == null)
            {
                Console.WriteLine($"❌ wewnetrznyKonturZLukami == null dla {regionId}");
                BledySystemowe.Add($"❌ wewnetrznyKonturZLukami == null dla {regionId}");
                return $"❌ wewnetrznyKonturZLukami == null dla {regionId}";
            }

            if (liniaSzkleniaKontur == null)
            {
                Console.WriteLine($"❌ liniaSzkleniaKontur == null dla {regionId}");
                BledySystemowe.Add($"❌ liniaSzkleniaKontur == null dla {regionId}");
                return $"❌ liniaSzkleniaKontur == null dla {regionId}";
            }

            Console.WriteLine($"[AddElements {callId}] PRZED GenerateGenericElementsWithJoins");

            List<KonfModeleElementy> konfModeleElementy = generatorStates.Values
                .Where(x => x.MVCKonfModelu?.KonfModeleElementy != null)
                .SelectMany(x => x.MVCKonfModelu!.KonfModeleElementy!)
                .Where(e => e != null)
                .ToList();

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
                konfModeleElementy,
                mouseClik,
                oryginalnyRegionId);

            Console.WriteLine($"[AddElements {callId}] PO GenerateGenericElementsWithJoins: {okLine}");
            Console.WriteLine($"◀ KONIEC AddElements: {callId}, regionId={regionId}");

            if (okLine)
            {
                Komunikaty.Add($"✅ Generowanie elementów zakończone sukcesem dla regionu {regionId}");
                return "";
            }
            else
            {
                BledySystemowe.Add($"❌ Generowanie elementów niepowiodło się dla regionu {regionId}");
                return $"❌ Generowanie elementów niepowiodło się dla regionu {regionId}";
            }
        }

        // ============================================================
        // Odległość punktu od odcinka
        // ============================================================
        private static double OdlegloscPunktuOdOdcinka(
            double px, double py, XPoint a, XPoint b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double dlugoscKw = dx * dx + dy * dy;

            if (dlugoscKw < 1e-12)
            {
                double ex0 = px - a.X, ey0 = py - a.Y;
                return Math.Sqrt(ex0 * ex0 + ey0 * ey0);
            }

            double t = ((px - a.X) * dx + (py - a.Y) * dy) / dlugoscKw;
            t = Math.Max(0, Math.Min(1, t));

            double nx = a.X + t * dx;
            double ny = a.Y + t * dy;

            double ex = px - nx, ey = py - ny;
            return Math.Sqrt(ex * ex + ey * ey);
        }

        // ============================================================
        // ORKIESTRATOR GenerateGenericElementsWithJoins
        // ============================================================
        public async Task<bool> GenerateGenericElementsWithJoins(
            List<XPoint> outer, List<XPoint> inner,
            List<ContourSegment> outerContourSegment, List<ContourSegment> innerContourSegment,
            float profileLeft, float profileRight, float profileTop, float profileBottom,
            string typKsztalt, string polaczenia, bool sposobLaczeniaCzop, List<KonfSystem> model, string regionId,
            Guid rowIdprofileLeft, Guid rowIdprofileRight, Guid rowIdprofileTop, Guid rowIdprofileBottom,
            string rowIndeksprofileLeft, string rowIndeksprofileRight, string rowIndeksprofileTop, string rowIndeksprofileBottom,
            string rowNazwaprofileLeft, string rowNazwaprofileRight, string rowNazwaprofileTop, string rowNazwaprofileBottom,
            string NazwaObiektu, string TypObiektu, List<DaneKwadratu> daneKwadratu, List<XPoint> punktyRegionuMaster,
            List<KonfModeleElementy> konfModeleElementy,
            XPoint mouseClik,
            string? oryginalnyRegionId = null)
        {
            await Task.Yield();
            await Task.Delay(2);

            // 1. Walidacja + przygotowanie outer/inner
            var walidacja = WalidujIWstepniePrzygotuj(
                ref outer, ref inner, regionId, ElementLiniowy, daneKwadratu,
                out var szukDaneKwadratu);

            if (!walidacja.Ok)
            {
                if (walidacja.Blad != null) BledySystemowe.Add(walidacja.Blad);
                return false;
            }

            int vertexCount = outer.Count;

            // 2. Parsowanie wzorca połączeń (z polaczenia string)
            var wzorzecPolaczen = ParsujWzorzecPolaczen(polaczenia);

            // 3. Zliczanie elementów wg stron
            var elementyWedlugStron = ZbudujElementyWedlugStron(
                outer, wzorzecPolaczen, ElementLiniowy);

            // 4. Typy narożników (pełne 4×4, z fallbackiem)
            var typyNaroznikow = ZbudujTypyNaroznikow(wzorzecPolaczen, ElementLiniowy);

            // 5. Główna pętla po bokach
            float firstangleDegrees = -1;
            string stonaOstanioDodanegoElementu = "";

            for (int i = 0; i < vertexCount; i++)
            {
                int next = (i + 1) % vertexCount;
                int prev = (i - 1 + vertexCount) % vertexCount;

                var geo = ObliczGeometrieBoku(
                    outer, inner, i, next, prev,
                    profileLeft, profileRight, profileTop, profileBottom,
                    ref firstangleDegrees);

                if (geo.Length < 0.001f) continue;

                // Uzupełnij geometrię o Guidy i profile (używane w WierzcholkiT5T5)
                geo.ProfileLeft = profileLeft;
                geo.ProfileRight = profileRight;
                geo.ProfileTop = profileTop;
                geo.ProfileBottom = profileBottom;
                geo.RowIdprofileLeft = rowIdprofileLeft;
                geo.RowIdprofileRight = rowIdprofileRight;
                geo.RowIdprofileTop = rowIdprofileTop;
                geo.RowIdprofileBottom = rowIdprofileBottom;

                StronaElementu = geo.CurrentSide;

                var (leftJoin, rightJoin) = PobierzTypyPolaczenDlaBoku(
                    geo.PrevSide, geo.CurrentSide, geo.NextSide,
                    typyNaroznikow, wzorzecPolaczen, ElementLiniowy);

                (leftJoin, rightJoin, bool dodajA, bool dodajB) = SkorygujPolaczeniaDlaKatow(
                    leftJoin, rightJoin,
                    geo.AngleDegreesStronaA, geo.AngleDegreesStronaB,
                    sposobLaczeniaCzop,
                    geo.IsAlmostHorizontal, geo.IsAlmostVertical,
                    vertexCount, i);

                var wynik = WyznaczWierzcholkiDlaBoku(
                    leftJoin, rightJoin,
                    outer, inner,
                    outerContourSegment, innerContourSegment,
                    geo, i, next, prev, vertexCount,
                    firstangleDegrees,
                    stonaOstanioDodanegoElementu,
                    daneKwadratu, punktyRegionuMaster, mouseClik,
                    ElementLiniowy);

                if (!wynik.Ok)
                {
                    if (wynik.Blad != null) BledySystemowe.Add(wynik.Blad);
                    Console.WriteLine($"❌ Błąd w WyznaczWierzcholkiDlaBoku dla regionu {regionId}: {wynik.Blad}");
                    continue;
                }

                var wierzcholki = wynik.Wierzcholki!;
                var wierzcholkiZLukami = wynik.WierzcholkiZLukami!;

                var zapisOk = ZapiszKsztaltElementu(
                    wierzcholki, wierzcholkiZLukami,
                    i, geo, typKsztalt,
                    NazwaObiektu, TypObiektu,
                    oryginalnyRegionId ?? regionId,
                    rowIdprofileLeft, rowIdprofileRight, rowIdprofileTop, rowIdprofileBottom,
                    rowIndeksprofileLeft, rowIndeksprofileRight, rowIndeksprofileTop, rowIndeksprofileBottom,
                    rowNazwaprofileLeft, rowNazwaprofileRight, rowNazwaprofileTop, rowNazwaprofileBottom,
                    leftJoin, rightJoin, dodajA, dodajB);

                if (!zapisOk) continue;

                stonaOstanioDodanegoElementu = StronaElementu;

                if (ElementLiniowy) return true;
            }

            await Task.CompletedTask;
            return true;
        }

        // ============================================================
        // WALIDACJA I WSTĘPNE PRZYGOTOWANIE
        // ============================================================
        private WynikWalidacji WalidujIWstepniePrzygotuj(
            ref List<XPoint> outer,
            ref List<XPoint> inner,
            string regionId,
            bool elementLiniowy,
            List<DaneKwadratu> daneKwadratu,
            out DaneKwadratu? szukDaneKwadratu)
        {
            szukDaneKwadratu = null;

            if (elementLiniowy)
            {
                if (outer == null || outer.Count < 2)
                    return new(false, $"❌ ElementLiniowy: min. 2 punkty (regionId={regionId}).");

                szukDaneKwadratu = daneKwadratu
                    .Where(x => x.Wierzcholki.Count == 2 && x.BoolElementLinia)
                    .DistinctBy(x => (
                        Math.Round(x.Wierzcholki[0].X, 2),
                        Math.Round(x.Wierzcholki[0].Y, 2),
                        Math.Round(x.Wierzcholki[1].X, 2),
                        Math.Round(x.Wierzcholki[1].Y, 2)
                    ))
                    .LastOrDefault();

                if (szukDaneKwadratu == null ||
                    szukDaneKwadratu.Wierzcholki == null ||
                    szukDaneKwadratu.Wierzcholki.Count < 2)
                {
                    return new(false, $"❌ ElementLiniowy: brak szukDaneKwadratu dla {regionId}.");
                }
            }
            else
            {
                if (outer == null || outer.Count < 3)
                    return new(false, $"❌ Wielokąt musi mieć ≥3 wierzchołki (regionId={regionId}).");

                outer = RemoveDuplicateConsecutivePoints(outer);
                inner = RemoveDuplicateConsecutivePoints(inner);
            }

            return new(true, null);
        }

        // ============================================================
        // PARSOWANIE WZORCA POŁĄCZEŃ
        // ============================================================
        private Dictionary<string, string> ParsujWzorzecPolaczen(string polaczenia)
        {
            var wzorzec = new Dictionary<string, string>();
            var katy = new List<(double kat, string strona, string typ)>();

            foreach (var pair in polaczenia.Split(';'))
            {
                var parts = pair.Split('-');
                if (parts.Length < 2) continue;
                if (!double.TryParse(parts[0], out double kat)) continue;

                string typ = parts[1];
                string strona = StronaOknaHelper.OkreslStroneNaPodstawieKataLinii(kat);
                katy.Add((kat, strona, typ));
            }

            foreach (var (_, strona, typ) in katy)
            {
                if (!wzorzec.ContainsKey(strona))
                    wzorzec[strona] = typ;
                else
                {
                    string pr = PrzeciwnaStrona(strona);
                    if (pr != "Nieznana" && !wzorzec.ContainsKey(pr)) wzorzec[pr] = typ;
                }
            }

            foreach (var s in new[] { "Góra", "Dół", "Prawa", "Lewa" })
            {
                if (!wzorzec.ContainsKey(s))
                {
                    string pr = PrzeciwnaStrona(s);
                    if (pr != "Nieznana" && wzorzec.ContainsKey(pr))
                        wzorzec[s] = wzorzec[pr];
                    else
                        wzorzec[s] = ElementLiniowy ? "T5" : "T2";
                }
            }

            return wzorzec;
        }

        private static string PrzeciwnaStrona(string strona) => strona switch
        {
            "Góra" => "Dół",
            "Dół" => "Góra",
            "Prawa" => "Lewa",
            "Lewa" => "Prawa",
            _ => "Nieznana"
        };

        // ============================================================
        // ZBUDUJ ELEMENTY WEDŁUG STRON
        // ============================================================
        private Dictionary<string, List<int>> ZbudujElementyWedlugStron(
            List<XPoint> outer, Dictionary<string, string> wzorzec, bool elementLiniowy)
        {
            var result = new Dictionary<string, List<int>>();
            if (outer == null || outer.Count < 2) return result;

            if (outer.Count == 2)
            {
                var p1 = outer[0]; var p2 = outer[1];
                float dx = (float)(p2.X - p1.X);
                float dy = (float)(p2.Y - p1.Y);
                float ang = MathF.Atan2(dy, dx) * (180f / MathF.PI);
                if (ang < 0) ang += 360f;

                string typLinii = elementLiniowy
                    ? "T5"
                    : (wzorzec.TryGetValue(StronaOknaHelper.OkreslStroneNaPodstawieKataLinii(ang), out var t) ? t : "T2");

                string g, pr;
                if (ang >= 315 || ang < 45 || (ang >= 135 && ang < 225)) { g = "Góra"; pr = "Dół"; }
                else { g = "Prawa"; pr = "Lewa"; }

                result[g] = new List<int> { 0 };
                result[pr] = new List<int> { 1 };
                wzorzec[g] = typLinii;
                wzorzec[pr] = typLinii;
                return result;
            }

            for (int i = 0; i < outer.Count; i++)
            {
                int next = (i + 1) % outer.Count;
                float dx = (float)(outer[next].X - outer[i].X);
                float dy = (float)(outer[next].Y - outer[i].Y);
                float ang = MathF.Atan2(dy, dx) * (180f / MathF.PI);
                if (ang < 0) ang += 360f;

                string strona = StronaOknaHelper.OkreslStroneNaPodstawieKataLinii(ang);
                if (!result.ContainsKey(strona)) result[strona] = new List<int>();
                result[strona].Add(i);
            }
            return result;
        }

        // ============================================================
        // ZBUDUJ TYPY NAROŻNIKÓW (pełne 4×4)
        // ============================================================
        private Dictionary<string, string> ZbudujTypyNaroznikow(
            Dictionary<string, string> wzorzec, bool elementLiniowy)
        {
            var typy = new Dictionary<string, string>();
            var wszystkie = new[] { "Góra", "Dół", "Prawa", "Lewa" };

            foreach (var a in wszystkie)
                foreach (var b in wszystkie)
                {
                    string klucz = $"{a}-{b}";
                    if (a == b)
                        typy[klucz] = wzorzec.TryGetValue(a, out var t) ? t : (elementLiniowy ? "T5" : "T2");
                    else
                    {
                        if (wzorzec.TryGetValue(b, out var tb)) typy[klucz] = tb;
                        else if (wzorzec.TryGetValue(a, out var ta)) typy[klucz] = ta;
                        else typy[klucz] = elementLiniowy ? "T5" : "T2";
                    }
                }
            return typy;
        }

        // ============================================================
        // OBLICZ GEOMETRIĘ BOKU
        // ============================================================
        private GeometriaBoku ObliczGeometrieBoku(
            List<XPoint> outer, List<XPoint> inner, int i, int next, int prev,
            float profileLeft, float profileRight, float profileTop, float profileBottom,
            ref float firstangleDegrees)
        {
            var g = new GeometriaBoku();

            g.Dx = (float)(outer[next].X - outer[i].X);
            g.Dy = (float)(outer[next].Y - outer[i].Y);
            g.AngleDegrees = MathF.Atan2(g.Dy, g.Dx) * (180f / MathF.PI);
            if (g.AngleDegrees < 0) g.AngleDegrees += 360f;

            if (firstangleDegrees == -1) firstangleDegrees = g.AngleDegrees;

            g.CurrentSide = StronaOknaHelper.OkreslStrone(g.AngleDegrees, i, outer);

            float dxPrev = (float)(outer[i].X - outer[prev].X);
            float dyPrev = (float)(outer[i].Y - outer[prev].Y);
            g.AnglePrev = MathF.Atan2(dyPrev, dxPrev) * 180f / MathF.PI;
            if (g.AnglePrev < 0) g.AnglePrev += 360f;
            g.PrevSide = StronaOknaHelper.OkreslStrone(g.AnglePrev, prev, outer);

            int next2 = (next + 1) % outer.Count;
            float dxNext = (float)(outer[next2].X - outer[next].X);
            float dyNext = (float)(outer[next2].Y - outer[next].Y);
            g.AngleNext = MathF.Atan2(dyNext, dxNext) * 180f / MathF.PI;
            if (g.AngleNext < 0) g.AngleNext += 360f;
            g.NextSide = StronaOknaHelper.OkreslStrone(g.AngleNext, next, outer);

            g.Length = MathF.Sqrt(g.Dx * g.Dx + g.Dy * g.Dy);
            if (g.Length < 0.001f) return g;

            g.Tx = g.Dx / g.Length;
            g.Ty = g.Dy / g.Length;
            g.Nx = -g.Ty;
            g.Ny = g.Tx;

            g.Profile = Math.Abs(g.Dx) > Math.Abs(g.Dy)
                ? (g.Ny > 0 ? profileTop : profileBottom)
                : (g.Nx > 0 ? profileRight : profileLeft);

            g.ProfileA = Math.Abs(g.Dx) > Math.Abs(g.Dy) ? profileTop : profileRight;
            g.ProfileB = Math.Abs(g.Dx) > Math.Abs(g.Dy) ? profileBottom : profileLeft;

            g.IsAlmostHorizontal = Math.Abs(g.Dy) < 1e-2;
            g.IsAlmostVertical = Math.Abs(g.Dx) < 1e-2;

            // Kąty między bokami
            double cdx = outer[next].X - outer[i].X;
            double cdy = outer[next].Y - outer[i].Y;
            double pdx = outer[i].X - outer[prev].X;
            double pdy = outer[i].Y - outer[prev].Y;
            double ndx = outer[next2].X - outer[next].X;
            double ndy = outer[next2].Y - outer[next].Y;

            double mc = Math.Sqrt(cdx * cdx + cdy * cdy);
            double mp = Math.Sqrt(pdx * pdx + pdy * pdy);
            double mn = Math.Sqrt(ndx * ndx + ndy * ndy);

            if (mc > 1e-6 && mp > 1e-6)
            {
                double dot = cdx * pdx + cdy * pdy;
                double cos = Math.Max(-1, Math.Min(1, dot / (mc * mp)));
                g.AngleDegreesStronaA = (float)(Math.Acos(cos) * 180.0 / Math.PI);
            }

            if (mc > 1e-6 && mn > 1e-6)
            {
                double dot = cdx * ndx + cdy * ndy;
                double cos = Math.Max(-1, Math.Min(1, dot / (mc * mn)));
                g.AngleDegreesStronaB = (float)(Math.Acos(cos) * 180.0 / Math.PI);
            }

            return g;
        }

        // ============================================================
        // POBIERZ TYPY POŁĄCZEŃ DLA BOKU (bezpieczne, z fallbackiem)
        // ============================================================
        private (string leftJoin, string rightJoin) PobierzTypyPolaczenDlaBoku(
            string prevSide, string currentSide, string nextSide,
            Dictionary<string, string> typyNaroznikow,
            Dictionary<string, string> wzorzec,
            bool elementLiniowy)
        {
            string lKey = $"{prevSide}-{currentSide}";
            string rKey = $"{currentSide}-{nextSide}";
            string def = elementLiniowy ? "T5" : "T2";

            string leftJoin = typyNaroznikow.TryGetValue(lKey, out var lj)
                ? lj
                : (wzorzec.TryGetValue(currentSide, out var c1) ? c1
                  : (wzorzec.TryGetValue(prevSide, out var p1) ? p1 : def));

            string rightJoin = typyNaroznikow.TryGetValue(rKey, out var rj)
                ? rj
                : (wzorzec.TryGetValue(currentSide, out var c2) ? c2
                  : (wzorzec.TryGetValue(nextSide, out var n1) ? n1 : def));

            if (typyNaroznikow.TryGetValue(lKey, out _) == false)
                Komunikaty.Add($"⚠️ Brak klucza '{lKey}' — fallback: {leftJoin}");

            if (typyNaroznikow.TryGetValue(rKey, out _) == false)
                Komunikaty.Add($"⚠️ Brak klucza '{rKey}' — fallback: {rightJoin}");

            return (leftJoin, rightJoin);
        }

        // ============================================================
        // SKORYGUJ POŁĄCZENIA DLA KĄTÓW (T2 + czopy)
        // ============================================================
        private (string leftJoin, string rightJoin, bool dodajA, bool dodajB) SkorygujPolaczeniaDlaKatow(
            string leftJoin, string rightJoin,
            float angleA, float angleB,
            bool sposobLaczeniaCzop,
            bool isAlmostHorizontal, bool isAlmostVertical,
            int vertexCount, int i)
        {
            const double katOstryT2 = 46.0;

            if (angleA > 0.001 && angleA <= katOstryT2 && leftJoin != "T5")
            {
                leftJoin = "T2";
                BledySystemowe.Add($"⚠️ Element {i + 1}: ostry kąt lewy {angleA:F1}° → T2.");
            }

            if (angleB > 0.001 && angleB <= katOstryT2 && rightJoin != "T5")
            {
                rightJoin = "T2";
                BledySystemowe.Add($"⚠️ Element {i + 1}: ostry kąt prawy {angleB:F1}° → T2.");
            }

            bool dodajA = false, dodajB = false;

            if (sposobLaczeniaCzop)
            {
                if (leftJoin == "T1" && isAlmostVertical) dodajA = true;
                if (rightJoin == "T1" && isAlmostVertical) dodajB = true;
                if (leftJoin == "T3" && isAlmostHorizontal) dodajA = true;
                if (rightJoin == "T3" && isAlmostHorizontal) dodajB = true;
                if (leftJoin == "T5") dodajA = true;
                if (rightJoin == "T5") dodajB = true;
                if (leftJoin == "T2") dodajA = true;
                if (rightJoin == "T2") dodajB = true;
            }

            return (leftJoin, rightJoin, dodajA, dodajB);
        }

        // ============================================================
        // WYZNACZ WIERZCHOŁKI DLA BOKU — dispatcher
        // ============================================================
        private WynikWierzcholkow WyznaczWierzcholkiDlaBoku(
            string leftJoin, string rightJoin,
            List<XPoint> outer, List<XPoint> inner,
            List<ContourSegment> outerContourSegment, List<ContourSegment> innerContourSegment,
            GeometriaBoku geo,
            int i, int next, int prev, int vertexCount,
            float firstangleDegrees,
            string stonaOstanioDodanegoElementu,
            List<DaneKwadratu> daneKwadratu,
            List<XPoint> punktyRegionuMaster,
            XPoint mouseClik,
            bool elementLiniowy)
        {
            List<XPoint>? wierzcholki = null;

            try
            {
                if (leftJoin == "T4" && rightJoin == "T4")
                {
                    var s = GetStartT4(inner[i]);
                    var e = GetEndT4(inner[next]);
                    wierzcholki = new List<XPoint> { s[1], e[1], e[0], s[0] };
                }
                else if ((leftJoin == "T1" && rightJoin == "T4") || (leftJoin == "T4" && rightJoin == "T1"))
                {
                    wierzcholki = WierzcholkiT1T4(leftJoin, rightJoin, outer, inner, geo, i, next, prev, vertexCount, firstangleDegrees, stonaOstanioDodanegoElementu);
                }
                else if (leftJoin == "T1" && rightJoin == "T1")
                {
                    wierzcholki = WierzcholkiT1T1(outer, inner, geo, i, next, prev, vertexCount, firstangleDegrees, stonaOstanioDodanegoElementu);
                }
                else if (leftJoin == "T3" && rightJoin == "T3")
                {
                    wierzcholki = WierzcholkiT3T3(outer, inner, geo, i, next, prev, vertexCount, firstangleDegrees, stonaOstanioDodanegoElementu);
                }
                else if (leftJoin == "T2" && rightJoin == "T2")
                {
                    wierzcholki = WierzcholkiT2T2(outer, inner, i, next);
                }
                else if (leftJoin == "T5" && rightJoin == "T5")
                {
                    var w = WierzcholkiT5T5(outer, inner, i, next, daneKwadratu, punktyRegionuMaster, mouseClik, geo);
                    if (!w.Ok) return w;
                    wierzcholki = w.Wierzcholki;
                }
                else if (leftJoin == "T2" && rightJoin == "T1")
                {
                    wierzcholki = WierzcholkiT2T1(outer, inner, geo, i, next, prev, vertexCount, firstangleDegrees, stonaOstanioDodanegoElementu);
                }
                else if (leftJoin == "T1" && rightJoin == "T2")
                {
                    wierzcholki = WierzcholkiT1T2(outer, inner, geo, i, next, prev, vertexCount, firstangleDegrees, stonaOstanioDodanegoElementu);
                }
                else if (leftJoin == "T3" && rightJoin == "T2")
                {
                    wierzcholki = WierzcholkiT3T2(outer, inner, geo, i, next, prev, vertexCount, firstangleDegrees, stonaOstanioDodanegoElementu);
                }
                else if (leftJoin == "T2" && rightJoin == "T3")
                {
                    wierzcholki = WierzcholkiT2T3(outer, inner, geo, i, next, prev, vertexCount, firstangleDegrees, stonaOstanioDodanegoElementu);
                }
                else if (leftJoin == "T3" && rightJoin == "T1")
                {
                    wierzcholki = WierzcholkiT3T1(outer, inner, geo, i, next, prev, vertexCount, firstangleDegrees, stonaOstanioDodanegoElementu);
                }
                else if (leftJoin == "T4" && rightJoin == "T3")
                {
                    wierzcholki = WierzcholkiT4T3(outer, inner, geo, i, next, prev, vertexCount, firstangleDegrees, stonaOstanioDodanegoElementu);
                }
                else if (leftJoin == "T3" && rightJoin == "T4")
                {
                    wierzcholki = WierzcholkiT3T4(outer, inner, geo, i, next, prev, vertexCount, firstangleDegrees, stonaOstanioDodanegoElementu);
                }
                else
                {
                    wierzcholki = WierzcholkiT2T2(outer, inner, i, next);
                }
            }
            catch (Exception ex)
            {
                return new WynikWierzcholkow
                {
                    Ok = false,
                    Blad = $"❌ WyznaczWierzcholkiDlaBoku [{leftJoin}/{rightJoin}] element {i + 1}: {ex.Message}"
                };
            }

            if (wierzcholki == null || wierzcholki.Count < 4)
            {
                return new WynikWierzcholkow
                {
                    Ok = false,
                    Blad = $"❌ Zła liczba wierzchołków dla elementu {i + 1} ({leftJoin}/{rightJoin}): {wierzcholki?.Count ?? 0}"
                };
            }

            wierzcholki = RotateContourSegments(wierzcholki, Corner.BottomLeft, clockwise: true);

            var wierzcholkiZLukami = Build4SegmentContour(
                wierzcholki, outerContourSegment, innerContourSegment,
                i + 1, StronaElementu, wierzcholki,
                leftJoin, rightJoin, geo.AngleDegrees, geo.AngleNext, geo.AnglePrev);

            return new WynikWierzcholkow
            {
                Ok = true,
                Wierzcholki = wierzcholki,
                WierzcholkiZLukami = wierzcholkiZLukami
            };
        }

        // ============================================================
        // ZAPISZ KSZTAŁT ELEMENTU
        // ============================================================
        private bool ZapiszKsztaltElementu(
            List<XPoint> wierzcholki,
            List<ContourSegment> wierzcholkiZLukami,
            int i,
            GeometriaBoku geo,
            string typKsztalt,
            string nazwaObiektu, string typObiektu,
            string idRegion,
            Guid rowIdprofileLeft, Guid rowIdprofileRight, Guid rowIdprofileTop, Guid rowIdprofileBottom,
            string rowIndeksprofileLeft, string rowIndeksprofileRight, string rowIndeksprofileTop, string rowIndeksprofileBottom,
            string rowNazwaprofileLeft, string rowNazwaprofileRight, string rowNazwaprofileTop, string rowNazwaprofileBottom,
            string leftJoin, string rightJoin,
            bool dodajA, bool dodajB)
        {
            Guid rowIdProfil;
            string nazwaElemntu;
            string indeksElementu;
            Guid rowIdElementuStronaA;
            Guid rowIdElementuStronaB;

            switch (StronaElementu)
            {
                case "Lewa":
                    rowIdProfil = rowIdprofileLeft;
                    nazwaElemntu = rowNazwaprofileLeft;
                    indeksElementu = rowIndeksprofileLeft;
                    rowIdElementuStronaA = rowIdprofileBottom;
                    rowIdElementuStronaB = rowIdprofileTop;
                    break;
                case "Prawa":
                    rowIdProfil = rowIdprofileRight;
                    nazwaElemntu = rowNazwaprofileRight;
                    indeksElementu = rowIndeksprofileRight;
                    rowIdElementuStronaA = rowIdprofileTop;
                    rowIdElementuStronaB = rowIdprofileBottom;
                    break;
                case "Góra":
                    rowIdProfil = rowIdprofileTop;
                    nazwaElemntu = rowNazwaprofileTop;
                    indeksElementu = rowIndeksprofileTop;
                    rowIdElementuStronaA = rowIdprofileLeft;
                    rowIdElementuStronaB = rowIdprofileRight;
                    break;
                case "Dół":
                    rowIdProfil = rowIdprofileBottom;
                    nazwaElemntu = rowNazwaprofileBottom;
                    indeksElementu = rowIndeksprofileBottom;
                    rowIdElementuStronaA = rowIdprofileLeft;
                    rowIdElementuStronaB = rowIdprofileRight;
                    break;
                default:
                    rowIdProfil = rowIdprofileLeft;
                    nazwaElemntu = rowNazwaprofileLeft;
                    indeksElementu = rowIndeksprofileLeft;
                    rowIdElementuStronaA = Guid.Empty;
                    rowIdElementuStronaB = Guid.Empty;
                    break;
            }

            if (rowIdProfil == Guid.Empty)
            {
                BledySystemowe.Add($"⚠️ Brak rowIdProfil dla elementu {i + 1} (strona={StronaElementu}). Pomijam.");
                return false;
            }

            double regionMinX = wierzcholki.Min(p => p.X);
            double regionMaxX = wierzcholki.Max(p => p.X);
            double regionMinY = wierzcholki.Min(p => p.Y);
            double regionMaxY = wierzcholki.Max(p => p.Y);

            int wartoscX = (int)Math.Round(regionMaxX - regionMinX);
            int wartoscY = (int)Math.Round(regionMaxY - regionMinY);

            float bazowaDlugosc = DlugoscElementu(wierzcholki);

            ElementyRamyRysowane.Add(new KsztaltElementu
            {
                NrPozWModelu = i + 1,
                TypKsztaltu = typKsztalt,
                Wierzcholki = wierzcholki,
                WierzcholkiZLukami = wierzcholkiZLukami,
                WypelnienieZewnetrzne = "wood-pattern",
                WypelnienieWewnetrzne = KolorSzyby,
                Grupa = nazwaObiektu + $" {StronaElementu}-{i + 1} {wartoscX}/{wartoscY}",
                Typ = typObiektu,
                ZIndex = Zindeks,
                RowIdElementu = rowIdProfil,
                IdRegion = idRegion,
                Kat = (float)geo.AngleDegrees,
                KatStronaA = (float)geo.AngleDegreesStronaA,
                KatStronaB = (float)geo.AngleDegreesStronaB,
                OffsetLewa = StronaElementu == "Lewa" ? geo.ProfileLeft : 0,
                OffsetPrawa = StronaElementu == "Prawa" ? geo.ProfileRight : 0,
                OffsetDol = StronaElementu == "Dól" ? geo.ProfileBottom : 0,
                OffsetGora = StronaElementu == "Góra" ? geo.ProfileTop : 0,
                Strona = StronaElementu,
                IndeksElementu = indeksElementu,
                NazwaElementu = nazwaElemntu,
                DlogoscElementu = bazowaDlugosc + ((dodajA ? geo.ProfileA : 0) + (dodajB ? geo.ProfileB : 0)),
                DlogoscWidocznaElementu = bazowaDlugosc,
                DlugoscCzopaA = dodajA ? geo.ProfileA : -1,
                DlugoscCzopaB = dodajB ? geo.ProfileB : -1,
                RodzajpolaczenAiB = $"{leftJoin}/{rightJoin}",
                PolaczenieStronaA = leftJoin,
                PolaczenieStronaB = rightJoin,
                RowIdElementuStronaA = rowIdElementuStronaA,
                RowIdElementuStronaB = rowIdElementuStronaB
            });

            return true;
        }

        // ============================================================
        // WIERZCHOŁKI T1/T1
        // ============================================================
        private List<XPoint> WierzcholkiT1T1(
            List<XPoint> outer, List<XPoint> inner,
            GeometriaBoku geo,
            int i, int next, int prev, int vertexCount,
            float firstangleDegrees,
            string stonaOstanioDodanegoElementu)
        {
            if (vertexCount == 3)
            {
                var startT1 = GetStartT1Triangle(
                    inner[i], outer[i], outer,
                    geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                    StronaElementu, stonaOstanioDodanegoElementu,
                    i, next, prev);

                int nextTriangle = (i + 1) % vertexCount;

                var endT1 = GetEndT1Triangle(
                    inner[nextTriangle], outer[nextTriangle], outer,
                    geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                    StronaElementu, stonaOstanioDodanegoElementu,
                    i, next, prev);

                return new List<XPoint>
                {
                    startT1[1], endT1[1], endT1[0], startT1[0]
                };
            }

            var _anglePrev = geo.AnglePrev;
            if (i == vertexCount - 1) _anglePrev = firstangleDegrees;

            var getStartT1 = GetStartT1(
                inner[i], outer[i], outer,
                geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            var getEndT1 = GetEndT1(
                inner[next], outer[next], outer,
                geo.AngleDegrees, _anglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            return new List<XPoint>
            {
                getStartT1[1], getEndT1[1], getEndT1[0], getStartT1[0]
            };
        }

        // ============================================================
        // WIERZCHOŁKI T1/T2
        // ============================================================
        private List<XPoint> WierzcholkiT1T2(
            List<XPoint> outer, List<XPoint> inner,
            GeometriaBoku geo,
            int i, int next, int prev, int vertexCount,
            float firstangleDegrees,
            string stonaOstanioDodanegoElementu)
        {
            if (vertexCount == 3)
            {
                var getStartT2 = GetStartT2(inner[i], outer[i]);
                var getEndT2 = GetEndT2(inner[next], outer[next]);

                var getStartT1 = GetStartT1Triangle(
                    inner[i], outer[i], outer,
                    geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                    StronaElementu, stonaOstanioDodanegoElementu,
                    i, next, prev);

                return new List<XPoint>
                {
                    getStartT1[1], getEndT2[1], getEndT2[0], getStartT1[0]
                };
            }

            var sT2 = GetStartT2(inner[i], outer[i]);
            var eT2 = GetEndT2(inner[next], outer[next]);

            var sT1 = GetStartT1(
                inner[i], outer[i], outer,
                geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            return new List<XPoint>
            {
                sT1[1], eT2[1], eT2[0], sT1[0]
            };
        }

        // ============================================================
        // WIERZCHOŁKI T1/T4 lub T4/T1
        // ============================================================
        private List<XPoint> WierzcholkiT1T4(
            string leftJoin, string rightJoin,
            List<XPoint> outer, List<XPoint> inner,
            GeometriaBoku geo,
            int i, int next, int prev, int vertexCount,
            float firstangleDegrees,
            string stonaOstanioDodanegoElementu)
        {
            bool isTriangle = vertexCount == 3;

            XPoint outerStart = outer[i];
            XPoint outerEnd = outer[next];
            XPoint _innerStart = inner[i];
            XPoint _innerEnd = inner[next];

            float dx = geo.Dx;
            float dy = geo.Dy;
            float nx = geo.Nx;
            float ny = geo.Ny;
            float tx = geo.Tx;
            float ty = geo.Ty;
            float profile = geo.Profile;
            float angleDegrees = geo.AngleDegrees;
            float anglePrev = geo.AnglePrev;
            float angleNext = geo.AngleNext;

            // PRZYPADEK: T4 -> T1
            if (leftJoin == "T4" && rightJoin == "T1")
            {
                if (isTriangle)
                {
                    var getStartT1 = GetStartT1Triangle(
                        inner[i], outer[i], outer,
                        angleDegrees, anglePrev, angleNext,
                        StronaElementu, stonaOstanioDodanegoElementu,
                        i, next, prev);

                    var _anglePrev = anglePrev;
                    if (i == vertexCount - 1) _anglePrev = firstangleDegrees;

                    var getEndT1 = GetEndT1Triangle(
                        inner[next], outer[next], outer,
                        angleDegrees, _anglePrev, angleNext,
                        StronaElementu, stonaOstanioDodanegoElementu,
                        i, next, prev);

                    return new List<XPoint>
                    {
                        getStartT1[1], getEndT1[1], getEndT1[0], getStartT1[0]
                    };
                }

                if (geo.IsAlmostHorizontal)
                {
                    var outerVecStart = FindFirstEdgeIntersection(outerStart, nx, ny, outer);
                    var outerVecEnd = FindFirstEdgeIntersection(outerEnd, nx, ny, outer);

                    var innerVecStart = FindFirstEdgeIntersection(
                        new XPoint(outerVecStart.X + nx * profile, outerVecStart.Y + ny * profile),
                        tx, ty, outer);

                    var innerVecEnd = FindFirstEdgeIntersection(
                        new XPoint(outerVecEnd.X + nx * profile, outerVecEnd.Y + ny * profile),
                        tx, ty, outer);

                    return new List<XPoint>
                    {
                        outerVecStart, outerVecEnd, innerVecEnd, innerVecStart
                    };
                }

                if (leftJoin == "T4" && rightJoin == "T4" && vertexCount > 4)
                {
                    var topY = Math.Min(inner[i].Y, inner[next].Y);
                    var bottomY = Math.Max(inner[i].Y, inner[next].Y);

                    var outerTop = GetHorizontalIntersection(_innerStart, _innerEnd, (float)topY);
                    var outerBottom = GetHorizontalIntersection(_innerStart, _innerEnd, (float)bottomY);
                    var innerTop = GetHorizontalIntersection(outer[i], outer[next], (float)topY);
                    var innerBottom = GetHorizontalIntersection(outer[i], outer[next], (float)bottomY);

                    return new List<XPoint>
                    {
                        outerTop, outerBottom, innerBottom, innerTop
                    };
                }
                else
                {
                    var topY = Math.Min(inner[i].Y, inner[next].Y);
                    var bottomY = Math.Max(inner[i].Y, inner[next].Y);

                    var outerBottom = GetHorizontalIntersection(outerStart, outerEnd, (float)bottomY);
                    var innerTop = GetHorizontalIntersection(inner[i], inner[next], (float)topY);
                    var innerBottom = GetHorizontalIntersection(inner[i], inner[next], (float)bottomY);

                    XPoint outerTop;

                    if (i == vertexCount - 1)
                    {
                        outerTop = FindFirstEdgeIntersectionByAngle(innerTop, firstangleDegrees - 180, outer);
                    }
                    else if (angleDegrees == 270)
                    {
                        outerTop = FindFirstEdgeIntersectionByAngle(innerTop, 180 + angleNext, outer);
                    }
                    else
                    {
                        outerTop = FindFirstEdgeIntersectionByAngle(innerTop, anglePrev, outer);
                    }

                    return new List<XPoint>
                    {
                        outerTop, outerBottom, innerBottom, innerTop
                    };
                }
            }

            // PRZYPADEK: T1 -> T4
            if (isTriangle)
            {
                var getStartT1 = GetStartT1Triangle(
                    inner[i], outer[i], outer,
                    angleDegrees, anglePrev, angleNext,
                    StronaElementu, stonaOstanioDodanegoElementu,
                    i, next, prev);

                var _anglePrev = anglePrev;
                if (i == vertexCount - 1) _anglePrev = firstangleDegrees;

                var getEndT1 = GetEndT1Triangle(
                    inner[next], outer[next], outer,
                    angleDegrees, _anglePrev, angleNext,
                    StronaElementu, stonaOstanioDodanegoElementu,
                    i, next, prev);

                return new List<XPoint>
                {
                    getStartT1[1], getEndT1[1], getEndT1[0], getStartT1[0]
                };
            }

            if (geo.IsAlmostHorizontal)
            {
                var outerVecStart = FindFirstEdgeIntersection(outerStart, nx, ny, outer);
                var outerVecEnd = FindFirstEdgeIntersection(outerEnd, nx, ny, outer);

                var innerVecStart = FindFirstEdgeIntersection(
                    new XPoint(outerVecStart.X + nx * profile, outerVecStart.Y + ny * profile),
                    tx, ty, outer);

                var innerVecEnd = FindFirstEdgeIntersection(
                    new XPoint(outerVecEnd.X + nx * profile, outerVecEnd.Y + ny * profile),
                    tx, ty, outer);

                return new List<XPoint>
                {
                    outerVecStart, outerVecEnd, innerVecEnd, innerVecStart
                };
            }

            // Pionowy
            {
                var topY = Math.Min(inner[i].Y, inner[next].Y);
                var bottomY = Math.Max(inner[i].Y, inner[next].Y);

                var outerBottom = GetHorizontalIntersection(outerStart, outerEnd, (float)bottomY);
                var innerTop = GetHorizontalIntersection(inner[i], inner[next], (float)topY);
                var innerBottom = GetHorizontalIntersection(inner[i], inner[next], (float)bottomY);

                XPoint outerTop;

                if (i == vertexCount - 1)
                {
                    outerTop = FindFirstEdgeIntersectionByAngle(innerTop, firstangleDegrees - 180, outer);
                }
                else if (anglePrev == -1 && vertexCount < 4)
                {
                    innerTop = inner[i];
                    outerTop = FindFirstEdgeIntersectionByAngle(innerTop, anglePrev, outer);
                }
                else
                {
                    outerTop = FindFirstEdgeIntersectionByAngle(innerTop, anglePrev, outer);
                }

                if (vertexCount < 4 && anglePrev != -1)
                {
                    innerTop = FindFirstEdgeIntersectionByAngle(innerTop, angleDegrees - 180, outer);
                    outerTop = FindFirstEdgeIntersectionByAngle(outerTop, angleDegrees - 180, outer);
                }

                return new List<XPoint>
                {
                    outerTop, outerBottom, innerBottom, innerTop
                };
            }
        }

        // ============================================================
        // WIERZCHOŁKI T2/T1
        // ============================================================
        private List<XPoint> WierzcholkiT2T1(
            List<XPoint> outer, List<XPoint> inner,
            GeometriaBoku geo,
            int i, int next, int prev, int vertexCount,
            float firstangleDegrees,
            string stonaOstanioDodanegoElementu)
        {
            if (vertexCount == 3)
            {
                var getStartT2 = GetStartT2(inner[i], outer[i]);
                var getEndT2 = GetEndT2(inner[next], outer[next]);

                var getStartT1 = GetStartT1(
                    inner[i], outer[i], outer,
                    geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                    StronaElementu, stonaOstanioDodanegoElementu,
                    vertexCount < 6 ? -1 : i);

                int nextTriangle = (i + 1) % vertexCount;

                var getEndT1 = GetEndT1Triangle(
                    inner[nextTriangle], outer[nextTriangle], outer,
                    geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                    StronaElementu, stonaOstanioDodanegoElementu,
                    i, next, prev);

                return new List<XPoint>
                {
                    getStartT2[1], getEndT2[1], getEndT1[0], getStartT2[0]
                };
            }

            var _anglePrev = geo.AnglePrev;
            if (i == vertexCount - 1) _anglePrev = firstangleDegrees;

            var sT2 = GetStartT2(inner[i], outer[i]);
            var eT2 = GetEndT2(inner[next], outer[next]);

            var sT1 = GetStartT1(
                inner[i], outer[i], outer,
                geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            var eT1 = GetEndT1(
                inner[next], outer[next], outer,
                geo.AngleDegrees, _anglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            return new List<XPoint>
            {
                sT2[1], eT2[1], eT1[0], sT2[0]
            };
        }

        // ============================================================
        // WIERZCHOŁKI T2/T2
        // ============================================================
        private List<XPoint> WierzcholkiT2T2(
            List<XPoint> outer, List<XPoint> inner, int i, int next)
        {
            var sT2 = GetStartT2(inner[i], outer[i]);
            var eT2 = GetEndT2(inner[next], outer[next]);
            return new List<XPoint> { sT2[1], eT2[1], eT2[0], sT2[0] };
        }

        // ============================================================
        // WIERZCHOŁKI T2/T3
        // ============================================================
        private List<XPoint> WierzcholkiT2T3(
            List<XPoint> outer, List<XPoint> inner,
            GeometriaBoku geo,
            int i, int next, int prev, int vertexCount,
            float firstangleDegrees,
            string stonaOstanioDodanegoElementu)
        {
            var sT3 = GetStartT3(
                inner[i], outer[i], outer,
                geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            var _anglePrev = geo.AnglePrev;
            if (i == vertexCount - 1) _anglePrev = firstangleDegrees;

            var eT3 = GetEndT3(
                inner[next], outer[next], outer,
                geo.AngleDegrees, _anglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            var sT2 = GetStartT2(inner[i], outer[i]);
            var eT2 = GetEndT2(inner[next], outer[next]);

            return new List<XPoint> { sT2[1], eT3[1], eT3[0], sT2[0] };
        }

        // ============================================================
        // WIERZCHOŁKI T3/T1
        // ============================================================
        private List<XPoint> WierzcholkiT3T1(
            List<XPoint> outer, List<XPoint> inner,
            GeometriaBoku geo,
            int i, int next, int prev, int vertexCount,
            float firstangleDegrees,
            string stonaOstanioDodanegoElementu)
        {
            var sT1 = GetStartT1(
                inner[i], outer[i], outer,
                geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            var _anglePrev = geo.AnglePrev;
            if (i == vertexCount - 1) _anglePrev = firstangleDegrees;

            List<XPoint> eT1;
            if (vertexCount == 3)
            {
                int nextTriangle = (i + 1) % vertexCount;
                eT1 = GetEndT1Triangle(
                    inner[nextTriangle], outer[nextTriangle], outer,
                    geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                    StronaElementu, stonaOstanioDodanegoElementu,
                    i, next, prev);
            }
            else
            {
                eT1 = GetEndT1(
                    inner[next], outer[next], outer,
                    geo.AngleDegrees, _anglePrev, geo.AngleNext,
                    StronaElementu, stonaOstanioDodanegoElementu,
                    vertexCount < 6 ? -1 : i);
            }

            var sT3 = GetStartT3(
                inner[i], outer[i], outer,
                geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            var eT3 = GetEndT3(
                inner[next], outer[next], outer,
                geo.AngleDegrees, _anglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            return new List<XPoint> { sT3[1], eT1[1], eT1[0], sT3[0] };
        }

        // ============================================================
        // WIERZCHOŁKI T3/T2
        // ============================================================
        private List<XPoint> WierzcholkiT3T2(
            List<XPoint> outer, List<XPoint> inner,
            GeometriaBoku geo,
            int i, int next, int prev, int vertexCount,
            float firstangleDegrees,
            string stonaOstanioDodanegoElementu)
        {
            var sT3 = GetStartT3(
                inner[i], outer[i], outer,
                geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 || geo.AnglePrev == 270 ? -1 : i);

            var _anglePrev = geo.AnglePrev;
            if (i == vertexCount - 1) _anglePrev = firstangleDegrees;

            var eT3 = GetEndT3(
                inner[next], outer[next], outer,
                geo.AngleDegrees, _anglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            var sT2 = GetStartT2(inner[i], outer[i]);
            var eT2 = GetEndT2(inner[next], outer[next]);

            return new List<XPoint> { sT3[1], eT2[1], eT2[0], sT3[0] };
        }

        // ============================================================
        // WIERZCHOŁKI T3/T3
        // ============================================================
        private List<XPoint> WierzcholkiT3T3(
            List<XPoint> outer, List<XPoint> inner,
            GeometriaBoku geo,
            int i, int next, int prev, int vertexCount,
            float firstangleDegrees,
            string stonaOstanioDodanegoElementu)
        {
            if (vertexCount == 3)
            {
                var sT3 = GetStartT3Triangle(
                    inner[i], outer[i], outer,
                    geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                    StronaElementu, stonaOstanioDodanegoElementu,
                    i, next, prev);

                var _anglePrev = geo.AnglePrev;
                if (i == vertexCount - 1) _anglePrev = firstangleDegrees;

                var eT3 = GetEndT3Triangle(
                    inner[next], outer[next], outer,
                    geo.AngleDegrees, _anglePrev, geo.AngleNext,
                    StronaElementu, stonaOstanioDodanegoElementu,
                    i, next, prev);

                return new List<XPoint>
                {
                    sT3[1], eT3[1], eT3[0], sT3[0]
                };
            }

            var _angPrev = geo.AnglePrev;
            if (i == vertexCount - 1) _angPrev = firstangleDegrees;

            var sT3e = GetStartT3(
                inner[i], outer[i], outer,
                geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            var eT3e = GetEndT3(
                inner[next], outer[next], outer,
                geo.AngleDegrees, _angPrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            return new List<XPoint>
            {
                sT3e[1], eT3e[1], eT3e[0], sT3e[0]
            };
        }

        // ============================================================
        // WIERZCHOŁKI T3/T4
        // ============================================================
        private List<XPoint> WierzcholkiT3T4(
            List<XPoint> outer, List<XPoint> inner,
            GeometriaBoku geo,
            int i, int next, int prev, int vertexCount,
            float firstangleDegrees,
            string stonaOstanioDodanegoElementu)
        {
            var sT3 = GetStartT3(
                inner[i], outer[i], outer,
                geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 && vertexCount > 3 ? -1 : i);

            var _anglePrev = geo.AnglePrev;
            if (i == vertexCount - 1) _anglePrev = firstangleDegrees;

            var eT3 = GetEndT3(
                inner[next], outer[next], outer,
                geo.AngleDegrees, _anglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            var eT4 = GetEndT4(inner[next]);

            var _getStartT4 = FindFirstEdgeIntersectionByAngle(eT4[0], geo.AngleNext - 180, outer);

            return new List<XPoint>
            {
                sT3[1], _getStartT4, eT4[0], sT3[0]
            };
        }

        // ============================================================
        // WIERZCHOŁKI T4/T3
        // ============================================================
        private List<XPoint> WierzcholkiT4T3(
            List<XPoint> outer, List<XPoint> inner,
            GeometriaBoku geo,
            int i, int next, int prev, int vertexCount,
            float firstangleDegrees,
            string stonaOstanioDodanegoElementu)
        {
            var sT3 = GetStartT3(
                inner[i], outer[i], outer,
                geo.AngleDegrees, geo.AnglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            var _anglePrev = geo.AnglePrev;
            if (i == vertexCount - 1) _anglePrev = firstangleDegrees;

            var eT3 = GetEndT3(
                inner[next], outer[next], outer,
                geo.AngleDegrees, _anglePrev, geo.AngleNext,
                StronaElementu, stonaOstanioDodanegoElementu,
                vertexCount < 6 ? -1 : i);

            var sT4 = GetStartT4(inner[i]);

            var _getStartT3 = FindFirstEdgeIntersectionByAngle(inner[i], geo.AnglePrev, outer);

            return new List<XPoint>
            {
                _getStartT3, eT3[1], eT3[0], sT4[0]
            };
        }

        // ============================================================
        // WIERZCHOŁKI T5/T5 — słupek stały
        // ============================================================
        private WynikWierzcholkow WierzcholkiT5T5(
            List<XPoint> outer, List<XPoint> inner, int i, int next,
            List<DaneKwadratu> daneKwadratu,
            List<XPoint> punktyRegionuMaster,
            XPoint mouseClik,
            GeometriaBoku geo)
        {
            Console.WriteLine($"🔷 T5-T5 case for element {i + 1}. " +
                              $"isAlmostHorizontal:{geo.IsAlmostHorizontal}, " +
                              $"isAlmostVertical:{geo.IsAlmostVertical}, " +
                              $"daneKwadratu.Count:{daneKwadratu?.Count ?? 0}");

            Console.WriteLine($"🔷 T5-T5 ElementLiniowy " +
                              $"profileLeft: {geo.ProfileLeft}, profileRight: {geo.ProfileRight}, " +
                              $"profileTop: {geo.ProfileTop}, profileBottom: {geo.ProfileBottom}");

            // ============================================================
            // 1. SZEROKOŚĆ SŁUPKA I OŚ SYMETRII
            // ============================================================
            double? SzerokoscSlupka = 0;
            float OsSymetrii = 0;

            if (daneKwadratu != null && daneKwadratu.Count > 0)
            {
                var szerSlupka = KonfiguracjeSystemu.FirstOrDefault(x => x.RowId == geo.RowIdprofileLeft);

                if (szerSlupka != null)
                {
                    if (szerSlupka.WystepujeDol && szerSlupka.WystepujeGora &&
                        (!szerSlupka.WystepujeLewa || !szerSlupka.WystepujePrawa))
                    {
                        OsSymetrii = (float)Math.Abs((float)szerSlupka.PoziomOsSymetrii);
                        SzerokoscSlupka = szerSlupka.PoziomGora - szerSlupka.PoziomDol;
                    }
                    else if (szerSlupka.WystepujeLewa && szerSlupka.WystepujePrawa)
                    {
                        OsSymetrii = (float)Math.Abs((float)szerSlupka.PoziomOsSymetrii);
                        SzerokoscSlupka = szerSlupka.PoziomGora - szerSlupka.PoziomDol;
                    }
                    else
                    {
                        BledySystemowe.Add($"⚠️ T5-T5: Brak danych dla rowIdprofileLeft: {geo.RowIdprofileLeft}. Nie można obliczyć szerokości słupka.");
                    }
                }
                else
                {
                    BledySystemowe.Add($"⚠️ T5-T5: Brak danych dla rowIdprofileLeft: {geo.RowIdprofileLeft}. Nie można obliczyć szerokości słupka.");
                }
            }
            else
            {
                BledySystemowe.Add($"⚠️ T5-T5: Brak danych dla rowIdprofileLeft: {geo.RowIdprofileLeft}. Nie można obliczyć szerokości słupka.");
            }

            if (SzerokoscSlupka == 0)
            {
                BledySystemowe.Add($"⚠️ T5-T5: Szerokość słupka wynosi 0 dla rowIdprofileLeft: {geo.RowIdprofileLeft}. Nie można obliczyć szerokości słupka.");
            }
            else
            {
                Console.WriteLine($"⚠️ T5-T5: Szerokość słupka wynosi: {SzerokoscSlupka} dla rowIdprofileLeft: {geo.RowIdprofileLeft}.");
            }

            // ============================================================
            // 2. LINIA SŁUPKA (LiniaStala)
            // ============================================================
            var liniaStala = daneKwadratu
                .Where(x => x.LiniaStala != null && x.LiniaStala.Count() > 1)
                .LastOrDefault();

            if (liniaStala == null ||
                liniaStala.Wierzcholki == null ||
                liniaStala.Wierzcholki.Count < 2)
            {
                return new WynikWierzcholkow
                {
                    Ok = false,
                    Blad = "⚠️ T5-T5: brak LiniaStala / Wierzcholki dla słupka."
                };
            }

            XPoint TopXT5 = new XPoint
            {
                X = liniaStala.Wierzcholki[0].X,
                Y = liniaStala.Wierzcholki[0].Y
            };

            XPoint BottomXT5 = new XPoint
            {
                X = liniaStala.Wierzcholki[1].X,
                Y = liniaStala.Wierzcholki[1].Y
            };

            XPoint tmpTopST5 = new XPoint { };
            XPoint tmpTopLT5 = new XPoint { };
            XPoint tmpTopRT5 = new XPoint { };

            // ============================================================
            // 3. WEKTOR OSI SŁUPKA
            // ============================================================
            double dxT5 = BottomXT5.X - TopXT5.X;
            double dyT5 = BottomXT5.Y - TopXT5.Y;
            double dlugosc = Math.Sqrt(dxT5 * dxT5 + dyT5 * dyT5);

            if (dlugosc < 0.001)
            {
                return new WynikWierzcholkow
                {
                    Ok = false,
                    Blad = "⚠️ T5-T5: długość linii słupka < 0.001 — nie mogę zbudować konturu."
                };
            }

            double uxT5 = dxT5 / dlugosc;
            double uyT5 = dyT5 / dlugosc;

            double vxT5 = -uyT5;
            double vyT5 = uxT5;

            // ============================================================
            // 4. PUNKT OSI NAJBLIŻSZY KLIKNIĘCIU
            // ============================================================
            double txT5 = mouseClik.X - TopXT5.X;
            double tyT5 = mouseClik.Y - TopXT5.Y;

            double t = (txT5 * uxT5 + tyT5 * uyT5) / dlugosc;
            t = Math.Max(0, Math.Min(1, t));

            tmpTopST5 = new XPoint
            {
                X = TopXT5.X + uxT5 * (t * dlugosc),
                Y = TopXT5.Y + uyT5 * (t * dlugosc)
            };

            // ============================================================
            // 5. SZEROKOŚĆ I POŁOŻENIE OSI SŁUPKA
            // ============================================================
            double szerokoscSlupka = Math.Abs(SzerokoscSlupka ?? 0);

            double odlegloscOsiDoLewej = Math.Abs(OsSymetrii);
            odlegloscOsiDoLewej = Math.Clamp(odlegloscOsiDoLewej, 0, szerokoscSlupka);

            double odlegloscOsiDoPrawej = szerokoscSlupka - odlegloscOsiDoLewej;
            odlegloscOsiDoPrawej = Math.Clamp(odlegloscOsiDoPrawej, 0, szerokoscSlupka);

            // ============================================================
            // 6. PUNKTY LEWY / PRAWY WZGLĘDEM OSI
            // ============================================================
            tmpTopLT5 = new XPoint
            {
                X = tmpTopST5.X - vxT5 * odlegloscOsiDoLewej,
                Y = tmpTopST5.Y - vyT5 * odlegloscOsiDoLewej
            };

            tmpTopRT5 = new XPoint
            {
                X = tmpTopST5.X + vxT5 * odlegloscOsiDoPrawej,
                Y = tmpTopST5.Y + vyT5 * odlegloscOsiDoPrawej
            };

            // ============================================================
            // 7. LOG
            // ============================================================
            foreach (var punkt in punktyRegionuMaster)
            {
                Console.WriteLine($"🔷 T5-T5 punktyRegionuMaster: #2 X={punkt.X}, Y={punkt.Y}");
            }

            if (daneKwadratu != null)
            {
                int idx = 0;
                foreach (var d in daneKwadratu)
                {
                    if (d?.Przesuniecia == null)
                    {
                        Console.WriteLine($"   Przesuniecia: NULL dla daneKwadratu[{idx}]");
                    }
                    else
                    {
                        int pIdx = 0;
                        foreach (var p in d.Przesuniecia)
                        {
                            Console.WriteLine(
                                $"   [{pIdx}] Strona='{p.Strona}', " +
                                $"PrzesuniecieX={p.PrzesuniecieX}, PrzesuniecieY={p.PrzesuniecieY}, " +
                                $"PrzesuniecieXStycznej={p.PrzesuniecieXStycznej}, " +
                                $"PrzesuniecieYStycznej={p.PrzesuniecieYStycznej}, " +
                                $"ElementZewnetrznyId={p.ElementZewnetrznyId}, " +
                                $"ElementWewnetrznyId={p.ElementWewnetrznyId}");
                            pIdx++;
                        }
                    }
                    idx++;
                }
            }

            // ============================================================
            // 8. PRZESUNIĘCIE PUNKTÓW OSI WZGLĘDEM STYCZNYCH
            // ============================================================
            var liniaT5 = daneKwadratu
                .Where(x => x.LiniaStala != null && x.LiniaStala.Count() > 1)
                .LastOrDefault()
                ?.Wierzcholki
                ?.ToList() ?? new List<XPoint>();

            TopXT5 = ApplyOffsetToPointST(TopXT5, liniaT5, daneKwadratu);
            BottomXT5 = ApplyOffsetToPointST(BottomXT5, liniaT5, daneKwadratu);

            // ============================================================
            // 9. FLAGI SĄSIADÓW (Strona A / Strona B)
            // ============================================================
            bool stronaA = daneKwadratu
                .FirstOrDefault(x => x.SasiadSlupekStronaA) != null;

            bool stronaB = daneKwadratu
                .FirstOrDefault(x => x.SasiadSlupekStronaB) != null;

            if (stronaA)
            {
                var sasiadA = daneKwadratu.FirstOrDefault(x => x.SasiadSlupekStronaA);
                if (sasiadA != null)
                    sasiadA.OsuniecieSlupekStronaA = geo.ProfileTop;
            }

            if (stronaB)
            {
                var sasiadB = daneKwadratu.FirstOrDefault(x => x.SasiadSlupekStronaB);
                if (sasiadB != null)
                    sasiadB.OsuniecieSlupekStronaB = geo.ProfileBottom;
            }

            // ============================================================
            // 10. PRZYCIĘCIE KONTURU MASTER
            // ============================================================
            List<XPoint> punktyRegionuMasterModyfikowane;

            if (!stronaA && !stronaB)
            {
                punktyRegionuMasterModyfikowane = punktyRegionuMaster;
            }
            else
            {
                punktyRegionuMasterModyfikowane = PrepareRegionPoints(
                    TopXT5,
                    BottomXT5,
                    punktyRegionuMaster,
                    stronaA,
                    stronaB,
                    geo.ProfileTop,
                    geo.ProfileBottom);
            }

            // ============================================================
            // 11. PRZECIĘCIA Z KONTUREM
            // ============================================================
            XPoint leftTopIntersection = FindFirstEdgeIntersectionByVector(tmpTopLT5, TopXT5, BottomXT5, punktyRegionuMasterModyfikowane, forward: false);
            XPoint midTopIntersection = FindFirstEdgeIntersectionByVector(tmpTopST5, TopXT5, BottomXT5, punktyRegionuMasterModyfikowane, forward: false);
            XPoint rightTopIntersection = FindFirstEdgeIntersectionByVector(tmpTopRT5, TopXT5, BottomXT5, punktyRegionuMasterModyfikowane, forward: false);

            XPoint leftBottomIntersection = FindFirstEdgeIntersectionByVector(tmpTopLT5, TopXT5, BottomXT5, punktyRegionuMasterModyfikowane, forward: true);
            XPoint midBottomIntersection = FindFirstEdgeIntersectionByVector(tmpTopST5, TopXT5, BottomXT5, punktyRegionuMasterModyfikowane, forward: true);
            XPoint rightBottomIntersection = FindFirstEdgeIntersectionByVector(tmpTopRT5, TopXT5, BottomXT5, punktyRegionuMasterModyfikowane, forward: true);

            var TopLT5 = leftTopIntersection;
            var TopST5 = midTopIntersection;
            var TopRT5 = rightTopIntersection;

            var BottomLT5 = leftBottomIntersection;
            var BottomSTT5 = midBottomIntersection;
            var BottomRT5 = rightBottomIntersection;

            // ============================================================
            // 12. CZWOROKĄT SŁUPKA
            // ============================================================
            var wierzcholki = new List<XPoint>
            {
                TopRT5,
                TopLT5,
                BottomLT5,
                BottomRT5
            };

            Console.WriteLine($"🔷 T5-T5 -> czworokąt słupka: {wierzcholki.Count} punktów");

            return new WynikWierzcholkow
            {
                Ok = true,
                Wierzcholki = wierzcholki,
                DodajA = false,
                DodajB = false
            };
        }

        // ============================================================
        // GetStartT1 / GetEndT1 / GetStartT2 / GetEndT2
        // ============================================================
        private List<XPoint> GetStartT1(
            XPoint _innerP, XPoint _outerP, List<XPoint> _outer,
            float angleDegrees, float prevangleDegrees, float nextangleDegrees,
            string stronaWModelu, string stonaOstanioDodanegoElementu, int nk)
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

            if (warunek)
            {
                var startT1 = FindFirstEdgeIntersectionByAngle(_innerP, angleDegrees - 180, _outer);
                intersections.Add(new XPoint(startT1.X, startT1.Y));
                intersections.Add(new XPoint(_outerP.X, _outerP.Y));
            }
            else
            {
                var startT1 = FindFirstEdgeIntersectionByAngle(_innerP, prevangleDegrees, _outer);
                intersections.Add(new XPoint(_innerP.X, _innerP.Y));
                intersections.Add(new XPoint(startT1.X, startT1.Y));
            }

            return intersections;
        }

        private List<XPoint> GetEndT1(
            XPoint _innerP, XPoint _outerP, List<XPoint> _outer,
            float angleDegrees, float prevangleDegrees, float nextangleDegrees,
            string stronaWModelu, string stonaOstanioDodanegoElementu, int nk)
        {
            List<XPoint> intersections = new List<XPoint>();

            bool czyParzysta = (nk + 1) % 2 == 0;
            bool warunek = false;

            if (nk < 0)
            {
                warunek =
                    (stronaWModelu == "Góra" && ElementyRamyRysowane.Count == 0)
                    || (stronaWModelu == "Góra" && _outer.Count() == 4)
                    || stronaWModelu == "Dół"
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
                intersections.Add(new XPoint(startT1.X, startT1.Y));
                intersections.Add(new XPoint(_outerP.X, _outerP.Y));
            }
            else
            {
                var startT1 = FindFirstEdgeIntersectionByAngle(_innerP, nextangleDegrees - 180, _outer);
                intersections.Add(new XPoint(_innerP.X, _innerP.Y));
                intersections.Add(new XPoint(startT1.X, startT1.Y));
            }

            return intersections;
        }

        private List<XPoint> GetStartT2(XPoint _inner, XPoint _outer)
        {
            List<XPoint> intersections = new List<XPoint>();
            intersections.Add(new XPoint(_inner.X, _inner.Y));
            intersections.Add(new XPoint(_outer.X, _outer.Y));
            return intersections;
        }

        private List<XPoint> GetEndT2(XPoint inner, XPoint outer)
        {
            List<XPoint> intersections = new List<XPoint>();
            intersections.Add(new XPoint(inner.X, inner.Y));
            intersections.Add(new XPoint(outer.X, outer.Y));
            return intersections;
        }

        // ============================================================
        // GetStartT3 / GetEndT3
        // ============================================================
        private List<XPoint> GetStartT3(
            XPoint _innerP, XPoint _outerP, List<XPoint> _outer,
            float angleDegrees, float prevangleDegrees, float nextangleDegrees,
            string stronaWModelu, string stonaOstanioDodanegoElementu, int nk)
        {
            List<XPoint> intersections = new List<XPoint>();

            bool czyParzysta = (nk + 1) % 2 == 0;
            bool warunek = false;

            if (nk < 0)
            {
                warunek =
                    (stronaWModelu == "Dół" && stonaOstanioDodanegoElementu != "Góra")
                    || (stronaWModelu == "Góra" && ElementyRamyRysowane.Count == 0)
                    || (stronaWModelu == "Góra" && stonaOstanioDodanegoElementu != "Góra" && stonaOstanioDodanegoElementu != "Dół");
            }
            else if (nk > 0)
            {
                warunek = czyParzysta;
            }

            if (warunek)
            {
                var startT1 = FindFirstEdgeIntersectionByAngle(_innerP, prevangleDegrees, _outer);
                intersections.Add(new XPoint(_innerP.X, _innerP.Y));
                intersections.Add(new XPoint(startT1.X, startT1.Y));
            }
            else
            {
                var startT1 = FindFirstEdgeIntersectionByAngle(_innerP, angleDegrees - 180, _outer);
                intersections.Add(new XPoint(startT1.X, startT1.Y));
                intersections.Add(new XPoint(_outerP.X, _outerP.Y));
            }

            return intersections;
        }

        private List<XPoint> GetEndT3(
            XPoint _innerP, XPoint _outerP, List<XPoint> _outer,
            float angleDegrees, float prevangleDegrees, float nextangleDegrees,
            string stronaWModelu, string stonaOstanioDodanegoElementu, int nk)
        {
            List<XPoint> intersections = new List<XPoint>();

            bool czyParzysta = (nk + 1) % 2 == 0;
            bool warunek = false;

            if (nk < 0)
            {
                warunek =
                    (stronaWModelu == "Góra" && ElementyRamyRysowane.Count == 0)
                    || stronaWModelu == "Dół"
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
                intersections.Add(new XPoint(_innerP.X, _innerP.Y));
                intersections.Add(new XPoint(startT1.X, startT1.Y));
            }
            else
            {
                var startT1 = FindFirstEdgeIntersectionByAngle(_innerP, angleDegrees, _outer);
                intersections.Add(new XPoint(startT1.X, startT1.Y));
                intersections.Add(new XPoint(_outerP.X, _outerP.Y));
            }

            return intersections;
        }

        // ============================================================
        // GetStartT4 / GetEndT4
        // ============================================================
        private List<XPoint> GetStartT4(XPoint _inner)
        {
            List<XPoint> intersections = new List<XPoint>();
            intersections.Add(new XPoint(_inner.X, _inner.Y));
            intersections.Add(new XPoint(_inner.X, _inner.Y));
            return intersections;
        }

        private List<XPoint> GetEndT4(XPoint _inner)
        {
            List<XPoint> intersections = new List<XPoint>();
            intersections.Add(new XPoint(_inner.X, _inner.Y));
            intersections.Add(new XPoint(_inner.X, _inner.Y));
            return intersections;
        }

        // ============================================================
        // GetStartT1Triangle / GetEndT1Triangle
        // ============================================================
        private List<XPoint> GetStartT1Triangle(
            XPoint _innerP, XPoint _outerP, List<XPoint> _outer,
            float angleDegrees, float prevangleDegrees, float nextangleDegrees,
            string stronaWModelu, string stonaOstanioDodanegoElementu,
            int nk, int next, int prev)
        {
            List<XPoint> intersections = new List<XPoint>();

            bool czyParzysta = (nk + 1) % 2 == 0;

            string prevSide = StronaOknaHelper.OkreslStrone(prevangleDegrees, prev, _outer);
            string nextSide = StronaOknaHelper.OkreslStrone(nextangleDegrees, next, _outer);

            bool poziomy = stronaWModelu == "Góra" || stronaWModelu == "Dół";

            if (stronaWModelu == "Prawa" && prevSide == "Lewa") { poziomy = false; czyParzysta = false; }
            if (stronaWModelu == "Prawa" && prevSide == "Góra") { poziomy = false; czyParzysta = false; }
            if ((nextSide == "Góra" || prevSide == "Góra") && stronaWModelu == "Dół") { poziomy = true; czyParzysta = false; }
            if ((nextSide == "Dół" || prevSide == "Dół") && stronaWModelu == "Góra") { poziomy = false; czyParzysta = false; }

            if (poziomy)
            {
                XPoint startT1 = FindTriangleEdgeIntersectionByAngle(_innerP, angleDegrees - 180.0, _outer);
                intersections.Add(new XPoint(startT1.X, startT1.Y));
                intersections.Add(new XPoint(_outerP.X, _outerP.Y));
            }
            else
            {
                if (czyParzysta)
                {
                    XPoint startT1 = FindTriangleEdgeIntersectionByAngle(_innerP, angleDegrees - 180.0, _outer);
                    intersections.Add(new XPoint(startT1.X, startT1.Y));
                    intersections.Add(new XPoint(_outerP.X, _outerP.Y));
                }
                else
                {
                    XPoint startT1 = FindTriangleEdgeIntersectionByAngle(_innerP, prevangleDegrees, _outer);
                    intersections.Add(new XPoint(_innerP.X, _innerP.Y));
                    intersections.Add(new XPoint(startT1.X, startT1.Y));
                }
            }

            return intersections;
        }

        private List<XPoint> GetEndT1Triangle(
            XPoint _innerP, XPoint _outerP, List<XPoint> _outer,
            float angleDegrees, float prevangleDegrees, float nextangleDegrees,
            string stronaWModelu, string stonaOstanioDodanegoElementu,
            int nk, int next, int prev)
        {
            List<XPoint> intersections = new List<XPoint>();

            string nextSide = StronaOknaHelper.OkreslStrone(nextangleDegrees, next, _outer);
            string prevSide = StronaOknaHelper.OkreslStrone(prevangleDegrees, prev, _outer);

            bool czyParzysta = (nk + 1) % 2 == 0;
            czyParzysta = false;

            bool poziome = stronaWModelu == "Góra" || stronaWModelu == "Dół";

            if (stronaWModelu == "Góra" && nextSide == "Dół") { poziome = true; czyParzysta = false; }
            if (stronaWModelu == "Lewa" && nextSide == "Prawa" && nextangleDegrees < 180.0) poziome = true;
            if (stronaWModelu == "Lewa" && nextSide == "Góra") { poziome = false; czyParzysta = true; }
            if (stronaWModelu == "Góra" && nextSide == "Prawa") { poziome = false; czyParzysta = true; }
            if (stronaWModelu == "Góra" && prevSide == "Dół") { poziome = true; czyParzysta = false; }

            if (poziome)
            {
                XPoint endT1 = FindTriangleEdgeIntersectionByAngle(_innerP, angleDegrees, _outer);
                intersections.Add(new XPoint(endT1.X, endT1.Y));
                intersections.Add(new XPoint(_outerP.X, _outerP.Y));
            }
            else
            {
                if (czyParzysta)
                {
                    XPoint endT1 = FindTriangleEdgeIntersectionByAngle(_innerP, angleDegrees, _outer);
                    intersections.Add(new XPoint(endT1.X, endT1.Y));
                    intersections.Add(new XPoint(_outerP.X, _outerP.Y));
                }
                else
                {
                    XPoint endT1 = FindTriangleEdgeIntersectionByAngle(_innerP, nextangleDegrees - 180.0, _outer);
                    intersections.Add(new XPoint(_innerP.X, _innerP.Y));
                    intersections.Add(new XPoint(endT1.X, endT1.Y));
                }
            }

            return intersections;
        }

        // ============================================================
        // GetStartT3Triangle / GetEndT3Triangle
        // ============================================================
        private List<XPoint> GetStartT3Triangle(
            XPoint _innerP, XPoint _outerP, List<XPoint> _outer,
            float angleDegrees, float prevangleDegrees, float nextangleDegrees,
            string stronaWModelu, string stonaOstanioDodanegoElementu,
            int nk, int next, int prev)
        {
            List<XPoint> intersections = new List<XPoint>();

            bool czyParzysta = (nk + 1) % 2 == 0;

            string prevSide = StronaOknaHelper.OkreslStrone(prevangleDegrees, prev, _outer);
            string nextSide = StronaOknaHelper.OkreslStrone(nextangleDegrees, next, _outer);

            bool pionowe = stronaWModelu == "Lewa" || stronaWModelu == "Prawa";

            if (stronaWModelu == "Góra" && prevSide == "Lewa") { pionowe = false; czyParzysta = false; }
            if (stronaWModelu == "Dół" && prevSide == "Lewa") { pionowe = false; czyParzysta = false; }
            if (stronaWModelu == "Góra" && prevSide == "Dół") { pionowe = false; czyParzysta = true; }
            if (stronaWModelu == "Góra" && nextSide == "Góra") { czyParzysta = true; }

            if (pionowe)
            {
                XPoint startT3 = FindTriangleEdgeIntersectionByAngle(_innerP, angleDegrees - 180.0, _outer);
                intersections.Add(new XPoint(startT3.X, startT3.Y));
                intersections.Add(new XPoint(_outerP.X, _outerP.Y));
                return intersections;
            }

            if (czyParzysta)
            {
                XPoint startT3 = FindTriangleEdgeIntersectionByAngle(_innerP, angleDegrees - 180.0, _outer);
                intersections.Add(new XPoint(startT3.X, startT3.Y));
                intersections.Add(new XPoint(_outerP.X, _outerP.Y));
            }
            else
            {
                XPoint startT3 = FindTriangleEdgeIntersectionByAngle(_innerP, prevangleDegrees, _outer);
                intersections.Add(new XPoint(_innerP.X, _innerP.Y));
                intersections.Add(new XPoint(startT3.X, startT3.Y));
            }

            return intersections;
        }

        private List<XPoint> GetEndT3Triangle(
            XPoint _innerP, XPoint _outerP, List<XPoint> _outer,
            float angleDegrees, float prevangleDegrees, float nextangleDegrees,
            string stronaWModelu, string stonaOstanioDodanegoElementu,
            int nk, int next, int prev)
        {
            List<XPoint> intersections = new List<XPoint>();

            string nextSide = StronaOknaHelper.OkreslStrone(nextangleDegrees, next, _outer);

            bool czyParzysta = (nk + 1) % 2 == 0;
            czyParzysta = false;

            bool pionowe = stronaWModelu == "Lewa" || stronaWModelu == "Prawa";

            if (stronaWModelu == "Góra" && nextSide == "Dół") { pionowe = false; czyParzysta = true; }
            if (stronaWModelu == "Lewa" && nextSide == "Prawa" && nextangleDegrees < 180.0) pionowe = true;

            if (pionowe)
            {
                XPoint endT3 = FindTriangleEdgeIntersectionByAngle(_innerP, angleDegrees, _outer);
                intersections.Add(new XPoint(endT3.X, endT3.Y));
                intersections.Add(new XPoint(_outerP.X, _outerP.Y));
                return intersections;
            }

            if (czyParzysta)
            {
                XPoint endT3 = FindTriangleEdgeIntersectionByAngle(_innerP, angleDegrees, _outer);
                intersections.Add(new XPoint(endT3.X, endT3.Y));
                intersections.Add(new XPoint(_outerP.X, _outerP.Y));
            }
            else
            {
                XPoint endT3 = FindTriangleEdgeIntersectionByAngle(_innerP, nextangleDegrees - 180.0, _outer);
                intersections.Add(new XPoint(_innerP.X, _innerP.Y));
                intersections.Add(new XPoint(endT3.X, endT3.Y));
            }

            return intersections;
        }

        // ============================================================
        // FindTriangleEdgeIntersectionByAngle
        // ============================================================
        private XPoint FindTriangleEdgeIntersectionByAngle(
            XPoint start, double angleDegrees, List<XPoint> outer)
        {
            if (outer == null || outer.Count < 3)
            {
                Console.WriteLine("⚠️ FindTriangleEdgeIntersectionByAngle: kontur ma <3 punktów.");
                return start;
            }

            double angleRad = angleDegrees * Math.PI / 180.0;
            double dx = Math.Cos(angleRad);
            double dy = Math.Sin(angleRad);

            double bestT = double.MaxValue;
            XPoint bestPoint = start;

            for (int i = 0; i < outer.Count; i++)
            {
                XPoint a = outer[i];
                XPoint b = outer[(i + 1) % outer.Count];

                double sx = b.X - a.X;
                double sy = b.Y - a.Y;

                double denominator = dx * sy - dy * sx;
                if (Math.Abs(denominator) < 0.000001) continue;

                double ax = a.X - start.X;
                double ay = a.Y - start.Y;

                double t = (ax * sy - ay * sx) / denominator;
                double u = (ax * dy - ay * dx) / denominator;

                if (t <= 0.000001) continue;
                if (u < -0.000001 || u > 1.000001) continue;

                if (t < bestT)
                {
                    bestT = t;
                    bestPoint = new XPoint(start.X + t * dx, start.Y + t * dy);
                }
            }

            return bestT == double.MaxValue ? start : bestPoint;
        }

        // ============================================================
        // FindFirstEdgeIntersection / FindFirstEdgeIntersectionByAngle
        // ============================================================
        private XPoint FindFirstEdgeIntersection(XPoint origin, float dx, float dy, List<XPoint> contour)
        {
            XPoint? closest = null;
            float minDist = float.MaxValue;

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

        private XPoint FindFirstEdgeIntersectionByAngle(XPoint origin, float angleDegrees, List<XPoint> contour)
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
                    contour[i], contour[next]);

                if (!inter.HasValue) continue;

                var p = inter.Value;
                double dot = (p.X - origin.X) * dx + (p.Y - origin.Y) * dy;
                if (dot <= 0) continue;

                double distSq = (p.X - origin.X) * (p.X - origin.X) + (p.Y - origin.Y) * (p.Y - origin.Y);
                if (distSq < minDistSq)
                {
                    minDistSq = (float)distSq;
                    closest = p;
                }
            }

            return closest ?? origin;
        }

        // ============================================================
        // FindFirstEdgeIntersectionByVector
        // ============================================================
        private XPoint FindFirstEdgeIntersectionByVector(
            XPoint basePoint, XPoint dirStart, XPoint dirEnd,
            List<XPoint> polygon, bool forward = true, double tolerance = 0.01)
        {
            double dx = dirEnd.X - dirStart.X;
            double dy = dirEnd.Y - dirStart.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);

            if (len < 1e-8)
            {
                dx = 0;
                dy = forward ? 1 : -1;
            }
            else
            {
                dx /= len;
                dy /= len;
                if (!forward) { dx = -dx; dy = -dy; }
            }

            XPoint? closest = null;
            double minDistSq = double.MaxValue;

            for (int i = 0; i < polygon.Count; i++)
            {
                int next = (i + 1) % polygon.Count;

                var inter = GetLinesIntersectionNullable(
                    basePoint,
                    new XPoint(basePoint.X + dx * 10000.0, basePoint.Y + dy * 10000.0),
                    polygon[i], polygon[next]);

                if (!inter.HasValue)
                {
                    foreach (var pt in new[] { polygon[i], polygon[next] })
                    {
                        double cross = Math.Abs((pt.X - basePoint.X) * dy - (pt.Y - basePoint.Y) * dx);
                        if (cross > tolerance) continue;

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

                double dotInter = (p.X - basePoint.X) * dx + (p.Y - basePoint.Y) * dy;
                if (dotInter < -tolerance) continue;

                double distSqInter = (p.X - basePoint.X) * (p.X - basePoint.X) +
                                     (p.Y - basePoint.Y) * (p.Y - basePoint.Y);

                if (distSqInter < tolerance) continue;

                if (distSqInter < minDistSq)
                {
                    minDistSq = distSqInter;
                    closest = p;
                }
            }

            if (!closest.HasValue)
                return new XPoint { X = -1, Y = -1 };

            return closest.Value;
        }

        // ============================================================
        // FindIntersectionByAngleWithSegments / GetRayCircleIntersections
        // ============================================================
        private List<XPoint> GetRayCircleIntersections(XPoint origin, double dx, double dy, XPoint center, double radius)
        {
            var result = new List<XPoint>();
            double fx = origin.X - center.X;
            double fy = origin.Y - center.Y;
            double a = dx * dx + dy * dy;
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

        private (XPoint intersection, ContourSegment? segment) FindIntersectionByAngleWithSegments(
            XPoint origin, double angleDegrees, List<ContourSegment> contour, double maxDistance = 10000.0)
        {
            if (contour == null || contour.Count == 0)
                return (origin, null);

            double angleRad = angleDegrees * Math.PI / 180.0;
            double dx = Math.Cos(angleRad);
            double dy = Math.Sin(angleRad);

            XPoint? closestPoint = null;
            ContourSegment? closestSegment = null;
            double minDistSq = double.MaxValue;

            XPoint endPoint = new XPoint(origin.X + dx * maxDistance, origin.Y + dy * maxDistance);

            foreach (var seg in contour)
            {
                if (seg.Type == SegmentType.Line)
                {
                    var inter = GetLinesIntersectionNullable(origin, endPoint, seg.Start, seg.End);
                    if (!inter.HasValue) continue;

                    var p = inter.Value;
                    double dot = (p.X - origin.X) * dx + (p.Y - origin.Y) * dy;
                    if (dot <= 0) continue;

                    double distSq = (p.X - origin.X) * (p.X - origin.X) + (p.Y - origin.Y) * (p.Y - origin.Y);
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        closestPoint = p;
                        closestSegment = null;
                    }
                }
                else if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    var intersections = GetRayCircleIntersections(origin, dx, dy, seg.Center.Value, seg.Radius);
                    foreach (var p in intersections)
                    {
                        if (!IsPointOnArc(p, seg, 0.1)) continue;

                        double dot = (p.X - origin.X) * dx + (p.Y - origin.Y) * dy;
                        if (dot <= 0) continue;

                        double distSq = (p.X - origin.X) * (p.X - origin.X) + (p.Y - origin.Y) * (p.Y - origin.Y);
                        if (distSq < minDistSq)
                        {
                            minDistSq = distSq;
                            closestPoint = p;
                            closestSegment = seg;
                        }
                    }
                }
            }

            return (closestPoint ?? origin, closestSegment);
        }

        // ============================================================
        // GetHorizontalIntersection
        // ============================================================
        private XPoint GetHorizontalIntersection(XPoint a, XPoint b, float y)
        {
            if (Math.Abs(a.Y - b.Y) < 1e-3f)
                return new XPoint(a.X, y);

            float t = (y - (float)a.Y) / ((float)b.Y - (float)a.Y);
            float x = (float)a.X + t * ((float)b.X - (float)a.X);
            return new XPoint(x, y);
        }

        // ============================================================
        // FindIntersectionWithContourByAngle / FindClosestPointOnContour / ProjectPointOnSegment
        // ============================================================
        private XPoint FindIntersectionWithContourByAngle(
            XPoint point, double angleDegrees, List<ContourSegment> contour,
            bool goForward = true, double maxDistance = 1000.0)
        {
            if (contour == null || contour.Count == 0)
                return point;

            double angleRad = angleDegrees * Math.PI / 180.0;
            double dx = Math.Cos(angleRad);
            double dy = Math.Sin(angleRad);

            if (!goForward) { dx = -dx; dy = -dy; }

            XPoint extendedPoint = new XPoint(point.X + dx * maxDistance, point.Y + dy * maxDistance);

            XPoint intersection = FindIntersectionWithContourT1i3(point, extendedPoint, contour);

            double dot = (intersection.X - point.X) * dx + (intersection.Y - point.Y) * dy;
            if (dot > 0.1 && Distance(point, intersection) > 0.1 && Distance(point, intersection) < maxDistance)
                return intersection;

            return FindClosestPointOnContour(point, contour);
        }

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
                    double dx = point.X - seg.Center.Value.X;
                    double dy = point.Y - seg.Center.Value.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist > 0.01)
                    {
                        XPoint candidate = new XPoint(
                            seg.Center.Value.X + seg.Radius * (dx / dist),
                            seg.Center.Value.Y + seg.Radius * (dy / dist));
                        double d = Distance(point, candidate);
                        if (d < minDist) { minDist = d; bestPoint = candidate; }
                    }
                }
                else if (seg.Type == SegmentType.Line)
                {
                    XPoint candidate = ProjectPointOnSegment(point, seg.Start, seg.End);
                    double d = Distance(point, candidate);
                    if (d < minDist) { minDist = d; bestPoint = candidate; }
                }
            }

            return bestPoint;
        }

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

        // ============================================================
        // Build4SegmentContour
        // ============================================================
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
            int sourceIndex = numerElemntu - 1;

            // W czasie zmiany wymiarów kontury są przebudowywane etapami. Nie
            // wykonuj modulo przez Count == 0 ani nie indeksuj pustego konturu.
            // Dla takiego przejściowego stanu zwróć poprawny prosty czworokąt;
            // łuki zostaną odtworzone w kolejnym pełnym przeliczeniu.
            if (wierzcholki == null || wierzcholki.Count < 4)
            {
                BledySystemowe.Add(
                    $"⚠️ Build4SegmentContour: za mało wierzchołków dla elementu {numerElemntu} " +
                    $"({wierzcholki?.Count ?? 0}).");
                return new List<ContourSegment>();
            }

            if (outerContour == null || innerContour == null ||
                outerContour.Count == 0 || innerContour.Count == 0 ||
                sourceIndex < 0 || sourceIndex >= outerContour.Count || sourceIndex >= innerContour.Count)
            {
                BledySystemowe.Add(
                    $"⚠️ Build4SegmentContour: niekompletny kontur dla elementu {numerElemntu} " +
                    $"(outer={outerContour?.Count ?? 0}, inner={innerContour?.Count ?? 0}, index={sourceIndex}). " +
                    "Zastosowano kontur liniowy do czasu ponownego przeliczenia geometrii.");

                return BuildLinearFourSegmentContour(wierzcholki);
            }

            var filteredOuter = GetSegmentsForSide(outerContour, _stronaElementu);
            var filteredInner = GetSegmentsForSide(innerContour, _stronaElementu);

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

                bool rightJoinIsT1orT3 = (rightJoin == "T1" || rightJoin == "T3");
                bool leftJoinIsT1orT3 = (leftJoin == "T1" || leftJoin == "T3");

                // 1a. Pionowe boki (Lewa / Prawa)
                if (_stronaElementu == "Lewa" && rightJoinIsT1orT3)
                {
                    var adjustedVerticesX = new List<XPoint>(wierzcholki);

                    if (rightJoin == "T1" && outerContour.Count() > 3)
                        adjustedVerticesX[2] = FindIntersectionWithContourByAngle(adjustedVerticesX[2], angleDegrees, innerContour);

                    if (rightJoin == "T3" && outerContour.Count() > 3)
                        adjustedVerticesX[2] = FindIntersectionWithContourByAngle(adjustedVerticesX[2], angleDegrees, outerContour);

                    var segZewnetrznyX = BuildSegmentWithArc(adjustedVerticesX[0], adjustedVerticesX[1], filteredOuter);
                    var segWewnetrznyX = BuildSegmentWithArc(adjustedVerticesX[2], adjustedVerticesX[3], filteredInner);

                    if (segWewnetrznyX.Type == SegmentType.Arc && segWewnetrznyX.Center.HasValue)
                    {
                        segWewnetrznyX = new ContourSegment(
                            segWewnetrznyX.End, segWewnetrznyX.Start,
                            segWewnetrznyX.Center, segWewnetrznyX.Radius,
                            !segWewnetrznyX.CounterClockwise);
                    }

                    List<ContourSegment> contourForSide = (rightJoin == "T3") ? filteredOuter : filteredInner;

                    var segBoczny1 = BuildSegmentWithArc(adjustedVerticesX[1], adjustedVerticesX[2], contourForSide);
                    var segBoczny2 = BuildSegmentWithArc(adjustedVerticesX[3], adjustedVerticesX[0], contourForSide);

                    return new List<ContourSegment>
                    {
                        segZewnetrznyX, segBoczny1, segWewnetrznyX, segBoczny2
                    };
                }

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
                            segWewnetrznyX.End, segWewnetrznyX.Start,
                            segWewnetrznyX.Center, segWewnetrznyX.Radius,
                            !segWewnetrznyX.CounterClockwise);
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

                        return new List<ContourSegment>
                        {
                            segZewnetrznyX, segBoczny1, segWewnetrznyX, segBoczny2
                        };
                    }
                }

                // 1b. Górne boki – gdy outerSegment i innerSegment są łukami
                if (outerSegment.Type == SegmentType.Arc && outerSegment.Center.HasValue &&
                    innerSegment.Type == SegmentType.Arc && innerSegment.Center.HasValue)
                {
                    bool leftT1Bevel = _stronaElementu == "Góra" && leftJoin == "T1" && outerContour[previousIndex].Type == SegmentType.Line;
                    bool rightT1Bevel = _stronaElementu == "Góra" && rightJoin == "T1" && outerContour[nextIndex].Type == SegmentType.Line;
                    bool leftT2Bevel = _stronaElementu == "Góra" && leftJoin == "T2" && outerContour[previousIndex].Type == SegmentType.Line;
                    bool rightT2Bevel = _stronaElementu == "Góra" && rightJoin == "T2" && outerContour[nextIndex].Type == SegmentType.Line;
                    bool leftT3Bevel = _stronaElementu == "Góra" && leftJoin == "T3" && outerContour[previousIndex].Type == SegmentType.Line;
                    bool rightT3Bevel = _stronaElementu == "Góra" && rightJoin == "T3" && outerContour[nextIndex].Type == SegmentType.Line;

                    var result = new List<ContourSegment>();

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
                    }
                    else if (rightT2Bevel)
                    {
                        result = new List<ContourSegment>
                        {
                            new ContourSegment(outerSegment.Start, outerSegment.End,
                                outerSegment.Center, outerSegment.Radius, false)
                        };

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
                    }
                    else if (rightT3Bevel)
                    {
                        var (pointOnOuter, _) = FindIntersectionByAngleWithSegments(
                            innerSegment.End, nextangleDegrees - 180, outerContour, 1000.0);

                        var outerArc = BuildSegmentWithArc(outerSegment.Start, pointOnOuter, outerContour);
                        var rightSide = BuildSegmentWithArc(pointOnOuter, innerSegment.End, outerContour);
                        var innerArc = BuildSegmentWithArc(innerSegment.End, innerSegment.Start, innerContour);
                        var leftSide = BuildSegmentWithArc(innerSegment.Start, outerSegment.Start, innerContour);

                        return new List<ContourSegment>
                        {
                            outerArc, rightSide, innerArc, leftSide
                        };
                    }

                    if (leftT3Bevel)
                    {
                        var (pointOnOuter, _) = FindIntersectionByAngleWithSegments(
                            innerSegment.Start, prevangleDegrees, outerContour, 1000.0);

                        var outerArc = BuildSegmentWithArc(pointOnOuter, outerSegment.End, outerContour);
                        var rightSide = BuildSegmentWithArc(outerSegment.End, innerSegment.End, outerContour);
                        var innerArc = BuildSegmentWithArc(innerSegment.End, innerSegment.Start, innerContour);
                        var leftSide = BuildSegmentWithArc(innerSegment.Start, pointOnOuter, outerContour);

                        return new List<ContourSegment>
                        {
                            outerArc, rightSide, innerArc, leftSide
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
                    }

                    result.Add(new ContourSegment(innerSegment.End, innerSegment.Start,
                        innerSegment.Center, innerSegment.Radius, true));

                    if (leftT1Bevel)
                    {
                        XPoint bevel = GetT1BevelPoint(outerSegment.Start, innerSegment.Start, innerSegment, outerContour);
                        result.Add(new ContourSegment(innerSegment.Start, bevel));
                        result.Add(new ContourSegment(bevel, outerSegment.Start));
                    }
                    else if (leftT2Bevel)
                    {
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
                    }
                    else if (leftT3Bevel)
                    {
                        var (pointOnOuter, segment) = FindIntersectionWithOuterContour(
                            outerSegment.Start, innerSegment.Start, outerContour, true);

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
                    }
                    else
                    {
                        result.Add(new ContourSegment(innerSegment.Start, outerSegment.Start));
                    }

                    return result;
                }
            }

            // ============================================================
            // PRZYPADEK 2: Standardowa ścieżka dla linii
            // ============================================================
            var adjustedVertices = new List<XPoint>(wierzcholki);

            if (CzyObaKonceWspolnyFragmentTylkoLinie(adjustedVertices, outerContour, innerContour, sourceIndex))
            {
                Console.WriteLine($"🟢 Build4SegmentContour ELEMENT {numerElemntu}: linie.");
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

            // PRZYPADEK 2.4: T5 — słupek stały
            bool isT5 = leftJoin == "T5" || rightJoin == "T5";
            if (isT5 && adjustedVertices.Count == 4)
            {
                var contoursForT5 = new List<ContourSegment>(outerContour.Count + innerContour.Count);
                contoursForT5.AddRange(outerContour);
                contoursForT5.AddRange(innerContour);

                var firstEnd = BuildSegmentWithArc(adjustedVertices[0], adjustedVertices[1], contoursForT5);
                var secondEnd = BuildSegmentWithArc(adjustedVertices[2], adjustedVertices[3], contoursForT5);

                return new List<ContourSegment>
                {
                    firstEnd,
                    new ContourSegment(adjustedVertices[1], adjustedVertices[2]),
                    secondEnd,
                    new ContourSegment(adjustedVertices[3], adjustedVertices[0])
                };
            }

            // PRZYPADEK 2.5: T3 dla skrzydła
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
                        result.Add(new ContourSegment(intersection, adjustedVertices[2],
                            segment.Center, segment.Radius, segment.CounterClockwise));
                    else
                        result.Add(new ContourSegment(adjustedVertices[1], adjustedVertices[2]));
                }
                else if (rightJoin == "T3" && _stronaElementu == "Lewa")
                {
                    var (intersection, segment) = FindIntersectionWithOuterContour(
                        adjustedVertices[3], adjustedVertices[2], outerContour, true);

                    if (segment != null)
                        result.Add(new ContourSegment(adjustedVertices[2], intersection,
                            segment.Center, segment.Radius, segment.CounterClockwise));
                    else
                        result.Add(new ContourSegment(adjustedVertices[1], adjustedVertices[2]));
                }
                else
                {
                    result.Add(new ContourSegment(adjustedVertices[1], adjustedVertices[2]));
                }

                result.Add(new ContourSegment(adjustedVertices[2], adjustedVertices[3]));
                result.Add(new ContourSegment(adjustedVertices[3], adjustedVertices[0]));

                return result;
            }

            // PRZYPADEK 3: T1 z łukiem
            bool isBottomElement = _stronaElementu == "Dół";

            bool t1AfterArc = !isBottomElement && sourceIndex >= 0 &&
                outerContour != null && sourceIndex < outerContour.Count && outerContour.Count > 0 &&
                leftJoin == "T1" && outerContour[(sourceIndex - 1 + outerContour.Count) % outerContour.Count].Type == SegmentType.Arc;

            bool t1BeforeArc = !isBottomElement && sourceIndex >= 0 &&
                outerContour != null && sourceIndex < outerContour.Count && outerContour.Count > 0 &&
                rightJoin == "T1" && outerContour[(sourceIndex + 1) % outerContour.Count].Type == SegmentType.Arc;

            if (t1AfterArc)
            {
                var outerArcSegment = outerContour[previousIndex];
                XPoint bevel = GetT1BevelPoint(adjustedVertices[2], adjustedVertices[1], outerArcSegment, outerContour);

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

                return new List<ContourSegment>
                {
                    segZewnetrzny,
                    new ContourSegment(adjustedVertices[1], bevel),
                    new ContourSegment(bevel, adjustedVertices[2]),
                    segWewnetrzny,
                    new ContourSegment(adjustedVertices[3], adjustedVertices[0])
                };
            }

            // PRZYPADEK 4: Standardowy kontur 4-segmentowy
            return new List<ContourSegment>
            {
                segZewnetrzny,
                new ContourSegment(adjustedVertices[1], adjustedVertices[2]),
                segWewnetrzny,
                new ContourSegment(adjustedVertices[3], adjustedVertices[0])
            };
        }

        private static List<ContourSegment> BuildLinearFourSegmentContour(IReadOnlyList<XPoint> vertices)
        {
            return new List<ContourSegment>
            {
                new(vertices[0], vertices[1]),
                new(vertices[1], vertices[2]),
                new(vertices[2], vertices[3]),
                new(vertices[3], vertices[0])
            };
        }

        // ============================================================
        // CzyObaKonceWspolnyFragmentTylkoLinie
        // ============================================================
        private static bool CzyObaKonceWspolnyFragmentTylkoLinie(
            List<XPoint> wierzcholki,
            List<ContourSegment> outerContour,
            List<ContourSegment> innerContour,
            int sourceIndex)
        {
            if (wierzcholki == null || wierzcholki.Count < 4) return false;
            if (outerContour == null || innerContour == null) return false;
            if (sourceIndex < 0 || sourceIndex >= outerContour.Count || sourceIndex >= innerContour.Count) return false;

            int previousIndex = (sourceIndex - 1 + outerContour.Count) % outerContour.Count;
            int nextIndex = (sourceIndex + 1) % outerContour.Count;

            return outerContour[previousIndex].Type == SegmentType.Line &&
                   outerContour[nextIndex].Type == SegmentType.Line &&
                   innerContour[previousIndex].Type == SegmentType.Line &&
                   innerContour[nextIndex].Type == SegmentType.Line;
        }

        // ============================================================
        // ReplaceNearestPoint
        // ============================================================
        private static void ReplaceNearestPoint(List<XPoint> points, XPoint replacement)
        {
            if (points == null || points.Count == 0) return;

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

        // ============================================================
        // GetT1BevelPoint
        // ============================================================
        private static XPoint GetT1BevelPoint(XPoint outerPoint, XPoint innerPoint,
            ContourSegment innerSegment, List<ContourSegment> outerContour)
        {
            if (innerSegment.Type == SegmentType.Arc && innerSegment.Center != null)
            {
                double dx = innerPoint.X - innerSegment.Center.Value.X;
                double dy = innerPoint.Y - innerSegment.Center.Value.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);

                if (length > 0.001)
                {
                    double tx, ty;
                    if (innerSegment.CounterClockwise) { tx = -dy / length; ty = dx / length; }
                    else { tx = dy / length; ty = -dx / length; }

                    XPoint tangentPlus = new XPoint(innerPoint.X + tx * 100.0, innerPoint.Y + ty * 100.0);
                    XPoint tangentMinus = new XPoint(innerPoint.X - tx * 100.0, innerPoint.Y - ty * 100.0);

                    double distPlus = Distance(outerPoint, tangentPlus);
                    double distMinus = Distance(outerPoint, tangentMinus);

                    double finalTx = (distPlus < distMinus) ? tx : -tx;
                    double finalTy = (distPlus < distMinus) ? ty : -ty;

                    XPoint intersection = FindIntersectionWithContourT1i3(
                        innerPoint,
                        new XPoint(innerPoint.X + finalTx * 1000.0, innerPoint.Y + finalTy * 1000.0),
                        outerContour);

                    double distToIntersection = Distance(innerPoint, intersection);
                    double distToOuter = Distance(innerPoint, outerPoint);

                    if (distToIntersection > 1.0 && distToIntersection < distToOuter * 2.0)
                        return intersection;

                    double bevelLength = Math.Max(Distance(innerPoint, outerPoint) * 0.3, 10.0);
                    return new XPoint(innerPoint.X + finalTx * bevelLength, innerPoint.Y + finalTy * bevelLength);
                }
            }
            else if (innerSegment.Type == SegmentType.Line)
            {
                double dx = innerSegment.End.X - innerSegment.Start.X;
                double dy = innerSegment.End.Y - innerSegment.Start.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);

                if (length > 0.001)
                {
                    double tx = dx / length;
                    double ty = dy / length;

                    XPoint forward = new XPoint(innerPoint.X + tx * 100.0, innerPoint.Y + ty * 100.0);
                    XPoint backward = new XPoint(innerPoint.X - tx * 100.0, innerPoint.Y - ty * 100.0);

                    double distForward = Distance(outerPoint, forward);
                    double distBackward = Distance(outerPoint, backward);

                    double finalTx = (distForward < distBackward) ? tx : -tx;
                    double finalTy = (distForward < distBackward) ? ty : -ty;

                    XPoint intersection = FindIntersectionWithContourT1i3(
                        innerPoint,
                        new XPoint(innerPoint.X + finalTx * 1000.0, innerPoint.Y + finalTy * 1000.0),
                        outerContour);

                    double distToIntersection = Distance(innerPoint, intersection);
                    double distToOuter = Distance(innerPoint, outerPoint);

                    if (distToIntersection > 1.0 && distToIntersection < distToOuter * 2.0)
                        return intersection;

                    double bevelLength = Math.Max(Distance(innerPoint, outerPoint) * 0.3, 10.0);
                    return new XPoint(innerPoint.X + finalTx * bevelLength, innerPoint.Y + finalTy * bevelLength);
                }
            }

            return new XPoint(outerPoint.X, outerPoint.Y + (innerPoint.Y - outerPoint.Y) * 0.5);
        }

        // ============================================================
        // GetT3BevelPoint
        // ============================================================
        private static XPoint GetT3BevelPoint(XPoint start, XPoint end,
            ContourSegment innerSegment, List<ContourSegment> outerContour)
        {
            if (outerContour == null || outerContour.Count == 0)
                return new XPoint(start.X + (end.X - start.X) * 0.5, end.Y);

            double tx, ty;

            if (innerSegment.Type == SegmentType.Arc && innerSegment.Center != null)
            {
                double dx = start.X - innerSegment.Center.Value.X;
                double dy = start.Y - innerSegment.Center.Value.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);

                if (length > 0.001)
                {
                    if (innerSegment.CounterClockwise) { tx = -dy / length; ty = dx / length; }
                    else { tx = dy / length; ty = -dx / length; }

                    double dot = tx * (end.X - start.X) + ty * (end.Y - start.Y);
                    if (dot < 0) { tx = -tx; ty = -ty; }
                }
                else
                {
                    double dx2 = end.X - start.X;
                    double dy2 = end.Y - start.Y;
                    double length2 = Math.Sqrt(dx2 * dx2 + dy2 * dy2);
                    if (length2 > 0.001) { tx = dx2 / length2; ty = dy2 / length2; }
                    else { tx = 0; ty = -1; }
                }
            }
            else
            {
                double dx = end.X - start.X;
                double dy = end.Y - start.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);

                if (length > 0.001) { tx = dx / length; ty = dy / length; }
                else { tx = 0; ty = -1; }
            }

            double extensionLength = 500.0;
            XPoint extendedPoint = new XPoint(start.X + tx * extensionLength, start.Y + ty * extensionLength);

            XPoint intersection = FindIntersectionWithContourT1i3(start, extendedPoint, outerContour);

            double distToIntersection = Distance(start, intersection);

            if (distToIntersection > 0.1 && distToIntersection < extensionLength)
                return intersection;

            double fallbackLength = Distance(start, end) * 1.5;
            return new XPoint(start.X + tx * fallbackLength, start.Y + ty * fallbackLength);
        }

        // ============================================================
        // FindIntersectionWithOuterContour
        // ============================================================
        private static (XPoint intersectionPoint, ContourSegment? segment) FindIntersectionWithOuterContour(
            XPoint point1, XPoint point2, List<ContourSegment> outerContour, bool goUp)
        {
            if (outerContour == null || outerContour.Count == 0)
                return (point2, null);

            double dx = point2.X - point1.X;
            double dy = point2.Y - point1.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);

            if (length < 0.001) return (point2, null);

            double tx = dx / length;
            double ty = dy / length;

            double extensionLength = 1000.0;
            XPoint extendedPoint = new XPoint(point1.X + tx * extensionLength, point1.Y + ty * extensionLength);

            XPoint bestPoint = point2;
            ContourSegment? bestSegment = null;
            double minDistance = double.MaxValue;

            for (int idx = 0; idx < outerContour.Count; idx++)
            {
                var seg = outerContour[idx];

                if (seg.Type == SegmentType.Line)
                {
                    var intersection = GetLinesIntersectionNullable(point1, extendedPoint, seg.Start, seg.End);

                    if (intersection.HasValue)
                    {
                        double dist = Distance(point1, intersection.Value);
                        double dot = (intersection.Value.X - point1.X) * tx + (intersection.Value.Y - point1.Y) * ty;
                        bool isForward = dot > 0;

                        if (isForward && dist > 0.1 && dist < minDistance)
                        {
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
                    var intersections = GetLineCircleIntersections(point1, extendedPoint, seg.Center.Value, seg.Radius);

                    foreach (var pt in intersections)
                    {
                        if (IsPointOnArc(pt, seg, 0.1))
                        {
                            double dist = Distance(point1, pt);
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

            if (bestSegment != null && bestSegment.Type == SegmentType.Arc && bestSegment.Center != null)
            {
                double startAngle = Math.Atan2(bestPoint.Y - bestSegment.Center.Value.Y, bestPoint.X - bestSegment.Center.Value.X);
                double endAngle = Math.Atan2(point1.Y - bestSegment.Center.Value.Y, point1.X - bestSegment.Center.Value.X);

                if (bestSegment.CounterClockwise) { while (endAngle <= startAngle) endAngle += 2 * Math.PI; }
                else { while (endAngle >= startAngle) endAngle -= 2 * Math.PI; }

                ContourSegment arcSegment = new ContourSegment(
                    bestPoint, point1, bestSegment.Center.Value, bestSegment.Radius, bestSegment.CounterClockwise);

                return (bestPoint, arcSegment);
            }

            return (bestPoint, null);
        }

        // ============================================================
        // IsPointOnSegment
        // ============================================================
        private static bool IsPointOnSegment(XPoint point, XPoint start, XPoint end, double tolerance = 0.1)
        {
            double cross = (point.X - start.X) * (end.Y - start.Y) - (point.Y - start.Y) * (end.X - start.X);
            if (Math.Abs(cross) > tolerance) return false;

            double dot = (point.X - start.X) * (end.X - start.X) + (point.Y - start.Y) * (end.Y - start.Y);
            if (dot < 0) return false;

            double squaredLength = (end.X - start.X) * (end.X - start.X) + (end.Y - start.Y) * (end.Y - start.Y);
            if (dot > squaredLength) return false;

            return true;
        }

        // ============================================================
        // GetLinesIntersectionNullable
        // ============================================================
        private static XPoint? GetLinesIntersectionNullable(XPoint a1, XPoint a2, XPoint b1, XPoint b2)
        {
            double dx1 = a2.X - a1.X;
            double dy1 = a2.Y - a1.Y;
            double dx2 = b2.X - b1.X;
            double dy2 = b2.Y - b1.Y;

            double det = dx1 * dy2 - dy1 * dx2;
            if (Math.Abs(det) < 1e-6) return null;

            double t = ((b1.X - a1.X) * dy2 - (b1.Y - a1.Y) * dx2) / det;

            return new XPoint(a1.X + t * dx1, a1.Y + t * dy1);
        }

        // ============================================================
        // GetLineCircleIntersections
        // ============================================================
        private static List<XPoint> GetLineCircleIntersections(XPoint p1, XPoint p2, XPoint center, double radius)
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
            if (discriminant < -1e-9) return result;

            discriminant = Math.Max(0, discriminant);
            double sqrtD = Math.Sqrt(discriminant);

            double t1 = (-b + sqrtD) / (2 * a);
            double t2 = (-b - sqrtD) / (2 * a);

            if (t1 >= 0 && t1 <= 1) result.Add(new XPoint(p1.X + t1 * dx, p1.Y + t1 * dy));
            if (t2 >= 0 && t2 <= 1 && Math.Abs(t1 - t2) > 0.0001) result.Add(new XPoint(p1.X + t2 * dx, p1.Y + t2 * dy));

            return result;
        }

        // ============================================================
        // IsPointOnArc / Distance / GetSegmentsForSide
        // ============================================================
        private static bool IsPointOnArc(XPoint point, ContourSegment arc, double tolerance = 1)
        {
            if (arc.Center == null) return false;

            double distToCenter = Distance(point, arc.Center.Value);
            double radiusDiff = Math.Abs(distToCenter - arc.Radius);
            if (radiusDiff > tolerance) return false;

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

        private static double Distance(XPoint a, XPoint b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private List<ContourSegment> GetSegmentsForSide(List<ContourSegment> contour, string strona)
        {
            if (contour == null || contour.Count == 0) return contour;

            var result = new List<ContourSegment>();

            foreach (var seg in contour)
            {
                double dx = seg.End.X - seg.Start.X;
                double dy = seg.End.Y - seg.Start.Y;
                double angleRad = Math.Atan2(dy, dx);
                double angleDeg = angleRad * 180.0 / Math.PI;
                if (angleDeg < 0) angleDeg += 360.0;

                string segmentSide = StronaOknaHelper.OkreslStroneNaPodstawieKataLinii(angleDeg);

                if (segmentSide == strona) result.Add(seg);
            }

            return result.Count > 0 ? result : contour.ToList();
        }

        // ============================================================
        // BuildSegmentWithArc / IsPointOnAngularSweep / PositiveAngleDelta
        // ============================================================
        private ContourSegment BuildSegmentWithArc(
            XPoint start, XPoint end, List<ContourSegment> contourToSearch)
        {
            const double tolerance = 5;
            var arc = FindArcBetweenPoints(contourToSearch, start, end, tolerance);
            if (arc == null || !arc.Center.HasValue)
                return new ContourSegment(start, end);

            bool followsSourceDirection = IsPointOnAngularSweep(
                end, start, arc.End, arc.Center.Value,
                arc.CounterClockwise, arc.Radius, tolerance);

            return new ContourSegment(
                start, end, arc.Center, arc.Radius,
                followsSourceDirection ? arc.CounterClockwise : !arc.CounterClockwise);
        }

        private static bool IsPointOnAngularSweep(
            XPoint point, XPoint start, XPoint end,
            XPoint center, bool increasingAngle, double radius, double pointTolerance)
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

            double angularTolerance = Math.Max(1e-9, pointTolerance / Math.Max(radius, 1e-9));
            return progress <= sweep + angularTolerance;
        }

        private static double PositiveAngleDelta(double from, double to)
        {
            double result = (to - from) % (2 * Math.PI);
            return result < 0 ? result + 2 * Math.PI : result;
        }

        // ============================================================
        // GetPointOnArcAtAngle / FindArcBetweenPoints
        // ============================================================
        private XPoint GetPointOnArcAtAngle(ContourSegment arc, XPoint referencePoint)
        {
            if (!arc.Center.HasValue) return referencePoint;

            double angle = Math.Atan2(referencePoint.Y - arc.Center.Value.Y,
                                      referencePoint.X - arc.Center.Value.X);

            return new XPoint(
                arc.Center.Value.X + arc.Radius * Math.Cos(angle),
                arc.Center.Value.Y + arc.Radius * Math.Sin(angle));
        }

        private ContourSegment? FindArcBetweenPoints(
            List<ContourSegment> contour, XPoint point1, XPoint point2, double tolerance = 1.0)
        {
            foreach (var seg in contour)
            {
                if (seg.Type != SegmentType.Arc || !seg.Center.HasValue) continue;
                if (IsPointOnArc(point1, seg, tolerance) && IsPointOnArc(point2, seg, tolerance))
                    return seg;
            }
            return null;
        }

        // ============================================================
        // FindIntersectionWithContourT1i3 / GetLinesIntersectionNullableT1i3
        // ============================================================
        private static XPoint FindIntersectionWithContourT1i3(
            XPoint startPoint, XPoint endPoint, List<ContourSegment> contour)
        {
            if (contour == null || contour.Count == 0) return endPoint;

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

        private static XPoint? GetLinesIntersectionNullableT1i3(XPoint a1, XPoint a2, XPoint b1, XPoint b2)
        {
            double dx1 = a2.X - a1.X;
            double dy1 = a2.Y - a1.Y;
            double dx2 = b2.X - b1.X;
            double dy2 = b2.Y - b1.Y;

            double det = dx1 * dy2 - dy1 * dx2;
            if (Math.Abs(det) < 1e-6) return null;

            double t = ((b1.X - a1.X) * dy2 - (b1.Y - a1.Y) * dx2) / det;

            return new XPoint(a1.X + t * dx1, a1.Y + t * dy1);
        }

        private static bool IsPointOnArcT1i3(XPoint point, ContourSegment arc, double tolerance = 0.1)
        {
            if (arc.Center == null) return false;

            double distToCenter = Distance(point, arc.Center.Value);
            double radiusDiff = Math.Abs(distToCenter - arc.Radius);
            if (radiusDiff > tolerance) return false;

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

        private static List<XPoint> GetLineCircleIntersectionsT1i3(XPoint p1, XPoint p2, XPoint center, double radius)
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
            if (discriminant < -1e-9) return result;

            discriminant = Math.Max(0, discriminant);
            double sqrtD = Math.Sqrt(discriminant);

            double t1 = (-b + sqrtD) / (2 * a);
            double t2 = (-b - sqrtD) / (2 * a);

            if (t1 >= 0 && t1 <= 1) result.Add(new XPoint(p1.X + t1 * dx, p1.Y + t1 * dy));
            if (t2 >= 0 && t2 <= 1 && Math.Abs(t1 - t2) > 0.0001) result.Add(new XPoint(p1.X + t2 * dx, p1.Y + t2 * dy));

            return result;
        }

        // ============================================================
        // CalculateOffsetPolygon (wielokąt)
        // ============================================================
        public async Task<List<XPoint>> CalculateOffsetPolygon(
            List<XPoint> points,
            float profileLeft,
            float profileRight,
            float profileTop,
            float profileBottom,
            bool slupekPoStronieA = false,
            bool slupekPoStronieB = false)
        {
            int count = points.Count;

            if (count > 0)
            {
                Console.WriteLine($"🔷CalculateOffsetPolygon Calculating offset polygon for {count} X:{points[0].X} Y:{points[0].Y} slupekPoStroanieA:{slupekPoStronieA} slupekPoStroanieB:{slupekPoStronieB} points with profiles L:{profileLeft}, R:{profileRight}, T:{profileTop}, B:{profileBottom}");
            }

            if (count < 2)
            {
                Komunikaty.Add("Figura musi mieć co najmniej 2 punkty.");
                return points;
            }

            if (count < 3)
            {
                Komunikaty.Add("Wielokąt musi mieć co najmniej 3 punkty.");
                return points;
            }

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

                float angleRadians = MathF.Atan2(dy, dx);
                float angleDegrees = angleRadians * (180f / MathF.PI);
                if (angleDegrees < 0) angleDegrees += 360f;

                string side = StronaOknaHelper.OkreslStrone(angleDegrees, i, points);

                float tx = dx / length;
                float ty = dy / length;

                float nx = ty;
                float ny = -tx;

                float offsetValue = 0f;
                bool usePositiveNormal = true;

                switch (side)
                {
                    case "Góra":
                        offsetValue = slupekPoStronieA ? profileBottom : profileTop;
                        usePositiveNormal = false;
                        break;
                    case "Dół":
                        offsetValue = slupekPoStronieB ? profileTop : profileBottom;
                        usePositiveNormal = false;
                        break;
                    case "Lewa":
                        offsetValue = profileLeft;
                        usePositiveNormal = false;
                        break;
                    case "Prawa":
                        offsetValue = profileRight;
                        usePositiveNormal = false;
                        break;
                }

                float offset = usePositiveNormal ? offsetValue : -offsetValue;

                var p1Offset = new XPoint(p1.X + nx * offset, p1.Y + ny * offset);
                var p2Offset = new XPoint(p2.X + nx * offset, p2.Y + ny * offset);

                offsetLines.Add((p1Offset, p2Offset, side, offset));
            }

            var result = new List<XPoint>();

            for (int i = 0; i < offsetLines.Count; i++)
            {
                var (a1, a2, sideA, offsetA) = offsetLines[i];
                var (b1, b2, sideB, offsetB) = offsetLines[(i - 1 + offsetLines.Count) % offsetLines.Count];

                var intersection = GetLinesIntersection(a1, a2, b1, b2);

                if (float.IsNaN((float)intersection.X) || float.IsNaN((float)intersection.Y))
                {
                    intersection = new XPoint((a1.X + b1.X) / 2f, (a1.Y + b1.Y) / 2f);
                }

                result.Add(intersection);
            }

            await Task.CompletedTask;
            return result;
        }

        // ============================================================
        // CalculateOffsetPolygonKontur
        // ============================================================
        public async Task<List<ContourSegment>> CalculateOffsetPolygonKontur(
            List<ContourSegment> segments,
            float profileLeft,
            float profileRight,
            float profileTop,
            float profileBottom)
        {
            if (segments == null || segments.Count == 0)
                return new List<ContourSegment>();

            const double EPS = 1e-6;
            const double TOLERANCJA = 0.01;

            var offsetSegments = new List<ContourSegment>();
            var arcRadiusCache = new Dictionary<string, float>();

            bool isFullCircle = segments.All(s => s.Type == SegmentType.Arc);

            var bboxCenter = new XPoint(
                segments.Average(s => (s.Start.X + s.End.X) / 2.0),
                segments.Average(s => (s.Start.Y + s.End.Y) / 2.0)
            );

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
                        arcRadiusCache[arcKey] = (float)(seg.Radius - offsetValue);

                    float newRadius = arcRadiusCache[arcKey];
                    if (newRadius < 0.1f) newRadius = 0.1f;

                    double startAngle = Math.Atan2(seg.Start.Y - center.Y, seg.Start.X - center.X);
                    double endAngle = Math.Atan2(seg.End.Y - center.Y, seg.End.X - center.X);

                    var newStart = new XPoint(
                        center.X + newRadius * Math.Cos(startAngle),
                        center.Y + newRadius * Math.Sin(startAngle));

                    var newEnd = new XPoint(
                        center.X + newRadius * Math.Cos(endAngle),
                        center.Y + newRadius * Math.Sin(endAngle));

                    newStart = SnapPoint(newStart);
                    newEnd = SnapPoint(newEnd);

                    offsetSegments.Add(new ContourSegment(newStart, newEnd, center, newRadius, true)
                    {
                        Informacja = seg.Informacja ?? (isFullCircle ? "ARC_FULL_CIRCLE" : side)
                    });
                }
            }

            var result = new List<ContourSegment>();

            for (int i = 0; i < offsetSegments.Count; i++)
            {
                var current = offsetSegments[i];
                var previous = offsetSegments[(i - 1 + offsetSegments.Count) % offsetSegments.Count];

                XPoint? intersection = null;

                if (current.Type == SegmentType.Line && previous.Type == SegmentType.Line)
                    intersection = GetLinesIntersectionK(previous.Start, previous.End, current.Start, current.End);
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
                        result[^1].End = intersection.Value;

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
                            (result[^1].End.Y + current.Start.Y) / 2.0);
                        result[^1].End = srodek;
                        current.Start = srodek;
                    }
                    result.Add(current);
                }
            }

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

            if (result.Count > 0)
            {
                var firstStart = result[0].Start;
                var lastEnd = result[^1].End;

                double odleglosc = Math.Sqrt(Math.Pow(lastEnd.X - firstStart.X, 2) +
                                             Math.Pow(lastEnd.Y - firstStart.Y, 2));

                if (odleglosc > TOLERANCJA)
                    result[^1].End = result[0].Start;
            }

            await Task.CompletedTask;
            return result;
        }

        // ============================================================
        // CalculateOffsetPolygonKonturSkrzydlo
        // ============================================================
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

            const double EPS = 1e-6;
            const double TOLERANCJA = 0.01;

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

                        switch (side)
                        {
                            case "Góra": offsetY = -profileTop; break;
                            case "Dół": offsetY = profileBottom; break;
                            case "Lewa": offsetX = profileLeft; break;
                            case "Prawa": offsetX = -profileRight; break;
                        }

                        var newSeg1Start = new XPoint(seg1.Start.X + offsetX, seg1.Start.Y + offsetY);
                        var newSeg1End = new XPoint(seg1.End.X + offsetX, seg1.End.Y + offsetY);
                        var newSeg2Start = new XPoint(seg2.Start.X + offsetX, seg2.Start.Y + offsetY);
                        var newSeg2End = new XPoint(seg2.End.X + offsetX, seg2.End.Y + offsetY);

                        return new List<ContourSegment>
                        {
                            new ContourSegment(newSeg1Start, newSeg1End) { Informacja = seg1.Informacja ?? side },
                            new ContourSegment(newSeg2Start, newSeg2End) { Informacja = seg2.Informacja ?? side }
                        };
                    }
                }
                else
                {
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
                        (seg.Start.Y + seg.End.Y) / 2.0);

                    var testA = new XPoint(midpoint.X + nx * offsetValue, midpoint.Y + ny * offsetValue);
                    var testB = new XPoint(midpoint.X - nx * offsetValue, midpoint.Y - ny * offsetValue);

                    double da = DistanceSquared(testA, bboxCenter);
                    double db = DistanceSquared(testB, bboxCenter);

                    double sign;
                    if (offsetValue >= 0) sign = da < db ? 1 : -1;
                    else sign = da < db ? -1 : 1;

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
                        arcRadiusCache[arcKey] = (float)(seg.Radius - offsetValue);

                    float newRadius = arcRadiusCache[arcKey];
                    if (newRadius < 0.1f) newRadius = 0.1f;

                    double startAngle = Math.Atan2(seg.Start.Y - center.Y, seg.Start.X - center.X);
                    double endAngle = Math.Atan2(seg.End.Y - center.Y, seg.End.X - center.X);

                    var newStart = new XPoint(
                        center.X + newRadius * Math.Cos(startAngle),
                        center.Y + newRadius * Math.Sin(startAngle));

                    var newEnd = new XPoint(
                        center.X + newRadius * Math.Cos(endAngle),
                        center.Y + newRadius * Math.Sin(endAngle));

                    newStart = SnapPoint(newStart);
                    newEnd = SnapPoint(newEnd);

                    offsetSegments.Add(new ContourSegment(newStart, newEnd, center, newRadius, true)
                    {
                        Informacja = seg.Informacja ?? (isFullCircle ? "ARC_FULL_CIRCLE" : side)
                    });
                }
            }

            var result = new List<ContourSegment>();

            for (int i = 0; i < offsetSegments.Count; i++)
            {
                var current = offsetSegments[i];
                var previous = offsetSegments[(i - 1 + offsetSegments.Count) % offsetSegments.Count];

                XPoint? intersection = null;

                if (current.Type == SegmentType.Line && previous.Type == SegmentType.Line)
                    intersection = GetLinesIntersectionK(previous.Start, previous.End, current.Start, current.End);
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
                        result[^1].End = intersection.Value;

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
                            (result[^1].End.Y + current.Start.Y) / 2.0);
                        result[^1].End = srodek;
                        current.Start = srodek;
                    }
                    result.Add(current);
                }
            }

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

            if (result.Count > 0)
            {
                var firstStart = result[0].Start;
                var lastEnd = result[^1].End;

                double odleglosc = Math.Sqrt(Math.Pow(lastEnd.X - firstStart.X, 2) +
                                             Math.Pow(lastEnd.Y - firstStart.Y, 2));

                if (odleglosc > TOLERANCJA)
                    result[^1].End = result[0].Start;
            }

            await Task.CompletedTask;
            return result;
        }

        // ============================================================
        // SnapPoint / DistanceSquared / ChooseClosestTo
        // ============================================================
        private static XPoint SnapPoint(XPoint p, double precision = 0.001)
        {
            return new XPoint(
                Math.Round(p.X / precision) * precision,
                Math.Round(p.Y / precision) * precision);
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

        // ============================================================
        // GetCircleCircleIntersections
        // ============================================================
        private List<XPoint> GetCircleCircleIntersections(XPoint c0, double r0, XPoint c1, double r1)
        {
            var results = new List<XPoint>();
            double dx = c1.X - c0.X;
            double dy = c1.Y - c0.Y;
            double d = Math.Sqrt(dx * dx + dy * dy);
            if (d < 1e-9) return results;
            if (d > r0 + r1 + 1e-9) return results;
            if (d < Math.Abs(r0 - r1) - 1e-9) return results;

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

        // ============================================================
        // GetLinesIntersectionK / GetLinesIntersection
        // ============================================================
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

        private XPoint GetLinesIntersection(XPoint a1, XPoint a2, XPoint b1, XPoint b2)
        {
            float dx1 = (float)(a2.X - a1.X);
            float dy1 = (float)(a2.Y - a1.Y);
            float dx2 = (float)(b2.X - b1.X);
            float dy2 = (float)(b2.Y - b1.Y);

            float determinant = dx1 * dy2 - dy1 * dx2;
            if (Math.Abs(determinant) < 1e-6f)
                return new XPoint((a1.X + b1.X) / 2, (a1.Y + b1.Y) / 2);

            float t = (float)((b1.X - a1.X) * dy2 - (b1.Y - a1.Y) * dx2) / determinant;

            return new XPoint(a1.X + t * dx1, a1.Y + t * dy1);
        }

        // ============================================================
        // ApplyOffsetToPointST
        // ============================================================
        private XPoint ApplyOffsetToPointST(
            XPoint point,
            List<XPoint> linia,
            List<DaneKwadratu> daneKwadratu)
        {
            if (point.IsEmpty || linia == null || linia.Count < 2 || daneKwadratu == null)
                return point;

            XPoint top = linia.First();
            XPoint bottom = linia.Last();

            double dx = bottom.X - top.X;
            double dy = bottom.Y - top.Y;
            bool isVertical = Math.Abs(dy) >= Math.Abs(dx);

            bool wewnatrz;
            if (isVertical)
            {
                double minY = Math.Min(top.Y, bottom.Y);
                double maxY = Math.Max(top.Y, bottom.Y);
                wewnatrz = point.Y >= minY && point.Y <= maxY;
            }
            else
            {
                double minX = Math.Min(top.X, bottom.X);
                double maxX = Math.Max(top.X, bottom.X);
                wewnatrz = point.X >= minX && point.X <= maxX;
            }

            if (wewnatrz)
            {
                Console.WriteLine($"[ApplyOffsetToPointST] point=({point.X};{point.Y}) wewnątrz obszaru — bez zmian.");
                return point;
            }

            double liniaX = (top.X + bottom.X) / 2.0;
            double liniaY = (top.Y + bottom.Y) / 2.0;

            string strona;
            if (isVertical)
                strona = point.X < liniaX ? "lewa" : "prawa";
            else
                strona = point.Y < liniaY ? "góra" : "dół";

            var wszystkie = daneKwadratu
                .Where(d => d?.Przesuniecia != null)
                .SelectMany(d => d.Przesuniecia)
                .ToList();

            var rekord = wszystkie
                .FirstOrDefault(p => string.Equals(p.Strona.ToLower(), strona.ToLower(), StringComparison.OrdinalIgnoreCase))
                ?? wszystkie.FirstOrDefault(p => string.Equals(p.Strona.ToLower(), "dół", StringComparison.OrdinalIgnoreCase));

            if (rekord == null)
            {
                Console.WriteLine($"[ApplyOffsetToPointST] Brak rekordu dla Strona='{strona}' — bez zmian.");
                return point;
            }

            double shiftX = 0;
            double shiftY = 0;
            double styczna = Math.Abs(rekord.PrzesuniecieYStycznej);

            switch (strona)
            {
                case "góra": shiftY = -styczna; break;
                case "dół": shiftY = +styczna; break;
                case "lewa": shiftX = -styczna; break;
                case "prawa": shiftX = +styczna; break;
            }

            Console.WriteLine($"[ApplyOffsetToPointST] point=({point.X};{point.Y}), " +
                              $"top=({top.X};{top.Y}), bottom=({bottom.X};{bottom.Y}), " +
                              $"isVertical={isVertical}, wewnatrz={wewnatrz}, strona='{strona}', " +
                              $"rekord='{rekord.Strona}', styczna={styczna}, " +
                              $"shiftX={shiftX}, shiftY={shiftY}");

            return new XPoint
            {
                X = point.X + shiftX,
                Y = point.Y + shiftY
            };
        }

        // ============================================================
        // PrepareRegionPoints
        // ============================================================
        private List<XPoint> PrepareRegionPoints(
            XPoint top,
            XPoint bottom,
            List<XPoint> source,
            bool topZmieniony,
            bool bottomZmieniony,
            double przesuniecieTop,
            double przesuniecieBottom,
            bool enableLogs = false)
        {
            if (source == null || source.Count == 0)
            {
                if (enableLogs) Console.WriteLine("🟡 PrepareRegionPoints: source == null lub pusty");
                return new List<XPoint>();
            }

            if (!topZmieniony && !bottomZmieniony)
            {
                if (enableLogs) Console.WriteLine("🟦 PrepareRegionPoints – POMINIĘTE");
                return source.Select(p => new XPoint(p.X, p.Y)).ToList();
            }

            if (enableLogs)
            {
                Console.WriteLine("═══════════════════════════════════════════════");
                Console.WriteLine("🟦 PrepareRegionPoints – START");
                Console.WriteLine($"   top                = ({top.X:F3}, {top.Y:F3})");
                Console.WriteLine($"   bottom             = ({bottom.X:F3}, {bottom.Y:F3})");
                Console.WriteLine($"   topZmieniony       = {topZmieniony}");
                Console.WriteLine($"   bottomZmieniony    = {bottomZmieniony}");
                Console.WriteLine($"   przesuniecieTop    = {przesuniecieTop:F3}");
                Console.WriteLine($"   przesuniecieBottom = {przesuniecieBottom:F3}");
                Console.WriteLine($"   source.Count       = {source.Count}");
            }

            double ax = bottom.X - top.X;
            double ay = bottom.Y - top.Y;
            double alenSq = ax * ax + ay * ay;

            if (alenSq < 1e-12)
            {
                if (enableLogs) Console.WriteLine("🟠 PrepareRegionPoints: oś zerowej długości");
                return source.Select(p => new XPoint(p.X, p.Y)).ToList();
            }

            double alen = Math.Sqrt(alenSq);
            double ux = ax / alen;
            double uy = ay / alen;

            double nx = -uy;
            double ny = ux;

            double tMin = 0.0 + (topZmieniony ? przesuniecieTop : 0.0);
            double tMax = alen - (bottomZmieniony ? przesuniecieBottom : 0.0);

            if (tMin > tMax)
            {
                double mid = (tMin + tMax) / 2.0;
                tMin = mid;
                tMax = mid;
                if (enableLogs) Console.WriteLine("🟠 PrepareRegionPoints: przesunięcia większe niż długość odcinka");
            }

            var result = new List<XPoint>(source.Count);

            for (int i = 0; i < source.Count; i++)
            {
                var p = source[i];

                double vx = p.X - top.X;
                double vy = p.Y - top.Y;

                double t = vx * ux + vy * uy;
                double s = vx * nx + vy * ny;

                double tClamped = t;

                if (topZmieniony && t < tMin) tClamped = tMin;
                if (bottomZmieniony && t > tMax) tClamped = tMax;

                var q = new XPoint(
                    top.X + tClamped * ux + s * nx,
                    top.Y + tClamped * uy + s * ny);

                result.Add(q);
            }

            return result;
        }

        // ============================================================
        // RotateContourSegments / Corner
        // ============================================================
        private List<XPoint> RotateContourSegments(
            List<XPoint> points,
            Corner startCorner = Corner.TopLeft,
            bool clockwise = true)
        {
            if (points == null || points.Count < 2) return points;

            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            XPoint startPoint = startCorner switch
            {
                Corner.TopLeft => points.OrderBy(p => Distance(p, new XPoint(minX, minY))).First(),
                Corner.TopRight => points.OrderBy(p => Distance(p, new XPoint(maxX, minY))).First(),
                Corner.BottomRight => points.OrderBy(p => Distance(p, new XPoint(maxX, maxY))).First(),
                Corner.BottomLeft => points.OrderBy(p => Distance(p, new XPoint(minX, maxY))).First(),
                _ => points.OrderBy(p => Distance(p, new XPoint(minX, minY))).First()
            };

            int startIndex = points.FindIndex(p => Distance(p, startPoint) < 0.001);
            if (startIndex < 0) return points;

            var rotated = new List<XPoint>(points.Count);

            for (int i = 0; i < points.Count; i++)
                rotated.Add(points[(startIndex + i) % points.Count]);

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

        public enum Corner
        {
            TopLeft,
            TopRight,
            BottomRight,
            BottomLeft
        }

        // ============================================================
        // RemoveDuplicateConsecutivePoints / ArePointsEqual
        // ============================================================
        private List<XPoint> RemoveDuplicateConsecutivePoints(List<XPoint> points)
        {
            var unique = new List<XPoint>();

            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0 || !ArePointsEqual(points[i], points[i - 1]))
                    unique.Add(points[i]);
            }

            if (unique.Count > 2 && ArePointsEqual(unique.First(), unique.Last()))
                unique.RemoveAt(unique.Count - 1);

            return unique;
        }

        private bool ArePointsEqual(XPoint p1, XPoint p2)
        {
            return Math.Abs(p1.X - p2.X) < 0.1 && Math.Abs(p1.Y - p2.Y) < 0.1;
        }

        // ============================================================
        // ObliczRoznicePoziomow / Szyba / KorpusWewnetrzny
        // ============================================================
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

                if (gora == 0 && dol != 0) return Math.Abs(dol);
                if (dol == 0 && gora != 0) return Math.Abs(gora);

                await Task.CompletedTask;
                return Math.Abs(gora - dol);
            }
            else
            {
                Komunikaty.Add($"Słupki stałe mają zawsze pełną wartość profilu, niezależnie od poziomów pozostałe dane z tabeli KonfPolaczenia");
                await Task.CompletedTask;
                return 0;
            }
        }

        private async Task<float> ObliczRoznicePoziomowSzyba(KonfSystem? konf, bool slupekStaly)
        {
            if (konf == null || !konf.CzyMozeBycFix) return 0;

            if (!slupekStaly)
            {
                float gora = (float)konf.PoziomLiniaSzkla;
                float dol = (float)konf.PoziomDol;

                if (gora == 0 && dol != 0) return Math.Abs(dol);
                if (dol == 0 && gora != 0) return Math.Abs(gora);

                await Task.CompletedTask;
                return Math.Abs(gora - dol);
            }
            else
            {
                BledySystemowe.Add($"Dla słupków stałych nie wyszukano w tabeli KonfPolaczenia wartość LINIA SZKŁA");
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

            if (gora == 0 && dol != 0) return Math.Abs(dol);
            if (dol == 0 && gora != 0) return Math.Abs(gora);

            await Task.CompletedTask;
            return Math.Abs(gora - dol);
        }

        // ============================================================
        // GetTopEdgeAngleFromFirstSegment
        // ============================================================
        public static float GetTopEdgeAngleFromFirstSegment(List<XPoint> outer)
        {
            if (outer == null || outer.Count < 2)
                throw new ArgumentException("Lista punktów musi mieć co najmniej 2 elementy.");

            var p1 = outer[0];
            var p2 = outer[1];

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            if (angle < 0) angle += 360;

            return (float)angle;
        }

        // ============================================================
        // ObliczDlugoscKonturu / DlugoscLukuKontur / OdlegloscKontur
        // ============================================================
        public float ObliczDlugoscKonturu(List<ContourSegment> kontur)
        {
            if (kontur == null || kontur.Count == 0) return 0;

            double sumaDlugosci = 0;

            foreach (var segment in kontur)
            {
                if (segment.Type == SegmentType.Arc)
                    sumaDlugosci += DlugoscLukuKontur(segment);
                else
                    sumaDlugosci += OdlegloscKontur(segment.Start, segment.End);
            }

            return (float)sumaDlugosci;
        }

        private double DlugoscLukuKontur(ContourSegment arc)
        {
            if (arc.Type != SegmentType.Arc || !arc.Center.HasValue)
                return 0;

            double startAngle = Math.Atan2(arc.Start.Y - arc.Center.Value.Y, arc.Start.X - arc.Center.Value.X);
            double endAngle = Math.Atan2(arc.End.Y - arc.Center.Value.Y, arc.End.X - arc.Center.Value.X);

            if (arc.CounterClockwise) { if (endAngle < startAngle) endAngle += 2 * Math.PI; }
            else { if (endAngle > startAngle) endAngle -= 2 * Math.PI; }

            double angleDelta = Math.Abs(endAngle - startAngle);

            return arc.Radius * angleDelta;
        }

        private double OdlegloscKontur(XPoint p1, XPoint p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        // ============================================================
        // DlugoscElementu
        // ============================================================
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
    }
}
