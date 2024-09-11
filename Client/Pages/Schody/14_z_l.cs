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
            double stepHeight = SzerokoscBieguSchodow * Skala;  // Wysokość stopnia (szerokość biegu schodów)

            // Rysujemy obrys schodów (widok od góry)
            await DrawShapeObrys(context, X, Y);

            // Wyświetlenie informacji
            await DrawTextAsync(context, X + (100) * Skala, Y - 45, $"Informacja: {Opis}");

            // Obliczenie liczby pionowych stopni (zmieszczących się w SzerokoscOtworu)
            double pionoweStopnie = (int)Math.Floor(SzerokoscOtworu / stepHeight);

            // Jeżeli liczba pionowych stopni jest większa niż liczba wszystkich stopni, wszystkie będą pionowe
            if (pionoweStopnie > LiczbaPodniesienStopni)
            {
                pionoweStopnie = LiczbaPodniesienStopni;
            }

            // Liczba poziomych stopni
            double poziomeStopnie = LiczbaPodniesienStopni - pionoweStopnie;

            double delatXKrok = 0;  // Przesunięcie po osi X
            double delatYKrok = 0;  // Przesunięcie po osi Y

            // Rysowanie poziomych stopni
            for (int i = 0; i < poziomeStopnie; i++)
            {
                // Rysowanie prostokątnego stopnia poziomo
                await context.BeginPathAsync();
                await context.RectAsync(currentX, currentY, stepWidth, stepHeight); // W poziomie: Szerokość Biegu Schodów jest wzdłuż osi X
                await context.StrokeAsync();

                Console.WriteLine($"Poziomy stopień {i + pionoweStopnie}: X = {currentX}, Y = {currentY}, Szerokość = {stepWidth}, Wysokość = {stepHeight}");

                // Przesuwamy się w prawo po osi X
                currentX += stepWidth;
            }

            // Rysowanie stopni trapezowych na rogu
            //int liczbaTrapezow = 6;
            //double trapezStartWidth = stepWidth;  // Pełna szerokość na początku (poziome stopnie)
            //double trapezEndWidth = stepHeight;  // Pełna szerokość na końcu (pionowe stopnie)
            //double trapezHeight = stepHeight / liczbaTrapezow;  // Wysokość pojedynczego trapezu

            //for (int i = 0; i < liczbaTrapezow; i++)
            //{
            //    // Obliczamy szerokości górną i dolną dla każdego trapezu
            //    double topWidth = trapezStartWidth - (i * (trapezStartWidth - trapezEndWidth) / liczbaTrapezow);
            //    double bottomWidth = trapezStartWidth - ((i + 1) * (trapezStartWidth - trapezEndWidth) / liczbaTrapezow);

            //    // Rysowanie trapezu
            //    await DrawTrapezoidAsync(context, currentX, currentY, topWidth, bottomWidth, trapezHeight);

            //    Console.WriteLine($"Trapezowy stopień {i}: X = {currentX}, Y = {currentY}, TopWidth = {topWidth}, BottomWidth = {bottomWidth}, Height = {trapezHeight}");

            //    // Przesunięcie po osi Y dla kolejnego trapezu
            //    currentY += trapezHeight;
            //}

            // Po trapezach resetujemy pozycje X i Y dla pionowych stopni (po bocznej krawędzi)
            currentX = X + DlugoscOtworu * Skala - stepHeight; // Przesunięcie na dolną krawędź prostokąta
            currentY = Y + stepHeight; // Ustawiamy Y poniżej trapezów

            // Rysowanie pionowych stopni
            for (int i = 0; i < pionoweStopnie; i++)
            {
                // Rysujemy pionowe stopnie wzdłuż lewej krawędzi
                currentY = Y + delatYKrok;

                // Rysowanie prostokątnego stopnia pionowo
                await context.BeginPathAsync();
                await context.RectAsync(currentX, currentY, stepHeight, stepWidth); // W pionie: Szerokość Biegu Schodów jest wzdłuż osi Y
                await context.StrokeAsync();

                Console.WriteLine($"Pionowy stopień {i}: X = {currentX}, Y = {currentY}, Szerokość = {stepHeight}, Wysokość = {stepWidth}");

                // Przesuwamy się w dół
                delatYKrok += stepWidth;
            }
        }

        // Funkcja pomocnicza do rysowania trapezów (dla przejścia stopni pionowych na poziome)
        private async Task DrawTrapezoidAsync(Canvas2DContext context, double x, double y, double topWidth, double bottomWidth, double height)
        {
            double halfDiff = (topWidth - bottomWidth) / 2;

            await context.BeginPathAsync();
            await context.MoveToAsync(x + halfDiff, y);  // Lewy dolny róg trapezu
            await context.LineToAsync(x + bottomWidth + halfDiff, y);  // Prawy dolny róg trapezu
            await context.LineToAsync(x + topWidth, y - height);  // Prawy górny róg trapezu
            await context.LineToAsync(x, y - height);  // Lewy górny róg trapezu
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
