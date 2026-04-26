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
            if (contour == null || contour.Count == 0)
                return new List<List<ContourSegment>> { contour ?? new List<ContourSegment>() };

            Console.WriteLine($"🔍 Podział konturu: {contour.Count} segmentów");
            foreach (var seg in contour)
                Console.WriteLine($"   {seg.Type}: ({seg.Start.X:F1},{seg.Start.Y:F1})->({seg.End.X:F1},{seg.End.Y:F1})");

            // --- Specjalna obsługa KOŁA ---
            bool isCompositeCircle = contour.All(s => s.Type == SegmentType.Arc && s.Center.HasValue);
            if (isCompositeCircle && contour.Count >= 2)
            {
                var firstCenter = contour[0].Center!.Value;
                double radius = contour[0].Radius;
                bool sameCenterAndRadius = contour.All(s =>
                    s.Center.HasValue &&
                    Distance(s.Center.Value, firstCenter) < 1.0 &&
                    Math.Abs(s.Radius - radius) < 1.0);
                bool closedLoop = Distance(contour[0].Start, contour.Last().End) < 1.0;

                if (sameCenterAndRadius && closedLoop)
                {
                    return SplitFullCircle(contour, line, firstCenter, radius);
                }
            }

            // --- Standardowa ścieżka ---
            var left = new List<ContourSegment>();
            var right = new List<ContourSegment>();
            var allIntersectionPoints = new List<XPoint>();

            foreach (var seg in contour)
            {
                var intersections = FindIntersectionsWithLine(seg, line);

                Console.WriteLine($"   Segment: ({seg.Start.X:F1},{seg.Start.Y:F1})->({seg.End.X:F1},{seg.End.Y:F1}) - przecięć: {intersections.Count}");

                foreach (var pt in intersections)
                {
                    if (!allIntersectionPoints.Any(p => Distance(p, pt) < 1.0))
                        allIntersectionPoints.Add(new XPoint(pt.X, pt.Y));
                }

                if (intersections.Count == 0)
                {
                    bool startLeft = PunktPoLewejStronie(seg.Start, line);
                    if (startLeft)
                    {
                        left.Add(CloneSegment(seg));
                        Console.WriteLine($"      -> LEWA (cały)");
                    }
                    else
                    {
                        right.Add(CloneSegment(seg));
                        Console.WriteLine($"      -> PRAWA (cały)");
                    }
                }
                else if (intersections.Count == 1)
                {
                    var pt = intersections[0];

                    if (seg.Type == SegmentType.Arc && seg.Center != null)
                    {
                        var (arc1, arc2) = SplitArcAtPoint(seg, pt);
                        bool arc1Left = PunktPoLewejStronie(GetArcMidPoint(arc1), line);

                        if (arc1Left)
                        {
                            left.Add(arc1); right.Add(arc2);
                            Console.WriteLine($"      -> LEWA: łuk1, PRAWA: łuk2");
                        }
                        else
                        {
                            right.Add(arc1); left.Add(arc2);
                            Console.WriteLine($"      -> PRAWA: łuk1, LEWA: łuk2");
                        }
                    }
                    else
                    {
                        bool startLeft = PunktPoLewejStronie(seg.Start, line);
                        var seg1 = new ContourSegment(seg.Start, pt) { Informacja = seg.Informacja };
                        var seg2 = new ContourSegment(pt, seg.End) { Informacja = seg.Informacja };

                        if (startLeft)
                        {
                            left.Add(seg1); right.Add(seg2);
                            Console.WriteLine($"      -> LEWA: seg1, PRAWA: seg2");
                        }
                        else
                        {
                            right.Add(seg1); left.Add(seg2);
                            Console.WriteLine($"      -> PRAWA: seg1, LEWA: seg2");
                        }
                    }
                }
                else if (intersections.Count == 2)
                {
                    var pt1 = intersections[0];
                    var pt2 = intersections[1];

                    // Sortuj punkty
                    if (seg.Type == SegmentType.Arc && seg.Center != null)
                    {
                        var sorted = SortPointsAlongArc(pt1, pt2, seg);
                        pt1 = sorted[0];
                        pt2 = sorted[1];
                    }
                    else
                    {
                        if (Distance(seg.Start, pt1) > Distance(seg.Start, pt2))
                            (pt1, pt2) = (pt2, pt1);
                    }

                    if (seg.Type == SegmentType.Arc && seg.Center != null)
                    {
                        var arc1 = CreateArcSegment(seg.Start, pt1, seg);
                        var arc2 = CreateArcSegment(pt1, pt2, seg);
                        var arc3 = CreateArcSegment(pt2, seg.End, seg);

                        bool arc1Left = PunktPoLewejStronie(GetArcMidPoint(arc1), line);
                        bool arc2Left = PunktPoLewejStronie(GetArcMidPoint(arc2), line);
                        bool arc3Left = PunktPoLewejStronie(GetArcMidPoint(arc3), line);

                        Console.WriteLine($"      -> arc1 left={arc1Left}, arc2 left={arc2Left}, arc3 left={arc3Left}");

                        if (arc1Left) left.Add(arc1); else right.Add(arc1);
                        if (arc2Left) left.Add(arc2); else right.Add(arc2);
                        if (arc3Left) left.Add(arc3); else right.Add(arc3);
                    }
                    else
                    {
                        bool startLeft = PunktPoLewejStronie(seg.Start, line);
                        var sA = new ContourSegment(seg.Start, pt1) { Informacja = seg.Informacja };
                        var sB = new ContourSegment(pt1, pt2) { Informacja = seg.Informacja };
                        var sC = new ContourSegment(pt2, seg.End) { Informacja = seg.Informacja };

                        if (startLeft)
                        {
                            left.Add(sA); right.Add(sB); left.Add(sC);
                            Console.WriteLine($"      -> LEWA: sA,sC, PRAWA: sB");
                        }
                        else
                        {
                            right.Add(sA); left.Add(sB); right.Add(sC);
                            Console.WriteLine($"      -> PRAWA: sA,sC, LEWA: sB");
                        }
                    }
                }
            }

            Console.WriteLine($"🔍 Punkty przecięcia: {allIntersectionPoints.Count}");
            foreach (var pt in allIntersectionPoints)
                Console.WriteLine($"   ({pt.X:F1}, {pt.Y:F1})");

            Console.WriteLine($"🔍 Lewe segmenty: {left.Count}");
            foreach (var s in left)
                Console.WriteLine($"   {s.Type}: ({s.Start.X:F1},{s.Start.Y:F1})->({s.End.X:F1},{s.End.Y:F1})");

            Console.WriteLine($"🔍 Prawe segmenty: {right.Count}");
            foreach (var s in right)
                Console.WriteLine($"   {s.Type}: ({s.Start.X:F1},{s.Start.Y:F1})->({s.End.X:F1},{s.End.Y:F1})");

            var result = new List<List<ContourSegment>>();

            List<ContourSegment> FinalizeSide(List<ContourSegment> side)
            {
                if (side == null || side.Count == 0) return new List<ContourSegment>();
                side = RemoveZeroLengthSegments(side);

                // Połącz segmenty w ciągłą ścieżkę tylko dla linii (nie dla łuków)
                bool hasArcs = side.Any(s => s.Type == SegmentType.Arc);
                if (!hasArcs)
                {
                    side = ConnectSegmentsIntoClosedPath(side, line, "linia podziału");
                }

                side = MakeSegmentsContiguous(side, 1.0);

                // Dodaj linię podziału
                if (side.Count >= 2 && allIntersectionPoints.Count >= 2)
                {
                    var sortedPoints = allIntersectionPoints
                        .OrderBy(p => Distance(p, new XPoint(line.X1, line.Y1)))
                        .ToList();

                    // Sprawdź czy linia podziału jest potrzebna
                    var first = side[0];
                    var last = side[side.Count - 1];

                    if (Distance(last.End, first.Start) > 1.0)
                    {
                        var dividing = new ContourSegment(sortedPoints[0], sortedPoints[1])
                        {
                            Informacja = "linia podziału",
                            Type = SegmentType.Line
                        };

                        if (!side.Any(s =>
                            (Distance(s.Start, dividing.Start) < 0.1 && Distance(s.End, dividing.End) < 0.1) ||
                            (Distance(s.Start, dividing.End) < 0.1 && Distance(s.End, dividing.Start) < 0.1)))
                        {
                            side.Add(dividing);
                            Console.WriteLine($"   ➕ Dodano linię podziału");
                        }
                    }
                }

                side = RemoveZeroLengthSegments(side);
                return side;
            }

            if (left.Count > 0)
            {
                left = FinalizeSide(left);
                if (left.Count >= 3) result.Add(left);
            }

            if (right.Count > 0)
            {
                right = FinalizeSide(right);
                if (right.Count >= 3) result.Add(right);
            }

            if (result.Count != 2)
            {
                Console.WriteLine($"⚠️ Nieudany podział: {result.Count} regionów, zwracam oryginał");
                return new List<List<ContourSegment>> { contour };
            }

            return result;
        }

        private static List<List<ContourSegment>> SplitFullCircle(
    List<ContourSegment> circle, XLineShape line, XPoint center, double radius)
        {
            var intersections = LineCircleIntersections(
                new XPoint(line.X1, line.Y1),
                new XPoint(line.X2, line.Y2),
                center, radius);

            if (intersections.Count != 2)
                return new List<List<ContourSegment>> { circle };

            Console.WriteLine("🔵 Dzielę pełne koło na 2 półkola");

            var informacja = circle[0].Informacja;
            bool originalCCW = circle[0].CounterClockwise;

            // Sortuj punkty przecięcia
            double a1 = Math.Atan2(intersections[0].Y - center.Y, intersections[0].X - center.X);
            double a2 = Math.Atan2(intersections[1].Y - center.Y, intersections[1].X - center.X);
            if (a1 < 0) a1 += 2 * Math.PI;
            if (a2 < 0) a2 += 2 * Math.PI;

            XPoint firstPt, secondPt;
            if (originalCCW)
            {
                if (a1 < a2) { firstPt = intersections[0]; secondPt = intersections[1]; }
                else { firstPt = intersections[1]; secondPt = intersections[0]; }
            }
            else
            {
                if (a1 > a2) { firstPt = intersections[0]; secondPt = intersections[1]; }
                else { firstPt = intersections[1]; secondPt = intersections[0]; }
            }

            Console.WriteLine($"   Punkty: first=({firstPt.X:F1},{firstPt.Y:F1}), second=({secondPt.X:F1},{secondPt.Y:F1})");

            // Półkole 1: firstPt -> secondPt
            var half1 = new ContourSegment(firstPt, secondPt, center, radius, originalCCW)
            {
                Informacja = informacja,
                Type = SegmentType.Arc
            };

            // Półkole 2: secondPt -> firstPt
            var half2 = new ContourSegment(secondPt, firstPt, center, radius, originalCCW)
            {
                Informacja = informacja,
                Type = SegmentType.Arc
            };

            // Sprawdź które półkole jest po lewej
            var mid1 = GetArcMidPoint(half1);
            var mid2 = GetArcMidPoint(half2);

            bool half1Left = PunktPoLewejStronie(mid1, line);
            bool half2Left = PunktPoLewejStronie(mid2, line);

            Console.WriteLine($"   Half1 mid=({mid1.X:F1},{mid1.Y:F1}) left={half1Left}");
            Console.WriteLine($"   Half2 mid=({mid2.X:F1},{mid2.Y:F1}) left={half2Left}");

            var region1 = new List<ContourSegment>();
            var region2 = new List<ContourSegment>();

            if (half1Left)
            {
                region1.Add(half1);
                region1.Add(new ContourSegment(secondPt, firstPt)
                {
                    Informacja = "linia podziału",
                    Type = SegmentType.Line
                });

                region2.Add(half2);
                region2.Add(new ContourSegment(firstPt, secondPt)
                {
                    Informacja = "linia podziału",
                    Type = SegmentType.Line
                });
            }
            else
            {
                region1.Add(half2);
                region1.Add(new ContourSegment(firstPt, secondPt)
                {
                    Informacja = "linia podziału",
                    Type = SegmentType.Line
                });

                region2.Add(half1);
                region2.Add(new ContourSegment(secondPt, firstPt)
                {
                    Informacja = "linia podziału",
                    Type = SegmentType.Line
                });
            }

            Console.WriteLine($"   Region1: {region1.Count} seg, Region2: {region2.Count} seg");

            return new List<List<ContourSegment>> { region1, region2 };
        }


        private static List<ContourSegment> OrderSegmentsAsPolyline(List<ContourSegment> segments, double tol = 0.5)
        {
            if (segments == null || segments.Count == 0) return new List<ContourSegment>();

            // Normalizacja punktów do klucza (zaokrąglenie)
            string Key(XPoint p) => $"{Math.Round(p.X, 3):F3}_{Math.Round(p.Y, 3):F3}";

            // Grupujemy segmenty wg startKey, ale zachowujemy oryginały (klonujemy przy użyciu CloneSegment)
            var remaining = segments.Select(CloneSegment).ToList();
            var byStart = new Dictionary<string, List<ContourSegment>>();
            var byEnd = new Dictionary<string, List<ContourSegment>>();

            void AddMap(Dictionary<string, List<ContourSegment>> map, string key, ContourSegment s)
            {
                if (!map.TryGetValue(key, out var list)) { list = new List<ContourSegment>(); map[key] = list; }
                list.Add(s);
            }

            foreach (var s in remaining)
            {
                AddMap(byStart, Key(s.Start), s);
                AddMap(byEnd, Key(s.End), s);
            }

            // Znajdź startKey: preferuj węzeł z out > in
            string startKey = null;
            foreach (var k in byStart.Keys.Union(byEnd.Keys))
            {
                int outCount = byStart.TryGetValue(k, out var o) ? o.Count : 0;
                int inCount = byEnd.TryGetValue(k, out var i) ? i.Count : 0;
                if (outCount > inCount) { startKey = k; break; }
            }
            if (startKey == null)
            {
                // fallback - wybierz klucz pierwszego segmentu startu
                startKey = Key(remaining[0].Start);
            }

            var result = new List<ContourSegment>();
            string currentKey = startKey;

            while (true)
            {
                // Jeśli brak segmentów wychodzących z currentKey, spróbuj znaleźć segment kończący w currentKey i odwrócić go
                ContourSegment next = null;
                if (byStart.TryGetValue(currentKey, out var outs) && outs.Count > 0)
                {
                    // deterministycznie wybierz pierwszego po posortowaniu (np. po współrzędnych end)
                    outs.Sort((a, b) =>
                    {
                        var ka = Key(a.End); var kb = Key(b.End);
                        return string.CompareOrdinal(ka, kb);
                    });
                    next = outs[0];
                }
                else if (byEnd.TryGetValue(currentKey, out var ins) && ins.Count > 0)
                {
                    // odwróć pierwszy znaleziony segment
                    ins.Sort((a, b) =>
                    {
                        var ka = Key(a.Start); var kb = Key(b.Start);
                        return string.CompareOrdinal(ka, kb);
                    });
                    next = ReverseSegment(ins[0]);
                }
                else
                {
                    // brak bezpośrednich dopasowań - spróbuj znaleźć segment, którego start jest "blisko" currentKey
                    var near = remaining.FirstOrDefault(s => Distance(s.Start, new XPoint(double.Parse(currentKey.Split('_')[0]), double.Parse(currentKey.Split('_')[1]))) < tol);
                    if (near != null) next = near;
                }

                if (next == null) break;

                // Dodaj next do wyniku i usuń z map
                result.Add(next);

                // Usuń referencje do tej krawędzi z byStart/byEnd/remaining
                var startK = Key(next.Start);
                var endK = Key(next.End);
                if (byStart.TryGetValue(startK, out var l1)) { l1.RemoveAll(x => Distance(x.Start, next.Start) < 1e-6 && Distance(x.End, next.End) < 1e-6); if (l1.Count == 0) byStart.Remove(startK); }
                if (byEnd.TryGetValue(endK, out var l2)) { l2.RemoveAll(x => Distance(x.Start, next.Start) < 1e-6 && Distance(x.End, next.End) < 1e-6); if (l2.Count == 0) byEnd.Remove(endK); }
                remaining.RemoveAll(x => Distance(x.Start, next.Start) < 1e-6 && Distance(x.End, next.End) < 1e-6);

                currentKey = Key(next.End);
            }

            // Jeśli nie wykorzystaliśmy wszystkich segmentów, dopisz pozostałe w uporządkowany, deterministyczny sposób (aby nie zgubić danych)
            if (remaining.Count > 0)
            {
                // dodaj pozostałe posortowane według startKey
                remaining.Sort((a, b) =>
                {
                    var ka = Key(a.Start); var kb = Key(b.Start);
                    return string.CompareOrdinal(ka, kb);
                });
                result.AddRange(remaining);
            }

            // Napraw drobne rozbieżności: snap kolejnych punktów jeśli odległość < tol
            for (int i = 1; i < result.Count; i++)
            {
                var prev = result[i - 1];
                var cur = result[i];
                if (Distance(prev.End, cur.Start) > 1e-6 && Distance(prev.End, cur.Start) <= tol)
                {
                    // ustaw start cur na prev.End
                    if (cur.Type == SegmentType.Arc && cur.Center != null)
                        result[i] = new ContourSegment(prev.End.Clone(), cur.End.Clone(), cur.Center, cur.Radius, cur.CounterClockwise) { Informacja = cur.Informacja, Type = SegmentType.Arc };
                    else
                        result[i] = new ContourSegment(prev.End.Clone(), cur.End.Clone()) { Informacja = cur.Informacja };
                }
            }

            // Upewnij się, że pierwszy.Start odpowiada ostat.End jeśli blisko
            if (result.Count > 1 && Distance(result.Last().End, result.First().Start) <= tol)
            {
                // snap
                var first = result[0];
                var last = result[result.Count - 1];
                if (first.Type == SegmentType.Arc && first.Center != null)
                    result[0] = new ContourSegment(last.End.Clone(), first.End.Clone(), first.Center, first.Radius, first.CounterClockwise) { Informacja = first.Informacja, Type = SegmentType.Arc };
                else
                    result[0] = new ContourSegment(last.End.Clone(), first.End.Clone()) { Informacja = first.Informacja };
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

        private static List<ContourSegment> MakeSegmentsContiguous(List<ContourSegment> segments, double tol = 0.5)
        {
            if (segments == null || segments.Count == 0) return segments;

            // NIE modyfikuj segmentów łuków - zwróć je bez zmian
            bool hasArcs = segments.Any(s => s.Type == SegmentType.Arc);
            if (hasArcs)
            {
                Console.WriteLine("⚠️ Kontur zawiera łuki - pomijam MakeSegmentsContiguous");
                return segments;
            }

            // Klonujemy wejście, żeby nie modyfikować oryginału
            var originals = segments.Select(CloneSegment).ToList();
            int n = originals.Count;

            // Spróbuj z każdym segmentem jako startem (greedy)
            for (int startIdx = 0; startIdx < n; startIdx++)
            {
                var remaining = originals.Select(CloneSegment).ToList();
                var result = new List<ContourSegment>();
                var current = remaining[startIdx];
                remaining.RemoveAt(startIdx);
                result.Add(current);

                while (remaining.Count > 0)
                {
                    int found = remaining.FindIndex(s => Distance(current.End, s.Start) < tol);
                    if (found == -1)
                    {
                        // spróbuj dopasować przez odwrócenie kandydata
                        found = remaining.FindIndex(s => Distance(current.End, s.End) < tol);
                        if (found != -1)
                        {
                            remaining[found] = ReverseSegment(remaining[found]);
                        }
                    }

                    if (found == -1)
                        break;

                    current = remaining[found];
                    remaining.RemoveAt(found);
                    result.Add(current);
                }

                if (result.Count == n)
                {
                    // Wyrównaj drobne różnice do identycznych punktów (dokładne łączenie)
                    for (int i = 1; i < result.Count; i++)
                    {
                        var prev = result[i - 1];
                        var seg = result[i];
                        if (Distance(prev.End, seg.Start) > 1e-6)
                        {
                            // utwórz nowy segment z wyrównanym Start
                            if (seg.Type == SegmentType.Arc && seg.Center != null)
                            {
                                var newSeg = new ContourSegment(
                                    new XPoint(prev.End.X, prev.End.Y),
                                    new XPoint(seg.End.X, seg.End.Y),
                                    seg.Center,
                                    seg.Radius,
                                    seg.CounterClockwise)
                                { Informacja = seg.Informacja, Type = SegmentType.Arc };
                                result[i] = newSeg;
                            }
                            else
                            {
                                var newSeg = new ContourSegment(
                                    new XPoint(prev.End.X, prev.End.Y),
                                    new XPoint(seg.End.X, seg.End.Y))
                                { Informacja = seg.Informacja };
                                result[i] = newSeg;
                            }
                        }
                    }

                    // Dopasuj ostatni -> pierwszy
                    var last = result.Last();
                    var first = result.First();
                    if (Distance(last.End, first.Start) > 1e-6)
                    {
                        // ustaw pierwszy.Start na last.End
                        var f = first;
                        if (f.Type == SegmentType.Arc && f.Center != null)
                        {
                            result[0] = new ContourSegment(
                                new XPoint(last.End.X, last.End.Y),
                                new XPoint(f.End.X, f.End.Y),
                                f.Center,
                                f.Radius,
                                f.CounterClockwise)
                            { Informacja = f.Informacja, Type = SegmentType.Arc };
                        }
                        else
                        {
                            result[0] = new ContourSegment(
                                new XPoint(last.End.X, last.End.Y),
                                new XPoint(f.End.X, f.End.Y))
                            { Informacja = f.Informacja };
                        }
                    }

                    return result;
                }
            }

            // Fallback: spróbuj lokalnie odwrócić segmenty tam gdzie lepiej pasują (naprawy miejscowe)
            var fallback = originals.Select(CloneSegment).ToList();
            for (int i = 1; i < fallback.Count; i++)
            {
                var prev = fallback[i - 1];
                var seg = fallback[i];
                if (Distance(prev.End, seg.Start) > tol && Distance(prev.End, seg.End) < Distance(prev.End, seg.Start))
                {
                    fallback[i] = ReverseSegment(seg);
                }
            }

            // Po próbach lokalnych wyrównaj drobne różnice
            for (int i = 1; i < fallback.Count; i++)
            {
                var prev = fallback[i - 1];
                var seg = fallback[i];
                if (Distance(prev.End, seg.Start) > 1e-6)
                {
                    // wyrównanie punktów
                    if (seg.Type == SegmentType.Arc && seg.Center != null)
                    {
                        fallback[i] = new ContourSegment(
                            new XPoint(prev.End.X, prev.End.Y),
                            new XPoint(seg.End.X, seg.End.Y),
                            seg.Center,
                            seg.Radius,
                            seg.CounterClockwise)
                        { Informacja = seg.Informacja, Type = SegmentType.Arc };
                    }
                    else
                    {
                        fallback[i] = new ContourSegment(
                            new XPoint(prev.End.X, prev.End.Y),
                            new XPoint(seg.End.X, seg.End.Y))
                        { Informacja = seg.Informacja };
                    }
                }
            }

            return fallback;
        }

        private static List<ContourSegment> NormalizeAndCloseContour(List<ContourSegment> segments, XLineShape dividingLine, string informacja)
        {
            if (segments == null || segments.Count == 0) return segments;
            const double tol = 0.5;

            // 1) Dopnij kontur, jeżeli nie jest zamknięty
            if (Distance(segments.Last().End, segments.First().Start) > tol)
            {
                // spróbuj znaleźć dwa punkty na linii podziału (jeżeli istnieją)
                var boundaryPoints = new List<XPoint>();
                foreach (var s in segments)
                {
                    if (PunktNaLinii(s.Start, dividingLine) && !boundaryPoints.Any(p => Distance(p, s.Start) < tol))
                        boundaryPoints.Add(new XPoint(s.Start.X, s.Start.Y));
                    if (PunktNaLinii(s.End, dividingLine) && !boundaryPoints.Any(p => Distance(p, s.End) < tol))
                        boundaryPoints.Add(new XPoint(s.End.X, s.End.Y));
                }

                if (boundaryPoints.Count >= 2)
                {
                    // dodajemy linię podziału łączącą znalezione punkty (w kierunku zamknięcia dodamy dalej)
                    var p1 = boundaryPoints[0];
                    var p2 = boundaryPoints[1];
                    segments.Add(new ContourSegment(p2, p1) { Informacja = informacja });
                }
                // W NormalizeAndCloseContour, po obliczeniu desiredCCW:
                // 3) Wymuś zgodność kierunku łuków z orientacją konturu
                // POMIŃ ten krok dla małych konturów (2-3 segmenty) które mogą być już prawidłowe
                else if (segments.Count > 3)  // Tylko dla konturów z więcej niż 3 segmentami
                {
                    for (int i = 0; i < segments.Count; i++)
                    {
                        var s = segments[i];
                        if (s.Type == SegmentType.Arc && s.Center != null)
                        {
                            if (s.CounterClockwise != true)
                            {
                                var rev = ReverseSegment(s);
                                rev.Informacja = s.Informacja;
                                rev.Type = s.Type;
                                segments[i] = rev;
                            }
                        }
                    }
                }
                else
                {
                    // dopięcie bezpośrednie
                    segments.Add(new ContourSegment(segments.Last().End, segments.First().Start) { Informacja = informacja });
                }

            }

            // Pomocnik: próbkowanie punktów z segmentu (dla obliczenia pola)
            List<XPoint> SampleSegmentPoints(ContourSegment seg, int samples = 12)
            {
                var pts = new List<XPoint>();
                if (seg.Type == SegmentType.Line)
                {
                    pts.Add(seg.Start.Clone());
                    pts.Add(new XPoint((seg.Start.X + seg.End.X) / 2.0, (seg.Start.Y + seg.End.Y) / 2.0));
                    // nie dodajemy end żeby uniknąć duplikatów przy łączeniu segmentów
                }
                else if (seg.Type == SegmentType.Arc && seg.Center != null)
                {
                    var c = seg.Center.Value;
                    double r = seg.Radius;
                    double a0 = Math.Atan2(seg.Start.Y - c.Y, seg.Start.X - c.X);
                    double a1 = Math.Atan2(seg.End.Y - c.Y, seg.End.X - c.X);

                    // ustal rzeczywistą różnicę kąta zgodnie z kierunkiem
                    if (seg.CounterClockwise)
                    {
                        if (a1 < a0) a1 += 2 * Math.PI;
                    }
                    else
                    {
                        if (a0 < a1) a0 += 2 * Math.PI;
                    }

                    double step = (a1 - a0) / Math.Max(1, samples);
                    for (int i = 0; i < samples; i++)
                    {
                        double ang = a0 + step * i;
                        pts.Add(new XPoint(c.X + r * Math.Cos(ang), c.Y + r * Math.Sin(ang)));
                    }
                    // nie dodajemy końcowego punktu żeby nie powielać startu następnego segmentu
                }
                return pts;
            }

            // 2) Zbierz próbki z całego konturu i oblicz pole (orientację)
            var sampled = new List<XPoint>();
            foreach (var s in segments)
            {
                var spts = SampleSegmentPoints(s, 16);
                sampled.AddRange(spts);
            }

            // Jeśli za mało próbek — fallback na midpoints
            if (sampled.Count < 3)
            {
                sampled.Clear();
                foreach (var s in segments)
                {
                    sampled.Add(s.Type == SegmentType.Line ? new XPoint((s.Start.X + s.End.X) / 2.0, (s.Start.Y + s.End.Y) / 2.0) : GetArcMidPoint(s));
                }
            }

            // oblicz pole (signed area)
            double area = 0.0;
            for (int i = 0; i < sampled.Count; i++)
            {
                var a = sampled[i];
                var b = sampled[(i + 1) % sampled.Count];
                area += (a.X * b.Y - b.X * a.Y);
            }
            bool desiredCCW = area > 0.0; // true = CCW

            // 3) Wymuś zgodność kierunku łuków z orientacją konturu
            for (int i = 0; i < segments.Count; i++)
            {
                var s = segments[i];
                if (s.Type == SegmentType.Arc && s.Center != null)
                {
                    if (s.CounterClockwise != desiredCCW)
                    {
                        // ReverseSegment zwraca sklonowany segment z odwróconym kierunkiem
                        var rev = ReverseSegment(s);
                        rev.Informacja = s.Informacja;
                        rev.Type = s.Type;
                        segments[i] = rev;
                    }
                }
            }

            // 4) Po ewentualnym odwróceniu sprawdź ponownie czy kontur jest zamknięty (małe przesunięcia)
            if (Distance(segments.Last().End, segments.First().Start) > tol)
            {
                // dodajemy linię zamykającą
                segments.Add(new ContourSegment(segments.Last().End, segments.First().Start) { Informacja = informacja });
            }

            // usuń krótkie segmenty
            segments = RemoveZeroLengthSegments(segments);

            // upewnij się, że typy i Informacja są zachowane
            foreach (var s in segments)
                if (s.Type == 0 && s.Center != null)
                    s.Type = SegmentType.Arc;

            return segments;
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