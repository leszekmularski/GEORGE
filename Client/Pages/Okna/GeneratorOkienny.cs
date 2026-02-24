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

        // Lista wierzcholkow (w kolejnosci zgodnej z ruchem wskazowek zegara)
        public List<XPoint> Wierzcholki { get; set; } = new();
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
        }
        public async Task<bool> AddElements(List<ShapeRegion> regions, string regionId, Dictionary<string, GeneratorState> generatorStates, List<ShapeRegion> regionAdd,
            List<DaneKwadratu> daneKwadratu, List<XPoint> punktyRegionuMaster, XPoint mouseClik, bool kasujKonsole = true)
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

            //if(daneKwadratu != null)
            //{
            //    foreach (var dk in daneKwadratu)
            //    {
            //        foreach (var w in dk.Wierzcholki)
            //        {
            //            Console.WriteLine($"➡️ AddElements daneKwadratu Wierzcholek X: {w.X} Y: {w.Y}");
            //        }
            //    }
            //}

            Console.WriteLine($"📏 AddElements Szerokosc: {Szerokosc}, Wysokosc: {Wysokosc}");

            Region = regionAdd;

            var region = regions.FirstOrDefault(r => r.Id == regionId);

            List<XPoint> punkty = new List<XPoint>();

            if (region == null && !ElementLiniowy)
            {
                Console.WriteLine($"❌ Nie znaleziono regionu o ID: {regionId} w AddElements - GeneratoryOkienne");
                return false;
            }
            else if (region != null && !ElementLiniowy)
            {
                punkty = region.Wierzcholki;
            }
            else if (ElementLiniowy)
            {
                region = regions.FirstOrDefault(r => r.Id != null);

                Console.WriteLine($"❌ Region o ID: {regionId} region.Wierzcholki.Count():{region.Wierzcholki.Count()}");

                punkty = region.Wierzcholki;
            }

            Wierzcholki = punkty;

            foreach (var x in punkty)
            {
                Console.WriteLine($"x.X: {x.X} / x.Y: {x.Y}");
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
                .ToList()
             ?? new List<XPoint>();

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
            List<XPoint> wewnetrznyKontur;

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
            }

            var ok = await GenerateGenericElementsWithJoins(
                     przeskalowanePunkty,
                     wewnetrznyKontur,
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

            if (ok)
            {
                Console.WriteLine($"✅ Generowanie elementów zakończone sukcesem dla regionu {regionId}");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ Generowanie elementów zakończone niepowodzeniem dla regionu {regionId}");
                return false;
            }

            //}
        }
        public async Task<bool> GenerateGenericElementsWithJoins(
            List<XPoint> outer, List<XPoint> inner,
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

            foreach (var test in inner)
            {
                Console.WriteLine($"🔷🔷 inner point X: {test.X} Y: {test.Y}");
            }

            foreach (var test in outer)
            {
                Console.WriteLine($"🔷🔷 outer point X: {test.X} Y: {test.Y}");
            }

            for (int i = 0; i < vertexCount; i++)
            {
                int next = (i + 1) % vertexCount;
                int prev = (i - 1 + vertexCount) % vertexCount;

                var leftJoin = polaczeniaArray[i].typ;
                var rightJoin = polaczeniaArray[next].typ;

                XPoint outerStart = outer[i];
                XPoint outerEnd = outer[next];

                XPoint _innerStart = inner[i];
                XPoint _innerEnd = inner[next];

                float dx = (float)(outerEnd.X - outerStart.X);
                float dy = (float)(outerEnd.Y - outerStart.Y);
                float length = MathF.Sqrt(dx * dx + dy * dy);

                float angleRadians = MathF.Atan2(dy, dx); // kąt w radianach
                float angleDegrees = angleRadians * (180f / MathF.PI); // kąt w stopniach

                bool dodajA = false;
                bool dodajB = false;

                // Przekształć do zakresu 0–360°, jeśli potrzebujesz
                if (angleDegrees < 0)
                    angleDegrees += 360f;

                // OKREŚLENIE STRONY PRZED generowaniem wierzchołków
                //StronaElementu = OkreslStroneNaPodstawieKata(angleDegrees, i, outer);
                StronaElementu = StronaOknaHelper.OkreslStrone(angleDegrees, i, outer);

                Console.WriteLine($"▶️ Processing element {i + 1}/{vertexCount} with joins: {leftJoin} - {rightJoin} wyliczony kąt: {angleDegrees} dla i: {i} StronaElementu: {StronaElementu} length: {length}");

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

                Console.WriteLine($"🔷 T1/T1 element --> {i + 1}/{vertexCount} with joins: {leftJoin} - {rightJoin} angleDegrees: {angleDegrees} katGornegoElemntu: {katGornegoElemntu} StronaElementu: {StronaElementu}");

                if (leftJoin == "T1" && rightJoin == "T4" || leftJoin == "T4" && rightJoin == "T1" || leftJoin == "T4" && rightJoin == "T4")
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

                        if (leftJoin == "T1" && rightJoin == "T4" && vertexCount > 4)
                        {
                            //  Console.WriteLine($"🔷 Horizontal case for element {i + 1} isAlmostHorizontal: {isAlmostHorizontal} isAlmostVertical: {isAlmostVertical}");
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
                        else if (leftJoin == "T4" && rightJoin == "T4" && vertexCount > 4)
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

                            var outerTop = GetHorizontalIntersection(outerStart, outerEnd, (float)topY);
                            var outerBottom = GetHorizontalIntersection(outerStart, outerEnd, (float)bottomY);

                            var innerTop = GetHorizontalIntersection(inner[i], inner[next], (float)topY);
                            var innerBottom = GetHorizontalIntersection(inner[i], inner[next], (float)bottomY);

                            wierzcholki = new List<XPoint> {
                                outerTop, outerBottom, innerBottom, innerTop
                            };
                        }


                    }
                }
                else if (leftJoin == "T1" && rightJoin == "T1")
                {
                    //Console.WriteLine($"🔷 T1/T1 element {i + 1} START isAlmostHorizontal: {isAlmostHorizontal} isAlmostVertical: {isAlmostVertical} vertexCount: {vertexCount} angleDegrees: {angleDegrees}");

                    if (vertexCount == 3 && angleDegrees == 0)
                    {
                        // Console.WriteLine($"🔷 T1/T1 element {i + 1} vertexCount == 3 && angleDegrees == 0 START");

                        var nextNext = (next + 1) % vertexCount;

                        // Pionowy przypadek (np. boczne elementy w trapezie)
                        var topY = Math.Min(inner[i].Y, inner[next].Y);
                        var bottomY = Math.Max(inner[i].Y, inner[next].Y);

                        // var topY2 = Math.Min(outer[i].Y, outer[next].Y);

                        var y1Min = Math.Min(inner[i].Y, inner[next].Y);
                        var y2Min = Math.Min(outer[i].Y, outer[next].Y);

                        var x1Min = Math.Min(inner[i].X, inner[next].X);
                        var x2Min = Math.Min(outer[i].X, outer[next].X);

                        // Obliczanie wartości sinusa (stosunku)
                        // 1. Obliczanie różnic między punktami (przyprostokątne)
                        double roznicaY = Math.Abs(y1Min - y2Min);
                        double roznicaX = Math.Abs(x1Min - x2Min);

                        // 2. Obliczanie kąta w radianach za pomocą Math.Atan2 (najbezpieczniejsza metoda)
                        // Atan2 przyjmuje najpierw Y, potem X
                        double katWRadianach = Math.Atan2(roznicaY, roznicaX);

                        Console.WriteLine($"🔷 Trapez T1/T1 vertexCount==3 katWRadianach: {katWRadianach}  roznicaY: {roznicaY}");

                        // ✨ Korekcja styku z pionami T3 z lewej i prawej strony

                        var outerVecStartFull = FindFirstEdgeIntersection(outerStart, nx, ny, outer);
                        var outerVecEndFull = FindFirstEdgeIntersection(outerEnd, nx, ny, outer);

                        // ✂️ Skrócenie o profile pionowe
                        var outerVecStart = new XPoint(
                            outerVecStartFull.X + tx * profileLeft,
                            outerVecStartFull.Y + ty * profileLeft);

                        var outerVecEnd = new XPoint(
                            outerVecEndFull.X - tx * profileRight,
                            outerVecEndFull.Y - ty * profileRight);

                        var outerBottom = GetHorizontalIntersection(outerStart, outerEnd, (float)bottomY);
                        var innerBottom = GetHorizontalIntersection(inner[i], inner[next], (float)bottomY);

                        // Przesunięcie do wnętrza
                        var innerVecStart = FindFirstEdgeIntersection(
                            new XPoint(outerVecStart.X + nx * profile, outerVecStart.Y + ny * profile),
                            tx, ty, inner);

                        var innerVecEnd = FindFirstEdgeIntersection(
                            new XPoint(outerVecEnd.X + nx * profile, outerVecEnd.Y + ny * profile),
                            tx, ty, inner);

                        if (katWRadianach < 1)
                        {
                            outerVecStart = outer[next];

                            innerVecStart = FindFirstEdgeIntersection(
                                new XPoint(innerVecEnd.X + nx, innerVecEnd.Y + ny),
                                tx, ty, outer);
                        }

                        if (angleDegrees == 180 || angleDegrees == 0)
                        {
                            outerBottom.Y = outerBottom.Y + profile;
                            // kierunek dolnego elementu (od outerBottom w stronę innerBottom)
                            //var dirX = tx;
                            //var dirY = ty;

                            // punkt wewnętrzny = outerBottom przesunięty:
                            // 1️⃣ do wnętrza (grubość profilu)
                            // 2️⃣ wzdłuż osi dolnego profilu
                            Console.WriteLine($"🔷 Trapez T1/T1 vertexCount==3 angleDegrees == 180 lub 0 ny: {ny} nx: {nx}");
                            //innerBottom = new XPoint(
                            //    outerBottom.X + nx * profile + dirX * profile ,
                            //    outerBottom.Y + ny * profile + dirY * profile
                            //);
                            innerBottom = GetHorizontalIntersection(outer[prev], outer[i], (float)outerBottom.Y + ny * profile + ty * profile);


                        }

                        wierzcholki = new List<XPoint> {
                            outerVecStart, outerBottom, innerBottom, innerVecStart
                            };
                    }
                    else
                    {
                        if (isAlmostHorizontal)
                        {
                            Console.WriteLine($"🔷 T1/T1 element {i + 1} --> else vertexCount == {vertexCount} && angleDegrees == {angleDegrees} isAlmostHorizontal = {isAlmostHorizontal}");
                            //var prev = (i - 1 + vertexCount) % vertexCount;
                            // 🔷 Pionowe – pełne
                            var outerVecTop = FindFirstEdgeIntersection(outerStart, nx, ny, outer);
                            var outerVecBottom = FindFirstEdgeIntersection(outerEnd, nx, ny, outer);

                            var innerVecTop = FindFirstEdgeIntersection(
                                new XPoint(outerVecTop.X + nx * profile, outerVecTop.Y + ny * profile),
                                tx, ty, outer);

                            var innerVecBottom = FindFirstEdgeIntersection(
                                new XPoint(outerVecBottom.X + nx * profile, outerVecBottom.Y + ny * profile),
                                tx, ty, outer);

                            const double eps = 0.1;

                            // ================= OUTER =================
                            double minYOuter = outer.Min(p => p.Y);
                            double maxYOuter = outer.Max(p => p.Y);
                            double minXOuter = outer.Min(p => p.X);
                            double maxXOuter = outer.Max(p => p.X);

                            var minOuter = outer
                                .Where(p => Math.Abs(p.Y - minYOuter) < eps)
                                .OrderBy(p => p.X)
                                .FirstOrDefault();

                            var leftOuter = outer
                                .Where(p => Math.Abs(p.Y - maxYOuter) < eps)
                                .OrderBy(p => p.X)
                                .FirstOrDefault();

                            var rightOuter = outer
                                .Where(p => Math.Abs(p.Y - maxYOuter) < eps)
                                .OrderByDescending(p => p.X)
                                .FirstOrDefault();


                            // ================= INNER =================
                            double minYInner = inner.Min(p => p.Y);
                            double maxYInner = inner.Max(p => p.Y);
                            double minXInner = inner.Min(p => p.X);
                            double maxXInner = inner.Max(p => p.X);

                            var minInner = inner
                                .Where(p => Math.Abs(p.Y - minYInner) < eps)
                                .OrderBy(p => p.X)
                                .FirstOrDefault();

                            var leftInner = inner
                                .Where(p => Math.Abs(p.Y - maxYInner) < eps)
                                .OrderBy(p => p.X)
                                .FirstOrDefault();

                            var rightInner = inner
                                .Where(p => Math.Abs(p.Y - maxYInner) < eps)
                                .OrderByDescending(p => p.X)
                                .FirstOrDefault();

                            if (vertexCount > 4 && angleDegrees < 90)
                            {
                                //TO DO POPRAWY
                                Console.WriteLine($"🔷 T1/T1 🔷 vertexCount > {vertexCount} && angleDegrees < {angleDegrees} for element {i + 1} with joins: {leftJoin}-{rightJoin} angleDegrees: {angleDegrees}");

                                // 2️⃣ Dolny punkt inner (prawy dół)
                                innerVecTop = minInner;

                                // 4️⃣ Dolny punkt outer (przecięcie poziome)
                                outerVecTop = FindFirstEdgeIntersectionByAngle(innerVecTop, 360 - angleDegrees, outer);

                            }

                            wierzcholki = new List<XPoint> {
                            outerVecTop, outerVecBottom, innerVecBottom, innerVecTop
                            };
                        }
                        else
                        {
                            Console.WriteLine($"🔷 T1/T1 element {i + 1} --> else vertexCount == {vertexCount} && angleDegrees == {angleDegrees} isAlmostHorizontal = {isAlmostHorizontal}");
                            // Przecięcia "normalne"
                            var outerVecStartFull = FindFirstEdgeIntersection(outerStart, nx, ny, outer);
                            var outerVecEndFull = FindFirstEdgeIntersection(outerEnd, nx, ny, outer);

                            // ✂️ Skrócenie o profile pionowe
                            var outerVecStart = new XPoint(
                                outerVecStartFull.X + tx * profileLeft,
                                outerVecStartFull.Y + ty * profileLeft);

                            var outerVecEnd = new XPoint(
                                outerVecEndFull.X - tx * profileRight,
                                outerVecEndFull.Y - ty * profileRight);

                            if (isAlmostVertical)
                            {
                                Console.WriteLine($"🔷 T1/T1 element {i + 1}  ✨ isAlmostVertical={isAlmostVertical}");

                                const double eps = 0.1;

                                // ================= OUTER =================
                                double minYOuter = outer.Min(p => p.Y);
                                double maxYOuter = outer.Max(p => p.Y);
                                double minXOuter = outer.Min(p => p.X);
                                double maxXOuter = outer.Max(p => p.X);

                                var minOuter = outer
                                    .Where(p => Math.Abs(p.Y - minYOuter) < eps)
                                    .OrderBy(p => p.X)
                                    .FirstOrDefault();

                                var leftOuter = outer
                                    .Where(p => Math.Abs(p.Y - maxYOuter) < eps)
                                    .OrderBy(p => p.X)
                                    .FirstOrDefault();

                                var rightOuter = outer
                                    .Where(p => Math.Abs(p.Y - maxYOuter) < eps)
                                    .OrderByDescending(p => p.X)
                                    .FirstOrDefault();


                                // ================= INNER =================
                                double minYInner = inner.Min(p => p.Y);
                                double maxYInner = inner.Max(p => p.Y);
                                double minXInner = inner.Min(p => p.X);
                                double maxXInner = inner.Max(p => p.X);

                                var minInner = inner
                                    .Where(p => Math.Abs(p.Y - minYInner) < eps)
                                    .OrderBy(p => p.X)
                                    .FirstOrDefault();

                                var leftInner = inner
                                    .Where(p => Math.Abs(p.Y - maxYInner) < eps)
                                    .OrderBy(p => p.X)
                                    .FirstOrDefault();

                                var rightInner = inner
                                    .Where(p => Math.Abs(p.Y - maxYInner) < eps)
                                    .OrderByDescending(p => p.X)
                                    .FirstOrDefault();

                                // var prev = (i - 1 + vertexCount) % vertexCount;
                                var nextNext = (next + 1) % vertexCount;

                                outerVecStart = FindFirstEdgeIntersection(outerVecStart, tx, ty, inner);

                                outerVecEnd = FindFirstEdgeIntersection(outerVecEnd, -tx, -ty, inner);

                                // Przesunięcie do wnętrza
                                var innerVecStart = FindFirstEdgeIntersection(
                                    new XPoint(outerVecStart.X + nx * profile, outerVecStart.Y + ny * profile),
                                    tx, ty, inner);

                                var innerVecEnd = FindFirstEdgeIntersection(
                                    new XPoint(outerVecEnd.X + nx * profile, outerVecEnd.Y + ny * profile),
                                    tx, ty, inner);

                                if (vertexCount > 3 && angleDegrees < 90)
                                {
                                    Console.WriteLine($"🔷 T1/T1 🔷 isAlmostVertical: {isAlmostVertical} vertexCount > {vertexCount} && angleDegrees < {angleDegrees} for element {i + 1} with joins: {leftJoin}-{rightJoin} angleDegrees: {angleDegrees}");

                                    // 1️⃣ Dolny Y z inner
                                    float bottomY = (float)inner.Max(p => p.Y);

                                    // 2️⃣ Dolny punkt inner (prawy dół)
                                    innerVecStart = FindAxisIntersection(
                                        value: bottomY,
                                        direction: AxisDirection.Horizontal,
                                        contour: inner,
                                        pick: AxisPick.Max
                                    );

                                    // 3️⃣ Skos outer
                                    XPoint outerSkosStart = outer.First(p => p.Y == outer.Min(o => o.Y));
                                    XPoint outerSkosEnd = outer.First(p => p.Y == outer.Max(o => o.Y) && p.X > innerVecStart.X);

                                    // 4️⃣ Dolny punkt outer (przecięcie poziome)
                                    outerVecStart = GetHorizontalIntersection(
                                        outerSkosStart,
                                        outerSkosEnd,
                                        bottomY
                                    );

                                    outerVecEnd = outerSkosStart;

                                    double angleRad = (90 - angleDegrees) * Math.PI / 180.0;

                                    // pionowa składowa przesunięcia po skosie
                                    float deltaY = (float)(profile / Math.Tan(angleRad));

                                    innerVecEnd.Y = (float)inner.Min(p => p.Y) - deltaY;
                                    innerVecEnd.X = (float)outer.Min(p => p.X);
                                }

                                wierzcholki = new List<XPoint> {
                                outerVecStart, outerVecEnd, innerVecEnd, innerVecStart
                                };
                            }
                            else
                            {
                                // ✨ Korekcja styku z pionami T1 z lewej i prawej strony
                                Console.WriteLine($"🔷 T1/T1 element {i + 1}  ✨ Korekcja styku z pionami T1 z lewej i prawej strony angleDegrees: {angleDegrees} StronaElementu: {StronaElementu}");
                                //var prev = (i - 1 + vertexCount) % vertexCount;
                                // 🔷 Pionowe – pełne

                                var outerVecTop = FindFirstEdgeIntersection(outerStart, nx, ny, outer);
                                var outerVecBottom = FindFirstEdgeIntersection(outerEnd, nx, ny, outer);

                                var innerVecTop = FindFirstEdgeIntersection(
                                    new XPoint(outerVecTop.X + nx * profile, outerVecTop.Y + ny * profile),
                                    tx, ty, outer);

                                var innerVecBottom = FindFirstEdgeIntersection(
                                    new XPoint(outerVecBottom.X + nx * profile, outerVecBottom.Y + ny * profile),
                                    tx, ty, outer);

                                const double eps = 0.01;

                                // ================= OUTER =================
                                double minYOuter = outer.Min(p => p.Y);
                                double maxYOuter = outer.Max(p => p.Y);
                                double minXOuter = outer.Min(p => p.X);
                                double maxXOuter = outer.Max(p => p.X);

                                var minOuter = outer
                                    .Where(p => Math.Abs(p.Y - minYOuter) < eps)
                                    .OrderBy(p => p.X)
                                    .FirstOrDefault();

                                var leftOuter = outer
                                    .Where(p => Math.Abs(p.Y - maxYOuter) < eps)
                                    .OrderBy(p => p.X)
                                    .FirstOrDefault();

                                var rightOuter = outer
                                    .Where(p => Math.Abs(p.Y - maxYOuter) < eps)
                                    .OrderByDescending(p => p.X)
                                    .FirstOrDefault();


                                // ================= INNER =================
                                double minYInner = inner.Min(p => p.Y);
                                double maxYInner = inner.Max(p => p.Y);
                                double minXInner = inner.Min(p => p.X);
                                double maxXInner = inner.Max(p => p.X);

                                var minInner = inner
                                    .Where(p => Math.Abs(p.Y - minYInner) < eps)
                                    .OrderBy(p => p.X)
                                    .FirstOrDefault();

                                var maxInner = inner
                                    .Where(p => Math.Abs(p.Y - maxYInner) < eps)
                                    .OrderByDescending(p => p.X)
                                    .FirstOrDefault();


                                var leftInner = inner
                                    .Where(p => Math.Abs(p.Y - maxYInner) < eps)
                                    .OrderBy(p => p.X)
                                    .FirstOrDefault();

                                var rightInner = inner
                                    .Where(p => Math.Abs(p.Y - maxYInner) < eps)
                                    .OrderByDescending(p => p.X)
                                    .FirstOrDefault();

                                var key = (
                                    vertexCount,
                                    angleGt90: angleDegrees > 90,
                                    angleLt90: angleDegrees < 90,
                                    sameOuterX: Math.Round(minOuter.X, 0) == Math.Round(leftOuter.X, 0),
                                    sameInnerX: Math.Round(minInner.X, 0) == Math.Round(leftInner.X, 0),
                                    StronaElementu
                                );

                                Console.WriteLine(
                                                    $"[T1/T1 KEY] " +
                                                    $"vertexCount={key.vertexCount}, " +
                                                    $"angleGt90={key.angleGt90}, " +
                                                    $"angleLt90={key.angleLt90}, " +
                                                    $"sameOuterX={key.sameOuterX}, " +
                                                    $"sameInnerX={key.sameInnerX}, " +
                                                    $"StronaElementu='{key.StronaElementu}'"
                                                );


                                switch (key)
                                {
                                    // =========================================================
                                    // vertexCount > 3 && StronaElementu == "Lewa"
                                    //// =========================================================
                                    case (3, true, false, false, false, _):
                                        {
                                            Console.WriteLine("[T1/T1 KEY] - 0");
                                            float bottomY = (float)inner.Max(p => p.Y);

                                            innerVecTop = FindAxisIntersection(
                                                value: bottomY,
                                                direction: AxisDirection.Horizontal,
                                                contour: inner,
                                                pick: AxisPick.Min
                                            );

                                            outerVecTop = GetHorizontalIntersection(minOuter, leftOuter, bottomY);

                                            if (Math.Round(rightInner.X, 0) != Math.Round(leftInner.X, 0))
                                            {
                                                outerVecTop = FindFirstEdgeIntersectionByAngle(leftInner, 180, outer);
                                            }
                                            break;
                                        }

                                    // =========================================================
                                    // vertexCount == 3 && angleDegrees > 90 && minOuter.X == leftOuter.X
                                    // =========================================================
                                    case (3, true, _, true, _, _):
                                        {
                                            Console.WriteLine("[T1/T1 KEY] - 1");
                                            float bottomY = (float)inner.Max(p => p.Y);

                                            innerVecTop = FindAxisIntersection(
                                                value: bottomY,
                                                direction: AxisDirection.Horizontal,
                                                contour: inner,
                                                pick: AxisPick.Min
                                            );

                                            outerVecTop = GetHorizontalIntersection(minOuter, leftOuter, bottomY);

                                            if (Math.Round(rightInner.X, 0) != Math.Round(leftInner.X, 0))
                                            {
                                                outerVecTop = FindFirstEdgeIntersectionByAngle(leftInner, 180, outer);
                                            }
                                            break;
                                        }

                                    // =========================================================
                                    // vertexCount == 3 && angleDegrees < 90 && minInner.X == leftInner.X
                                    // =========================================================
                                    case (3, _, true, _, true, _):
                                        {
                                            Console.WriteLine("[T1/T1 KEY] - 2");
                                            float bottomY = (float)inner.Max(p => p.Y);

                                            innerVecTop = FindAxisIntersection(
                                                value: bottomY,
                                                direction: AxisDirection.Horizontal,
                                                contour: inner,
                                                pick: AxisPick.Max
                                            );

                                            XPoint outerSkosStart = outer.First(p => p.Y == outer.Min(o => o.Y));
                                            XPoint outerSkosEnd = outer.First(
                                                p => p.Y == outer.Max(o => o.Y) && p.X > innerVecTop.X
                                            );

                                            outerVecTop = GetHorizontalIntersection(
                                                outerSkosStart,
                                                outerSkosEnd,
                                                bottomY
                                            );

                                            outerVecBottom = outerSkosStart;

                                            double angleRad = (90 - angleDegrees) * Math.PI / 180.0;
                                            float deltaY = (float)(profile / Math.Tan(angleRad));

                                            innerVecBottom.Y = (float)inner.Min(p => p.Y) - deltaY;
                                            innerVecBottom.X = (float)outer.Min(p => p.X);
                                            break;
                                        }

                                    // =========================================================
                                    // vertexCount == 3 && angleDegrees < 90 && minInner.X != leftInner.X
                                    // =========================================================
                                    case (3, _, true, _, false, _):
                                        {
                                            Console.WriteLine("[T1/T1 KEY] - 3");
                                            innerVecTop = minInner;
                                            outerVecTop = FindFirstEdgeIntersectionByAngle(
                                                innerVecTop,
                                                360 - angleDegrees,
                                                outer
                                            );
                                            break;
                                        }

                                    // =========================================================
                                    // vertexCount > 4 && angleDegrees < 90
                                    // =========================================================
                                    case ( > 4, _, true, _, _, _):
                                        {
                                            Console.WriteLine("[T1/T1 KEY] - 4");

                                            float bottomY = (float)inner.Max(p => p.Y);

                                            innerVecTop = FindAxisIntersection(
                                                value: bottomY,
                                                direction: AxisDirection.Horizontal,
                                                contour: inner,
                                                pick: AxisPick.Max
                                            );

                                            XPoint outerSkosStart = outer.First(p => p.Y == outer.Min(o => o.Y));
                                            XPoint outerSkosEnd = outer.First(
                                                p => p.Y == outer.Max(o => o.Y) && p.X > innerVecTop.X
                                            );

                                            outerVecTop = GetHorizontalIntersection(
                                                outerSkosStart,
                                                outerSkosEnd,
                                                bottomY
                                            );

                                            outerVecBottom = outerSkosStart;

                                            double angleRad = (90 - angleDegrees) * Math.PI / 180.0;
                                            float deltaY = (float)(profile / Math.Tan(angleRad));

                                            innerVecBottom.Y = (float)inner.Min(p => p.Y) - deltaY;
                                            innerVecBottom.X = (float)outer.Min(p => p.X);
                                            break;
                                        }
                                }

                                wierzcholki = new List<XPoint> {
                                outerVecTop, outerVecBottom, innerVecBottom, innerVecTop
                                };

                            }

                        }

                    }

                }
                else if (leftJoin == "T3" && rightJoin == "T3")
                {
                    Console.WriteLine($"🔷 T3/T3 element {i + 1} isAlmostHorizontal: {isAlmostHorizontal} isAlmostVertical: {isAlmostVertical} angleDegrees: {angleDegrees}");

                    if (isAlmostVertical)
                    {
                        // 🔷 Pionowe – pełne
                        var outerVecTop = FindFirstEdgeIntersection(outerStart, nx, ny, outer);
                        var outerVecBottom = FindFirstEdgeIntersection(outerEnd, nx, ny, outer);

                        var innerVecTop = FindFirstEdgeIntersection(
                            new XPoint(outerVecTop.X + nx * profile, outerVecTop.Y + ny * profile),
                            tx, ty, outer);

                        var innerVecBottom = FindFirstEdgeIntersection(
                            new XPoint(outerVecBottom.X + nx * profile, outerVecBottom.Y + ny * profile),
                            tx, ty, outer);

                        wierzcholki = new List<XPoint> {
                            outerVecTop, outerVecBottom, innerVecBottom, innerVecTop
                        };
                    }
                    else
                    {
                        // Przecięcia "normalne"
                        var outerVecStartFull = FindFirstEdgeIntersection(outerStart, nx, ny, outer);
                        var outerVecEndFull = FindFirstEdgeIntersection(outerEnd, nx, ny, outer);

                        // ✂️ Skrócenie o profile pionowe
                        var outerVecStart = new XPoint(
                            outerVecStartFull.X + tx * profileLeft,
                            outerVecStartFull.Y + ty * profileLeft);

                        var outerVecEnd = new XPoint(
                            outerVecEndFull.X - tx * profileRight,
                            outerVecEndFull.Y - ty * profileRight);

                        if (isAlmostHorizontal)
                        {
                            // ✨ Korekcja styku z pionami T3 z lewej i prawej strony
                            //var prev = (i - 1 + vertexCount) % vertexCount;
                            var nextNext = (next + 1) % vertexCount;

                            outerVecStart = FindFirstEdgeIntersection(outerVecStart, tx, ty, inner);

                            outerVecEnd = FindFirstEdgeIntersection(outerVecEnd, -tx, -ty, inner);

                            // Przesunięcie do wnętrza
                            var innerVecStart = FindFirstEdgeIntersection(
                                new XPoint(outerVecStart.X + nx * profile, outerVecStart.Y + ny * profile),
                                tx, ty, inner);

                            var innerVecEnd = FindFirstEdgeIntersection(
                                new XPoint(outerVecEnd.X + nx * profile, outerVecEnd.Y + ny * profile),
                                tx, ty, inner);

                            wierzcholki = new List<XPoint> {
                            outerVecStart, outerVecEnd, innerVecEnd, innerVecStart
                            };
                        }
                        else
                        {
                            // ✨ Korekcja styku z pionami T3 z lewej i prawej strony

                            // var prev = (i - 1 + vertexCount) % vertexCount;
                            var nextNext = (next + 1) % vertexCount;

                            const double eps = 1.0; // ZWIĘKSZ EPSILON! 0.001 jest za mały dla Twoich danych

                            // ================= OUTER =================
                            double minYOuter = outer.Min(p => p.Y);
                            double maxYOuter = outer.Max(p => p.Y);

                            XPoint minOuter = new XPoint();
                            XPoint leftOuter = new XPoint();
                            XPoint rightOuter = new XPoint();
                            XPoint rightTopOuter = new XPoint();

                            // Dla OUTER: punkty mają Y: 338,11 (góra) i 943,67 (dół)

                            // Punkty GÓRNE outer (najmniejsze Y z tolerancją)
                            var topPointsOuter = outer.Where(p => p.Y <= minYOuter + eps).ToList();
                            //Console.WriteLine($"🔷 topPointsOuter znaleziono {topPointsOuter.Count} punktów:");
                            //foreach (var p in topPointsOuter)
                            //{
                            //    Console.WriteLine($"   ({p.X:F2}, {p.Y:F2})");
                            //}

                            if (topPointsOuter.Count >= 2)
                            {
                                minOuter = topPointsOuter.OrderBy(p => p.X).First();
                                rightTopOuter = topPointsOuter.OrderByDescending(p => p.X).First();
                            }
                            else if (topPointsOuter.Count == 1)
                            {
                                minOuter = topPointsOuter.First();
                                // Szukaj drugiego punktu górnego - niekoniecznie z DOKŁADNIE tym samym Y
                                var otherTopOuter = outer.Where(p => p.Y <= minYOuter + eps * 5) // Szersza tolerancja
                                                       .Where(p => Math.Abs(p.X - minOuter.X) > eps) // Inny X
                                                       .OrderByDescending(p => p.X)
                                                       .FirstOrDefault();

                                if (otherTopOuter.X == 0 && otherTopOuter.Y == 0)
                                {
                                    // Jeśli nie znaleziono, weź punkt z drugim najmniejszym Y
                                    otherTopOuter = outer.OrderBy(p => p.Y)
                                                       .Skip(1)
                                                       .FirstOrDefault();
                                }
                                rightTopOuter = otherTopOuter;
                            }

                            // Punkty DOLNE outer (największe Y)
                            var bottomPointsOuter = outer.Where(p => p.Y >= maxYOuter - eps).ToList();
                            //Console.WriteLine($"🔷 bottomPointsOuter znaleziono {bottomPointsOuter.Count} punktów:");

                            if (bottomPointsOuter.Count >= 2)
                            {
                                leftOuter = bottomPointsOuter.OrderBy(p => p.X).First();
                                rightOuter = bottomPointsOuter.OrderByDescending(p => p.X).First();
                            }
                            else if (bottomPointsOuter.Count == 1)
                            {
                                leftOuter = bottomPointsOuter.First();
                                // Szukaj drugiego dolnego punktu
                                var otherBottomOuter = outer.Where(p => p.Y >= maxYOuter - eps * 5)
                                                          .Where(p => Math.Abs(p.X - leftOuter.X) > eps)
                                                          .OrderByDescending(p => p.X)
                                                          .FirstOrDefault();

                                rightOuter = (otherBottomOuter.X == 0 && otherBottomOuter.Y == 0)
                                    ? outer.OrderByDescending(p => p.Y).Skip(1).FirstOrDefault()
                                    : otherBottomOuter;
                            }


                            // ================= INNER =================
                            double minYInner = inner.Min(p => p.Y);
                            double maxYInner = inner.Max(p => p.Y);

                            XPoint minInner = new XPoint();
                            XPoint minRightInner = new XPoint();
                            XPoint leftInner = new XPoint();
                            XPoint rightInner = new XPoint();

                            // ZWIĘKSZ TOLERANCJĘ dla inner!
                            float innerTopTolerance = 50.0f; // Dopuszczamy różnicę 50 jednostek

                            var topPointsInner = inner.Where(p => p.Y <= minYInner + innerTopTolerance).ToList();

                            if (topPointsInner.Count >= 2)
                            {
                                // Posortuj po X dla górnych punktów
                                topPointsInner = topPointsInner.OrderBy(p => p.X).ToList();
                                minInner = topPointsInner.First();
                                minRightInner = topPointsInner.Last();
                            }
                            else if (topPointsInner.Count == 1)
                            {
                                minInner = topPointsInner.First();

                                // Drugi punkt górny: albo z podobnym Y, albo drugi najmniejszy Y
                                var potentialTopPoints = inner.Where(p => p.Y <= minYInner + innerTopTolerance * 2)
                                                            .Where(p => Math.Abs(p.X - minInner.X) > 1.0)
                                                            .OrderBy(p => p.Y)
                                                            .ThenByDescending(p => p.X)
                                                            .ToList();

                                if (potentialTopPoints.Any())
                                {
                                    minRightInner = potentialTopPoints.First();
                                }
                                else
                                {
                                    // Jako ostatnią deskę ratunku: weź punkt z drugim najmniejszym Y
                                    minRightInner = inner.OrderBy(p => p.Y)
                                                       .Skip(1)
                                                       .FirstOrDefault();
                                }
                            }

                            // Punkty DOLNE inner
                            var bottomPointsInner = inner.Where(p => p.Y >= maxYInner - eps).ToList();
                            // Console.WriteLine($"🔷 bottomPointsInner znaleziono {bottomPointsInner.Count} punktów:");

                            if (bottomPointsInner.Count >= 2)
                            {
                                var sortedBottom = bottomPointsInner.OrderBy(p => p.X).ToList();
                                leftInner = sortedBottom.First();
                                rightInner = sortedBottom.Last();
                            }
                            else if (bottomPointsInner.Count == 1)
                            {
                                leftInner = bottomPointsInner.First();

                                var otherBottomInner = inner.Where(p => p.Y >= maxYInner - eps * 5)
                                                          .Where(p => Math.Abs(p.X - leftInner.X) > eps)
                                                          .OrderByDescending(p => p.X)
                                                          .FirstOrDefault();

                                rightInner = (otherBottomInner.X == 0 && otherBottomInner.Y == 0)
                                    ? inner.OrderByDescending(p => p.Y).Skip(1).FirstOrDefault()
                                    : otherBottomInner;
                            }

                            outerVecStart = FindFirstEdgeIntersection(outerVecStart, tx, ty, outer);

                            outerVecEnd = FindFirstEdgeIntersection(outerVecEnd, -tx, -ty, outer);

                            // Przesunięcie do wnętrza
                            var innerVecStart = FindFirstEdgeIntersection(
                                new XPoint(outerVecStart.X + nx * profile, outerVecStart.Y + ny * profile),
                                tx, ty, outer);

                            var innerVecEnd = FindFirstEdgeIntersection(
                                new XPoint(outerVecEnd.X + nx * profile, outerVecEnd.Y + ny * profile),
                                tx, ty, outer);

                            if (vertexCount == 3 && angleDegrees < 90 && Math.Round(minOuter.X, 0) == Math.Round(leftOuter.X, 0))
                            {
                                // 1️⃣ Punkt bazowy na outer (lewy górny np.)
                                outerVecEnd = minInner;

                                outerVecStart = FindFirstEdgeIntersectionByAngle(outerVecEnd, 270, outer);

                                innerVecStart = rightOuter;

                                Console.WriteLine($"🔷 innerVecStart (przeciwny bok): X={minOuter.X}, Y={leftOuter.X}");
                            }

                            if (vertexCount == 3 && angleDegrees > 90 && Math.Round(minOuter.X, 0) == Math.Round(rightOuter.X, 0))
                            {
                                float bottomY = (float)outer.Max(p => p.Y);
                                //TU SKONCZYĆ
                                outerVecStart = GetHorizontalIntersection(
                                    innerVecEnd,
                                    outerVecEnd,
                                    bottomY
                                );

                                outerVecStart.X = (float)outer.Min(p => p.X);

                                innerVecEnd = minInner;

                                outerVecEnd = FindFirstEdgeIntersectionByAngle(innerVecEnd, 270, outer);
                            }
                            else if (vertexCount == 3 && angleDegrees > 90 && Math.Round(minOuter.X, 0) != Math.Round(rightOuter.X, 0))
                            {
                                outerVecStart = leftOuter;

                                innerVecEnd = minInner;

                                outerVecEnd = FindFirstEdgeIntersectionByAngle(innerVecEnd, 180 - angleDegrees, outer);
                            }
                            else if (vertexCount == 3 && angleDegrees < 90 && Math.Round(minOuter.X, 0) != Math.Round(leftOuter.X, 0))
                            {
                                outerVecStart = minOuter;

                                outerVecEnd = rightOuter;
                            }
                            else if (vertexCount > 3 && angleDegrees > 90)
                            {

                                innerVecStart = minInner;
                                outerVecStart = FindFirstEdgeIntersectionByAngle(innerVecStart, 270, outer);
                                innerVecEnd = minRightInner;
                                outerVecEnd = FindFirstEdgeIntersectionByAngle(innerVecEnd, 270, outer);

                                //Console.WriteLine($"🔷 vertexCount: {vertexCount} angleDegrees: {angleDegrees} outerVecStart.X/Y: {outerVecStart.X}/{outerVecStart.Y}, " +
                                //    $"outerVecEnd.X/Y: {outerVecEnd.X}/{outerVecEnd.Y}, innerVecEnd.X/Y: {innerVecEnd.X}/{innerVecEnd.Y}, innerVecStart.X/Y: {innerVecStart.X}/{innerVecStart.Y}");
                            }
                            else if (vertexCount > 3 && angleDegrees < 90)
                            {

                                innerVecStart = minInner;
                                outerVecStart = FindFirstEdgeIntersectionByAngle(innerVecStart, 270, outer);
                                innerVecEnd = minRightInner;
                                outerVecEnd = FindFirstEdgeIntersectionByAngle(innerVecEnd, 270, outer);

                                //Console.WriteLine($"🔷 vertexCount: {vertexCount} angleDegrees: {angleDegrees} outerVecStart.X/Y: {outerVecStart.X}/{outerVecStart.Y}, " +
                                //    $"outerVecEnd.X/Y: {outerVecEnd.X}/{outerVecEnd.Y}, innerVecEnd.X/Y: {innerVecEnd.X}/{innerVecEnd.Y}, innerVecStart.X/Y: {innerVecStart.X}/{innerVecStart.Y}");
                            }

                            wierzcholki = new List<XPoint> {
                            outerVecStart, outerVecEnd, innerVecEnd, innerVecStart
                            };

                            Console.WriteLine($"🔷 T3/T3 element {i + 1} ✨ Korekcja styku z pionami T3 z lewej i prawej strony angleDegrees: {angleDegrees} innerVecStart.X: {innerVecStart.X}, innerVecStart.Y: {innerVecStart.Y} outerVecEnd.X: {outerVecEnd.X}, outerVecEnd.Y: {outerVecEnd.Y}");
                        }

                    }

                }
                else if (leftJoin == "T2" && rightJoin == "T2")
                {
                    // Console.WriteLine($"🔷 T2/T2 element {i + 1} - styczne ścięcia pod kątem");
                    // Przecięcia z konturem na bazie normalnej
                    var outerVecStart = FindFirstEdgeIntersection(outerStart, nx, ny, outer);
                    var outerVecEnd = FindFirstEdgeIntersection(outerEnd, nx, ny, outer);

                    var _innerVecStart = FindFirstEdgeIntersection(_innerStart, nx, ny, outer);
                    var _innerVecEnd = FindFirstEdgeIntersection(_innerEnd, nx, ny, outer);

                    var innerVecStart = FindFirstEdgeIntersection(
                        new XPoint(_innerVecStart.X + nx * profile, _innerVecStart.Y + ny * profile),
                        tx, ty, inner);

                    var innerVecEnd = FindFirstEdgeIntersection(
                        new XPoint(_innerVecEnd.X + nx * profile, _innerVecEnd.Y + ny * profile),
                        tx, ty, inner);

                    wierzcholki = new List<XPoint> {
                            outerVecStart, outerVecEnd, innerVecEnd, innerVecStart
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

                    Console.WriteLine($"🔷 🔷🔷 T5-T5 TopXT5.X/Y: {TopXT5.X}/{TopXT5.Y}");
                    Console.WriteLine($"🔷 🔷🔷 T5-T5 BottomXT5.X/Y: {BottomXT5.X}/{BottomXT5.Y}");
                    Console.WriteLine($"🔷 🔷🔷 T5-T5 tmpTopST5.X/Y: {tmpTopST5.X}/{tmpTopST5.Y}");
                    Console.WriteLine($"🔷 🔷🔷 T5-T5 tmpTopLT5.X/Y: {tmpTopLT5.X}/{tmpTopLT5.Y}");
                    Console.WriteLine($"🔷 🔷🔷 T5-T5 tmpTopRT5.X/Y: {tmpTopRT5.X}/{tmpTopRT5.Y}");
                    Console.WriteLine($"🔷 🔷🔷 T5-T5 midTopIntersection.X/Y: {midTopIntersection.X}/{midTopIntersection.Y}");
                    Console.WriteLine($"🔷 🔷🔷 T5-T5 midBottomIntersection.X/Y: {midBottomIntersection.X}/{midBottomIntersection.Y}");
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
                else
                {
                    Console.WriteLine($"🔷 Default case for element {i + 1} with joins: {leftJoin}-{rightJoin}");

                    // Przecięcia z konturem na bazie normalnej
                    var outerVecStart = FindFirstEdgeIntersection(outerStart, nx, ny, outer);
                    var outerVecEnd = FindFirstEdgeIntersection(outerEnd, nx, ny, outer);

                    var _innerVecStart = FindFirstEdgeIntersection(_innerStart, nx, ny, outer);
                    var _innerVecEnd = FindFirstEdgeIntersection(_innerEnd, nx, ny, outer);

                    var innerVecStart = FindFirstEdgeIntersection(
                        new XPoint(_innerVecStart.X + nx * profile, _innerVecStart.Y + ny * profile),
                        tx, ty, inner);

                    var innerVecEnd = FindFirstEdgeIntersection(
                        new XPoint(_innerVecEnd.X + nx * profile, _innerVecEnd.Y + ny * profile),
                        tx, ty, inner);

                    wierzcholki = new List<XPoint> {
                            outerVecStart, outerVecEnd, innerVecEnd, innerVecStart
                        };
                }

                double regionMinX = wierzcholki.Min(p => p.X);
                double regionMaxX = wierzcholki.Max(p => p.X);
                double regionMinY = wierzcholki.Min(p => p.Y);
                double regionMaxY = wierzcholki.Max(p => p.Y);

                int wartoscX = (int)Math.Round(regionMaxX - regionMinX);
                int wartoscY = (int)Math.Round(regionMaxY - regionMinY);

                // Console.WriteLine($"leftJoin: {leftJoin} rightJoin:{rightJoin} wierzcholki: {wierzcholki.Count()} isAlmostVertical:{isAlmostVertical}");
                float bazowaDlugosc = ObliczDlugoscElementu(wierzcholki, angleDegrees);

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
                        TypKsztaltu = typKsztalt,
                        Wierzcholki = wierzcholki,
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
                    });

                Console.WriteLine($"▶️ Element {i + 1}/{vertexCount} dodałem do ElementyRamyRysowane. Total elements now: {ElementyRamyRysowane.Count} - >3 rowIdProfil:{rowIdProfil} Angle: {angleDegrees}°");

                await Task.CompletedTask;

                if (ElementLiniowy) return true;

            }

            return true;
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

        private float ObliczDlugoscElementu(List<XPoint> wierzcholki, float kat)
        {
            double dx = Math.Abs(wierzcholki[1].X - wierzcholki[0].X);
            double dy = Math.Abs(wierzcholki[1].Y - wierzcholki[0].Y);

            double dlugosc = Math.Sqrt(dx * dx + dy * dy);

            Console.WriteLine($"▶️ Calculating length for element with dx: {dx}, dy: {dy}, kat: {kat}° dlugosc: {dlugosc}");

            return (float)Math.Round(dlugosc, 2);
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

        private XPoint FindEdgeIntersectionByLineForTriangle(
        XPoint basePoint,
        XPoint dirStart,
        XPoint dirEnd,
        List<XPoint> points,      // zmieniona nazwa z triangle na points
        bool forward = true,
        double eps = 1e-6)
        {
            if (points == null || points.Count < 2)
                throw new ArgumentException("Points list must contain at least 2 points");

            // kierunek
            double dx = dirEnd.X - dirStart.X;
            double dy = dirEnd.Y - dirStart.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len < eps) len = 1;

            dx /= len;
            dy /= len;

            if (!forward)
            {
                dx = -dx;
                dy = -dy;
            }

            List<(double t, XPoint p)> hits = new();

            // Dla trójkąta (3 punkty) - zamykamy pętlę
            if (points.Count == 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    XPoint a = points[i];
                    XPoint b = points[(i + 1) % 3];

                    if (TryIntersectLineWithSegment(
                            basePoint, dx, dy,
                            a, b,
                            out double t,
                            out XPoint hit,
                            eps))
                    {
                        hits.Add((t, hit));
                    }
                }
            }
            else // Dla linii (2 punkty) lub dowolnej listy punktów - łączymy po kolei
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    XPoint a = points[i];
                    XPoint b = points[i + 1];

                    if (TryIntersectLineWithSegment(
                            basePoint, dx, dy,
                            a, b,
                            out double t,
                            out XPoint hit,
                            eps))
                    {
                        hits.Add((t, hit));
                    }
                }

                // Opcjonalnie: jeśli chcesz zamknąć pętlę (dla wielokątów)
                // if (points.Count > 2)
                // {
                //     XPoint a = points[points.Count - 1];
                //     XPoint b = points[0];
                //     
                //     if (TryIntersectLineWithSegment(
                //             basePoint, dx, dy,
                //             a, b,
                //             out double t,
                //             out XPoint hit,
                //             eps))
                //     {
                //         hits.Add((t, hit));
                //     }
                // }
            }

            if (hits.Count == 0)
                return new XPoint(-1, -1);

            // sortujemy po parametrze linii
            hits.Sort((x, y) => x.t.CompareTo(y.t));

            // punkt "przed" i "za" basePoint
            if (forward)
            {
                // najmniejsze t > 0
                foreach (var h in hits)
                    if (h.t > eps)
                        return h.p;
            }
            else
            {
                // największe t < 0
                for (int i = hits.Count - 1; i >= 0; i--)
                    if (hits[i].t < -eps)
                        return hits[i].p;
            }

            // fallback: najbliższy
            return hits[0].p;
        }

        private bool TryIntersectLineWithSegment(
        XPoint p, double dx, double dy,   // linia: p + t·d
        XPoint a, XPoint b,               // odcinek AB
        out double t,
        out XPoint hit,
        double eps)
        {
            hit = default;
            t = 0;

            double sx = b.X - a.X;
            double sy = b.Y - a.Y;

            double denom = dx * sy - dy * sx;

            // równoległe
            if (Math.Abs(denom) < eps)
                return false;

            double qpx = a.X - p.X;
            double qpy = a.Y - p.Y;

            t = (qpx * sy - qpy * sx) / denom;
            double u = (qpx * dy - qpy * dx) / denom;

            if (u < -eps || u > 1 + eps)
                return false;

            hit = new XPoint(
                p.X + t * dx,
                p.Y + t * dy
            );

            return true;
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
        public static List<XPoint> OffsetPolygonInside(List<XPoint> polygon, double offset)
        {
            if (polygon == null || polygon.Count < 3)
                throw new ArgumentException("Polygon must have at least 3 points.");

            // Sprawdzenie kierunku konturu (CW/CCW)
            bool isClockwise = IsPolygonClockwise(polygon);

            int n = polygon.Count;
            var offsetPolygon = new List<XPoint>(n);

            for (int i = 0; i < n; i++)
            {
                // Poprzedni i następny punkt
                XPoint p0 = polygon[(i - 1 + n) % n];
                XPoint p1 = polygon[i];
                XPoint p2 = polygon[(i + 1) % n];

                // Wektory krawędzi
                XPoint v1 = new XPoint(p1.X - p0.X, p1.Y - p0.Y);
                XPoint v2 = new XPoint(p2.X - p1.X, p2.Y - p1.Y);

                // Normalne do krawędzi (prostopadłe, jednostkowe)
                XPoint n1 = Normalize(new XPoint(-v1.Y, v1.X));
                XPoint n2 = Normalize(new XPoint(-v2.Y, v2.X));

                // Jeśli kontur CW, normalne odwracamy
                if (isClockwise)
                {
                    n1 = new XPoint(-n1.X, -n1.Y);
                    n2 = new XPoint(-n2.X, -n2.Y);
                }

                // Średnia normalnych dla punktu
                XPoint nAvg = new XPoint((n1.X + n2.X) / 2, (n1.Y + n2.Y) / 2);
                nAvg = Normalize(nAvg);

                // Przesunięcie punktu w kierunku normalnej
                XPoint pOffset = new XPoint(
                    p1.X + nAvg.X * offset,
                    p1.Y + nAvg.Y * offset
                );

                offsetPolygon.Add(pOffset);
            }

            return offsetPolygon;
        }

        // Funkcja pomocnicza: jednostkowy wektor
        private static XPoint Normalize(XPoint v)
        {
            double len = Math.Sqrt(v.X * v.X + v.Y * v.Y);
            if (len < 1e-9) return new XPoint(0, 0);
            return new XPoint(v.X / len, v.Y / len);
        }

        // Funkcja pomocnicza: sprawdza czy kontur jest CW
        private static bool IsPolygonClockwise(List<XPoint> poly)
        {
            double sum = 0;
            int n = poly.Count;
            for (int i = 0; i < n; i++)
            {
                XPoint p1 = poly[i];
                XPoint p2 = poly[(i + 1) % n];
                sum += (p2.X - p1.X) * (p2.Y + p1.Y);
            }
            return sum > 0;
        }

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

        private XPoint GetVerticalIntersection(XPoint a, XPoint b, float x)
        {
            if (Math.Abs(a.X - b.X) < 1e-3f)
                return new XPoint(x, a.Y);

            float t = (x - (float)a.X) / ((float)b.X - (float)a.X);
            float y = (float)a.Y + t * ((float)b.Y - (float)a.Y);
            return new XPoint(x, y);
        }
        private float GetJoinOffset(string joinType, float profile)
        {
            return joinType switch
            {
                "T1" => 0f,
                "T2" => profile * 0.5f,
                "T3" => profile,
                "T4" => -profile * 0.5f,
                _ => 0f
            };
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
                Console.WriteLine($"🔷 Calculating offset polygon for {count} X:{points[0].X} Y:{points[0].Y} elementLiniowy:{elementLiniowy} points with profiles L:{profileLeft}, R:{profileRight}, T:{profileTop}, B:{profileBottom}");

            if (count < 2)
                throw new ArgumentException("Figura musi mieć co najmniej 2 punkty.");

            // 🟢 OBSŁUGA ELEMENTÓW LINIOWYCH (np. słupków)
            if (elementLiniowy)
            {
                var p1 = points[0];
                var p2 = points[1];

                float dx = (float)(p2.X - p1.X);
                float dy = (float)(p2.Y - p1.Y);
                float length = MathF.Sqrt(dx * dx + dy * dy);
                if (length < 1e-6f) return points;

                // jednostkowy wektor kierunku i normalna
                float tx = dx / length;
                float ty = dy / length;
                float nx = -ty;
                float ny = tx;

                const float EPS = 1e-5f;
                bool isHorizontal = Math.Abs(dy) < EPS;
                bool isVertical = Math.Abs(dx) < EPS;

                float offsetX = 0f;
                float offsetY = 0f;

                Console.WriteLine($"🔷 Calculating isHorizontal: {isHorizontal} isVertical: {isVertical}");

                if (isHorizontal)
                {
                    // Linia pozioma – przesuwamy tylko w osi Y
                    offsetX = 0f;  // 🔹 brak przesunięcia w osi X
                    offsetY = dy >= 0 ? profileTop : -profileBottom;

                    Console.WriteLine($"🔷 Calculating 🔷 Horizontal element → offset only in Y: offsetY={offsetY}");
                }
                else if (isVertical)
                {
                    // Linia pionowa – przesuwamy tylko w osi X
                    offsetY = 0f;
                    offsetX = dx >= 0 ? profileRight : -profileLeft;

                    Console.WriteLine($"🔷 Calculating 🔷 Vertical element → offset only in Y: offsetX={offsetX}");
                }
                else
                {
                    // Skośna — kombinacja proporcjonalna
                    offsetX = nx >= 0 ? profileRight : -profileLeft;
                    offsetY = ny >= 0 ? profileTop : -profileBottom;
                }

                // przesuwamy linię o składniki X i Y niezależnie
                var p1Offset = new XPoint(p1.X + offsetX, p1.Y + offsetY);
                var p2Offset = new XPoint(p2.X + offsetX, p2.Y + offsetY);

                Console.WriteLine($"🔷 Calculating 🔷 Offset liniowy p1Offset: X={p1Offset.X}, Y={p1Offset.Y} | p2Offset: X={p2Offset.X}, Y={p2Offset.Y} | isHorizontal={isHorizontal}");

                return new List<XPoint> { p1Offset, p2Offset };
            }


            // 🟢 OBSŁUGA WIELOKĄTA (oryginalna logika)
            if (count < 3)
                throw new ArgumentException("Wielokąt musi mieć co najmniej 3 punkty.");

            float minX = (float)points.Min(p => p.X);
            float maxX = (float)points.Max(p => p.X);
            float minY = (float)points.Min(p => p.Y);
            float maxY = (float)points.Max(p => p.Y);

            var offsetLines = new List<(XPoint p1, XPoint p2)>();

            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                var p1 = points[i];
                var p2 = points[next];

                float dx = (float)(p2.X - p1.X);
                float dy = (float)(p2.Y - p1.Y);
                float length = MathF.Sqrt(dx * dx + dy * dy);
                if (length < 1e-6f) continue;

                float tx = dx / length;
                float ty = dy / length;
                float nx = -ty;
                float ny = tx;

                float midX = ((float)p1.X + (float)p2.X) / 2f;
                float midY = ((float)p1.Y + (float)p2.Y) / 2f;

                float offset = 0f;
                bool isHorizontal = Math.Abs(dy) < Math.Abs(dx);

                if (isHorizontal)
                {
                    offset = midY < (minY + maxY) / 2f ? profileTop : profileBottom;
                }
                else
                {
                    offset = midX < (minX + maxX) / 2f ? profileLeft : profileRight;
                }

                var p1Offset = new XPoint(p1.X + nx * offset, p1.Y + ny * offset);
                var p2Offset = new XPoint(p2.X + nx * offset, p2.Y + ny * offset);

                offsetLines.Add((p1Offset, p2Offset));
            }

            var result = new List<XPoint>();
            for (int i = 0; i < offsetLines.Count; i++)
            {
                var (a1, a2) = offsetLines[i];
                var (b1, b2) = offsetLines[(i - 1 + offsetLines.Count) % offsetLines.Count];

                var intersection = GetLinesIntersection(a1, a2, b1, b2);

                if (float.IsNaN((float)intersection.X) || float.IsNaN((float)intersection.Y))
                {
                    intersection = new XPoint((a1.X + b1.X) / 2f, (a1.Y + b1.Y) / 2f);
                }

                result.Add(intersection);
            }

            foreach (var pt in result)
            {
                Console.WriteLine($"🔷 Calculated offset polygon point: X={pt.X}, Y={pt.Y}");
            }

            return result;
        }

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
        public double DlugoscElementu(List<XPoint> vertices)
        {
            if (vertices == null || vertices.Count < 2)
            {
                throw new ArgumentException("Vertices list must contain at least two points.");
            }

            double length = 0.0;

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                var point1 = vertices[i];
                var point2 = vertices[i + 1];
                length += Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
            }

            // Optionally, close the shape by connecting the last point to the first
            var firstPoint = vertices[0];
            var lastPoint = vertices[vertices.Count - 1];
            length += Math.Sqrt(Math.Pow(lastPoint.X - firstPoint.X, 2) + Math.Pow(lastPoint.Y - firstPoint.Y, 2));

            return length;
        }

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

    }
}