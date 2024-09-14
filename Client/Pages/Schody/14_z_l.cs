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
        private double KatZabiegu { get; set; }
        private double IloscSchodowZabiegowych { get; set; }

        public CSchody(double x, double y, double skala, double dlugoscOtworu, double szerokoscOtworu, double dlugoscNaWejsciu, double wysokoscDoStropu, double wysokoscCalkowita, double liczbaPodniesienStopni, 
            double szerokoscOstatniegoStopnia, double szerokoscBieguSchodow, double dlugoscLiniiBiegu, double katNachylenia, double szerokoscSchodow, double wysokoscPodniesieniaStopnia, 
            double glebokoscStopnia, double przecietnaDlugoscKroku, double przestrzenSwobodnaNadGlowa , string opis, double katZabiegu, double iloscSchodowZabiegowych)
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
            IloscSchodowZabiegowych = iloscSchodowZabiegowych;

            DlugoscLiniiBiegu = dlugoscLiniiBiegu;
            KatNachylenia = katNachylenia;
            SzerokoscSchodow = szerokoscSchodow;
            WysokoscPodniesieniaStopnia = wysokoscPodniesieniaStopnia;
            GlebokoscStopnia = glebokoscStopnia;
            PrzecietnaDlugoscKroku = przecietnaDlugoscKroku;
            Opis = opis;
            KatZabiegu = katZabiegu;
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
            await DrawTextAsync(context, X + 10, Y + 45, $"Informacja: {Opis}");


            // Tworzymy instancję klasy Wzory
            Wzory wzory = new Wzory();

            // Obliczamy promień okręgu
            double Radius = wzory.WartoscZaczepPromnienia(SzerokoscBieguSchodow, KatZabiegu / 2);

            // DlugoscOtworu / GlebokoscStopnia
            double szukanyStopien = (int)Math.Ceiling((DlugoscOtworu - Radius) / GlebokoscStopnia);

            Console.WriteLine($"Radius wylicz: {Radius} / Radius real: {DlugoscOtworu - szukanyStopien * GlebokoscStopnia} GlebokoscStopnia:{GlebokoscStopnia} szukanyStopien: {szukanyStopien}");

            Radius = DlugoscOtworu - szukanyStopien * GlebokoscStopnia;

            // Obliczenie liczby pionowych stopni (zmieszczących się w SzerokoscOtworu)
            double pionoweStopnie = (int)Math.Ceiling((SzerokoscOtworu - Radius) / GlebokoscStopnia);

            // Jeżeli liczba pionowych stopni jest większa niż liczba wszystkich stopni, wszystkie będą pionowe
            if (pionoweStopnie > LiczbaPodniesienStopni)
            {
                pionoweStopnie = LiczbaPodniesienStopni;
            }
            //await DrawTextAsync(context, X + 10, Y + 85, $"pionoweStopnie: {pionoweStopnie} {SzerokoscOtworu} / {GlebokoscStopnia}");

            double polawaIloscSchodowZabiegowychPoz = (int)Math.Floor(IloscSchodowZabiegowych / 2);

            double polawaIloscSchodowZabiegowychPion = IloscSchodowZabiegowych - polawaIloscSchodowZabiegowychPoz;

            //pionoweStopnie = pionoweStopnie - (IloscSchodowZabiegowych - polawaIloscSchodowZabiegowychPion);

            await DrawTextAsync(context, X + 10, Y + 105, $"pionoweStopnie: {pionoweStopnie}");

            // Liczba poziomych stopni
            double poziomeStopnie = szukanyStopien;// LiczbaPodniesienStopni - pionoweStopnie - IloscSchodowZabiegowych;

            //double delatXKrok = 0;  // Przesunięcie po osi X ???
            double delatYKrok = 0;  // Przesunięcie po osi Y

            // Rysowanie poziomych stopni
            for (int i = 0; i < poziomeStopnie; i++)
            {
                // Rysowanie prostokątnego stopnia poziomo
                await context.BeginPathAsync();
                await context.RectAsync(currentX, currentY, stepWidth, stepHeight); // W poziomie: Szerokość Biegu Schodów jest wzdłuż osi X
                await context.StrokeAsync();

                  // Console.WriteLine($"Poziomy stopień {i + pionoweStopnie}: X = {currentX}, Y = {currentY}, Szerokość = {stepWidth}, Wysokość = {stepHeight}");
                  // Przesuwamy się w prawo po osi X
                currentX += stepWidth;
            }

            //*******************************************************************************************************************************************************

            // Określamy kąty w stopniach
            double startAngleDegrees = 270;  // Kąt startu łuku
            double endAngleDegrees = startAngleDegrees + KatZabiegu;   // Kąt zakończenia łuku

            // Obliczamy kąty w radianach
            double startAngle = startAngleDegrees * (Math.PI / 180);  // Kąt startu w radianach
            double endAngle = endAngleDegrees * (Math.PI / 180);      // Kąt końcowy w radianach

            // Obliczamy środek okręgu
            double centerX = X + (DlugoscOtworu - Radius) * Skala;
            double centerY = Y + Radius * Skala;

            // Obliczamy długość boku kwadratu wpisanego w okrąg
            double squareSide = Radius * 2 * Skala;  // Bok kwadratu = 2 * średnica okręgu

            // Obliczamy współrzędne kwadratu (kwadrat styczny do okręgu)
            double squareX1 = centerX - squareSide / 2;  // Lewa górna krawędź kwadratu
            double squareY1 = centerY - squareSide / 2;  // Lewa górna krawędź kwadratu
            double squareX2 = centerX + squareSide / 2;  // Prawa dolna krawędź kwadratu
            double squareY2 = centerY + squareSide / 2;  // Prawa dolna krawędź kwadratu

            //// Rysowanie kwadratu
            //await context.BeginPathAsync();
            //await context.MoveToAsync(squareX1, squareY1);  // Lewy górny róg
            //await context.LineToAsync(squareX2, squareY1);  // Prawy górny róg
            //await context.LineToAsync(squareX2, squareY2);  // Prawy dolny róg
            //await context.LineToAsync(squareX1, squareY2);  // Lewy dolny róg
            //await context.LineToAsync(squareX1, squareY1);  // Powrót do lewego górnego rogu
            //await context.StrokeAsync();

            //// Rysowanie okręgu wpisanego w kwadrat
            //await context.BeginPathAsync();
            //await context.ArcAsync(centerX, centerY, Radius * Skala, 0, 2 * Math.PI);  // Rysujemy okrąg wpisany w kwadrat
            //await context.StrokeAsync();

            // Rysowanie fragmentu okręgu (łuku)
            await context.BeginPathAsync();
            await context.ArcAsync(centerX, centerY, Radius * Skala, startAngle, endAngle);  // Rysujemy łuk
            await context.StrokeAsync();

            // Obliczamy środek okręgu
            double centerXOsi = X + (DlugoscOtworu - SzerokoscBieguSchodow) * Skala;
            double centerYOsi = Y + SzerokoscBieguSchodow * Skala;
            // Rysowanie fragmentu okręgu (łuku) - oś schodów do poprawy
            await context.BeginPathAsync();
            await context.ArcAsync(centerXOsi, centerYOsi, (SzerokoscBieguSchodow / 2) * Skala, startAngle, endAngle);  // Rysujemy łuk
            await context.StrokeAsync();

            // Rysowanie linii wewnątrz łuku, ale kończących się na krawędziach kwadratu
            double angleRange = endAngle - startAngle;  // Zakres kąta, który musimy pokryć liniami
            double angleStep = angleRange / (IloscSchodowZabiegowych - 1);  // Krok kąta dla linii

            for (int i = 0; i < IloscSchodowZabiegowych; i++)
            {
                // Obliczamy kąt dla danej linii w radianach
                double angle = startAngle + i * angleStep;  // Kąt między startowym a końcowym

                // Obliczamy nachylenie linii (dy/dx)
                double slope = Math.Tan(angle);

                // Współrzędne końcowe linii (domyślnie w obrębie kwadratu)
                double endX, endY;

                // Sprawdzamy, z którą krawędzią kwadratu linia przecina się najpierw
                if (Math.Abs(Math.Cos(angle)) > Math.Abs(Math.Sin(angle)))
                {
                    // Linia przecięła się najpierw z lewą lub prawą krawędzią kwadratu
                    if (Math.Cos(angle) > 0)
                    {
                        // Prawa krawędź kwadratu
                        endX = squareX2;
                        endY = centerY + slope * (squareX2 - centerX);
                    }
                    else
                    {
                        // Lewa krawędź kwadratu
                        endX = squareX1;
                        endY = centerY + slope * (squareX1 - centerX);
                    }
                }
                else
                {
                    // Linia przecięła się najpierw z górną lub dolną krawędzią kwadratu
                    if (Math.Sin(angle) > 0)
                    {
                        // Dolna krawędź kwadratu
                        endY = squareY2;
                        endX = centerX + (squareY2 - centerY) / slope;
                    }
                    else
                    {
                        // Górna krawędź kwadratu
                        endY = squareY1;
                        endX = centerX + (squareY1 - centerY) / slope;
                    }
                }

                // Rysowanie linii
                await context.BeginPathAsync();
                await context.MoveToAsync(centerX, centerY);  // Początek linii w środku okręgu
                await context.LineToAsync(endX, endY);        // Koniec linii na krawędzi kwadratu
                await context.StrokeAsync();
            }


            //*******************************************************************************************************************************************************
            // Po trapezach resetujemy pozycje X i Y dla pionowych stopni (po bocznej krawędzi)
            currentX = X + DlugoscOtworu * Skala - stepHeight; // Przesunięcie na dolną krawędź prostokąta
            currentY = Y + stepHeight; // Ustawiamy Y poniżej trapezów


            // Rysowanie pionowych stopni
            for (int i = 0; i < pionoweStopnie; i++)
            {
                // Rysujemy pionowe stopnie wzdłuż lewej krawędzi
                currentY = Y + delatYKrok + (Radius * Skala);

                // Rysowanie prostokątnego stopnia pionowo
                await context.BeginPathAsync();
                await context.RectAsync(currentX, currentY, stepHeight, stepWidth); // W pionie: Szerokość Biegu Schodów jest wzdłuż osi Y
                await context.StrokeAsync();

              //  Console.WriteLine($"Pionowy stopień {i}: X = {currentX}, Y = {currentY}, Szerokość = {stepHeight}, Wysokość = {stepWidth}");

                // Przesuwamy się w dół
                delatYKrok += stepWidth;
            }

            await context.SetFontAsync("16px Arial");
            await context.SetFillStyleAsync("red");

            // Draw text
            await context.FillTextAsync($"Poziome: {poziomeStopnie}({poziomeStopnie * GlebokoscStopnia} = {poziomeStopnie * GlebokoscStopnia + Radius})" +
                $" Pionowe {pionoweStopnie}({pionoweStopnie * GlebokoscStopnia} = {pionoweStopnie * GlebokoscStopnia + Radius})" +
                $" Promień:{Radius} Wysokość: {WysokoscPodniesieniaStopnia * (poziomeStopnie + pionoweStopnie + IloscSchodowZabiegowych - 1)}" +
                $" - Suma {poziomeStopnie + pionoweStopnie + IloscSchodowZabiegowych - 1}", X + 10, Y + 20);
    
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
            if (XLinePoint == null) return;

            XLinePoint.Add(new LinePoint(x, y, x + width, y));
            XLinePoint.Add(new LinePoint(x + width, y, x + width, y + height));
            XLinePoint.Add(new LinePoint(x + width, y + height, x, y + height));
            XLinePoint.Add(new LinePoint(x, y + height, x, y));
        }

        private void AddLinePoints(double x1, double y1, double x2, double y2)
        {
           if (XLinePoint == null) return;

           XLinePoint.Add(new LinePoint(x1, y1, x2, y2));
        }
        public override Task<List<LinePoint>> ReturnLinePoints()
        {
            return Task.FromResult(XLinePoint ?? new List<LinePoint>());
        }
    }

}
