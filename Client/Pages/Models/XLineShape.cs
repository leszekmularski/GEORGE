using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    public class XLineShape : IShapeDC
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }

        public string NazwaObj { get; set; } = "Linia";
        public bool RuchomySlupek { get; set; } = false;
        public bool StalySlupek { get; set; } = false;
        public bool PionPoziom { get; set; } = false;
        public bool DualRama { get; set; } = false;

        private double _scaleFactor = 1.0;

        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }

        public bool GenerowaneZRamy { get; set; } = false;

        public List<XPoint> Points { get; set; } = new();
        public List<XPoint> NominalPoints { get; set; } = new();
        public List<XPoint> GetPoints() => Points;
        public List<XPoint> GetNominalPoints() =>
            NominalPoints.Select(p => new XPoint(p.X, p.Y)).ToList();

        public string ID { get; set; } = Guid.NewGuid().ToString();

        public XLineShape(
            double x1, double y1, double x2, double y2, double scaleFactor,
            string nazwaObj, bool ruchomySlupek = false, bool pionPoziom = false,
            bool dualRama = false, bool generowaneZRamy = false, bool stalySlupek = false)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;

            _scaleFactor = scaleFactor;
            NazwaObj = nazwaObj;
            RuchomySlupek = ruchomySlupek;
            PionPoziom = pionPoziom;
            DualRama = dualRama;
            GenerowaneZRamy = generowaneZRamy;
            StalySlupek = stalySlupek;

            EnforceLineType();
            UpdateSize();
            GeneratePoints();
        }

        private void GeneratePoints()
        {
            Points = new()
            {
                new XPoint(X1, Y1),
                new XPoint(X2, Y2)
            };

            NominalPoints = Points.Select(p => new XPoint(p.X, p.Y)).ToList();
        }

        private void EnforceLineType()
        {
            if (RuchomySlupek)
                X2 = X1;

            if (PionPoziom)
            {
                if (Math.Abs(X1 - X2) > 0.01)
                    Y2 = Y1;     // pozioma
                else
                    X2 = X1;     // pionowa
            }
        }

        private void UpdateSize()
        {
            Szerokosc = Math.Abs(X2 - X1);
            Wysokosc = Math.Abs(Y2 - Y1);
        }

        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 2)
                return;

            X1 = newPoints[0].X;
            Y1 = newPoints[0].Y;
            X2 = newPoints[1].X;
            Y2 = newPoints[1].Y;

            EnforceLineType();
            UpdateSize();
            GeneratePoints();
        }

        public IShapeDC Clone()
        {
            return new XLineShape(X1, Y1, X2, Y2, _scaleFactor, NazwaObj,
                RuchomySlupek, PionPoziom, DualRama, GenerowaneZRamy, StalySlupek);
        }

        public async Task Draw(Canvas2DContext ctx)
        {
            GeneratePoints();
            UpdateSize();

            if (RuchomySlupek)
                await ctx.SetStrokeStyleAsync("red");
            else if (DualRama)
                await ctx.SetStrokeStyleAsync("orange");
            else
                await ctx.SetStrokeStyleAsync("green");

            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(X1, Y1);
            await ctx.LineToAsync(X2, Y2);
            await ctx.StrokeAsync();
        }

        public void Move(double offsetX, double offsetY)
        {
            X1 += offsetX;
            Y1 += offsetY;
            X2 += offsetX;
            Y2 += offsetY;

            GeneratePoints();
            UpdateSize();
        }

        public void Scale(double factor)
        {
            double cx = (X1 + X2) / 2;
            double cy = (Y1 + Y2) / 2;

            X1 = cx + (X1 - cx) * factor;
            Y1 = cy + (Y1 - cy) * factor;
            X2 = cx + (X2 - cx) * factor;
            Y2 = cy + (Y2 - cy) * factor;

            GeneratePoints();
            UpdateSize();
        }

        // ✔️ WYMAGANA przez interfejs metoda — brakowało jej!
        public void Transform(double scale, double offsetX, double offsetY)
        {
            X1 = X1 * scale + offsetX;
            Y1 = Y1 * scale + offsetY;
            X2 = X2 * scale + offsetX;
            Y2 = Y2 * scale + offsetY;

            GeneratePoints();
            UpdateSize();
        }

        // Druga metoda Transform (zgodnie z interfejsem)
        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X1 = X1 * scaleX + offsetX;
            Y1 = Y1 * scaleY + offsetY;
            X2 = X2 * scaleX + offsetX;
            Y2 = Y2 * scaleY + offsetY;

            GeneratePoints();
            UpdateSize();
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(
                Math.Min(X1, X2),
                Math.Min(Y1, Y2),
                Math.Abs(X2 - X1),
                Math.Abs(Y2 - Y1),
                NazwaObj
            );
        }

        public List<EditableProperty> GetEditableProperties()
        {
            return new()
            {
                new EditableProperty("X1", () => X1, v => X1 = v, NazwaObj),
                new EditableProperty("Y1", () => Y1, v => Y1 = v, NazwaObj),
                new EditableProperty("X2", () => X2, v => X2 = v, NazwaObj),
                new EditableProperty("Y2", () => Y2, v => Y2 = v, NazwaObj)
            };
        }
    }
}
