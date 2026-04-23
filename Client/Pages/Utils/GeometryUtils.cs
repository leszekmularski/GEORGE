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
                            bool counterClockwise = false;

                            var segment = new ContourSegment(
                                p,
                                next,
                                center,
                                circ.Radius,
                                counterClockwise
                            );

                            segment.Informacja = ramaInfo;
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

                            // Sprawdź czy punkt jest na łuku (odległość od środka ≈ promień)
                            double d1 = Distance(p, arcCenter);
                            double d2 = Distance(next, arcCenter);

                            bool isArcSegment = Math.Abs(d1 - rtr.Radius) < 2.0 && Math.Abs(d2 - rtr.Radius) < 2.0;

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

                                    segment.Informacja = ramaInfo;
                                    return segment;
                                }
                            }

                            // Dla linii pionowych i poziomych
                            var lineSegment = new ContourSegment(p, next);
                            lineSegment.Informacja = ramaInfo;
                            return lineSegment;
                        }
                        // =========================
                        // PROSTOKĄT ZAOKRĄGLONY
                        // =========================
                        else if (shape is XRoundedRectangleShape rr)
                        {
                            double r = rr.Radius;

                            var centers = new List<(XPoint Center, string Corner)>
                    {
                        (new XPoint(rr.X + r, rr.Y + r), "TL"),                         // Top-Left
                        (new XPoint(rr.X + rr.Width - r, rr.Y + r), "TR"),              // Top-Right
                        (new XPoint(rr.X + rr.Width - r, rr.Y + rr.Height - r), "BR"),  // Bottom-Right
                        (new XPoint(rr.X + r, rr.Y + rr.Height - r), "BL")              // Bottom-Left
                    };

                            foreach (var (center, corner) in centers)
                            {
                                double d1 = Distance(p, center);
                                double d2 = Distance(next, center);

                                if (Math.Abs(d1 - r) < 0.5 && Math.Abs(d2 - r) < 0.5)
                                {
                                    // Określ kierunek łuku na podstawie narożnika
                                    bool counterClockwise = corner == "TL" || corner == "BR";

                                    var segment = new ContourSegment(
                                        p,
                                        next,
                                        center,
                                        r,
                                        counterClockwise
                                    );

                                    segment.Informacja = ramaInfo;
                                    return segment;
                                }
                            }

                            var lineSeg = new ContourSegment(p, next);
                            lineSeg.Informacja = ramaInfo;
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

                                segment.Informacja = ramaInfo;
                                return segment;
                            }

                            var lineSegLeft = new ContourSegment(p, next);
                            lineSegLeft.Informacja = ramaInfo;
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
                                bool counterClockwise = p.Y > next.Y;

                                var segment = new ContourSegment(
                                    p,
                                    next,
                                    new XPoint(centerX, centerY),
                                    r,
                                    counterClockwise
                                );

                                segment.Informacja = ramaInfo;
                                return segment;
                            }

                            var lineSegRight = new ContourSegment(p, next);
                            lineSegRight.Informacja = ramaInfo;
                            return lineSegRight;
                        }

                        // =========================
                        // DOMYŚLNA LINIA
                        // =========================
                        else
                        {
                            var segment = new ContourSegment(p, next);
                            segment.Informacja = ramaInfo;
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

                        // 2️⃣ grupowanie (BEZ NaN + normalizacja kierunku)
                        var unikalneSegmenty = r.Kontur
                            .GroupBy(s =>
                            {
                                var startX = Math.Round(s.Start.X, 2);
                                var startY = Math.Round(s.Start.Y, 2);
                                var endX = Math.Round(s.End.X, 2);
                                var endY = Math.Round(s.End.Y, 2);

                                // 🔥 NORMALIZACJA KIERUNKU (A→B == B→A)
                                if (startX > endX || (startX == endX && startY > endY))
                                {
                                    (startX, endX) = (endX, startX);
                                    (startY, endY) = (endY, startY);
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
                            .Select(g => g.First())
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
                                    if (startX > endX || (startX == endX && startY > endY))
                                    {
                                        (startX, endX) = (endX, startX);
                                        (startY, endY) = (endY, startY);
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
                                .Select(g => g.First())
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

        public static List<List<ContourSegment>> PodzielKonturPoLinii(List<ContourSegment> contour, XLineShape line)
        {
            var left = new List<ContourSegment>();
            var right = new List<ContourSegment>();

            // Zbierz wszystkie punkty przecięcia
            var allIntersectionPoints = new List<XPoint>();

            foreach (var seg in contour)
            {
                // Sprawdź przecięcia segmentu z linią
                var intersections = FindIntersectionsWithLine(seg, line);

                foreach (var pt in intersections)
                {
                    if (!allIntersectionPoints.Any(p => Distance(p, pt) < 0.5))
                        allIntersectionPoints.Add(pt);
                }

                if (intersections.Count == 0)
                {
                    // Brak przecięć - cały segment po jednej stronie
                    bool startLeft = PunktPoLewejStronie(seg.Start, line);
                    if (startLeft)
                        left.Add(CloneSegment(seg));
                    else
                        right.Add(CloneSegment(seg));
                }
                else if (intersections.Count == 1)
                {
                    // Jedno przecięcie
                    var pt = intersections[0];

                    if (seg.Type == SegmentType.Arc && seg.Center != null)
                    {
                        var (arc1, arc2) = SplitArcAtPoint(seg, pt);

                        bool arc1Left = PunktPoLewejStronie(GetArcMidPoint(arc1), line);

                        if (arc1Left)
                        {
                            left.Add(arc1);
                            right.Add(arc2);
                        }
                        else
                        {
                            right.Add(arc1);
                            left.Add(arc2);
                        }
                    }
                    else
                    {
                        // Linia prosta
                        bool startLeft = PunktPoLewejStronie(seg.Start, line);

                        var seg1 = new ContourSegment(seg.Start, pt);
                        seg1.Informacja = seg.Informacja;
                        var seg2 = new ContourSegment(pt, seg.End);
                        seg2.Informacja = seg.Informacja;

                        if (startLeft)
                        {
                            left.Add(seg1);
                            right.Add(seg2);
                        }
                        else
                        {
                            right.Add(seg1);
                            left.Add(seg2);
                        }
                    }
                }
                else if (intersections.Count == 2)
                {
                    // Dwa przecięcia - segment przechodzi przez linię
                    var pt1 = intersections[0];
                    var pt2 = intersections[1];

                    var sorted = SortPointsAlongArc(pt1, pt2, seg);
                    pt1 = sorted[0];
                    pt2 = sorted[1];

                    if (seg.Type == SegmentType.Arc && seg.Center != null)
                    {
                        var arc1 = CreateArcSegment(seg.Start, pt1, seg);
                        var arc2 = CreateArcSegment(pt1, pt2, seg);
                        var arc3 = CreateArcSegment(pt2, seg.End, seg);

                        bool arc1Left = PunktPoLewejStronie(GetArcMidPoint(arc1), line);
                        bool arc2Left = PunktPoLewejStronie(GetArcMidPoint(arc2), line);
                        bool arc3Left = PunktPoLewejStronie(GetArcMidPoint(arc3), line);

                        if (arc1Left) left.Add(arc1); else right.Add(arc1);
                        if (arc2Left) left.Add(arc2); else right.Add(arc2);
                        if (arc3Left) left.Add(arc3); else right.Add(arc3);
                    }
                }
            }

            // DODAJ LINIĘ PODZIAŁU DO OBU STRON!
            if (allIntersectionPoints.Count >= 2)
            {
                // Sortuj punkty przecięcia wzdłuż linii
                var sortedPoints = allIntersectionPoints
                    .OrderBy(p => Distance(p, new XPoint(line.X1, line.Y1)))
                    .ToList();

                // Weź pierwsze dwa punkty przecięcia (powinny być dokładnie 2)
                var pt1 = sortedPoints[0];
                var pt2 = sortedPoints[1];

                // Dodaj linię podziału do lewej strony
                var dividingLineLeft = new ContourSegment(pt1, pt2);
                dividingLineLeft.Informacja = "linia podziału";
                left.Add(dividingLineLeft);

                // Dodaj linię podziału do prawej strony (odwróconą)
                var dividingLineRight = new ContourSegment(pt2, pt1);
                dividingLineRight.Informacja = "linia podziału";
                right.Add(dividingLineRight);
            }

            var result = new List<List<ContourSegment>>();

            if (left.Count > 0)
            {
                left = RemoveZeroLengthSegments(left);
                left = ConnectSegmentsIntoClosedPath(left, line, "linia podziału");
                if (left.Count >= 3)
                    result.Add(left);
            }

            if (right.Count > 0)
            {
                right = RemoveZeroLengthSegments(right);
                right = ConnectSegmentsIntoClosedPath(right, line, "linia podziału");
                if (right.Count >= 3)
                    result.Add(right);
            }

            // Jeśli nie udało się podzielić, zwróć oryginał
            if (result.Count != 2)
            {
                return new List<List<ContourSegment>> { contour };
            }

            return result;
        }

        #region Funkcje pomocnicze dla PodzielKonturPoLinii

        private static List<XPoint> FindIntersectionsWithLine(ContourSegment seg, XLineShape line)
        {
            var intersections = new List<XPoint>();

            if (seg.Type == SegmentType.Line)
            {
                if (ObliczPrzeciecie(seg.Start, seg.End, line, out var pt))
                {
                    if (IsPointOnLineSegment(pt, seg.Start, seg.End))
                        intersections.Add(pt);
                }
            }
            else if (seg.Type == SegmentType.Arc && seg.Center != null)
            {
                var circleIntersections = LineCircleIntersections(
                    new XPoint(line.X1, line.Y1),
                    new XPoint(line.X2, line.Y2),
                    seg.Center.Value,
                    seg.Radius
                );

                foreach (var pt in circleIntersections)
                {
                    if (IsPointOnArc(pt, seg))
                        intersections.Add(pt);
                }
            }

            return intersections;
        }

        private static List<XPoint> LineCircleIntersections(XPoint p1, XPoint p2, XPoint center, double radius)
        {
            var result = new List<XPoint>();

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double fx = p1.X - center.X;
            double fy = p1.Y - center.Y;

            double a = dx * dx + dy * dy;
            double b = 2 * (fx * dx + fy * dy);
            double c = (fx * fx + fy * fy) - radius * radius;

            double discriminant = b * b - 4 * a * c;

            if (discriminant < 0) return result;

            discriminant = Math.Sqrt(discriminant);

            double t1 = (-b - discriminant) / (2 * a);
            double t2 = (-b + discriminant) / (2 * a);

            if (t1 >= 0 && t1 <= 1)
                result.Add(new XPoint(p1.X + t1 * dx, p1.Y + t1 * dy));

            if (t2 >= 0 && t2 <= 1 && Math.Abs(t1 - t2) > 0.001)
                result.Add(new XPoint(p1.X + t2 * dx, p1.Y + t2 * dy));

            return result;
        }

        private static bool IsPointOnLineSegment(XPoint pt, XPoint segStart, XPoint segEnd)
        {
            const double tolerance = 0.5;

            double minX = Math.Min(segStart.X, segEnd.X) - tolerance;
            double maxX = Math.Max(segStart.X, segEnd.X) + tolerance;
            double minY = Math.Min(segStart.Y, segEnd.Y) - tolerance;
            double maxY = Math.Max(segStart.Y, segEnd.Y) + tolerance;

            if (pt.X < minX || pt.X > maxX || pt.Y < minY || pt.Y > maxY)
                return false;

            double cross = (pt.Y - segStart.Y) * (segEnd.X - segStart.X) -
                          (pt.X - segStart.X) * (segEnd.Y - segStart.Y);

            return Math.Abs(cross) < tolerance * 10;
        }

        private static bool IsPointOnArc(XPoint pt, ContourSegment arc)
        {
            if (arc.Center == null) return false;

            var center = arc.Center.Value;
            double dist = Distance(pt, center);

            if (Math.Abs(dist - arc.Radius) > 1.0) return false;

            double angle = Math.Atan2(pt.Y - center.Y, pt.X - center.X);
            double startAngle = Math.Atan2(arc.Start.Y - center.Y, arc.Start.X - center.X);
            double endAngle = Math.Atan2(arc.End.Y - center.Y, arc.End.X - center.X);

            angle = NormalizeAngle(angle);
            startAngle = NormalizeAngle(startAngle);
            endAngle = NormalizeAngle(endAngle);

            return IsAngleInArcRange(angle, startAngle, endAngle, arc.CounterClockwise);
        }

        private static double NormalizeAngle(double angle)
        {
            while (angle < 0) angle += 2 * Math.PI;
            while (angle >= 2 * Math.PI) angle -= 2 * Math.PI;
            return angle;
        }

        private static bool IsAngleInArcRange(double angle, double startAngle, double endAngle, bool isCCW)
        {
            const double tolerance = 0.01;

            if (isCCW)
            {
                if (startAngle <= endAngle)
                    return angle >= startAngle - tolerance && angle <= endAngle + tolerance;
                else
                    return angle >= startAngle - tolerance || angle <= endAngle + tolerance;
            }
            else
            {
                if (startAngle >= endAngle)
                    return angle <= startAngle + tolerance && angle >= endAngle - tolerance;
                else
                    return angle <= startAngle + tolerance || angle >= endAngle - tolerance;
            }
        }

        private static (ContourSegment, ContourSegment) SplitArcAtPoint(ContourSegment arc, XPoint splitPoint)
        {
            var arc1 = CreateArcSegment(arc.Start, splitPoint, arc);
            var arc2 = CreateArcSegment(splitPoint, arc.End, arc);
            return (arc1, arc2);
        }

        private static ContourSegment CreateArcSegment(XPoint start, XPoint end, ContourSegment originalArc)
        {
            var seg = new ContourSegment(start, end, originalArc.Center, originalArc.Radius, originalArc.CounterClockwise);
            seg.Informacja = originalArc.Informacja;
            return seg;
        }

        private static XPoint GetArcMidPoint(ContourSegment seg)
        {
            if (seg.Type == SegmentType.Line)
            {
                return new XPoint((seg.Start.X + seg.End.X) / 2, (seg.Start.Y + seg.End.Y) / 2);
            }
            else if (seg.Type == SegmentType.Arc && seg.Center != null)
            {
                var center = seg.Center.Value;
                double startAngle = Math.Atan2(seg.Start.Y - center.Y, seg.Start.X - center.X);
                double endAngle = Math.Atan2(seg.End.Y - center.Y, seg.End.X - center.X);

                double midAngle;
                if (seg.CounterClockwise)
                {
                    if (endAngle < startAngle) endAngle += 2 * Math.PI;
                    midAngle = (startAngle + endAngle) / 2;
                }
                else
                {
                    if (startAngle < endAngle) startAngle += 2 * Math.PI;
                    midAngle = (startAngle + endAngle) / 2;
                }

                return new XPoint(
                    center.X + seg.Radius * Math.Cos(midAngle),
                    center.Y + seg.Radius * Math.Sin(midAngle)
                );
            }

            return new XPoint((seg.Start.X + seg.End.X) / 2, (seg.Start.Y + seg.End.Y) / 2);
        }

        private static List<XPoint> SortPointsAlongArc(XPoint pt1, XPoint pt2, ContourSegment arc)
        {
            if (arc.Center == null) return new List<XPoint> { pt1, pt2 };

            var center = arc.Center.Value;

            double angle1 = Math.Atan2(pt1.Y - center.Y, pt1.X - center.X);
            double angle2 = Math.Atan2(pt2.Y - center.Y, pt2.X - center.X);
            double startAngle = Math.Atan2(arc.Start.Y - center.Y, arc.Start.X - center.X);

            angle1 = NormalizeAngleRelative(angle1, startAngle);
            angle2 = NormalizeAngleRelative(angle2, startAngle);

            return angle1 < angle2 ? new List<XPoint> { pt1, pt2 } : new List<XPoint> { pt2, pt1 };
        }

        private static double NormalizeAngleRelative(double angle, double reference)
        {
            angle = NormalizeAngle(angle);
            reference = NormalizeAngle(reference);
            double diff = angle - reference;
            if (diff < 0) diff += 2 * Math.PI;
            return diff;
        }

        private static ContourSegment CloneSegment(ContourSegment seg)
        {
            ContourSegment clone;
            if (seg.Type == SegmentType.Arc && seg.Center != null)
            {
                clone = new ContourSegment(
                    new XPoint(seg.Start.X, seg.Start.Y),
                    new XPoint(seg.End.X, seg.End.Y),
                    new XPoint(seg.Center.Value.X, seg.Center.Value.Y),
                    seg.Radius,
                    seg.CounterClockwise
                );
            }
            else
            {
                clone = new ContourSegment(
                    new XPoint(seg.Start.X, seg.Start.Y),
                    new XPoint(seg.End.X, seg.End.Y)
                );
            }
            clone.Informacja = seg.Informacja;
            return clone;
        }

        private static List<ContourSegment> RemoveZeroLengthSegments(List<ContourSegment> segments)
        {
            return segments.Where(s => Distance(s.Start, s.End) > 0.1).ToList();
        }

        private static List<ContourSegment> ConnectSegmentsIntoClosedPath(List<ContourSegment> segments, XLineShape dividingLine, string informacja)
        {
            if (segments.Count == 0) return segments;

            // Znajdź punkty przecięcia z linią dzielącą
            var boundaryPoints = new List<XPoint>();
            foreach (var seg in segments)
            {
                if (PunktNaLinii(seg.Start, dividingLine) && !boundaryPoints.Any(p => Distance(p, seg.Start) < 0.5))
                    boundaryPoints.Add(new XPoint(seg.Start.X, seg.Start.Y));
                if (PunktNaLinii(seg.End, dividingLine) && !boundaryPoints.Any(p => Distance(p, seg.End) < 0.5))
                    boundaryPoints.Add(new XPoint(seg.End.X, seg.End.Y));
            }

            // Usuń duplikaty segmentów (odwrócone kopie)
            var uniqueSegments = new List<ContourSegment>();
            foreach (var seg in segments)
            {
                bool isDuplicate = uniqueSegments.Any(s =>
                    (Distance(s.Start, seg.Start) < 0.5 && Distance(s.End, seg.End) < 0.5) ||
                    (Distance(s.Start, seg.End) < 0.5 && Distance(s.End, seg.Start) < 0.5));

                if (!isDuplicate)
                    uniqueSegments.Add(seg);
            }

            if (uniqueSegments.Count == 0) return uniqueSegments;

            // Uporządkuj segmenty w ciągłą ścieżkę
            var result = new List<ContourSegment>();
            var remaining = new List<ContourSegment>(uniqueSegments);

            var current = remaining[0];
            remaining.RemoveAt(0);
            result.Add(current);

            while (remaining.Count > 0)
            {
                var next = remaining.FirstOrDefault(s => Distance(current.End, s.Start) < 0.5);

                if (next == null)
                {
                    next = remaining.FirstOrDefault(s => Distance(current.End, s.End) < 0.5);
                    if (next != null)
                    {
                        next = ReverseSegment(next);
                    }
                }

                if (next == null)
                    break;

                remaining.Remove(next);
                result.Add(next);
                current = next;
            }

            // Domknij kontur
            if (result.Count > 0)
            {
                var first = result[0];
                var last = result[result.Count - 1];

                // Sprawdź czy kontur jest zamknięty
                if (Distance(last.End, first.Start) > 0.5)
                {
                    // Sprawdź czy mamy punkty na linii podziału
                    if (boundaryPoints.Count >= 2)
                    {
                        // Sortuj punkty wzdłuż linii podziału
                        var sortedBoundary = boundaryPoints
                            .OrderBy(p => Distance(p, new XPoint(dividingLine.X1, dividingLine.Y1)))
                            .ToList();

                        // Sprawdź połączenia z końcami ścieżki
                        var lastPoint = last.End;
                        var firstPoint = first.Start;

                        // Znajdź punkty na linii, które łączą się z końcami
                        var matchingLast = sortedBoundary.FirstOrDefault(p => Distance(p, lastPoint) < 0.5);
                        var matchingFirst = sortedBoundary.FirstOrDefault(p => Distance(p, firstPoint) < 0.5);

                        if (matchingLast.X != 0 || matchingLast.Y != 0)
                        {
                            // Koniec ścieżki jest na linii podziału
                            if (matchingFirst.X != 0 || matchingFirst.Y != 0)
                            {
                                // Oba końce są na linii - połącz je linią podziału
                                var dividingSegment = new ContourSegment(matchingLast, matchingFirst);
                                dividingSegment.Informacja = informacja;
                                result.Add(dividingSegment);
                            }
                            else
                            {
                                // Tylko koniec jest na linii - znajdź drugi punkt na linii
                                var otherPoint = sortedBoundary.FirstOrDefault(p => Distance(p, matchingLast) > 0.5);

                                if (otherPoint.X != 0 || otherPoint.Y != 0)
                                {
                                    // Dodaj linię podziału
                                    var dividingSegment = new ContourSegment(matchingLast, otherPoint);
                                    dividingSegment.Informacja = informacja;
                                    result.Add(dividingSegment);

                                    // Połącz z początkiem jeśli potrzeba
                                    if (Distance(otherPoint, firstPoint) > 0.5)
                                    {
                                        var finalSegment = new ContourSegment(otherPoint, firstPoint);
                                        finalSegment.Informacja = informacja;
                                        result.Add(finalSegment);
                                    }
                                }
                                else
                                {
                                    // Tylko jeden punkt na linii - połącz bezpośrednio
                                    var closingSegment = new ContourSegment(lastPoint, firstPoint);
                                    closingSegment.Informacja = informacja;
                                    result.Add(closingSegment);
                                }
                            }
                        }
                        else if (matchingFirst.X != 0 || matchingFirst.Y != 0)
                        {
                            // Tylko początek jest na linii
                            var otherPoint = sortedBoundary.FirstOrDefault(p => Distance(p, matchingFirst) > 0.5);

                            if (otherPoint.X != 0 || otherPoint.Y != 0)
                            {
                                // Najpierw połącz koniec z otherPoint
                                var firstSegment = new ContourSegment(lastPoint, otherPoint);
                                firstSegment.Informacja = informacja;
                                result.Add(firstSegment);

                                // Potem otherPoint z początkiem (linią podziału)
                                var dividingSegment = new ContourSegment(otherPoint, matchingFirst);
                                dividingSegment.Informacja = informacja;
                                result.Add(dividingSegment);
                            }
                            else
                            {
                                var closingSegment = new ContourSegment(lastPoint, firstPoint);
                                closingSegment.Informacja = informacja;
                                result.Add(closingSegment);
                            }
                        }
                        else
                        {
                            // Żaden koniec nie jest na linii - połącz bezpośrednio
                            var closingSegment = new ContourSegment(lastPoint, firstPoint);
                            closingSegment.Informacja = informacja;
                            result.Add(closingSegment);
                        }
                    }
                    else
                    {
                        // Brak punktów na linii - połącz bezpośrednio
                        var closingSegment = new ContourSegment(last.End, first.Start);
                        closingSegment.Informacja = informacja;
                        result.Add(closingSegment);
                    }
                }
            }

            // Usuń segmenty o zerowej długości
            result = result.Where(s => Distance(s.Start, s.End) > 0.1).ToList();

            return result;
        }

        private static bool PunktNaLinii(XPoint pt, XLineShape line)
        {
            const double tolerance = 1.0;

            double minX = Math.Min(line.X1, line.X2) - tolerance;
            double maxX = Math.Max(line.X1, line.X2) + tolerance;
            double minY = Math.Min(line.Y1, line.Y2) - tolerance;
            double maxY = Math.Max(line.Y1, line.Y2) + tolerance;

            if (pt.X < minX || pt.X > maxX || pt.Y < minY || pt.Y > maxY)
                return false;

            double dx = line.X2 - line.X1;
            double dy = line.Y2 - line.Y1;
            double len = Math.Sqrt(dx * dx + dy * dy);

            if (len < 0.1) return false;

            double dist = Math.Abs((pt.X - line.X1) * dy - (pt.Y - line.Y1) * dx) / len;
            return dist < tolerance;
        }

        private static ContourSegment ReverseSegment(ContourSegment seg)
        {
            ContourSegment reversed;
            if (seg.Type == SegmentType.Arc && seg.Center != null)
            {
                reversed = new ContourSegment(seg.End, seg.Start, seg.Center, seg.Radius, !seg.CounterClockwise);
            }
            else
            {
                reversed = new ContourSegment(seg.End, seg.Start);
            }
            reversed.Informacja = seg.Informacja;
            return reversed;
        }

        #endregion

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
                var b = s.GetBoundingBox();
                var key = $"{b.X:F2}_{b.Y:F2}_{b.Width:F2}_{b.Height:F2}";
                if (seen.Add(key)) list.Add(s);
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
        public static bool CzyPunktWielokacie(XPoint point, List<XPoint> polygon)
        {
            int i, j;
            bool result = false;
            for (i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if ((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) /
                        (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    result = !result;
                }
            }
            return result;
        }

        public static XPoint ObliczCentroid(List<XPoint> punkty)
        {
            double x = 0, y = 0;
            foreach (var p in punkty)
            {
                x += p.X;
                y += p.Y;
            }
            return new XPoint(x / punkty.Count, y / punkty.Count);
        }

        public static XPoint CalculateCentroid(List<XPoint> pts)
        {
            double cx = 0;
            double cy = 0;

            foreach (var p in pts)
            {
                cx += p.X;
                cy += p.Y;
            }

            return new XPoint(cx / pts.Count, cy / pts.Count);
        }

    }
}