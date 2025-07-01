using System;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Client.Pages.Models;
using static GEORGE.Client.Pages.CAD.DrawCAD;

namespace GEORGE.Client.Pages.Utils
{
    public static class GeometryUtils
    {
        public static List<ShapeRegion> GenerujRegionyZPodzialu(List<IShapeDC> shapes, int szerokosc, int wysokosc)
        {
            Console.WriteLine($"📦 Przed usunięciem duplikatów: {shapes.Count} obiektów.");
            shapes = UsunDuplikatyShape(shapes);
            Console.WriteLine($"📦 Po usunięciu duplikatów: {shapes.Count} obiektów.");

            var regions = new List<ShapeRegion>();

            var linieDzielace = shapes
                .OfType<XLineShape>()
                .Where(l => l.DualRama)
                .ToList();

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

            double scaleX = szerokosc / (maxX - minX);
            double scaleY = wysokosc / (maxY - minY);
            double offsetX = -minX * scaleX;
            double offsetY = -minY * scaleY;

            foreach (var shape in shapes)
            {
                shape.Transform(scaleX, scaleY, offsetX, offsetY);
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

                Console.WriteLine($"🔍GenerujRegionyZPodzialu --> Generowanie regionu z kształtu: {typ}, liczba wierzchołków: {pts.Count}");

                var initial = new ShapeRegion
                {
                    Wierzcholki = pts,
                    TypKsztaltu = typ,
                    Id = Guid.NewGuid().ToString()
                };

                var podzielone = PodzielRegionRekurencyjnie(initial, linieDzielace);

                foreach (var r in podzielone)
                {
                    r.RozpoznajTyp();

                    if (r.TypKsztaltu == "xhouseshape" && r.Wierzcholki.Count == 4)
                    {
                        r.TypKsztaltu = "trapez";
                    }
                    if (r.TypKsztaltu == "trapez" && r.Wierzcholki.Count == 3)
                    {
                        r.TypKsztaltu = "trójkąt";
                    }
                    r.Id = Guid.NewGuid().ToString();
                }

                regions.AddRange(podzielone);
            }

            return regions;
        }
        private static List<ShapeRegion> PodzielRegionRekurencyjnie(ShapeRegion region, List<XLineShape> lines)
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
                                Id = Guid.NewGuid().ToString()
                            });
                    }
                    else
                        next.Add(r);
                }

                wynik = next;
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
        private static XBoundingBox CalculateBoundingBox(List<XPoint> points)
        {
            double minX = points.Min(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxX = points.Max(p => p.X);
            double maxY = points.Max(p => p.Y);

            return new XBoundingBox(minX, minY, maxX - minX, maxY - minY, "BoundingBox");
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
        /// Przykładowa implementacja dzielenia prostokąta – tylko pionowy podział.
        /// </summary>
        private static List<ShapeRegion> PodzielProstokat(XRectangleShape rect, XLineShape linia)
        {
            var result = new List<ShapeRegion>();

            var bbox = rect.GetBoundingBox();

            // Sprawdzamy czy linia jest pionowa (stały X)
            if (linia.X1 == linia.X2)
            {
                double x = linia.X1;
                if (x > bbox.X && x < bbox.X + bbox.Width)
                {
                    // Lewy region
                    result.Add(new ShapeRegion
                    {
                        Wierzcholki = new List<XPoint>
                    {
                        new(bbox.X, bbox.Y),
                        new(x, bbox.Y),
                        new(x, bbox.Y + bbox.Height),
                        new(bbox.X, bbox.Y + bbox.Height)
                    },
                        TypKsztaltu = "prostokąt",
                        LinieDzielace = new List<XLineShape> { linia },
                        Id = Guid.NewGuid().ToString()
                    });

                    // Prawy region
                    result.Add(new ShapeRegion
                    {
                        Wierzcholki = new List<XPoint>
                    {
                        new(x, bbox.Y),
                        new(bbox.X + bbox.Width, bbox.Y),
                        new(bbox.X + bbox.Width, bbox.Y + bbox.Height),
                        new(x, bbox.Y + bbox.Height)
                    },
                        TypKsztaltu = "prostokąt",
                        LinieDzielace = new List<XLineShape> { linia },
                        Id = Guid.NewGuid().ToString()
                    });
                }
            }

            // TODO: Możesz dodać obsługę poziomych lub ukośnych linii tutaj
            Console.WriteLine($"Ilość elementów result: {result}");
            return result;
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

    }
}