using GEORGE.Client.Pages.Models;
using GEORGE.Shared.Models;

namespace GEORGE.Client.Pages.Okna
{
    public class Generator : GenerujOkno
    {
        public new List<KsztaltElementu> ElementyRamyRysowane { get; set; } = new();
        public List<KonfSystem> KonfiguracjeSystemu { get; set; } = new(); // Dodaj tę linię

        public KonfModele? EdytowanyModel;

        public new MVCKonfModele? PowiazanyModel;


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
            //RowIdSystemu = Guid.NewGuid();
            //RowIdModelu = Guid.NewGuid();
            PowiazanyModel = null;
        }

        public void AddElements(List<ShapeRegion> regions)
        {
            if (regions == null) return;

            if (KonfiguracjeSystemu == null || PowiazanyModel == null)
            {
                Console.WriteLine($"Brak KonfiguracjeSystemu !!!!");
                return;
            }

            if (EdytowanyModel == null)
            {
                Console.WriteLine($"Brak EdytowanyModel !!!!");
                return;
            }

            Console.WriteLine($"EdytowanyModel.PolaczenieNaroza: {EdytowanyModel.PolaczenieNaroza}");

            Console.WriteLine($"Szerokosc: {Szerokosc}");

            foreach (var region in regions)
            {
                var punkty = region.Wierzcholki;
                if (punkty == null || punkty.Count < 3)
                    continue;

                Console.WriteLine($"GenerujOkno: Przetwarzanie regionu typu: {region.TypKsztaltu}");

                // Wyznacz bounding box
                float minX = (float)punkty.Min(p => p.X);
                float maxX = (float)punkty.Max(p => p.X);
                float minY = (float)punkty.Min(p => p.Y);
                float maxY = (float)punkty.Max(p => p.Y);

                Console.WriteLine($"EdytowanyModel.NazwaKonfiguracji: {EdytowanyModel.NazwaKonfiguracji}");


                // Oblicz rzeczywiste grubości z modelu

                float profileLeft = (float)(PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujeLewa)?.PionPrawa ?? 0 -
                                          PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujeLewa)?.PionLewa ?? 0);
                float profileRight = (float)(PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.PionPrawa ?? 0 -
                                           PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.PionLewa ?? 0);
                float profileTop = (float)(PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujeGora)?.PionPrawa ?? 0 -
                                         PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujeGora)?.PionLewa ?? 0);
                float profileBottom = (float)(PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujeDol)?.PionPrawa ?? 0 -
                                            PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujeDol)?.PionLewa ?? 0);

                Console.WriteLine($"System ilość konfiguracji: {KonfiguracjeSystemu.Count()} Pobrane grubości profili: profileLeft: {profileLeft} profileRight: {profileRight} profileTop: {profileTop} profileBottom: {profileBottom}");

                // Oblicz wewnętrzny kontur
                var wewnetrznyKontur = CalculateOffsetPolygon(
                punkty,
                profileLeft, profileRight, profileTop, profileBottom);


                // Generuj elementy ramy w zależności od typu kształtu
                if (region.TypKsztaltu == "prostokąt" || region.TypKsztaltu == "kwadrat")
                {
                    GenerateRectangleElements(punkty, wewnetrznyKontur, profileLeft, profileRight, profileTop, profileBottom, region.TypKsztaltu, EdytowanyModel.PolaczenieNaroza, KonfiguracjeSystemu);

                }
                else if (region.TypKsztaltu == "trójkąt")
                {
                    GenerateTriangleElements(punkty, wewnetrznyKontur, profileLeft, profileRight, profileTop, profileBottom);
                }
                else
                {
                    //GenerateGenericElements(punkty, wewnetrznyKontur);
                    // Wywołanie funkcji
                    GenerateGenericElementsWithJoins(
                        punkty,          // List<XPoint> - zewnętrzne punkty konturu
                        wewnetrznyKontur,// List<XPoint> - wewnętrzne punkty przeszklenia
                        profileLeft,     // float - grubość profilu lewego
                        profileRight,    // float - grubość profilu prawego
                        profileTop,      // float - grubość profilu górnego
                        profileBottom,   // float - grubość profilu dolnego
                        region.TypKsztaltu, // string - typ kształtu (np. "prostokąt")
                        EdytowanyModel.PolaczenieNaroza, // string - np. "T1;T2;T3;T1"
                        KonfiguracjeSystemu              // List<KonfSystem> - konfiguracje systemowe,
                    );
                }
            }

            //0 - Lewy górny
            //1 - Prawy górny
            //2 - Prawy dolny
            //3 - Lewy dolny

        }
        private void GenerateRectangleElements(
                List<XPoint> outer, List<XPoint> inner,
                float profileLeft, float profileRight, float profileTop, float profileBottom,
                string typKsztalt, string polaczenia, List<KonfSystem> model)
        {
            float minX = outer.Min(p => (float)p.X);
            float maxX = outer.Max(p => (float)p.X);
            float minY = outer.Min(p => (float)p.Y);
            float maxY = outer.Max(p => (float)p.Y);

            float imageWidth = maxX - minX;
            float imageHeight = maxY - minY;

            var polaczeniaArray = polaczenia.Split(';')
                .Select(p => p.Split('-'))
                .Select(parts => (kat: int.Parse(parts[0]), typ: parts[1].Trim()))
                .ToArray();

            if (polaczeniaArray.Length != 4)
                throw new Exception("Oczekiwano 4 połączeń narożników.");

            var joinTypes = new[]
            {
        (Left: polaczeniaArray[0].typ, Right: polaczeniaArray[1].typ), // Top
        (Left: polaczeniaArray[1].typ, Right: polaczeniaArray[2].typ), // Right
        (Left: polaczeniaArray[2].typ, Right: polaczeniaArray[3].typ), // Bottom
        (Left: polaczeniaArray[3].typ, Right: polaczeniaArray[0].typ), // Left
    };

            // Obliczenia pozycji i długości
            int topX = (joinTypes[0].Left == "T3") ? (int)profileLeft : 0;
            int topW = (int)imageWidth - ((joinTypes[0].Left == "T3" ? (int)profileLeft : 0) +
                                          (joinTypes[0].Right == "T3" ? (int)profileRight : 0));

            int rightY = (joinTypes[1].Left == "T1") ? (int)profileTop : 0;
            int rightH = (int)imageHeight - ((joinTypes[1].Left == "T1" ? (int)profileTop : 0) +
                                             (joinTypes[1].Right == "T1" ? (int)profileBottom : 0));

            int bottomX = (joinTypes[2].Left == "T3") ? (int)profileLeft : 0;
            int bottomW = (int)imageWidth - ((joinTypes[2].Left == "T3" ? (int)profileLeft : 0) +
                                             (joinTypes[2].Right == "T3" ? (int)profileRight : 0));

            int leftY = (joinTypes[3].Left == "T1" || joinTypes[0].Left == "T1") ? (int)profileTop : 0;
            int leftH = (int)imageHeight - ((joinTypes[3].Left == "T1" || joinTypes[2].Left == "T4" ? (int)profileTop : 0) +
                                            (joinTypes[3].Right == "T1" ? (int)profileBottom : 0));

            void AddElement(int index, int x, int y, int w, int h, string typLewy, string typPrawy, string grupa)
            {
                int offset = (index % 2 == 0) ? h : w;
                bool isLeftT2 = typLewy == "T2";
                bool isRightT2 = typPrawy == "T2";

                var points = index switch
                {
                    0 => new List<XPoint> // Góra
            {
                new(minX + x, minY + y),
                new(minX + x + w, minY + y),
                new(minX + x + w - (isRightT2 ? offset : 0), minY + y + h),
                new(minX + x + (isLeftT2 ? offset : 0), minY + y + h),
            },
                    1 => new List<XPoint> // Prawa
            {
                new(maxX - profileRight, minY + y + (isLeftT2 ? offset : 0)),
                new(maxX, minY + y),
                new(maxX, minY + y + h),
                new(maxX - profileRight, minY + y + h - (isRightT2 ? offset : 0)),
            },
                    2 => new List<XPoint> // Dół
            {
                new(minX + x + (isLeftT2 ? offset : 0), maxY - h),
                new(minX + x + w - (isRightT2 ? offset : 0), maxY - h),
                new(minX + x + w, maxY),
                new(minX + x, maxY),
            },
                    3 => new List<XPoint> // Lewa
            {
                new(minX, minY + y),
                new(minX + profileLeft, minY + y + (isRightT2 ? offset : 0)),
                new(minX + profileLeft, minY + y + h - (isLeftT2 ? offset : 0)),
                new(minX, minY + y + h),
            },
                    _ => throw new ArgumentOutOfRangeException()
                };

                ElementyRamyRysowane.Add(new KsztaltElementu
                {
                    TypKsztaltu = "trapez",
                    Wierzcholki = points,
                    WypelnienieZewnetrzne = "wood-pattern",
                    WypelnienieWewnetrzne = KolorSzyby,
                    Grupa = grupa
                });
            }

            // Generuj wszystkie ramy
            AddElement(0, topX, 0, topW, (int)profileTop, joinTypes[0].Left, joinTypes[0].Right, "Gora");
            AddElement(1, 0, rightY, (int)profileRight, rightH, joinTypes[1].Left, joinTypes[1].Right, "Prawo");
            AddElement(2, bottomX, 0, bottomW, (int)profileBottom, joinTypes[2].Left, joinTypes[2].Right, "Dol");
            AddElement(3, 0, leftY, (int)profileLeft, leftH, joinTypes[3].Left, joinTypes[3].Right, "Lewo");
        }

        private void GenerateTriangleElements(List<XPoint> outer, List<XPoint> inner,
            float leftOffset, float rightOffset, float topOffset, float bottomOffset)
        {

            double maxLength = 0;
            int baseIndex1 = 0, baseIndex2 = 1;

            for (int i = 0; i < 3; i++)
            {
                int next = (i + 1) % 3;
                double length = Math.Sqrt(Math.Pow(outer[next].X - outer[i].X, 2) +
                                          Math.Pow(outer[next].Y - outer[i].Y, 2));
                if (length > maxLength)
                {
                    maxLength = length;
                    baseIndex1 = i;
                    baseIndex2 = next;
                }
            }

            int vertexIndex = Enumerable.Range(0, 3).First(i => i != baseIndex1 && i != baseIndex2);

            for (int i = 0; i < 3; i++)
            {
                int next = (i + 1) % 3;

                bool isBase = (i == baseIndex1 && next == baseIndex2) || (i == baseIndex2 && next == baseIndex1);
                string grupa;

                if (isBase)
                {
                    grupa = "Podstawa";
                }
                else if (i == vertexIndex || next == vertexIndex)
                {
                    // Rozróżnij boki względem X wierzchołka
                    var drugiPunkt = (i == vertexIndex) ? outer[next] : outer[i];
                    grupa = drugiPunkt.X < outer[vertexIndex].X ? "LewyBok" : "PrawyBok";
                }
                else
                {
                    grupa = "NieznanyBok";
                }

                ElementyRamyRysowane.Add(new KsztaltElementu
                {
                    TypKsztaltu = isBase ? "prostokat" : "trapez",
                    Wierzcholki = new List<XPoint>
            {
                outer[i],
                outer[next],
                inner[next],
                inner[i]
            },
                    WypelnienieZewnetrzne = "wood-pattern",
                    WypelnienieWewnetrzne = KolorSzyby,
                    Grupa = grupa
                });
            }
        }


        //private void GenerateGenericElementsWithJoins(
        //List<XPoint> outer, List<XPoint> inner,
        //float profileLeft, float profileRight, float profileTop, float profileBottom,
        //string typKsztalt, string polaczenia, List<KonfSystem> model)
        //{
        //    int vertexCount = outer.Count;
        //    if (vertexCount < 3)
        //        throw new Exception("Polygon must have at least 3 vertices.");


        //    Console.WriteLine("▶️ DEBUG: Start GenerateGenericElementsWithJoins");

        //    Console.WriteLine("🔷 Outer:");
        //    for (int i = 0; i < outer.Count; i++)
        //        Console.WriteLine($"  [{i}] X: {outer[i].X:F2}, Y: {outer[i].Y:F2}");

        //    Console.WriteLine("🔶 Inner:");
        //    for (int i = 0; i < inner.Count; i++)
        //        Console.WriteLine($"  [{i}] X: {inner[i].X:F2}, Y: {inner[i].Y:F2}");

        //    var polaczeniaArray = polaczenia.Split(';')
        //        .Select(p => p.Split('-'))
        //        .Select(parts => (kat: int.Parse(parts[0]), typ: parts[1].Trim()))
        //        .ToArray();

        //    if (polaczeniaArray.Length != vertexCount)
        //        throw new Exception($"Expected {vertexCount} corner connections.");

        //    for (int i = 0; i < vertexCount; i++)
        //    {
        //        int next = (i + 1) % vertexCount;
        //        var leftJoin = polaczeniaArray[i].typ;
        //        var rightJoin = polaczeniaArray[next].typ;

        //        XPoint current = outer[i];
        //        XPoint nextPt = outer[next];

        //        // Oblicz wektor kierunku krawędzi
        //        float dx = (float)(nextPt.X - current.X);
        //        float dy = (float)(nextPt.Y - current.Y);
        //        float length = (float)Math.Sqrt(dx * dx + dy * dy);

        //        if (length == 0) continue;

        //        // Oblicz wektory normalne
        //        float nx = -dy / length;
        //        float ny = dx / length;

        //        // Określ grubość profilu w zależności od orientacji
        //        float profileThickness = Math.Abs(dx) > Math.Abs(dy)
        //            ? (ny > 0 ? profileTop : profileBottom)
        //            : (nx > 0 ? profileRight : profileLeft);

        //        // Oblicz przesunięcia dla połączeń
        //        float leftOffset = GetJoinOffset(leftJoin, profileThickness);
        //        float rightOffset = GetJoinOffset(rightJoin, profileThickness);

        //        // Oblicz punkty wewnętrzne
        //        var innerStart = new XPoint(
        //            current.X + nx * profileThickness,
        //            current.Y + ny * profileThickness);

        //        var innerEnd = new XPoint(
        //            nextPt.X + nx * profileThickness,
        //            nextPt.Y + ny * profileThickness);

        //        // Dostosuj punkty do typów połączeń
        //        innerStart.X += dx * leftOffset / length;
        //        innerStart.Y += dy * leftOffset / length;

        //        innerEnd.X -= dx * rightOffset / length;
        //        innerEnd.Y -= dy * rightOffset / length;

        //        ElementyRamyRysowane.Add(new KsztaltElementu
        //        {
        //            TypKsztaltu = "trapez",
        //            Wierzcholki = new List<XPoint> { current, nextPt, innerEnd, innerStart },
        //            WypelnienieZewnetrzne = "wood-pattern",
        //            WypelnienieWewnetrzne = KolorSzyby,
        //            Grupa = $"Bok{i + 1}"
        //        });

        //        break;
        //    }
        //}
        private void GenerateGenericElementsWithJoins(
            List<XPoint> outer, List<XPoint> inner,
            float profileLeft, float profileRight, float profileTop, float profileBottom,
            string typKsztalt, string polaczenia, List<KonfSystem> model)
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

                float dx = (float)(outerEnd.X - outerStart.X);
                float dy = (float)(outerEnd.Y - outerStart.Y);
                float length = MathF.Sqrt(dx * dx + dy * dy);
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

                List<XPoint> wierzcholki;

                if (leftJoin == "T1" && rightJoin == "T1")
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

                        var outerTop = GetHorizontalIntersection(outerStart, outerEnd, (float)topY);
                        var outerBottom = GetHorizontalIntersection(outerStart, outerEnd, (float)bottomY);

                        var innerTop = GetHorizontalIntersection(inner[i], inner[next], (float)topY);
                        var innerBottom = GetHorizontalIntersection(inner[i], inner[next], (float)bottomY);

                        wierzcholki = new List<XPoint> {
                    outerTop, outerBottom, innerBottom, innerTop
                };
                    }
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

                ElementyRamyRysowane.Add(new KsztaltElementu
                {
                    TypKsztaltu = typKsztalt,
                    Wierzcholki = wierzcholki,
                    WypelnienieZewnetrzne = "wood-pattern",
                    WypelnienieWewnetrzne = KolorSzyby,
                    Grupa = $"Bok{i + 1}"
                });
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


        private List<(int kat, string typ)> ExtendPolaczeniaForPolygon(List<XPoint> outer, string polaczenia)
        {
            var basePolaczenia = polaczenia.Split(';')
                .Select(p => p.Split('-'))
                .Select(parts => (kat: int.Parse(parts[0]), typ: parts[1].Trim()))
                .ToArray();

            if (basePolaczenia.Length != 4)
                throw new Exception("Expected 4 base connections for extension");

            var extended = new List<(int kat, string typ)>();
            int vertexCount = outer.Count;

            // Wyznacz środek obszaru
            float centerX = (float)((outer.Min(p => p.X) + outer.Max(p => p.X)) / 2);
            float centerY = (float)((outer.Min(p => p.Y) + outer.Max(p => p.Y)) / 2);

            for (int i = 0; i < vertexCount; i++)
            {
                int next = (i + 1) % vertexCount;

                var p1 = outer[i];
                var p2 = outer[next];

                float dx = (float)Math.Abs(p2.X - p1.X);
                float dy = (float)Math.Abs(p2.Y - p1.Y);
                float midX = (float)((p1.X + p2.X) / 2);
                float midY = (float)((p1.Y + p2.Y) / 2);

                int index = 0; // domyślnie Top

                if (dy < 10)
                    index = midY < centerY ? 0 : 2; // Top / Bottom
                else if (dx < 10)
                    index = midX < centerX ? 3 : 1; // Left / Right

                extended.Add(basePolaczenia[index]);
            }

            return extended;
        }


        private XPoint GetHorizontalIntersection(XPoint a, XPoint b, float y)
        {
            if (Math.Abs(a.Y - b.Y) < 1e-3f)
                return new XPoint(a.X, y); // linia pozioma – przyjmujemy X a

            float t = (y - (float)a.Y) / ((float)b.Y - (float)a.Y);
            float x = (float)a.X + t * ((float)b.X - (float)a.X);
            return new XPoint(x, y);
        }

        private XPoint GetIntersectionWithEdge(XPoint a1, XPoint a2, XPoint b1, XPoint b2)
        {
            float dx1 = (float)(a2.X - a1.X);
            float dy1 = (float)(a2.Y - a1.Y);
            float dx2 = (float)(b2.X - b1.X);
            float dy2 = (float)(b2.Y - b1.Y);

            float determinant = dx1 * dy2 - dy1 * dx2;
            if (Math.Abs(determinant) < 1e-6)
            {
                return new XPoint((a1.X + b1.X) / 2, (a1.Y + b1.Y) / 2); // fallback
            }

            float t = (float)((b1.X - a1.X) * dy2 - (b1.Y - a1.Y) * dx2) / determinant;

            return new XPoint(
                a1.X + t * dx1,
                a1.Y + t * dy1
            );
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