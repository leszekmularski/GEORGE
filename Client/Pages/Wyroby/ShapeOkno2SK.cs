using Blazor.Extensions.Canvas.Canvas2D;
using static iText.IO.Util.IntHashtable;

namespace GEORGE.Client.Pages.Wyroby
{
    public class COKN2SK68 : ShapeOkno
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public List<WymiaryOpis>? XWymiaryOpis;
        public double X { get; set; } //Margines LEWY
        public double Y { get; set; } //Margines GORNY
        public double Width { get; set; }
        public double Height { get; set; }
        public double LiniaPodzialuSkrzydel { get; set; }
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

        private double ZachodzenieZaSiebieSkrzydel = 13; //Zachodzienie skrzydeł za siebie w oknach bez słupka stałego

        //-------------------------------------------------------------  ZMIENNE WEW  ----------------------------------------------------------------------------------------
        private double WymiarWewOknaWidth1SK = 0; // od tego wymiaru liczymy szklenie - wymiar szkła WymiarWewOknaWidth - LuzPomiedzySkrzyASzyba = ??
        private double WymiarWewOknaHeight1SK = 0;

        private double WymiarZewOknaWidth1SK = 0;
        private double WymiarZewOknaHeight1SK = 0;

        private double WymiarWewOknaWidth2SK = 0; // od tego wymiaru liczymy szklenie - wymiar szkła WymiarWewOknaWidth - LuzPomiedzySkrzyASzyba = ??
        private double WymiarWewOknaHeight2SK = 0;

        private double WymiarZewOknaWidth2SK = 0;
        private double WymiarZewOknaHeight2SK = 0;

