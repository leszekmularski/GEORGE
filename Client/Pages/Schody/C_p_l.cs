using Blazor.Extensions.Canvas.Canvas2D;
using netDxf;
using netDxf.Entities;
using Microsoft.JSInterop;
using netDxf.Tables;

namespace GEORGE.Client.Pages.Schody
{

    public abstract partial class C_p_l
    {
        public abstract Task DrawAsync(Canvas2DContext context);
        public abstract Task<List<Point>> ReturnPoints();
        public abstract Task<List<LinePoint>> ReturnLinePoints();

    }

    //---------------------------------------------------------------- PIÓRO -------------------------------------------------------------------------------------------
    public class CSchodyPL : Shape
    {
        private readonly IJSRuntime _jsRuntime;

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
        private char Lewe { get; set; }

        public CSchodyPL(IJSRuntime jsRuntime, double x, double y, double skala, double dlugoscOtworu, double szerokoscOtworu, double dlugoscNaWejsciu, double wysokoscDoStropu, double wysokoscCalkowita, double liczbaPodniesienStopni,
            double szerokoscOstatniegoStopnia, double szerokoscBieguSchodow, double dlugoscLiniiBiegu, double katNachylenia, double szerokoscSchodow, double wysokoscPodniesieniaStopnia,
            double glebokoscStopnia, double przecietnaDlugoscKroku, double przestrzenSwobodnaNadGlowa, string opis, char lewe)
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
            Lewe= lewe;

            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            Xpoints = new List<Point>();
            XLinePoint = new List<LinePoint>();

            double currentX = 0; // Początkowa pozycja X
            double currentY = Y; // Początkowa pozycja Y

            double stepWidth = GlebokoscStopnia * Skala;  // Szerokość stopnia (długość biegu schodów)
            double stepHeight = SzerokoscBieguSchodow * Skala;  // Wysokość stopnia (szerokość biegu schodów)

            // Rysujemy obrys schodów (widok od góry)
            await DrawShapeObrys(context, X, Y);

            // Wyświetlenie informacji
            await DrawTextAsync(context, X + 10, Y + 45, $"Informacja: {Opis}");

            if (Lewe == 'l')
            {
                currentX = X + (DlugoscOtworu - GlebokoscStopnia) * Skala; // Początkowa pozycja X

                for (int i = 0; i < LiczbaPodniesienStopni; i++)
                {
                    await context.BeginPathAsync();

                    if (i == 0)
                    {
                        await context.RectAsync(currentX - SzerokoscOstatniegoStopnia * Skala, currentY, stepWidth + SzerokoscOstatniegoStopnia * Skala, stepHeight);
                        await context.StrokeAsync();
                        currentX -= stepWidth + SzerokoscOstatniegoStopnia * Skala;
                    }
                    else
                    {
                        await context.RectAsync(currentX, currentY, stepWidth, stepHeight);
                        await context.StrokeAsync();
                        currentX -= stepWidth;
                    }

                }

                // Rysowanie grotu strzałki
                await context.BeginPathAsync();
                await context.LineToAsync(0, (SzerokoscSchodow * Skala / 2));      // Koniec linii na dużym kwadracie
                await context.LineToAsync(20, (SzerokoscSchodow * Skala / 2) + 5);      // Koniec linii na dużym kwadracie
                await context.LineToAsync(20, (SzerokoscSchodow * Skala / 2) - 5);      // Koniec linii na dużym kwadracie
                await context.ClosePathAsync();
                await context.StrokeAsync();

                //Rysowanie kierunku biegu
                await context.BeginPathAsync();

                // 1. Pierwsza linia (linia pozioma)
                await context.MoveToAsync(DlugoscOtworu * Skala, SzerokoscSchodow * Skala / 2);  // Początek linii
                await context.LineToAsync(X, SzerokoscSchodow * Skala / 2);  // Koniec pierwszej linii
                await context.StrokeAsync();  // Zakończ rysowa
            }
            else if (Lewe == 'p')
            {
                currentX = X; // Początkowa pozycja X

                for (int i = 0; i < LiczbaPodniesienStopni; i++)
                {
                    await context.BeginPathAsync();

                    if (i == LiczbaPodniesienStopni)
                    {
                        await context.RectAsync(currentX - SzerokoscOstatniegoStopnia * Skala, currentY, stepWidth + SzerokoscOstatniegoStopnia * Skala, stepHeight);
                        await context.StrokeAsync();
                        currentX += stepWidth + SzerokoscOstatniegoStopnia * Skala;
                    }
                    else
                    {
                        await context.RectAsync(currentX, currentY, stepWidth, stepHeight);
                        await context.StrokeAsync();
                        currentX += stepWidth;
                    }

                }

                // Rysowanie grotu strzałki
                await context.BeginPathAsync();
                await context.LineToAsync(DlugoscOtworu * Skala, (SzerokoscSchodow * Skala / 2));      // Koniec linii na dużym kwadracie
                await context.LineToAsync(DlugoscOtworu * Skala - 20, (SzerokoscSchodow * Skala / 2) + 5);      // Koniec linii na dużym kwadracie
                await context.LineToAsync(DlugoscOtworu * Skala - 20, (SzerokoscSchodow * Skala / 2) - 5);      // Koniec linii na dużym kwadracie
                await context.ClosePathAsync();
                await context.StrokeAsync();

                //Rysowanie kierunku biegu
                await context.BeginPathAsync();

                // 1. Pierwsza linia (linia pozioma)
                await context.MoveToAsync(DlugoscOtworu * Skala, SzerokoscSchodow * Skala / 2);  // Początek linii
                await context.LineToAsync(X, SzerokoscSchodow * Skala / 2);  // Koniec pierwszej linii
                await context.StrokeAsync();  // Zakończ rysowa
            }

