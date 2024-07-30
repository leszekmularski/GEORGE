using Blazor.Extensions.Canvas.Canvas2D;

namespace GEORGE.Client.Pages.Drzwi
{
    public abstract class ShapeSkrzy
    {
        public abstract Task DrawAsync(Canvas2DContext context);
        public abstract Task<List<Point>> ReturnPoints();
        public abstract Task<List<LinePoint>> ReturnLinePoints();
    }

    //---------------------------------------------------------------- PIÓRO -------------------------------------------------------------------------------------------
    public class CSkrzy3Okna : ShapeSkrzy
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public double X { get; set; }
        public double Y { get; set; }
        public double SzerRamSk { get; set; }
        public double GruboscRamSk { get; set; }
        public double SzerRamy { get; set; }
        public double GlebRamSk { get; set; }
        public double GrubPiora { get; set; }
        public double SzerDrzwi { get; set; }
        public double WysDrzwi { get; set; }
        public double WysProgu { get; set; }
        public double IloscPrzeszkelen { get; set; }
        public double WielkoscPrzeszklenia { get; set; }
        public double Wyspiora { get; set; }
        public double Skala { get; set; }

        double Szczelina = 6;
        double SzerSamegoSkrzydla = 0;
        double WysSamegoSkrzydla = 0;

        double StartYPrzekroj = 0;

        public CSkrzy3Okna(double x, double y, double gruboscramsk, double szerramsk, double glebramsk, double szerramy, double szerdrzwi, double wysdrzwi, 
            double wysprogu, double iloscprzeszkelen, double wielkoscprzeszklenia, double wyspiora , double skala, double startyprzekroj)
        {
       
            X = x; 
            Y = y;
            SzerRamSk = szerramsk;
            GlebRamSk = glebramsk;
            GruboscRamSk = gruboscramsk;
            SzerRamy = szerramy;
            SzerDrzwi = szerdrzwi;
            WysDrzwi = wysdrzwi;
            WysProgu = wysprogu;
            IloscPrzeszkelen = iloscprzeszkelen;
            WielkoscPrzeszklenia = wielkoscprzeszklenia;
            Wyspiora = wyspiora;
            Skala = skala;
            StartYPrzekroj = startyprzekroj;

            //*************************************************************************************************************************************************

            SzerSamegoSkrzydla = szerdrzwi - 2 * szerramy - 2 * Szczelina; // 20 zachodznie za ramę

            if (wysprogu > 0) 
            {
                WysSamegoSkrzydla = wysdrzwi - szerramy - Szczelina - wysprogu / 2;
            }
            else
            {
                WysSamegoSkrzydla = wysdrzwi - szerramy - Szczelina;
            }
            
        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            Xpoints = new List<Point>();
            XLinePoint = new List<LinePoint>();

            double delatY = ((WysSamegoSkrzydla - 50)  / (IloscPrzeszkelen + 1)) * Skala;
            double delatX = X + (SzerDrzwi / 2 - WielkoscPrzeszklenia / 2 ) * Skala;
            double delatYKrok = 0;

                  // Draw the original shape
            await DrawShapeAsync(context, X, Y, StartYPrzekroj);

            await DrawShapeSkrzy(context, X, Y);

            await DrawTextAsync(context, X + (SzerDrzwi / 4) * Skala, Y - 45, $"Sk: {SzerSamegoSkrzydla} x Wk: {WysSamegoSkrzydla}");

            for (int i = 0; i < IloscPrzeszkelen ; i++)
            {

                delatYKrok += delatY;
                await DrawShapePrzesklenie(context, delatX, delatYKrok, Y);
                //Console.WriteLine(delatYKrok);
            }

            // Draw the mirrored shape
            // Translate the context to the position where the mirrored shape will be drawn
            await context.TranslateAsync(0, 0);

            // Scale by -1 on the X axis to mirror the shape
            await context.ScaleAsync(-1, 1);

            await DrawShapeAsync(context, -((SzerDrzwi + Wyspiora) - 2 * SzerRamy) * Skala - X, Y, StartYPrzekroj, true);

            await context.RestoreAsync();
 
            Console.WriteLine($"DrawAsync/SK - SzerDrzwi: {SzerDrzwi} x {WysDrzwi} / Skala: {Skala}");
        }

