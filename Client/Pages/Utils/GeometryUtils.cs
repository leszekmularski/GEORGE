using System;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Client.Pages.Models;
using static GEORGE.Client.Pages.CAD.DrawCAD;

namespace GEORGE.Client.Pages.Utils
{
    public static class GeometryUtils
    {
        public static List<ShapeRegion> GenerujRegionyZPodzialu(List<IShapeDC> shapes)
        {
            Console.WriteLine($"Analizuje dane wewnątrz GenerujRegionyZPodzialu shapes: {shapes.Count}");

            var regions = new List<ShapeRegion>();

            var linie = shapes.OfType<XLineShape>().ToList(); // Wszystkie linie dzielące

            foreach (var shape in shapes)
            {
                List<PointDC> wierzcholki = new();
                string typKształtu = "inny";

                if (shape is XRectangleShape rect)
                {
                    wierzcholki = rect.GetCorners();
                    typKształtu = "prostokąt";
                }
                else if (shape is XTriangleShape triangle)
                {
                    wierzcholki = triangle.GetVertices();
                    typKształtu = "trójkąt";
                }
                else if (shape is XTrapezoidShape trapezoid)
                {
                    wierzcholki = trapezoid.GetVertices();
                    typKształtu = "trapez";
                }
                else if (shape is XCircleShape circle)
                {
                    // Możesz przybliżyć okrąg wielokątem np. 32 wierzchołki
                    wierzcholki = GenerateCircleVertices(circle.X, circle.Y, circle.Radius, 32);
                    typKształtu = "okrąg";
                }
                else if (shape is XHouseShape house)
                {
                    var (roof, walls) = house.GetVertices();
                    wierzcholki.AddRange(roof.Select(p => new PointDC(p.X, p.Y)));
                    wierzcholki.AddRange(walls.Select(p => new PointDC(p.X, p.Y)));
                    typKształtu = "domek";
                }

                else if (shape is XRoundedTopRectangleShape roundedRect)
                {
                    wierzcholki = roundedRect.GetVertices();
                    typKształtu = "zaokrąglony prostokąt";
                }
                else
                {
                    Console.WriteLine("⚠️ Nieznany kształt w kolekcji shapes.");
                    continue;
                }

                var bbox = CalculateBoundingBox(wierzcholki);
                var podzielony = false;

                foreach (var linia in linie)
                {
                    if (LiniaPrzecinaProstokat(linia, bbox))
                    {
                        // Na razie podział tylko dla prostokątów (możesz rozszerzyć później dla innych)
                        if (typKształtu == "prostokąt")
                        {
                            var noweRegiony = PodzielProstokat((XRectangleShape)shape, linia);
                            regions.AddRange(noweRegiony);
                            podzielony = true;
                            break;
                        }
                    }
                }

                if (!podzielony)
                {
                    regions.Add(new ShapeRegion
                    {
                        Wierzcholki = wierzcholki,
                        TypKształtu = typKształtu
                    });
                }
            }

            return regions;
        }
        private static List<PointDC> GenerateCircleVertices(double centerX, double centerY, double radius, int segments)
        {
            var points = new List<PointDC>();

            for (int i = 0; i < segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                points.Add(new PointDC(
                    centerX + radius * Math.Cos(angle),
                    centerY + radius * Math.Sin(angle)
                ));
            }

            return points;
        }
        private static XBoundingBox CalculateBoundingBox(List<PointDC> points)
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
                        Wierzcholki = new List<PointDC>
                    {
                        new(bbox.X, bbox.Y),
                        new(x, bbox.Y),
                        new(x, bbox.Y + bbox.Height),
                        new(bbox.X, bbox.Y + bbox.Height)
                    },
                        TypKształtu = "prostokąt",
                        LinieDzielace = new List<XLineShape> { linia }
                    });

                    // Prawy region
                    result.Add(new ShapeRegion
                    {
                        Wierzcholki = new List<PointDC>
                    {
                        new(x, bbox.Y),
                        new(bbox.X + bbox.Width, bbox.Y),
                        new(bbox.X + bbox.Width, bbox.Y + bbox.Height),
                        new(x, bbox.Y + bbox.Height)
                    },
                        TypKształtu = "prostokąt",
                        LinieDzielace = new List<XLineShape> { linia }
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