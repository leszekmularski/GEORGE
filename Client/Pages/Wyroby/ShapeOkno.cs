using Blazor.Extensions.Canvas.Canvas2D;
using static iText.IO.Util.IntHashtable;

namespace GEORGE.Client.Pages.Wyroby
{
    public abstract class ShapeOkno
    {
        public abstract Task DrawAsync(Canvas2DContext context);
        public abstract Task<List<Point>> ReturnPoints();
        public abstract Task<List<LinePoint>> ReturnLinePoints();
        public abstract Task<List<WymiaryOpis>> ReturnWymiaryOpis();
    }
    public class RectangleShape : ShapeOkno
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public List<WymiaryOpis>? XWymiaryOpis;
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
        public override Task<List<WymiaryOpis>> ReturnWymiaryOpis()
        {
            return Task.FromResult(XWymiaryOpis ?? new List<WymiaryOpis>());
        }

    }
    public class CircleShape : ShapeOkno
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public List<WymiaryOpis>? XWymiaryOpis;
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
        public override Task<List<WymiaryOpis>> ReturnWymiaryOpis()
        {
            return Task.FromResult(XWymiaryOpis ?? new List<WymiaryOpis>());
        }
    }
    public class LineShape : ShapeOkno
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public List<WymiaryOpis>? XWymiaryOpis;
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
        public override Task<List<WymiaryOpis>> ReturnWymiaryOpis()
        {
            return Task.FromResult(XWymiaryOpis ?? new List<WymiaryOpis>());
        }
    }
    public class EllipseShape : ShapeOkno
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public List<WymiaryOpis>? XWymiaryOpis;
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
        public override Task<List<WymiaryOpis>> ReturnWymiaryOpis()
        {
            return Task.FromResult(XWymiaryOpis ?? new List<WymiaryOpis>());
        }
    }
    public class TriangleShape : ShapeOkno
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public List<WymiaryOpis>? XWymiaryOpis;
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
        public override Task<List<WymiaryOpis>> ReturnWymiaryOpis()
        {
            return Task.FromResult(XWymiaryOpis ?? new List<WymiaryOpis>());
        }
    }
    public class LShape : ShapeOkno
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public List<WymiaryOpis>? XWymiaryOpis;
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
        public override Task<List<WymiaryOpis>> ReturnWymiaryOpis()
        {
            return Task.FromResult(XWymiaryOpis ?? new List<WymiaryOpis>());
        }
    }
    public class CShape : ShapeOkno
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public List<WymiaryOpis>? XWymiaryOpis;
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
        public override Task<List<WymiaryOpis>> ReturnWymiaryOpis()
        {
            return Task.FromResult(XWymiaryOpis ?? new List<WymiaryOpis>());
        }

    }
    public class COKN1SK68 : ShapeOkno
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public List<WymiaryOpis>? XWymiaryOpis;
        public double X { get; set; } //Margines LEWY
        public double Y { get; set; } //Margines GORNY
        public double Width { get; set; }
        public double Height { get; set; }
        public string OtwDoWenatrz { get; set; }
        public string SposOtwierania { get; set; }
        public double Skala { get; set; }

        //--------------------------------------------------------------  STAŁE  -----------------------------------------------------------------------------------------

        private double SzerokoscRamy = 82;

        private double SzerokoscSkrzydla = 82;

        private double SkrzydloMniejszczeO_Gora = 42;
        private double SkrzydloMniejszczeO_Dol = 26;
        private double SkrzydloMniejszczeO_Lewa = 42;
        private double SkrzydloMniejszczeO_Prawa = 42;

        private double GrboscRdzeniaSkrzydla = 64;

        private double WysPiora = 18;

        private double LuzPomiedzySkrzyASzyba = 6;

        //-------------------------------------------------------------  ZMIENNE WEW  ----------------------------------------------------------------------------------------
        private double WymiarWewOknaWidth = 0; // od tego wymiaru liczymy szklenie - wymiar szkła WymiarWewOknaWidth - LuzPomiedzySkrzyASzyba = ??
        private double WymiarWewOknaHeight = 0;

        private double WymiarZewOknaWidth = 0;
        private double WymiarZewOknaHeight = 0;

        public COKN1SK68(double x, double y, double szerokosc, double wysokosc, string otwdowenatrz, string sposotwierania, double skala)
        {
            X = x;
            Y = y;
            Width = szerokosc;
            Height = wysokosc;
            OtwDoWenatrz = otwdowenatrz;
            SposOtwierania = sposotwierania;
            Skala = skala;
        }
        public override async Task DrawAsync(Canvas2DContext context)
        {
            Xpoints = new List<Point>();
            XLinePoint = new List<LinePoint>();
            XWymiaryOpis = new List<WymiaryOpis>();

           // await DrawTextAsync(context, X + (Width / 4) * Skala, Y, $"S: {Width} x W: {Height}");

            if (OtwDoWenatrz == "TAK") await DrawShapeObrRam(context, X, Y); // Rama Okna

            await context.RestoreAsync();

            await DrawShapeObrSkrzydloAsync(context, X, Y); // Skrzydło

            Console.WriteLine($"DrawAsync - SzerDrzwi: {Width} x {Height} / Skala: {Skala}");

            await AddWymInformacja(WymiarWewOknaWidth + (2 * WysPiora  - 2 * LuzPomiedzySkrzyASzyba), WymiarWewOknaHeight + (2 * WysPiora - 2 * LuzPomiedzySkrzyASzyba), WymiarZewOknaWidth, WymiarZewOknaHeight, WymiarWewOknaWidth, WymiarWewOknaHeight, Guid.NewGuid().ToString());

       }
        public override Task<List<Point>> ReturnPoints()
        {
            return Task.FromResult(Xpoints ?? new List<Point>());
        }
        private async Task DrawSposOtwLines(Canvas2DContext context, double xWewSkrzy, double yWewSkrzyd)
        {
            if(SposOtwierania == "RUP")
            {
                // Rysowanie linii promienistych
                await context.BeginPathAsync();
                await context.SetStrokeStyleAsync("black"); // Kolor linii
                await context.SetLineWidthAsync(1);
                await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię

                // WymiarWewOknaWidth, WymiarWewOknaHeight, SzerokoscRamy, SzerokoscSkrzydla, SkrzydloMniejszczeO

                // Rysowanie linii
                await context.MoveToAsync(xWewSkrzy, yWewSkrzyd);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + (WymiarWewOknaHeight / 2 * Skala));
                await context.LineToAsync(xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight * Skala);
                await context.StrokeAsync();

                await context.MoveToAsync(xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight * Skala);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth / 2 * Skala, yWewSkrzyd);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight * Skala);
                await context.StrokeAsync();

                await DrawKlamkaLines(context, xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight / 2 * Skala, 'P');

                //AddLinePoints(xWewSkrzy, yWewSkrzyd, xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaWidth / 2 * Skala, "dashed"); //Generuj linie pod DXF
                //AddLinePoints(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaWidth / 2 * Skala, xWewSkrzy, yWewSkrzyd + WymiarWewOknaWidth * Skala, "dashed"); //Generuj linie pod DXF
                //AddLinePoints(xWewSkrzy, yWewSkrzyd + WymiarWewOknaWidth * Skala, xWewSkrzy, yWewSkrzyd + WymiarWewOknaWidth * Skala, "dashed"); //Generuj linie pod DXF
            }
            else if (SposOtwierania == "RUL")
            {
                // Rysowanie linii promienistych
                await context.BeginPathAsync();
                await context.SetStrokeStyleAsync("black"); // Kolor linii
                await context.SetLineWidthAsync(1);
                await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię

                // WymiarWewOknaWidth, WymiarWewOknaHeight, SzerokoscRamy, SzerokoscSkrzydla, SkrzydloMniejszczeO

                // Rysowanie linii
                await context.MoveToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd);
                await context.LineToAsync(xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight / 2 * Skala);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight * Skala);
                await context.StrokeAsync();

                await context.MoveToAsync(xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight * Skala);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth / 2 * Skala, yWewSkrzyd);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight * Skala);
                await context.StrokeAsync();

                await DrawKlamkaLines(context, xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight / 2 * Skala, 'L');

            }
            else if (SposOtwierania == "U")
            {
                // Rysowanie linii promienistych
                await context.BeginPathAsync();
                await context.SetStrokeStyleAsync("black"); // Kolor linii
                await context.SetLineWidthAsync(1);
                await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię

                // WymiarWewOknaWidth, WymiarWewOknaHeight, SzerokoscRamy, SzerokoscSkrzydla, SkrzydloMniejszczeO

                await context.MoveToAsync(xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight * Skala);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth / 2 * Skala, yWewSkrzyd);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight * Skala);
                await context.StrokeAsync();

                await DrawKlamkaLines(context, xWewSkrzy + WymiarWewOknaWidth / 2 * Skala, yWewSkrzyd, 'U');

            }
            else if (SposOtwierania == "RP")
            {
                // Rysowanie linii promienistych
                await context.BeginPathAsync();
                await context.SetStrokeStyleAsync("black"); // Kolor linii
                await context.SetLineWidthAsync(1);
                await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię

                // WymiarWewOknaWidth, WymiarWewOknaHeight, SzerokoscRamy, SzerokoscSkrzydla, SkrzydloMniejszczeO

                // Rysowanie linii
                await context.MoveToAsync(xWewSkrzy, yWewSkrzyd);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight / 2 * Skala);
                await context.LineToAsync(xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight * Skala);
                await context.StrokeAsync();

                await DrawKlamkaLines(context, xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight / 2 * Skala, 'P');

            }
            else if (SposOtwierania == "RL")
            {
                // Rysowanie linii promienistych
                await context.BeginPathAsync();
                await context.SetStrokeStyleAsync("black"); // Kolor linii
                await context.SetLineWidthAsync(1);
                await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię

                // WymiarWewOknaWidth, WymiarWewOknaHeight, SzerokoscRamy, SzerokoscSkrzydla, SkrzydloMniejszczeO

                // Rysowanie linii
                await context.MoveToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd);
                await context.LineToAsync(xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight / 2 * Skala);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight * Skala);
                await context.StrokeAsync();

                await DrawKlamkaLines(context, xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight / 2 * Skala, 'L');

            }

        }
        private async Task DrawKlamkaLines(Canvas2DContext context, double xOsObr, double yOsObr, char strona)
        {
            await context.BeginPathAsync();
            await context.SetLineWidthAsync(5);
            await context.SetStrokeStyleAsync("blue"); // Kolor linii
            await context.SetLineDashAsync(new float[] { });

            if (strona == 'L')
            {
                await context.RectAsync(xOsObr - 10 * Skala, yOsObr - 10 * Skala, 90 * Skala, 10 * Skala); // #1
                AddRectanglePoints(xOsObr - 10 * Skala, yOsObr - 10 * Skala, 90 * Skala, 10 * Skala); // #1
            }
            else if(strona == 'P')
            {
                await context.RectAsync(xOsObr + 10 * Skala, yOsObr - 10 * Skala,  -90 * Skala, 10 * Skala); // #1
                AddRectanglePoints(xOsObr + 10 * Skala, yOsObr - 10 * Skala, -90 * Skala, 10 * Skala); // #1
            }
            else if (strona == 'U')
            {
                await context.RectAsync(xOsObr, yOsObr - 10 * Skala, 90 * Skala, 10 * Skala); // #1
                AddRectanglePoints(xOsObr, yOsObr - 10 * Skala, 90 * Skala, 10 * Skala); // #1
            }
                await context.StrokeAsync();
        }
        private void AddLinePoints(double x1, double y1, double x2, double y2, string typeLine = "")
        {
            if (XLinePoint == null) return;

            XLinePoint.Add(new LinePoint(x1, y1, x2, y2, typeLine));
        }
        private async Task AddWymInformacja(double szerokoscSzyby, double wysokoscSzyby, double szerokoscSkrzydla, double wysokoscSkrzydla, double wymiarWewSzeSkrzydla, double wymiarWewWyskrzydla, string id_Okna)
        {
            if (XWymiaryOpis == null) return;

            XWymiaryOpis.Add(new WymiaryOpis(szerokoscSzyby, wysokoscSzyby, szerokoscSkrzydla, wysokoscSkrzydla, wymiarWewSzeSkrzydla, wymiarWewWyskrzydla, id_Okna));

            await Task.CompletedTask;
        }
        private async Task DrawShapeObrSkrzydloAsync(Canvas2DContext context, double offsetX, double offsetY)
        {
            await context.BeginPathAsync();
            await context.SetLineWidthAsync(3);
            await context.SetLineDashAsync(new float[] { });

            WymiarZewOknaWidth = Width - 2 * SzerokoscSkrzydla + SkrzydloMniejszczeO_Lewa + SkrzydloMniejszczeO_Prawa;
            WymiarZewOknaHeight = Height - 2 * SzerokoscSkrzydla + SkrzydloMniejszczeO_Gora + SkrzydloMniejszczeO_Dol;

            WymiarWewOknaWidth = WymiarZewOknaWidth - 2 * GrboscRdzeniaSkrzydla - 2 * WysPiora;
            WymiarWewOknaHeight = WymiarZewOknaHeight - 2 * GrboscRdzeniaSkrzydla - 2 * WysPiora;

            // Draw outer rectangle
            await context.RectAsync(offsetX + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Lewa) * Skala, offsetY + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Gora) * Skala, 
                WymiarZewOknaWidth * Skala, WymiarZewOknaHeight * Skala); // #1
           
            await context.RectAsync(offsetX + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Lewa + SzerokoscSkrzydla) * Skala, offsetY + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Gora + SzerokoscSkrzydla) * Skala, 
                WymiarWewOknaWidth * Skala, WymiarWewOknaHeight * Skala); // #2
           
            await context.StrokeAsync();

            //Poniże zmienne pod dxf
            //AddRectanglePoints(offsetX + SkrzydloMniejszczeO * Skala, offsetY + SkrzydloMniejszczeO * Skala, (Width - 2 * SkrzydloMniejszczeO) * Skala, (Height - 2 * SkrzydloMniejszczeO) * Skala); // #1
            //AddRectanglePoints(offsetX + (SkrzydloMniejszczeO + SzerokoscSkrzydla) * Skala, offsetY + (SkrzydloMniejszczeO + SzerokoscSkrzydla) * Skala, (WymiarWewOknaWidth) * Skala, (WymiarWewOknaHeight) * Skala); // #2

            await DrawSposOtwLines(context, offsetX + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Lewa + SzerokoscSkrzydla) * Skala,
                offsetY + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Gora + SzerokoscSkrzydla) * Skala); // Sposób otwierania
        }
        private async Task DrawShapeObrRam(Canvas2DContext context, double offsetX, double offsetY)
        {
            await context.BeginPathAsync();
            await context.SetLineWidthAsync(3);
            // Ustaw kolor linii na czarny
            await context.SetStrokeStyleAsync("black");
            // Draw outer rectangle
            await context.SetLineDashAsync(new float[] { });
            await context.RectAsync(offsetX, offsetY, Width * Skala, Height * Skala); // #1
            await context.StrokeAsync();

            await context.SetLineWidthAsync(1);
            await context.RectAsync(offsetX + SzerokoscRamy * Skala, offsetY + SzerokoscRamy * Skala, (Width - 2 * SzerokoscRamy) * Skala, (Height - 2 * SzerokoscRamy) * Skala); // #2 Continuous
            await context.SetLineDashAsync(new float[] { 10, 5 }); // Ustaw przerywaną linię
            await context.StrokeAsync();

            AddRectanglePoints(offsetX, offsetY, Width * Skala, Height * Skala); // #1
            AddRectanglePoints(offsetX + SzerokoscRamy * Skala, offsetY + SzerokoscRamy * Skala, (Width - 2 * SzerokoscRamy) * Skala, (Height - 2 * SzerokoscRamy) * Skala); // #2 dashed

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
            if (XLinePoint != null)
            {
                XLinePoint.Add(new LinePoint(x, y, x + width, y, ""));
                XLinePoint.Add(new LinePoint(x + width, y, x + width, y + height, ""));
                XLinePoint.Add(new LinePoint(x + width, y + height, x, y + height, ""));
                XLinePoint.Add(new LinePoint(x, y + height, x, y, ""));
            }
        }
        private void AddLinePoints(double x1, double y1, double x2, double y2)
        {
            if (XLinePoint != null)
                XLinePoint.Add(new LinePoint(x1, y1, x2, y2, ""));
        }
        public override Task<List<LinePoint>> ReturnLinePoints()
        {
            return Task.FromResult(XLinePoint ?? new List<LinePoint>());
        }
        public override Task<List<WymiaryOpis>> ReturnWymiaryOpis()
        {
            return Task.FromResult(XWymiaryOpis ?? new List<WymiaryOpis>());
        }

    }

}
