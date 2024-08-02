using Blazor.Extensions.Canvas.Canvas2D;
using System.Linq.Expressions;

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

        public double Szczelina = 6;
        public double SzerSamegoSkrzydla = 0;
        public double WysSamegoSkrzydla = 0;

        public double StartYPrzekroj = 0;

        public string Typ = "";

        public double IloscZawiasow = 3;
        public string KierOtw = "N";

        public CSkrzy3Okna(string typ, string kierotw , double ilosczawiasow, double x, double y, double gruboscramsk, double szerramsk, double glebramsk, double szerramy, double szerdrzwi, double wysdrzwi,
            double wysprogu, double iloscprzeszkelen, double wielkoscprzeszklenia, double wyspiora, double skala, double startyprzekroj)
        {
            Typ = typ;
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

            IloscZawiasow = ilosczawiasow;
            KierOtw = kierotw;

        //*************************************************************************************************************************************************
        // 15 podfrezowanie
        // SzerSamegoSkrzydla = szerdrzwi - 2 * szerramy - 2; // 20 zachodznie za ramę

        SzerSamegoSkrzydla = (SzerDrzwi + Wyspiora) - 2 * SzerRamy - szerramsk;

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

            double korekcja = 0;

            double delatY = ((WysSamegoSkrzydla - 50) / (IloscPrzeszkelen + 1)) * Skala;

            if (Typ == "Typ2")
            {
                korekcja = (SzerRamSk + 50) * Skala;
                delatY = ((WysSamegoSkrzydla + 2 * SzerRamSk) / (IloscPrzeszkelen + 1)) * Skala;
            }

            double delatX = X + (SzerDrzwi / 2 - WielkoscPrzeszklenia / 2) * Skala;
            double delatYKrok = 0;

            // Draw the original shape
            await DrawShapeAsync(context, X, Y, StartYPrzekroj);

            await DrawShapeSkrzy(context, X, Y);

            await DrawTextAsync(context, X + (SzerDrzwi / 4) * Skala, Y - 45, $"Sk: {SzerSamegoSkrzydla + SzerRamSk} x Wk: {WysSamegoSkrzydla}");
            
            await DrawShapeZawiasKlamka(context, X, Y);

            for (int i = 0; i < IloscPrzeszkelen; i++)
            {

                delatYKrok += delatY;
                await DrawShapePrzesklenie(context, delatX, delatYKrok - korekcja, Y);
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

            double ramaskChowana = 0;

            if (Typ == "Typ2")
            {
                ramaskChowana = 18;//Rama odchudzona o 2 grubości płyt 9 * 2
            }
                // Draw outer rectangle
            await context.MoveToAsync(offsetX, loffsetY + 15 * Skala);
            await context.LineToAsync(offsetX + 15 * Skala, loffsetY + 15 * Skala);
            await context.LineToAsync(offsetX + 15 * Skala, loffsetY);
            await context.LineToAsync(offsetX + SzerRamSk * Skala, loffsetY);
            await context.LineToAsync(offsetX + SzerRamSk * Skala, loffsetY + (GruboscRamSk - ramaskChowana) * Skala);
            await context.LineToAsync(offsetX, loffsetY + (GruboscRamSk - ramaskChowana) * Skala);
            await context.ClosePathAsync();

            if (!lustro)
            {
                AddLinePoints(offsetX, loffsetY + 15 * Skala, offsetX + 15 * Skala, loffsetY + 15 * Skala);
                AddLinePoints(offsetX + 15 * Skala, loffsetY + 15 * Skala, offsetX + 15 * Skala, loffsetY);
                AddLinePoints(offsetX + 15 * Skala, loffsetY, offsetX + SzerRamSk * Skala, loffsetY);
                AddLinePoints(offsetX + SzerRamSk * Skala, loffsetY, offsetX + SzerRamSk * Skala, loffsetY + (GruboscRamSk - ramaskChowana) * Skala);
                AddLinePoints(offsetX + SzerRamSk * Skala, loffsetY + (GruboscRamSk - ramaskChowana) * Skala, offsetX, loffsetY + (GruboscRamSk - ramaskChowana) * Skala);
                AddLinePoints(offsetX, loffsetY + (GruboscRamSk - ramaskChowana) * Skala, offsetX, loffsetY + 15 * Skala);
            }
            await context.StrokeAsync();
        }

        private async Task DrawShapeSkrzy(Canvas2DContext context, double offsetX, double offsetY)
        {

            await context.BeginPathAsync();

            // Draw outer rectangle
            await context.RectAsync(offsetX, offsetY, (SzerSamegoSkrzydla + SzerRamSk) * Skala, WysSamegoSkrzydla * Skala); // #OBRYS CALEGO SKRZYDLA

            AddRectanglePoints(offsetX, offsetY, (SzerSamegoSkrzydla + SzerRamSk) * Skala, WysSamegoSkrzydla * Skala);

            if (Typ == "" || Typ == "Typ1")
            {
                await context.RectAsync(offsetX, offsetY, SzerRamSk * Skala, WysSamegoSkrzydla * Skala);  // #BOK LEWY
                await context.RectAsync(offsetX + SzerSamegoSkrzydla * Skala, offsetY, SzerRamSk * Skala, WysSamegoSkrzydla * Skala);  // #BOK PRAWY
                await context.RectAsync(offsetX + SzerRamSk * Skala, offsetY, (SzerSamegoSkrzydla - SzerRamSk) * Skala, SzerRamSk * Skala);  // #GORA
                await context.RectAsync(offsetX + SzerRamSk * Skala, offsetY + (WysSamegoSkrzydla - SzerRamSk) * Skala, (SzerSamegoSkrzydla - SzerRamSk) * Skala, SzerRamSk * Skala);  // #GORA

                AddRectanglePoints(offsetX, offsetY, SzerRamSk * Skala, WysSamegoSkrzydla * Skala);
                AddRectanglePoints(offsetX + SzerSamegoSkrzydla * Skala, offsetY, SzerRamSk * Skala, WysSamegoSkrzydla * Skala);
                AddRectanglePoints(offsetX + SzerRamSk * Skala, offsetY, (SzerSamegoSkrzydla - SzerRamSk) * Skala, SzerRamSk * Skala);
                AddRectanglePoints(offsetX + SzerRamSk * Skala, offsetY + (WysSamegoSkrzydla - SzerRamSk) * Skala, (SzerSamegoSkrzydla - SzerRamSk) * Skala, SzerRamSk * Skala);
            }

            //   await context.ClosePathAsync();

            await context.StrokeAsync();
        }

        private async Task DrawShapePrzesklenie(Canvas2DContext context, double offsetX, double offsetY, double Y)
        {

            await context.BeginPathAsync();

            // Draw outer rectangle
            await context.RectAsync(offsetX - ((10 + Wyspiora / 2) * Skala), offsetY + Y - ((SzerRamSk / 2 * Skala) + (Wyspiora / 2 + 10) * Skala), (WielkoscPrzeszklenia + 20) * Skala, (WielkoscPrzeszklenia + 20) * Skala); // #Ramka przeszklenia ZEW
            await context.RectAsync(offsetX - (Wyspiora / 2 * Skala), offsetY + Y - (SzerRamSk / 2 * Skala + (Wyspiora / 2 * Skala)), WielkoscPrzeszklenia * Skala, WielkoscPrzeszklenia * Skala);  // #Ramka przeszklenia WEW
                                                                                                                                                                                                    //   await context.ClosePathAsync();
            AddRectanglePoints(offsetX - ((10 + Wyspiora / 2) * Skala), offsetY + Y - ((SzerRamSk / 2 * Skala) + (Wyspiora / 2 + 10) * Skala), (WielkoscPrzeszklenia + 20) * Skala, (WielkoscPrzeszklenia + 20) * Skala); // #Ramka przeszklenia ZEW
            AddRectanglePoints(offsetX - (Wyspiora / 2 * Skala), offsetY + Y - (SzerRamSk / 2 * Skala + (Wyspiora / 2 * Skala)), WielkoscPrzeszklenia * Skala, WielkoscPrzeszklenia * Skala);  // #Ramka przeszklenia WEW

            await context.StrokeAsync();
        }

        private async Task DrawShapeZawiasKlamka(Canvas2DContext context, double offsetX, double offsetY)
        {

            if (KierOtw == "N") return;

            //   public double IloscZawiasow = 3;
            double xZawiasu = 0;
            double xKlamki1 = 0;
            double xKlamki2 = 0;

            if (KierOtw == "L")
            {
                xZawiasu = - 20 * Skala;
                xKlamki1 = (SzerSamegoSkrzydla + SzerRamSk/2 - 20) * Skala;
                xKlamki2 = (SzerSamegoSkrzydla + SzerRamSk/2 + 10 - 120) * Skala;
            }
            else if(KierOtw == "R")
            {
                xZawiasu = (SzerSamegoSkrzydla + SzerRamSk) * Skala;
                xKlamki1 = (SzerRamSk / 2 - 20) * Skala;
                xKlamki2 = (SzerRamSk / 2) * Skala;
            }

            // Draw outer rectangle
            await context.BeginPathAsync();

           
            switch (IloscZawiasow)
            {
                case 1:
                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((SzerRamSk / 2 * Skala) + (Wyspiora / 2 + 20) * Skala), 20 * Skala, 110 * Skala); // #Zawias 1
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((SzerRamSk / 2 * Skala) + (Wyspiora / 2 + 20) * Skala), 20 * Skala, 110 * Skala); // #RZawias 1
                    break;
                case 2:
                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((SzerRamSk / 2 * Skala) + (Wyspiora / 2 + 20) * Skala), 20 * Skala, 110 * Skala); // #Zawias 1
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((SzerRamSk / 2 * Skala) + (Wyspiora / 2 + 20) * Skala), 20 * Skala, 110 * Skala); // #RZawias 1

                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 2
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 2
                    break;
                case 3:
                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((SzerRamSk / 2 * Skala) + (Wyspiora / 2 + 20) * Skala), 20 * Skala, 110 * Skala); // #Zawias 1
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((SzerRamSk / 2 * Skala) + (Wyspiora / 2 + 20) * Skala), 20 * Skala, 110 * Skala); // #RZawias 1

                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 2
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 2

                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla / 2 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 3
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla / 2 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 3
                    break;
                case 4:
                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((SzerRamSk / 2 * Skala) + (Wyspiora / 2 + 20) * Skala), 20 * Skala, 110 * Skala); // #Zawias 1
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((SzerRamSk / 2 * Skala) + (Wyspiora / 2 + 20) * Skala), 20 * Skala, 110 * Skala); // #RZawias 1

                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 2
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 2

                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla / 3 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 3
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla / 3 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 3

                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla / 3 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala) + ((WysSamegoSkrzydla / 3 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 4
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla / 3 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala) + ((WysSamegoSkrzydla / 3 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 4
                    break;
                case 5:
                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((SzerRamSk / 2 * Skala) + (Wyspiora / 2 + 20) * Skala), 20 * Skala, 110 * Skala); // #Zawias 1
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((SzerRamSk / 2 * Skala) + (Wyspiora / 2 + 20) * Skala), 20 * Skala, 110 * Skala); // #RZawias 1

                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 2
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 2

                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla / 4 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 3
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla / 4 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 3

                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla / 4 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala) + ((WysSamegoSkrzydla / 4 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 4
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla / 4 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala) + ((WysSamegoSkrzydla / 4 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 4

                    await context.RectAsync(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla / 2 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala) + ((WysSamegoSkrzydla / 4 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 5
                    AddRectanglePoints(offsetX + xZawiasu, offsetY + ((WysSamegoSkrzydla / 2 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala) + ((WysSamegoSkrzydla / 4 - (SzerRamSk / 2 * Skala) - (Wyspiora / 2 + 120)) * Skala), 20 * Skala, 110 * Skala); // #Zawias 5
                    break;
                default:
                    break;
            }

            //Zamek,

            await context.RectAsync(offsetX + xKlamki1, offsetY + ((WysSamegoSkrzydla / 2 - (SzerRamSk / 2) - (Wyspiora / 2 + 120)) * Skala), 40 * Skala, 200 * Skala); // #Szyld
            AddRectanglePoints(offsetX + xKlamki1, offsetY + ((WysSamegoSkrzydla / 2 - (SzerRamSk / 2) - (Wyspiora / 2 + 120)) * Skala), 40 * Skala, 200 * Skala); // #Szyld

            await context.RectAsync(offsetX + xKlamki2, offsetY + ((WysSamegoSkrzydla / 2 - ((SzerRamSk - 90) / 2) - (Wyspiora / 2 + 120)) * Skala), 120 * Skala, 20 * Skala); // #Klamka
            AddRectanglePoints(offsetX + xKlamki2, offsetY + ((WysSamegoSkrzydla / 2 - ((SzerRamSk - 90) / 2) - (Wyspiora / 2 + 120)) * Skala), 120 * Skala, 20 * Skala); // #Klamka

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
