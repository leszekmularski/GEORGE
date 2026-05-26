using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    public class XRoundedTopRectangleShape : IShapeDC
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();

        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private double _radius;
        private double _arcHeight;
        private int _iloscElementowLuki = 4;

        public double X
        {
            get => _x;
            set => _x = value;
        }

        public double Y
        {
            get => _y;
            set => _y = value;
        }

        public double Width
        {
            get => _width;
            set => _width = Math.Max(100, value);
        }

        public double Height
        {
            get => _height;
            set => _height = Math.Max(100, value);
        }

        public double Radius
        {
            get => _radius;
            set => _radius = value;
        }

        public double ArcHeight
        {
            get => _arcHeight;
            set => _arcHeight = value;
        }

        public double Szerokosc { get => Width; set => Width = value; }
        public double Wysokosc { get => Height; set => Height = value; }

        public List<XPoint> Points { get; set; } = new();
        public List<XPoint> NominalPoints { get; set; } = new();

        public List<XPoint> GetPoints() => Points;
        public List<XPoint> GetNominalPoints() => NominalPoints.Select(p => p.Clone()).ToList();

        private readonly double _scaleFactor;
        public string NazwaObj { get; set; } = "Prostokąt z wypukłym łukiem u góry";

        public int IloscElementowLuki
        {
            get => _iloscElementowLuki;
            set
            {
                _iloscElementowLuki = Math.Max(4, value);
                GenerateCompleteOutline(_iloscElementowLuki);
            }
        }

        public List<ContourSegment> ContourSegments => GetContourSegments();

        // Flaga zapobiegająca rekurencji podczas inicjalizacji
        private bool _isInitializing = false;

        // Cache dla CalculateArcGeometry
        private (double centerX, double centerY, double startAngle, double endAngle)? _cachedArcGeometry;
        private double _lastWidth, _lastHeight, _lastArcHeight, _lastX, _lastY;
        private bool _geometryDirty = true;

        // W XRoundedTopRectangleShape.cs dodaj konstruktor domyślny

        public XRoundedTopRectangleShape(double x, double y, double width, double height,
                          double radius = 0, double arcHeight = 0, double scaleFactor = 1.0)
        {
          //  _isInitializing = true;

            Console.WriteLine($"[DEBUG] Konstruktor START {NazwaObj}: Height={height}, ArcHeight={arcHeight}");

            _x = x;
            _y = y;
            _width = Math.Max(100, width);
            _height = Math.Max(100, height);
            _scaleFactor = scaleFactor;

            // Ustaw ArcHeight
            if (arcHeight <= 0)
                _arcHeight = Height / 4;  // Domyślnie 1/4 wysokości
            else
                _arcHeight = Math.Clamp(arcHeight, 5, Height - 10);  // Min 5, max Height-10

            // ZAWSZE obliczaj promień z geometrii (ignoruj parametr radius)
            // R = (w² + 4h²) / (8h)
            _radius = (Width * Width + 4 * ArcHeight * ArcHeight) / (8 * ArcHeight);

            // Upewnij się, że promień jest wystarczający
            _radius = Math.Max(_radius, Width / 2);

            CalculatePointsFromProperties();

            //_isInitializing = false;

            Console.WriteLine($"[DEBUG] Konstruktor END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");
        }

        // ===========================
        // Oblicz punkty zgodne z Canvas
        // ===========================
        private void CalculatePointsFromProperties()
        {
            if (_isInitializing) return;

            Console.WriteLine($"[DEBUG] CalculatePointsFromProperties START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");
            Points = GenerateCompleteOutline(IloscElementowLuki);
            NormalizeToPositiveQuadrant();
            NominalPoints = Points.Select(p => p.Clone()).ToList();

            // Oznacz geometrię jako brudną po zmianie właściwości
            MarkGeometryDirty();

            Console.WriteLine($"[DEBUG] CalculatePointsFromProperties END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");
        }

        // ===========================
        // Oblicz środek łuku i kąty start/end z cache'owaniem
        // ===========================
        public (double centerX, double centerY, double startAngle, double endAngle) CalculateArcGeometry()
        {
            // Jeśli inicjalizacja, zwróć domyślną wartość
            if (_isInitializing)
            {
                return (X + Width / 2, Y + ArcHeight, 0, Math.PI);
            }

            // Sprawdź cache
            if (!_geometryDirty &&
                _lastWidth == Width &&
                _lastHeight == Height &&
                _lastArcHeight == ArcHeight &&
                _lastX == X &&
                _lastY == Y &&
                _cachedArcGeometry.HasValue)
            {
                return _cachedArcGeometry.Value;
            }

            Console.WriteLine($"[DEBUG] CalculateArcGeometry START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            double leftX = X;
            double rightX = X + Width;
            double topY = Y;
            double arcBaseY = Y + ArcHeight;

            (double centerX, double centerY, double startAngle, double endAngle) result;

            // Przypadek standardowy: łuk półokrągły gdy ArcHeight >= Height
            if (arcBaseY < Y + Height)
            {
                result = CalculateArcGeometryD();
            }
            else if (ArcHeight * 2 < Width && Height > Width)
            {
                result = CalculateArcGeometryByArcHeight();
            }
            else
            {
                var (cx, cy, r) = CircleFromThreePoints(
                    leftX, arcBaseY,
                    X + Width / 2.0, topY,
                    rightX, arcBaseY
                );

                _radius = r;

                // Kąty do punktów
                double angleRight = Math.Atan2(arcBaseY - cy, rightX - cx);
                double angleLeft = Math.Atan2(arcBaseY - cy, leftX - cx);
                double angleTop = Math.Atan2(topY - cy, (X + Width / 2.0) - cx);

                // Normalizacja
                if (angleRight < 0) angleRight += 2 * Math.PI;
                if (angleLeft < 0) angleLeft += 2 * Math.PI;
                if (angleTop < 0) angleTop += 2 * Math.PI;

                // Sprawdzamy, czy środek jest nad czy pod łukiem
                bool centerBelow = cy > topY;

                double startAngle, endAngle;

                if (centerBelow)
                {
                    startAngle = angleRight;
                    endAngle = angleLeft;
                    if (Math.Abs(startAngle - endAngle) > Math.PI)
                    {
                        if (startAngle < endAngle)
                            startAngle += 2 * Math.PI;
                        else
                            endAngle += 2 * Math.PI;
                    }
                }
                else
                {
                    startAngle = angleLeft;
                    endAngle = angleRight;
                    if (startAngle > endAngle)
                        endAngle += 2 * Math.PI;
                }

                result = (cx, cy, startAngle, endAngle);
            }

            Console.WriteLine($"[DEBUG] CalculateArcGeometry {NazwaObj}: Height={Height}, ArcHeight={ArcHeight} Radius: {Radius}");
            Console.WriteLine($"[DEBUG] CalculateArcGeometry END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            // Zapisz w cache
            _cachedArcGeometry = result;
            _lastWidth = Width;
            _lastHeight = Height;
            _lastArcHeight = ArcHeight;
            _lastX = X;
            _lastY = Y;
            _geometryDirty = false;

            return result;
        }

        // Oznacz geometrię jako wymagającą przeliczenia
        private void MarkGeometryDirty()
        {
            _geometryDirty = true;
        }

        public (double centerX, double centerY, double startAngle, double endAngle) CalculateArcGeometryD()
        {
            double centerX = X + Width / 2;
            double centerY = Y + ArcHeight;
            double startAngle = 0;
            double endAngle = Math.PI;
            return (centerX, centerY, startAngle, endAngle);
        }

        public (double centerX, double centerY, double startAngle, double endAngle) CalculateArcGeometryByArcHeight()
        {
            Console.WriteLine($"[DEBUG] CalculateArcGeometryByArcHeight START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            double chordWidth = Width;
            double sagitta = ArcHeight;
            double arcBaseY = Y + ArcHeight;

            _radius = (chordWidth * chordWidth + 4 * sagitta * sagitta) / (8 * sagitta);

            double centerX = X + Width / 2.0;
            double centerY = arcBaseY + (Radius - sagitta);

            double leftX = X;
            double rightX = X + Width;

            double startAngle = Math.Atan2(arcBaseY - centerY, rightX - centerX);
            double endAngle = Math.Atan2(arcBaseY - centerY, leftX - centerX);

            if (startAngle < 0) startAngle += 2 * Math.PI;
            if (endAngle < 0) endAngle += 2 * Math.PI;
            if (startAngle > endAngle) endAngle += 2 * Math.PI;

            Console.WriteLine($"[DEBUG] CalculateArcGeometryByArcHeight END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            return (centerX, centerY, startAngle, endAngle);
        }

        private double CalculateRadiusFromArcGeometry(double width, double arcHeight)
        {
            double w = width;
            double h = arcHeight;
            return (w * w + 4 * h * h) / (8 * h);
        }

        private (double cx, double cy, double radius) CircleFromThreePoints(
            double x1, double y1,
            double x2, double y2,
            double x3, double y3)
        {
            Console.WriteLine($"[DEBUG] CircleFromThreePoints START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            double width = x3 - x1;
            double arcHeight = y1 - y2;
            double radius = CalculateRadiusFromArcGeometry(width, arcHeight);
            double cx = (x1 + x3) / 2;
            double cy = y1 + (radius - arcHeight);

            Console.WriteLine($"[DEBUG] CircleFromThreePoints END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");
            return (cx, cy, radius);
        }

        // ===========================
        // Generowanie punktów wzdłuż łuku i boków
        // ===========================
        private List<XPoint> GenerateCompleteOutline(int segments)
        {
            Console.WriteLine($"[DEBUG] GenerateCompleteOutline START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            var outline = new List<XPoint>();

            double leftX = X;
            double rightX = X + Width;
            double bottomY = Y + Height;
            double arcStartY = Y + ArcHeight;

            var (arcCenterX, arcCenterY, startAngle, endAngle) = CalculateArcGeometry();

            outline.Add(new XPoint(leftX, bottomY));
            outline.Add(new XPoint(rightX, bottomY));
            outline.Add(new XPoint(rightX, arcStartY));

            if (segments <= 0) segments = 2;

            for (int i = 0; i <= segments; i++)
            {
                double t = i / (double)segments;
                double angle = startAngle + t * (endAngle - startAngle);
                double x = arcCenterX + Radius * Math.Cos(angle);

                if (Width / 2 <= Height)
                {
                    double y = arcCenterY - Radius * Math.Sin(angle);
                    outline.Add(new XPoint(x, y));
                }
                else
                {
                    double y = arcCenterY + Radius * Math.Sin(angle);
                    outline.Add(new XPoint(x, y));
                }
            }

            outline.Add(new XPoint(leftX, arcStartY));
            outline.Add(new XPoint(leftX, bottomY));
            outline.Reverse();

            for (int i = 0; i < outline.Count; i++)
            {
                var p = outline[i];
                outline[i] = new XPoint(Math.Round(p.X, 4), Math.Round(p.Y, 4));
            }

            Console.WriteLine($"[DEBUG] GenerateCompleteOutline END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            return outline;
        }

        private void NormalizeToPositiveQuadrant()
        {
            if (Points == null || Points.Count == 0) return;

            double minX = Points.Min(p => p.X);
            double minY = Points.Min(p => p.Y);

            double offsetX = minX < 0 ? -minX : 0;
            double offsetY = minY < 0 ? -minY : 0;

            Points = Points.Select(p => new XPoint(p.X + offsetX, p.Y + offsetY)).ToList();

            X += offsetX;
            Y += offsetY;

            NominalPoints = Points.Select(p => p.Clone()).ToList();
        }

        // ===========================
        // Rysowanie Canvas
        // ===========================
        public async Task Draw(Canvas2DContext ctx)
        {
            double leftX = X;
            double rightX = X + Width;
            double bottomY = Y + Height;
            double arcStartY = Y + ArcHeight;

            var (arcCenterX, arcCenterY, startAngle, endAngle) = CalculateArcGeometry();

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync(3);
            await ctx.BeginPathAsync();

            await ctx.MoveToAsync(leftX, bottomY);
            await ctx.LineToAsync(rightX, bottomY);
            await ctx.LineToAsync(rightX, arcStartY);

            await ctx.ArcAsync(arcCenterX, arcCenterY, Radius, startAngle, endAngle, true);

            await ctx.LineToAsync(leftX, bottomY);
            await ctx.StrokeAsync();
        }

        // ===========================
        // Bounding box i wierzchołki
        // ===========================
        public BoundingBox GetBoundingBox() => new BoundingBox(X, Y, Width, Height, NazwaObj);

        public List<XPoint> GetVertices() => GenerateCompleteOutline(IloscElementowLuki);

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var v = GetVertices();
            var edges = new List<(XPoint, XPoint)>();
            for (int i = 0; i < v.Count - 1; i++)
                edges.Add((v[i], v[i + 1]));
            edges.Add((v[^1], v[0]));
            return edges;
        }

        // ===========================
        // Modyfikacja punktów
        // ===========================
        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 5) return;
            Console.WriteLine($"[DEBUG] UpdatePoints START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            Points = newPoints.ToList();

            X = Points.Min(p => p.X);
            Y = Points.Min(p => p.Y);
            Width = Points.Max(p => p.X) - X;
            Height = Points.Max(p => p.Y) - Y;

            ArcHeight = Points[Points.Count / 2].Y - Y;
            Radius = Math.Sqrt(Math.Pow(Points[Points.Count / 2].X - (X + Width / 2), 2) +
                               Math.Pow(Points[Points.Count / 2].Y - (Y + ArcHeight), 2));

            MarkGeometryDirty();
            CalculatePointsFromProperties();

            Console.WriteLine($"[DEBUG] UpdatePoints END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");
        }

        // ===========================
        // Transformacje
        // ===========================
        public void Scale(double factor)
        {
            if (factor == 0) return;
            X *= factor; Y *= factor;
            Width *= factor; Height *= factor;
            Radius *= factor; ArcHeight *= factor;
            MarkGeometryDirty();
            CalculatePointsFromProperties();
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX; Y += offsetY;
            Points = Points.Select(p => new XPoint(p.X + offsetX, p.Y + offsetY)).ToList();
            NominalPoints = Points.Select(p => p.Clone()).ToList();
            MarkGeometryDirty();
        }

        public void Transform(double scale, double offsetX, double offsetY) => Transform(scale, scale, offsetX, offsetY);

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            Console.WriteLine($"[DEBUG] Transform START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            double scale = (scaleX + scaleY) / 2.0;
            X = X * scale + offsetX;
            Y = Y * scale + offsetY;
            Width *= scale;
            Height *= scale;
            Radius *= scale;
            ArcHeight *= scale;

            Width = Math.Max(200, Width);
            Height = Math.Max(100, Height);
            Radius = Math.Max(50, Width / 2);
            ArcHeight = Math.Max(50, Width / 2);
            ArcHeight = Math.Min(ArcHeight, Height);

            MarkGeometryDirty();
            CalculatePointsFromProperties();

            Console.WriteLine($"[DEBUG] Transform END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");
        }

        public IShapeDC Clone()
        {
            return new XRoundedTopRectangleShape(X, Y, Width, Height, Radius, ArcHeight, _scaleFactor)
            {
                ID = Guid.NewGuid().ToString(),
                Points = Points.Select(p => p.Clone()).ToList(),
                NominalPoints = NominalPoints.Select(p => p.Clone()).ToList(),
                NazwaObj = NazwaObj,
                Radius = Radius,
                ArcHeight = ArcHeight,
            };
        }

        public List<EditableProperty> GetEditableProperties() => new()
        {
            new EditableProperty("Pozycja X: ", () => X, v => { X = v; MarkGeometryDirty(); CalculatePointsFromProperties(); }, NazwaObj, true, false, false, true),
            new EditableProperty("Pozycja Y: ", () => Y, v => { Y = v; MarkGeometryDirty(); CalculatePointsFromProperties(); }, NazwaObj, true, false, false, true),
            new EditableProperty("Szerokość: ", () => Width, v => { Width = Math.Max(200, v); MarkGeometryDirty(); CalculatePointsFromProperties(); }, NazwaObj),
            new EditableProperty("Wysokość: ", () => Height, v => { Height = Math.Max(100, v); MarkGeometryDirty(); CalculatePointsFromProperties(); }, NazwaObj),
            new EditableProperty("Promień łuku: ", () => Radius, v => { Radius = v; MarkGeometryDirty(); CalculatePointsFromProperties(); }, NazwaObj, true),
            new EditableProperty("Wysokość łuku: ", () => ArcHeight, v => { ArcHeight = v; MarkGeometryDirty(); CalculatePointsFromProperties(); }, NazwaObj),
            new EditableProperty("Podział na elementy: ", () => IloscElementowLuki, v => {
                int newValue = (int)Math.Round(v / 2.0) * 2;
                IloscElementowLuki = Math.Max(2, newValue);
            }, NazwaObj),
        };

        // ===========================
        // Generowanie segmentów konturu
        // ===========================
        public List<ContourSegment> GetContourSegments()
        {
            if (_isInitializing) return new List<ContourSegment>();

            Console.WriteLine($"[DEBUG] GetContourSegments START {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            var segments = new List<ContourSegment>();
            var mode = GetArcMode();

            double leftX = X;
            double rightX = X + Width;
            double bottomY = Y + Height;
            double arcStartY = Y + ArcHeight;

            var arc = CalculateArcGeometry();
            double cx = arc.centerX;
            double cy = arc.centerY;

            var bottomLeft = new XPoint(leftX, bottomY);
            var bottomRight = new XPoint(rightX, bottomY);
            var topRight = new XPoint(rightX, arcStartY);
            var topLeft = new XPoint(leftX, arcStartY);

            segments.Add(new ContourSegment(bottomLeft, bottomRight));

            if (mode == ArcMode.PelnyLuk)
                segments.Add(new ContourSegment(bottomRight, topRight));

            segments.Add(new ContourSegment(
                topRight,
                topLeft,
                new XPoint(cx, cy),
                Radius,
                true
            ));

            if (mode == ArcMode.PelnyLuk)
                segments.Add(new ContourSegment(topLeft, bottomLeft));

            Console.WriteLine($"[DEBUG] GetContourSegments END {NazwaObj}: Height={Height}, ArcHeight={ArcHeight}");

            return segments;
        }

        private XPoint CalculateCentroid(List<XPoint> pts)
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

        private ArcMode GetArcMode()
        {
            double ratio = Width / Height;

            if (Height > Width) return ArcMode.PelnyLuk;
            if (Height < Width / 2) return ArcMode.TylkoLuk;
            return ArcMode.NiepelnyLuk;
        }

        private enum ArcMode
        {
            PelnyLuk,
            TylkoLuk,
            NiepelnyLuk
        }
    }
}