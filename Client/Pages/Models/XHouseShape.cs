using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    public class XHouseShape : IShapeDC
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double HeightLeft { get; set; }
        public double HeightRight { get; set; }

        private double _scaleFactor;
        public string NazwaObj { get; set; } = "Domek";
        public double Szerokosc { get => Width; set => Width = value; }
        public double Wysokosc { get => Height; set => Height = value; }
        public List<XPoint> Points { get; set; } = new List<XPoint>();
        public List<XPoint> NominalPoints { get; set; } = new();
        public string ID { get; set; } = Guid.NewGuid().ToString();

        public XHouseShape(double x, double y, double width, double height,
                  double heightLeft, double heightRight, double scaleFactor)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            HeightLeft = heightLeft;
            HeightRight = heightRight;
            _scaleFactor = scaleFactor;

            // Wygeneruj punkty nominalne
            UpdatePoints();
            GenerateNominalPoints();
        }

        // Generuje punkty nominalne dla domku (5 punktów)
        private void GenerateNominalPoints()
        {
            double baseYL = Y + Height - Math.Max(HeightLeft, 0);
            double baseYR = Y + Height - Math.Max(HeightRight, 0);
            double roofPeakX = X + Width / 2;
            double roofPeakY = Y;
            double bottomY = Y + Height;

            NominalPoints = new List<XPoint>
            {
                new XPoint(X, baseYL),         // Lewy dolny róg dachu (punkt 0)
                new XPoint(roofPeakX, roofPeakY), // Szczyt dachu (punkt 1)
                new XPoint(X + Width, baseYR),  // Prawy dolny róg dachu (punkt 2)
                new XPoint(X + Width, bottomY), // Prawy dolny róg ściany (punkt 3)
                new XPoint(X, bottomY)         // Lewy dolny róg ściany (punkt 4)
            };

            Console.WriteLine($"Generated NominalPoints: {NominalPoints.Count} points");

            for (int i = 0; i < NominalPoints.Count; i++)
            {
                Console.WriteLine($"  Point {i}: X={NominalPoints[i].X}, Y={NominalPoints[i].Y}");
            }
        }

        // Generuje punkty przeskalowane dla canvas
        private void GenerateScaledPoints()
        {
            // Funkcja pomocnicza do zaokrąglania
            double Round(double value) => Math.Round(value, 3, MidpointRounding.AwayFromZero);

            Points = NominalPoints
                .Select(p => new XPoint(Round(p.X * _scaleFactor), Round(p.Y * _scaleFactor)))
                .ToList();

        }

        public void UpdatePoints(List<XPoint> newPoints = null)
        {
            if (newPoints != null && newPoints.Count >= 5)
            {
                // Aktualizuj właściwości na podstawie nowych punktów (nominalnych)
                X = newPoints[0].X;
                Y = newPoints.Min(p => p.Y);

                double minX = newPoints.Min(p => p.X);
                double maxX = newPoints.Max(p => p.X);
                Width = maxX - minX;

                double maxY = newPoints.Max(p => p.Y);
                Height = maxY - Y;

                // Oblicz wysokości boków
                HeightLeft = newPoints[4].Y - newPoints[0].Y; // Lewa ściana: punkt 4 - punkt 0
                HeightRight = newPoints[3].Y - newPoints[2].Y; // Prawa ściana: punkt 3 - punkt 2
            }

            // Po aktualizacji właściwości, generuj punkty
            GenerateNominalPoints();
            GenerateScaledPoints();
        }

        public IShapeDC Clone()
        {
            var clone = new XHouseShape(X, Y, Width, Height, HeightLeft, HeightRight, _scaleFactor)
            {
                NazwaObj = this.NazwaObj,
                // Kopiuj punkty, a nie referencje
                Points = this.Points.Select(p => new XPoint(p.X, p.Y)).ToList(),
                NominalPoints = this.NominalPoints.Select(p => new XPoint(p.X, p.Y)).ToList()
            };
            return clone;
        }

        public List<XPoint> GetFullOutline()
        {
            return GetNominalPoints();
        }

        public async Task Draw(Canvas2DContext ctx)
        {
            if (Points == null || Points.Count < 5) return;

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();

            // Rysuj dach
            await ctx.MoveToAsync(Points[0].X, Points[0].Y);  // Lewy dolny róg dachu
            await ctx.LineToAsync(Points[1].X, Points[1].Y);  // Szczyt dachu
            await ctx.LineToAsync(Points[2].X, Points[2].Y);  // Prawy dolny róg dachu

            // Rysuj ściany
            await ctx.LineToAsync(Points[3].X, Points[3].Y);  // Prawy dolny róg ściany
            await ctx.LineToAsync(Points[4].X, Points[4].Y);  // Lewy dolny róg ściany
            await ctx.LineToAsync(Points[0].X, Points[0].Y);  // Zamknij kształt

            await ctx.ClosePathAsync();
            await ctx.StrokeAsync();
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(X, Y, Width, Height, NazwaObj);
        }

        public List<EditableProperty> GetEditableProperties() => new()
        {
            new EditableProperty("X", () => X, v => {
                X = v;
                UpdatePoints();
            }, NazwaObj, true),
            new EditableProperty("Y", () => Y, v => {
                Y = v;
                UpdatePoints();
            }, NazwaObj, true),
            new EditableProperty("Szerokość", () => Width, v => {
                Width = v;
                UpdatePoints();
            }, NazwaObj),
            new EditableProperty("Wysokość", () => Height, v => {
                Height = v;
                UpdatePoints();
            }, NazwaObj),
            new EditableProperty("Wysokość bok lewy", () => HeightLeft, v => {
                HeightLeft = v;
                UpdatePoints();
            }, NazwaObj),
            new EditableProperty("Wysokość bok prawy", () => HeightRight, v => {
                HeightRight = v;
                UpdatePoints();
            }, NazwaObj)
        };

        public void Scale(double factor)
        {
            Width *= factor;
            Height *= factor;
            HeightRight *= factor;
            HeightLeft *= factor;
            UpdatePoints();
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
            UpdatePoints();
        }

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            if (NominalPoints.Count < 5)
                return new List<(XPoint, XPoint)>();

            return new List<(XPoint, XPoint)>
            {
                (NominalPoints[0], NominalPoints[1]), // Lewa krawędź dachu
                (NominalPoints[1], NominalPoints[2]), // Prawa krawędź dachu
                (NominalPoints[0], NominalPoints[4]), // Lewa ściana
                (NominalPoints[2], NominalPoints[3]), // Prawa ściana
                (NominalPoints[3], NominalPoints[4])  // Podstawa
            };
        }

        public (List<XPoint> Roof, List<XPoint> House) GetVertices()
        {
            if (NominalPoints.Count < 5)
                return (new List<XPoint>(), new List<XPoint>());

            var roof = new List<XPoint>
            {
                NominalPoints[0],
                NominalPoints[1],
                NominalPoints[2]
            };

            var house = new List<XPoint>
            {
                NominalPoints[3],
                NominalPoints[4]
            };

            return (roof, house);
        }

        public void Transform(double scale, double offsetX, double offsetY)
        {
            X = (X * scale) + offsetX;
            Y = (Y * scale) + offsetY;
            Width *= scale;
            Height *= scale;
            HeightLeft *= scale;
            HeightRight *= scale;
            UpdatePoints();
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X = (X * scaleX) + offsetX;
            Y = (Y * scaleY) + offsetY;
            Width *= scaleX;
            Height *= scaleY;
            HeightLeft *= scaleY;
            HeightRight *= scaleY;
            UpdatePoints();
        }

        public (List<(XPoint Start, XPoint End)> RoofEdges, List<(XPoint Start, XPoint End)> BaseEdges) GetEdgesDel()
        {
            double roofHeight = Height * 0.5;
            double baseY = Y + roofHeight;
            double roofPeakX = X + Width / 2;
            double roofPeakY = Y;
            double bottomY = Y + Height;

            List<(XPoint, XPoint)> roofEdges = new()
        {
            (new XPoint(X, baseY), new XPoint(roofPeakX, roofPeakY)), // Dach - lewa krawędź
            (new XPoint(roofPeakX, roofPeakY), new XPoint(X + Width, baseY)), // Dach - prawa krawędź
           // (new XPoint(X, baseY), new XPoint(X + Width, baseY)) // Podstawa dachu
        };

            List<(XPoint, XPoint)> baseEdges = new()
        {
            (new XPoint(X, baseY), new XPoint(X, bottomY)), // Lewa ściana domu
            (new XPoint(X + Width, baseY), new XPoint(X + Width, bottomY)), // Prawa ściana domu
            (new XPoint(X, bottomY), new XPoint(X + Width, bottomY)) // Podstawa domu
        };

            return (roofEdges, baseEdges);
        }

        public List<XPoint> GetPoints() => Points.Select(p => new XPoint(p.X, p.Y)).ToList();

        public List<XPoint> GetNominalPoints() => NominalPoints.Select(p => new XPoint(p.X, p.Y)).ToList();
    }
}