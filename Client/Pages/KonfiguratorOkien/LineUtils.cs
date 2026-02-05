using DocumentFormat.OpenXml.Presentation;
using GEORGE.Client.Pages.Models;
using GEORGE.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public static class LineUtils
    {
        private const double Tolerance = 0.001;

        // 🔹 Tworzenie kopii linii z zachowaniem właściwości
        public static XLineShape CreateLine(XLineShape template, double x1, double y1, double x2, double y2, double scale)
        {
            return new XLineShape(x1, y1, x2, y2, scale,
                template.NazwaObj, template.RuchomySlupek, template.PionPoziom,
                template.DualRama, template.GenerowaneZRamy, template.StalySlupek)
            {
                ID = template.ID
            };
        }

        // 🔹 Usuwanie linii całkowicie poza zamkniętymi kształtami
        public static void RemoveLinesOutsideShapes(List<IShapeDC> shapes)
        {
            var closedShapes = shapes.Where(s => s is not XLineShape).ToList();
            var lines = shapes.OfType<XLineShape>().ToList();

            foreach (var line in lines)
            {
                bool intersectsAny = closedShapes.Any(s =>
                {
                    var bbox = s.GetBoundingBox();
                    return LineIntersectsBoundingBox(line, bbox);
                });

                if (!intersectsAny)
                {
                    shapes.Remove(line);
                }
            }
        }

        // 🔹 Rozszerz linie do granic zamkniętych kształtów
        public static void ExtendLinesToShapes(List<IShapeDC> shapes, double scaleFactor)
        {
            var closedShapes = shapes.Where(s => s is not XLineShape).ToList();
            var lines = shapes.OfType<XLineShape>().ToList();

            foreach (var line in lines)
            {
                foreach (var shape in closedShapes)
                {
                    var bbox = shape.GetBoundingBox();

                    var extended = ExtendLineToBoundingBox(line, bbox, scaleFactor);

                    // zamiast usuwać i dodawać
                    line.X1 = extended.X1;
                    line.Y1 = extended.Y1;
                    line.X2 = extended.X2;
                    line.Y2 = extended.Y2;
                }
            }
        }


        // 🔹 Rozmieszczenie pionowych i poziomych linii
        public static void DistributeLines(List<IShapeDC> shapes)
        {
            var closedShapes = shapes.Where(s => s is not XLineShape).ToList();
            var lines = shapes.OfType<XLineShape>().ToList();

            var verticalLines = lines.Where(l => Math.Abs(l.X1 - l.X2) < Tolerance).OrderBy(l => l.X1).ToList();
            var horizontalLines = lines.Where(l => Math.Abs(l.Y1 - l.Y2) < Tolerance).OrderBy(l => l.Y1).ToList();

            // Pionowe
            if (verticalLines.Any())
            {
                double minX = closedShapes.Min(s => s.GetBoundingBox().Left);
                double maxX = closedShapes.Max(s => s.GetBoundingBox().Right);
                double centerX = (minX + maxX) / 2.0;

                if (verticalLines.Count == 1)
                {
                    var line = verticalLines.First();
                    line.X1 = line.X2 = centerX;
                }
                else
                {
                    double spacing = (maxX - minX) / (verticalLines.Count + 1);
                    int i = 1;
                    foreach (var line in verticalLines)
                    {
                        line.X1 = line.X2 = minX + i * spacing;
                        i++;
                    }
                }
            }

            // Poziome
            if (horizontalLines.Any())
            {
                double minY = closedShapes.Min(s => s.GetBoundingBox().Top);
                double maxY = closedShapes.Max(s => s.GetBoundingBox().Bottom);
                double centerY = (minY + maxY) / 2.0;

                if (horizontalLines.Count == 1)
                {
                    var line = horizontalLines.First();
                    line.Y1 = line.Y2 = centerY;
                }
                else
                {
                    double spacing = (maxY - minY) / (horizontalLines.Count + 1);
                    int i = 1;
                    foreach (var line in horizontalLines)
                    {
                        line.Y1 = line.Y2 = minY + i * spacing;
                        i++;
                    }
                }
            }
        }

        // 🔹 Przycinanie linii do wszystkich zamkniętych kształtów
        public static void ShortenLinesInsideShapes(List<IShapeDC> shapes, double _scaleFactor)
        {
            var closedShapes = shapes.Where(s => s is not XLineShape).ToList();
            var lines = shapes.OfType<XLineShape>().ToList();

            foreach (var line in lines)
            {
                foreach (var shape in closedShapes)
                {
                    var bbox = shape.GetBoundingBox();
                    ShortenLineToBoundingBox(line, bbox, _scaleFactor);

                    // Dodatkowo przycinanie do polygonów/skosów/dachu itd.
                    switch (shape)
                    {
                        case XCircleShape circle:
                            ShortenLineInsideCircle(line, circle);
                            break;
                        case XTriangleShape triangle:
                            ShortenLineInsidePolygon(line, triangle.GetVertices());
                            break;
                        case XHouseShape house:
                            ShortenLineInsideShape(line, house.GetBoundingBox());
                            ShortenLineToShape(line, house.GetEdges());
                            break;
                        case XRoundedTopRectangleShape rounded:
                            ShortenLineInsideEdges(line, rounded.GetEdges(),
                                new XPoint(rounded.X + rounded.Width / 2, rounded.Y + rounded.Radius), rounded.Radius);
                            break;
                        case XTrapezoidShape trap:
                            ShortenLineInsideShape(line, trap.GetBoundingBox());
                            ShortenLineToShape(line, trap.GetEdges());
                            break;
                        default:
                            ShortenLineInsideShape(line, shape.GetBoundingBox());
                            break;
                    }
                }
            }
        }

        #region --- PODSTAWOWE METODY GEOMETRYCZNE ---

        public static XLineShape ExtendLineToBoundingBox(XLineShape line, BoundingBox bbox, double _scaleFactor)
        {
            double x1 = line.X1, y1 = line.Y1, x2 = line.X2, y2 = line.Y2;
            double dx = x2 - x1, dy = y2 - y1;

            if (dx == 0)
            {
                return new XLineShape(x1, bbox.Y, x2, bbox.Y + bbox.Height, _scaleFactor, line.NazwaObj, line.RuchomySlupek, line.PionPoziom, line.DualRama, line.GenerowaneZRamy, line.StalySlupek);
            }
            if (dy == 0)
            {
                return new XLineShape(bbox.X, y1, bbox.X + bbox.Width, y2, _scaleFactor, line.NazwaObj, line.RuchomySlupek, line.PionPoziom, line.DualRama, line.GenerowaneZRamy, line.StalySlupek);
            }

            double leftFactor = (bbox.X - x1) / dx;
            double rightFactor = ((bbox.X + bbox.Width) - x1) / dx;
            double topFactor = (bbox.Y - y1) / dy;
            double bottomFactor = ((bbox.Y + bbox.Height) - y1) / dy;

            double minFactor = Math.Min(leftFactor, Math.Min(rightFactor, Math.Min(topFactor, bottomFactor)));
            double maxFactor = Math.Max(leftFactor, Math.Max(rightFactor, Math.Max(topFactor, bottomFactor)));

            double newX1 = x1 + dx * minFactor;
            double newY1 = y1 + dy * minFactor;
            double newX2 = x1 + dx * maxFactor;
            double newY2 = y1 + dy * maxFactor;

            return new XLineShape(newX1, newY1, newX2, newY2, _scaleFactor, line.NazwaObj, line.RuchomySlupek);
        }
        public static void ShortenLineToBoundingBox(XLineShape line, BoundingBox bbox, double _scaleFactor)
        {
            // można użyć tego samego co w ExtendLineToBoundingBox
            ExtendLineToBoundingBox(line, bbox, _scaleFactor);
        }

        public static void CheckEdgeIntersection(double x1, double y1, double x2, double y2,
                                                 XLineShape line, List<XPoint> intersections)
        {
            if (FindIntersection(x1, y1, x2, y2, line.X1, line.Y1, line.X2, line.Y2,
                                 out double ix, out double iy))
            {
                intersections.Add(new XPoint(ix, iy));
            }
        }

        public static bool FindIntersection(double aX1, double aY1, double aX2, double aY2,
                                            double bX1, double bY1, double bX2, double bY2,
                                            out double x, out double y)
        {
            x = 0; y = 0;
            double d = (aX1 - aX2) * (bY1 - bY2) - (aY1 - aY2) * (bX1 - bX2);
            if (Math.Abs(d) < Tolerance) return false;

            double t = ((aX1 - bX1) * (bY1 - bY2) - (aY1 - bY1) * (bX1 - bX2)) / d;
            double u = -((aX1 - aX2) * (aY1 - bY1) - (aY1 - aY2) * (aX1 - bX1)) / d;

            if (t < 0 || t > 1 || u < 0 || u > 1) return false;

            x = aX1 + t * (aX2 - aX1);
            y = aY1 + t * (aY2 - aY1);
            return true;
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        public static bool LineIntersectsBoundingBox(XLineShape line, BoundingBox bbox)
        {
            return !(line.X2 < bbox.X || line.X1 > bbox.X + bbox.Width ||
                     line.Y2 < bbox.Y || line.Y1 > bbox.Y + bbox.Height);
        }

        #endregion

        #region --- Placeholder dla przycinania do kształtów (do uzupełnienia wg RysOkna) ---

        public static void ShortenLineInsideCircle(XLineShape line, XCircleShape circle)
        {
            List<XPoint> intersections = new List<XPoint>();

            // Znajdź przecięcia linii z okręgiem
            FindCircleIntersections(circle, line, ref intersections);

            if (intersections.Count == 2)
            {
                line.X1 = intersections[0].X;
                line.Y1 = intersections[0].Y;
                line.X2 = intersections[1].X;
                line.Y2 = intersections[1].Y;
            }
            else if (intersections.Count == 1)
            {
                if (IsPointInsideCircle(line.X1, line.Y1, circle))
                {
                    line.X2 = intersections[0].X;
                    line.Y2 = intersections[0].Y;
                }
                else
                {
                    line.X1 = intersections[0].X;
                    line.Y1 = intersections[0].Y;
                }
            }
        }

        public static void ShortenLineInsidePolygon(XLineShape line, List<XPoint> polygonVertices)
        {
            List<XPoint> intersections = new List<XPoint>();

            int count = polygonVertices.Count;
            for (int i = 0; i < count; i++)
            {
                XPoint p1 = polygonVertices[i];
                XPoint p2 = polygonVertices[(i + 1) % count];

                if (FindIntersection(p1.X, p1.Y, p2.X, p2.Y, line.X1, line.Y1, line.X2, line.Y2, out double ix, out double iy))
                {
                    intersections.Add(new XPoint(ix, iy));
                }
            }

            intersections = intersections.Distinct()
                .OrderBy(p => Distance(line.X1, line.Y1, p.X, p.Y))
                .ToList();

            Console.WriteLine($"ShortenLineInsidePolygon -> intersections.Count: {intersections.Count}");

            if (intersections.Count == 2)
            {
                line.X1 = intersections[0].X;
                line.Y1 = intersections[0].Y;
                line.X2 = intersections[1].X;
                line.Y2 = intersections[1].Y;
            }
            else if (intersections.Count == 1)
            {
                if (IsPointInsidePolygon(line.X1, line.Y1, polygonVertices))
                {
                    line.X2 = intersections[0].X;
                    line.Y2 = intersections[0].Y;
                }
                else
                {
                    line.X1 = intersections[0].X;
                    line.Y1 = intersections[0].Y;
                }
            }
        }
        private static bool IsPointInsidePolygon(double x, double y, List<XPoint> polygonVertices)
        {
            int count = polygonVertices.Count;
            bool inside = false;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                double xi = polygonVertices[i].X, yi = polygonVertices[i].Y;
                double xj = polygonVertices[j].X, yj = polygonVertices[j].Y;

                bool intersect = ((yi > y) != (yj > y)) &&
                                 (x < (xj - xi) * (y - yi) / (yj - yi) + xi);
                if (intersect)
                    inside = !inside;
            }

            return inside;
        }
        public static void ShortenLineToShape(XLineShape line, List<(XPoint Start, XPoint End)> edges)
        {
            List<XPoint> intersections = new List<XPoint>();

            Console.WriteLine($"Sprawdzanie linii ({line.X1}, {line.Y1}) → ({line.X2}, {line.Y2})");

            // 1️⃣ Znajdź wszystkie przecięcia linii z krawędziami domu
            foreach (var (start, end) in edges)
            {
                if (FindIntersection(start.X, start.Y, end.X, end.Y,
                                     line.X1, line.Y1, line.X2, line.Y2,
                                     out double ix, out double iy))
                {
                    intersections.Add(new XPoint(ix, iy));
                }
            }

            // 2️⃣ Usuń duplikaty i posortuj przecięcia względem wysokości (Y)
            intersections = intersections
                .Distinct()
                .OrderBy(p => p.Y) // Sortujemy od góry do dołu
                .ToList();

            Console.WriteLine($"Znalezione przecięcia: {intersections.Count}");

            if (intersections.Count >= 2)
            {
                // 3️⃣ Przycinamy do najwyższego i najniższego przecięcia
                line.X1 = intersections[0].X;
                line.Y1 = intersections[0].Y;
                line.X2 = intersections[^1].X; // Ostatni element listy to dolne przecięcie (bottomY)
                line.Y2 = intersections[^1].Y;

                Console.WriteLine($"Linia obcięta do: ({line.X1}, {line.Y1}) → ({line.X2}, {line.Y2})");
            }
            else if (intersections.Count == 1)
            {
                List<XPoint> vertices = edges
                    .SelectMany(e => new[] { e.Start, e.End })
                    .Distinct()
                    .ToList();

                if (IsPointInsidePolygon(line.X1, line.Y1, vertices))
                {
                    // Jeśli linia zaczyna się wewnątrz kształtu – przycinamy koniec
                    line.X2 = intersections[0].X;
                    line.Y2 = intersections[0].Y;
                    Console.WriteLine($"Przycinam końcówkę do: ({line.X2}, {line.Y2})");
                }
                else
                {
                    // Jeśli linia zaczyna się na zewnątrz – przycinamy początek
                    line.X1 = intersections[0].X;
                    line.Y1 = intersections[0].Y;
                    Console.WriteLine($"Przycinam początek do: ({line.X1}, {line.Y1})");
                }
            }
            else
            {
                Console.WriteLine("Brak przecięć – linia pozostaje bez zmian.");
            }
        }
        public static void ShortenLineInsideShape(XLineShape line, BoundingBox shapeBox)
        {
            List<XPoint> intersections = new List<XPoint>();

            // Sprawdź przecięcia z krawędziami zamkniętego kształtu
            CheckEdgeIntersection(shapeBox.X, shapeBox.Y, shapeBox.X + shapeBox.Width, shapeBox.Y, line, intersections); // Góra
            CheckEdgeIntersection(shapeBox.X + shapeBox.Width, shapeBox.Y, shapeBox.X + shapeBox.Width, shapeBox.Y + shapeBox.Height, line, intersections); // Prawa
            CheckEdgeIntersection(shapeBox.X, shapeBox.Y + shapeBox.Height, shapeBox.X + shapeBox.Width, shapeBox.Y + shapeBox.Height, line, intersections); // Dół
            CheckEdgeIntersection(shapeBox.X, shapeBox.Y, shapeBox.X, shapeBox.Y + shapeBox.Height, line, intersections); // Lewa

            intersections = intersections.Distinct()
                .OrderBy(p => Distance(line.X1, line.Y1, p.X, p.Y))
                .ToList();

            if (intersections.Count == 2)
            {
                line.X1 = intersections[0].X;
                line.Y1 = intersections[0].Y;
                line.X2 = intersections[1].X;
                line.Y2 = intersections[1].Y;
            }
            else if (intersections.Count == 1)
            {
                if (shapeBox.Contains(line.X1, line.Y1))
                {
                    line.X2 = intersections[0].X;
                    line.Y2 = intersections[0].Y;
                }
                else
                {
                    line.X1 = intersections[0].X;
                    line.Y1 = intersections[0].Y;
                }
            }
        }
        public static void ShortenLineInsideEdges(XLineShape line, List<(XPoint Start, XPoint End)> edges, XPoint arcCenter, double radius)
        {
            List<XPoint> intersections = new List<XPoint>();

            // Sprawdzenie przecięć z prostymi krawędziami
            foreach (var edge in edges)
            {
                if (FindIntersection(edge.Start.X, edge.Start.Y, edge.End.X, edge.End.Y,
                                     line.X1, line.Y1, line.X2, line.Y2,
                                     out double ix, out double iy))
                {
                    if (IsPointOnSegment(edge.Start, edge.End, new XPoint(ix, iy)))
                    {
                        intersections.Add(new XPoint(ix, iy));
                    }
                }
            }

            // Sprawdzenie przecięć z łukiem
            List<XPoint> arcIntersections = FindCircleLineIntersections(arcCenter, radius, line);
            intersections.AddRange(arcIntersections);

            Console.WriteLine($"Znalezione przecięcia (po filtracji): {intersections.Count}");
            foreach (var p in intersections)
            {
                Console.WriteLine($"Punkt przecięcia: ({p.X}, {p.Y})");
            }

            intersections = intersections.Distinct()
                .OrderBy(p => Distance(line.X1, line.Y1, p.X, p.Y))
                .ToList();

            if (intersections.Count == 2)
            {
                Console.WriteLine($"Przycinam linię do dwóch przecięć: ({intersections[0].X}, {intersections[0].Y}) → ({intersections[1].X}, {intersections[1].Y})");
                line.X1 = intersections[0].X;
                line.Y1 = intersections[0].Y;
                line.X2 = intersections[1].X;
                line.Y2 = intersections[1].Y;
            }
            else if (intersections.Count == 1)
            {
                if (IsPointInsideEdges(line.X1, line.Y1, edges, arcCenter, radius))
                {
                    Console.WriteLine($"Przycinam linię do punktu ({intersections[0].X}, {intersections[0].Y}) na końcu.");
                    line.X2 = intersections[0].X;
                    line.Y2 = intersections[0].Y;
                }
                else
                {
                    Console.WriteLine($"Przycinam linię do punktu ({intersections[0].X}, {intersections[0].Y}) na początku.");
                    line.X1 = intersections[0].X;
                    line.Y1 = intersections[0].Y;
                }
            }
        }
        private static bool IsPointInsideEdges(double x, double y, List<(XPoint Start, XPoint End)> edges, XPoint? arcCenter = null, double arcRadius = 0)
        {
            int intersections = 0;

            foreach (var edge in edges)
            {
                double x1 = edge.Start.X, y1 = edge.Start.Y;
                double x2 = edge.End.X, y2 = edge.End.Y;

                // Sprawdzamy, czy pozioma linia przecina dany odcinek
                if ((y1 > y) != (y2 > y))
                {
                    double intersectX = x1 + (y - y1) * (x2 - x1) / (y2 - y1);
                    if (intersectX > x)
                    {
                        intersections++;
                    }
                }
            }

            // Jeśli mamy łuk – sprawdzamy, czy punkt jest wewnątrz okręgu
            if (arcCenter.HasValue)
            {
                double dx = x - arcCenter.Value.X;
                double dy = y - arcCenter.Value.Y;
                double distanceSquared = dx * dx + dy * dy;

                if (distanceSquared <= arcRadius * arcRadius)
                {
                    return true; // Punkt jest wewnątrz łuku
                }
            }

            return (intersections % 2) == 1; // Jeśli liczba przecięć jest nieparzysta, punkt jest wewnątrz
        }
        private static bool IsPointOnSegment(XPoint start, XPoint end, XPoint point)
        {
            double crossProduct = (point.Y - start.Y) * (end.X - start.X) - (point.X - start.X) * (end.Y - start.Y);
            if (Math.Abs(crossProduct) > 0.001) return false; // Punkt nie leży dokładnie na linii

            double dotProduct = (point.X - start.X) * (end.X - start.X) + (point.Y - start.Y) * (end.Y - start.Y);
            if (dotProduct < 0) return false; // Punkt leży przed startem

            double squaredLength = (end.X - start.X) * (end.X - start.X) + (end.Y - start.Y) * (end.Y - start.Y);
            if (dotProduct > squaredLength) return false; // Punkt leży za końcem

            return true; // Punkt leży na odcinku
        }
        private static List<XPoint> FindCircleLineIntersections(XPoint center, double radius, XLineShape line)
        {
            List<XPoint> intersections = new List<XPoint>();

            double dx = line.X2 - line.X1;
            double dy = line.Y2 - line.Y1;
            double fx = line.X1 - center.X;
            double fy = line.Y1 - center.Y;

            double a = dx * dx + dy * dy;
            double b = 2 * (fx * dx + fy * dy);
            double c = (fx * fx + fy * fy) - (radius * radius);

            double discriminant = b * b - 4 * a * c;

            if (discriminant < 0) return intersections; // Brak przecięć

            discriminant = Math.Sqrt(discriminant);
            double t1 = (-b - discriminant) / (2 * a);
            double t2 = (-b + discriminant) / (2 * a);

            if (t1 >= 0 && t1 <= 1)
            {
                double ix = line.X1 + t1 * dx;
                double iy = line.Y1 + t1 * dy;
                if (iy <= center.Y) // Punkt musi być wewnątrz górnego łuku
                {
                    intersections.Add(new XPoint(ix, iy));
                }
            }

            if (t2 >= 0 && t2 <= 1)
            {
                double ix = line.X1 + t2 * dx;
                double iy = line.Y1 + t2 * dy;
                if (iy <= center.Y) // Punkt musi być wewnątrz górnego łuku
                {
                    intersections.Add(new XPoint(ix, iy));
                }
            }

            return intersections;
        }
        private static void FindCircleIntersections(XCircleShape circle, XLineShape line, ref List<XPoint> intersections)
        {
            double dx = line.X2 - line.X1;
            double dy = line.Y2 - line.Y1;
            double fx = line.X1 - circle.X;
            double fy = line.Y1 - circle.Y;

            double a = dx * dx + dy * dy;
            double b = 2 * (fx * dx + fy * dy);
            double c = (fx * fx + fy * fy) - (circle.Radius * circle.Radius);

            double discriminant = b * b - 4 * a * c;

            if (discriminant < 0) return; // Brak przecięć

            discriminant = Math.Sqrt(discriminant);
            double t1 = (-b - discriminant) / (2 * a);
            double t2 = (-b + discriminant) / (2 * a);

            if (t1 >= 0 && t1 <= 1)
                intersections.Add(new XPoint(line.X1 + t1 * dx, line.Y1 + t1 * dy));

            if (t2 >= 0 && t2 <= 1)
                intersections.Add(new XPoint(line.X1 + t2 * dx, line.Y1 + t2 * dy));
        }
        private static bool IsPointInsideCircle(double x, double y, XCircleShape circle)
        {
            double dx = x - circle.X;
            double dy = y - circle.Y;
            return (dx * dx + dy * dy) <= (circle.Radius * circle.Radius);
        }

        #endregion
    }
}
