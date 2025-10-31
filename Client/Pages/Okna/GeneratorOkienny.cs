using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Client.Pages.Models;
using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;

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

        // public new MVCKonfModele? PowiazanyModel;

        public Generator()
        {
            // Inicjalizacja domyślnych wartości
            Szerokosc = 1000;
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

        public void AddElements(List<ShapeRegion> regions, string regionId, Dictionary<string, GeneratorState> generatorStates, List<ShapeRegion> regionAdd, List<DaneKwadratu> daneKwadratu)
        {
            if (regions == null) return;

            if (KonfiguracjeSystemu == null || MVCKonfModelu == null)
            {
                Console.WriteLine($"❌ Brak KonfiguracjeSystemu lub PowiazanyModel!");
                return;
            }

            if (EdytowanyModel == null)
            {
                Console.WriteLine($"❌ Brak EdytowanyModel jest nie ustawiony!!!");
                return;
            }

            Console.WriteLine($"➡️ EdytowanyModel.PolaczenieNaroza: {EdytowanyModel.PolaczenieNaroza}");
            Console.WriteLine($"📏 Szerokosc: {Szerokosc}, Wysokosc: {Wysokosc}");

            Region = regionAdd;

            var region = regions.FirstOrDefault(r => r.Id == regionId);

            List<XPoint> punkty = new List<XPoint>();

            if (region == null && !ElementLiniowy)
            {
                Console.WriteLine($"❌ Nie znaleziono regionu o ID: {regionId} w AddElements - GeneratoryOkienne");
                return;
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
                return;
            }

            if ((punkty == null || punkty.Count < 2))
            {
                Console.WriteLine($"❌ Region o ID: {regionId} ma zbyt mało punktów! punkty.Count: {punkty.Count}");
                return;
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

            foreach (var konf in MVCKonfModelu.KonfSystem)
            {
                Console.WriteLine($"🔧 KonfiguracjeSystemu: {konf.Typ} Nazwa: {konf.Nazwa} W sumie: {MVCKonfModelu.KonfSystem.Count()}");
            }

            Console.WriteLine($"slruchPoPrawej = {slruchPoPrawej} slruchPoLewej = {slruchPoLewej}");

            // 🔧 Profile z konfiguracji
            float profileLeft = (float)(MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa && e.Typ == slruchPoLewej)?.PionPrawa ?? 0 -
                                        MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa && e.Typ == slruchPoLewej)?.PionLewa ?? 0);

            float profileRight = (float)(MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa && e.Typ == slruchPoPrawej)?.PionPrawa ?? 0 -
                                         MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa && e.Typ == slruchPoPrawej)?.PionLewa ?? 0);

            float profileTop = (float)(MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeGora)?.PionPrawa ?? 0 -
                                       MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeGora)?.PionLewa ?? 0);

            float profileBottom = (float)(MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeDol)?.PionPrawa ?? 0 -
                                          MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeDol)?.PionLewa ?? 0);


            Guid RowIdprofileLeft = MVCKonfModelu.KonfSystem
                .FirstOrDefault(e => e.WystepujeLewa && e.Typ == slruchPoLewej)?.RowId ?? Guid.Empty;

            Guid RowIdprofileRight = MVCKonfModelu.KonfSystem
                .FirstOrDefault(e => e.WystepujePrawa && e.Typ == slruchPoPrawej)?.RowId ?? Guid.Empty;

            Guid RowIdprofileTop = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeGora)?.RowId ?? Guid.Empty;
            Guid RowIdprofileBottom = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeDol)?.RowId ?? Guid.Empty;

            string RowIndeksprofileLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa && e.Typ == slruchPoLewej)?.Indeks ?? "BRAK-DANYCH";
            string RowIndeksprofileRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa && e.Typ == slruchPoPrawej)?.Indeks ?? "BRAK-DANYCH";
            string RowIndeksprofileTop = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeGora)?.Indeks ?? "BRAK-DANYCH";
            string RowIndeksprofileBottom = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeDol)?.Indeks ?? "BRAK-DANYCH";

            string RowNazwaprofileLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa && e.Typ == slruchPoLewej)?.Nazwa ?? "BRAK-DANYCH";
            string RowNazwaprofileRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa && e.Typ == slruchPoPrawej)?.Nazwa ?? "BRAK-DANYCH";
            string RowNazwaprofileTop = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeGora)?.Nazwa ?? "BRAK-DANYCH";
            string RowNazwaprofileBottom = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeDol)?.Nazwa ?? "BRAK-DANYCH";

            if (profileLeft == 0)
            {
                //Spróbuj bez słupka
                slruchPoLewej = "";
                profileLeft = (float)(MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa)?.PionPrawa ?? 0 -
                                        MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa)?.PionLewa ?? 0);

                RowIdprofileLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa)?.RowId ?? Guid.Empty;
                RowIndeksprofileLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa)?.Indeks ?? "BRAK-DANYCH";

                RowNazwaprofileLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa)?.Nazwa ?? "BRAK-DANYCH";
            }
            if (profileRight == 0)
            {
                //Spróbuj bez słupka
                slruchPoPrawej = "";
                profileRight = (float)(MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.PionPrawa ?? 0 -
                                         MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.PionLewa ?? 0);

                RowIdprofileRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.RowId ?? Guid.Empty;
                RowIndeksprofileRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.Indeks ?? "BRAK-DANYCH";
                RowNazwaprofileRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.Nazwa ?? "BRAK-DANYCH";
            }

            string NazwaObiektu = MVCKonfModelu.KonfSystem.First().Nazwa ?? "";

            Console.WriteLine($"📐Generator ----> region.TypKsztaltu: {region.TypKsztaltu} profileLeft: {profileLeft}, profileRight: {profileRight}, profileTop: {profileTop}, profileBottom: {profileBottom} slruchPoPrawej: {slruchPoPrawej} slruchPoLewej: {slruchPoLewej}");

            // 🔲 Oblicz wewnętrzny kontur
            var wewnetrznyKontur = CalculateOffsetPolygon(
                przeskalowanePunkty,
                profileLeft, profileRight, profileTop, profileBottom);

            GenerateGenericElementsWithJoins(
                przeskalowanePunkty,
                wewnetrznyKontur,
                profileLeft, profileRight, profileTop, profileBottom,
                region.TypKsztaltu,
                EdytowanyModel.PolaczenieNaroza,
                KonfiguracjeSystemu,
                regionId,
                RowIdprofileLeft, RowIdprofileRight, RowIdprofileTop, RowIdprofileBottom,
                RowIndeksprofileLeft, RowIndeksprofileRight, RowIndeksprofileTop, RowIndeksprofileBottom,
                RowNazwaprofileLeft, RowNazwaprofileRight, RowNazwaprofileTop, RowNazwaprofileBottom,
                NazwaObiektu,
                daneKwadratu
            );
            //}
        }

        private void GenerateGenericElementsWithJoins(
            List<XPoint> outer, List<XPoint> inner,
            float profileLeft, float profileRight, float profileTop, float profileBottom,
            string typKsztalt, string polaczenia, List<KonfSystem> model, string regionId,
            Guid rowIdprofileLeft, Guid rowIdprofileRight, Guid rowIdprofileTop, Guid rowIdprofileBottom,
            string rowIndeksprofileLeft, string rowIndeksprofileRight, string rowIndeksprofileTop, string rowIndeksprofileBottom,
            string rowNazwaprofileLeft, string rowNazwaprofileRight, string rowNazwaprofileTop, string rowNazwaprofileBottom,
            string NazwaObiektu, List<DaneKwadratu> daneKwadratu)
        {

            Console.WriteLine($"❌ Generowanie elementów dla regionu {regionId} z typem kształtu: {typKsztalt} oraz ElementLiniowy: {ElementLiniowy} profileLeft: {profileLeft}, profileRight :{profileRight}");

            float angleDegreesElementLionowy = 0;

            // 🔹 Nowy tryb – jeśli to tylko element liniowy (np. słupek)
            if (ElementLiniowy)
            {
                if (outer == null || outer.Count < 2)
                {
                    Console.WriteLine("▶️ Element: brak wystarczającej liczby punktów (min. 2 wymagane).");
                    return;
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

                Console.WriteLine($"▶️ Element 1 wartość X: {szukDaneKwadratu.Wierzcholki[0].X} dotyczy: ElementLiniowy: {ElementLiniowy}");

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

            for (int i = 0; i < vertexCount; i++)
            {
                int next = (i + 1) % vertexCount;

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

                // Przekształć do zakresu 0–360°, jeśli potrzebujesz
                if (angleDegrees < 0)
                    angleDegrees += 360f;

                Console.WriteLine($"▶️ Processing element {i + 1}/{vertexCount} with joins: {leftJoin} - {rightJoin} wyliczony kąt: {angleDegrees} dla i: {i}");

                if (length < 0.001f) continue;

                float tx = dx / length;
                float ty = dy / length;
                float nx = -ty;
                float ny = tx;

                float profile = Math.Abs(dx) > Math.Abs(dy)
                    ? (ny > 0 ? profileTop : profileBottom)
                    : (nx > 0 ? profileRight : profileLeft);

                bool isAlmostHorizontal = Math.Abs(dy) < 1e-2;
                bool isAlmostVertical = Math.Abs(dx) < 1e-2;

                //bool isMoreHorizontal = Math.Abs(dy) < Math.Abs(dx); // To do poprawy kiedy jest pionowy a kiedy poziomy
                //bool isMoreVertical = Math.Abs(dx) < Math.Abs(dy);

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

                Console.WriteLine($"▶️ DEBUG: Generating element {i + 1}/{vertexCount} with joins: {leftJoin} - {rightJoin}");

                if (leftJoin == "T1" && rightJoin == "T1" || leftJoin == "T1" && rightJoin == "T4" || leftJoin == "T4" && rightJoin == "T1" || leftJoin == "T4" && rightJoin == "T4")
                {
                    if (isAlmostHorizontal)
                    {
                        //Console.WriteLine($"🔷 Horizontal case for element {i + 1} isAlmostHorizontal: {isAlmostHorizontal} isAlmostVertical: {isAlmostVertical}");
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
                            var outerTop = GetHorizontalIntersection(_innerStart, _innerEnd, (float)topY, 0);
                            var outerBottom = GetHorizontalIntersection(_innerStart, _innerEnd, (float)bottomY, 0);

                            // Normalne punkty wewnętrzne
                            var innerTop = GetHorizontalIntersection(outer[i], outer[next], (float)topY, 0);
                            var innerBottom = GetHorizontalIntersection(outer[i], outer[next], (float)bottomY, 0);

                            wierzcholki = new List<XPoint> {
                                outerTop, outerBottom, innerBottom, innerTop
                            };
                        }
                        else
                        {
                            // Pionowy przypadek (np. boczne elementy w trapezie)
                            var topY = Math.Min(inner[i].Y, inner[next].Y);
                            var bottomY = Math.Max(inner[i].Y, inner[next].Y);

                            var outerTop = GetHorizontalIntersection(outerStart, outerEnd, (float)topY, 0);
                            var outerBottom = GetHorizontalIntersection(outerStart, outerEnd, (float)bottomY, 0);

                            var innerTop = GetHorizontalIntersection(inner[i], inner[next], (float)topY, 0);
                            var innerBottom = GetHorizontalIntersection(inner[i], inner[next], (float)bottomY, 0);

                            wierzcholki = new List<XPoint> {
                                outerTop, outerBottom, innerBottom, innerTop
                            };
                        }

                    }
                }
                else if (leftJoin == "T3" && rightJoin == "T3")
                {
                    //Console.WriteLine($"🔷 T3/T3 element {i + 1} isAlmostHorizontal: {isAlmostHorizontal} isAlmostVertical: {isAlmostVertical}");

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
                            var prev = (i - 1 + vertexCount) % vertexCount;
                            var nextNext = (next + 1) % vertexCount;

                            if (polaczeniaArray[prev].typ == "T3")
                            {
                                outerVecStart = FindFirstEdgeIntersection(outerVecStart, tx, ty, inner);
                            }

                            if (polaczeniaArray[nextNext % vertexCount].typ == "T3")
                            {
                                outerVecEnd = FindFirstEdgeIntersection(outerVecEnd, -tx, -ty, inner);
                            }

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
                            var prev = (i - 1 + vertexCount) % vertexCount;
                            var nextNext = (next + 1) % vertexCount;

                            if (polaczeniaArray[prev].typ == "T3")
                            {
                                outerVecStart = FindFirstEdgeIntersection(outerVecStart, tx, ty, outer);
                            }

                            if (polaczeniaArray[nextNext % vertexCount].typ == "T3")
                            {
                                outerVecEnd = FindFirstEdgeIntersection(outerVecEnd, -tx, -ty, outer);
                            }

                            // Przesunięcie do wnętrza
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

                    XPoint outerTopT5 = new XPoint { };
                    XPoint outerBottomT5 = new XPoint { };
                    XPoint innerTopT5 = new XPoint { };
                    XPoint innerBottomT5 = new XPoint { };

                    if (daneKwadratu != null && daneKwadratu.Count > 0)
                    {
                        foreach (var xx in daneKwadratu)
                        {
                            foreach (var yy in xx.Wierzcholki)
                            {
                                Console.WriteLine($"🔷 T5-T5 BoolElementLinia: {xx.BoolElementLinia} KatLinii:{xx.KatLinii} X:{yy.X} Y:{yy.X}");
                            }
                        }

                        var szerSlupka = KonfiguracjeSystemu.FirstOrDefault(x => x.RowId == rowIdprofileLeft); // w słupku stałym rowIdprofileLeft to samo jest we wszystkich pozycjach!!!!

                        float PionOsSymetrii = 0;

                        if (szerSlupka != null)
                            PionOsSymetrii = (float)Math.Abs((float)szerSlupka.PionOsSymetrii);

                        // Ustal kierunki (poziomy vs pionowy)
                        if (isAlmostVertical)
                        {
                            double topYShift = 0;
                            double bottomYShift = 0;

                            double topY = 0;
                            double bottomY = 0;

                            var IdWymTop = daneKwadratu.FirstOrDefault(x => x.KatLinii is >= 0 and < 90)?.RowIdSasiada ?? Guid.Empty;
                            var IdWymBottom = daneKwadratu.FirstOrDefault(x => x.KatLinii is >= 180 and < 270)?.RowIdSasiada ?? Guid.Empty;

                            if (IdWymTop != Guid.Empty && IdWymBottom != Guid.Empty)
                            {
                                var topElement = model.FirstOrDefault(x => x.RowId == IdWymTop);
                                var bottomElement = model.FirstOrDefault(x => x.RowId == IdWymBottom);

                                topYShift = Math.Abs((topElement?.PoziomGora ?? 0) - (topElement?.PoziomDol ?? 0));
                                bottomYShift = Math.Abs((bottomElement?.PoziomGora ?? 0) - (bottomElement?.PoziomDol ?? 0));

                                Console.WriteLine($"🔷 Vertical shifts → topYShift:{topYShift}, bottomYShift:{bottomYShift}");
                            }

                            topY = Math.Min(inner[i].Y, inner[next].Y) + topYShift;
                            bottomY = Math.Max(inner[i].Y, inner[next].Y) - bottomYShift;

                            // Start liczymy względem punktu przecięcia z inner[i] (czyli skrócony)
                            outerTopT5 = GetHorizontalIntersection(_innerStart, _innerEnd, (float)topY, PionOsSymetrii);
                            outerBottomT5 = GetHorizontalIntersection(_innerStart, _innerEnd, (float)bottomY, PionOsSymetrii);

                            // Normalne punkty wewnętrzne
                            innerTopT5 = GetHorizontalIntersection(outer[i], outer[next], (float)topY, PionOsSymetrii);
                            innerBottomT5 = GetHorizontalIntersection(outer[i], outer[next], (float)bottomY, PionOsSymetrii);

                            foreach (var x in inner)
                            {
                                Console.WriteLine($"🔷 Vertical shifts → inner.X: {x.X} inner.Y: {x.Y}");
                            }

                            foreach (var x in outer)
                            {
                                Console.WriteLine($"🔷 Vertical shifts → outer.X: {x.X} outer.Y: {x.Y}");
                            }

                            Console.WriteLine($"🔷 Vertical shifts → innerTopT5.X: {innerTopT5.X} innerTopT5.Y: {innerTopT5.Y} innerBottomT5.X: {innerBottomT5.X} innerBottomT5.Y: {innerBottomT5.Y}");
                            Console.WriteLine($"🔷 Vertical shifts → outerTopT5.X: {outerTopT5.X} outerTopT5.Y: {outerTopT5.Y} outerBottomT5.X: {outerBottomT5.X} outerBottomT5.Y: {outerBottomT5.Y}");
                        }
                        else if (isAlmostHorizontal)
                        {
                            double leftXShift = 0;
                            double rightXShift = 0;

                            double leftX = 0;
                            double rightX = 0;

                            var IdWymTop = daneKwadratu.FirstOrDefault(x => x.KatLinii is >= 270 and < 360)?.RowIdSasiada ?? Guid.Empty;
                            var IdWymBottom = daneKwadratu.FirstOrDefault(x => x.KatLinii is >= 90 and < 180)?.RowIdSasiada ?? Guid.Empty;

                            if (IdWymTop != Guid.Empty && IdWymBottom != Guid.Empty)
                            {
                                var topElement = model.FirstOrDefault(x => x.RowId == IdWymTop);
                                var bottomElement = model.FirstOrDefault(x => x.RowId == IdWymBottom);

                                leftXShift = Math.Abs((topElement?.PoziomGora ?? 0) - (topElement?.PoziomDol ?? 0)); // linie są generowane domyślnie!!!!
                                rightXShift = Math.Abs((bottomElement?.PoziomGora ?? 0) - (bottomElement?.PoziomDol ?? 0));

                                Console.WriteLine($"🔷 Horizontal shifts → leftXShift:{leftXShift}, rightXShift:{rightXShift}");
                            }

                            leftX = Math.Min(inner[i].X, inner[next].X) + leftXShift;
                            rightX = Math.Max(inner[i].X, inner[next].X) - rightXShift;

                            // Oblicz przecięcia
                            outerTopT5 = GetVerticalIntersection(_innerStart, _innerEnd, (float)leftX, PionOsSymetrii);
                            outerBottomT5 = GetVerticalIntersection(_innerStart, _innerEnd, (float)rightX, PionOsSymetrii);

                            innerTopT5 = GetVerticalIntersection(outer[i], outer[next], (float)leftX, PionOsSymetrii);
                            innerBottomT5 = GetVerticalIntersection(outer[i], outer[next], (float)rightX, PionOsSymetrii);

                            foreach (var x in inner)
                            {
                                Console.WriteLine($"🔷 Horizontal shifts → inner.X: {x.X} inner.Y: {x.Y}");
                            }

                            foreach (var x in outer)
                            {
                                Console.WriteLine($"🔷 Horizontal shifts → outer.X: {x.X} outer.Y: {x.Y}");
                            }

                            Console.WriteLine($"🔷 Horizontal shifts → innerTopT5.X: {innerTopT5.X} innerTopT5.Y: {innerTopT5.Y} innerBottomT5.X: {innerBottomT5.X} innerBottomT5.Y: {innerBottomT5.Y}");
                            Console.WriteLine($"🔷 Horizontal shifts → outerTopT5.X: {outerTopT5.X} outerTopT5.Y: {outerTopT5.Y} outerBottomT5.X: {outerBottomT5.X} outerBottomT5.Y: {outerBottomT5.Y}");
                        }
                    }

                    // Bezpieczne granice


                    // Zbierz punkty w kolejności
                    wierzcholki = new List<XPoint>
                    {
                        outerTopT5,
                        outerBottomT5,
                        innerBottomT5,
                        innerTopT5
                    };

                    Console.WriteLine($"🔷 T5-T5 -> wierzcholki: {wierzcholki.Count} new List<XPoint>");
                }
                else
                {
                    Console.WriteLine($"🔷 Default case for element {i + 1} with joins: {leftJoin}-{rightJoin}");

                    float leftOffset = GetJoinOffset(leftJoin, profile);
                    float rightOffset = GetJoinOffset(rightJoin, profile);

                    var adjOuterStart = new XPoint(
                        outerStart.X + tx * leftOffset,
                        outerStart.Y + ty * leftOffset);

                    var adjOuterEnd = new XPoint(
                        outerEnd.X - tx * rightOffset,
                        outerEnd.Y - ty * rightOffset);

                    var innerStart = new XPoint(
                        adjOuterStart.X + nx * profile,
                        adjOuterStart.Y + ny * profile);

                    var innerEnd = new XPoint(
                        adjOuterEnd.X + nx * profile,
                        adjOuterEnd.Y + ny * profile);

                    wierzcholki = new List<XPoint> {
                    adjOuterStart, adjOuterEnd, innerEnd, innerStart
                    };
                }

                double regionMinX = wierzcholki.Min(p => p.X);
                double regionMaxX = wierzcholki.Max(p => p.X);
                double regionMinY = wierzcholki.Min(p => p.Y);
                double regionMaxY = wierzcholki.Max(p => p.Y);

                int wartoscX = (int)Math.Round(regionMaxX - regionMinX);
                int wartoscY = (int)Math.Round(regionMaxY - regionMinY);

                // Console.WriteLine($"leftJoin: {leftJoin} rightJoin:{rightJoin} wierzcholki: {wierzcholki.Count()} isAlmostVertical:{isAlmostVertical}");
                float bazowaDlugosc = ObliczDlugoscElementu(wierzcholki);

                Console.WriteLine($"▶️ Element Start switch {i + 1}/{vertexCount}: Length: {length}, angleDegreesElementLionowy:{angleDegreesElementLionowy}, Angle: {angleDegrees}°, Profile: {profile}, Wierzchołki: {wierzcholki.Count}, BazowaDlugosc: {bazowaDlugosc}, wartoscX: {wartoscX}, wartoscY: {wartoscY} ElementLiniowy:{ElementLiniowy} wierzcholki X0: {wierzcholki[0].X} Y0: {wierzcholki[0].Y}");

                // Określenie kierunku
                string stronaOpis = angleDegrees switch
                {
                    >= 45 and < 135 => "Prawa",   // w okolicach 90°
                    >= 135 and < 225 => "Dół",    // w okolicach 180°
                    >= 225 and < 315 => "Lewa",   // w okolicach 270°
                    _ => "Góra"                   // w okolicach 0° lub 360°
                };

                switch (i)
                {
                    case 0:
                        if (angleDegreesElementLionowy != angleDegrees && ElementLiniowy) break;
                        if (rowIdprofileTop != Guid.Empty)
                            ElementyRamyRysowane.Add(new KsztaltElementu
                            {
                                TypKsztaltu = typKsztalt,
                                Wierzcholki = wierzcholki,
                                WypelnienieZewnetrzne = "wood-pattern",
                                WypelnienieWewnetrzne = KolorSzyby,
                                Grupa = NazwaObiektu + $" {stronaOpis}-{i + 1} {wartoscX}/{wartoscY}",
                                ZIndex = Zindeks,
                                RowIdElementu = rowIdprofileTop,
                                IdRegion = regionId,
                                Kat = (int)angleDegrees,
                                Strona = stronaOpis,//Była Góra
                                IndeksElementu = rowIndeksprofileTop,
                                NazwaElementu = rowNazwaprofileTop,
                                DlogoscElementu = bazowaDlugosc + (profileLeft + profileRight),
                                DlogoscNaGotowoElementu = bazowaDlugosc
                            });
                        Console.WriteLine($"▶️ Element {i + 1}/{vertexCount} dodałem do ElementyRamyRysowane. Total elements now: {ElementyRamyRysowane.Count} - 0 rowIdprofileTop:{rowIdprofileTop} Angle: {angleDegrees}°");
                        if (ElementLiniowy) return;
                        break;
                    case 1:
                        if (angleDegreesElementLionowy != angleDegrees && ElementLiniowy) break;
                        if (rowIdprofileRight != Guid.Empty)
                            ElementyRamyRysowane.Add(new KsztaltElementu
                            {
                                TypKsztaltu = typKsztalt,
                                Wierzcholki = wierzcholki,
                                WypelnienieZewnetrzne = "wood-pattern",
                                WypelnienieWewnetrzne = KolorSzyby,
                                Grupa = NazwaObiektu + $" {stronaOpis}-{i + 1} {wartoscX}/{wartoscY}",
                                ZIndex = Zindeks,
                                RowIdElementu = rowIdprofileRight,
                                IdRegion = regionId,
                                Kat = (int)angleDegrees,
                                Strona = stronaOpis,//Była prawa
                                IndeksElementu = rowIndeksprofileRight,
                                NazwaElementu = rowNazwaprofileTop,
                                DlogoscElementu = bazowaDlugosc + (profileLeft + profileRight),
                                DlogoscNaGotowoElementu = bazowaDlugosc
                            });
                        Console.WriteLine($"▶️ Element {i + 1}/{vertexCount} dodałem do ElementyRamyRysowane. Total elements now: {ElementyRamyRysowane.Count} - 1 rowIdprofileRight:{rowIdprofileRight} Angle: {angleDegrees}°");
                        if (ElementLiniowy) return;
                        break;
                    case 2:
                        if (angleDegreesElementLionowy != angleDegrees && ElementLiniowy) break;
                        if (rowIdprofileBottom != Guid.Empty)
                            ElementyRamyRysowane.Add(new KsztaltElementu
                            {
                                TypKsztaltu = typKsztalt,
                                Wierzcholki = wierzcholki,
                                WypelnienieZewnetrzne = "wood-pattern",
                                WypelnienieWewnetrzne = KolorSzyby,
                                Grupa = NazwaObiektu + $" {stronaOpis}-{i + 1} {wartoscX}/{wartoscY}",
                                ZIndex = Zindeks,
                                RowIdElementu = rowIdprofileBottom,
                                IdRegion = regionId,
                                Kat = (int)angleDegrees,
                                Strona = stronaOpis,//Był Dół
                                IndeksElementu = rowIndeksprofileBottom,
                                NazwaElementu = rowNazwaprofileTop,
                                DlogoscElementu = bazowaDlugosc + (profileLeft + profileRight),
                                DlogoscNaGotowoElementu = bazowaDlugosc
                            });
                        Console.WriteLine($"▶️ Element {i + 1}/{vertexCount} dodałem do ElementyRamyRysowane. Total elements now: {ElementyRamyRysowane.Count} - 2 rowIdprofileBottom:{rowIdprofileBottom} Angle: {angleDegrees}°");
                        if (ElementLiniowy) return;
                        break;
                    case 3:
                        if (angleDegreesElementLionowy != angleDegrees && ElementLiniowy) break;
                        if (rowIdprofileLeft != Guid.Empty)
                            ElementyRamyRysowane.Add(new KsztaltElementu
                            {
                                TypKsztaltu = typKsztalt,
                                Wierzcholki = wierzcholki,
                                WypelnienieZewnetrzne = "wood-pattern",
                                WypelnienieWewnetrzne = KolorSzyby,
                                Grupa = NazwaObiektu + $" {stronaOpis}-{i + 1} {wartoscX}/{wartoscY}",
                                ZIndex = Zindeks,
                                RowIdElementu = rowIdprofileLeft,
                                IdRegion = regionId,
                                Kat = (int)angleDegrees,
                                Strona = stronaOpis, //Była Lewa
                                IndeksElementu = rowIndeksprofileLeft,
                                NazwaElementu = rowNazwaprofileTop,
                                DlogoscElementu = bazowaDlugosc + (profileLeft + profileRight),
                                DlogoscNaGotowoElementu = bazowaDlugosc
                            });
                        Console.WriteLine($"▶️ Element {i + 1}/{vertexCount} dodałem do ElementyRamyRysowane. Total elements now: {ElementyRamyRysowane.Count} - 3 rowIdprofileLeft:{rowIdprofileLeft} Angle: {angleDegrees}°");
                        if (ElementLiniowy) return;
                        break;
                }
            }
        }

        private float ObliczDlugoscElementu(List<XPoint> wierzcholki)
        {
            double d1 = Math.Sqrt(Math.Pow(wierzcholki[1].X - wierzcholki[0].X, 2) +
                                  Math.Pow(wierzcholki[1].Y - wierzcholki[0].Y, 2));

            double d2 = Math.Sqrt(Math.Pow(wierzcholki[2].X - wierzcholki[3].X, 2) +
                                  Math.Pow(wierzcholki[2].Y - wierzcholki[3].Y, 2));

            return (float)Math.Round(Math.Max(d1, d2));
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

        private XPoint GetHorizontalIntersection(XPoint a, XPoint b, float y, float pionOsSymetrii)
        {
            if (Math.Abs(a.Y - b.Y) < 1e-3f)
                return new XPoint(a.X - pionOsSymetrii, y);

            float t = (y - (float)a.Y) / ((float)b.Y - (float)a.Y);
            float x = (float)a.X + t * ((float)b.X - (float)a.X);
            return new XPoint(x - pionOsSymetrii, y);
        }

        private XPoint GetVerticalIntersection(XPoint a, XPoint b, float x, float pionOsSymetrii)
        {
            if (Math.Abs(a.X - b.X) < 1e-3f)
                return new XPoint(x, a.Y - pionOsSymetrii);

            float t = (x - (float)a.X) / ((float)b.X - (float)a.X);
            float y = (float)a.Y + t * ((float)b.Y - (float)a.Y);
            return new XPoint(x, y - pionOsSymetrii);
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

        private List<XPoint> CalculateOffsetPolygon(
            List<XPoint> points,
            float profileLeft,
            float profileRight,
            float profileTop,
            float profileBottom)
        {
            int count = points.Count;

            Console.WriteLine($"🔷 Calculating offset polygon for {count} ElementLiniowy:{ElementLiniowy} points with profiles L:{profileLeft}, R:{profileRight}, T:{profileTop}, B:{profileBottom}");

            if (count < 2)
                throw new ArgumentException("Figura musi mieć co najmniej 2 punkty.");

            // 🟢 OBSŁUGA ELEMENTÓW LINIOWYCH (np. słupków)
            if (ElementLiniowy)
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

                    Console.WriteLine($"🔷 Horizontal element → offset only in Y: offsetY={offsetY}");
                }
                else if (isVertical)
                {
                    // Linia pionowa – przesuwamy tylko w osi X
                    offsetY = 0f;
                    offsetX = dx >= 0 ? profileRight : -profileLeft;

                    Console.WriteLine($"🔷 Vertical element → offset only in Y: offsetX={offsetX}");
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

                Console.WriteLine($"🔷 Offset liniowy p1Offset: X={p1Offset.X}, Y={p1Offset.Y} | p2Offset: X={p2Offset.X}, Y={p2Offset.Y} | isHorizontal={isHorizontal}");

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


        // Other properties and methods of the Generator class...

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

    }
}