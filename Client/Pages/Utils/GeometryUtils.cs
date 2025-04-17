using System;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Client.Pages.Models;

namespace GEORGE.Client.Pages.Utils
{
    public static class GeometryUtils
    {

        public static List<ShapeRegion> GenerujRegionyZPodzialu(List<IShapeDC> shapes)
        {
            var regions = new List<ShapeRegion>();

            var prostokaty = shapes.OfType<XRectangleShape>().ToList();
            var linie = shapes.OfType<XLineShape>().ToList();

            foreach (var rect in prostokaty)
            {
                var bbox = rect.GetBoundingBox();
                var podzielony = false;

                foreach (var linia in linie)
                {
                    if (LiniaPrzecinaProstokat(linia, bbox))
                    {
                        var noweRegiony = PodzielProstokat(rect, linia);
                        regions.AddRange(noweRegiony);
                        podzielony = true;
                        break;
                    }
                }

                if (!podzielony)
                {
                    regions.Add(new ShapeRegion
                    {
                        Wierzcholki = rect.GetCorners(),
                        TypKształtu = "prostokąt"
                    });
                }
            }

            return regions;
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

            return result;
        }

        /// <summary>
        /// Sprawdza, czy dwa odcinki się przecinają (algorytm oparty na orientacji).
        /// </summary>
        private static bool LinesIntersect(double x1, double y1, double x2, double y2,
                                           double x3, double y3, double x4, double y4)
        {
            double d1 = Direction(x3, y3, x4, y4, x1, y1);
            double d2 = Direction(x3, y3, x4, y4, x2, y2);
            double d3 = Direction(x1, y1, x2, y2, x3, y3);
            double d4 = Direction(x1, y1, x2, y2, x4, y4);

            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            {
                return true;
            }

            return false;
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