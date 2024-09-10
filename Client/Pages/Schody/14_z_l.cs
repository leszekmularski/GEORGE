using Blazor.Extensions.Canvas.Canvas2D;

namespace GEORGE.Client.Pages.Schody
{
    public abstract partial class C14_z_l
    {
        public abstract Task DrawAsync(Canvas2DContext context);
        public abstract Task<List<Point>> ReturnPoints();
        public abstract Task<List<LinePoint>> ReturnLinePoints();
    }

    //---------------------------------------------------------------- PIÓRO -------------------------------------------------------------------------------------------
    public class CSchody : Shape
    {
        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public double X { get; set; }
        public double Y { get; set; }
        public double Skala { get; set; }
        private double DlugoscOtworu { get; set; }
        private double SzerokoscOtworu { get; set; }
        private double DlugoscNaWejsciu { get; set; }
        private double WysokoscDoStropu { get; set; }
        private double WysokoscCalkowita { get; set; }
        private double LiczbaPodniesienStopni { get; set; }
        private double SzerokoscOstatniegoStopnia { get; set; }
        private double SzerokoscBieguSchodow { get; set; }

        // Output properties (results)
        private double DlugoscLiniiBiegu { get; set; }
        private double KatNachylenia { get; set; }
        private double SzerokoscSchodow { get; set; }
        private double WysokoscPodniesieniaStopnia { get; set; }
        private double GlebokoscStopnia { get; set; }
        private double PrzecietnaDlugoscKroku { get; set; }
        private double PrzestrzenSwobodnaNadGlowa { get; set; }
        private string Opis { get; set; }

        public CSchody(double x, double y, double skala, double dlugoscOtworu, double szerokoscOtworu, double dlugoscNaWejsciu, double wysokoscDoStropu, double wysokoscCalkowita, double liczbaPodniesienStopni, 
            double szerokoscOstatniegoStopnia, double szerokoscBieguSchodow, double dlugoscLiniiBiegu, double katNachylenia, double szerokoscSchodow, double wysokoscPodniesieniaStopnia, 
            double glebokoscStopnia, double przecietnaDlugoscKroku, double przestrzenSwobodnaNadGlowa , string opis)
        {

            X = x;
            Y = y;
            Skala = skala;
            DlugoscOtworu = dlugoscOtworu;
            SzerokoscOtworu = szerokoscOtworu;
            DlugoscNaWejsciu = dlugoscNaWejsciu;
            WysokoscDoStropu = wysokoscDoStropu;
            WysokoscCalkowita = wysokoscCalkowita;
            LiczbaPodniesienStopni = liczbaPodniesienStopni;
            SzerokoscOstatniegoStopnia = szerokoscOstatniegoStopnia;
            SzerokoscBieguSchodow = szerokoscBieguSchodow;

            DlugoscLiniiBiegu = dlugoscLiniiBiegu;
            KatNachylenia = katNachylenia;
            SzerokoscSchodow = szerokoscSchodow;
            WysokoscPodniesieniaStopnia = wysokoscPodniesieniaStopnia;
            GlebokoscStopnia = glebokoscStopnia;
            PrzecietnaDlugoscKroku = przecietnaDlugoscKroku;
            Opis = opis;

        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            Xpoints = new List<Point>();
            XLinePoint = new List<LinePoint>();

            double currentX = X; // Początkowa pozycja X
            double currentY = Y; // Początkowa pozycja Y

            double stepWidth = GlebokoscStopnia * Skala;  // Szerokość stopnia (długość biegu schodów)
            double stepHeight = SzerokoscBieguSchodow * Skala;  // Szerokość stopnia
            double totalHeight = WysokoscPodniesieniaStopnia * LiczbaPodniesienStopni * Skala;  // Całkowita wysokość

            // Rysujemy obrys schodów (widok od góry)
            await DrawShapeObrys(context, X, Y);

            // Wyświetlenie informacji
            await DrawTextAsync(context, X + (100) * Skala, Y - 45, $"Informacja: {Opis}");

            double delatXKrok = 0;  // Przesunięcie po osi X
            double delatYKrok = 0;  // Przesunięcie po osi Y

            // Rysowanie stopni (widok od góry)
            for (int i = 0; i < LiczbaPodniesienStopni; i++)
            {
                // Aktualizacja pozycji każdego stopnia
                currentX = X + delatXKrok;
                currentY = Y + delatYKrok;

                // Sprawdzamy, czy jesteśmy w prawym górnym rogu (trapezowe stopnie)
                if (currentX + stepWidth > X + DlugoscOtworu * Skala)
                {
                    // Rysowanie trapezowego stopnia (zawijanie schodów)
                    double topWidth = stepWidth * 0.8;  // Górna szerokość trapezu
                    double bottomWidth = stepWidth;  // Dolna szerokość trapezu

                    await DrawTrapezoidAsync(context, currentX, currentY, bottomWidth, topWidth, stepHeight);

                    // Aktualizujemy krok po osi Y, aby przesuwać się do góry
                    delatYKrok -= stepHeight;  // Przesunięcie w górę po osi Y (zawijanie schodów)
                }
                else
                {
                    // Rysowanie prostokątnego stopnia
                    await context.BeginPathAsync();
                    await context.RectAsync(currentX, currentY, stepWidth, stepHeight);
                    await context.StrokeAsync();

                    Console.WriteLine($"Stopień {i}: X = {currentX}, Y = {currentY}, Szerokość = {stepWidth}, Wysokość = {stepHeight}");
                }

                // Aktualizacja przesunięcia dla kolejnych stopni
                delatXKrok += stepWidth;  // Przesuwamy stopnie w prawo

                // Ostatni stopień (obracamy o 90 stopni w prawym dolnym rogu)
                if (i == LiczbaPodniesienStopni - 1)
                {
                    // Pozycja ostatniego stopnia (obracany o 90 stopni)
                    double lastStepX = X + DlugoscOtworu * Skala - stepHeight;
                    double lastStepY = Y + SzerokoscOtworu * Skala - stepWidth;

                    await context.SaveAsync();
                    await context.TranslateAsync(lastStepX, lastStepY);
                    await context.RotateAsync((float)(Math.PI / 2));

                    // Rysowanie obróconego stopnia
                    await context.RectAsync(0, 0, stepHeight, stepWidth);
                    await context.StrokeAsync();
                    await context.RestoreAsync();
                }
            }
        }