        public COKN2SK68(double x, double y, double szerokosc, double wysokosc, double liniaPodzialuSkrzydel, string otwdowenatrz, string sposotwierania, double skala)
        {
            X = x;
            Y = y;
            Width = szerokosc;
            Height = wysokosc;
            OtwDoWenatrz = otwdowenatrz;
            SposOtwierania = sposotwierania;
            LiniaPodzialuSkrzydel = liniaPodzialuSkrzydel;
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

            await DrawShapeObrSkrzydloAsync2SK(context, X, Y , 1); // Skrzydło 1
            await DrawShapeObrSkrzydloAsync2SK(context, X, Y, 2); // Skrzydło 2

            Console.WriteLine($"DrawAsync - SzerDrzwi: {Width} x {Height} / Skala: {Skala}");

            await AddWymInformacja(WymiarWewOknaWidth1SK + (2 * WysPiora - 2 * LuzPomiedzySkrzyASzyba), WymiarWewOknaHeight1SK + (2 * WysPiora - 2 * LuzPomiedzySkrzyASzyba), 
                WymiarZewOknaWidth1SK, WymiarZewOknaHeight1SK, WymiarWewOknaWidth1SK, WymiarWewOknaHeight1SK, Guid.NewGuid().ToString());

            await AddWymInformacja(WymiarWewOknaWidth2SK + (2 * WysPiora - 2 * LuzPomiedzySkrzyASzyba), WymiarWewOknaHeight2SK + (2 * WysPiora - 2 * LuzPomiedzySkrzyASzyba), 
                WymiarZewOknaWidth2SK, WymiarZewOknaHeight2SK, WymiarWewOknaWidth2SK, WymiarWewOknaHeight2SK, Guid.NewGuid().ToString());

        }
        public override Task<List<Point>> ReturnPoints()
        {
            return Task.FromResult(Xpoints ?? new List<Point>());
        }
        private async Task DrawSposOtwLines(Canvas2DContext context, double xWewSkrzy, double yWewSkrzyd, string sposoOTW, double WymiarWewOknaWidth)
        {
            if (sposoOTW == "RUP")
            {
                // Rysowanie linii promienistych
                await context.BeginPathAsync();
                await context.SetStrokeStyleAsync("black"); // Kolor linii
                await context.SetLineWidthAsync(1);
                await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię

                // WymiarWewOknaWidth, WymiarWewOknaHeight, SzerokoscRamy, SzerokoscSkrzydla, SkrzydloMniejszczeO

                // Rysowanie linii
                await context.MoveToAsync(xWewSkrzy, yWewSkrzyd);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + (WymiarWewOknaHeight1SK / 2 * Skala));
                await context.LineToAsync(xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight1SK * Skala);
                await context.StrokeAsync();

                await context.MoveToAsync(xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight1SK * Skala);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth / 2 * Skala, yWewSkrzyd);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight1SK * Skala);
                await context.StrokeAsync();

                if (sposoOTW.Length > 2)
                    await DrawKlamkaLines(context, xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight1SK / 2 * Skala, 'P');

                //AddLinePoints(xWewSkrzy, yWewSkrzyd, xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaWidth / 2 * Skala, "dashed"); //Generuj linie pod DXF
                //AddLinePoints(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaWidth / 2 * Skala, xWewSkrzy, yWewSkrzyd + WymiarWewOknaWidth * Skala, "dashed"); //Generuj linie pod DXF
                //AddLinePoints(xWewSkrzy, yWewSkrzyd + WymiarWewOknaWidth * Skala, xWewSkrzy, yWewSkrzyd + WymiarWewOknaWidth * Skala, "dashed"); //Generuj linie pod DXF
            }
            else if (sposoOTW == "RUL")
            {
                // Rysowanie linii promienistych
                await context.BeginPathAsync();
                await context.SetStrokeStyleAsync("black"); // Kolor linii
                await context.SetLineWidthAsync(1);
                await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię

                // WymiarWewOknaWidth, WymiarWewOknaHeight, SzerokoscRamy, SzerokoscSkrzydla, SkrzydloMniejszczeO

                // Rysowanie linii
                await context.MoveToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd);
                await context.LineToAsync(xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight1SK / 2 * Skala);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight1SK * Skala);
                await context.StrokeAsync();

                await context.MoveToAsync(xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight1SK * Skala);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth / 2 * Skala, yWewSkrzyd);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight1SK * Skala);
                await context.StrokeAsync();

                if (sposoOTW.Length > 2)
                    await DrawKlamkaLines(context, xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight1SK / 2 * Skala, 'L');

            }
            else if (sposoOTW == "U")
            {
                // Rysowanie linii promienistych
                await context.BeginPathAsync();
                await context.SetStrokeStyleAsync("black"); // Kolor linii
                await context.SetLineWidthAsync(1);
                await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię

                // WymiarWewOknaWidth, WymiarWewOknaHeight, SzerokoscRamy, SzerokoscSkrzydla, SkrzydloMniejszczeO

                await context.MoveToAsync(xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight1SK * Skala);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth / 2 * Skala, yWewSkrzyd);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight1SK * Skala);
                await context.StrokeAsync();

                if (sposoOTW.Length > 2)
                    await DrawKlamkaLines(context, xWewSkrzy + WymiarWewOknaWidth / 2 * Skala, yWewSkrzyd, 'U');

            }
            else if (sposoOTW == "RP")
            {
                // Rysowanie linii promienistych
                await context.BeginPathAsync();
                await context.SetStrokeStyleAsync("black"); // Kolor linii
                await context.SetLineWidthAsync(1);
                await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię

                // WymiarWewOknaWidth, WymiarWewOknaHeight, SzerokoscRamy, SzerokoscSkrzydla, SkrzydloMniejszczeO

                // Rysowanie linii
                await context.MoveToAsync(xWewSkrzy, yWewSkrzyd);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight1SK / 2 * Skala);
                await context.LineToAsync(xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight1SK * Skala);
                await context.StrokeAsync();

                if (sposoOTW.Length > 2)
                    await DrawKlamkaLines(context, xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight1SK / 2 * Skala, 'P');

            }
            else if (sposoOTW == "RL")
            {
                // Rysowanie linii promienistych
                await context.BeginPathAsync();
                await context.SetStrokeStyleAsync("black"); // Kolor linii
                await context.SetLineWidthAsync(1);
                await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię

                // WymiarWewOknaWidth, WymiarWewOknaHeight, SzerokoscRamy, SzerokoscSkrzydla, SkrzydloMniejszczeO

                // Rysowanie linii
                await context.MoveToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd);
                await context.LineToAsync(xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight1SK / 2 * Skala);
                await context.LineToAsync(xWewSkrzy + WymiarWewOknaWidth * Skala, yWewSkrzyd + WymiarWewOknaHeight1SK * Skala);
                await context.StrokeAsync();

                if (sposoOTW.Length > 2)
                    await DrawKlamkaLines(context, xWewSkrzy, yWewSkrzyd + WymiarWewOknaHeight1SK / 2 * Skala, 'L');

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
            else if (strona == 'P')
            {
                await context.RectAsync(xOsObr + 10 * Skala, yOsObr - 10 * Skala, -90 * Skala, 10 * Skala); // #1
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
        private async Task DrawShapeObrSkrzydloAsync2SK(Canvas2DContext context, double offsetX, double offsetY, int numer_skrzydla)
        {
            await context.BeginPathAsync();
            await context.SetLineWidthAsync(3);
            await context.SetStrokeStyleAsync("black"); // Kolor linii
            await context.SetLineDashAsync(new float[] { });

            string[] spsOTW = SposOtwierania.Split('/');

            if (numer_skrzydla ==  1)
            {
                double podzialLiniaX = LiniaPodzialuSkrzydel;

                WymiarZewOknaWidth1SK = podzialLiniaX - SzerokoscSkrzydla + SkrzydloMniejszczeO_Lewa + ZachodzenieZaSiebieSkrzydel;
                WymiarZewOknaHeight1SK = Height - 2 * SzerokoscSkrzydla + SkrzydloMniejszczeO_Gora + SkrzydloMniejszczeO_Dol;

                WymiarWewOknaWidth1SK = WymiarZewOknaWidth1SK - 2 * GrboscRdzeniaSkrzydla - 2 * WysPiora;
                WymiarWewOknaHeight1SK = WymiarZewOknaHeight1SK - 2 * GrboscRdzeniaSkrzydla - 2 * WysPiora;

                // Draw outer rectangle
                await context.RectAsync(offsetX + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Lewa) * Skala, offsetY + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Gora) * Skala,
                    WymiarZewOknaWidth1SK * Skala, WymiarZewOknaHeight1SK * Skala); // #1

                await context.RectAsync(offsetX + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Lewa + SzerokoscSkrzydla) * Skala, offsetY + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Gora + SzerokoscSkrzydla) * Skala,
                    WymiarWewOknaWidth1SK * Skala, WymiarWewOknaHeight1SK * Skala); // #2

                await context.StrokeAsync();

                if (spsOTW != null && spsOTW.Count() > 0)
                {
                    await DrawSposOtwLines(context, offsetX + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Lewa + SzerokoscSkrzydla) * Skala,
                    offsetY + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Gora + SzerokoscSkrzydla) * Skala, spsOTW[0], WymiarWewOknaWidth1SK); // Sposób otwierania
                }

            }
            else if(numer_skrzydla == 2)
            {
                double podzialLiniaX = LiniaPodzialuSkrzydel;// Width - LiniaPodzialuSkrzydel;

                offsetX = offsetX + ((podzialLiniaX - SzerokoscSkrzydla + SkrzydloMniejszczeO_Prawa - ZachodzenieZaSiebieSkrzydel) * Skala);//50 zachodzenie za siebie skrzydeł

                WymiarZewOknaWidth2SK = Width - LiniaPodzialuSkrzydel - SzerokoscSkrzydla + SkrzydloMniejszczeO_Prawa + ZachodzenieZaSiebieSkrzydel;
                WymiarZewOknaHeight2SK = Height - 2 * SzerokoscSkrzydla + SkrzydloMniejszczeO_Gora + SkrzydloMniejszczeO_Dol;

                WymiarWewOknaWidth2SK = WymiarZewOknaWidth2SK - 2 * GrboscRdzeniaSkrzydla - 2 * WysPiora;
                WymiarWewOknaHeight2SK = WymiarZewOknaHeight2SK - 2 * GrboscRdzeniaSkrzydla - 2 * WysPiora;

                // Draw outer rectangle
                await context.RectAsync(offsetX + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Lewa) * Skala, offsetY + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Gora) * Skala,
                    WymiarZewOknaWidth2SK * Skala, WymiarZewOknaHeight2SK * Skala); // #1

                await context.RectAsync(offsetX + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Lewa + SzerokoscSkrzydla) * Skala, offsetY + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Gora + SzerokoscSkrzydla) * Skala,
                    WymiarWewOknaWidth2SK * Skala, WymiarWewOknaHeight2SK * Skala); // #2

                //Poniże zmienne pod dxf
                //AddRectanglePoints(offsetX + SkrzydloMniejszczeO * Skala, offsetY + SkrzydloMniejszczeO * Skala, (Width - 2 * SkrzydloMniejszczeO) * Skala, (Height - 2 * SkrzydloMniejszczeO) * Skala); // #1
                //AddRectanglePoints(offsetX + (SkrzydloMniejszczeO + SzerokoscSkrzydla) * Skala, offsetY + (SkrzydloMniejszczeO + SzerokoscSkrzydla) * Skala, (WymiarWewOknaWidth) * Skala, (WymiarWewOknaHeight) * Skala); // #2
                await context.StrokeAsync();

                if (spsOTW != null && spsOTW.Count() > 1)
                {
                    await DrawSposOtwLines(context, offsetX + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Lewa + SzerokoscSkrzydla) * Skala,
                    offsetY + (SzerokoscSkrzydla - SkrzydloMniejszczeO_Gora + SzerokoscSkrzydla) * Skala, spsOTW[1], WymiarWewOknaWidth2SK); // Sposób otwierania
                } 

 
            }

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
