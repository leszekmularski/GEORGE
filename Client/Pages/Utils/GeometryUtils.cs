using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Client.Pages.Models;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Utils
{
    public static class GeometryUtils
    {
        public static List<ShapeRegion> GenerujRegionyZPodzialu(List<IShapeDC> shapes, int szerokosc, int wysokosc, bool rama)
        {
           // Console.WriteLine($"📦 Przed usunięciem duplikatów: {shapes.Count} obiektów.");
            shapes = UsunDuplikatyShape(shapes);
            //Console.WriteLine($"📦 Po usunięciu duplikatów: {shapes.Count} obiektów.");

            var regions = new List<ShapeRegion>();

            //var linieDzielace = shapes
            //    .OfType<XLineShape>()
            //    .Where(l => rama ? l.DualRama : l.RuchomySlupek)
            //    .ToList();

            var shapesDoRegionow = shapes.Where(s =>
                s is XRectangleShape or XSquareShape or XTriangleShape
                or XTrapezoidShape or XCircleShape or XHouseShape
                or XRoundedTopRectangleShape or XRoundedRectangleShape
                or XRoundedRectangleShapeLeft or XRoundedRectangleShapeRight)
                .ToList();

            if (!shapesDoRegionow.Any()) return regions;

            double minX = shapesDoRegionow.Min(s => s.GetBoundingBox().X);
            double minY = shapesDoRegionow.Min(s => s.GetBoundingBox().Y);
            double maxX = shapesDoRegionow.Max(s => s.GetBoundingBox().X + s.GetBoundingBox().Width);
            double maxY = shapesDoRegionow.Max(s => s.GetBoundingBox().Y + s.GetBoundingBox().Height);

            double scaleX = (double)szerokosc / (maxX - minX);
            double scaleY = (double)wysokosc / (maxY - minY);
            double offsetX = -minX * scaleX;
            double offsetY = -minY * scaleY;

            foreach (var shape in shapes)
            {
                shape.Transform(scaleX, scaleY, offsetX, offsetY);
                shape.Szerokosc = szerokosc;
                shape.Wysokosc = wysokosc;
            }

            foreach (var shape in shapes)
            {
                List<XPoint> pts = shape switch
                {
                    XLineShape lin => new() { new(lin.X1, lin.Y1), new(lin.X2, lin.Y2) },
                    XSquareShape sq => sq.GetCorners(),
                    XRectangleShape rect => rect.GetCorners(),
                    XTriangleShape tri => tri.GetVertices(),
                    XTrapezoidShape trap => trap.GetVertices(),
                    XCircleShape circ => GenerateCircleVertices(circ.X, circ.Y, circ.Radius, 32),
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

                string id = Guid.NewGuid().ToString(); ;

                if (shape is XLineShape linia && linia.RuchomySlupek)
                {
                    typLinii = "Słupek ruchomy";
                   // id = "SR-" + id;
                }
                else
                {
                    if (shape is XLineShape liniaD && liniaD.DualRama)
                    {
                        typLinii = "Podwójna rama";
                        //id = "DR-" + id;
                    }
                    else
                    {
                        typLinii = "Brak podziału";
                        //id = "BP-" + id;
                    }
                }
   
               // Console.WriteLine($"🔍GenerujRegionyZPodzialu --> Generowanie regionu z kształtu: {typ}, liczba wierzchołków: {pts.Count}");

                var initial = new ShapeRegion
                {
                    Wierzcholki = pts,
                    TypKsztaltu = typ,
                    TypLiniiDzielacej = typLinii,
                    Id = id,
                    IdMaster = id,
                };


                if (rama)
                {
                    var linieDzielace = shapes
                    .OfType<XLineShape>()
                    .Where(l => l.DualRama)
                    .ToList();

                   // initial.Id = "R-" + initial.Id;

                    var podzielone = PodzielRegionRekurencyjnie(initial, linieDzielace, id);

                    foreach (var r in podzielone)
                    {
                        r.RozpoznajTyp(r.TypKsztaltu);

                        if (r.TypKsztaltu == "xhouseshape" && r.Wierzcholki.Count == 4)
                        {
                            r.TypKsztaltu = "trapez";
                        }

                        if (r.TypKsztaltu == "trapez")
                        {
                            if (r.Wierzcholki.Count == 3)
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
                else
                {
                    var linieDzielace = shapes
                        .OfType<XLineShape>()
                        .ToList();

                    //initial.Id = "N-" + initial.Id;

                    var podzielone = PodzielRegionRekurencyjnie(initial, linieDzielace, id);

                    foreach (var r in podzielone)
                    {
                        r.RozpoznajTyp(r.TypKsztaltu);

                        if (r.TypKsztaltu == "xhouseshape" && r.Wierzcholki.Count == 4)
                        {
                            r.TypKsztaltu = "trapez";
                        }

                        if (r.TypKsztaltu == "trapez")
                        {
                            if (r.Wierzcholki.Count == 3)
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

            return regions;
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

                var nowyRegion = new ShapeRegion
                {
                    Id = region.Id,                    // zachowaj Id
                    TypKsztaltu = region.TypKsztaltu,  // zachowaj typ
                    TypLiniiDzielacej = region.TypLiniiDzielacej,
                    Wierzcholki = noweWierzcholki
                };

                noweRegiony.Add(nowyRegion);
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

        private static List<ShapeRegion> PodzielRegionRekurencyjnie(ShapeRegion region, List<XLineShape> lines, string idMaster)
        {
            var wynik = new List<ShapeRegion> { region };

            foreach (var line in lines)
            {
                var next = new List<ShapeRegion>();

                foreach (var r in wynik)
                {
                    var split = PodzielPolygonPoLinii(r.Wierzcholki, line);

                    if (split.Count > 1)
                    {
                        foreach (var poly in split)
                            next.Add(new ShapeRegion
                            {
                                Wierzcholki = poly,
                                TypKsztaltu = r.TypKsztaltu,
                                LinieDzielace = r.LinieDzielace.Concat(new[] { line }).ToList(),
                                IdMaster = idMaster,
                            });
                    }
                    else
                        next.Add(r);

                wynik = next;
                }

            }

            return wynik;
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

        /// <summary>
        /// Sprawdza, czy linia przecina dany prostokąt (bounding box).
        /// </summary>
        public static bool LiniaPrzecinaProstokat(XLineShape linia, XBoundingBox prostokat)
        {
            var edges = prostokat.GetEdges(); // lista krawędzi jako (Start, End)

            foreach (var edge in edges)
            {
                if (LinesIntersect(
                    linia.X1, linia.Y1, linia.X2, linia.Y2,
                    edge.Start.X, edge.Start.Y, edge.End.X, edge.End.Y))
                {
                    return true; // przynajmniej jedna krawędź przecięta
                }
            }

            return false;
        }

        /// <summary>
        /// Sprawdza, czy dwa odcinki się przecinają, uwzględniając przypadki specjalne.
        /// </summary>
        private static bool LinesIntersect(double x1, double y1, double x2, double y2,
                                           double x3, double y3, double x4, double y4)
        {
            double d1 = Direction(x3, y3, x4, y4, x1, y1);
            double d2 = Direction(x3, y3, x4, y4, x2, y2);
            double d3 = Direction(x1, y1, x2, y2, x3, y3);
            double d4 = Direction(x1, y1, x2, y2, x4, y4);

            // Standardowe przecięcie
            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            {
                return true;
            }

            // Specjalne przypadki - współliniowość
            if (d1 == 0 && OnSegment(x3, y3, x4, y4, x1, y1)) return true;
            if (d2 == 0 && OnSegment(x3, y3, x4, y4, x2, y2)) return true;
            if (d3 == 0 && OnSegment(x1, y1, x2, y2, x3, y3)) return true;
            if (d4 == 0 && OnSegment(x1, y1, x2, y2, x4, y4)) return true;

            return false;
        }

        /// <summary>
        /// Sprawdza, czy punkt (px, py) leży na odcinku (x1, y1) -> (x2, y2).
        /// </summary>
        private static bool OnSegment(double x1, double y1, double x2, double y2, double px, double py)
        {
            return px >= Math.Min(x1, x2) && px <= Math.Max(x1, x2) &&
                   py >= Math.Min(y1, y2) && py <= Math.Max(y1, y2);
        }


        /// <summary>
        /// Pomocnicza funkcja do określenia kierunku skrętu (orientacji).
        /// </summary>
        private static double Direction(double xi, double yi, double xj, double yj, double xk, double yk)
        {
            return (xk - xi) * (yj - yi) - (xj - xi) * (yk - yi);
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


    }
}