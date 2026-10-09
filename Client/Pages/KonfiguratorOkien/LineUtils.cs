using GEORGE.Client.Pages.Models;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public static class LineUtils
    {
        private const double Tolerance = 0.001;
        private const double MoveTolerance = 0.5;

        // =====================================================================
        // USUWANIE LINII POZA KSZTAŁTAMI
        // =====================================================================

        public static async Task RemoveLinesOutsideShapes(List<IShapeDC> shapes)
        {
            var closedShapes = shapes.Where(s => s is not XLineShape).ToList();
            var lines = shapes.OfType<XLineShape>().ToList();

            foreach (var line in lines)
            {
                bool intersectsAny = closedShapes.Any(s =>
                {
                    var bbox = s.GetBoundingBox();

                    // Szybki test bbox
                    if (!LineIntersectsBoundingBox(line, bbox))
                        return false;

                    // Test rzeczywistych krawędzi
                    var edges = PobierzKrawedzieKsztaltu(s, bbox);

                    foreach (var (start, end) in edges)
                    {
                        if (FindIntersection(start.X, start.Y, end.X, end.Y,
                                             line.X1, line.Y1, line.X2, line.Y2,
                                             out _, out _))
                            return true;
                    }

                    // Sprawdź, czy któryś koniec linii leży wewnątrz kształtu
                    return IsPointInsideShape(line.X1, line.Y1, s, edges)
                        || IsPointInsideShape(line.X2, line.Y2, s, edges);
                });

                if (!intersectsAny)
                    shapes.Remove(line);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Zwraca listę krawędzi (odcinków) kształtu.
        /// Dla prostokątów i kwadratów używa bbox, dla pozostałych – rzeczywiste krawędzie.
        /// </summary>
        private static List<(XPoint Start, XPoint End)> PobierzKrawedzieKsztaltu(
            IShapeDC shape, BoundingBox bbox)
        {
            return shape switch
            {
                XTriangleShape t => t.GetEdges(),
                XTrapezoidShape trap => trap.GetEdges(),
                XHouseShape h => h.GetEdges(),
                XRoundedTopRectangleShape r => r.GetEdges(),
                XRoundedTopRectangleShapeFixed rf => rf.GetEdges(),
                XRoundedRectangleShape rr => rr.GetEdges(),
                _ => GetBoundingBoxEdges(bbox)
            };
        }

        private static List<(XPoint Start, XPoint End)> GetBoundingBoxEdges(BoundingBox bbox)
        {
            var p1 = new XPoint(bbox.X, bbox.Y);
            var p2 = new XPoint(bbox.X + bbox.Width, bbox.Y);
            var p3 = new XPoint(bbox.X + bbox.Width, bbox.Y + bbox.Height);
            var p4 = new XPoint(bbox.X, bbox.Y + bbox.Height);

            return new List<(XPoint, XPoint)>
            {
                (p1, p2), (p2, p3), (p3, p4), (p4, p1)
            };
        }

        private static bool IsPointInsideShape(
            double x, double y, IShapeDC shape, List<(XPoint Start, XPoint End)> edges)
        {
            // Test promienia na krawędziach
            return IsPointInsidePolygon(
                x, y,
                edges.SelectMany(e => new[] { e.Start, e.End }).Distinct().ToList());
        }

        // =====================================================================
        // WYDŁUŻANIE LINII DO KSZTAŁTÓW
        // =====================================================================

        public static async Task ExtendLinesToShapes(List<IShapeDC> shapes, double scaleFactor)
        {
            var closedShapes = shapes.Where(s => s is not XLineShape).ToList();
            var lines = shapes.OfType<XLineShape>().ToList();

            foreach (var lineGroup in GroupRelatedLines(lines))
            {
                // ⭐ Grupa NIEKOLINIOWA – użytkownik zmienił położenie jednej linii w grupie.
                //    Nie traktuj grupy jako całości – rozciągnij każdą linię OSOBNO.
                if (lineGroup.Count > 1 && !CzyGrupaJestKoliniowa(lineGroup))
                {
                    Console.WriteLine($"⚠️ ExtendLinesToShapes: grupa {lineGroup.First().SplitGroupId} NIEKOLINIOWA " +
                                      $"(użytkownik zmienił położenie) – traktuję każdą linię osobno");

                    foreach (var line in lineGroup)
                    {
                        Console.WriteLine($"   Line {line.ID?.Substring(0, 8)}: " +
                                          $"({line.X1:F1},{line.Y1:F1}) → ({line.X2:F1},{line.Y2:F1})");

                        foreach (var shape in closedShapes)
                        {
                            var bbox = shape.GetBoundingBox();
                            var extended = await ExtendLineToBoundingBox(line, bbox, scaleFactor);
                            SetLineEndpoints(line, extended.X1, extended.Y1, extended.X2, extended.Y2);
                        }
                    }

                    continue;
                }

                // Grupa koliniowa – traktuj jako jedną podzieloną linię
                foreach (var shape in closedShapes)
                {
                    var bbox = shape.GetBoundingBox();

                    if (lineGroup.Count == 1)
                    {
                        var line = lineGroup[0];
                        var extended = await ExtendLineToBoundingBox(line, bbox, scaleFactor);
                        SetLineEndpoints(line, extended.X1, extended.Y1, extended.X2, extended.Y2);
                    }
                    else
                    {
                        var envelope = CreateGroupEnvelope(lineGroup);
                        var extended = await ExtendLineToBoundingBox(envelope, bbox, scaleFactor);
                        SetGroupOuterEndpoints(lineGroup, extended.X1, extended.Y1, extended.X2, extended.Y2);
                    }
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Sprawdza, czy linie w grupie leżą na tej samej osi (pion lub poziom).
        /// Jeśli tak – można je traktować jako jedną linię i tworzyć wspólną envelope.
        /// Jeśli nie – użytkownik zmienił ręcznie położenie jednej z nich i grupy nie wolno łączyć.
        /// </summary>
        private static bool CzyGrupaJestKoliniowa(IReadOnlyCollection<XLineShape> lineGroup)
        {
            if (lineGroup.Count <= 1) return true;

            var first = lineGroup.First();

            bool firstVertical = Math.Abs(first.X2 - first.X1) < Tolerance;
            bool firstHorizontal = Math.Abs(first.Y2 - first.Y1) < Tolerance;

            if (!firstVertical && !firstHorizontal)
            {
                // Pierwsza linia skośna – grupa „koliniowa”, jeśli wszystkie są tą samą skośną
                foreach (var line in lineGroup.Skip(1))
                {
                    double dx1 = first.X2 - first.X1, dy1 = first.Y2 - first.Y1;
                    double dx2 = line.X2 - line.X1, dy2 = line.Y2 - line.Y1;

                    // Sprawdź równoległość (iloczyn wektorowy = 0)
                    double cross = dx1 * dy2 - dy1 * dx2;
                    if (Math.Abs(cross) > 0.5) return false;

                    // Sprawdź, czy leżą na tej samej prostej
                    double cross2 = dx1 * (line.Y1 - first.Y1) - dy1 * (line.X1 - first.X1);
                    if (Math.Abs(cross2) > 0.5) return false;
                }
                return true;
            }

            if (firstVertical)
            {
                // Wszystkie muszą mieć to samo X
                foreach (var line in lineGroup.Skip(1))
                {
                    if (Math.Abs(line.X1 - first.X1) > 0.5) return false;
                    if (Math.Abs(line.X2 - first.X2) > 0.5) return false;
                }
                return true;
            }

            // Poziome
            foreach (var line in lineGroup.Skip(1))
            {
                if (Math.Abs(line.Y1 - first.Y1) > 0.5) return false;
                if (Math.Abs(line.Y2 - first.Y2) > 0.5) return false;
            }
            return true;
        }

        /// <summary>
        /// Wydłuża linię do krawędzi bounding boxa.
        /// ⭐ Poprawka: przekazuje WSZYSTKIE flagi linii (w tym IsSkosna).
        /// </summary>
        public static async Task<XLineShape> ExtendLineToBoundingBox(
           XLineShape line,
           BoundingBox bbox,
           double scaleFactor)
        {
            double x1 = line.X1;
            double y1 = line.Y1;
            double x2 = line.X2;
            double y2 = line.Y2;

            double dx = x2 - x1;
            double dy = y2 - y1;

            XLineShape result;

            // ============================================================
            // PIONOWA
            // ============================================================
            // X MUSI POZOSTAĆ STAŁY.
            // Wydłużamy wyłącznie Y.
            // ============================================================
            if (Math.Abs(dx) < Tolerance)
            {
                double fixedX = (x1 + x2) / 2.0;

                result = new XLineShape(
                    fixedX,
                    bbox.Y,
                    fixedX,
                    bbox.Y + bbox.Height,
                    scaleFactor,
                    line.NazwaObj,
                    line.RuchomySlupek,
                    line.PionPoziom,
                    line.DualRama,
                    line.GenerowaneZRamy,
                    line.StalySlupek,
                    line.IsSkosna)
                {
                    ID = line.ID,
                    SplitGroupId = line.SplitGroupId
                };

                await Task.CompletedTask;
                return result;
            }

            // ============================================================
            // POZIOMA
            // ============================================================
            // Y MUSI POZOSTAĆ STAŁY.
            // Wydłużamy wyłącznie X.
            // ============================================================
            if (Math.Abs(dy) < Tolerance)
            {
                double fixedY = (y1 + y2) / 2.0;

                result = new XLineShape(
                    bbox.X,
                    fixedY,
                    bbox.X + bbox.Width,
                    fixedY,
                    scaleFactor,
                    line.NazwaObj,
                    line.RuchomySlupek,
                    line.PionPoziom,
                    line.DualRama,
                    line.GenerowaneZRamy,
                    line.StalySlupek,
                    line.IsSkosna)
                {
                    ID = line.ID,
                    SplitGroupId = line.SplitGroupId
                };

                await Task.CompletedTask;
                return result;
            }

            // ============================================================
            // SKOŚNA
            // ============================================================
            // Dla linii ukośnych pozostawiamy dotychczasowy mechanizm.
            // ============================================================

            double leftFactor = (bbox.X - x1) / dx;
            double rightFactor = ((bbox.X + bbox.Width) - x1) / dx;

            double topFactor = (bbox.Y - y1) / dy;
            double bottomFactor = ((bbox.Y + bbox.Height) - y1) / dy;

            double minFactor = Math.Min(
                leftFactor,
                Math.Min(
                    rightFactor,
                    Math.Min(topFactor, bottomFactor)));

            double maxFactor = Math.Max(
                leftFactor,
                Math.Max(
                    rightFactor,
                    Math.Max(topFactor, bottomFactor)));

            double newX1 = x1 + dx * minFactor;
            double newY1 = y1 + dy * minFactor;

            double newX2 = x1 + dx * maxFactor;
            double newY2 = y1 + dy * maxFactor;

            result = new XLineShape(
                newX1,
                newY1,
                newX2,
                newY2,
                scaleFactor,
                line.NazwaObj,
                line.RuchomySlupek,
                line.PionPoziom,
                line.DualRama,
                line.GenerowaneZRamy,
                line.StalySlupek,
                line.IsSkosna)
            {
                ID = line.ID,
                SplitGroupId = line.SplitGroupId
            };

            await Task.CompletedTask;
            return result;
        }


        // =====================================================================
        // PRZYCINANIE LINII DO BBOX (prawdziwe skracanie)
        // =====================================================================

        /// <summary>
        /// ⭐ Poprawka: prawdziwe przycinanie linii do bboxa.
        /// Jeśli linia wystaje poza bbox – obcina do krawędzi.
        /// Jeśli linia jest w środku – zostawia bez zmian.
        /// </summary>
        public static async Task ShortenLineToBoundingBox(
            XLineShape line, BoundingBox bbox, double _scaleFactor)
        {
            var intersections = new List<XPoint>();

            CheckEdgeIntersection(bbox.X, bbox.Y, bbox.X + bbox.Width, bbox.Y, line, intersections);
            CheckEdgeIntersection(bbox.X + bbox.Width, bbox.Y, bbox.X + bbox.Width, bbox.Y + bbox.Height, line, intersections);
            CheckEdgeIntersection(bbox.X, bbox.Y + bbox.Height, bbox.X + bbox.Width, bbox.Y + bbox.Height, line, intersections);
            CheckEdgeIntersection(bbox.X, bbox.Y, bbox.X, bbox.Y + bbox.Height, line, intersections);

            intersections = intersections
                .Distinct()
                .OrderBy(p => Distance(line.X1, line.Y1, p.X, p.Y))
                .ToList();

            if (intersections.Count == 2)
            {
                line.X1 = intersections[0].X;
                line.Y1 = intersections[0].Y;
                line.X2 = intersections[1].X;
                line.Y2 = intersections[1].Y;
            }
            else if (intersections.Count == 1)
            {
                bool startWewnatrz = bbox.Contains(line.X1, line.Y1);
                bool endWewnatrz = bbox.Contains(line.X2, line.Y2);

                if (startWewnatrz && !endWewnatrz)
                {
                    line.X2 = intersections[0].X;
                    line.Y2 = intersections[0].Y;
                }
                else if (!startWewnatrz && endWewnatrz)
                {
                    line.X1 = intersections[0].X;
                    line.Y1 = intersections[0].Y;
                }
            }
            // 0 przecięć: linia w całości wewnątrz lub w całości zewnątrz – bez zmian

            await Task.CompletedTask;
        }

        // =====================================================================
        // PRZYCINANIE LINII DO WSZYSTKICH ZAMKNIĘTYCH KSZTAŁTÓW
        // =====================================================================
        public static async Task ShortenLinesInsideShapes(List<IShapeDC> shapes, double _scaleFactor)
        {
            var closedShapes = shapes.Where(s => s is not XLineShape).ToList();
            var lines = shapes.OfType<XLineShape>().ToList();

            foreach (var lineGroup in GroupRelatedLines(lines))
            {
                // ⭐ Grupa NIEKOLINIOWA – traktuj każdą linię osobno
                if (lineGroup.Count > 1 && !CzyGrupaJestKoliniowa(lineGroup))
                {
                    Console.WriteLine($"⚠️ ShortenLinesInsideShapes: grupa {lineGroup.First().SplitGroupId} NIEKOLINIOWA – traktuję osobno");

                    foreach (var line in lineGroup)
                    {
                        foreach (var shape in closedShapes)
                        {
                            await ShortenLineInsideShapeForBoundary(line, shape, _scaleFactor);
                        }
                    }
                    continue;
                }

                // Grupa koliniowa – traktuj jako jedną linię
                foreach (var shape in closedShapes)
                {
                    if (lineGroup.Count == 1)
                    {
                        await ShortenLineInsideShapeForBoundary(lineGroup[0], shape, _scaleFactor);
                    }
                    else
                    {
                        var envelope = CreateGroupEnvelope(lineGroup);
                        await ShortenLineInsideShapeForBoundary(envelope, shape, _scaleFactor);
                        SetGroupOuterEndpoints(lineGroup, envelope.X1, envelope.Y1, envelope.X2, envelope.Y2);
                    }
                }
            }

            await Task.CompletedTask;
        }

        private static async Task ShortenLineInsideShapeForBoundary(
            XLineShape line, IShapeDC shape, double scaleFactor)
        {
            await ShortenLineToBoundingBox(line, shape.GetBoundingBox(), scaleFactor);

            switch (shape)
            {
                case XCircleShape circle:
                    await ShortenLineInsideCircle(line, circle);
                    break;
                case XTriangleShape triangle:
                    await ShortenLineInsidePolygon(line, triangle.GetVertices());
                    break;
                case XHouseShape house:
                    await ShortenLineToShape(line, house.GetEdges());
                    break;
                case XRoundedTopRectangleShape rounded:
                    await ShortenLineInsideEdges(line, rounded.GetEdges(),
                        new XPoint(rounded.X + rounded.Width / 2, rounded.Y + rounded.Radius),
                        rounded.Radius);
                    break;
                case XRoundedTopRectangleShapeFixed roundedf:
                    await ShortenLineInsideEdges(line, roundedf.GetEdges(),
                        new XPoint(roundedf.X + roundedf.Width / 2, roundedf.Y + roundedf.Radius),
                        roundedf.Radius);
                    break;
                case XRoundedRectangleShape roundedRect:
                    await ShortenLineInsideEdges(line, roundedRect.GetEdges(),
                        new XPoint(roundedRect.X + roundedRect.Width / 2, roundedRect.Y + roundedRect.Radius),
                        roundedRect.Radius);
                    break;
                case XTrapezoidShape trap:
                    await ShortenLineToShape(line, trap.GetEdges());
                    break;
                default:
                    // Prostokąt, kwadrat – bbox już przyciął
                    break;
            }
        }

        // =====================================================================
        // ROZMIESZCZANIE LINII
        // =====================================================================
        public static async Task DistributeLines(
        List<IShapeDC> shapes,
        bool recznaZmiana,
        bool wymusWysrodkowanie = false)
        {
            if (recznaZmiana) return;

            var lines = shapes.OfType<XLineShape>().ToList();
            if (lines.Count == 0) return;

            var closedShapes = shapes.Where(s => s is not XLineShape).ToList();
            if (closedShapes.Count == 0) return;

            double minX = closedShapes.Min(s => s.GetBoundingBox().Left);
            double maxX = closedShapes.Max(s => s.GetBoundingBox().Right);
            double minY = closedShapes.Min(s => s.GetBoundingBox().Top);
            double maxY = closedShapes.Max(s => s.GetBoundingBox().Bottom);

            // ⭐ Sprawdź, czy linie są już rozmieszczone
            //    (pomijane, gdy wymusWysrodkowanie == true)
            if (!wymusWysrodkowanie)
            {
                bool wszystkieWDomyslnychPozycjach = lines.All(l =>
                    l.X1 < minX - 1 || l.X1 > maxX + 1 ||
                    l.Y1 < minY - 1 || l.Y1 > maxY + 1);

                if (!wszystkieWDomyslnychPozycjach)
                {
                    Console.WriteLine("⏭️ DistributeLines: linie już są rozmieszczone, pomijam");
                    return;
                }
            }
            else
            {
                Console.WriteLine("📏 DistributeLines: WYMUSZAM wyśrodkowanie");
            }

            // === Grupowanie linii ===
            var verticalLineGroups = GroupLinesForDistribution(
                lines.Where(l => Math.Abs(l.X1 - l.X2) < Tolerance),
                line => line.X1);

            var horizontalLineGroups = GroupLinesForDistribution(
                lines.Where(l => Math.Abs(l.Y1 - l.Y2) < Tolerance),
                line => line.Y1);

            const double MinOffsetFromAxis = 1.0;

            // ============================================================
            // PIONOWE
            // ============================================================
            if (verticalLineGroups.Any())
            {
                if (verticalLineGroups.Count == 1)
                {
                    // Jedna linia pionowa → środek X
                    var group = verticalLineGroups.First();
                    double oldX = group[0].X1;
                    double newX = (minX + maxX) / 2.0;

                    SetVerticalGroupPosition(group, newX);
                    MoveAttachedPoints(lines, oldX, newX, isVertical: true);

                    Console.WriteLine($"   📏 Pion: {oldX:F3} → {newX:F3} (środek)");
                }
                else
                {
                    // Wiele linii pionowych → równomierne rozłożenie
                    double spacing = (maxX - minX) / (verticalLineGroups.Count + 1);
                    int i = 1;

                    foreach (var lineGroup in verticalLineGroups)
                    {
                        double x = minX + i * spacing;
                        if (Math.Abs(x) < Tolerance) x = MinOffsetFromAxis;

                        double oldX = lineGroup[0].X1;
                        SetVerticalGroupPosition(lineGroup, x);
                        MoveAttachedPoints(lines, oldX, x, isVertical: true);

                        Console.WriteLine($"   📏 Pion [{i}]: {oldX:F3} → {x:F3}");
                        i++;
                    }
                }
            }

            // ============================================================
            // POZIOME
            // ============================================================
            if (horizontalLineGroups.Any())
            {
                if (horizontalLineGroups.Count == 1)
                {
                    // Jedna linia pozioma → środek Y
                    var group = horizontalLineGroups.First();
                    double oldY = group[0].Y1;
                    double newY = (minY + maxY) / 2.0;

                    SetHorizontalGroupPosition(group, newY);
                    MoveAttachedPoints(lines, oldY, newY, isVertical: false);

                    Console.WriteLine($"   📏 Poziom: {oldY:F3} → {newY:F3} (środek)");
                }
                else
                {
                    // Wiele linii poziomych → równomierne rozłożenie
                    double spacing = (maxY - minY) / (horizontalLineGroups.Count + 1);
                    int i = 1;

                    foreach (var lineGroup in horizontalLineGroups)
                    {
                        double y = minY + i * spacing;
                        double oldY = lineGroup[0].Y1;
                        SetHorizontalGroupPosition(lineGroup, y);
                        MoveAttachedPoints(lines, oldY, y, isVertical: false);

                        Console.WriteLine($"   📏 Poziom [{i}]: {oldY:F3} → {y:F3}");
                        i++;
                    }
                }
            }

            await Task.CompletedTask;
        }
        private static void MoveAttachedPoints(
            List<XLineShape> allLines,
            double oldCoord,
            double newCoord,
            bool isVertical,
            double tolerance = MoveTolerance)
        {
            if (Math.Abs(oldCoord - newCoord) < 0.001) return;

            foreach (var line in allLines)
            {
                if (isVertical)
                {
                    if (Math.Abs(line.X1 - oldCoord) <= tolerance) line.X1 = newCoord;
                    if (Math.Abs(line.X2 - oldCoord) <= tolerance) line.X2 = newCoord;
                }
                else
                {
                    if (Math.Abs(line.Y1 - oldCoord) <= tolerance) line.Y1 = newCoord;
                    if (Math.Abs(line.Y2 - oldCoord) <= tolerance) line.Y2 = newCoord;
                }
            }
        }

        private static List<List<XLineShape>> GroupLinesForDistribution(
            IEnumerable<XLineShape> lines,
            Func<XLineShape, double> coordinate)
        {
            return GroupRelatedLines(lines)
                .OrderBy(group => group.Average(coordinate))
                .ToList();
        }

        private static List<List<XLineShape>> GroupRelatedLines(IEnumerable<XLineShape> lines)
        {
            var grupy = lines
                .GroupBy(line => line.SplitGroupId ?? line.ID)
                .Select(group => group.ToList())
                .ToList();

            Console.WriteLine($"🔍 GroupRelatedLines: {lines.Count()} linii → {grupy.Count} grup");
            foreach (var g in grupy)
            {
                Console.WriteLine($"   Grupa {(g.First().SplitGroupId ?? g.First().ID)?.Substring(0, 8) ?? "null"}: {g.Count} linii");
                foreach (var l in g)
                {
                    Console.WriteLine($"      Line {l.ID?.Substring(0, 8)}: " +
                                      $"({l.X1:F1},{l.Y1:F1}) → ({l.X2:F1},{l.Y2:F1})");
                }
            }

            return grupy;
        }

        // =====================================================================
        // ENVELOPE DLA GRUP LINII
        // =====================================================================

        /// <summary>
        /// ⭐ Poprawka: envelope dostaje unikalne ID i czyści SplitGroupId.
        /// </summary>
        private static XLineShape CreateGroupEnvelope(IReadOnlyCollection<XLineShape> lineGroup)
        {
            var (start, end) = GetGroupOuterEndpoints(lineGroup);
            var envelope = (XLineShape)lineGroup.First().Clone();

            SetLineEndpoints(envelope, start.X, start.Y, end.X, end.Y);

            // ⭐ Envelope to twór tymczasowy – unikalne ID, brak grupowania
            envelope.ID = Guid.NewGuid().ToString();
            envelope.SplitGroupId = null;

            return envelope;
        }


        private static void SetGroupOuterEndpoints(
        IReadOnlyCollection<XLineShape> lineGroup,
        double startX, double startY,
        double endX, double endY)
        {
            var (start, end) = GetGroupOuterEndpoints(lineGroup);
            SetEndpoint(start, startX, startY);
            SetEndpoint(end, endX, endY);

            // ⭐ Walidacja: po ustawieniu endpoints sprawdź, czy grupa nadal jest koliniowa
            if (!CzyGrupaJestKoliniowa(lineGroup))
            {
                Console.WriteLine($"⚠️ SetGroupOuterEndpoints: grupa NIEKOLINIOWA po ustawieniu! " +
                                  $"SplitGroupId = {lineGroup.First().SplitGroupId}");

                foreach (var line in lineGroup)
                {
                    Console.WriteLine($"   Line {line.ID?.Substring(0, 8)}: " +
                                      $"({line.X1:F1},{line.Y1:F1}) → ({line.X2:F1},{line.Y2:F1})");
                }
            }
        }

        /// <summary>
        /// ⭐ Poprawka: wymusza dodatni kierunek referencyjny.
        /// Zapobiega odwróceniu start/end przy ujemnym dx/dy.
        /// </summary>
        private static (LineEndpoint Start, LineEndpoint End) GetGroupOuterEndpoints(
            IReadOnlyCollection<XLineShape> lineGroup)
        {
            var reference = lineGroup
                .OrderByDescending(line => Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2))
                .First();

            double deltaX = reference.X2 - reference.X1;
            double deltaY = reference.Y2 - reference.Y1;

            // ⭐ Wymuś dodatni kierunek
            if (deltaX < 0 || (Math.Abs(deltaX) < Tolerance && deltaY < 0))
            {
                deltaX = -deltaX;
                deltaY = -deltaY;
            }

            var endpoints = lineGroup.SelectMany(line => new[]
            {
                new LineEndpoint(line, true, line.X1, line.Y1),
                new LineEndpoint(line, false, line.X2, line.Y2)
            }).ToList();

            double Project(LineEndpoint endpoint) =>
                (endpoint.X - reference.X1) * deltaX + (endpoint.Y - reference.Y1) * deltaY;

            return (endpoints.OrderBy(Project).First(),
                    endpoints.OrderByDescending(Project).First());
        }

        private static void SetEndpoint(LineEndpoint endpoint, double x, double y)
        {
            if (endpoint.IsStart)
            {
                endpoint.Line.X1 = x;
                endpoint.Line.Y1 = y;
            }
            else
            {
                endpoint.Line.X2 = x;
                endpoint.Line.Y2 = y;
            }
        }

        private static void SetLineEndpoints(XLineShape line, double x1, double y1, double x2, double y2)
        {
            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;
        }

        private readonly record struct LineEndpoint(XLineShape Line, bool IsStart, double X, double Y);

        private static void SetVerticalGroupPosition(IEnumerable<XLineShape> lineGroup, double x)
        {
            foreach (var line in lineGroup)
                line.X1 = line.X2 = x;
        }

        private static void SetHorizontalGroupPosition(IEnumerable<XLineShape> lineGroup, double y)
        {
            foreach (var line in lineGroup)
                line.Y1 = line.Y2 = y;
        }

        // =====================================================================
        // PODSTAWOWE METODY GEOMETRYCZNE
        // =====================================================================

        public static void CheckEdgeIntersection(
            double x1, double y1, double x2, double y2,
            XLineShape line, List<XPoint> intersections)
        {
            if (FindIntersection(x1, y1, x2, y2, line.X1, line.Y1, line.X2, line.Y2,
                                 out double ix, out double iy))
            {
                intersections.Add(new XPoint(ix, iy));
            }
        }

        public static bool FindIntersection(
            double aX1, double aY1, double aX2, double aY2,
            double bX1, double bY1, double bX2, double bY2,
            out double x, out double y)
        {
            x = 0; y = 0;
            double d = (aX1 - aX2) * (bY1 - bY2) - (aY1 - aY2) * (bX1 - bX2);
            if (Math.Abs(d) < Tolerance) return false;

            double t = ((aX1 - bX1) * (bY1 - bY2) - (aY1 - bY1) * (bX1 - bX2)) / d;
            double u = -((aX1 - aX2) * (aY1 - bY1) - (aY1 - aY2) * (aX1 - bX1)) / d;

            if (t < 0 || t > 1 || u < 0 || u > 1) return false;

            x = aX1 + t * (aX2 - aX1);
            y = aY1 + t * (aY2 - aY1);
            return true;
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        public static bool LineIntersectsBoundingBox(XLineShape line, BoundingBox bbox)
        {
            return !(line.X2 < bbox.X || line.X1 > bbox.X + bbox.Width ||
                     line.Y2 < bbox.Y || line.Y1 > bbox.Y + bbox.Height);
        }

        // =====================================================================
        // PRZYCINANIE DO OKRĘGU / POLYGONU / KRAWĘDZI
        // =====================================================================

        public static async Task ShortenLineInsideCircle(XLineShape line, XCircleShape circle)
        {
            var intersections = new List<XPoint>();
            FindCircleIntersections(circle, line, ref intersections);

            if (intersections.Count == 2)
            {
                line.X1 = intersections[0].X;
                line.Y1 = intersections[0].Y;
                line.X2 = intersections[1].X;
                line.Y2 = intersections[1].Y;
            }
            else if (intersections.Count == 1)
            {
                if (IsPointInsideCircle(line.X1, line.Y1, circle))
                {
                    line.X2 = intersections[0].X;
                    line.Y2 = intersections[0].Y;
                }
                else
                {
                    line.X1 = intersections[0].X;
                    line.Y1 = intersections[0].Y;
                }
            }

            await Task.CompletedTask;
        }

        public static async Task ShortenLineInsidePolygon(XLineShape line, List<XPoint> polygonVertices)
        {
            var intersections = new List<XPoint>();

            int count = polygonVertices.Count;
            for (int i = 0; i < count; i++)
            {
                XPoint p1 = polygonVertices[i];
                XPoint p2 = polygonVertices[(i + 1) % count];

                if (FindIntersection(p1.X, p1.Y, p2.X, p2.Y,
                                     line.X1, line.Y1, line.X2, line.Y2,
                                     out double ix, out double iy))
                {
                    intersections.Add(new XPoint(ix, iy));
                }
            }

            intersections = intersections.Distinct()
                .OrderBy(p => Distance(line.X1, line.Y1, p.X, p.Y))
                .ToList();

            if (intersections.Count == 2)
            {
                line.X1 = intersections[0].X;
                line.Y1 = intersections[0].Y;
                line.X2 = intersections[1].X;
                line.Y2 = intersections[1].Y;
            }
            else if (intersections.Count == 1)
            {
                if (IsPointInsidePolygon(line.X1, line.Y1, polygonVertices))
                {
                    line.X2 = intersections[0].X;
                    line.Y2 = intersections[0].Y;
                }
                else
                {
                    line.X1 = intersections[0].X;
                    line.Y1 = intersections[0].Y;
                }
            }

            await Task.CompletedTask;
        }

        private static bool IsPointInsidePolygon(double x, double y, List<XPoint> polygonVertices)
        {
            int count = polygonVertices.Count;
            bool inside = false;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                double xi = polygonVertices[i].X, yi = polygonVertices[i].Y;
                double xj = polygonVertices[j].X, yj = polygonVertices[j].Y;

                bool intersect = ((yi > y) != (yj > y)) &&
                                 (x < (xj - xi) * (y - yi) / (yj - yi) + xi);
                if (intersect) inside = !inside;
            }

            return inside;
        }

        public static async Task ShortenLineToShape(XLineShape line, List<(XPoint Start, XPoint End)> edges)
        {
            var intersections = new List<XPoint>();

            foreach (var (start, end) in edges)
            {
                if (FindIntersection(start.X, start.Y, end.X, end.Y,
                                     line.X1, line.Y1, line.X2, line.Y2,
                                     out double ix, out double iy))
                {
                    intersections.Add(new XPoint(ix, iy));
                }
            }

            intersections = intersections
                .Distinct()
                .OrderBy(p => Distance(line.X1, line.Y1, p.X, p.Y))
                .ToList();

            if (intersections.Count >= 2)
            {
                line.X1 = intersections[0].X;
                line.Y1 = intersections[0].Y;
                line.X2 = intersections[^1].X;
                line.Y2 = intersections[^1].Y;
            }
            else if (intersections.Count == 1)
            {
                var vertices = edges
                    .SelectMany(e => new[] { e.Start, e.End })
                    .Distinct()
                    .ToList();

                if (IsPointInsidePolygon(line.X1, line.Y1, vertices))
                {
                    line.X2 = intersections[0].X;
                    line.Y2 = intersections[0].Y;
                }
                else
                {
                    line.X1 = intersections[0].X;
                    line.Y1 = intersections[0].Y;
                }
            }

            await Task.CompletedTask;
        }

        public static async Task ShortenLineInsideShape(XLineShape line, BoundingBox shapeBox)
        {
            var intersections = new List<XPoint>();

            CheckEdgeIntersection(shapeBox.X, shapeBox.Y, shapeBox.X + shapeBox.Width, shapeBox.Y, line, intersections);
            CheckEdgeIntersection(shapeBox.X + shapeBox.Width, shapeBox.Y, shapeBox.X + shapeBox.Width, shapeBox.Y + shapeBox.Height, line, intersections);
            CheckEdgeIntersection(shapeBox.X, shapeBox.Y + shapeBox.Height, shapeBox.X + shapeBox.Width, shapeBox.Y + shapeBox.Height, line, intersections);
            CheckEdgeIntersection(shapeBox.X, shapeBox.Y, shapeBox.X, shapeBox.Y + shapeBox.Height, line, intersections);

            intersections = intersections.Distinct()
                .OrderBy(p => Distance(line.X1, line.Y1, p.X, p.Y))
                .ToList();

            if (intersections.Count == 2)
            {
                line.X1 = intersections[0].X;
                line.Y1 = intersections[0].Y;
                line.X2 = intersections[1].X;
                line.Y2 = intersections[1].Y;
            }
            else if (intersections.Count == 1)
            {
                if (shapeBox.Contains(line.X1, line.Y1))
                {
                    line.X2 = intersections[0].X;
                    line.Y2 = intersections[0].Y;
                }
                else
                {
                    line.X1 = intersections[0].X;
                    line.Y1 = intersections[0].Y;
                }
            }

            await Task.CompletedTask;
        }

        public static async Task ShortenLineInsideEdges(
            XLineShape line,
            List<(XPoint Start, XPoint End)> edges,
            XPoint arcCenter, double radius)
        {
            var intersections = new List<XPoint>();

            foreach (var edge in edges)
            {
                if (FindIntersection(edge.Start.X, edge.Start.Y, edge.End.X, edge.End.Y,
                                     line.X1, line.Y1, line.X2, line.Y2,
                                     out double ix, out double iy))
                {
                    if (IsPointOnSegment(edge.Start, edge.End, new XPoint(ix, iy)))
                        intersections.Add(new XPoint(ix, iy));
                }

                await Task.CompletedTask;
            }

            var arcIntersections = FindCircleLineIntersections(arcCenter, radius, line);
            intersections.AddRange(arcIntersections);

            intersections = intersections.Distinct()
                .OrderBy(p => Distance(line.X1, line.Y1, p.X, p.Y))
                .ToList();

            if (intersections.Count == 2)
            {
                line.X1 = intersections[0].X;
                line.Y1 = intersections[0].Y;
                line.X2 = intersections[1].X;
                line.Y2 = intersections[1].Y;
            }
            else if (intersections.Count == 1)
            {
                if (IsPointInsideEdges(line.X1, line.Y1, edges, arcCenter, radius))
                {
                    line.X2 = intersections[0].X;
                    line.Y2 = intersections[0].Y;
                }
                else
                {
                    line.X1 = intersections[0].X;
                    line.Y1 = intersections[0].Y;
                }
            }
        }

        private static bool IsPointInsideEdges(
            double x, double y,
            List<(XPoint Start, XPoint End)> edges,
            XPoint? arcCenter = null, double arcRadius = 0)
        {
            int intersections = 0;

            foreach (var edge in edges)
            {
                double x1 = edge.Start.X, y1 = edge.Start.Y;
                double x2 = edge.End.X, y2 = edge.End.Y;

                if ((y1 > y) != (y2 > y))
                {
                    double intersectX = x1 + (y - y1) * (x2 - x1) / (y2 - y1);
                    if (intersectX > x) intersections++;
                }
            }

            if (arcCenter.HasValue)
            {
                double dx = x - arcCenter.Value.X;
                double dy = y - arcCenter.Value.Y;
                if (dx * dx + dy * dy <= arcRadius * arcRadius) return true;
            }

            return (intersections % 2) == 1;
        }

        private static bool IsPointOnSegment(XPoint start, XPoint end, XPoint point)
        {
            double crossProduct = (point.Y - start.Y) * (end.X - start.X) - (point.X - start.X) * (end.Y - start.Y);
            if (Math.Abs(crossProduct) > 0.001) return false;

            double dotProduct = (point.X - start.X) * (end.X - start.X) + (point.Y - start.Y) * (end.Y - start.Y);
            if (dotProduct < 0) return false;

            double squaredLength = (end.X - start.X) * (end.X - start.X) + (end.Y - start.Y) * (end.Y - start.Y);
            if (dotProduct > squaredLength) return false;

            return true;
        }

        private static List<XPoint> FindCircleLineIntersections(XPoint center, double radius, XLineShape line)
        {
            var intersections = new List<XPoint>();

            double dx = line.X2 - line.X1;
            double dy = line.Y2 - line.Y1;
            double fx = line.X1 - center.X;
            double fy = line.Y1 - center.Y;

            double a = dx * dx + dy * dy;
            double b = 2 * (fx * dx + fy * dy);
            double c = (fx * fx + fy * fy) - (radius * radius);

            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0) return intersections;

            discriminant = Math.Sqrt(discriminant);
            double t1 = (-b - discriminant) / (2 * a);
            double t2 = (-b + discriminant) / (2 * a);

            if (t1 >= 0 && t1 <= 1)
            {
                double ix = line.X1 + t1 * dx;
                double iy = line.Y1 + t1 * dy;
                if (iy <= center.Y) intersections.Add(new XPoint(ix, iy));
            }

            if (t2 >= 0 && t2 <= 1)
            {
                double ix = line.X1 + t2 * dx;
                double iy = line.Y1 + t2 * dy;
                if (iy <= center.Y) intersections.Add(new XPoint(ix, iy));
            }

            return intersections;
        }

        private static void FindCircleIntersections(XCircleShape circle, XLineShape line, ref List<XPoint> intersections)
        {
            double dx = line.X2 - line.X1;
            double dy = line.Y2 - line.Y1;
            double fx = line.X1 - circle.X;
            double fy = line.Y1 - circle.Y;

            double a = dx * dx + dy * dy;
            double b = 2 * (fx * dx + fy * dy);
            double c = (fx * fx + fy * fy) - (circle.Radius * circle.Radius);

            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0) return;

            discriminant = Math.Sqrt(discriminant);
            double t1 = (-b - discriminant) / (2 * a);
            double t2 = (-b + discriminant) / (2 * a);

            if (t1 >= 0 && t1 <= 1)
                intersections.Add(new XPoint(line.X1 + t1 * dx, line.Y1 + t1 * dy));

            if (t2 >= 0 && t2 <= 1)
                intersections.Add(new XPoint(line.X1 + t2 * dx, line.Y1 + t2 * dy));
        }

        private static bool IsPointInsideCircle(double x, double y, XCircleShape circle)
        {
            double dx = x - circle.X;
            double dy = y - circle.Y;
            return (dx * dx + dy * dy) <= (circle.Radius * circle.Radius);
        }

        // =====================================================================
        // NORMALIZACJA POZYCJI
        // =====================================================================

        public static async Task ShiftAllShapesToPositiveQuadrant(List<IShapeDC> shapes)
        {
            if (shapes == null || !shapes.Any()) return;

            double minX = shapes.Min(s => s.Points.Min(p => p.X));
            double minY = shapes.Min(s => s.Points.Min(p => p.Y));

            double shiftX = minX < 0 ? -minX : 0;
            double shiftY = minY < 0 ? -minY : 0;

            foreach (var shape in shapes)
                shape.Move(shiftX, shiftY);

            await Task.CompletedTask;
        }

        public static async Task UstawPozycjeXiYnaZero(List<IShapeDC> shapes)
        {
            if (shapes == null || shapes.Count == 0) return;

            double minX = double.MaxValue;
            double minY = double.MaxValue;

            foreach (var shape in shapes)
            {
                var bbox = shape.GetBoundingBox();
                if (bbox.Left < minX) minX = bbox.Left;
                if (bbox.Top < minY) minY = bbox.Top;
            }

            double offsetX = -minX;
            double offsetY = -minY;

            foreach (var shape in shapes)
                shape.Move(offsetX, offsetY);

            await Task.CompletedTask;
        }

        // =====================================================================
        // SKALOWANIE ZBIORU KSZTAŁTÓW
        // =====================================================================

        /// <summary>
        /// Skaluje WSZYSTKIE kształty proporcjonalnie (jedna skala).
        /// ⭐ Poprawka: pomija XLineShape przy nadpisywaniu Szerokosc/Wysokosc,
        /// filtruje zerowe bbox.
        /// </summary>
        public static void SkalujShapesDoWymiarowZachowujacProporcje(
            List<IShapeDC> shapes,
            double docelowaSzerokosc,
            double docelowaWysokosc)
        {
            if (shapes == null || shapes.Count == 0) return;

            var (minX, minY, maxX, maxY) = ObliczWspolnyBBox(shapes);

            double aktualnaSzerokosc = maxX - minX;
            double aktualnaWysokosc = maxY - minY;

            if (aktualnaSzerokosc < 0.001 || aktualnaWysokosc < 0.001)
            {
                Console.WriteLine("⚠️ SkalujShapesDoWymiarowZachowujacProporcje: zerowy bbox, pomijam");
                return;
            }

            double skala = Math.Min(
                docelowaSzerokosc / aktualnaSzerokosc,
                docelowaWysokosc / aktualnaWysokosc);

            double nowaSzerokosc = aktualnaSzerokosc * skala;
            double nowaWysokosc = aktualnaWysokosc * skala;
            double offsetX = (docelowaSzerokosc - nowaSzerokosc) / 2.0 - minX * skala;
            double offsetY = (docelowaWysokosc - nowaWysokosc) / 2.0 - minY * skala;

            Console.WriteLine($"📐 SkalujShapes (proporcje): {aktualnaSzerokosc:F1}x{aktualnaWysokosc:F1} → " +
                              $"{nowaSzerokosc:F1}x{nowaWysokosc:F1} " +
                              $"(skala={skala:F4}, offset=({offsetX:F1},{offsetY:F1}))");

            foreach (var shape in shapes)
            {
                shape.Transform(skala, skala, offsetX, offsetY);

                // ⭐ Pomiń XLineShape – Transform już ustawił Szerokosc/Wysokosc
                if (shape is not XLineShape)
                {
                    shape.Szerokosc = docelowaSzerokosc;
                    shape.Wysokosc = docelowaWysokosc;
                }
            }
        }

        /// <summary>
        /// Skaluje WSZYSTKIE kształty z osobnymi skalami X i Y (rozciąga).
        /// ⭐ Poprawka: pomija XLineShape, filtruje zerowe bbox.
        /// </summary>
        public static void SkalujShapesDoWymiarowRozciagajac(
            List<IShapeDC> shapes,
            double docelowaSzerokosc,
            double docelowaWysokosc)
        {
            if (shapes == null || shapes.Count == 0) return;

            // Wymiar docelowy opisuje obrys okna, a nie obwiednię linii
            // podziału. Linie mogą chwilowo wystawać poza kontur (przed
            // przycięciem), więc nie mogą wpływać na współczynniki skali.
            var shapesKonturu = shapes.Where(shape => shape is not XLineShape).ToList();
            var shapesDoObwiedni = shapesKonturu.Count > 0 ? shapesKonturu : shapes;
            var (minX, minY, maxX, maxY) = ObliczWspolnyBBox(shapesDoObwiedni);

            double aktualnaSzerokosc = maxX - minX;
            double aktualnaWysokosc = maxY - minY;

            if (aktualnaSzerokosc < 0.001 || aktualnaWysokosc < 0.001)
            {
                Console.WriteLine("⚠️ SkalujShapesDoWymiarowRozciagajac: zerowy bbox, pomijam");
                return;
            }

            double scaleX = docelowaSzerokosc / aktualnaSzerokosc;
            double scaleY = docelowaWysokosc / aktualnaWysokosc;
            double offsetX = -minX * scaleX;
            double offsetY = -minY * scaleY;

            Console.WriteLine($"📐 SkalujShapes (rozciąganie): {aktualnaSzerokosc:F1}x{aktualnaWysokosc:F1} → " +
                              $"{docelowaSzerokosc:F1}x{docelowaWysokosc:F1} " +
                              $"(scaleX={scaleX:F4}, scaleY={scaleY:F4})");

            // Zapamiętaj kierunek linii osiowych przed skalowaniem. Osobne
            // skale X/Y zachowują go matematycznie, jednak błąd numeryczny
            // powiększony np. z 1250 mm do 3250 mm później powodował, że
            // przycinanie uznawało linię pionową za poziomą albo skośną.
            var kierunkiOsiowe = ZapamietajKierunkiOsiowe(shapes.OfType<XLineShape>());

            foreach (var shape in shapes)
            {
                if (shape is XLineShape line)
                {
                    Console.WriteLine(
                        $"🔵 PRZED Transform LINE {line.ID.Substring(0, Math.Min(8, line.ID.Length))}: " +
                        $"({line.X1:F2},{line.Y1:F2}) → ({line.X2:F2},{line.Y2:F2})");
                }

                shape.Transform(scaleX, scaleY, offsetX, offsetY);

                if (shape is not XLineShape)
                {
                    shape.Szerokosc = docelowaSzerokosc;
                    shape.Wysokosc = docelowaWysokosc;
                }

                if (shape is XLineShape linePo)
                {
                    Console.WriteLine(
                        $"🔴 PO Transform LINE {linePo.ID.Substring(0, Math.Min(8, linePo.ID.Length))}: " +
                        $"({linePo.X1:F2},{linePo.Y1:F2}) → ({linePo.X2:F2},{linePo.Y2:F2})");
                }
            }

            PrzywrocKierunkiOsiowe(kierunkiOsiowe);
        }

        private static Dictionary<XLineShape, bool> ZapamietajKierunkiOsiowe(
            IEnumerable<XLineShape> lines)
        {
            var result = new Dictionary<XLineShape, bool>();

            foreach (var line in lines)
            {
                double dx = Math.Abs(line.X2 - line.X1);
                double dy = Math.Abs(line.Y2 - line.Y1);

                // Pion/poziom wymuszany przez narzędzie rysowania albo przez
                // aktualną geometrię musi pozostać takim samym po zmianie
                // wymiarów. Linie rzeczywiście skośne pozostają bez zmian.
                bool jestOsiowa = line.RuchomySlupek || line.PionPoziom ||
                                  dx < Tolerance || dy < Tolerance;
                if (jestOsiowa)
                    result[line] = line.RuchomySlupek || dx <= dy;
            }

            return result;
        }

        private static void PrzywrocKierunkiOsiowe(
            IReadOnlyDictionary<XLineShape, bool> kierunkiOsiowe)
        {
            foreach (var (line, pionowa) in kierunkiOsiowe)
            {
                if (pionowa)
                {
                    line.UpdatePoints(new List<XPoint>
                    {
                        new(line.X1, line.Y1),
                        new(line.X1, line.Y2)
                    });
                }
                else
                {
                    line.UpdatePoints(new List<XPoint>
                    {
                        new(line.X1, line.Y1),
                        new(line.X2, line.Y1)
                    });
                }
            }
        }

        /// <summary>
        /// Skaluje kształty osiami – zmiana szerokości nie zmienia wysokości.
        /// ⭐ Poprawka: pomija XLineShape.
        /// </summary>
        public static void SkalujShapesOsiami(
            List<IShapeDC> shapes,
            double staraSzerokosc,
            double staraWysokosc,
            double nowaSzerokosc,
            double nowaWysokosc)
        {
            if (shapes == null || shapes.Count == 0) return;
            if (staraSzerokosc < 0.001 || staraWysokosc < 0.001)
            {
                Console.WriteLine("⚠️ SkalujShapesOsiami: zerowa stara szerokość/wysokość, pomijam");
                return;
            }

            double scaleX = nowaSzerokosc / staraSzerokosc;
            double scaleY = nowaWysokosc / staraWysokosc;

            Console.WriteLine($"📐 SkalujShapesOsiami: scaleX={scaleX:F4}, scaleY={scaleY:F4}");

            foreach (var shape in shapes)
            {
                shape.Transform(scaleX, scaleY, 0, 0);

                if (shape is not XLineShape)
                {
                    shape.Szerokosc = nowaSzerokosc;
                    shape.Wysokosc = nowaWysokosc;
                }
            }
        }

        /// <summary>
        /// ⭐ Poprawka: filtruje zerowe bbox, żeby nie psuć wspólnego bboxa.
        /// </summary>
        private static (double MinX, double MinY, double MaxX, double MaxY) ObliczWspolnyBBox(
            List<IShapeDC> shapes)
        {
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;

            foreach (var shape in shapes)
            {
                var bb = shape.GetBoundingBox();

                // ⭐ Pomiń zerowe bbox (nie zniekształcają wspólnego bboxa)
                if (bb.Width < 0.001 && bb.Height < 0.001)
                    continue;

                if (bb.Left < minX) minX = bb.Left;
                if (bb.Top < minY) minY = bb.Top;
                if (bb.Left + bb.Width > maxX) maxX = bb.Left + bb.Width;
                if (bb.Top + bb.Height > maxY) maxY = bb.Top + bb.Height;
            }

            // Jeśli nie znaleziono żadnego sensownego kształtu – zwróć 0,0,0,0
            if (minX == double.MaxValue || minY == double.MaxValue)
                return (0, 0, 0, 0);

            return (minX, minY, maxX, maxY);
        }
    }
}
