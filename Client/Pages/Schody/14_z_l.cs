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
            double glebokoscStopnia, double przecietnaDlugoscKroku, double przestrzenSwobodnaNadGlowa , string opis, double katZabiegu, 
            double iloscSchodowZabiegowych)
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

            double delatYKrok = 0;  // Przesunięcie po osi Y

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

            double poziomeStopnie = szukanyStopien;

            // Rysowanie poziomych stopni
            for (int i = 0; i < poziomeStopnie; i++)
            {
                await context.BeginPathAsync();
                await context.RectAsync(currentX, currentY, stepWidth, stepHeight);
                await context.StrokeAsync();
                currentX += stepWidth;
            }

            //*******************************************************************************************************************************************************
            // Rysowanie linii wewnątrz łuku, zaczynających się od kwadratu o boku 140

            // Określamy kąty w stopniach
            double startAngleDegrees = 270;  // Kąt startu łuku
            double endAngleDegrees = startAngleDegrees + KatZabiegu;   // Kąt zakończenia łuku

            // Obliczamy kąty w radianach
            double startAngle = startAngleDegrees * (Math.PI / 180);
            double endAngle = endAngleDegrees * (Math.PI / 180);

            // Obliczamy środek okręgu
            double centerX = X + (DlugoscOtworu - Radius) * Skala;
            double centerY = Y + Radius * Skala;

            // Bok małego kwadratu wynosi 140, uwzględniamy skalę
            double smallSquareSide = (Radius - SzerokoscBieguSchodow) * Skala * 2;

            // Bok większego kwadratu stycznego do małego
            double largeSquareSide = Radius * 2 * Skala;

            // Obliczamy współrzędne małego kwadratu (kwadrat wpisany w okrąg)
            double smallSquareX1 = centerX - smallSquareSide / 2;  // Lewa górna krawędź kwadratu
            double smallSquareY1 = centerY - smallSquareSide / 2;  // Lewa górna krawędź kwadratu
            double smallSquareX2 = centerX + smallSquareSide / 2;  // Prawa dolna krawędź kwadratu
            double smallSquareY2 = centerY + smallSquareSide / 2;  // Prawa dolna krawędź kwadratu

            // Obliczamy współrzędne dużego kwadratu (kwadrat styczny do okręgu)
            double largeSquareX1 = centerX - largeSquareSide / 2;
            double largeSquareY1 = centerY - largeSquareSide / 2;
            double largeSquareX2 = centerX + largeSquareSide / 2;
            double largeSquareY2 = centerY + largeSquareSide / 2;

            // Rysowanie linii zaczynających się od krawędzi małego kwadratu i kończących się na krawędzi większego kwadratu
            double angleRange = endAngle - startAngle;
            double angleStep = angleRange / (IloscSchodowZabiegowych - 1);

            for (int i = 0; i < IloscSchodowZabiegowych; i++)
            {
                double angle = startAngle + i * angleStep;

                // Początek linii na krawędzi małego kwadratu
                double startX = centerX + (smallSquareSide / 2) * Math.Cos(angle);
                double startY = centerY + (smallSquareSide / 2) * Math.Sin(angle);

                // Koniec linii na krawędzi dużego kwadratu
                double endX, endY;
                double slope = Math.Tan(angle);

                // Sprawdzamy, z którą krawędzią większego kwadratu linia przecina się najpierw
                if (Math.Abs(Math.Cos(angle)) > Math.Abs(Math.Sin(angle)))
                {
                    if (Math.Cos(angle) > 0)
                    {
                        endX = largeSquareX2;
                        endY = centerY + slope * (largeSquareX2 - centerX);
                    }
                    else
                    {
                        endX = largeSquareX1;
                        endY = centerY + slope * (largeSquareX1 - centerX);
                    }
                }
                else
                {
                    if (Math.Sin(angle) > 0)
                    {
                        endY = largeSquareY2;
                        endX = centerX + (largeSquareY2 - centerY) / slope;
                    }
                    else
                    {
                        endY = largeSquareY1;
                        endX = centerX + (largeSquareY1 - centerY) / slope;
                    }
                }

                // Rysowanie linii
                await context.BeginPathAsync();
                await context.MoveToAsync(startX, startY);  // Początek linii na małym kwadracie
                await context.LineToAsync(endX, endY);      // Koniec linii na dużym kwadracie
                await context.StrokeAsync();
            }

            // Rysowanie fragmentu okręgu (łuku)
            await context.BeginPathAsync();
            await context.ArcAsync(centerX, centerY, Radius * Skala, startAngle, endAngle);  // Rysujemy łuk
            await context.StrokeAsync();

            // Obliczamy środek okręgu
            double centerXOsi = X + (DlugoscOtworu - SzerokoscBieguSchodow) * Skala;
            double centerYOsi = Y + SzerokoscBieguSchodow * Skala;
            // Rysowanie fragmentu okręgu (łuku) - oś schodów
            await context.BeginPathAsync();
            await context.LineToAsync(0, (SzerokoscBieguSchodow / 2) * Skala);
            await context.ArcAsync(centerXOsi, centerYOsi, (SzerokoscBieguSchodow / 2) * Skala, startAngle, endAngle);  // Rysujemy łuk
            await context.LineToAsync(centerXOsi + (SzerokoscBieguSchodow / 2) * Skala, SzerokoscOtworu * Skala);      // Koniec linii na dużym kwadracie
            await context.StrokeAsync();

            // Rysowanie fragmentu okręgu (łuku) minumum
            await context.BeginPathAsync();
            await context.MoveToAsync(centerX, centerY - (Radius - SzerokoscBieguSchodow) * Skala);
            await context.LineToAsync(centerX + (Radius - SzerokoscBieguSchodow) * Skala, centerY - (Radius - SzerokoscBieguSchodow) * Skala);
            await context.LineToAsync(centerX + (Radius - SzerokoscBieguSchodow) * Skala, centerY);
            await context.StrokeAsync();

            await context.BeginPathAsync();
            await context.ArcAsync(centerX, centerY, (Radius - SzerokoscBieguSchodow) * Skala, startAngle, endAngle);  // Rysujemy łuk
            await context.StrokeAsync();

            //*******************************************************************************************************************************************************
            // Po trapezach resetujemy pozycje X i Y dla pionowych stopni (po bocznej krawędzi)
            currentX = X + DlugoscOtworu * Skala - stepHeight;  // Przesunięcie na dolną krawędź prostokąta
            currentY = Y + stepHeight;  // Ustawiamy Y poniżej trapezów

            // Rysowanie pionowych stopni
            for (int i = 0; i < pionoweStopnie; i++)
            {
                currentY = Y + delatYKrok + (Radius * Skala);
                await context.BeginPathAsync();
                await context.RectAsync(currentX, currentY, stepHeight, stepWidth);
                await context.StrokeAsync();
                delatYKrok += stepWidth;
            }

            await context.SetFontAsync("16px Arial");
            await context.SetFillStyleAsync("red");

            // Wyświetlanie dodatkowych informacji
            // Draw text
            await context.FillTextAsync($"Poziome: {poziomeStopnie}({poziomeStopnie * GlebokoscStopnia} = {poziomeStopnie * GlebokoscStopnia + Radius})" +
                $" Pionowe {pionoweStopnie}({pionoweStopnie * GlebokoscStopnia} = {pionoweStopnie * GlebokoscStopnia + Radius})" +
                $" Promień:{Radius} Wysokość: {WysokoscPodniesieniaStopnia * (poziomeStopnie + pionoweStopnie + IloscSchodowZabiegowych - 1)}" +
                $" - Suma {poziomeStopnie + pionoweStopnie + IloscSchodowZabiegowych - 1}", X + 10, Y + 20);
        }

        //public override async Task DrawAsync(Canvas2DContext context)
        //{
        //    Xpoints = new List<Point>();
        //    XLinePoint = new List<LinePoint>();

        //    double currentX = X; // Początkowa pozycja X
        //    double currentY = Y; // Początkowa pozycja Y

        //    double stepWidth = GlebokoscStopnia * Skala;  // Szerokość stopnia (długość biegu schodów)
        //    double stepHeight = SzerokoscBieguSchodow * Skala;  // Wysokość stopnia (szerokość biegu schodów)

        //    // Rysujemy obrys schodów (widok od góry)
        //    await DrawShapeObrys(context, X, Y);

        //    // Wyświetlenie informacji
        //    await DrawTextAsync(context, X + 10, Y + 45, $"Informacja: {Opis}");


        //    // Tworzymy instancję klasy Wzory
        //    Wzory wzory = new Wzory();

        //    // Obliczamy promień okręgu
        //    double Radius = wzory.WartoscZaczepPromnienia(SzerokoscBieguSchodow, KatZabiegu / 2);

        //    // DlugoscOtworu / GlebokoscStopnia
        //    double szukanyStopien = (int)Math.Ceiling((DlugoscOtworu - Radius) / GlebokoscStopnia);

        //    Console.WriteLine($"Radius wylicz: {Radius} / Radius real: {DlugoscOtworu - szukanyStopien * GlebokoscStopnia} GlebokoscStopnia:{GlebokoscStopnia} szukanyStopien: {szukanyStopien}");

        //    Radius = DlugoscOtworu - szukanyStopien * GlebokoscStopnia;

        //    // Obliczenie liczby pionowych stopni (zmieszczących się w SzerokoscOtworu)
        //    double pionoweStopnie = (int)Math.Ceiling((SzerokoscOtworu - Radius) / GlebokoscStopnia);

        //    // Jeżeli liczba pionowych stopni jest większa niż liczba wszystkich stopni, wszystkie będą pionowe
        //    if (pionoweStopnie > LiczbaPodniesienStopni)
        //    {
        //        pionoweStopnie = LiczbaPodniesienStopni;
        //    }
        //    //await DrawTextAsync(context, X + 10, Y + 85, $"pionoweStopnie: {pionoweStopnie} {SzerokoscOtworu} / {GlebokoscStopnia}");

        //    double polawaIloscSchodowZabiegowychPoz = (int)Math.Floor(IloscSchodowZabiegowych / 2);

        //    double polawaIloscSchodowZabiegowychPion = IloscSchodowZabiegowych - polawaIloscSchodowZabiegowychPoz;

        //    //pionoweStopnie = pionoweStopnie - (IloscSchodowZabiegowych - polawaIloscSchodowZabiegowychPion);

        //    await DrawTextAsync(context, X + 10, Y + 105, $"pionoweStopnie: {pionoweStopnie}");

        //    // Liczba poziomych stopni
        //    double poziomeStopnie = szukanyStopien;// LiczbaPodniesienStopni - pionoweStopnie - IloscSchodowZabiegowych;

        //    //double delatXKrok = 0;  // Przesunięcie po osi X ???
        //    double delatYKrok = 0;  // Przesunięcie po osi Y

        //    // Rysowanie poziomych stopni
        //    for (int i = 0; i < poziomeStopnie; i++)
        //    {
        //        // Rysowanie prostokątnego stopnia poziomo
        //        await context.BeginPathAsync();
        //        await context.RectAsync(currentX, currentY, stepWidth, stepHeight); // W poziomie: Szerokość Biegu Schodów jest wzdłuż osi X
        //        await context.StrokeAsync();

        //          // Console.WriteLine($"Poziomy stopień {i + pionoweStopnie}: X = {currentX}, Y = {currentY}, Szerokość = {stepWidth}, Wysokość = {stepHeight}");
        //          // Przesuwamy się w prawo po osi X
        //        currentX += stepWidth;
        //    }

        //    //*******************************************************************************************************************************************************

        //    //// Rysowanie kwadratu
        //    //await context.BeginPathAsync();
        //    //await context.MoveToAsync(squareX1, squareY1);  // Lewy górny róg
        //    //await context.LineToAsync(squareX2, squareY1);  // Prawy górny róg
        //    //await context.LineToAsync(squareX2, squareY2);  // Prawy dolny róg
        //    //await context.LineToAsync(squareX1, squareY2);  // Lewy dolny róg
        //    //await context.LineToAsync(squareX1, squareY1);  // Powrót do lewego górnego rogu
        //    //await context.StrokeAsync();

        //    //// Rysowanie okręgu wpisanego w kwadrat
        //    //await context.BeginPathAsync();
        //    //await context.ArcAsync(centerX, centerY, Radius * Skala, 0, 2 * Math.PI);  // Rysujemy okrąg wpisany w kwadrat
        //    //await context.StrokeAsync();            //*******************************************************************************************************************************************************
        //    // Rysowanie linii wewnątrz łuku, zaczynających się od kwadratu o boku 140

        //    // Określamy kąty w stopniach
        //    double startAngleDegrees = 270;  // Kąt startu łuku
        //    double endAngleDegrees = startAngleDegrees + KatZabiegu;   // Kąt zakończenia łuku

        //    // Obliczamy kąty w radianach
        //    double startAngle = startAngleDegrees * (Math.PI / 180);
        //    double endAngle = endAngleDegrees * (Math.PI / 180);

        //    // Obliczamy środek okręgu
        //    double centerX = X + (DlugoscOtworu - Radius) * Skala;
        //    double centerY = Y + Radius * Skala;

        //    // Bok małego kwadratu wynosi 140, uwzględniamy skalę
        //    double smallSquareSide = (Radius - SzerokoscBieguSchodow) * Skala * 2;

        //    // Bok większego kwadratu stycznego do małego
        //    double largeSquareSide = Radius * 2 * Skala;

        //    // Obliczamy współrzędne małego kwadratu (kwadrat wpisany w okrąg)
        //    double smallSquareX1 = centerX - smallSquareSide / 2;  // Lewa górna krawędź kwadratu
        //    double smallSquareY1 = centerY - smallSquareSide / 2;  // Lewa górna krawędź kwadratu
        //    double smallSquareX2 = centerX + smallSquareSide / 2;  // Prawa dolna krawędź kwadratu
        //    double smallSquareY2 = centerY + smallSquareSide / 2;  // Prawa dolna krawędź kwadratu

        //    // Obliczamy współrzędne dużego kwadratu (kwadrat styczny do okręgu)
        //    double largeSquareX1 = centerX - largeSquareSide / 2;
        //    double largeSquareY1 = centerY - largeSquareSide / 2;
        //    double largeSquareX2 = centerX + largeSquareSide / 2;
        //    double largeSquareY2 = centerY + largeSquareSide / 2;

        //    // Rysowanie linii zaczynających się od krawędzi małego kwadratu i kończących się na krawędzi większego kwadratu
        //    double angleRange = endAngle - startAngle;
        //    double angleStep = angleRange / (IloscSchodowZabiegowych - 1);

        //    for (int i = 0; i < IloscSchodowZabiegowych; i++)
        //    {
        //        double angle = startAngle + i * angleStep;

        //        // Początek linii na krawędzi małego kwadratu
        //        double startX = centerX + (smallSquareSide / 2) * Math.Cos(angle);
        //        double startY = centerY + (smallSquareSide / 2) * Math.Sin(angle);

        //        // Koniec linii na krawędzi dużego kwadratu
        //        double endX, endY;
        //        double slope = Math.Tan(angle);

        //        // Sprawdzamy, z którą krawędzią większego kwadratu linia przecina się najpierw
        //        if (Math.Abs(Math.Cos(angle)) > Math.Abs(Math.Sin(angle)))
        //        {
        //            if (Math.Cos(angle) > 0)
        //            {
        //                endX = largeSquareX2;
        //                endY = centerY + slope * (largeSquareX2 - centerX);
        //            }
        //            else
        //            {
        //                endX = largeSquareX1;
        //                endY = centerY + slope * (largeSquareX1 - centerX);
        //            }
        //        }
        //        else
        //        {
        //            if (Math.Sin(angle) > 0)
        //            {
        //                endY = largeSquareY2;
        //                endX = centerX + (largeSquareY2 - centerY) / slope;
        //            }
        //            else
        //            {
        //                endY = largeSquareY1;
        //                endX = centerX + (largeSquareY1 - centerY) / slope;
        //            }
        //        }

        //        // Rysowanie linii
        //        await context.BeginPathAsync();
        //        await context.MoveToAsync(startX, startY);  // Początek linii na małym kwadracie
        //        await context.LineToAsync(endX, endY);      // Koniec linii na dużym kwadracie
        //        await context.StrokeAsync();
        //    }

        //    //*******************************************************************************************************************************************************
        //    // Po trapezach resetujemy pozycje X i Y dla pionowych stopni (po bocznej krawędzi)
        //    currentX = X + DlugoscOtworu * Skala - stepHeight; // Przesunięcie na dolną krawędź prostokąta
        //    currentY = Y + stepHeight; // Ustawiamy Y poniżej trapezów


        //    // Rysowanie pionowych stopni
        //    for (int i = 0; i < pionoweStopnie; i++)
        //    {
        //        // Rysujemy pionowe stopnie wzdłuż lewej krawędzi
        //        currentY = Y + delatYKrok + (Radius * Skala);

        //        // Rysowanie prostokątnego stopnia pionowo
        //        await context.BeginPathAsync();
        //        await context.RectAsync(currentX, currentY, stepHeight, stepWidth); // W pionie: Szerokość Biegu Schodów jest wzdłuż osi Y
        //        await context.StrokeAsync();

        //      //  Console.WriteLine($"Pionowy stopień {i}: X = {currentX}, Y = {currentY}, Szerokość = {stepHeight}, Wysokość = {stepWidth}");

        //        // Przesuwamy się w dół
        //        delatYKrok += stepWidth;
        //    }

        //    await context.SetFontAsync("16px Arial");
        //    await context.SetFillStyleAsync("red");

        //    // Draw text
        //    await context.FillTextAsync($"Poziome: {poziomeStopnie}({poziomeStopnie * GlebokoscStopnia} = {poziomeStopnie * GlebokoscStopnia + Radius})" +
        //        $" Pionowe {pionoweStopnie}({pionoweStopnie * GlebokoscStopnia} = {pionoweStopnie * GlebokoscStopnia + Radius})" +
        //        $" Promień:{Radius} Wysokość: {WysokoscPodniesieniaStopnia * (poziomeStopnie + pionoweStopnie + IloscSchodowZabiegowych - 1)}" +
        //        $" - Suma {poziomeStopnie + pionoweStopnie + IloscSchodowZabiegowych - 1}", X + 10, Y + 20);

        //}

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