        // Funkcja pomocnicza do rysowania trapezów (dla schodów zabiegowych)
        private async Task DrawTrapezoidAsync(Canvas2DContext context, double x, double y, double bottomWidth, double topWidth, double height)
        {
            double halfDiff = (bottomWidth - topWidth) / 2;

            await context.BeginPathAsync();
            await context.MoveToAsync(x + halfDiff, y);  // Lewy dolny róg trapezu
            await context.LineToAsync(x + bottomWidth, y);  // Prawy dolny róg trapezu
            await context.LineToAsync(x + bottomWidth - halfDiff, y - height);  // Prawy górny róg trapezu
            await context.LineToAsync(x + halfDiff, y - height);  // Lewy górny róg trapezu
            await context.ClosePathAsync();

            await context.StrokeAsync();  // Rysowanie konturu trapezu
        }


        private async Task DrawShapeObrys(Canvas2DContext context, double offsetX, double offsetY)
        {

            await context.BeginPathAsync();

            // Draw outer rectangle
            await context.RectAsync(offsetX, offsetY, DlugoscOtworu * Skala, SzerokoscOtworu * Skala); // #OBRYS SCHODOW
            await context.RectAsync(offsetX + 1, offsetY + 1, DlugoscOtworu * Skala - 1, SzerokoscOtworu * Skala - 1); // #OBRYS SCHODOW

            AddRectanglePoints(offsetX, offsetY, DlugoscOtworu * Skala, SzerokoscOtworu * Skala);

            //   await context.ClosePathAsync();

            await context.StrokeAsync();
        }

        //private async Task DrawShapeStopnie(Canvas2DContext context, double offsetX, double offsetY, double x, double y)
        //{

        //    await context.BeginPathAsync();

        //    // Draw outer rectangle
        //    await context.RectAsync(offsetX, offsetY, GlebokoscStopnia * Skala, SzerokoscBieguSchodow * Skala); // #OBRYS STOPNIA
            
        //    AddRectanglePoints(offsetX, offsetY, GlebokoscStopnia * Skala, SzerokoscBieguSchodow * Skala); // #Ramka przeszklenia ZEW

        //    await context.StrokeAsync();
        //}

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
