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
            Console.WriteLine($"🔍 Analizuje dane wewnątrz GenerujRegionyZPodzialu shapes: {shapes.Count} dla szerokosc: {szerokosc} wysokosc: {wysokosc}");

            var regions = new List<ShapeRegion>();

            // Wydziel linie, które są typami podziałowymi
            var linieDzielace = shapes
                .OfType<XLineShape>()
                .Where(l => l.DualRama)
                .ToList();

            // 1. Oblicz globalny bounding box — tylko dla shape'ów tworzących regiony
            var shapesDoRegionow = shapes.Where(s =>
                s is XRectangleShape or XSquareShape or XTriangleShape or XTrapezoidShape or XCircleShape or XHouseShape or XRoundedTopRectangleShape or XRoundedRectangleShape or XRoundedRectangleShapeLeft or XRoundedRectangleShapeRight
            ).ToList();

            double minX = shapesDoRegionow.Min(s => s.GetBoundingBox().X);
            double minY = shapesDoRegionow.Min(s => s.GetBoundingBox().Y);
            double maxX = shapesDoRegionow.Max(s => s.GetBoundingBox().X + s.GetBoundingBox().Width);
            double maxY = shapesDoRegionow.Max(s => s.GetBoundingBox().Y + s.GetBoundingBox().Height);

            double originalWidth = maxX - minX;
            double originalHeight = maxY - minY;

            // 2. Oblicz skalę osobno w osi X i Y (pełne dopasowanie, bez proporcji)
            double scaleX = szerokosc / originalWidth;
            double scaleY = wysokosc / originalHeight;

            // 3. Offsety tak, by przesunąć kształty do (0,0)
            double offsetX = -minX * scaleX;
            double offsetY = -minY * scaleY;

            foreach (var shape in shapes)
            {
                var bboxBefore = shape.GetBoundingBox();
                Console.WriteLine($"Przed transformacji shape'ów: X={bboxBefore.X:F2}, Y={bboxBefore.Y:F2}, W={bboxBefore.Width:F2}, H={bboxBefore.Height:F2}");

                shape.Transform(scaleX, scaleY, offsetX, offsetY);

                var bboxAfter = shape.GetBoundingBox();
                Console.WriteLine($"Po transformacji shape'ów:    X={bboxAfter.X:F2}, Y={bboxAfter.Y:F2}, W={bboxAfter.Width:F2}, H={bboxAfter.Height:F2}");
                Console.WriteLine("---");
            }


            foreach (var shape in shapes)
            {
                List<XPoint> wierzcholki = new();
                string typKształtu = "inny";

                switch (shape)
                {
                    case XSquareShape rect:
                        wierzcholki = rect.GetCorners();
                        typKształtu = "kwadrat";
                        break;

                    case XRectangleShape rect:
                        wierzcholki = rect.GetCorners();
                        typKształtu = "prostokąt";
                        break;

                    case XTriangleShape triangle:
                        wierzcholki = triangle.GetVertices();
                        typKształtu = "trójkąt";
                        break;

                    case XTrapezoidShape trapezoid:
                        wierzcholki = trapezoid.GetVertices();
                        typKształtu = "trapez";
                        break;

                    case XCircleShape circle:
                        wierzcholki = GenerateCircleVertices(circle.X, circle.Y, circle.Radius, 32);
                        typKształtu = "okrąg";
                        break;

                    case XHouseShape house:
                        var (roof, walls) = house.GetVertices();
                        wierzcholki.AddRange(roof.Select(p => new XPoint(p.X, p.Y)));
                        wierzcholki.AddRange(walls.Select(p => new XPoint(p.X, p.Y)));
                        typKształtu = "domek";
                        break;

                    case XRoundedTopRectangleShape roundedTop:
                        wierzcholki = roundedTop.GetVertices();
                        typKształtu = "zaokrąglony prostokąt (góra)";
                        break;

                    case XRoundedRectangleShape rounded:
                        wierzcholki = rounded.GetVertices();
                        typKształtu = "zaokrąglony prostokąt";
                        break;

                    case XRoundedRectangleShapeLeft roundedLeft:
                        wierzcholki = roundedLeft.GetVertices();
                        typKształtu = "zaokrąglony lewy prostokąt";
                        break;

                    case XRoundedRectangleShapeRight roundedRight:
                        wierzcholki = roundedRight.GetVertices();
                        typKształtu = "zaokrąglony prawy prostokąt";
                        break;

                    case XLineShape linia:
                        // Traktuj wszystkie linie jako osobne regiony
                        wierzcholki = new List<XPoint> { new(linia.X1, linia.Y1), new(linia.X2, linia.Y2) };
                        typKształtu = linia.NazwaObj ?? "linia";
                        regions.Add(new ShapeRegion
                        {
                            Wierzcholki = wierzcholki,
                            TypKsztaltu = typKształtu
                        });
                        continue;

                    default:
                        Console.WriteLine("⚠️ Nieznany kształt w kolekcji shapes. Pominięto.");
                        continue;
                }

                var bbox = CalculateBoundingBox(wierzcholki);
                var podzielony = false;

                foreach (var linia in linieDzielace)
                {
                    if (LiniaPrzecinaProstokat(linia, bbox) && shape is XRectangleShape)
                    {
                        var noweRegiony = PodzielProstokat((XRectangleShape)shape, linia);
                        regions.AddRange(noweRegiony);
                        podzielony = true;
                        break;
                    }
                }

                if (!podzielony)
                {
                    // 🛠️ Jeśli nie ma podziału, to wymuś jeden region o pełnym rozmiarze
                    var pelnyRegion = new List<XPoint>
                    {
                        new XPoint(0, 0),
                        new XPoint(szerokosc, 0),
                        new XPoint(szerokosc, wysokosc),
                        new XPoint(0, wysokosc)
                    };

                        regions.Add(new ShapeRegion
                        {
                            Wierzcholki = pelnyRegion,
                            TypKsztaltu = "prostokąt" // lub typKształtu jeśli istotne
                        });
                    }
                }

           // NormalizeRegionSize(regions, szerokosc, wysokosc);

            return regions;
        }

        //private static void NormalizeRegionSize(List<ShapeRegion> regions, int targetWidth, int targetHeight)
        //{
        //    var minX = regions.SelectMany(r => r.Wierzcholki).Min(p => p.X);
        //    var minY = regions.SelectMany(r => r.Wierzcholki).Min(p => p.Y);
        //    var maxX = regions.SelectMany(r => r.Wierzcholki).Max(p => p.X);
        //    var maxY = regions.SelectMany(r => r.Wierzcholki).Max(p => p.Y);

        //    var currentWidth = maxX - minX;
        //    var currentHeight = maxY - minY;

        //    var scaleX = targetWidth / currentWidth;
        //    var scaleY = targetHeight / currentHeight;

        //    foreach (var region in regions)
        //    {
        //        for (int i = 0; i < region.Wierzcholki.Count; i++)
        //        {
        //            var p = region.Wierzcholki[i];
        //            region.Wierzcholki[i] = new XPoint(
        //                (p.X - minX) * scaleX,
        //                (p.Y - minY) * scaleY
        //            );
        //        }
        //    }
        //}

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