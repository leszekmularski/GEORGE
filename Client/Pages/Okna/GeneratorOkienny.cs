using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Client.Pages.Models;
using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;
using System.Threading.Tasks;

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

        // public new MVCKonfModele? PowiazanyModel;

        public Generator()
        {
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

        public async Task AddElements(List<ShapeRegion> regions, string regionId, Dictionary<string, GeneratorState> generatorStates, List<ShapeRegion> regionAdd,
            List<DaneKwadratu> daneKwadratu, List<XPoint> punktyRegionuMaster = null)
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

            //foreach (var konf in MVCKonfModelu.KonfSystem)
            //{
            //    Console.WriteLine($"🔧 KonfiguracjeSystemu: {konf.Typ} Nazwa: {konf.Nazwa} W sumie: {MVCKonfModelu.KonfSystem.Count()}");
            //}

            //Console.WriteLine($"slruchPoPrawej = {slruchPoPrawej} slruchPoLewej = {slruchPoLewej}");

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

            string RowIndeksprofileLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa && e.Typ == slruchPoLewej)?.IndeksElementu ?? "BRAK-DANYCH";
            string RowIndeksprofileRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa && e.Typ == slruchPoPrawej)?.IndeksElementu ?? "BRAK-DANYCH";
            string RowIndeksprofileTop = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeGora)?.IndeksElementu ?? "BRAK-DANYCH";
            string RowIndeksprofileBottom = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeDol)?.IndeksElementu ?? "BRAK-DANYCH";

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
                RowIndeksprofileLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa)?.IndeksElementu ?? "BRAK-DANYCH";

                RowNazwaprofileLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa)?.Nazwa ?? "BRAK-DANYCH";
            }
            if (profileRight == 0)
            {
                //Spróbuj bez słupka
                slruchPoPrawej = "";
                profileRight = (float)(MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.PionPrawa ?? 0 -
                                         MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.PionLewa ?? 0);

                RowIdprofileRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.RowId ?? Guid.Empty;
                RowIndeksprofileRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.IndeksElementu ?? "BRAK-DANYCH";
                RowNazwaprofileRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.Nazwa ?? "BRAK-DANYCH";
            }

            string NazwaObiektu = MVCKonfModelu.KonfSystem.First().Nazwa ?? "";
            string TypObiektu = MVCKonfModelu.KonfSystem.First().Typ ?? "";

            Console.WriteLine($"📐Generator ----> region.TypKsztaltu: {region.TypKsztaltu} profileLeft: {profileLeft}, profileRight: {profileRight}, profileTop: {profileTop}, profileBottom: {profileBottom} slruchPoPrawej: {slruchPoPrawej} slruchPoLewej: {slruchPoLewej}");

            // 🔲 Oblicz wewnętrzny kontur
            var wewnetrznyKontur = CalculateOffsetPolygon(
                przeskalowanePunkty,
                profileLeft, profileRight, profileTop, profileBottom);

         await  GenerateGenericElementsWithJoins(
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
                punktyRegionuMaster
            );

            //}
        }
        public async Task GenerateGenericElementsWithJoins(
            List<XPoint> outer, List<XPoint> inner,
            float profileLeft, float profileRight, float profileTop, float profileBottom,
            string typKsztalt, string polaczenia, bool sposobLaczeniaCzop, List<KonfSystem> model, string regionId,
            Guid rowIdprofileLeft, Guid rowIdprofileRight, Guid rowIdprofileTop, Guid rowIdprofileBottom,
            string rowIndeksprofileLeft, string rowIndeksprofileRight, string rowIndeksprofileTop, string rowIndeksprofileBottom,
            string rowNazwaprofileLeft, string rowNazwaprofileRight, string rowNazwaprofileTop, string rowNazwaprofileBottom,
            string NazwaObiektu, string TypObiektu, List<DaneKwadratu> daneKwadratu, List<XPoint> punktyRegionuMaster)
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


                ////var szukDaneKwadratu = daneKwadratu
                //// .Where(x => x.Wierzcholki.Count == 2)
                ////   .LastOrDefault();

                //Console.WriteLine($"▶️ Element 1 wartość X: {szukDaneKwadratu.Wierzcholki[0].X} dotyczy: ElementLiniowy: {ElementLiniowy} rowIdprofileLeft: {rowIdprofileLeft} rowIdprofileRight: {rowIdprofileRight} rowIdprofileTop: {rowIdprofileTop} rowIdprofileBottom: {rowIdprofileBottom}");

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
                StronaElementu = OkreslStroneNaPodstawieKata(angleDegrees, i, outer);

               Console.WriteLine($"▶️ Processing element {i + 1}/{vertexCount} with joins: {leftJoin} - {rightJoin} wyliczony kąt: {angleDegrees} dla i: {i} StronaElementu: {StronaElementu} length: {length}");

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
                            //             if(angleDegrees )

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
                else if (leftJoin == "T1" && rightJoin == "T1")
                {
                    //Console.WriteLine($"🔷 T1/T1 element {i + 1} START isAlmostHorizontal: {isAlmostHorizontal} isAlmostVertical: {isAlmostVertical} vertexCount: {vertexCount} angleDegrees: {angleDegrees}");

                    if (vertexCount == 3 && angleDegrees == 0)
                    {
                       // Console.WriteLine($"🔷 T1/T1 element {i + 1} vertexCount == 3 && angleDegrees == 0 START");

                        var prev = (i - 1 + vertexCount) % vertexCount;
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

                        var outerBottom = GetHorizontalIntersection(outerStart, outerEnd, (float)bottomY, 0);
                        var innerBottom = GetHorizontalIntersection(inner[i], inner[next], (float)bottomY, 0);

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
                            innerBottom = GetHorizontalIntersection(outer[prev], outer[i], (float)outerBottom.Y + ny * profile + ty * profile, 0);


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

                                var prev = (i - 1 + vertexCount) % vertexCount;
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
                                        bottomY,
                                        0
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

                                            outerVecTop = GetHorizontalIntersection(minOuter, leftOuter, bottomY, 0);

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

                                            outerVecTop = GetHorizontalIntersection(minOuter, leftOuter, bottomY, 0);

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
                                                bottomY,
                                                0
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
                                                bottomY,
                                                0
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
                            var prev = (i - 1 + vertexCount) % vertexCount;
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

                            var prev = (i - 1 + vertexCount) % vertexCount;
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

                            //Console.WriteLine($"🔷 OUTER narożniki trapezoidu:");
                            //Console.WriteLine($"   minOuter (lewy górny): ({minOuter.X:F2}, {minOuter.Y:F2})");
                            //Console.WriteLine($"   rightTopOuter (prawy górny): ({rightTopOuter.X:F2}, {rightTopOuter.Y:F2})");
                            //Console.WriteLine($"   leftOuter (lewy dolny): ({leftOuter.X:F2}, {leftOuter.Y:F2})");
                            //Console.WriteLine($"   rightOuter (prawy dolny): ({rightOuter.X:F2}, {rightOuter.Y:F2})");

                            // ================= INNER =================
                            double minYInner = inner.Min(p => p.Y);
                            double maxYInner = inner.Max(p => p.Y);

                            XPoint minInner = new XPoint();
                            XPoint minRightInner = new XPoint();
                            XPoint leftInner = new XPoint();
                            XPoint rightInner = new XPoint();

                            // Console.WriteLine($"🔷 inner Y zakres: min={minYInner:F2}, max={maxYInner:F2}");

                            // Punkty GÓRNE inner - PROBLEM: tylko JEDEN punkt ma Y=525,36
                            // Drugi "górny" punkt ma Y=562,64 (różnica 37,28)

                            // ZWIĘKSZ TOLERANCJĘ dla inner!
                            float innerTopTolerance = 50.0f; // Dopuszczamy różnicę 50 jednostek

                            var topPointsInner = inner.Where(p => p.Y <= minYInner + innerTopTolerance).ToList();
                            //Console.WriteLine($"🔷 topPointsInner (tolerancja={innerTopTolerance}) znaleziono {topPointsInner.Count} punktów:");
                            //foreach (var p in topPointsInner)
                            //{
                            //    Console.WriteLine($"   ({p.X:F2}, {p.Y:F2})");
                            //}

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

                            //Console.WriteLine($"🔷 INNER narożniki trapezoidu:");
                            //Console.WriteLine($"   minInner (lewy górny): ({minInner.X:F2}, {minInner.Y:F2})");
                            //Console.WriteLine($"   minRightInner (prawy górny): ({minRightInner.X:F2}, {minRightInner.Y:F2})");
                            //Console.WriteLine($"   leftInner (lewy dolny): ({leftInner.X:F2}, {leftInner.Y:F2})");
                            //Console.WriteLine($"   rightInner (prawy dolny): ({rightInner.X:F2}, {rightInner.Y:F2})");

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
                                    bottomY,
                                    0
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

                    //XPoint TopLT5 = new XPoint { };
                    //XPoint TopST5 = new XPoint { };
                    //XPoint TopRT5 = new XPoint { };

                    //XPoint BottomLT5 = new XPoint { };
                    //XPoint BottomRT5 = new XPoint { };


                    XPoint TopLT5 = new XPoint { };
                    XPoint TopST5 = new XPoint { };
                    XPoint TopRT5 = new XPoint { };

                    XPoint BottomLT5 = new XPoint { };
                    XPoint BottomSTT5 = new XPoint { };
                    XPoint BottomRT5 = new XPoint { };

                    double? SzerokoscSlupka = 0;

                    double topYShift = 0;
                    double bottomYShift = 0;

                    double leftXShift = 0;
                    double rightXShift = 0;

                    if (daneKwadratu != null && daneKwadratu.Count > 0)
                    {
                        //foreach (var xx in daneKwadratu)
                        //{
                        //    foreach (var yy in xx.Wierzcholki)
                        //    {
                        //        Console.WriteLine($"🔷 T5-T5 BoolElementLinia: {xx.BoolElementLinia} KatLinii:{xx.KatLinii} X:{yy.X} Y:{yy.Y} RowIdSasiada: {xx.RowIdSasiada} RowIdSasiadaStronaA: {xx.RowIdSasiadaStronaA} RowIdSasiadaStronaB: {xx.RowIdSasiadaStronaB}");
                        //    }
                        //}

                        var szerSlupka = KonfiguracjeSystemu.FirstOrDefault(x => x.RowId == rowIdprofileLeft); // w słupku stałym rowIdprofileLeft to samo jest we wszystkich pozycjach!!!!

                        float PionOsSymetrii = 0;

                        if (szerSlupka != null)
                        {
                            PionOsSymetrii = (float)Math.Abs((float)szerSlupka.PionOsSymetrii);
                            SzerokoscSlupka = szerSlupka.PionPrawa - szerSlupka.PionLewa;
                        }

                        // Ustal kierunki (poziomy vs pionowy)
                        if (isAlmostVertical)
                        {
                            double topY = 0;
                            double bottomY = 0;

                            var IdWymTop = daneKwadratu.FirstOrDefault(s => s.BoolElementLinia)?.RowIdSasiadaStronaA ?? Guid.Empty;
                            var IdWymBottom = daneKwadratu.FirstOrDefault(s => s.BoolElementLinia)?.RowIdSasiadaStronaB ?? Guid.Empty;

                            Console.WriteLine($"🔷 T5-T5 Vertical shifts → IdWymTop:{IdWymTop}, IdWymBottom:{IdWymBottom}");

                            if (IdWymTop != Guid.Empty && IdWymBottom != Guid.Empty)
                            {
                                var topElement = model.FirstOrDefault(x => x.RowId == IdWymTop);
                                var bottomElement = model.FirstOrDefault(x => x.RowId == IdWymBottom);

                                topYShift = Math.Abs((topElement?.PoziomGora ?? 0) - (topElement?.PoziomDol ?? 0));
                                bottomYShift = Math.Abs((bottomElement?.PoziomGora ?? 0) - (bottomElement?.PoziomDol ?? 0));

                                Console.WriteLine($"🔷 T5-T5 Vertical shifts → topYShift:{topYShift}, bottomYShift:{bottomYShift}");
                            }

                            topY = Math.Min(inner[i].Y, inner[next].Y) + topYShift;
                            bottomY = Math.Max(inner[i].Y, inner[next].Y) - bottomYShift;

                            // Start liczymy względem punktu przecięcia z inner[i] (czyli skrócony)
                            TopLT5 = GetHorizontalIntersection(_innerStart, _innerEnd, (float)topY, PionOsSymetrii);
                            BottomRT5 = GetHorizontalIntersection(_innerStart, _innerEnd, (float)bottomY, PionOsSymetrii);

                            // Normalne punkty wewnętrzne
                            TopRT5 = GetHorizontalIntersection(outer[i], outer[next], (float)topY, PionOsSymetrii);
                            BottomLT5 = GetHorizontalIntersection(outer[i], outer[next], (float)bottomY, PionOsSymetrii);

                            //foreach (var x in inner)
                            //{
                            //    Console.WriteLine($"🔷 T5-T5 Vertical shifts → inner.X: {x.X} inner.Y: {x.Y}");
                            //}

                            //foreach (var x in outer)
                            //{
                            //    Console.WriteLine($"🔷 T5-T5 Vertical shifts → outer.X: {x.X} outer.Y: {x.Y}");
                            //}

                            //Console.WriteLine($"🔷 T5-T5 Vertical shifts → TopRT5.X: {TopRT5.X} TopRT5.Y: {TopRT5.Y} BottomLT5.X: {BottomLT5.X} BottomLT5.Y: {BottomLT5.Y}");
                            //Console.WriteLine($"🔷 T5-T5 Vertical shifts → TopLT5.X: {TopLT5.X} TopLT5.Y: {TopLT5.Y} BottomRT5.X: {BottomRT5.X} BottomRT5.Y: {BottomRT5.Y}");
                        }
                        else if (isAlmostHorizontal)
                        {

                            double leftX = 0;
                            double rightX = 0;

                            var IdWymTop = daneKwadratu.FirstOrDefault(s => s.BoolElementLinia)?.RowIdSasiadaStronaA ?? Guid.Empty;
                            var IdWymBottom = daneKwadratu.FirstOrDefault(s => s.BoolElementLinia)?.RowIdSasiadaStronaB ?? Guid.Empty;

                            if (IdWymTop != Guid.Empty && IdWymBottom != Guid.Empty)
                            {
                                var topElement = model.FirstOrDefault(x => x.RowId == IdWymTop);
                                var bottomElement = model.FirstOrDefault(x => x.RowId == IdWymBottom);

                                leftXShift = Math.Abs((topElement?.PoziomGora ?? 0) - (topElement?.PoziomDol ?? 0)); // linie są generowane domyślnie!!!!
                                rightXShift = Math.Abs((bottomElement?.PoziomGora ?? 0) - (bottomElement?.PoziomDol ?? 0));

                                Console.WriteLine($"🔷 T5-T5 Horizontal shifts → leftXShift:{leftXShift}, rightXShift:{rightXShift}");
                            }

                            leftX = Math.Min(inner[i].X, inner[next].X) + leftXShift;
                            rightX = Math.Max(inner[i].X, inner[next].X) - rightXShift;

                            // Oblicz przecięcia
                            TopLT5 = GetVerticalIntersection(_innerStart, _innerEnd, (float)leftX, PionOsSymetrii);
                            BottomRT5 = GetVerticalIntersection(_innerStart, _innerEnd, (float)rightX, PionOsSymetrii);

                            TopRT5 = GetVerticalIntersection(outer[i], outer[next], (float)leftX, PionOsSymetrii);
                            BottomLT5 = GetVerticalIntersection(outer[i], outer[next], (float)rightX, PionOsSymetrii);

                            //foreach (var x in inner)
                            //{
                            //    Console.WriteLine($"🔷 T5-T5 Horizontal shifts → inner.X: {x.X} inner.Y: {x.Y}");
                            //}

                            //foreach (var x in outer)
                            //{
                            //    Console.WriteLine($"🔷 T5-T5 Horizontal shifts → outer.X: {x.X} outer.Y: {x.Y}");
                            //}

                            //Console.WriteLine($"🔷 T5-T5 Horizontal shifts → TopRT5.X: {TopRT5.X} TopRT5.Y: {TopRT5.Y} BottomLT5.X: {BottomLT5.X} BottomLT5.Y: {BottomLT5.Y}");
                            //Console.WriteLine($"🔷 T5-T5 Horizontal shifts → TopLT5.X: {TopLT5.X} TopLT5.Y: {TopLT5.Y} BottomRT5.X: {BottomRT5.X} BottomRT5.Y: {BottomRT5.Y}");
                        }
                    }

                    // Bezpieczne granice
                    //                    double half = (SzerokoscSlupka ?? 0) / 2.0;

                    var punkYModelu = punktyRegionuMaster.Max(p => p.Y) / 2;

                    TopST5.Y = punkYModelu;
                    TopST5.X = TopLT5.X - ((TopLT5.X - TopRT5.X) / 2);
                    Console.WriteLine($"🔷 T5-T5 TopST5.X/Y: {TopST5.X}/{TopST5.Y}");
                    BottomSTT5.Y = punkYModelu;
                    BottomSTT5.X = BottomLT5.X - ((BottomLT5.X - BottomRT5.X) / 2);
                    Console.WriteLine($"🔷 T5-T5 BottomSTT5.X/Y: {BottomSTT5.X}/{BottomSTT5.Y}");

                    var offset_punktyRegionuMaster = OffsetPolygonInside(punktyRegionuMaster, topYShift);

                    //foreach (var x in offset_punktyRegionuMaster)
                    //{
                    //    Console.WriteLine($"🔷 T5-T5 offset_punktyRegionuMaster.X: {x.X} punktyRegionuMaster.Y: {x.Y}");
                    //}

                    var tmpTopLT5 = new XPoint(TopLT5.X, punkYModelu + 1);
                    var tmpTopRT5 = new XPoint(TopRT5.X, punkYModelu + 1);

                    XPoint leftTopIntersection = FindFirstEdgeIntersectionByVector(tmpTopLT5, TopLT5, BottomRT5, offset_punktyRegionuMaster, forward: false);
                    XPoint midTopIntersection = FindFirstEdgeIntersectionByVector(TopST5, TopLT5, BottomRT5, offset_punktyRegionuMaster, forward: false);
                    XPoint rightTopIntersection = FindFirstEdgeIntersectionByVector(tmpTopRT5, TopLT5, BottomRT5, offset_punktyRegionuMaster, forward: false);

                    offset_punktyRegionuMaster = OffsetPolygonInside(punktyRegionuMaster, bottomYShift);

                    //foreach (var x in offset_punktyRegionuMaster)
                    //{
                    //    Console.WriteLine($"🔷 T5-T5 offset_punktyRegionuMaster.X: {x.X} punktyRegionuMaster.Y: {x.Y}");
                    //}

                    var tmpBottomLT5 = new XPoint(BottomLT5.X, punkYModelu - 1);
                    var tmpBottomRT5 = new XPoint(BottomRT5.X, punkYModelu - 1);

                    XPoint leftBottomIntersection = FindFirstEdgeIntersectionByVector(tmpBottomLT5, TopLT5, BottomRT5, offset_punktyRegionuMaster, forward: true);
                    XPoint midBottomIntersection = FindFirstEdgeIntersectionByVector(BottomSTT5, TopLT5, BottomRT5, offset_punktyRegionuMaster, forward: true);
                    XPoint righBottomtIntersection = FindFirstEdgeIntersectionByVector(tmpBottomRT5, TopLT5, BottomRT5, offset_punktyRegionuMaster, forward: true);

                    Console.WriteLine($"🔷 T5-T5 TopLT5.X/Y: {TopLT5.X}/{TopLT5.Y}");
                    Console.WriteLine($"🔷 T5-T5 BottomRT5.X/Y: {BottomRT5.X}/{BottomRT5.Y}");

                    Console.WriteLine($"🔷 T5-T5 midTopIntersection.X/Y: {midTopIntersection.X}/{midTopIntersection.Y}");

                    BottomSTT5 = midBottomIntersection;
                    TopST5 = midTopIntersection;

                    // var TopMin = punktyRegionuMaster.Min(p => p.Y);
                    // var TopMax = punktyRegionuMaster.Max(p => p.Y);

                    //  Console.WriteLine($"🔷 T5-T5 Punkty regionu Master → TopMin:{TopMin}, TopMax:{TopMax}");

                    // Przesunięcie górnej krawędzi
                    //  var moveTopL = TopMin - leftTopIntersection.Y;
                    //   var moveTopR = TopMin - rightTopIntersection.Y;

                    // Przesunięcie dolnej krawędzi
                    //var moveBottomL = TopMax - leftBottomIntersection.Y;
                    //   var moveBottomR = TopMax - righBottomtIntersection.Y;

                    // Console.WriteLine($"🔷 T5-T5 Moves -> moveTopL:{moveTopL}, moveTopR:{moveTopR}, moveBottomL:{moveBottomL}, moveBottomR:{moveBottomR}");


                    //// Zastosuj przesunięcie
                    //TopLT5.Y -= moveTopL;
                    //TopRT5.Y -= moveTopR;
                    //BottomLT5.Y -= moveBottomL;
                    //BottomRT5.Y -= moveBottomR;

                    TopLT5 = leftTopIntersection;
                    TopRT5 = rightTopIntersection;
                    BottomLT5 = leftBottomIntersection;
                    BottomRT5 = righBottomtIntersection;


                    // Zbierz punkty w kolejności
                    wierzcholki = new List<XPoint>
                     {
                        TopLT5,
                        TopST5,
                        TopRT5,
                        BottomLT5,
                        BottomSTT5,
                        BottomRT5,

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
                        OffsetLewa = StronaElementu == "Lewa" ? profileLeft : 0,
                        OffsetPrawa = StronaElementu == "Prawa" ? profileRight : 0,
                        OffsetDol = StronaElementu == "Dól" ? profileBottom : 0,
                        OffsetGora = StronaElementu == "Góra" ? profileTop : 0,
                        Strona = StronaElementu,
                        IndeksElementu = indeksElementu,
                        NazwaElementu = nazwaElemntu,
                        DlogoscElementu = bazowaDlugosc + ((dodajA ? profileLeft : 0) + (dodajB ? profileRight : 0)),
                        DlogoscNaGotowoElementu = bazowaDlugosc
                    });

                Console.WriteLine($"▶️ Element {i + 1}/{vertexCount} dodałem do ElementyRamyRysowane. Total elements now: {ElementyRamyRysowane.Count} - >3 rowIdProfil:{rowIdProfil} Angle: {angleDegrees}°");
   
                await Task.CompletedTask;

                if (ElementLiniowy) return;

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
            double tolerance = 0.0001)
        {
            // Wektor kierunku linii słupka
            double dx = dirEnd.X - dirStart.X;
            double dy = dirEnd.Y - dirStart.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 1e-8) len = 1; // zabezpieczenie przed zerowym wektorem
            dx /= len;
            dy /= len;

            if (!forward)
            {
                dx = -dx;
                dy = -dy;
            }

            // Kąt w stopniach (0° = w prawo, 90° = w górę)
            double angleDeg = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            Console.WriteLine($"🔷 FindFirstEdgeIntersectionByVector → basePoint: ({basePoint.X}, {basePoint.Y}), angle: {angleDeg:0.00}°, forward: {forward}");

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

                if (!inter.HasValue) continue;

                var p = inter.Value;

                // 🔥 KLUCZOWE — punkt w kierunku promienia
                double dot = (p.X - basePoint.X) * dx + (p.Y - basePoint.Y) * dy;
                if (dot <= tolerance) continue; // za plecami lub za mały margines

                double distSq = (p.X - basePoint.X) * (p.X - basePoint.X) + (p.Y - basePoint.Y) * (p.Y - basePoint.Y);
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closest = p;
                }
            }

            if (!closest.HasValue)
                throw new InvalidOperationException("No valid intersections");

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

        public List<XPoint> CalculateOffsetPolygon(
        List<XPoint> points,
        float profileLeft,
        float profileRight,
        float profileTop,
        float profileBottom)
        {
            int count = points.Count;

            if (count > 0)
                Console.WriteLine($"🔷 Calculating offset polygon for {count} X:{points[0].X} Y:{points[0].Y} ElementLiniowy:{ElementLiniowy} points with profiles L:{profileLeft}, R:{profileRight}, T:{profileTop}, B:{profileBottom}");

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


        public string OkreslStroneNaPodstawieKata(float angleDegrees, int i, List<XPoint> outer)
        {
            // Standardowy przypadek
            string strona = angleDegrees switch
            {
                >= 45 and < 135 => "Prawa",
                >= 135 and < 225 => "Dół",
                >= 225 and < 315 => "Lewa",
                _ => "Góra"
            };

            // KOREKTA: Jeśli element jest w górnej części i ma kąt bliski 0/360, 
            // to traktuj go jako "Góra" nawet jeśli logika mówi inaczej
            if (outer != null && outer.Count >= 2)
            {
                int next = (i + 1) % outer.Count;
                double edgeCenterY = (outer[i].Y + outer[next].Y) / 2;
                double minY = outer.Min(p => p.Y);
                double maxY = outer.Max(p => p.Y);

                // Jeśli środek krawędzi jest w górnej 1/3 okna
                if (edgeCenterY < minY + (maxY - minY) * 0.33)
                {
                    // Sprawdź czy kąt jest zbliżony do poziomego
                    bool isNearHorizontal = (Math.Abs(angleDegrees) < 30) ||
                                           (Math.Abs(angleDegrees - 360) < 30) ||
                                           (Math.Abs(angleDegrees - 180) < 30);

                    if (isNearHorizontal)
                    {
                        return "Góra";
                    }
                }
            }

            return strona;
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