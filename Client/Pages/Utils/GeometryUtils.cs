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
                    XCircleShape circ => GenerateCircleVertices(circ.X, circ.Y, circ.Radius, 8),// liczna 8 - podział koła na 8 segmentów
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

                            // dzielimy okrąg na pół: góra / dół
                            bool isTopArc = p.Y <= circ.Y && next.Y <= circ.Y;

                            return new ContourSegment(
                                p,
                                next,
                                center,
                                circ.Radius,
                                false
                            );
                        }

                        // =========================
                        // PROSTOKĄT Z ŁUKIEM NA GÓRZE
                        // =========================
                        else if (shape is XRoundedTopRectangleShape rtr)
                        {
                            double arcStartY = rtr.Y + rtr.ArcHeight;

                            if (p.Y <= arcStartY && next.Y <= arcStartY)
                            {
                                var (arcCenterX, arcCenterY, _, _) = rtr.CalculateArcGeometry();

                                return new ContourSegment(
                                    p,
                                    next,
                                    new XPoint(arcCenterX, arcCenterY),
                                    rtr.Radius,
                                    false
                                );
                            }

                            return new ContourSegment(p, next);
                        }

                        // =========================
                        // PROSTOKĄT ZAOKRĄGLONY
                        // =========================
                        else if (shape is XRoundedRectangleShape rr)
                        {
                            double r = rr.Radius;

                            bool isCorner =
                                Math.Abs(p.X - next.X) < r ||
                                Math.Abs(p.Y - next.Y) < r;

                            if (isCorner)
                            {
                                double centerX = (p.X + next.X) / 2;
                                double centerY = (p.Y + next.Y) / 2;

                                return new ContourSegment(
                                    p,
                                    next,
                                    new XPoint(centerX, centerY),
                                    r,
                                    false
                                );
                            }

                            return new ContourSegment(p, next);
                        }

                        // =========================
                        // ZAOKRĄGLONY LEWY BOK
                        // =========================
                        else if (shape is XRoundedRectangleShapeLeft rrl)
                        {
                            double r = rrl.Radius;

                            bool isLeftArc =
                                p.X <= rrl.X + r &&
                                next.X <= rrl.X + r;

                            if (isLeftArc)
                            {
                                double centerX = rrl.X + r;
                                double centerY = (p.Y + next.Y) / 2;

                                return new ContourSegment(
                                    p,
                                    next,
                                    new XPoint(centerX, centerY),
                                    r,
                                    false
                                );
                            }

                            return new ContourSegment(p, next);
                        }

                        // =========================
                        // ZAOKRĄGLONY PRAWY BOK
                        // =========================
                        else if (shape is XRoundedRectangleShapeRight rrr)
                        {
                            double r = rrr.Radius;

                            bool isRightArc =
                                p.X >= rrr.X + rrr.Width - r &&
                                next.X >= rrr.X + rrr.Width - r;

                            if (isRightArc)
                            {
                                double centerX = rrr.X + rrr.Width - r;
                                double centerY = (p.Y + next.Y) / 2;

                                return new ContourSegment(
                                    p,
                                    next,
                                    new XPoint(centerX, centerY),
                                    r,
                                    false
                                );
                            }

                            return new ContourSegment(p, next);
                        }

                        // =========================
                        // DOMYŚLNA LINIA
                        // =========================
                        else
                        {
                            return new ContourSegment(p, next);
                        }

                    }).ToList(),

                    TypKsztaltu = typ,
                    TypLiniiDzielacej = typLinii,
                    Id = id,
                    IdMaster = id,
                    Rama = rama
                };

                if (rama)
                {
                    var linieDzielace = shapes
                    .OfType<XLineShape>()
                    .Where(l => l.DualRama)
                    .ToList();

                    // initial.Id = "R-" + initial.Id;

                    var podzielone = PodzielRegionRekurencyjnie(initial, linieDzielace, id, rama);

                    Console.WriteLine($"🔲 Generowanie regionów PodzielRegionRekurencyjnie podzielone.Count: {podzielone.Count} id:{id}");

                    int idCounter = 0;

                    foreach (var r in podzielone)
                    {
                        r.Wierzcholki = r.Wierzcholki
                            .GroupBy(p => new { X = Math.Round(p.X, 2), Y = Math.Round(p.Y, 2) })
                            .Select(g => g.First())
                            .ToList();
                        r.Kontur = r.Kontur
                            .GroupBy(s => new
                            {
                                StartX = Math.Round(s.Start.X, 2),
                                StartY = Math.Round(s.Start.Y, 2),
                                EndX = Math.Round(s.End.X, 2),
                                EndY = Math.Round(s.End.Y, 2)
                            })
                            .Select(g => g.First())
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

                        // **UWAGA**: NIE NADPISUJEMY Id — zachowujemy oryginalne Id
                    }

                    regions.AddRange(podzielone);
                }
                else
                {
                    var linieDzielace = shapes
                        .OfType<XLineShape>()
                        .ToList();

                    //initial.Id = initial.Id;
                    Console.WriteLine($"🔲 Generowanie regionów bez ramy dla shape id: {initial.Id} id: {id}");
                    //var podzielone = PodzielRegionRekurencyjnie(initial, linieDzielace, id, rama);
                    var podzielone = PodzielRegionRekurencyjnieDeterministycznie(initial, linieDzielace, id, rama);

                    Console.WriteLine($"🔲 Generowanie regionów PodzielRegionRekurencyjnieDeterministycznie podzielone.Count: {podzielone.Count}");

                    foreach (var r in podzielone)
                    {
                        r.Wierzcholki = r.Wierzcholki
                       .GroupBy(p => new { X = Math.Round(p.X, 2), Y = Math.Round(p.Y, 2) })
                       .Select(g => g.First())
                       .ToList();

                        r.Kontur = r.Kontur
                            .GroupBy(s => new
                            {
                                StartX = Math.Round(s.Start.X, 2),
                                StartY = Math.Round(s.Start.Y, 2),
                                EndX = Math.Round(s.End.X, 2),
                                EndY = Math.Round(s.End.Y, 2)
                            })
                            .Select(g => g.First())
                            .ToList();

                        r.RozpoznajTyp(r.TypKsztaltu);

                        Console.WriteLine($"🔹 Region po podziale: {r.TypKsztaltu} z {r.Wierzcholki.Count} wierzchołkami. - SKRZYDŁO");

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

                        // **UWAGA**: NIE NADPISUJEMY Id — zachowujemy oryginalne Id
                    }

                    regions.AddRange(podzielone);
                }

            }

            await Task.Delay(1);

            return regions;
        }
        public static List<ShapeRegion> SkalujSkrzydlaDoRamy(
        List<ShapeRegion> stareSkrzydla,
        List<ShapeRegion> rama,
        int nowaSzerokosc,
        int nowaWysokosc)
        {
            if (stareSkrzydla == null || !stareSkrzydla.Any() ||
                rama == null || !rama.Any())
                return new List<ShapeRegion>();

            // Wyznacz bounding box ramy
            double minXRama = rama.Min(r => r.Wierzcholki.Min(p => p.X));
            double minYRama = rama.Min(r => r.Wierzcholki.Min(p => p.Y));
            double maxXRama = rama.Max(r => r.Wierzcholki.Max(p => p.X));
            double maxYRama = rama.Max(r => r.Wierzcholki.Max(p => p.Y));

            double szerRamy = maxXRama - minXRama;
            double wysRamy = maxYRama - minYRama;

            // Wyznacz bounding box skrzydeł
            double minXSkrzydla = stareSkrzydla.Min(r => r.Wierzcholki.Min(p => p.X));
            double minYSkrzydla = stareSkrzydla.Min(r => r.Wierzcholki.Min(p => p.Y));
            double maxXSkrzydla = stareSkrzydla.Max(r => r.Wierzcholki.Max(p => p.X));
            double maxYSkrzydla = stareSkrzydla.Max(r => r.Wierzcholki.Max(p => p.Y));

            double szerSkrzydel = maxXSkrzydla - minXSkrzydla;
            double wysSkrzydel = maxYSkrzydla - minYSkrzydla;

            // Skala względem ramy
            double scaleX = szerRamy / szerSkrzydel;
            double scaleY = wysRamy / wysSkrzydel;

            // Skaluj każde skrzydło
            var noweSkrzydla = new List<ShapeRegion>();
            foreach (var skrzydlo in stareSkrzydla)
            {
                var noweWierzcholki = skrzydlo.Wierzcholki
                    .Select(p => new XPoint(
                        minXRama + (p.X - minXSkrzydla) * scaleX,
                        minYRama + (p.Y - minYSkrzydla) * scaleY
                    ))
                    .ToList();

                var nowyKontur = skrzydlo.Kontur
                    .Select(s =>
                    {
                        var start = new XPoint(
                            minXRama + (s.Start.X - minXSkrzydla) * scaleX,
                            minYRama + (s.Start.Y - minYSkrzydla) * scaleY
                        );
                        var end = new XPoint(
                            minXRama + (s.End.X - minXSkrzydla) * scaleX,
                            minYRama + (s.End.Y - minYSkrzydla) * scaleY
                        );

                        if (s.Type == SegmentType.Line)
                        {
                            return new ContourSegment(start, end);
                        }
                        else // łuk
                        {
                            XPoint center = new();
                            if (s.Center != null)
                            {
                                center = new XPoint(
                                    minXRama + (s.Center.Value.X - minXSkrzydla) * scaleX,
                                    minYRama + (s.Center.Value.Y - minYSkrzydla) * scaleY
                                );
                            }

                            return new ContourSegment(start, end, center, s.Radius, s.CounterClockwise);
                        }
                    })
                    .ToList();

                noweSkrzydla.Add(new ShapeRegion
                {
                    Id = skrzydlo.Id,
                    TypKsztaltu = skrzydlo.TypKsztaltu,
                    TypLiniiDzielacej = skrzydlo.TypLiniiDzielacej,
                    Wierzcholki = noweWierzcholki,
                    Kontur = nowyKontur
                });
            }

            return noweSkrzydla;
        }

        public static List<ShapeRegion> SkalujRegiony(
        List<ShapeRegion> stareRegiony,
        int nowaSzerokosc,
        int nowaWysokosc)
        {
            if (stareRegiony == null || !stareRegiony.Any())
                return new List<ShapeRegion>();

            // Oblicz bounding box dla całego zbioru regionów
            double minX = stareRegiony.Min(r => r.Wierzcholki.Min(p => p.X));
            double minY = stareRegiony.Min(r => r.Wierzcholki.Min(p => p.Y));
            double maxX = stareRegiony.Max(r => r.Wierzcholki.Max(p => p.X));
            double maxY = stareRegiony.Max(r => r.Wierzcholki.Max(p => p.Y));

            double originalWidth = maxX - minX;
            double originalHeight = maxY - minY;

            double scaleX = nowaSzerokosc / originalWidth;
            double scaleY = nowaWysokosc / originalHeight;

            // Skaluj każdy region
            var noweRegiony = new List<ShapeRegion>();

            foreach (var region in stareRegiony)
            {
                var noweWierzcholki = region.Wierzcholki
                    .Select(p => new XPoint(
                        (p.X - minX) * scaleX,
                        (p.Y - minY) * scaleY))
                    .ToList();

                var nowyKontur = region.Kontur
                    .Select(s =>
                    {
                        var start = new XPoint(
                            (s.Start.X - minX) * scaleX,
                            (s.Start.Y - minY) * scaleY
                        );
                        var end = new XPoint(
                            (s.End.X - minX) * scaleX,
                            (s.End.Y - minY) * scaleY
                        );

                        if (s.Type == SegmentType.Line)
                        {
                            return new ContourSegment(start, end);
                        }
                        else // łuk
                        {
                            XPoint center = new();
                            if (s.Center != null)
                            {
                                center = new XPoint(
                                    (s.Center.Value.X - minX) * scaleX,
                                    (s.Center.Value.Y - minY) * scaleY
                                );
                            }

                            return new ContourSegment(start, end, center, s.Radius, s.CounterClockwise);
                        }
                    })
                    .ToList();

                var nowyRegion = new ShapeRegion
                {
                    Id = region.Id,                    // zachowaj Id
                    TypKsztaltu = region.TypKsztaltu,  // zachowaj typ
                    TypLiniiDzielacej = region.TypLiniiDzielacej,
                    Wierzcholki = noweWierzcholki,
                    Kontur = nowyKontur
                };

                noweRegiony.Add(nowyRegion);
            }

            return noweRegiony;
        }
        // --- helper: głęboka kopia regionu
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

        // --- skalowanie skrzydeł względem ramy przed/po
        public static List<ShapeRegion> SkalujSkrzydlaDoRamy(
            List<ShapeRegion> skrzydla,
            ShapeRegion ramaBefore,
            ShapeRegion ramaAfter)
        {
            if (skrzydla == null || !skrzydla.Any() || ramaBefore == null || ramaAfter == null)
                return skrzydla ?? new List<ShapeRegion>();

            // bounding box ramy przed
            double minXOld = ramaBefore.Wierzcholki.Min(p => p.X);
            double minYOld = ramaBefore.Wierzcholki.Min(p => p.Y);
            double maxXOld = ramaBefore.Wierzcholki.Max(p => p.X);
            double maxYOld = ramaBefore.Wierzcholki.Max(p => p.Y);
            double oldWidth = maxXOld - minXOld;
            double oldHeight = maxYOld - minYOld;
            if (oldWidth == 0 || oldHeight == 0)
                return skrzydla;

            // bounding box ramy po
            double minXNew = ramaAfter.Wierzcholki.Min(p => p.X);
            double minYNew = ramaAfter.Wierzcholki.Min(p => p.Y);
            double maxXNew = ramaAfter.Wierzcholki.Max(p => p.X);
            double maxYNew = ramaAfter.Wierzcholki.Max(p => p.Y);
            double newWidth = maxXNew - minXNew;
            double newHeight = maxYNew - minYNew;
            if (newWidth == 0 || newHeight == 0)
                return skrzydla;

            double scaleX = newWidth / oldWidth;
            double scaleY = newHeight / oldHeight;

            var wynik = new List<ShapeRegion>(skrzydla.Count);
            foreach (var s in skrzydla)
            {
                var kopia = CloneRegion(s);

                //kopia.Id = s.Id; // zachowaj oryginalne Id do sprawdzenia

                // dla każdego punktu: przenieś względnie do ramy przed i przeskaluj do ramy po, następnie wypośrodkuj do nowej pozycji
                for (int i = 0; i < kopia.Wierzcholki.Count; i++)
                {
                    var p = kopia.Wierzcholki[i];
                    double relX = (p.X - minXOld); // odległość od lewej krawędzi ramy przed
                    double relY = (p.Y - minYOld); // od górnej krawędzi ramy przed

                    double newX = minXNew + relX * scaleX;
                    double newY = minYNew + relY * scaleY;

                    kopia.Wierzcholki[i] = new XPoint { X = newX, Y = newY };
                    kopia.Kontur[i] = new ContourSegment(
                        new XPoint { X = newX, Y = newY },
                        new XPoint
                        {
                            X = minXNew + (kopia.Kontur[i].End.X - minXOld) * scaleX,
                            Y = minYNew + (kopia.Kontur[i].End.Y - minYOld) * scaleY
                        },
                        kopia.Kontur[i].Center != null ? new XPoint
                        {
                            X = minXNew + (kopia.Kontur[i].Center.Value.X - minXOld) * scaleX,
                            Y = minYNew + (kopia.Kontur[i].Center.Value.Y - minYOld) * scaleY
                        } : null,
                        kopia.Kontur[i].Radius * ((scaleX + scaleY) / 2), // średnia skala dla promienia łuku
                        kopia.Kontur[i].CounterClockwise
                    );
                }

                // skaluj też linie dzielące wewnątrz skrzydła
                if (kopia.LinieDzielace != null)
                {
                    foreach (var lin in kopia.LinieDzielace)
                    {
                        if (lin.Points == null) continue;
                        for (int j = 0; j < lin.Points.Count; j++)
                        {
                            var q = lin.Points[j];
                            double relX = (q.X - minXOld);
                            double relY = (q.Y - minYOld);
                            double newX = minXNew + relX * scaleX;
                            double newY = minYNew + relY * scaleY;
                            lin.Points[j] = new XPoint { X = newX, Y = newY };
                        }
                    }
                }

                wynik.Add(kopia);
            }

            return wynik;
        }

        public static List<ShapeRegion> SkalujRegionyIndywidualnie(
        List<ShapeRegion> stareRegiony,
        int nowaSzerokosc,
        int nowaWysokosc)
        {
            if (stareRegiony == null || !stareRegiony.Any())
                return new List<ShapeRegion>();

            var noweRegiony = new List<ShapeRegion>();

            foreach (var region in stareRegiony)
            {
                // Bounding box dla pojedynczego regionu
                double minX = region.Wierzcholki.Min(p => p.X);
                double minY = region.Wierzcholki.Min(p => p.Y);
                double maxX = region.Wierzcholki.Max(p => p.X);
                double maxY = region.Wierzcholki.Max(p => p.Y);

                double originalWidth = maxX - minX;
                double originalHeight = maxY - minY;

                if (originalWidth == 0 || originalHeight == 0)
                    continue;

                double scaleX = nowaSzerokosc / originalWidth;
                double scaleY = nowaWysokosc / originalHeight;

                var noweWierzcholki = region.Wierzcholki
                    .Select(p => new XPoint(
                        (p.X - minX) * scaleX,
                        (p.Y - minY) * scaleY))
                    .ToList();

                var nowyKontur = region.Kontur
                .Select(s =>
                {
                    var start = new XPoint(
                        (s.Start.X - minX) * scaleX,
                        (s.Start.Y - minY) * scaleY
                    );
                    var end = new XPoint(
                        (s.End.X - minX) * scaleX,
                        (s.End.Y - minY) * scaleY
                    );

                    if (s.Type == SegmentType.Line)
                    {
                        return new ContourSegment(start, end);
                    }
                    else // łuk
                    {
                        XPoint center = new();
                        if (s.Center != null)
                        {
                            center = new XPoint(
                                (s.Center.Value.X - minX) * scaleX,
                                (s.Center.Value.Y - minY) * scaleY
                            );
                        }

                        return new ContourSegment(start, end, center, s.Radius, s.CounterClockwise);
                    }
                })
                .ToList();

                noweRegiony.Add(new ShapeRegion
                {
                    Id = region.Id,
                    IdMaster = region.IdMaster,
                    IdRegionuPonizej = region.IdRegionuPonizej,
                    TypKsztaltu = region.TypKsztaltu,
                    TypLiniiDzielacej = region.TypLiniiDzielacej,
                    Wierzcholki = noweWierzcholki,
                    Kontur = nowyKontur,
                });
            }

            return noweRegiony;
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
            // Początkowa lista wyników zawiera oryginalny region
            var wynik = new List<ShapeRegion> { region };

            // Iterujemy po wszystkich liniach dzielących
            foreach (var line in lines)
            {
                var next = new List<ShapeRegion>();

                foreach (var r in wynik)
                {
                    // 1️⃣ Podział wierzchołków
                    var splitLinie = PodzielPolygonPoLinii(r.Wierzcholki, line);

                    // 2️⃣ Podział konturu
                    var splitFullKontur = PodzielKonturPoLinii(r.Kontur, line);

                    // Jeśli podział faktycznie wystąpił
                    if (splitLinie.Count > 1 && splitFullKontur.Count > 0)
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

            foreach (var seg in contour)
            {
                bool startLeft = PunktPoLewejStronie(seg.Start, line);
                bool endLeft = PunktPoLewejStronie(seg.End, line);

                if (startLeft && endLeft)
                {
                    left.Add(seg);
                }
                else if (!startLeft && !endLeft)
                {
                    right.Add(seg);
                }
                else
                {
                    // Segment przecina linię – dzielimy na dwa
                    if (ObliczPrzeciecie(seg.Start, seg.End, line, out var intersection))
                    {
                        if (startLeft)
                        {
                            left.Add(new ContourSegment(seg.Start, intersection));
                            right.Add(new ContourSegment(intersection, seg.End));
                        }
                        else
                        {
                            right.Add(new ContourSegment(seg.Start, intersection));
                            left.Add(new ContourSegment(intersection, seg.End));
                        }
                    }
                    else
                    {
                        // Brak przecięcia, traktujemy jako cały segment na odpowiedniej stronie
                        if (startLeft) left.Add(seg); else right.Add(seg);
                    }
                }
            }

            var res = new List<List<ContourSegment>>();
            if (left.Count > 0) res.Add(left);
            if (right.Count > 0) res.Add(right);
            return res;
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

        private static List<XPoint> GenerateCircleVertices(double centerX, double centerY, double radius, int segments)
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