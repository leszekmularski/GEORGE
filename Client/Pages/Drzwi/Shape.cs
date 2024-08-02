using Blazor.Extensions.Canvas.Canvas2D;

namespace GEORGE.Client.Pages.Drzwi
{
    public abstract class Shape
    {
        public abstract Task DrawAsync(Canvas2DContext context);
        public abstract Task<List<Point>> ReturnPoints();
        public abstract Task<List<LinePoint>> ReturnLinePoints();
    }

    public class RectangleShape : Shape
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public RectangleShape(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            await context.BeginPathAsync();
            await context.RectAsync(X, Y, Width, Height);
            await context.StrokeAsync();
        }

        public override Task<List<Point>> ReturnPoints()
        {
            return Task.FromResult(Xpoints ?? new List<Point>());
        }

        public override Task<List<LinePoint>> ReturnLinePoints()
        {
            return Task.FromResult(XLinePoint ?? new List<LinePoint>());
        }
    }

    public class CircleShape : Shape
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Radius { get; set; }

        public CircleShape(double centerX, double centerY, double radius)
        {
            CenterX = centerX;
            CenterY = centerY;
            Radius = radius;
        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            await context.BeginPathAsync();
            await context.ArcAsync(CenterX, CenterY, Radius, 0, 2 * Math.PI);
            await context.StrokeAsync();
        }

        public override Task<List<Point>> ReturnPoints()
        {
            return Task.FromResult(Xpoints ?? new List<Point>());
        }
        public override Task<List<LinePoint>> ReturnLinePoints()
        {
            return Task.FromResult(XLinePoint ?? new List<LinePoint>());
        }
    }

    public class LineShape : Shape
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }

        public LineShape(double startX, double startY, double endX, double endY)
        {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            await context.BeginPathAsync();
            await context.MoveToAsync(StartX, StartY);
            await context.LineToAsync(EndX, EndY);
            await context.StrokeAsync();
        }

        public override Task<List<Point>> ReturnPoints()
        {
            return Task.FromResult(Xpoints ?? new List<Point>());
        }
        public override Task<List<LinePoint>> ReturnLinePoints()
        {
            return Task.FromResult(XLinePoint ?? new List<LinePoint>());
        }
    }

    public class EllipseShape : Shape
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double RadiusX { get; set; }
        public double RadiusY { get; set; }

        public EllipseShape(double centerX, double centerY, double radiusX, double radiusY)
        {
            CenterX = centerX;
            CenterY = centerY;
            RadiusX = radiusX;
            RadiusY = radiusY;
        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            await context.SaveAsync();
            await context.TranslateAsync(CenterX, CenterY);
            await context.ScaleAsync(RadiusX, RadiusY);
            await context.BeginPathAsync();
            await context.ArcAsync(0, 0, 1, 0, 2 * Math.PI);
            await context.RestoreAsync();
            await context.StrokeAsync();
        }

        public override Task<List<Point>> ReturnPoints()
        {
            return Task.FromResult(Xpoints ?? new List<Point>());
        }
        public override Task<List<LinePoint>> ReturnLinePoints()
        {
            return Task.FromResult(XLinePoint ?? new List<LinePoint>());
        }
    }

    public class TriangleShape : Shape
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double X3 { get; set; }
        public double Y3 { get; set; }

        public TriangleShape(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            X3 = x3;
            Y3 = y3;
        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            await context.BeginPathAsync();
            await context.MoveToAsync(X1, Y1);
            await context.LineToAsync(X2, Y2);
            await context.LineToAsync(X3, Y3);
            await context.ClosePathAsync();
            await context.StrokeAsync();
        }

        public override Task<List<Point>> ReturnPoints()
        {
            return Task.FromResult(Xpoints ?? new List<Point>());
        }
        public override Task<List<LinePoint>> ReturnLinePoints()
        {
            return Task.FromResult(XLinePoint ?? new List<LinePoint>());
        }
    }

    public class LShape : Shape
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Thickness { get; set; }

        public LShape(double x, double y, double width, double height, double thickness)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Thickness = thickness;
        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            await context.BeginPathAsync();

            // Draw outer rectangle
            await context.MoveToAsync(X, Y);
            await context.LineToAsync(X + Width, Y);
            await context.LineToAsync(X + Width, Y + Thickness);
            await context.LineToAsync(X + Thickness, Y + Thickness);
            await context.LineToAsync(X + Thickness, Y + Height);
            await context.LineToAsync(X, Y + Height);
            await context.ClosePathAsync();

            await context.StrokeAsync();
        }
        public override Task<List<Point>> ReturnPoints()
        {
            return Task.FromResult(Xpoints ?? new List<Point>());
        }
        public override Task<List<LinePoint>> ReturnLinePoints()
        {
            return Task.FromResult(XLinePoint ?? new List<LinePoint>());
        }
    }

    public class CShape : Shape
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Thickness { get; set; }

        public CShape(double x, double y, double width, double height, double thickness)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Thickness = thickness;
        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            await context.BeginPathAsync();

            // Draw outer rectangle
            await context.MoveToAsync(X, Y);
            await context.LineToAsync(X + Width, Y);
            await context.LineToAsync(X + Width, Y + Thickness);
            await context.LineToAsync(X + Thickness, Y + Thickness);
            await context.LineToAsync(X + Thickness, Y + Height - Thickness);
            await context.LineToAsync(X + Width, Y + Height - Thickness);
            await context.LineToAsync(X + Width, Y + Height);
            await context.LineToAsync(X, Y + Height);
            await context.ClosePathAsync();

            await context.StrokeAsync();
        }
        public override Task<List<Point>> ReturnPoints()
        {
            return Task.FromResult(Xpoints ?? new List<Point>());
        }
        public override Task<List<LinePoint>> ReturnLinePoints()
        {
            return Task.FromResult(XLinePoint ?? new List<LinePoint>());
        }

    }

    public class COscStala : Shape
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Thickness { get; set; }

        public COscStala(double x, double y, double width, double height, double thickness)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Thickness = thickness;
        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            await context.BeginPathAsync();

            // Draw outer rectangle
            await context.MoveToAsync(X, Y);
            await context.LineToAsync(X + Width, Y);
            await context.LineToAsync(X + Width, Y + Thickness);
            await context.LineToAsync(X + Thickness, Y + Thickness);
            await context.LineToAsync(X + Thickness, Y + Height - Thickness);
            await context.LineToAsync(X + Width, Y + Height - Thickness);
            await context.LineToAsync(X + Width, Y + Height);
            await context.LineToAsync(X, Y + Height);
            await context.ClosePathAsync();

            await context.StrokeAsync();
        }

        public override Task<List<Point>> ReturnPoints()
        {
            return Task.FromResult(Xpoints ?? new List<Point>());
        }
        public override Task<List<LinePoint>> ReturnLinePoints()
        {
            return Task.FromResult(XLinePoint ?? new List<LinePoint>());
        }
    }
    //---------------------------------------------------------------- PIÓRO -------------------------------------------------------------------------------------------
    public class COscZPiorem : Shape
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double WysPiora { get; set; }
        public double GrubPiora { get; set; }
        public double SzerDrzwi { get; set; }
        public double WysDrzwi { get; set; }
        public double WysProgu { get; set; }
        public double Skala { get; set; }
        public string Lacz_Osc { get; set; }

        public COscZPiorem(double x, double y, double wysosc, double szrosc, double wyspiora, double grubpiora, double szerdrzwi, double wysdrzwi,
            double wysprogu, double skala, string lacz_osc)
        {
            X = x;
            Y = y;
            Width = szrosc;
            Height = wysosc;
            WysPiora = wyspiora;
            GrubPiora = grubpiora;
            SzerDrzwi = szerdrzwi;
            WysDrzwi = wysdrzwi;
            WysProgu = wysprogu;
            Skala = skala;
            Lacz_Osc = lacz_osc;
        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            Xpoints = new List<Point>();
            XLinePoint = new List<LinePoint>();
          // Draw the original shape
          await DrawShapeAsync(context, X, Y);

            await DrawTextAsync(context, X + (SzerDrzwi / 4) * Skala, Y, $"S: {SzerDrzwi} x W: {WysDrzwi}");

            if (Lacz_Osc == "45") await DrawShapeObrRamLaczenieAsync(context, X, Y); // Sposób łączenia ramy górą
            if (Lacz_Osc == "90") await DrawShapeObrRamLaczenie90Async(context, X, Y); // Sposób łączenia ramy górą

            // Draw the mirrored shape
            await context.SaveAsync();
            await context.TranslateAsync(0, 0);
            await context.ScaleAsync(-1, 1);

            await DrawShapeAsync(context, -(SzerDrzwi - 2 * Width) * Skala - X, Y, true);

            if (Lacz_Osc == "45") await DrawShapeObrRamLaczenieAsync(context, -(SzerDrzwi - 2 * Width) * Skala - X, Y); // Sposób łączenia ramy górą
            if (Lacz_Osc == "90") await DrawShapeObrRamLaczenie90Async(context, -(SzerDrzwi - 2 * Width) * Skala - X, Y); // Sposób łączenia ramy górą
            await context.RestoreAsync();

            await DrawShapeObrRamAsync(context, X, Y); // Obrys ramy

            Console.WriteLine($"DrawAsync - SzerDrzwi: {SzerDrzwi} x {WysDrzwi} / Skala: {Skala}");

        }

        public override Task<List<Point>> ReturnPoints()
        {
            return Task.FromResult(Xpoints ?? new List<Point>());
        }

        private async Task DrawShapeAsync(Canvas2DContext context, double offsetX, double offsetY, bool lutro = false)
        {
            await context.BeginPathAsync();

            double loffsetY = offsetY + ((100 + WysPiora) * Skala);


            await context.MoveToAsync(offsetX, loffsetY);
            await context.LineToAsync(offsetX - Width * Skala, loffsetY);
            await context.LineToAsync(offsetX - Width * Skala, loffsetY + Height * Skala);
            await context.LineToAsync(offsetX - (Width - 15) * Skala, loffsetY + Height * Skala);
            await context.LineToAsync(offsetX - (Width - 15) * Skala, loffsetY + (Height - 15) * Skala);
            await context.LineToAsync(offsetX - (Width - 20) * Skala, loffsetY + (Height - 15) * Skala);
            await context.LineToAsync(offsetX - (Width - 20) * Skala, loffsetY + (Height - 20) * Skala);
            await context.LineToAsync(offsetX - (Width - 23) * Skala, loffsetY + (Height - 20) * Skala);
            await context.LineToAsync(offsetX - (Width - 25) * Skala, loffsetY + (Height - 15) * Skala);
            await context.LineToAsync(offsetX, loffsetY + (Height - 15) * Skala);

            await context.ClosePathAsync();

            await context.RectAsync(offsetX - (Width - 15) * Skala, loffsetY + Height * Skala, -WysPiora * Skala, GrubPiora * Skala); // Pióro

            if (!lutro)
            {
                AddLinePoints(offsetX, loffsetY, offsetX - Width * Skala, loffsetY);
                AddLinePoints(offsetX - Width * Skala, loffsetY, offsetX - Width * Skala, loffsetY + Height * Skala);
                AddLinePoints(offsetX - Width * Skala, loffsetY + Height * Skala, offsetX - (Width - 15) * Skala, loffsetY + Height * Skala);
                AddLinePoints(offsetX - (Width - 15) * Skala, loffsetY + Height * Skala, offsetX - (Width - 15) * Skala, loffsetY + (Height - 15) * Skala);
                AddLinePoints(offsetX - (Width - 15) * Skala, loffsetY + (Height - 15) * Skala, offsetX - (Width - 20) * Skala, loffsetY + (Height - 15) * Skala);
                AddLinePoints(offsetX - (Width - 20) * Skala, loffsetY + (Height - 15) * Skala, offsetX - (Width - 20) * Skala, loffsetY + (Height - 20) * Skala);
                AddLinePoints(offsetX - (Width - 20) * Skala, loffsetY + (Height - 20) * Skala, offsetX - (Width - 23) * Skala, loffsetY + (Height - 20) * Skala);
                AddLinePoints(offsetX - (Width - 23) * Skala, loffsetY + (Height - 20) * Skala, offsetX - (Width - 25) * Skala, loffsetY + (Height - 15) * Skala);
                AddLinePoints(offsetX - (Width - 25) * Skala, loffsetY + (Height - 15) * Skala, offsetX, loffsetY + (Height - 15) * Skala);
                AddLinePoints(offsetX, loffsetY + (Height - 15) * Skala, offsetX, loffsetY);

                AddRectanglePoints(offsetX - (Width - 15) * Skala, loffsetY + Height * Skala, -WysPiora * Skala, GrubPiora * Skala);
            }

    

            await context.StrokeAsync();
        }

        private async Task DrawShapeObrRamAsync(Canvas2DContext context, double offsetX, double offsetY)
        {
            await context.BeginPathAsync();

            // Draw outer rectangle
            await context.RectAsync(offsetX - (Width + 0) * Skala, offsetY + (Width + 0) * Skala, SzerDrzwi * Skala, (WysDrzwi - Width) * Skala); // #1
            await context.RectAsync(offsetX - (Width - Width) * Skala, offsetY + (Width + Width) * Skala, (SzerDrzwi - Width * 2) * Skala, (WysDrzwi - Width * 2) * Skala); // #2
            await context.RectAsync(offsetX - (Width + WysPiora - 15) * Skala, offsetY + (Width - (WysPiora - 15)) * Skala, (SzerDrzwi + (WysPiora - 15) * 2) * Skala, (WysDrzwi + WysPiora - Width - 15) * Skala); // #3
            await context.RectAsync(offsetX, offsetY + (WysDrzwi - WysProgu) * Skala, (SzerDrzwi - 2 * Width) * Skala, WysProgu * Skala); //PRÓG

            AddRectanglePoints(offsetX - (Width + 0) * Skala, offsetY + (Width + 0) * Skala, SzerDrzwi * Skala, (WysDrzwi - Width) * Skala); // #1
            AddRectanglePoints(offsetX - (Width - Width) * Skala, offsetY + (Width + Width) * Skala, (SzerDrzwi - Width * 2) * Skala, (WysDrzwi - Width * 2) * Skala); // #2
            AddRectanglePoints(offsetX - (Width + WysPiora - 15) * Skala, offsetY + (Width - (WysPiora - 15)) * Skala, (SzerDrzwi + (WysPiora - 15) * 2) * Skala, (WysDrzwi + WysPiora - Width - 15) * Skala); // #3
            AddRectanglePoints(offsetX, offsetY + (WysDrzwi - WysProgu) * Skala, (SzerDrzwi - 2 * Width) * Skala, WysProgu * Skala); //PRÓG

            await context.StrokeAsync();
        }

        private async Task DrawShapeObrRamLaczenieAsync(Canvas2DContext context, double offsetX, double offsetY)
        {
            await context.BeginPathAsync();
            await context.MoveToAsync(offsetX, offsetY - (WysPiora - 2 * Width - 15) * Skala + (WysPiora - 15) * Skala);
            await context.LineToAsync(offsetX - (Width + WysPiora - 15) * Skala, offsetY - (WysPiora - 15 - Width) * Skala);

            AddLinePoints(offsetX, offsetY - (WysPiora - 2 * Width - 15) * Skala + (WysPiora - 15) * Skala,
                          offsetX - (Width + WysPiora - 15) * Skala, offsetY - (WysPiora - 15 - Width) * Skala);

            await context.StrokeAsync();
        }

        private async Task DrawShapeObrRamLaczenie90Async(Canvas2DContext context, double offsetX, double offsetY)
        {
            await context.BeginPathAsync();
            await context.MoveToAsync(offsetX, offsetY + (Width + 0) * Skala);
            await context.LineToAsync(offsetX - (Width + WysPiora - 15) * Skala, offsetY + (Width + 0) * Skala);

            AddLinePoints(offsetX, offsetY + (Width + 0) * Skala,
                offsetX - (Width + WysPiora - 15) * Skala, offsetY + (Width + 0) * Skala);

            await context.MoveToAsync(offsetX, offsetY + 2 * Width * Skala);
            await context.LineToAsync(offsetX, offsetY + Width * Skala);
            AddLinePoints(offsetX, offsetY + 2 * Width * Skala,
                offsetX, offsetY + Width * Skala);

            await context.StrokeAsync();
        }

        private async Task DrawTextAsync(Canvas2DContext context, double x, double y, string text)
        {
            // Set text style
            await context.SetFontAsync("16px Arial");
            await context.SetFillStyleAsync("black");

            // Draw text
            await context.FillTextAsync(text, x, y);
        }

        private void AddRectanglePoints(double x, double y, double width, double height)
        {
            //Xpoints.Add(new Point(x, y)); // Top-left corner
            //Xpoints.Add(new Point(x + width, y)); // Top-right corner
            //Xpoints.Add(new Point(x + width, y + height)); // Bottom-right corner
            //Xpoints.Add(new Point(x, y + height)); // Bottom-left corner
            XLinePoint.Add(new LinePoint(x, y, x + width, y));
            XLinePoint.Add(new LinePoint(x + width, y, x + width, y + height));
            XLinePoint.Add(new LinePoint(x + width, y + height, x, y + height));
            XLinePoint.Add(new LinePoint(x, y + height, x, y));
        }

        private void AddLinePoints(double x1, double y1, double x2, double y2)
        {
            XLinePoint.Add(new LinePoint(x1, y1, x2, y2));
        }
        public override Task<List<LinePoint>> ReturnLinePoints()
        {
            return Task.FromResult(XLinePoint ?? new List<LinePoint>());
        }

    }

    public class COscRegulowana : Shape
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Thickness { get; set; }

        public COscRegulowana(double x, double y, double width, double height, double thickness)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Thickness = thickness;
        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            await context.BeginPathAsync();

            // Draw outer rectangle
            await context.MoveToAsync(X, Y);
            await context.LineToAsync(X + Width, Y);
            await context.LineToAsync(X + Width, Y + Thickness);
            await context.LineToAsync(X + Thickness, Y + Thickness);
            await context.LineToAsync(X + Thickness, Y + Height - Thickness);
            await context.LineToAsync(X + Width, Y + Height - Thickness);
            await context.LineToAsync(X + Width, Y + Height);
            await context.LineToAsync(X, Y + Height);
            await context.ClosePathAsync();

            await context.StrokeAsync();
        }
        public override Task<List<Point>> ReturnPoints()
        {
            return Task.FromResult(Xpoints ?? new List<Point>());
        }
        public override Task<List<LinePoint>> ReturnLinePoints()
        {
            return Task.FromResult(XLinePoint ?? new List<LinePoint>());
        }
    }

}
