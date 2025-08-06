using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Client.Pages.Models;
using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;
using System.Globalization;

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
        }

        public void AddElements(List<ShapeRegion> regions, string regionId, Dictionary<string, GeneratorState> generatorStates)
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

            var region = regions.FirstOrDefault(r => r.Id == regionId);

            if (region == null)
            {
                Console.WriteLine($"❌ Nie znaleziono regionu o ID: {regionId}");
                return;
            }

            var punkty = region.Wierzcholki;
            if (punkty == null || punkty.Count < 3)
            {
                Console.WriteLine($"❌ Region o ID: {regionId} ma zbyt mało punktów");
                return;
            }

            Wierzcholki = region.Wierzcholki;

            Console.WriteLine($"🟩 Generuj okno dla regionu ID {regionId} typu: {region.TypKsztaltu}");

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


            // 🔧 Profile z konfiguracji
            float profileLeft = (float)(MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa && e.Typ.Contains(slruchPoPrawej))?.PionPrawa ?? 0 -
                                        MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa && e.Typ.Contains(slruchPoPrawej))?.PionLewa ?? 0);
            float profileRight = (float)(MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa && e.Typ.Contains(slruchPoLewej))?.PionPrawa ?? 0 -
                                         MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa && e.Typ.Contains(slruchPoLewej))?.PionLewa ?? 0);
            float profileTop = (float)(MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeGora)?.PionPrawa ?? 0 -
                                       MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeGora)?.PionLewa ?? 0);
            float profileBottom = (float)(MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeDol)?.PionPrawa ?? 0 -
                                          MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeDol)?.PionLewa ?? 0);

            if(profileLeft == 0)
            {
                //Spróbuj bez słupka
                slruchPoLewej = "";
                profileLeft = (float)(MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa)?.PionPrawa ?? 0 -
                                        MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa)?.PionLewa ?? 0);
            }
            if(profileRight == 0)
            {
                //Spróbuj bez słupka
                slruchPoPrawej = "";
                profileRight = (float)(MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.PionPrawa ?? 0 -
                                         MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.PionLewa ?? 0);
            }

            Guid RowIdprofileLeft = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeLewa && e.Typ.Contains(slruchPoPrawej))?.RowId ?? Guid.Empty;
            Guid RowIdprofileRight = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujePrawa && e.Typ.Contains(slruchPoLewej))?.RowId ?? Guid.Empty;
            Guid RowIdprofileTop = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeGora)?.RowId ?? Guid.Empty;
            Guid RowIdprofileBottom = MVCKonfModelu.KonfSystem.FirstOrDefault(e => e.WystepujeDol)?.RowId ?? Guid.Empty;

            string NazwaObiektu = MVCKonfModelu.KonfSystem.First().Nazwa ?? "";

            Console.WriteLine($"📐 region.TypKsztaltu: {region.TypKsztaltu} profileLeft: {profileLeft}, profileRight: {profileRight}, profileTop: {profileTop}, profileBottom: {profileBottom}");

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
                NazwaObiektu
            );
            //}
        }

        private void GenerateGenericElementsWithJoins(
            List<XPoint> outer, List<XPoint> inner,
            float profileLeft, float profileRight, float profileTop, float profileBottom,
            string typKsztalt, string polaczenia, List<KonfSystem> model, string regionId,
            Guid rowIdprofileLeft, Guid rowIdprofileRight, Guid rowIdprofileTop, Guid rowIdprofileBottom,
            string NazwaObiektu)
        {
            int vertexCount = outer.Count;
            if (vertexCount < 3)
                throw new Exception("Polygon must have at least 3 vertices.");

            outer = RemoveDuplicateConsecutivePoints(outer);
            inner = RemoveDuplicateConsecutivePoints(inner);

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

                List<XPoint> wierzcholki;

                //Console.WriteLine($"▶️ DEBUG: Generating element {i + 1}/{vertexCount} with joins: {leftJoin} - {rightJoin}");

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

                        if(leftJoin == "T1" && rightJoin == "T4" && vertexCount > 4)
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
                else
                {
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

                switch (i)
                    {
                    case 0:
                        ElementyRamyRysowane.Add(new KsztaltElementu
                        {
                            TypKsztaltu = typKsztalt,
                            Wierzcholki = wierzcholki,
                            WypelnienieZewnetrzne = "wood-pattern",
                            WypelnienieWewnetrzne = KolorSzyby,
                            Grupa = NazwaObiektu + $" Góra-{i + 1} {wartoscX}/{wartoscY}",
                            ZIndex = Zindeks,
                            RowIdElementu = rowIdprofileTop,
                            IdRegion = regionId,
                            Kat = (int)angleDegrees,
                            Strona = "Góra",
                        });
                        break;
                    case 1:
                        ElementyRamyRysowane.Add(new KsztaltElementu
                        {
                            TypKsztaltu = typKsztalt,
                            Wierzcholki = wierzcholki,
                            WypelnienieZewnetrzne = "wood-pattern",
                            WypelnienieWewnetrzne = KolorSzyby,
                            Grupa = NazwaObiektu + $" Prawa-{i + 1} {wartoscX}/{wartoscY}",
                            ZIndex = Zindeks,
                            RowIdElementu = rowIdprofileRight,
                            IdRegion = regionId,
                            Kat = (int)angleDegrees,
                            Strona = "Prawa",
                        });
                        break;
                    case 2:
                        ElementyRamyRysowane.Add(new KsztaltElementu
                        {
                            TypKsztaltu = typKsztalt,
                            Wierzcholki = wierzcholki,
                            WypelnienieZewnetrzne = "wood-pattern",
                            WypelnienieWewnetrzne = KolorSzyby,
                            Grupa = NazwaObiektu + $" Dół-{i + 1} {wartoscX}/{wartoscY}",
                            ZIndex = Zindeks,
                            RowIdElementu = rowIdprofileBottom,
                            IdRegion = regionId,
                            Kat = (int)angleDegrees,
                            Strona = "Dół",
                        });
                        break;
                    case 3:
                        ElementyRamyRysowane.Add(new KsztaltElementu
                        {
                            TypKsztaltu = typKsztalt,
                            Wierzcholki = wierzcholki,
                            WypelnienieZewnetrzne = "wood-pattern",
                            WypelnienieWewnetrzne = KolorSzyby,
                            Grupa = NazwaObiektu + $" Lewa-{i + 1} {wartoscX}/{wartoscY}",
                            ZIndex = Zindeks,
                            RowIdElementu = rowIdprofileLeft,
                            IdRegion = regionId,
                            Kat = (int)angleDegrees,
                            Strona = "Lewa",
                        }); ;
                        break;
                }
            }
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

        private XPoint GetHorizontalIntersection(XPoint a, XPoint b, float y)
        {
            if (Math.Abs(a.Y - b.Y) < 1e-3f)
                return new XPoint(a.X, y); // linia pozioma – przyjmujemy X a

            float t = (y - (float)a.Y) / ((float)b.Y - (float)a.Y);
            float x = (float)a.X + t * ((float)b.X - (float)a.X);
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


        private List<XPoint> CalculateOffsetPolygon(
        List<XPoint> points,
        float profileLeft,
        float profileRight,
        float profileTop,
        float profileBottom)
        {
            int count = points.Count;
            if (count < 3)
                throw new ArgumentException("Polygon must have at least 3 points.");

            // Oblicz bounding box, żeby ocenić położenie boku
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

                // Środek boku
                float midX = ((float)p1.X + (float)p2.X) / 2f;
                float midY = ((float)p1.Y + (float)p2.Y) / 2f;

                // Domyślnie bez przesunięcia
                float offset = 0f;

                bool isHorizontal = Math.Abs(dy) < Math.Abs(dx);
                bool isVertical = !isHorizontal;

                // Ustal offset w zależności od położenia środka boku
                if (isHorizontal)
                {
                    // linia pozioma
                    offset = midY < (minY + maxY) / 2f ? profileTop : profileBottom;
                }
                else
                {
                    // linia pionowa
                    offset = midX < (minX + maxX) / 2f ? profileLeft : profileRight;
                }

                // Przesuń oba końce boku do środka (normalna do wnętrza)
                var p1Offset = new XPoint(p1.X + nx * offset, p1.Y + ny * offset);
                var p2Offset = new XPoint(p2.X + nx * offset, p2.Y + ny * offset);

                offsetLines.Add((p1Offset, p2Offset));
            }

            // Oblicz przecięcia sąsiednich przesuniętych boków
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