            await context.SetFontAsync("16px Arial");
            await context.SetFillStyleAsync("red");

            // Wyświetlanie dodatkowych informacji
            // Draw text
            await context.FillTextAsync($"Poziome: {LiczbaPodniesienStopni}({LiczbaPodniesienStopni * GlebokoscStopnia} = {LiczbaPodniesienStopni * GlebokoscStopnia})" +
               $" Wysokość:{WysokoscPodniesieniaStopnia} x {(LiczbaPodniesienStopni + 1)} = {WysokoscPodniesieniaStopnia * (LiczbaPodniesienStopni + 1)}" +
               $" - Suma stopni {LiczbaPodniesienStopni}", X + 10, Y + 20);
        }

        private async Task DrawShapeObrys(Canvas2DContext context, double offsetX, double offsetY)
        {
            await context.BeginPathAsync();

            // Ustaw kolor linii na czerwony
            await context.SetStrokeStyleAsync("red");

            // Ustaw grubość linii (na przykład na 3 piksele)
            await context.SetLineWidthAsync(3);

            // Rysowanie zewnętrznego prostokąta
            await context.RectAsync(offsetX, offsetY, DlugoscOtworu * Skala, SzerokoscOtworu * Skala); // #OBRYS SCHODOW
            await context.RectAsync(offsetX + 1, offsetY + 1, DlugoscOtworu * Skala - 1, SzerokoscOtworu * Skala - 1); // #OBRYS SCHODOW

            // Dodanie punktów prostokąta do listy
            AddRectanglePoints(offsetX, offsetY, DlugoscOtworu * Skala, SzerokoscOtworu * Skala);

            // Rysowanie konturu
            await context.StrokeAsync();

            // Ustaw kolor linii na czerwony
            await context.SetStrokeStyleAsync("black");

            // Ustaw grubość linii (na przykład na 3 piksele)
            await context.SetLineWidthAsync(1);
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

        // Funkcja zapisująca rysunek do pliku DXF
        public async Task SaveToDxfAsync()
        {
            DxfDocument dxf = new DxfDocument();

            // Ustawienia podstawowe
            double currentX = X;
            double currentY = Y;
            double stepWidth = GlebokoscStopnia * Skala;
            double stepHeight = SzerokoscBieguSchodow * Skala;
            double delatYKrok = 0;

            // Tworzymy instancję klasy Wzory
            Wzory wzory = new Wzory();

            // Obliczamy promień okręgu
            double Radius = wzory.WartoscZaczepPromnienia(SzerokoscBieguSchodow, 90 / 2);

            // DlugoscOtworu / GlebokoscStopnia
            double szukanyStopien = (int)Math.Ceiling((DlugoscOtworu - Radius) / GlebokoscStopnia);
            Radius = DlugoscOtworu - szukanyStopien * GlebokoscStopnia - SzerokoscOstatniegoStopnia;

            // Jeżeli promień jest mniejszy od szerokości biegu schodów, dostosowujemy go
            if (Radius < SzerokoscBieguSchodow)
            {
                Radius = SzerokoscBieguSchodow;
                szukanyStopien = (int)Math.Floor((DlugoscOtworu - Radius) / GlebokoscStopnia);
                Radius = DlugoscOtworu - szukanyStopien * GlebokoscStopnia - SzerokoscOstatniegoStopnia;
            }

            // Obliczamy liczbę pionowych stopni
            double pionoweStopnie = (int)Math.Ceiling((SzerokoscOtworu - Radius) / GlebokoscStopnia);
            if (pionoweStopnie > LiczbaPodniesienStopni)
            {
                pionoweStopnie = LiczbaPodniesienStopni;
            }

            double poziomeStopnie = szukanyStopien;

            // ** Dodanie obrysu schodów **
            AddObrysSchodow(dxf, X, Y, DlugoscOtworu * Skala, SzerokoscOtworu * Skala);

            // Dodajemy poziome stopnie jako prostokąty do pliku DXF
            for (int i = 0; i < poziomeStopnie; i++)
            {
                //AddRectangleToDxf(dxf, currentX, currentY, stepWidth, stepHeight); // Dodajemy poziomy prostokąt (stopień)
                //currentX += stepWidth;
                if (i == 0)
                {

                    AddRectangleToDxf(dxf, currentX, currentY, stepWidth + SzerokoscOstatniegoStopnia * Skala, stepHeight); // Dodajemy poziomy prostokąt (stopień)
                    currentX += stepWidth + SzerokoscOstatniegoStopnia * Skala;
                }
                else
                {
                    AddRectangleToDxf(dxf, currentX, currentY, stepWidth, stepHeight); // Dodajemy poziomy prostokąt (stopień)
                    currentX += stepWidth;
                }
            }

            // Rysowanie linii wewnątrz łuku (konwersja na linie w DXF)
            double startAngleDegrees = 270;
            double endAngleDegrees = startAngleDegrees + 90;
            double startAngle = startAngleDegrees * (Math.PI / 180);
            double endAngle = endAngleDegrees * (Math.PI / 180);
            double centerX = X + (DlugoscOtworu - Radius) * Skala;
            double centerY = Y + Radius * Skala;

            double smallSquareSide = (Radius - SzerokoscBieguSchodow) * Skala * 2;
            double largeSquareSide = Radius * 2 * Skala;

            double angleRange = endAngle - startAngle;
            double angleStep = angleRange / (1 - 1);

            // Dodawanie linii zabiegowych
            for (int i = 0; i < 1; i++)
            {
                double angle = startAngle + i * angleStep;

                // Początek linii na krawędzi małego kwadratu
                double startX = centerX + (smallSquareSide / 2) * Math.Cos(angle);
                double startY = centerY + (smallSquareSide / 2) * Math.Sin(angle);

                // Koniec linii na krawędzi dużego kwadratu
                double endX, endY;
                double slope = Math.Tan(angle);

                if (Math.Abs(Math.Cos(angle)) > Math.Abs(Math.Sin(angle)))
                {
                    if (Math.Cos(angle) > 0)
                    {
                        endX = centerX + largeSquareSide / 2;
                        endY = centerY + slope * (largeSquareSide / 2);
                    }
                    else
                    {
                        endX = centerX - largeSquareSide / 2;
                        endY = centerY + slope * (-largeSquareSide / 2);
                    }
                }
                else
                {
                    if (Math.Sin(angle) > 0)
                    {
                        endY = centerY + largeSquareSide / 2;
                        endX = centerX + (largeSquareSide / 2) / slope;
                    }
                    else
                    {
                        endY = centerY - largeSquareSide / 2;
                        endX = centerX + (-largeSquareSide / 2) / slope;
                    }
                }

                // Dodawanie linii zabiegowej do pliku DXF
                Line zabiegLine = new Line(new Vector2(startX, startY), new Vector2(endX, endY));
                dxf.Entities.Add(zabiegLine);
            }

            // Dodanie fragmentu okręgu (łuku) do pliku DXF
            Arc arc = new Arc(new Vector2(centerX, centerY), Radius * Skala, startAngleDegrees, endAngleDegrees);
            dxf.Entities.Add(arc);

            // Dodanie wewnętrznego fragmentu okręgu (mniejszy łuk)
            Arc arc2 = new Arc(new Vector2(centerX, centerY), (Radius - SzerokoscBieguSchodow) * Skala, startAngleDegrees, endAngleDegrees);
            dxf.Entities.Add(arc2);

            // Resetowanie pozycji X i Y dla pionowych stopni
            currentX = X + DlugoscOtworu * Skala - stepHeight;
            currentY = Y + stepHeight;

            // Dodawanie pionowych stopni
            for (int i = 0; i < pionoweStopnie; i++)
            {
                currentY = Y + delatYKrok + (Radius * Skala);
                AddRectangleToDxf(dxf, currentX, currentY, stepHeight, stepWidth); // Dodaj pionowy prostokąt
                delatYKrok += stepWidth;
            }

            // Aktualizacja stylu tekstu (np. dla wymiarów)
            UpdateDimensionStyleTextHeight(dxf, "Standard", 35, "arial.ttf");

            // Przekształcanie ujemnych współrzędnych (jeśli potrzebne)
            SetNegativeCoordinates(dxf);

            // Zapis pliku DXF
            using (MemoryStream stream = new MemoryStream())
            {
                dxf.Save(stream);

                // Konwersja do Base64
                string base64String = Convert.ToBase64String(stream.ToArray());

                // Wywołanie JavaScript w celu pobrania pliku
                await _jsRuntime.InvokeVoidAsync("downloadFileDXF", "14_z_p.dxf", base64String);
            }

            Console.WriteLine("Plik DXF został wygenerowany.");
        }

        void UpdateDimensionStyleTextHeight(DxfDocument dxf, string dimensionStyleName, double newTextHeight, string fontFilePath)
        {
            // Sprawdź, czy styl wymiarów istnieje
            if (dxf.DimensionStyles.TryGetValue(dimensionStyleName, out DimensionStyle dimensionStyle))
            {
                // Sprawdź, czy styl tekstu istnieje
                if (dimensionStyle.TextStyle != null)
                {
                    // Zmieniaj wysokość czcionki
                    dimensionStyle.TextStyle.WidthFactor = 1;
                    dimensionStyle.TextStyle.Height = newTextHeight;
                }
                else
                {
                    // Jeśli brak stylu tekstu, utwórz nowy styl tekstu
                    var newTextStyle = new TextStyle(fontFilePath)
                    {
                        WidthFactor = 1,
                        Height = newTextHeight
                    };
                    dimensionStyle.TextStyle = newTextStyle;
                }
            }
            else
            {
                Console.WriteLine("Nie znaleziono stylu wymiarów o nazwie: " + dimensionStyleName);
            }
        }
        void SetNegativeCoordinates(DxfDocument dxf)
        {
            var entities = dxf.Entities;
            if (entities != null)
            {
                foreach (var line in entities.Lines)
                {
                    if (line is Line)
                    {
                        // Przypisujemy nowe współrzędne z konwersją na Vector3
                        line.StartPoint = new Vector3(line.StartPoint.X, -line.StartPoint.Y, line.StartPoint.Z);
                        line.EndPoint = new Vector3(line.EndPoint.X, -line.EndPoint.Y, line.EndPoint.Z);
                    }
                }

                foreach (var arc in entities.Arcs)
                {
                    if (arc is Arc)
                    {
                        // Przypisujemy nowe współrzędne z konwersją na Vector3
                        arc.Center = new Vector3(arc.Center.X, -arc.Center.Y, arc.Center.Z);

                        // Przekształcamy kąty, aby dostosować się do nowego układu współrzędnych
                        // Zakładamy, że kąty są wyrażone w stopniach lub radianach, więc zmiana znaku jest potrzebna
                        arc.StartAngle = 90 + arc.StartAngle;
                        arc.EndAngle = 90 + arc.EndAngle;

                    }
                }

                // Dodaj inne przypadki dla różnych typów obiektów, które mogą zawierać współrzędne
            }
        }


        // Funkcja dodająca prostokąt do pliku DXF
        private void AddRectangleToDxf(DxfDocument dxf, double x, double y, double width, double height)
        {
            // Tworzenie czterech linii, aby reprezentować prostokąt
            Line line1 = new Line(new Vector2(x, y), new Vector2(x + width, y)); // Dolna krawędź
            Line line2 = new Line(new Vector2(x + width, y), new Vector2(x + width, y + height)); // Prawa krawędź
            Line line3 = new Line(new Vector2(x + width, y + height), new Vector2(x, y + height)); // Górna krawędź
            Line line4 = new Line(new Vector2(x, y + height), new Vector2(x, y)); // Lewa krawędź

            // Dodawanie linii do pliku DXF
            dxf.Entities.Add(line1);
            dxf.Entities.Add(line2);
            dxf.Entities.Add(line3);
            dxf.Entities.Add(line4);
        }

        // Funkcja dodająca obrys schodów do pliku DXF
        private void AddObrysSchodow(DxfDocument dxf, double offsetX, double offsetY, double dlugosc, double szerokosc)
        {
            // Dodanie zewnętrznego prostokąta
            Line line1 = new Line(new Vector2(offsetX, offsetY), new Vector2(offsetX + dlugosc, offsetY));
            Line line2 = new Line(new Vector2(offsetX + dlugosc, offsetY), new Vector2(offsetX + dlugosc, offsetY + szerokosc));
            Line line3 = new Line(new Vector2(offsetX + dlugosc, offsetY + szerokosc), new Vector2(offsetX, offsetY + szerokosc));
            Line line4 = new Line(new Vector2(offsetX, offsetY + szerokosc), new Vector2(offsetX, offsetY));

            dxf.Entities.Add(line1);
            dxf.Entities.Add(line2);
            dxf.Entities.Add(line3);
            dxf.Entities.Add(line4);

        }

    }

}
