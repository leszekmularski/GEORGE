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
                            var (arcCenterX, arcCenterY, startAngle, endAngle) = rtr.CalculateArcGeometry();
                            var arcCenter = new XPoint(arcCenterX, arcCenterY);

                            double radius = rtr.Radius;

                            // -------------------------
                            // helpery
                            // -------------------------
                            double Angle(XPoint p)
                                => Math.Atan2(-(p.Y - arcCenter.Y), p.X - arcCenter.X);

                            double Normalize(double a)
                            {
                                if (a < 0) a += 2 * Math.PI;
                                return a;
                            }

                            bool InArcRange(double a)
                            {
                                double s = Normalize(startAngle);
                                double e = Normalize(endAngle);
                                a = Normalize(a);

                                return s > e ? (a >= s || a <= e) : (a >= s && a <= e);
                            }

                            // -------------------------
                            // geometria punktów
                            // -------------------------
                            double a1 = Normalize(Angle(p));
                            double a2 = Normalize(Angle(next));

                            bool onCircle =
                                Math.Abs(Distance(p, arcCenter) - radius) < 5.0 &&
                                Math.Abs(Distance(next, arcCenter) - radius) < 5.0;

                            bool onArc =
                                InArcRange(a1) &&
                                InArcRange(a2);

                            bool validArcSegment = onCircle && onArc;

                            Console.WriteLine(
                                $"🔍 ({p.X},{p.Y}) -> ({next.X},{next.Y}) | arc={validArcSegment}"
                            );

                            // -------------------------
                            // ARC
                            // -------------------------
                            if (validArcSegment)
                            {
                                var segment = new ContourSegment(
                                    p,
                                    next,
                                    arcCenter,
                                    radius,
                                    true
                                );

                                segment.Informacja = ramaInfo;
                                return segment;
                            }

                            // -------------------------
                            // LINIA
                            // -------------------------
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

                        //Console.WriteLine("==== KONTUR PRZED GROUPBY ====");

                        //int i = 0;
                        //foreach (var s in r.Kontur)
                        //{
                        //    Console.WriteLine(
                        //        $"S{i++}: ({Math.Round(s.Start.X, 2)},{Math.Round(s.Start.Y, 2)}) -> ({Math.Round(s.End.X, 2)},{Math.Round(s.End.Y, 2)})"
                        //    );
                        //}

                        // USUŃ SEGMENTY O ZEROWEJ DŁUGOŚCI Z KONTURU
                        r.Kontur = r.Kontur
                            .Where(s => !CzySegmentZerowejDlugosci(s))
                            .ToList();

                        // Przetwarzanie konturu - usuwanie duplikatów i dodawanie informacji
                        var unikalneSegmenty = r.Kontur
                            .GroupBy(s =>
                            {
                                double sx = Math.Round(s.Start.X, 2);
                                double sy = Math.Round(s.Start.Y, 2);
                                double ex = Math.Round(s.End.X, 2);
                                double ey = Math.Round(s.End.Y, 2);

                                bool swap = (sx > ex) || (sx == ex && sy > ey);

                                return new
                                {
                                    Type = s.Type,
                                    StartX = swap ? ex : sx,
                                    StartY = swap ? ey : sy,
                                    EndX = swap ? sx : ex,
                                    EndY = swap ? sy : ey,
                                    CenterX = s.Type == SegmentType.Arc && s.Center.HasValue ? Math.Round(s.Center.Value.X, 2) : double.NaN,
                                    CenterY = s.Type == SegmentType.Arc && s.Center.HasValue ? Math.Round(s.Center.Value.Y, 2) : double.NaN,
                                    Radius = s.Type == SegmentType.Arc ? Math.Round(s.Radius, 2) : double.NaN,
                                    CounterClockwise = s.Type == SegmentType.Arc ? s.CounterClockwise : false
                                };
                            })
                            .Select(g => g.First())
                            .ToList();

                        //Console.WriteLine("==== KONTUR PO GROUPBY ====");

                        //i = 0;
                        //foreach (var s in unikalneSegmenty)
                        //{
                        //    Console.WriteLine(
                        //        $"S{i++}: ({Math.Round(s.Start.X, 2)},{Math.Round(s.Start.Y, 2)}) -> ({Math.Round(s.End.X, 2)},{Math.Round(s.End.Y, 2)})"
                        //    );
                        //}

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

                            nowySegment.Informacja = ramaInfo;
                            nowyKontur.Add(nowySegment);
                        }

                        r.Kontur = nowyKontur;

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
                            r.Kontur = r.Kontur
                                .Where(s => !CzySegmentZerowejDlugosci(s))
                                .ToList();

                            // Przetwarzanie konturu - usuwanie duplikatów
                            r.Kontur = r.Kontur
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
                                .Select(g =>
                                {
                                    var originalSegment = g.First();

                                    var segment = new ContourSegment(
                                        new XPoint(originalSegment.Start.X, originalSegment.Start.Y),
                                        new XPoint(originalSegment.End.X, originalSegment.End.Y),
                                        originalSegment.Center.HasValue
                                            ? new XPoint(originalSegment.Center.Value.X, originalSegment.Center.Value.Y)
                                            : (XPoint?)null,
                                        originalSegment.Radius,
                                        originalSegment.CounterClockwise
                                    );

                                    segment.Informacja = "kontur skrzydłowy";
                                    return segment;
                                })
                                .ToList();

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

        private static List<ShapeRegion> PodzielRegionRekurencyjnie(
        ShapeRegion region,
        List<XLineShape> lines,
        string idMaster,
        bool rama)
        {
            var wynik = new List<ShapeRegion> { region };

            int lineIndex = 0;

            foreach (var line in lines)
            {
                var next = new List<ShapeRegion>();

                foreach (var r in wynik)
                {
                    // 1️⃣ podział geometrii (polygon)
                    var splitPolygons = PodzielPolygonPoLinii(r.Wierzcholki, line);

                    // 2️⃣ podział konturu (linia + łuk)
                    var splitKontury = PodzielKonturPoLinii(r.Kontur, line);

                    // 🔥 jeśli nie ma realnego podziału → zostaw region
                    if (splitPolygons.Count <= 1 || splitKontury.Count == 0)
                    {
                        next.Add(r);
                        continue;
                    }

                    // 🔥 tworzymy regiony 1:1 (bez zgadywania dopasowania)
                    for (int i = 0; i < splitPolygons.Count; i++)
                    {
                        var poly = splitPolygons[i];

                        // ⬇️ bez heurystyk — tylko bezpieczne indeksowanie
                        var kontur = (i < splitKontury.Count)
                            ? splitKontury[i]
                            : splitKontury.Last();

                        next.Add(new ShapeRegion
                        {
                            Wierzcholki = poly,
                            Kontur = kontur,

                            TypKsztaltu = r.TypKsztaltu,
                            LinieDzielace = r.LinieDzielace.Concat(new[] { line }).ToList(),

                            IdMaster = idMaster,
                            Rama = rama,

                            Id = $"{r.Id}|L{lineIndex}|{i}|{Guid.NewGuid()}",
                            TypLiniiDzielacej = r.TypLiniiDzielacej
                        });
                    }
                }

                wynik = next;
                lineIndex++;
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
                            // var kontur = i < splitFullKontur.Count ? splitFullKontur[i] : splitFullKontur.Last();
                            var rawKontur = i < splitFullKontur.Count
                             ? splitFullKontur[i]
                             : splitFullKontur.Last();

                            // 🔥 KLUCZOWA POPRAWKA
                            var kontur = ScalArcSegmenty(rawKontur);

                            string newId = $"{rootId}|L{indexLinii}|C{indexChild}";

                            next.Add(new ShapeRegion
                            {
                                Wierzcholki = poly,
                                Kontur = kontur,//BuildContourFromPolygon(poly, r.Kontur),
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

        private static List<ContourSegment> ScalArcSegmenty(List<ContourSegment> segments)
        {
            var result = new List<ContourSegment>();

            var arcs = segments
                .Where(s => s.Type == SegmentType.Arc && s.Center != null)
                .ToList();

            var lines = segments
                .Where(s => s.Type == SegmentType.Line)
                .ToList();

            result.AddRange(lines);

            var grouped = arcs.GroupBy(a => new
            {
                a.Center!.Value.X,
                a.Center!.Value.Y,
                Radius = Math.Round(a.Radius, 3),
                a.CounterClockwise
            });

            foreach (var group in grouped)
            {
                var center = new XPoint(group.Key.X, group.Key.Y);

                var ordered = group
                    .OrderBy(a => GetAngle(center, a.Start))
                    .ToList();

                var merged = new List<ContourSegment>();
                var current = ordered[0];

                for (int i = 1; i < ordered.Count; i++)
                {
                    var next = ordered[i];

                    if (AreContinuous(current, next, center))
                    {
                        // łączymy
                        current = new ContourSegment(
                            current.Start,
                            next.End,
                            center,
                            current.Radius,
                            current.CounterClockwise
                        );
                    }
                    else
                    {
                        merged.Add(current);
                        current = next;
                    }
                }

                merged.Add(current);
                result.AddRange(merged);
            }

            return result;
        }

        private static double GetAngle(XPoint center, XPoint p)
        {
            return Math.Atan2(p.Y - center.Y, p.X - center.X);
        }

        private static bool AreContinuous(ContourSegment a, ContourSegment b, XPoint center)
        {
            var aEndAngle = GetAngle(center, a.End);
            var bStartAngle = GetAngle(center, b.Start);

            const double eps = 0.0001;

            return Math.Abs(aEndAngle - bStartAngle) < eps;
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







        public static List<List<ContourSegment>> PodzielKonturPoLinii(
        List<ContourSegment> contour,
        XLineShape line)
        {
            var left = new List<ContourSegment>();
            var right = new List<ContourSegment>();

            foreach (var seg in contour)
            {
                // Sprawdź przecięcia segmentu z linią
                var intersections = FindIntersectionsWithLine(seg, line);

                if (intersections.Count == 0)
                {
                    // Brak przecięć - cały segment po jednej stronie
                    bool startLeft = PunktPoLewejStronie(seg.Start, line);
                    if (startLeft)
                        left.Add(seg);
                    else
                        right.Add(seg);
                }
                else if (intersections.Count == 1)
                {
                    // Jedno przecięcie - segment jest przecięty na dwie części
                    var pt = intersections[0];

                    if (seg.Type == SegmentType.Arc && seg.Center != null)
                    {
                        // 🔥 Dzielenie łuku na dwa półłuki
                        var (arc1, arc2) = SplitArcAtPoint(seg, pt);

                        // Określ, która część jest po lewej stronie linii
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
                        var seg2 = new ContourSegment(pt, seg.End);

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
                    // 🔥 Dwa przecięcia - linia przechodzi przez łuk w dwóch miejscach
                    // (np. gdy łuk jest "większy" niż półokręgu)

                    var pt1 = intersections[0];
                    var pt2 = intersections[1];

                    // Sortuj punkty przecięcia wzdłuż łuku
                    var sorted = SortPointsAlongArc(pt1, pt2, seg);
                    pt1 = sorted[0];
                    pt2 = sorted[1];

                    if (seg.Type == SegmentType.Arc && seg.Center != null)
                    {
                        // Dzielimy łuk na TRZY części
                        var arc1 = CreateArcSegment(seg.Start, pt1, seg);
                        var arc2 = CreateArcSegment(pt1, pt2, seg);
                        var arc3 = CreateArcSegment(pt2, seg.End, seg);

                        // Określ, które części są po lewej stronie
                        bool arc1Left = PunktPoLewejStronie(arc1.MidPoint(), line);
                        bool arc2Left = PunktPoLewejStronie(arc2.MidPoint(), line);
                        bool arc3Left = PunktPoLewejStronie(arc3.MidPoint(), line);

                        if (arc1Left) left.Add(arc1); else right.Add(arc1);
                        if (arc2Left) left.Add(arc2); else right.Add(arc2);
                        if (arc3Left) left.Add(arc3); else right.Add(arc3);
                    }
                }
            }

            // Usuń segmenty o zerowej długości i połącz ciągłe
            left = CleanupSegments(left);
            right = CleanupSegments(right);

            var result = new List<List<ContourSegment>>();

            if (left.Count > 0)
                result.Add(ConnectSegments(left));

            if (right.Count > 0)
                result.Add(ConnectSegments(right));

            return result;
        }

        private static XPoint GetArcMidPoint(ContourSegment seg)
        {
            if (seg.Type == SegmentType.Line)
            {
                return new XPoint(
                    (seg.Start.X + seg.End.X) / 2,
                    (seg.Start.Y + seg.End.Y) / 2
                );
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

            return new XPoint(
                (seg.Start.X + seg.End.X) / 2,
                (seg.Start.Y + seg.End.Y) / 2
            );
        }

        /// <summary>
        /// Metoda rozszerzająca dla ContourSegment - zwraca punkt środkowy segmentu
        /// </summary>
        public static XPoint MidPoint(this ContourSegment seg)
        {
            if (seg.Type == SegmentType.Line)
            {
                return new XPoint(
                    (seg.Start.X + seg.End.X) / 2,
                    (seg.Start.Y + seg.End.Y) / 2
                );
            }
            else if (seg.Type == SegmentType.Arc && seg.Center != null)
            {
                var center = seg.Center.Value;

                double startAngle = Math.Atan2(seg.Start.Y - center.Y, seg.Start.X - center.X);
                double endAngle = Math.Atan2(seg.End.Y - center.Y, seg.End.X - center.X);

                double midAngle;

                if (seg.CounterClockwise)
                {
                    // Dla CCW: kąt rośnie
                    if (endAngle < startAngle)
                        endAngle += 2 * Math.PI;
                    midAngle = (startAngle + endAngle) / 2;
                }
                else
                {
                    // Dla CW: kąt maleje
                    if (startAngle < endAngle)
                        startAngle += 2 * Math.PI;
                    midAngle = (startAngle + endAngle) / 2;
                }

                // Normalizuj do [-π, π] lub [0, 2π]
                midAngle = Math.Atan2(Math.Sin(midAngle), Math.Cos(midAngle));

                return new XPoint(
                    center.X + seg.Radius * Math.Cos(midAngle),
                    center.Y + seg.Radius * Math.Sin(midAngle)
                );
            }

            // Fallback - środek geometryczny
            return new XPoint(
                (seg.Start.X + seg.End.X) / 2,
                (seg.Start.Y + seg.End.Y) / 2
            );
        }

        /// <summary>
        /// Znajduje wszystkie przecięcia segmentu (linii lub łuku) z linią
        /// </summary>
        private static List<XPoint> FindIntersectionsWithLine(ContourSegment seg, XLineShape line)
        {
            var intersections = new List<XPoint>();

            if (seg.Type == SegmentType.Line)
            {
                if (ObliczPrzeciecie(seg.Start, seg.End, line, out var pt))
                {
                    // Sprawdź, czy punkt przecięcia leży na segmencie
                    if (IsPointOnSegment(pt, seg.Start, seg.End))
                        intersections.Add(pt);
                }
            }
            else if (seg.Type == SegmentType.Arc && seg.Center != null)
            {
                // Przecięcie linii z okręgiem
                var circleIntersections = LineCircleIntersections(
                    new XPoint(line.X1, line.Y1),
                    new XPoint(line.X2, line.Y2),
                    seg.Center.Value,
                    seg.Radius
                );

                foreach (var pt in circleIntersections)
                {
                    // Sprawdź, czy punkt leży na łuku (w zakresie kątowym)
                    if (IsPointOnArc(pt, seg))
                        intersections.Add(pt);
                }
            }

            return intersections;
        }

        /// <summary>
        /// Dzieli łuk na dwa w punkcie przecięcia
        /// </summary>
        private static (ContourSegment, ContourSegment) SplitArcAtPoint(ContourSegment arc, XPoint splitPoint)
        {
            return (
                CreateArcSegment(arc.Start, splitPoint, arc),
                CreateArcSegment(splitPoint, arc.End, arc)
            );
        }

        /// <summary>
        /// Tworzy nowy segment łuku między dwoma punktami
        /// </summary>
        private static ContourSegment CreateArcSegment(XPoint start, XPoint end, ContourSegment originalArc)
        {
            return new ContourSegment(
                start,
                end,
                originalArc.Center,
                originalArc.Radius,
                originalArc.CounterClockwise
            );
        }

        /// <summary>
        /// Oblicza przecięcia linii z okręgiem
        /// </summary>
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

            if (discriminant < 0)
                return result; // Brak przecięć

            discriminant = Math.Sqrt(discriminant);

            double t1 = (-b - discriminant) / (2 * a);
            double t2 = (-b + discriminant) / (2 * a);

            if (t1 >= 0 && t1 <= 1)
            {
                result.Add(new XPoint(p1.X + t1 * dx, p1.Y + t1 * dy));
            }

            if (t2 >= 0 && t2 <= 1 && Math.Abs(t1 - t2) > 0.001)
            {
                result.Add(new XPoint(p1.X + t2 * dx, p1.Y + t2 * dy));
            }

            return result;
        }

        /// <summary>
        /// Sprawdza, czy punkt leży na segmencie linii
        /// </summary>
        private static bool IsPointOnSegment(XPoint pt, XPoint segStart, XPoint segEnd)
        {
            const double tolerance = 0.01;

            double minX = Math.Min(segStart.X, segEnd.X) - tolerance;
            double maxX = Math.Max(segStart.X, segEnd.X) + tolerance;
            double minY = Math.Min(segStart.Y, segEnd.Y) - tolerance;
            double maxY = Math.Max(segStart.Y, segEnd.Y) + tolerance;

            return pt.X >= minX && pt.X <= maxX && pt.Y >= minY && pt.Y <= maxY;
        }

        /// <summary>
        /// Sprawdza, czy punkt leży na łuku (w zakresie kątowym)
        /// </summary>
        private static bool IsPointOnArc(XPoint pt, ContourSegment arc)
        {
            if (arc.Center == null) return false;

            var center = arc.Center.Value;

            double angle = Math.Atan2(pt.Y - center.Y, pt.X - center.X);
            double startAngle = Math.Atan2(arc.Start.Y - center.Y, arc.Start.X - center.X);
            double endAngle = Math.Atan2(arc.End.Y - center.Y, arc.End.X - center.X);

            angle = NormalizeAngle(angle);
            startAngle = NormalizeAngle(startAngle);
            endAngle = NormalizeAngle(endAngle);

            return IsAngleInArcRange(angle, startAngle, endAngle, arc.CounterClockwise);
        }

        /// <summary>
        /// Sortuje dwa punkty wzdłuż łuku
        /// </summary>
        private static List<XPoint> SortPointsAlongArc(XPoint pt1, XPoint pt2, ContourSegment arc)
        {
            if (arc.Center == null) return new List<XPoint> { pt1, pt2 };

            var center = arc.Center.Value;

            double angle1 = Math.Atan2(pt1.Y - center.Y, pt1.X - center.X);
            double angle2 = Math.Atan2(pt2.Y - center.Y, pt2.X - center.X);

            double startAngle = Math.Atan2(arc.Start.Y - center.Y, arc.Start.X - center.X);

            // Normalizuj kąty względem startu łuku
            angle1 = NormalizeAngleRelative(angle1, startAngle);
            angle2 = NormalizeAngleRelative(angle2, startAngle);

            return angle1 < angle2
                ? new List<XPoint> { pt1, pt2 }
                : new List<XPoint> { pt2, pt1 };
        }

        private static double NormalizeAngle(double angle)
        {
            while (angle < 0) angle += 2 * Math.PI;
            while (angle >= 2 * Math.PI) angle -= 2 * Math.PI;
            return angle;
        }

        private static double NormalizeAngleRelative(double angle, double reference)
        {
            angle = NormalizeAngle(angle);
            reference = NormalizeAngle(reference);

            double diff = angle - reference;
            if (diff < 0) diff += 2 * Math.PI;

            return diff;
        }

        private static bool IsAngleInArcRange(double angle, double startAngle, double endAngle, bool isCCW)
        {
            const double tolerance = 0.001;

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

  
        private static List<ContourSegment> CleanupSegments(List<ContourSegment> input)
        {
            return input
                .Where(s => Distance(s.Start, s.End) > 0.01)
                .ToList();
        }

        private static List<ContourSegment> ConnectSegments(List<ContourSegment> segments)
        {
            var result = new List<ContourSegment>();

            var used = new HashSet<int>();

            for (int i = 0; i < segments.Count; i++)
            {
                if (used.Contains(i)) continue;

                var current = segments[i];
                used.Add(i);

                bool extended = true;

                while (extended)
                {
                    extended = false;

                    for (int j = 0; j < segments.Count; j++)
                    {
                        if (used.Contains(j)) continue;

                        var s = segments[j];

                        if (Distance(current.End, s.Start) < 0.5)
                        {
                            current = new ContourSegment(current.Start, s.End);
                            used.Add(j);
                            extended = true;
                        }
                    }
                }

                result.Add(current);
            }

            return result;
        }
        private static List<List<XPoint>> PodzielPolygonPoLinii(List<XPoint> poly, XLineShape line)
        {
            var left = new List<XPoint>();
            var right = new List<XPoint>();

            for (int i = 0; i < poly.Count; i++)
            {
                var curr = poly[i];
                var nxt = poly[(i + 1) % poly.Count];

                bool cl = PunktPoLewejStronie(curr, line);
                bool nl = PunktPoLewejStronie(nxt, line);

                if (cl) left.Add(curr); else right.Add(curr);

                if (cl != nl && ObliczPrzeciecie(curr, nxt, line, out var pt))
                {
                    left.Add(pt);
                    right.Add(pt);
                }
            }

            var res = new List<List<XPoint>>();
            if (left.Count >= 3) res.Add(left);
            if (right.Count >= 3) res.Add(right);
            return res;
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