        private async Task DrawShapeAsync(Canvas2DContext context, double offsetX, double offsetY, double loffsetY, bool lustro = false)
        {

            await context.BeginPathAsync();
;
            // Draw outer rectangle
            await context.MoveToAsync(offsetX, loffsetY + 15 * Skala);
            await context.LineToAsync(offsetX + 15 * Skala, loffsetY + 15 * Skala);
            await context.LineToAsync(offsetX + 15 * Skala, loffsetY);
            await context.LineToAsync(offsetX + SzerRamSk * Skala, loffsetY);
            await context.LineToAsync(offsetX + SzerRamSk * Skala, loffsetY + GruboscRamSk * Skala);
            await context.LineToAsync(offsetX, loffsetY + GruboscRamSk * Skala);
            await context.ClosePathAsync();

            if (!lustro)
            {
                AddLinePoints(offsetX, loffsetY + 15 * Skala, offsetX + 15 * Skala, loffsetY + 15 * Skala);
                AddLinePoints(offsetX + 15 * Skala, loffsetY + 15 * Skala, offsetX + 15 * Skala, loffsetY);
                AddLinePoints(offsetX + 15 * Skala, loffsetY, offsetX + SzerRamSk * Skala, loffsetY);
                AddLinePoints(offsetX + SzerRamSk * Skala, loffsetY, offsetX + SzerRamSk * Skala, loffsetY + GruboscRamSk * Skala);
                AddLinePoints(offsetX + SzerRamSk * Skala, loffsetY + GruboscRamSk * Skala, offsetX, loffsetY + GruboscRamSk * Skala);
                AddLinePoints(offsetX, loffsetY + GruboscRamSk * Skala, offsetX, loffsetY + 15 * Skala);
            }
            await context.StrokeAsync();
        }

        private async Task DrawShapeSkrzy(Canvas2DContext context, double offsetX, double offsetY)
        {

            await context.BeginPathAsync();

            // Draw outer rectangle
            await context.RectAsync(offsetX, offsetY, SzerSamegoSkrzydla * Skala, WysSamegoSkrzydla * Skala); // #OBRYS CALEGO SKRZYDLA
            await context.RectAsync(offsetX, offsetY, SzerRamSk * Skala, WysSamegoSkrzydla * Skala);  // #BOK LEWY
            await context.RectAsync(offsetX + SzerSamegoSkrzydla * Skala, offsetY, SzerRamSk * Skala, WysSamegoSkrzydla * Skala);  // #BOK PRAWY
            await context.RectAsync(offsetX + SzerRamSk * Skala, offsetY, (SzerSamegoSkrzydla - SzerRamSk) * Skala, SzerRamSk * Skala);  // #GORA
            await context.RectAsync(offsetX + SzerRamSk * Skala, offsetY + (WysSamegoSkrzydla - SzerRamSk) * Skala, (SzerSamegoSkrzydla - SzerRamSk) * Skala, SzerRamSk * Skala);  // #GORA
            //   await context.ClosePathAsync();

            AddRectanglePoints(offsetX, offsetY, SzerSamegoSkrzydla * Skala, WysSamegoSkrzydla * Skala);
            AddRectanglePoints(offsetX, offsetY, SzerRamSk * Skala, WysSamegoSkrzydla * Skala);
            AddRectanglePoints(offsetX + SzerSamegoSkrzydla * Skala, offsetY, SzerRamSk * Skala, WysSamegoSkrzydla * Skala);
            AddRectanglePoints(offsetX + SzerRamSk * Skala, offsetY, (SzerSamegoSkrzydla - SzerRamSk) * Skala, SzerRamSk * Skala);
            AddRectanglePoints(offsetX + SzerRamSk * Skala, offsetY + (WysSamegoSkrzydla - SzerRamSk) * Skala, (SzerSamegoSkrzydla - SzerRamSk) * Skala, SzerRamSk * Skala);

            await context.StrokeAsync();
        }

        private async Task DrawShapePrzesklenie(Canvas2DContext context, double offsetX, double offsetY, double Y)
        {
    
            await context.BeginPathAsync();

            // Draw outer rectangle
            await context.RectAsync(offsetX - 2, offsetY + Y - (SzerRamSk / 2 * Skala) - 2, WielkoscPrzeszklenia * Skala + 4, WielkoscPrzeszklenia * Skala + 4); // #Ramka przeszklenia ZEW
            await context.RectAsync(offsetX, offsetY + Y - (SzerRamSk / 2 * Skala), WielkoscPrzeszklenia * Skala, WielkoscPrzeszklenia * Skala);  // #Ramka przeszklenia WEW
            //   await context.ClosePathAsync();
            AddRectanglePoints(offsetX - 2, offsetY + Y - (SzerRamSk / 2 * Skala) - 2, WielkoscPrzeszklenia * Skala + 4, WielkoscPrzeszklenia * Skala + 4);
            AddRectanglePoints(offsetX, offsetY + Y - (SzerRamSk / 2 * Skala), WielkoscPrzeszklenia * Skala, WielkoscPrzeszklenia * Skala);

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

        public override Task<List<Point>> ReturnPoints()
        {
            return Task.FromResult(Xpoints ?? new List<Point>());
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

}
