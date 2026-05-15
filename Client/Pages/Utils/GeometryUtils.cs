using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Client.Pages.Models;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Utils
{
    public static class GeometryUtils
    {

        public static async Task<List<ShapeRegion>> GenerujRegionyZPodzialu(List<IShapeDC> shapes, int _szerokosc, int _wysokosc, bool rama)
        {
            // Console.WriteLine($"📦 Przed usunięciem duplikatów: {shapes.Count} obiektów.");
            shapes = UsunDuplikatyShape(shapes);
            //Console.WriteLine($"📦 Po usunięciu duplikatów: {shapes.Count} obiektów.");

            var regions = new List<ShapeRegion>();

            Console.WriteLine($"🔲 Generowanie regionów z podziału dla {shapes.Count} kształtów. {_szerokosc}x{_wysokosc} typ rama: {rama}");

            var shapesDoRegionow = shapes.Where(s =>
                s is XRectangleShape or XSquareShape or XTriangleShape
                or XTrapezoidShape or XCircleShape or XHouseShape
                or XRoundedTopRectangleShape or XRoundedRectangleShape
                or XRoundedRectangleShapeLeft or XRoundedRectangleShapeRight or XLinePoint)
                .ToList();

            if (!shapesDoRegionow.Any()) return regions;

            double minX = shapesDoRegionow.Min(s => s.GetBoundingBox().X);
            double minY = shapesDoRegionow.Min(s => s.GetBoundingBox().Y);
            double maxX = shapesDoRegionow.Max(s => s.GetBoundingBox().X + s.GetBoundingBox().Width);
            double maxY = shapesDoRegionow.Max(s => s.GetBoundingBox().Y + s.GetBoundingBox().Height);

            double scaleX = (double)_szerokosc / (maxX - minX);
            double scaleY = (double)_wysokosc / (maxY - minY);
            double offsetX = -minX * scaleX;
            double offsetY = -minY * scaleY;

            foreach (var shape in shapes)
            {
                shape.Transform(scaleX, scaleY, offsetX, offsetY);
                shape.Szerokosc = _szerokosc;
                shape.Wysokosc = _wysokosc;
            }

            foreach (var shape in shapes)
            {
                List<XPoint>? pts = shape switch
                {
                    XLineShape lin => new() { new(lin.X1, lin.Y1), new(lin.X2, lin.Y2) },
                    XSquareShape sq => sq.GetCorners(),
                    XRectangleShape rect => rect.GetCorners(),
                    XTriangleShape tri => tri.GetVertices(),
                    XTrapezoidShape trap => trap.GetVertices(),
                    XCircleShape circ => GenerateCircleVertices(circ.X, circ.Y, circ.Radius, circ.IloscElementowLuki),
                    XHouseShape house => house.GetFullOutline(),
                    XRoundedTopRectangleShape rtr => rtr.GetVertices(),
                    XRoundedRectangleShape rr => rr.GetVertices(),
                    XRoundedRectangleShapeLeft rrl => rrl.GetVertices(),
                    XRoundedRectangleShapeRight rrr => rrr.GetVertices(),
                    _ => null
                };

                if (pts == null) continue;

                var typ = shape.GetType().Name.ToLower();

                string typLinii = null;

                string id = shape.ID;

                typLinii = shape switch
                {
                    XLineShape { RuchomySlupek: true } => "Słupek ruchomy",
                    XLineShape { DualRama: true } => "Podwójna rama",
                    XLineShape { StalySlupek: true } => "Słupek stały",
                    _ => "Brak podziału"
                };

                Console.WriteLine($"🔲 Generowanie regionów z podziału dla {shapes.Count} kształtów. {_szerokosc}x{_wysokosc} typLinii: {typLinii}");

                if (pts == null) continue;

                //------------------------------

                string ramaInfo = rama ? "kontur ramowy" : "kontur skrzydłowy";

                var initial = new ShapeRegion
                {
                    Wierzcholki = pts.Select(p => new XPoint((float)Math.Round(p.X, 4), (float)Math.Round(p.Y, 4))).ToList(),

                    Kontur = pts.Select((p, i) =>
                    {
                        var next = pts[(i + 1) % pts.Count];

                        // =========================
                        // OKRĄG → 2 łuki
                        // =========================
                        if (shape is XCircleShape circ)
                        {
                            // środek okręgu
                            var center = new XPoint(circ.X, circ.Y);

                            // Oblicz kąty dla segmentu
                            double startAngle = Math.Atan2(p.Y - center.Y, p.X - center.X);
                            double endAngle = Math.Atan2(next.Y - center.Y, next.X - center.X);

                            // Określ kierunek (zgodnie z ruchem wskazówek zegara)
                            bool counterClockwise = true;

                            var segment = new ContourSegment(
                                p,
                                next,
                                center,
                                circ.Radius,
                                counterClockwise
                            );

                            segment.Informacja = ramaInfo + " " + shape.GetType().Name;
                            return segment;
                        }

                        // =========================
                        // PROSTOKĄT Z ŁUKIEM NA GÓRZE
                        // =========================
                        else if (shape is XRoundedTopRectangleShape rtr)
                        {
                            double arcStartY = rtr.Y + rtr.ArcHeight;
                            var (arcCenterX, arcCenterY, startAngle, endAngle) = rtr.CalculateArcGeometry();
                            var arcCenter = new XPoint(arcCenterX, arcCenterY);

                            // Sprawdź czy punkty są poziome (w przybliżeniu ten sam Y)
                            bool isHorizontalLine = Math.Abs(p.Y - next.Y) < 0.001 && Math.Abs(p.X - next.X) > shape.Szerokosc - 50;

                            // Sprawdź czy punkt jest na łuku (odległość od środka ≈ promień)
                            double d1 = Distance(p, arcCenter);
                            double d2 = Distance(next, arcCenter);

                            // Jeśli to linia pozioma -> na pewno nie łuk
                            bool isArcSegment = !isHorizontalLine &&
                                                Math.Abs(d1 - rtr.Radius) < 2.0 &&
                                                Math.Abs(d2 - rtr.Radius) < 2.0;

                            if (isArcSegment)
                            {
                                // Określ kierunek łuku - dla górnego łuku od prawej do lewej
                                // to jest przeciwnie do ruchu wskazówek zegara
                                bool counterClockwise = true; // Dla górnego łuku

                                // Sprawdź czy to łuk (oba punkty mają Y mniejsze lub równe arcStartY)
                                if (p.Y <= arcStartY && next.Y <= arcStartY)
                                {
                                    var segment = new ContourSegment(
                                        p,
                                        next,
                                        arcCenter,
                                        rtr.Radius,
                                        counterClockwise
                                    );

                                    segment.Informacja = ramaInfo + " " + shape.GetType().Name;
                                    return segment;
                                }
                            }

                            // Dla linii pionowych i poziomych
                            var lineSegment = new ContourSegment(p, next);
                            lineSegment.Informacja = ramaInfo + " " + shape.GetType().Name;
                            return lineSegment;
                        }

                        // =========================
                        // PROSTOKĄT ZAOKRĄGLONY (FINAL STABILNY)
                        // =========================
                        else if (shape is XRoundedRectangleShape rr)
                        {
                            var segments = rr.GetContourSegments();

                            if (segments == null || segments.Count == 0)
                                goto FallbackLine;

                            const double tolerance = 0.4;

                            // =========================
                            // 1. DEDUPLIKACJA GEOMETRYCZNA
                            // =========================
                            segments = segments
                                .GroupBy(s => new
                                {
                                    Ax = Math.Round(s.Start.X, 4),
                                    Ay = Math.Round(s.Start.Y, 4),
                                    Bx = Math.Round(s.End.X, 4),
                                    By = Math.Round(s.End.Y, 4),
                                    s.Type
                                })
                                .Select(g => g.First())
                                .ToList();

                            // =========================
                            // 2. GLOBALNY TRACKING (KLUCZ FIX NA DUPLIKATY)
                            // =========================
                            var used = new HashSet<string>();

                            foreach (var seg in segments)
                            {
                                string key = $"{seg.Type}-" +
                                             $"{Math.Round(seg.Start.X, 3)}:{Math.Round(seg.Start.Y, 3)}-" +
                                             $"{Math.Round(seg.End.X, 3)}:{Math.Round(seg.End.Y, 3)}";

                                if (!used.Add(key))
                                    continue;

                                bool direct =
                                    Distance(seg.Start, p) < tolerance &&
                                    Distance(seg.End, next) < tolerance;

                                bool reverse =
                                    Distance(seg.Start, next) < tolerance &&
                                    Distance(seg.End, p) < tolerance;

                                if (!direct && !reverse)
                                    continue;

                                bool reversed = reverse;

                                Console.WriteLine(
                                    $"🔍 XRoundedRectangleShape match: " +
                                    $"Start({seg.Start.X},{seg.Start.Y}) End({seg.End.X},{seg.End.Y}) Reversed={reversed}");

                                ContourSegment result;

                                // =========================
                                // ŁUK (POPRAWIONA LOGIKA – KLUCZ FIX)
                                // =========================
                                if (seg.Type == SegmentType.Arc && seg.Center.HasValue)
                                {
                                    var a = reversed ? next : p;
                                    var b = reversed ? p : next;

                                    var center = seg.Center.Value;

                                    // 🔥 wyznacz rzeczywisty kierunek łuku
                                    double a1 = Math.Atan2(a.Y - center.Y, a.X - center.X);
                                    double a2 = Math.Atan2(b.Y - center.Y, b.X - center.X);

                                    double sweep = a2 - a1;

                                    bool clockwise = sweep < 0;

                                    result = new ContourSegment(a, b)
                                    {
                                        Type = SegmentType.Arc,
                                        Center = center,
                                        Radius = seg.Radius,

                                        // 🔥 KLUCZOWA POPRAWKA:
                                        // NIE ufamy staremu flagowaniu, tylko rekalkulacja
                                        CounterClockwise = !clockwise,

                                        IsArcFragment = true,
                                        Informacja = ramaInfo + " " + shape.GetType().Name,
                                    };
                                }
                                else
                                {
                                    // =========================
                                    // LINIA
                                    // =========================
                                    result = reversed
                                        ? new ContourSegment(next, p)
                                        : new ContourSegment(p, next);
                                }

                                result.Informacja = ramaInfo + " " + shape.GetType().Name;
                                return result;
                            }

                        FallbackLine:

                            var lineSeg = new ContourSegment(p, next)
                            {
                                Informacja = ramaInfo + " " + shape.GetType().Name
                            };

                            return lineSeg;
                        }

                        // =========================
                        // ZAOKRĄGLONY LEWY BOK
                        // =========================
                        else if (shape is XRoundedRectangleShapeLeft rrl)
                        {
                            double r = rrl.Radius;

                            double centerX = rrl.X + r;
                            double centerY = rrl.Y + r;

                            double d1 = Distance(p, new XPoint(centerX, centerY));
                            double d2 = Distance(next, new XPoint(centerX, centerY));

                            bool isLeftArc = Math.Abs(d1 - r) < 2.0 && Math.Abs(d2 - r) < 2.0;

                            if (isLeftArc)
                            {
                                // Dla lewego boku - kierunek zależy od tego, czy idziemy w górę czy w dół
                                bool counterClockwise = p.Y < next.Y;

                                var segment = new ContourSegment(
                                    p,
                                    next,
                                    new XPoint(centerX, centerY),
                                    r,
                                    counterClockwise
                                );

                                segment.Informacja = ramaInfo + " " + shape.GetType().Name;
                                return segment;
                            }

                            var lineSegLeft = new ContourSegment(p, next);
                            lineSegLeft.Informacja = ramaInfo + " " + shape.GetType().Name;
                            return lineSegLeft;
                        }
                        // =========================
                        // ZAOKRĄGLONY PRAWY BOK
                        // =========================
                        else if (shape is XRoundedRectangleShapeRight rrr)
                        {
                            double r = rrr.Radius;

                            double centerX = rrr.X + rrr.Width - r;
                            double centerY = rrr.Y + r;

                            double d1 = Distance(p, new XPoint(centerX, centerY));
                            double d2 = Distance(next, new XPoint(centerX, centerY));

                            bool isRightArc = Math.Abs(d1 - r) < 2.0 && Math.Abs(d2 - r) < 2.0;

                            if (isRightArc)
                            {
                                // Dla prawego boku - kierunek przeciwny niż dla lewego
                                // bool counterClockwise = p.Y > next.Y;

                                bool counterClockwise = true; // ZMIANA: zawsze true

                                var segment = new ContourSegment(
                                    p,
                                    next,
                                    new XPoint(centerX, centerY),
                                    r,
                                    counterClockwise
                                );

                                segment.Informacja = ramaInfo + " " + shape.GetType().Name;
                                return segment;
                            }

                            var lineSegRight = new ContourSegment(p, next);
                            lineSegRight.Informacja = ramaInfo + " " + shape.GetType().Name;
                            return lineSegRight;
                        }

                        // =========================
                        // DOMYŚLNA LINIA
                        // =========================
                        else
                        {
                            var segment = new ContourSegment(p, next);
                            segment.Informacja = ramaInfo + " " + shape.GetType().Name;
                            return segment;
                        }

                    }).ToList(),

                    TypKsztaltu = typ,
                    TypLiniiDzielacej = typLinii,
                    Id = id,
                    IdMaster = id,
                    Rama = rama,
                };

                if (rama)
                {
                    var linieDzielace = shapes
                        .OfType<XLineShape>()
                        .Where(l => l.DualRama)
                        .ToList();

                    var podzielone = PodzielRegionRekurencyjnie(initial, linieDzielace, id, rama);

                    Console.WriteLine($"🔲 Generowanie regionów PodzielRegionRekurencyjnie podzielone.Count: {podzielone.Count} id:{id}");

                    int idCounter = 0;

                    foreach (var r in podzielone)
                    {
                        // Usuwanie duplikatów wierzchołków
                        r.Wierzcholki = r.Wierzcholki
                            .GroupBy(p => new { X = Math.Round(p.X, 2), Y = Math.Round(p.Y, 2) })
                            .Select(g => g.First())
                            .ToList();

                        // USUŃ SEGMENTY O ZEROWEJ DŁUGOŚCI Z KONTURU
                        // 1️⃣ usuń śmieci (zerowe segmenty)
                        r.Kontur = r.Kontur
                            .Where(s => !CzySegmentZerowejDlugosci(s))
                            .ToList();

                        // 2️⃣ Usuwanie duplikatów (oba kierunki linii)
                        r.Kontur = RemoveDuplicateSegments(r.Kontur);

                        // 2️⃣ grupowanie (BEZ NaN + normalizacja kierunku)
                        var unikalneSegmenty = r.Kontur
                            .GroupBy(s =>
                            {
                                var startX = Math.Round(s.Start.X, 2);
                                var startY = Math.Round(s.Start.Y, 2);
                                var endX = Math.Round(s.End.X, 2);
                                var endY = Math.Round(s.End.Y, 2);

                                // 🔥 NORMALIZACJA KIERUNKU (A→B == B→A)
                                // Oba kierunki mają ten sam klucz
                                bool reversed = false;
                                if (startX > endX || (startX == endX && startY > endY))
                                {
                                    (startX, endX) = (endX, startX);
                                    (startY, endY) = (endY, startY);
                                    reversed = true;
                                }

                                return new
                                {
                                    Type = s.Type,
                                    StartX = startX,
                                    StartY = startY,
                                    EndX = endX,
                                    EndY = endY,

                                    // 🔥 zamiast NaN → 0
                                    CenterX = s.Type == SegmentType.Arc && s.Center.HasValue ? Math.Round(s.Center.Value.X, 2) : 0,
                                    CenterY = s.Type == SegmentType.Arc && s.Center.HasValue ? Math.Round(s.Center.Value.Y, 2) : 0,
                                    Radius = s.Type == SegmentType.Arc ? Math.Round(s.Radius, 2) : 0,
                                    CounterClockwise = s.Type == SegmentType.Arc && s.CounterClockwise
                                };
                            })
                            .Select(g =>
                            {
                                var seg = g.First();
                                // Jeśli segment był odwrócony w kluczu, zwróć go w znormalizowanej formie
                                if (seg.Start.X > seg.End.X || (seg.Start.X == seg.End.X && seg.Start.Y > seg.End.Y))
                                {
                                    if (seg.Type == SegmentType.Arc && seg.Center.HasValue)
                                    {
                                        return new ContourSegment(
                                            seg.End,
                                            seg.Start,
                                            seg.Center,
                                            seg.Radius,
                                            !seg.CounterClockwise
                                        )
                                        {
                                            Informacja = seg.Informacja
                                        };
                                    }
                                    else
                                    {
                                        return new ContourSegment(seg.End, seg.Start)
                                        {
                                            Informacja = seg.Informacja
                                        };
                                    }
                                }
                                return seg;
                            })
                            .ToList();

                        // 3️⃣ odbudowa segmentów
                        var nowyKontur = new List<ContourSegment>();

                        foreach (var segment in unikalneSegmenty)
                        {
                            ContourSegment nowySegment;

                            if (segment.Type == SegmentType.Arc && segment.Center.HasValue)
                            {
                                nowySegment = new ContourSegment(
                                    new XPoint(segment.Start.X, segment.Start.Y),
                                    new XPoint(segment.End.X, segment.End.Y),
                                    new XPoint(segment.Center.Value.X, segment.Center.Value.Y),
                                    segment.Radius,
                                    segment.CounterClockwise
                                );
                            }
                            else
                            {
                                nowySegment = new ContourSegment(
                                    new XPoint(segment.Start.X, segment.Start.Y),
                                    new XPoint(segment.End.X, segment.End.Y)
                                );
                            }

                            nowySegment.Informacja = ramaInfo;
                            nowyKontur.Add(nowySegment);
                        }

                        // USUŃ SEGMENTY O ZEROWEJ DŁUGOŚCI PONOWNIE (po ewentualnych przekształceniach)
                        r.Kontur = r.Kontur
                            .Where(s => !CzySegmentZerowejDlugosci(s))
                            .ToList();

                        // 2️⃣ Usuwanie duplikatów (oba kierunki linii)
                        r.Kontur = RemoveDuplicateSegments(r.Kontur);

                        // 4️⃣ Sortowanie
                        r.Kontur = OrderSegmentsForClosedContour(r.Kontur);

                        r.RozpoznajTyp(r.TypKsztaltu);

                        Console.WriteLine($"🔹 Region id: {r.Id} po podziale: {r.TypKsztaltu} z {r.Wierzcholki.Count} wierzchołkami. - RAMA");

                        if (r.TypKsztaltu == "xhouseshape" && r.Wierzcholki.Count == 4)
                        {
                            r.TypKsztaltu = "trapez";
                        }

                        if (r.TypKsztaltu == "trapez")
                        {
                            if (r.Wierzcholki.Count == 2)
                            {
                                r.TypKsztaltu = "linia";
                            }
                            else if (r.Wierzcholki.Count == 3)
                            {
                                r.TypKsztaltu = "trójkąt";
                            }
                            else if (r.Wierzcholki.Count == 4 && CzyProstokat(r.Wierzcholki))
                            {
                                r.TypKsztaltu = "prostokąt";
                            }
                        }

                        r.Id = id + "|" + idCounter++;
                    }

                    regions.AddRange(podzielone);
                }
                else
                {
                    // SKRZYDŁO - bez ramy
                    var linieDzielace = shapes
                        .OfType<XLineShape>()
                        .Where(l => l.DualRama || l.StalySlupek || l.RuchomySlupek)
                        .ToList();

                    Console.WriteLine($"🔲 Generowanie regionów bez ramy dla shape id: {initial.Id} id: {id}");

                    List<ShapeRegion> podzielone;

                    // Sprawdź czy są jakieś linie dzielące
                    if (!linieDzielace.Any())
                    {
                        // BRAK LINII DZIELĄCYCH - zachowaj oryginalny kształt
                        Console.WriteLine($"⚠️ Brak linii dzielących - zachowuję oryginalny kontur");
                        podzielone = new List<ShapeRegion> { initial };

                        // Przetwarzanie dla przypadku bez linii dzielących (TAK SAMO JAK DLA RAMY)
                        foreach (var r in podzielone)
                        {
                            // Usuwanie duplikatów wierzchołków
                            r.Wierzcholki = r.Wierzcholki
                                .GroupBy(p => new { X = Math.Round(p.X, 2), Y = Math.Round(p.Y, 2) })
                                .Select(g => g.First())
                                .ToList();

                            // USUŃ SEGMENTY O ZEROWEJ DŁUGOŚCI Z KONTURU
                            r.Kontur = r.Kontur
                                .Where(s => !CzySegmentZerowejDlugosci(s))
                                .ToList();

                            // Przetwarzanie konturu - usuwanie duplikatów
                            var unikalneSegmenty = r.Kontur
                                .GroupBy(s => new
                                {
                                    Type = s.Type,
                                    StartX = Math.Round(s.Start.X, 2),
                                    StartY = Math.Round(s.Start.Y, 2),
                                    EndX = Math.Round(s.End.X, 2),
                                    EndY = Math.Round(s.End.Y, 2),
                                    CenterX = s.Type == SegmentType.Arc && s.Center.HasValue ? Math.Round(s.Center.Value.X, 2) : double.NaN,
                                    CenterY = s.Type == SegmentType.Arc && s.Center.HasValue ? Math.Round(s.Center.Value.Y, 2) : double.NaN,
                                    Radius = s.Type == SegmentType.Arc ? Math.Round(s.Radius, 2) : double.NaN,
                                    CounterClockwise = s.Type == SegmentType.Arc ? s.CounterClockwise : false
                                })
                                .Select(g => g.First())
                                .ToList();

                            var nowyKontur = new List<ContourSegment>();

                            foreach (var segment in unikalneSegmenty)
                            {
                                ContourSegment nowySegment;

                                if (segment.Type == SegmentType.Arc)
                                {
                                    nowySegment = new ContourSegment(
                                        segment.Start,
                                        segment.End,
                                        segment.Center,
                                        segment.Radius,
                                        segment.CounterClockwise
                                    );
                                }
                                else
                                {
                                    nowySegment = new ContourSegment(
                                        segment.Start,
                                        segment.End
                                    );
                                }

                                nowySegment.Informacja = "kontur skrzydłowy";
                                nowyKontur.Add(nowySegment);
                            }

                            r.Kontur = nowyKontur;

                            // 4️⃣ Sortowanie
                            r.Kontur = OrderSegmentsForClosedContour(r.Kontur);

                            // USUŃ SEGMENTY O ZEROWEJ DŁUGOŚCI PONOWNIE
                            r.Kontur = r.Kontur
                                .Where(s => !CzySegmentZerowejDlugosci(s))
                                .ToList();

                            r.RozpoznajTyp(r.TypKsztaltu);

                            Console.WriteLine($"🔹 Region po podziale: {r.TypKsztaltu} z {r.Wierzcholki.Count} wierzchołkami. - SKRZYDŁO (bez podziału)");

                            if (r.TypKsztaltu == "xhouseshape" && r.Wierzcholki.Count == 4)
                            {
                                r.TypKsztaltu = "trapez";
                            }

                            if (r.TypKsztaltu == "trapez")
                            {
                                if (r.Wierzcholki.Count == 2)
                                {
                                    r.TypKsztaltu = "linia";
                                }
                                else if (r.Wierzcholki.Count == 3)
                                {
                                    r.TypKsztaltu = "trójkąt";
                                }
                                else if (r.Wierzcholki.Count == 4 && CzyProstokat(r.Wierzcholki))
                                {
                                    r.TypKsztaltu = "prostokąt";
                                }
                            }
                        }
                    }
                    else
                    {
                        // SĄ LINIE DZIELĄCE - wykonaj podział
                        Console.WriteLine($"✅ Znaleziono {linieDzielace.Count} linii dzielących - wykonuję podział");
                        podzielone = PodzielRegionRekurencyjnieDeterministycznie(initial, linieDzielace, id, rama);

                        Console.WriteLine($"🔲 Generowanie regionów PodzielRegionRekurencyjnieDeterministycznie podzielone.Count: {podzielone.Count}");

                        foreach (var r in podzielone)
                        {
                            // Usuwanie duplikatów wierzchołków
                            r.Wierzcholki = r.Wierzcholki
                                .GroupBy(p => new { X = Math.Round(p.X, 2), Y = Math.Round(p.Y, 2) })
                                .Select(g => g.First())
                                .ToList();

                            // USUŃ SEGMENTY O ZEROWEJ DŁUGOŚCI Z KONTURU
                            // 1️⃣ usuń śmieci (zerowe segmenty)
                            r.Kontur = r.Kontur
                                .Where(s => !CzySegmentZerowejDlugosci(s))
                                .ToList();

                            // 2️⃣ grupowanie (BEZ NaN + normalizacja kierunku)
                            var unikalneSegmenty = r.Kontur
                                .GroupBy(s =>
                                {
                                    var startX = Math.Round(s.Start.X, 2);
                                    var startY = Math.Round(s.Start.Y, 2);
                                    var endX = Math.Round(s.End.X, 2);
                                    var endY = Math.Round(s.End.Y, 2);

                                    // 🔥 NORMALIZACJA KIERUNKU (A→B == B→A)
                                    // Oba kierunki mają ten sam klucz
                                    bool reversed = false;
                                    if (startX > endX || (startX == endX && startY > endY))
                                    {
                                        (startX, endX) = (endX, startX);
                                        (startY, endY) = (endY, startY);
                                        reversed = true;
                                    }

                                    return new
                                    {
                                        Type = s.Type,
                                        StartX = startX,
                                        StartY = startY,
                                        EndX = endX,
                                        EndY = endY,

                                        // 🔥 zamiast NaN → 0
                                        CenterX = s.Type == SegmentType.Arc && s.Center.HasValue ? Math.Round(s.Center.Value.X, 2) : 0,
                                        CenterY = s.Type == SegmentType.Arc && s.Center.HasValue ? Math.Round(s.Center.Value.Y, 2) : 0,
                                        Radius = s.Type == SegmentType.Arc ? Math.Round(s.Radius, 2) : 0,
                                        CounterClockwise = s.Type == SegmentType.Arc && s.CounterClockwise
                                    };
                                })
                                .Select(g =>
                                {
                                    var seg = g.First();
                                    // Jeśli segment był odwrócony w kluczu, zwróć go w znormalizowanej formie
                                    if (seg.Start.X > seg.End.X || (seg.Start.X == seg.End.X && seg.Start.Y > seg.End.Y))
                                    {
                                        if (seg.Type == SegmentType.Arc && seg.Center.HasValue)
                                        {
                                            return new ContourSegment(
                                                seg.End,
                                                seg.Start,
                                                seg.Center,
                                                seg.Radius,
                                                !seg.CounterClockwise
                                            )
                                            {
                                                Informacja = seg.Informacja
                                            };
                                        }
                                        else
                                        {
                                            return new ContourSegment(seg.End, seg.Start)
                                            {
                                                Informacja = seg.Informacja
                                            };
                                        }
                                    }
                                    return seg;
                                })
                                .ToList();

                            // 3️⃣ odbudowa segmentów
                            var nowyKontur = new List<ContourSegment>();

                            foreach (var segment in unikalneSegmenty)
                            {
                                ContourSegment nowySegment;

                                if (segment.Type == SegmentType.Arc && segment.Center.HasValue)
                                {
                                    nowySegment = new ContourSegment(
                                        new XPoint(segment.Start.X, segment.Start.Y),
                                        new XPoint(segment.End.X, segment.End.Y),
                                        new XPoint(segment.Center.Value.X, segment.Center.Value.Y),
                                        segment.Radius,
                                        segment.CounterClockwise
                                    );
                                }
                                else
                                {
                                    nowySegment = new ContourSegment(
                                        new XPoint(segment.Start.X, segment.Start.Y),
                                        new XPoint(segment.End.X, segment.End.Y)
                                    );
                                }

                                nowySegment.Informacja = ramaInfo;
                                nowyKontur.Add(nowySegment);
                            }

                            // 4️⃣ przypisanie
                            r.Kontur = nowyKontur;

                            // USUŃ SEGMENTY O ZEROWEJ DŁUGOŚCI PONOWNIE
                            r.Kontur = r.Kontur
                                .Where(s => !CzySegmentZerowejDlugosci(s))
                                .ToList();

                            // 5️⃣ Sortowanie
                            r.Kontur = OrderSegmentsForClosedContour(r.Kontur);

                            r.RozpoznajTyp(r.TypKsztaltu);

                            Console.WriteLine($"🔹 Region po podziale: {r.TypKsztaltu} z {r.Wierzcholki.Count} wierzchołkami. - SKRZYDŁO (z podziałem)");

                            if (r.TypKsztaltu == "xhouseshape" && r.Wierzcholki.Count == 4)
                            {
                                r.TypKsztaltu = "trapez";
                            }

                            if (r.TypKsztaltu == "trapez")
                            {
                                if (r.Wierzcholki.Count == 2)
                                {
                                    r.TypKsztaltu = "linia";
                                }
                                else if (r.Wierzcholki.Count == 3)
                                {
                                    r.TypKsztaltu = "trójkąt";
                                }
                                else if (r.Wierzcholki.Count == 4 && CzyProstokat(r.Wierzcholki))
                                {
                                    r.TypKsztaltu = "prostokąt";
                                }
                            }
                        }
                    }

                    regions.AddRange(podzielone);
                }
            }

            await Task.Delay(1);
            return regions;
        }

        // Dodaj tę funkcję pomocniczą na końcu klasy
        private static bool CzySegmentZerowejDlugosci(ContourSegment segment)
        {
            const double tolerance = 0.01;

            if (segment.Type == SegmentType.Line)
            {
                return Distance(segment.Start, segment.End) < tolerance;
            }

            // ARC
            if (!segment.Center.HasValue)
                return true;

            // ten sam punkt start i end = łuk zerowy
            if (Distance(segment.Start, segment.End) < tolerance)
                return true;

            // promień zerowy
            if (segment.Radius < tolerance)
                return true;

            // dodatkowo sprawdzamy czy oba punkty naprawdę leżą na łuku
            double d1 = Distance(segment.Start, segment.Center.Value);
            double d2 = Distance(segment.End, segment.Center.Value);

            if (Math.Abs(d1 - segment.Radius) > 1.0) return true;
            if (Math.Abs(d2 - segment.Radius) > 1.0) return true;

            return false;
        }
        public static double Distance(XPoint a, XPoint b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static ShapeRegion CloneRegion(ShapeRegion src, double _currentScale = 1)
        {
            if (src == null) return null!;
            return new ShapeRegion
            {
                Id = src.Id,
                IdMaster = src.IdMaster,
                IdRegionuPonizej = src.IdRegionuPonizej,
                TypKsztaltu = src.TypKsztaltu,
                TypLiniiDzielacej = src.TypLiniiDzielacej,
                Rama = src.Rama,
                LinieDzielace = src.LinieDzielace?.Select(l =>
                    new XLineShape(
                        l.X1,
                        l.Y1,
                        l.X2,
                        l.Y2,
                        _currentScale,
                        l.NazwaObj,
                        l.RuchomySlupek,
                        l.PionPoziom,
                        l.DualRama,
                        l.GenerowaneZRamy
                    )
                ).ToList() ?? new List<XLineShape>(),
                Wierzcholki = src.Wierzcholki?
                    .Select(p => new XPoint { X = p.X, Y = p.Y })
                    .ToList() ?? new List<XPoint>(),
                Kontur = src.Kontur?.Select(s =>
                {
                    if (s.Type == SegmentType.Line)
                        return new ContourSegment(new XPoint { X = s.Start.X, Y = s.Start.Y },
                                                  new XPoint { X = s.End.X, Y = s.End.Y });
                    else
                        return new ContourSegment(new XPoint { X = s.Start.X, Y = s.Start.Y },
                                                  new XPoint { X = s.End.X, Y = s.End.Y },
                                                  s.Center != null ? new XPoint { X = s.Center.Value.X, Y = s.Center.Value.Y } : null,
                                                  s.Radius,
                                                  s.CounterClockwise);
                }).ToList() ?? new List<ContourSegment>()
            };
        }

        private static bool CzyProstokat(List<XPoint> punkty)
        {
            if (punkty.Count != 4) return false;

            static double DistanceSquared(XPoint a, XPoint b) =>
                Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2);

            static double Dot(XPoint a, XPoint b, XPoint c)
            {
                var ab = new { X = b.X - a.X, Y = b.Y - a.Y };
                var bc = new { X = c.X - b.X, Y = c.Y - b.Y };
                return ab.X * bc.X + ab.Y * bc.Y;
            }

            // Kolejność zakładana: 0-1-2-3 (np. zgodnie z ruchem wskazówek zegara)
            bool kątyProste =
                Math.Abs(Dot(punkty[0], punkty[1], punkty[2])) < 1e-2 &&
                Math.Abs(Dot(punkty[1], punkty[2], punkty[3])) < 1e-2 &&
                Math.Abs(Dot(punkty[2], punkty[3], punkty[0])) < 1e-2 &&
                Math.Abs(Dot(punkty[3], punkty[0], punkty[1])) < 1e-2;

            bool bokiRowne = Math.Abs(DistanceSquared(punkty[0], punkty[1]) - DistanceSquared(punkty[2], punkty[3])) < 1e-2 &&
                                Math.Abs(DistanceSquared(punkty[1], punkty[2]) - DistanceSquared(punkty[3], punkty[0])) < 1e-2;

            return kątyProste && bokiRowne;
        }

        private static List<ShapeRegion> PodzielRegionRekurencyjnie(
         ShapeRegion region,
         List<XLineShape> lines,
         string idMaster,
         bool rama)
        {
            var wynik = new List<ShapeRegion> { region };

            foreach (var line in lines)
            {
                var next = new List<ShapeRegion>();

                foreach (var r in wynik)
                {
                    // 1️⃣ Podział wierzchołków
                    var splitLinie = PodzielPolygonPoLinii(r.Wierzcholki, line);

                    // 2️⃣ Podział konturu
                    var splitFullKontur = PodzielKonturPoLinii(r.Kontur, line);

                    // ✅ Walidacja: liczba podziałów musi się zgadzać
                    if (splitLinie.Count > 1 && splitFullKontur.Count == splitLinie.Count)
                    {
                        for (int i = 0; i < splitLinie.Count; i++)
                        {
                            var poly = splitLinie[i];

                            // Dopasowanie konturu do poligonu
                            var kontur = i < splitFullKontur.Count ? splitFullKontur[i] : splitFullKontur.Last();

                            next.Add(new ShapeRegion
                            {
                                Wierzcholki = poly,
                                Kontur = kontur,
                                TypKsztaltu = r.TypKsztaltu,
                                LinieDzielace = r.LinieDzielace.Concat(new[] { line }).ToList(),
                                IdMaster = idMaster,
                                Rama = rama,
                                Id = r.Id + "_" + line.ID + "_" + Guid.NewGuid().ToString(),
                                TypLiniiDzielacej = r.TypLiniiDzielacej
                            });
                        }
                    }
                    else
                    {
                        // Jeśli podział nie wystąpił, zostawiamy region bez zmian
                        next.Add(r);
                    }
                }

                // Aktualizujemy listę wyników po podziale przez tę linię
                wynik = next;
            }

            return wynik;
        }

        private static List<ShapeRegion> PodzielRegionRekurencyjnieDeterministycznie(
        ShapeRegion region,
        List<XLineShape> lines,
        string rootId,
        bool rama)
        {
            // Początkowa lista wyników zawiera oryginalny region
            var wynik = new List<ShapeRegion> { region };

            int indexLinii = 0;

            foreach (var line in lines)
            {
                var next = new List<ShapeRegion>();

                foreach (var r in wynik)
                {
                    // 1️⃣ Podział wierzchołków
                    var splitLinie = PodzielPolygonPoLinii(r.Wierzcholki, line);

                    // 2️⃣ Podział konturu
                    var splitFullKontur = PodzielKonturPoLinii(r.Kontur, line);

                    if (splitLinie.Count > 1 && splitFullKontur.Count > 0)
                    {
                        int indexChild = 0;

                        for (int i = 0; i < splitLinie.Count; i++)
                        {
                            var poly = splitLinie[i];

                            // Dopasowanie konturu do poligonu
                            var kontur = i < splitFullKontur.Count ? splitFullKontur[i] : splitFullKontur.Last();

                            string newId = $"{rootId}|L{indexLinii}|C{indexChild}";

                            next.Add(new ShapeRegion
                            {
                                Wierzcholki = poly,
                                Kontur = kontur,
                                TypKsztaltu = r.TypKsztaltu,
                                LinieDzielace = r.LinieDzielace.Concat(new[] { line }).ToList(),
                                IdMaster = rootId,
                                Rama = rama,
                                Id = newId,
                                TypLiniiDzielacej = r.TypLiniiDzielacej
                            });

                            indexChild++;
                        }
                    }
                    else
                    {
                        // Jeśli podział nie wystąpił, pozostawiamy region bez zmian
                        next.Add(r);
                    }
                }

                // Aktualizacja listy wyników po podziale tą linią
                wynik = next;
                indexLinii++;
            }

            return wynik;
        }

        public static List<List<ContourSegment>> PodzielKonturPoLinii(
            List<ContourSegment> contour,
            XLineShape line)
        {
            const double EPS = 1e-6;
            var result = new List<List<ContourSegment>>();

            // Krok 1: Znajdź wszystkie unikalne punkty przecięcia
            var uniqueIntersectionPoints = new List<XPoint>();
            var segmentIntersectionMap = new Dictionary<int, List<int>>();

            for (int i = 0; i < contour.Count; i++)
            {
                var segment = contour[i];
                segmentIntersectionMap[i] = new List<int>();

                if (segment.Type == SegmentType.Line)
                {
                    var pts = IntersectLineLine(segment, line);
                    foreach (var p in pts)
                    {
                        int idx = uniqueIntersectionPoints.FindIndex(x => Distance(x, p) < EPS);
                        if (idx == -1)
                        {
                            idx = uniqueIntersectionPoints.Count;
                            uniqueIntersectionPoints.Add(p);
                        }
                        segmentIntersectionMap[i].Add(idx);
                    }
                }
                else if (segment.Type == SegmentType.Arc)
                {
                    var pts = IntersectLineCircle(line, segment.Center.Value, segment.Radius)
                        .Where(p => IsPointOnArc(p, segment))
                        .ToList();

                    foreach (var p in pts)
                    {
                        int idx = uniqueIntersectionPoints.FindIndex(x => Distance(x, p) < EPS);
                        if (idx == -1)
                        {
                            idx = uniqueIntersectionPoints.Count;
                            uniqueIntersectionPoints.Add(p);
                        }
                        segmentIntersectionMap[i].Add(idx);
                    }
                }
            }

            Console.WriteLine($"PodzielKonturPoLinii Znaleziono {uniqueIntersectionPoints.Count} unikalnych punktów przecięcia");

            if (uniqueIntersectionPoints.Count < 2)
                return new List<List<ContourSegment>> { new List<ContourSegment>(contour) };

            // Weź dwa punkty przecięcia (pierwszy i ostatni na linii)
            var orderedPoints = uniqueIntersectionPoints
                .OrderBy(p => ProjectAlongLine(p, line))
                .ToList();

            var p1 = orderedPoints.First();
            var p2 = orderedPoints.Last();

            Console.WriteLine($"PodzielKonturPoLinii P1: ({p1.X:F2},{p1.Y:F2}), P2: ({p2.X:F2},{p2.Y:F2})");

            // Krok 2: Przypisz każdy segment do lewej lub prawej strony
            var leftSegments = new List<ContourSegment>();
            var rightSegments = new List<ContourSegment>();

            for (int i = 0; i < contour.Count; i++)
            {
                var segment = contour[i];
                var intersectionIndices = segmentIntersectionMap[i];

                if (intersectionIndices.Count == 0)
                {
                    // Segment bez przecięcia - sprawdź jego środek
                    var midPoint = new XPoint(
                        (segment.Start.X + segment.End.X) / 2,
                        (segment.Start.Y + segment.End.Y) / 2);

                    bool isLeft = IsPointOnLeftSide(line, midPoint);

                    if (isLeft)
                        leftSegments.Add(segment);
                    else
                        rightSegments.Add(segment);
                }
                else
                {
                    // Segment z przecięciem - podziel go
                    var splitPoints = intersectionIndices
                        .Select(idx => uniqueIntersectionPoints[idx])
                        .OrderBy(p => Distance(segment.Start, p))
                        .ToList();

                    var allPoints = new List<XPoint> { segment.Start };
                    allPoints.AddRange(splitPoints);
                    allPoints.Add(segment.End);

                    for (int j = 0; j < allPoints.Count - 1; j++)
                    {
                        if (Distance(allPoints[j], allPoints[j + 1]) < EPS)
                            continue;

                        ContourSegment newSeg;
                        if (segment.Type == SegmentType.Arc)
                            newSeg = CreateArc(allPoints[j], allPoints[j + 1], segment);

                        else
                            newSeg = new ContourSegment(allPoints[j], allPoints[j + 1])
                            {
                                Informacja = segment.Informacja
                            };

                        // Określ przynależność na podstawie punktu środkowego
                        var midPoint = new XPoint(
                            (allPoints[j].X + allPoints[j + 1].X) / 2,
                            (allPoints[j].Y + allPoints[j + 1].Y) / 2);

                        bool isLeft = IsPointOnLeftSide(line, midPoint);

                        if (isLeft)
                            leftSegments.Add(newSeg);
                        else
                            rightSegments.Add(newSeg);
                    }
                }
            }

            Console.WriteLine($"PodzielKonturPoLinii Lewa strona: {leftSegments.Count} segmentów");
            Console.WriteLine($"PodzielKonturPoLinii Prawa strona: {rightSegments.Count} segmentów");

            // Krok 3: Dodaj linię podziału
            if (leftSegments.Count > 0 && rightSegments.Count > 0)
            {
                leftSegments.Add(new ContourSegment(p2, p1)
                {
                    Informacja = "linia podziału"
                });

                rightSegments.Add(new ContourSegment(p1, p2)
                {
                    Informacja = "linia podziału"
                });
            }

            // NIE SCALAJ ŁUKÓW - zostaw oryginalny podział
            // leftSegments = MergeAdjacentArcsSimple(leftSegments);  // ZAKOMENTUJ
            // rightSegments = MergeAdjacentArcsSimple(rightSegments); // ZAKOMENTUJ

            // Krok 4: Uporządkuj kontury
            if (leftSegments.Count > 0)
                result.Add(OrderSegmentsForClosedContour(leftSegments));

            if (rightSegments.Count > 0)
                result.Add(OrderSegmentsForClosedContour(rightSegments));

            return result;
        }


        static double ProjectAlongLine(XPoint p, XLineShape line)
        {
            double dx = line.X2 - line.X1;
            double dy = line.Y2 - line.Y1;

            return (p.X - line.X1) * dx + (p.Y - line.Y1) * dy;
        }

        // Nowa funkcja pomocnicza do porządkowania segmentów w zamknięty kontur
        private static List<ContourSegment> OrderSegmentsForClosedContour(List<ContourSegment> segments)
        {
            if (segments.Count <= 1)
                return segments;

            segments = RemoveDuplicateSegments(segments);
            segments = segments.Where(s => !CzySegmentZerowejDlugosci(s)).ToList();

            var ordered = new List<ContourSegment>();
            var remaining = new List<ContourSegment>(segments);

            // Zacznij od pierwszego segmentu
            ordered.Add(remaining[0]);
            var currentEnd = remaining[0].End;
            remaining.RemoveAt(0);

            while (remaining.Count > 0)
            {
                // Znajdź segment, który zaczyna się w miejscu końca poprzedniego
                var nextSegment = remaining
                    .OrderBy(s => Distance(s.Start, currentEnd))
                    .FirstOrDefault(s => Distance(s.Start, currentEnd) < 0.5);

                if (nextSegment == null)
                {
                    // Jeśli nie znaleziono, spróbuj odwrócić któryś z segmentów
                    nextSegment = remaining.FirstOrDefault(s =>
                        Distance(s.End, currentEnd) < 0.5);

                    if (nextSegment != null)
                    {
                        // Odwróć segment
                        var reversed = ReverseSegment(nextSegment);
                        remaining.Remove(nextSegment);
                        ordered.Add(reversed);
                        currentEnd = reversed.End;
                        continue;
                    }

                    // Jeśli nadal nie znaleziono, przerwij
                    break;
                }

                remaining.Remove(nextSegment);
                ordered.Add(nextSegment);
                currentEnd = nextSegment.End;
            }

            // 🔑 SPRAWDŹ CZY KONTUR JEST CCW (POLECENIE: ZAWSZE CCW)
            if (ordered.Count > 2)
            {
                // Oblicz pole konturu (dodatnie = CCW, ujemne = CW)
                double pole = 0;
                for (int i = 0; i < ordered.Count; i++)
                {
                    var current = ordered[i];
                    var next = ordered[(i + 1) % ordered.Count];
                    pole += (current.Start.X * next.Start.Y) - (next.Start.X * current.Start.Y);
                }
                pole /= 2.0;

                // Jeśli kontur jest CW (pole ujemne), odwróć go do CCW
                if (pole < 0)
                {
                    // Odwróć kolejność segmentów
                    ordered.Reverse();

                    // Odwróć każdy segment (zamień Start↔End)
                    for (int i = 0; i < ordered.Count; i++)
                    {
                        ordered[i] = ReverseSegment(ordered[i]);
                    }
                }
            }

            // 🔑 TERAZ USTAW ŁUKI NA CCW (TRUE)
            for (int i = 0; i < ordered.Count; i++)
            {
                if (ordered[i].Type == SegmentType.Arc)
                {
                    ordered[i].CounterClockwise = true; // ✅ CCW - przeciwnie do zegara
                }
            }

            return ordered;
        }

        // Pomocnicza funkcja do odwracania segmentu
        private static ContourSegment ReverseSegment(ContourSegment seg)
        {
            if (seg.Type == SegmentType.Arc && seg.Center.HasValue)
            {
                return new ContourSegment(
                    seg.End,
                    seg.Start,
                    seg.Center,
                    seg.Radius,
                    !seg.CounterClockwise  // Odwróć kierunek
                )
                {
                    Informacja = seg.Informacja
                };
            }
            else
            {
                return new ContourSegment(seg.End, seg.Start)
                {
                    Informacja = seg.Informacja
                };
            }
        }


        static bool IsPointOnArc(XPoint p, ContourSegment arc)
        {
            if (arc.Center == null)
                return false;

            var center = arc.Center.Value;

            double startAngle = Math.Atan2(arc.Start.Y - center.Y, arc.Start.X - center.X);
            double endAngle = Math.Atan2(arc.End.Y - center.Y, arc.End.X - center.X);
            double pointAngle = Math.Atan2(p.Y - center.Y, p.X - center.X);

            startAngle = NormalizeAngle(startAngle);
            endAngle = NormalizeAngle(endAngle);
            pointAngle = NormalizeAngle(pointAngle);

            if (arc.CounterClockwise)
            {
                if (startAngle <= endAngle)
                    return pointAngle >= startAngle && pointAngle <= endAngle;
                else
                    return pointAngle >= startAngle || pointAngle <= endAngle;
            }
            else
            {
                if (startAngle >= endAngle)
                    return pointAngle <= startAngle && pointAngle >= endAngle;
                else
                    return pointAngle <= startAngle || pointAngle >= endAngle;
            }
        }


        static ContourSegment CreateArc(XPoint start, XPoint end, ContourSegment original)
        {
            if (original.Center == null)
                throw new Exception("Arc without center");

            var center = original.Center.Value;

            // Sprawdź, czy punkty leżą na okręgu (z tolerancją)
            double distStart = Distance(start, center);
            double distEnd = Distance(end, center);

            const double radiusTolerance = 2.0;
            if (Math.Abs(distStart - original.Radius) > radiusTolerance ||
                Math.Abs(distEnd - original.Radius) > radiusTolerance)
            {
                return new ContourSegment(start, end)
                {
                    Informacja = original.Informacja
                };
            }

            // Jeżeli to dokładnie oryginalny segment, zwróć go
            if (Distance(start, original.Start) < 1e-3 && Distance(end, original.End) < 1e-3)
                return original;

            // ZAWSZE zachowuj oryginalny kierunek łuku
            // Nie próbuj zgadywać kierunku - użyj oryginalnego
            return new ContourSegment(start, end, center, original.Radius, original.CounterClockwise)
            {
                Informacja = original.Informacja
            };
        }


        static List<XPoint> IntersectLineCircle(XLineShape line, XPoint center, double radius)
        {
            // standardowa matematyka przecięcia
            var result = new List<XPoint>();

            var dx = line.X2 - line.X1;
            var dy = line.Y2 - line.Y1;

            var fx = line.X1 - center.X;
            var fy = line.Y1 - center.Y;

            var a = dx * dx + dy * dy;
            var b = 2 * (fx * dx + fy * dy);
            var c = fx * fx + fy * fy - radius * radius;

            var discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
                return result;

            discriminant = Math.Sqrt(discriminant);

            var t1 = (-b - discriminant) / (2 * a);
            var t2 = (-b + discriminant) / (2 * a);

            result.Add(new XPoint(line.X1 + t1 * dx, line.Y1 + t1 * dy));

            if (discriminant > 0)
                result.Add(new XPoint(line.X1 + t2 * dx, line.Y1 + t2 * dy));

            return result;
        }

        static List<XPoint> IntersectLineLine(ContourSegment seg, XLineShape line)
        {
            var result = new List<XPoint>();

            double x1 = seg.Start.X;
            double y1 = seg.Start.Y;
            double x2 = seg.End.X;
            double y2 = seg.End.Y;

            double x3 = line.X1;
            double y3 = line.Y1;
            double x4 = line.X2;
            double y4 = line.Y2;

            double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

            if (Math.Abs(denom) < 1e-9)
                return result; // równoległe

            double px = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / denom;
            double py = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / denom;

            var p = new XPoint(px, py);

            if (IsPointOnSegment(p, seg))
                result.Add(p);

            return result;
        }

        static bool IsPointOnLeftSide(XLineShape line, XPoint p)
        {
            double value =
                (line.X2 - line.X1) * (p.Y - line.Y1) -
                (line.Y2 - line.Y1) * (p.X - line.X1);

            return value > 0;
        }

        static bool IsPointOnSegment(XPoint p, ContourSegment seg)
        {
            double minX = Math.Min(seg.Start.X, seg.End.X) - 1e-6;
            double maxX = Math.Max(seg.Start.X, seg.End.X) + 1e-6;
            double minY = Math.Min(seg.Start.Y, seg.End.Y) - 1e-6;
            double maxY = Math.Max(seg.Start.Y, seg.End.Y) + 1e-6;

            return p.X >= minX && p.X <= maxX &&
                   p.Y >= minY && p.Y <= maxY;
        }

        private static double NormalizeAngle(double angle)
        {
            while (angle < 0) angle += 2 * Math.PI;
            while (angle >= 2 * Math.PI) angle -= 2 * Math.PI;
            return angle;
        }
        private static List<List<XPoint>> PodzielPolygonPoLinii(List<XPoint> poly, XLineShape line)
        {
            var left = new List<XPoint>();
            var right = new List<XPoint>();


            // Dodajemy punkty w kolejności zgodnej z oryginalnym wielokątem
            for (int i = 0; i < poly.Count; i++)
            {
                var curr = poly[i];
                var next = poly[(i + 1) % poly.Count];

                bool currLeft = PunktPoLewejStronie(curr, line);

                // Dodaj bieżący punkt do odpowiedniej listy
                if (currLeft)
                    left.Add(new XPoint(curr.X, curr.Y));
                else
                    right.Add(new XPoint(curr.X, curr.Y));

                // Sprawdź czy segment przecina linię
                bool nextLeft = PunktPoLewejStronie(next, line);

                if (currLeft != nextLeft)
                {
                    // Segment przecina linię - znajdź punkt przecięcia
                    if (ObliczPrzeciecie(curr, next, line, out var pt))
                    {
                        // Dodaj punkt przecięcia do OBU list
                        left.Add(new XPoint(pt.X, pt.Y));
                        right.Add(new XPoint(pt.X, pt.Y));
                    }
                }
            }

            var result = new List<List<XPoint>>();

            // Oczyść listy z duplikatów zachowując kolejność
            if (left.Count >= 3)
            {
                left = RemoveDuplicatePoints(left);
                result.Add(left);
            }

            if (right.Count >= 3)
            {
                right = RemoveDuplicatePoints(right);
                result.Add(right);
            }

            return result;
        }

        // Dodaj tę funkcję pomocniczą
        private static List<XPoint> RemoveDuplicatePoints(List<XPoint> points)
        {
            var result = new List<XPoint>();

            foreach (var p in points)
            {
                // Sprawdź czy punkt już istnieje (z tolerancją)
                if (!result.Any(existing => Distance(existing, p) < 0.5))
                {
                    result.Add(p);
                }
            }

            // Upewnij się, że pierwszy i ostatni punkt nie są takie same
            if (result.Count > 1 && Distance(result[0], result[result.Count - 1]) < 0.5)
            {
                result.RemoveAt(result.Count - 1);
            }

            return result;
        }

        private static bool PunktPoLewejStronie(XPoint p, XLineShape l)
        {
            double d = (l.X2 - l.X1) * (p.Y - l.Y1) - (l.Y2 - l.Y1) * (p.X - l.X1);
            return d >= 0;
        }
        private static bool ObliczPrzeciecie(XPoint p1, XPoint p2, XLineShape l, out XPoint pt)
        {
            pt = new XPoint(); double x1 = p1.X, y1 = p1.Y, x2 = p2.X, y2 = p2.Y;
            double x3 = l.X1, y3 = l.Y1, x4 = l.X2, y4 = l.Y2;
            double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (Math.Abs(denom) < 1e-6) return false;
            double px = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / denom;
            double py = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / denom;
            pt = new XPoint(px, py);
            return true;
        }

        private static List<IShapeDC> UsunDuplikatyShape(List<IShapeDC> shapes)
        {
            var seen = new HashSet<string>();
            var list = new List<IShapeDC>();

            foreach (var s in shapes)
            {
                string key;

                // Dla linii - normalizuj kierunek (A→B == B→A)
                if (s is XLineShape line)
                {
                    double x1 = Math.Round(line.X1, 2);
                    double y1 = Math.Round(line.Y1, 2);
                    double x2 = Math.Round(line.X2, 2);
                    double y2 = Math.Round(line.Y2, 2);

                    // Normalizacja: zawsze mniejsza współrzędna na początek
                    if (x1 > x2 || (x1 == x2 && y1 > y2))
                    {
                        (x1, x2) = (x2, x1);
                        (y1, y2) = (y2, y1);
                    }

                    key = $"LINE_{x1}_{y1}_{x2}_{y2}";
                }
                else
                {
                    // Dla innych kształtów - bounding box
                    var b = s.GetBoundingBox();
                    key = $"SHAPE_{b.X:F2}_{b.Y:F2}_{b.Width:F2}_{b.Height:F2}";
                }

                if (seen.Add(key))
                {
                    list.Add(s);
                }
            }

            return list;
        }

        public static List<XPoint> GenerateCircleVertices(
        double centerX,
        double centerY,
        double radius,
        int segments)
        {
            var points = new List<XPoint>();

            for (int i = 0; i < segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                points.Add(new XPoint(
                    centerX + radius * Math.Cos(angle),
                    centerY + radius * Math.Sin(angle)
                ));
            }

            return points;
        }

        private static List<ContourSegment> RemoveDuplicateSegments(List<ContourSegment> segments)
        {
            var seen = new HashSet<string>();
            var result = new List<ContourSegment>();

            foreach (var seg in segments)
            {
                var startX = Math.Round(seg.Start.X, 2);
                var startY = Math.Round(seg.Start.Y, 2);
                var endX = Math.Round(seg.End.X, 2);
                var endY = Math.Round(seg.End.Y, 2);

                // Normalizacja kierunku
                if (startX > endX || (startX == endX && startY > endY))
                {
                    (startX, endX) = (endX, startX);
                    (startY, endY) = (endY, startY);
                }

                string key = $"{seg.Type}|{startX}|{startY}|{endX}|{endY}";

                if (seg.Type == SegmentType.Arc && seg.Center.HasValue)
                {
                    key += $"|{Math.Round(seg.Center.Value.X, 2)}|{Math.Round(seg.Center.Value.Y, 2)}|{Math.Round(seg.Radius, 2)}";
                }

                if (seen.Add(key))
                {
                    result.Add(seg);
                }
            }

            return result;
        }
    }
}