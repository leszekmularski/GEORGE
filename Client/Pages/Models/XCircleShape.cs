using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    // 🟢 KLASA KOŁA
    /// <summary>
    /// Reprezentuje kształt okręgu z pełną obsługą transformacji
    /// </summary>
    public class XCircleShape : IShapeDC
    {
        private double _x;
        private double _y;
        private double _radius;
        private double _scaleFactor = 1.0;
        private List<XPoint> _points = new List<XPoint>();

        public double X
        {
            get => _x;
            set
            {
                _x = value;
                UpdateCirclePoints();
            }
        }

        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                UpdateCirclePoints();
            }
        }

        public double Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                UpdateCirclePoints();
            }
        }

        public string NazwaObj { get; set; } = "Okrąg";

        public double Szerokosc
        {
            get => Radius * 2;
            set => Radius = value / 2;
        }

        public double Wysokosc
        {
            get => Radius * 2;
            set => Radius = value / 2;
        }

        public List<XPoint> Points
        {
            get => _points;
            set
            {
                if (value != null && value.Count >= 2)
                {
                    _points = new List<XPoint>(value);
                    _x = _points[0].X;
                    _y = _points[0].Y;
                    _radius = CalculateDistance(_points[0], _points[1]);
                    UpdateCirclePoints();
                }
            }
        }

        public XCircleShape() { }

        public XCircleShape(double x, double y, double radius, double scaleFactor = 1.0)
        {
            _x = x;
            _y = y;
            _radius = radius;
            _scaleFactor = scaleFactor;
            UpdateCirclePoints();
        }

        public List<XPoint> GetPoints() => new List<XPoint>(_points);

        public IShapeDC Clone()
        {
            return new XCircleShape(X, Y, Radius, _scaleFactor)
            {
                NazwaObj = this.NazwaObj,
                Points = this._points.Select(p => new XPoint(p.X, p.Y)).ToList()
            };
        }

        public async Task Draw(Canvas2DContext ctx)
        {
            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();
            await ctx.ArcAsync(X, Y, Radius, 0, 2 * Math.PI);
            await ctx.StrokeAsync();
        }

        public List<EditableProperty> GetEditableProperties() => new()
    {
        new EditableProperty("X", () => X, v => X = v, NazwaObj, true),
        new EditableProperty("Y", () => Y, v => Y = v, NazwaObj, true),
        new EditableProperty("Promień", () => Radius, v => Radius = v, NazwaObj),
        new EditableProperty("Skala", () => _scaleFactor, v => _scaleFactor = v, NazwaObj)
    };

        public void Scale(double factor)
        {
            Radius *= factor;
            _scaleFactor *= factor;
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(
                X - Radius,
                Y - Radius,
                Radius * 2,
                Radius * 2,
                NazwaObj
            );
        }

        public void Transform(double scale, double offsetX, double offsetY)
        {
            X = (X * scale) + offsetX;
            Y = (Y * scale) + offsetY;
            Radius *= scale;
            _scaleFactor *= scale;
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X = (X * scaleX) + offsetX;
            Y = (Y * scaleY) + offsetY;
            double avgScale = (scaleX + scaleY) / 2;
            Radius *= avgScale;
            _scaleFactor *= avgScale;
        }

        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints != null && newPoints.Count >= 2)
            {
                _points = new List<XPoint>(newPoints);
                X = _points[0].X;
                Y = _points[0].Y;
                Radius = CalculateDistance(_points[0], _points[1]);
            }
        }

        private void UpdateCirclePoints()
        {
            _points = GenerateCirclePoints(32);
        }

        private List<XPoint> GenerateCirclePoints(int segments = 32)
        {
            var points = new List<XPoint>();

            points.Add(new XPoint(X, Y));

            for (int i = 0; i < segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                points.Add(new XPoint(
                    X + Radius * Math.Cos(angle),
                    Y + Radius * Math.Sin(angle)
                ));
            }

            return points;
        }

        private double CalculateDistance(XPoint p1, XPoint p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
