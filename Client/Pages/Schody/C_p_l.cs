using Blazor.Extensions.Canvas.Canvas2D;
using netDxf;
using netDxf.Entities;
using Microsoft.JSInterop;
using netDxf.Tables;
using netDxf.Units;
using netDxf.Header;

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

            double gruboscStopnia = 40 * Skala; // Zrobić zmienną!!!!!!

            if (Lewe == 'l')
            {

                // Rysujemy obrys schodów (widok od góry)
                await DrawShapeObrys(context, X, Y);

                // Wyświetlenie informacji
                await DrawTextAsync(context, X + 10, Y + 45, $"Informacja: {Opis}");

                currentX = X + (DlugoscOtworu - GlebokoscStopnia) * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth; // Początkowa pozycja X

                for (int i = 0; i < LiczbaPodniesienStopni; i++)
                {
                    await context.BeginPathAsync();

                    if (i == LiczbaPodniesienStopni - 1)
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

                //Rysowanie schodów widok z boku ------------------------------------------------------------------------------------------------------------------------------
                currentX = X + (DlugoscOtworu - GlebokoscStopnia) * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth; // Początkowa pozycja X

                currentY = SzerokoscOtworu * Skala + ((LiczbaPodniesienStopni + 1) * WysokoscPodniesieniaStopnia) * Skala;

                // Zapisz początkowe wartości do obliczenia rozmiaru obwiedni
                double startX = currentX;
                double startY = currentY;

                for (int i = 0; i < LiczbaPodniesienStopni; i++)
                {
                    await context.BeginPathAsync();

                    if (i == LiczbaPodniesienStopni - 1)
                    {
                        await DrawShapeStopinRysBok(context, currentX - SzerokoscOstatniegoStopnia * Skala, currentY, stepWidth + SzerokoscOstatniegoStopnia * Skala, gruboscStopnia);
                        currentX -= stepWidth + SzerokoscOstatniegoStopnia * Skala;
                        currentY -= WysokoscPodniesieniaStopnia * Skala;
                    }
                    else
                    {
                        await DrawShapeStopinRysBok(context, currentX, currentY,stepWidth, gruboscStopnia);
                        currentX -= stepWidth;
                        currentY -= WysokoscPodniesieniaStopnia * Skala;
                    }

                }


                await DrawObrysZKatem(context, startX + stepWidth, startY + WysokoscPodniesieniaStopnia * Skala, currentY + WysokoscPodniesieniaStopnia * Skala, DlugoscLiniiBiegu * Skala, 
                    ((LiczbaPodniesienStopni + 1) * WysokoscPodniesieniaStopnia) * Skala, 90 + KatNachylenia, stepWidth + SzerokoscOstatniegoStopnia * Skala);


            //Rysowanie schodów widok z boku KONIEC --------------------------------------------------------------------------------------------------------------------------

        }
            else if (Lewe == 'p')
            {

                // Rysujemy obrys schodów (widok od góry)
                await DrawShapeObrys(context, X + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, Y);

                // Wyświetlenie informacji
                await DrawTextAsync(context, X + 10, Y + 45, $"Informacja: {Opis}");

                currentX = X; // Początkowa pozycja X

                for (int i = 0; i < LiczbaPodniesienStopni; i++)
                {
                    await context.BeginPathAsync();

                    if (i == LiczbaPodniesienStopni - 1)
                    {
                        await context.RectAsync(currentX, currentY, stepWidth + SzerokoscOstatniegoStopnia * Skala, stepHeight);
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
                await context.LineToAsync(DlugoscOtworu * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, (SzerokoscSchodow * Skala / 2));      // Koniec linii na dużym kwadracie
                await context.LineToAsync(DlugoscOtworu * Skala - 20 + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, (SzerokoscSchodow * Skala / 2) + 5);      // Koniec linii na dużym kwadracie
                await context.LineToAsync(DlugoscOtworu * Skala - 20 + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, (SzerokoscSchodow * Skala / 2) - 5);      // Koniec linii na dużym kwadracie
                await context.ClosePathAsync();
                await context.StrokeAsync();

                //Rysowanie kierunku biegu
                await context.BeginPathAsync();

                // 1. Pierwsza linia (linia pozioma)
                await context.MoveToAsync(DlugoscOtworu * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, SzerokoscSchodow * Skala / 2);  // Początek linii
                await context.LineToAsync(X + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, SzerokoscSchodow * Skala / 2);  // Koniec pierwszej linii
                await context.StrokeAsync();  // Zakończ rysowa
            }

            await context.SetFontAsync("16px Arial");
            await context.SetFillStyleAsync("red");

            // Wyświetlanie dodatkowych informacji
            // Draw text
            await context.FillTextAsync($"Poziome: {LiczbaPodniesienStopni} = ({System.Math.Round(LiczbaPodniesienStopni * GlebokoscStopnia + SzerokoscOstatniegoStopnia, 0)})" +
               $" Wysokość:{WysokoscPodniesieniaStopnia} x {(LiczbaPodniesienStopni + 1)} = {WysokoscPodniesieniaStopnia * (LiczbaPodniesienStopni + 1)}" +
               $" -> Suma stopni {LiczbaPodniesienStopni}", X + 10, Y + 20);
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

        // Funkcja rysująca obrys pod kątem
        private async Task DrawObrysZKatem(Canvas2DContext context, double startX, double startY, double YStartStopienGorny, double szerokosc, double wysokosc,
                                           double katNachylenia, double stepWidth)
        {
            // Konwersja kąta nachylenia na radiany
            double katRadians = katNachylenia * (Math.PI / 180);

            // Obliczenie przesunięć wzdłuż osi X i Y
            double deltaX = szerokosc * Math.Cos(katRadians);
            double deltaY = wysokosc * Math.Sin(katRadians);

            // Punkt początkowy
            double endX = startX + deltaX;
            double endY = startY - deltaY;

            // Ustaw kolor na czerwony i zwiększ grubość linii
            await context.SetStrokeStyleAsync("red");
            await context.SetLineWidthAsync(3);

            // Rysowanie nachylonego prostokąta
            await context.BeginPathAsync();

            // Punkt początkowy (lewy dolny róg)
            await context.MoveToAsync(startX - stepWidth, startY);
            AddLinePoints(startX - stepWidth, startY, startX, startY); // Dodanie linii z lewego do prawego dolnego rogu

            // Linia pozioma (lewy dolny do prawy dolny)
            await context.LineToAsync(startX, startY);
            AddLinePoints(startX, startY, startX, startY - WysokoscPodniesieniaStopnia * Skala); // Dodanie linii pionowej

            // Linia pionowa (prawy dolny do prawy górny)
            await context.LineToAsync(startX, startY - WysokoscPodniesieniaStopnia * Skala);
            AddLinePoints(startX, startY - WysokoscPodniesieniaStopnia * Skala, X + stepWidth, YStartStopienGorny);  // Dodanie linii skośnej

            // Skośna linia (od prawego górnego punktu do prawego górnego stopnia)
            await context.LineToAsync(X + stepWidth, YStartStopienGorny);
            AddLinePoints(X + stepWidth, YStartStopienGorny, X, YStartStopienGorny); // Dodanie poziomej linii górnej

            // Linia pozioma na górze
            await context.LineToAsync(X, YStartStopienGorny);
            AddLinePoints(X, YStartStopienGorny, X, YStartStopienGorny + WysokoscPodniesieniaStopnia * Skala - SzerokoscOstatniegoStopnia * Skala); // Dodanie końcowej linii pionowej

            // Linia końcowa pionowa
            await context.LineToAsync(X, YStartStopienGorny + WysokoscPodniesieniaStopnia * Skala - SzerokoscOstatniegoStopnia * Skala);

            // Zamknięcie ścieżki
            await context.ClosePathAsync();
            await context.StrokeAsync();

            // Ustawienie koloru i grubości linii z powrotem do standardowych wartości
            await context.SetStrokeStyleAsync("black");
            await context.SetLineWidthAsync(1);
        }

        private async Task DrawShapeStopinRysBok(Canvas2DContext context, double offsetX, double offsetY, double szerStopnia, double gruboscStopnia)
        {
            await context.BeginPathAsync();

            // Ustaw kolor linii na czerwony
            await context.SetStrokeStyleAsync("blue");

            // Ustaw grubość linii (na przykład na 3 piksele)
            await context.SetLineWidthAsync(2);

            // Rysowanie zewnętrznego prostokąta
            await context.RectAsync(offsetX, offsetY, szerStopnia, gruboscStopnia); // #OBRYS SCHODOW

            // Dodanie punktów prostokąta do listy
            AddRectanglePoints(offsetX, offsetY, szerStopnia, gruboscStopnia);

            // Rysowanie konturu
            await context.StrokeAsync();

            // Ustaw kolor linii na czarny
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
            double currentX = 0; // Początkowa pozycja X
            double currentY = Y; // Początkowa pozycja Y
            double stepWidth = GlebokoscStopnia * Skala;  // Szerokość stopnia (długość biegu schodów)
            double stepHeight = SzerokoscBieguSchodow * Skala;  // Wysokość stopnia (szerokość biegu schodów)
            double gruboscStopnia = 40 * Skala; // Grubość stopnia

            // Obliczenie początkowych wartości schodów
            double startX = X + (DlugoscOtworu - GlebokoscStopnia) * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth;
            double startY = SzerokoscOtworu * Skala + ((LiczbaPodniesienStopni + 1) * WysokoscPodniesieniaStopnia) * Skala;

            // Obrys schodów widok z góry
            AddObrysSchodow(dxf, X, Y, DlugoscOtworu * Skala, SzerokoscOtworu * Skala);

            // Rysowanie stopni (widok od góry)
            currentX = startX;
            currentY = Y;

            for (int i = 0; i < LiczbaPodniesienStopni; i++)
            {
                if (i == LiczbaPodniesienStopni - 1)
                {
                    // Ostatni stopień ma dodatkową szerokość
                    AddRectangleToDxf(dxf, currentX - SzerokoscOstatniegoStopnia * Skala, currentY, stepWidth + SzerokoscOstatniegoStopnia * Skala, stepHeight);
                    currentX -= stepWidth + SzerokoscOstatniegoStopnia * Skala;
                }
                else
                {
                    AddRectangleToDxf(dxf, currentX, currentY, stepWidth, stepHeight);
                    currentX -= stepWidth;
                }
            }

            // Rysowanie grotu strzałki
           // AddArrowToDxf(dxf, 0, SzerokoscSchodow * Skala / 2);

            // Rysowanie schodów widok z boku
            currentX = startX;
            currentY = startY;

            for (int i = 0; i < LiczbaPodniesienStopni; i++)
            {
                if (i == LiczbaPodniesienStopni - 1)
                {
                    // Ostatni stopień z dodatkową szerokością
                    AddRectangleToDxf(dxf, currentX - SzerokoscOstatniegoStopnia * Skala, currentY, stepWidth + SzerokoscOstatniegoStopnia * Skala, gruboscStopnia);
                    currentX -= stepWidth + SzerokoscOstatniegoStopnia * Skala;
                    currentY -= WysokoscPodniesieniaStopnia * Skala;
                }
                else
                {
                    AddRectangleToDxf(dxf, currentX, currentY, stepWidth, gruboscStopnia);
                    currentX -= stepWidth;
                    currentY -= WysokoscPodniesieniaStopnia * Skala;
                }
            }

            //// Dodanie obwiedni z kątem nachylenia
            //await DrawObrysZKatem(context, startX + stepWidth, startY + WysokoscPodniesieniaStopnia * Skala, currentY + WysokoscPodniesieniaStopnia * Skala,
            //                      DlugoscLiniiBiegu * Skala, ((LiczbaPodniesienStopni + 1) * WysokoscPodniesieniaStopnia) * Skala, 90 + KatNachylenia, stepWidth + SzerokoscOstatniegoStopnia * Skala);


            SetNegativeCoordinates(dxf);

            UpdateDimensionStyleTextHeightAndFitView(dxf, "Standard", 35, "arial.ttf");

            // Zapis pliku DXF do strumienia i pobranie go
            using (MemoryStream stream = new MemoryStream())
            {
                dxf.Save(stream);
                string base64String = Convert.ToBase64String(stream.ToArray());
                await _jsRuntime.InvokeVoidAsync("downloadFileDXF", $"schody_proste_{Lewe}.dxf", base64String);
            }
        }
        void UpdateDimensionStyleTextHeightAndFitView(DxfDocument dxf, string dimensionStyleName, double newTextHeight, string fontFilePath)
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

            // Automatyczne dopasowanie widoku (ustawienie granic rysunku)
            FitDrawingInView(dxf);
        }

        void FitDrawingInView(DxfDocument dxf)
        {
            dxf.DrawingVariables.InsUnits = DrawingUnits.Millimeters;
            dxf.DrawingVariables.LwDisplay = true;
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
