using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;

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

        public XHouseShape(double x, double y, double width, double height, double heightLeft, double heightRight, double scaleFactor)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            HeightLeft = heightLeft;
            HeightRight = heightRight;
            _scaleFactor = scaleFactor;
        }

        public async Task Draw(Canvas2DContext ctx)
        {
            //double roofHeight = Height * 0.5;
            double baseYL = Y + Height - Math.Max(HeightLeft, 0);
            double baseYR = Y + Height - Math.Max(HeightRight, 0);

            double roofPeakX = X + Width / 2;
            double roofPeakY = Y;

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();

            // Dach (trójkąt)
            await ctx.MoveToAsync(X, baseYL);  // Lewy róg dachu
            await ctx.LineToAsync(roofPeakX, roofPeakY);  // Szczyt dachu
            await ctx.LineToAsync(X + Width, baseYR);  // Prawy róg dachu

            // Podstawa (prostokąt)
            await ctx.LineToAsync(X + Width, Y + Height);  // Prawy dolny róg
            await ctx.LineToAsync(X, Y + Height);  // Lewy dolny róg
            await ctx.LineToAsync(X, baseYL);  // Powrót do początku

            await ctx.ClosePathAsync();
            await ctx.StrokeAsync();

            Console.WriteLine($"BaseYL: {baseYL}, BaseYR: {baseYR}, RoofPeakY: {roofPeakY}");
            Console.WriteLine($"Drawing at: X={X}, Y={Y}, Width={Width}, Height={Height}");


        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(X, Y, Width, Height, "Domek");
        }

        public List<EditableProperty> GetEditableProperties() => new()
    {
        new EditableProperty("X", () => X, v => X = v, NazwaObj, true),
        new EditableProperty("Y", () => Y, v => Y = v, NazwaObj, true),
        new EditableProperty("Szerokość", () => Width, v => Width = v, NazwaObj),
        new EditableProperty("Wysokość", () => Height, v => Height = v, NazwaObj),
        new EditableProperty("Wysokość bok lewy", () => HeightLeft, v => HeightLeft = v, NazwaObj),
        new EditableProperty("Wysokość bok prawy", () => HeightRight, v => HeightRight = v, NazwaObj)
    };

        public void Scale(double factor)
        {
            Width *= factor;
            Height *= factor;
            HeightRight *= factor;
            HeightLeft *= factor;
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            double roofHeight = Height * 0.5;
            double baseY = Y + roofHeight;
            double roofPeakX = X + Width / 2;
            double roofPeakY = Y;
            double bottomY = Y + Height; // Dolna krawędź domu

            return new List<(XPoint, XPoint)>
            {
                (new XPoint(X, baseY), new XPoint(roofPeakX, roofPeakY)), // Dach - lewa krawędź
                (new XPoint(roofPeakX, roofPeakY), new XPoint(X + Width, baseY)), // Dach - prawa krawędź
                (new XPoint(X, baseY), new XPoint(X + Width, baseY)), // Podstawa dachu
                (new XPoint(X, baseY), new XPoint(X, bottomY)), // Lewa ściana domu
                (new XPoint(X + Width, baseY), new XPoint(X + Width, bottomY)), // Prawa ściana domu
                (new XPoint(X, bottomY), new XPoint(X + Width, bottomY)) // **Podstawa domu (DOLNA KRAWĘDŹ)**
            };
        }

        public (List<XPoint> Roof, List<XPoint> House) GetVertices()
        {
            // double roofHeight = Height * 0.5;
            double baseY = Y + HeightLeft;
            double roofPeakX = X + Width / 2;
            double roofPeakY = Y;
            double bottomY = Y + Height; // Dolna krawędź domu

            // Trójkąt dachu
            List<XPoint> roof = new List<XPoint>
        {
            new XPoint(X, baseY),         // Lewy dolny róg dachu
            new XPoint(roofPeakX, roofPeakY), // Szczyt dachu
            new XPoint(X + Width, baseY)  // Prawy dolny róg dachu
        };

            // Prostokąt (ściany domu)
            List<XPoint> house = new List<XPoint>
        {
            new XPoint(X, baseY),          // Lewy górny róg ściany
            new XPoint(X + Width, baseY),  // Prawy górny róg ściany
            new XPoint(X + Width, bottomY), // Prawy dolny róg ściany
            new XPoint(X, bottomY)         // Lewy dolny róg ściany
        };

            return (roof, house);
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
            (new XPoint(X, baseY), new XPoint(X + Width, baseY)) // Podstawa dachu
        };

            List<(XPoint, XPoint)> baseEdges = new()
        {
            (new XPoint(X, baseY), new XPoint(X, bottomY)), // Lewa ściana domu
            (new XPoint(X + Width, baseY), new XPoint(X + Width, bottomY)), // Prawa ściana domu
            (new XPoint(X, bottomY), new XPoint(X + Width, bottomY)) // Podstawa domu
        };

            return (roofEdges, baseEdges);
        }

        public void Transform(double scale, double offsetX, double offsetY)
        {
            X = (X * scale) + offsetX;
            Y = (Y * scale) + offsetY;
            Width *= scale;
            Height *= scale;
            HeightLeft *= scale;
            HeightRight *= scale;
        }

    }

}
