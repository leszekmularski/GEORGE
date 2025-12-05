using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    public class XTrapezoidShape : IShapeDC
    {
        public double BaseWidth { get; set; }
        public double TopWidth { get; set; }
        public double Height { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string NazwaObj { get; set; } = "Trapezoid";
        private int Typ { get; set; }

        private double _scaleFactor = 1.0; // Początkowa skala = 1.0 (bez skalowania)
        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }
        public List<XPoint> Points { get; set; }
        public List<XPoint> NominalPoints { get; set; } = new();
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public List<XPoint> GetPoints() => Points;
        public List<XPoint> GetNominalPoints() => NominalPoints;
        // Konstruktor przyjmujący współrzędne i współczynnik szerokości góry
        public XTrapezoidShape(double startX, double startY, double endX, double endY, double topWidthFactor, double scaleFactor, int typ)
        {
            X = Math.Min(startX, endX);           // Ustal minimalne X (lewy brzeg)
            Y = Math.Min(startY, endY);           // Ustal minimalne Y (dolny brzeg)
            BaseWidth = Math.Abs(endX - startX);  // Oblicz szerokość podstawy trapezu
            Height = Math.Abs(endY - startY);     // Oblicz wysokość trapezu
            TopWidth = Math.Min(BaseWidth, BaseWidth * topWidthFactor);  // Oblicz szerokość góry trapezu
            _scaleFactor = scaleFactor;
            Typ = typ;

            Points = GeneratePoints();

            NominalPoints = Points.Select(p => new XPoint(p.X, p.Y)).ToList();
        }

        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 4)
                return;

            Points = newPoints;

            // KOPIA do nominal
            NominalPoints = newPoints.Select(p => new XPoint(p.X, p.Y)).ToList();

            switch (Typ)
            {
                case 0: UpdateStandardTrapezoid(Points); break;
                case 1: UpdateLeftVerticalTrapezoid(Points); break;
                case 2: UpdateRightVerticalTrapezoid(Points); break;
            }

            Szerokosc = BaseWidth;
            Wysokosc = Height;
        }


        // Metody pomocnicze dla każdego typu trapezu
        private void UpdateStandardTrapezoid(List<XPoint> points)
        {
            // Punkty powinny być w kolejności: lewy górny, prawy górny, prawy dolny, lewy dolny
            X = points[3].X; // Lewy dolny X
            Y = points[0].Y; // Górna krawędź Y

            BaseWidth = points[2].X - points[3].X; // Szerokość podstawy
            TopWidth = points[1].X - points[0].X;  // Szerokość góry
            Height = points[2].Y - points[0].Y;    // Wysokość

            // Sprawdzenie spójności
            if (Math.Abs(points[0].Y - points[1].Y) > 0.1 ||
                Math.Abs(points[2].Y - points[3].Y) > 0.1)
            {
                // Jeśli górna lub dolna krawędź nie jest pozioma, dostosowujemy Y
                Y = Math.Min(points[0].Y, points[1].Y);
                double baseY = Math.Max(points[2].Y, points[3].Y);
                Height = baseY - Y;
            }
        }

        private void UpdateLeftVerticalTrapezoid(List<XPoint> points)
        {
            // Punkty w kolejności: lewy górny, prawy górny, prawy dolny, lewy dolny
            X = points[0].X; // Lewa pionowa ściana X
            Y = points[0].Y; // Górna krawędź Y

            BaseWidth = points[2].X - points[3].X; // Szerokość podstawy
            TopWidth = points[1].X - points[0].X;  // Szerokość góry
            Height = points[2].Y - points[0].Y;    // Wysokość

            // Weryfikacja pionowej lewej ściany
            if (Math.Abs(points[0].X - points[3].X) > 0.1)
            {
                // Jeśli lewa ściana nie jest pionowa, korygujemy
                X = (points[0].X + points[3].X) / 2;
            }
        }

        private void UpdateRightVerticalTrapezoid(List<XPoint> points)
        {
            // Punkty w kolejności: lewy górny, prawy górny, prawy dolny, lewy dolny
            X = points[3].X; // Lewy dolny X (podstawa)
            Y = points[0].Y; // Górna krawędź Y

            BaseWidth = points[2].X - points[3].X; // Szerokość podstawy
            TopWidth = points[1].X - points[0].X;  // Szerokość góry
            Height = points[2].Y - points[0].Y;    // Wysokość

            // Weryfikacja pionowej prawej ściany
            if (Math.Abs(points[1].X - points[2].X) > 0.1)
            {
                // Jeśli prawa ściana nie jest pionowa, korygujemy
                double rightX = (points[1].X + points[2].X) / 2;
                BaseWidth = rightX - X;
            }
        }
        public IShapeDC Clone()
        {
            var clone = new XTrapezoidShape(X, Y, X + BaseWidth, Y + Height,
                BaseWidth != 0 ? TopWidth / BaseWidth : 1.0,
                _scaleFactor, Typ);

            // KLONOWANIE NOMINALNYCH PUNKTÓW
            clone.NominalPoints = NominalPoints.Select(p => new XPoint(p.X, p.Y)).ToList();

            return clone;
        }

        // Metoda rysująca trapez
        public async Task Draw(Canvas2DContext ctx)
        {
            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            if (Typ == 0)
            {
                // Oblicz pozycje wierzchołków trapezu
                var baseLeft = X;
                var baseRight = X + BaseWidth;
                var topLeft = X + (BaseWidth - TopWidth) / 2;   // Oblicz pozycję lewej strony góry
                var topRight = topLeft + TopWidth;               // Oblicz pozycję prawej strony góry
                var verticalY = Y + Height;                      // Oblicz wysokość (dolna krawędź)

                // Rysowanie trapezu za pomocą punktów
                await ctx.BeginPathAsync();
                await ctx.MoveToAsync(topLeft, Y);            // Lewy górny
                await ctx.LineToAsync(topRight, Y);           // Prawy górny
                await ctx.LineToAsync(baseRight, verticalY);  // Prawy dolny
                await ctx.LineToAsync(baseLeft, verticalY);   // Lewy dolny
                await ctx.ClosePathAsync();

                await ctx.StrokeAsync();
            }
            else if (Typ == 1)
            {
                // Oblicz pozycje wierzchołków trapezu
                var baseLeft = X;
                var baseRight = X + BaseWidth;
                var topLeft = X + (BaseWidth - TopWidth) / 2;   // Oblicz pozycję lewej strony góry
                var topRight = topLeft + TopWidth;               // Oblicz pozycję prawej strony góry
                var verticalY = Y + Height;                      // Oblicz wysokość (dolna krawędź)

                // Rysowanie trapezu za pomocą punktów
                await ctx.BeginPathAsync();
                await ctx.MoveToAsync(baseLeft, Y);            // Lewy górny
                await ctx.LineToAsync(topRight, Y);           // Prawy górny
                await ctx.LineToAsync(baseRight, verticalY);  // Prawy dolny
                await ctx.LineToAsync(baseLeft, verticalY);   // Lewy dolny
                await ctx.ClosePathAsync();

                await ctx.StrokeAsync();
            }
            else if (Typ == 2)
            {
                // Oblicz pozycje wierzchołków trapezu
                var baseLeft = X;
                var baseRight = X + BaseWidth;
                var topLeft = X + (BaseWidth - TopWidth) / 2;   // Oblicz pozycję lewej strony góry
                var topRight = topLeft + TopWidth;               // Oblicz pozycję prawej strony góry
                var verticalY = Y + Height;                      // Oblicz wysokość (dolna krawędź)

                // Rysowanie trapezu za pomocą punktów
                await ctx.BeginPathAsync();
                await ctx.MoveToAsync(topLeft, Y);            // Lewy górny
                await ctx.LineToAsync(baseRight, Y);           // Prawy górny
                await ctx.LineToAsync(baseRight, verticalY);  // Prawy dolny
                await ctx.LineToAsync(baseLeft, verticalY);   // Lewy dolny
                await ctx.ClosePathAsync();

                await ctx.StrokeAsync();
            }

        }

        // Właściwości edytowalne
        public List<EditableProperty> GetEditableProperties() => new()
    {
        new("Pozycja X", () => X, v => X = v, NazwaObj, true),
        new("Pozycja Y", () => Y, v => Y = v, NazwaObj, true),
        new("Szerokość podstawy", () => BaseWidth, v => BaseWidth = v, NazwaObj),
        new("Szerokość góry", () => TopWidth, v => TopWidth = Math.Clamp(v, 10, BaseWidth - 10), NazwaObj),
        new("Wysokość", () => Height, v => Height = v, NazwaObj)
    };

        public void Scale(double factor)
        {
            BaseWidth *= factor;
            TopWidth *= factor;
            Height *= factor;
            X -= (BaseWidth * (factor - 1)) / 2; // Przesuwamy trapez w lewo, aby zachować środek
            Y -= (Height * (factor - 1)) / 2; // Przesuwamy go w górę
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(X, Y, BaseWidth, Height, "Trapezoid");
        }

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var baseLeft = X;
            var baseRight = X + BaseWidth;
            var topLeft = X + (BaseWidth - TopWidth) / 2;
            var topRight = topLeft + TopWidth;
            var verticalY = Y + Height;

            if (Typ == 0)
            {
                return new List<(XPoint, XPoint)>
        {
            (new XPoint(topLeft, Y), new XPoint(topRight, Y)),         // Górna krawędź trapezu
            (new XPoint(topRight, Y), new XPoint(baseRight, verticalY)), // Prawa skośna krawędź
            (new XPoint(baseRight, verticalY), new XPoint(baseLeft, verticalY)), // Dolna krawędź
            (new XPoint(baseLeft, verticalY), new XPoint(topLeft, Y))  // Lewa skośna krawędź
        };
            }
            else if (Typ == 1)
            {
                return new List<(XPoint, XPoint)>
        {
            (new XPoint(baseLeft, Y), new XPoint(topRight, Y)),        // Górna krawędź trapezu
            (new XPoint(topRight, Y), new XPoint(baseRight, verticalY)), // Prawa skośna krawędź
            (new XPoint(baseRight, verticalY), new XPoint(baseLeft, verticalY)), // Dolna krawędź
            (new XPoint(baseLeft, verticalY), new XPoint(baseLeft, Y))  // Lewa pionowa krawędź
        };
            }
            else if (Typ == 2)
            {
                return new List<(XPoint, XPoint)>
        {
            (new XPoint(topLeft, Y), new XPoint(baseRight, Y)),         // Górna krawędź trapezu
            (new XPoint(baseRight, Y), new XPoint(baseRight, verticalY)), // Prawa pionowa krawędź
            (new XPoint(baseRight, verticalY), new XPoint(baseLeft, verticalY)), // Dolna krawędź
            (new XPoint(baseLeft, verticalY), new XPoint(topLeft, Y))  // Lewa skośna krawędź
        };
            }

            return new List<(XPoint, XPoint)>();
        }

        /// <summary>
        /// Zwraca cztery wierzchołki trapezu zgodnie z aktualnym typem trapezu.
        /// </summary>
        public List<XPoint> GetVertices()
        {
            var baseLeft = X;
            var baseRight = X + BaseWidth;
            var topLeft = X + (BaseWidth - TopWidth) / 2;
            var topRight = topLeft + TopWidth;
            var verticalY = Y + Height;

            if (Typ == 0)
            {
                Console.WriteLine("Zwracam wierzchołki trapezu typu 0");
                return new List<XPoint>
        {
            new XPoint(topLeft, Y),       // Lewy górny
            new XPoint(topRight, Y),      // Prawy górny
            new XPoint(baseRight, verticalY), // Prawy dolny
            new XPoint(baseLeft, verticalY)   // Lewy dolny
        };
            }
            else if (Typ == 1)
            {
                Console.WriteLine("Zwracam wierzchołki trapezu typu 1");
                return new List<XPoint>
        {
            new XPoint(baseLeft, Y),       // Lewy górny (podstawa zaczyna się nisko)
            new XPoint(topRight, Y),        // Prawy górny
            new XPoint(baseRight, verticalY), // Prawy dolny
            new XPoint(baseLeft, verticalY)   // Lewy dolny
        };
            }
            else if (Typ == 2)
            {
                Console.WriteLine("Zwracam wierzchołki trapezu typu 2");
                return new List<XPoint>
        {
            new XPoint(topLeft, Y),       // Lewy górny
            new XPoint(baseRight, Y),      // Prawy górny (cała szerokość podstawy)
            new XPoint(baseRight, verticalY), // Prawy dolny
            new XPoint(baseLeft, verticalY)   // Lewy dolny
        };
            }
            else
            {
                // Jeśli Typ jest nieznany - zwróć pustą listę
                Console.WriteLine("Nieznany typ trapezu, zwracam pustą listę wierzchołków.");
                return new List<XPoint>();
            }
        }


        public void Transform(double scale, double offsetX, double offsetY)
        {
            X = (X * scale) + offsetX;
            Y = (Y * scale) + offsetY;
            BaseWidth *= scale;
            TopWidth *= scale;
            Height *= scale;
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X = (X * scaleX) + offsetX;
            Y = (Y * scaleY) + offsetY;
            BaseWidth *= scaleX;
            TopWidth *= scaleX;
            Height *= scaleY;
        }

        private List<XPoint> GeneratePoints()
        {
            var baseLeft = X;
            var baseRight = X + BaseWidth;
            var topLeft = X + (BaseWidth - TopWidth) / 2;
            var topRight = topLeft + TopWidth;
            var verticalY = Y + Height;

            return Typ switch
            {
                0 => new List<XPoint>     // standard
        {
            new XPoint(topLeft, Y),
            new XPoint(topRight, Y),
            new XPoint(baseRight, verticalY),
            new XPoint(baseLeft, verticalY)
        },

                1 => new List<XPoint>     // lewa pionowa
        {
            new XPoint(baseLeft, Y),
            new XPoint(topRight, Y),
            new XPoint(baseRight, verticalY),
            new XPoint(baseLeft, verticalY)
        },

                2 => new List<XPoint>     // prawa pionowa
        {
            new XPoint(topLeft, Y),
            new XPoint(baseRight, Y),
            new XPoint(baseRight, verticalY),
            new XPoint(baseLeft, verticalY)
        },

                _ => new List<XPoint>()
            };
        }


    }

}
