using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Shading;
using Blazor.Extensions.Canvas.Canvas2D;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Exceptions;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Layout;
using iText.Layout.Properties;
using Microsoft.JSInterop;
using netDxf;
using netDxf.Collections;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Tables;
using netDxf.Units;
using Org.BouncyCastle.Asn1.Pkcs;
using iText.Kernel.Geom;

namespace GEORGE.Client.Pages.Schody
{
    public abstract partial class Ck_l
    {
        public abstract Task DrawAsync(Canvas2DContext context);
        public abstract Task<List<Point>> ReturnPoints();
        public abstract Task<List<LinePoint>> ReturnLinePoints();

    }

    public class CSchodyKL : Shape
    {
        private readonly IJSRuntime _jsRuntime;

        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public double X { get; set; }
        public double Y { get; set; }
        // Input properties
        private double SrednicaRuru { get; set; }
        private double WysokoscCalkowita { get; set; }
        private double LiczbaPodniesienStopni { get; set; }
        private double SrednicaSchodow { get; set; }
        private double KatRozwarciaSchodow { get; set; }
        private double KatRozwarciaPodestuWyjsciowego { get; set; }

        // Output properties (results)
        private double DlugoscLiniiBiegu { get; set; }
        private double SzerokoscSchodow { get; set; }
        private double WysokoscPodniesieniaStopnia { get; set; }
        private double GlebokoscStopnia { get; set; }
        private double PrzecietnaDlugoscKroku { get; set; }
        private double PrzestrzenSwobodnaNadGlowa { get; set; }
        private double GlebokoscZewnatrz { get; set; }

        private double Skala { get; set; }
        private string Opis { get; set; }
        private char Lewe { get; set; }
        private string NazwaProgramuCNC { get; set; }

        public CSchodyKL(IJSRuntime jsRuntime, double x, double y, double skala, double srednicaRuru, double wysokoscCalkowita, double liczbaPodniesienStopni, double srednicaSchodow,
            double katRozwarciaSchodow, double dlugoscLiniiBiegu, double szerokoscSchodow, double wysokoscPodniesieniaStopnia, double glebokoscStopnia, double przecietnaDlugoscKroku,
            double przestrzenSwobodnaNadGlowa, double glebokoscZewnatrz, double katRozwarciaPodestuWyjsciowego, string opis, char lewe, string nazwaProgramuCNC)
        {
            X = x;
            Y = y;
            Skala = skala;
            SrednicaRuru = srednicaRuru;
            WysokoscCalkowita = wysokoscCalkowita;
            LiczbaPodniesienStopni = liczbaPodniesienStopni - 1;//Ostatni krok stopień -> płyta podłogi
            SrednicaSchodow = srednicaSchodow;
            KatRozwarciaSchodow = katRozwarciaSchodow;
            DlugoscLiniiBiegu = dlugoscLiniiBiegu;
            SzerokoscSchodow = szerokoscSchodow;
            WysokoscPodniesieniaStopnia = wysokoscPodniesieniaStopnia;
            GlebokoscStopnia = glebokoscStopnia;
            PrzecietnaDlugoscKroku = przecietnaDlugoscKroku;
            PrzestrzenSwobodnaNadGlowa = przestrzenSwobodnaNadGlowa;
            GlebokoscZewnatrz = glebokoscZewnatrz;
            KatRozwarciaPodestuWyjsciowego = katRozwarciaPodestuWyjsciowego;
            Opis = opis;
            Lewe = lewe;
            NazwaProgramuCNC = nazwaProgramuCNC.Replace("/", "_").Replace("\\", "_").Replace(".", "_").Replace(",", "_");

            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));

        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            Xpoints = new List<Point>();
            XLinePoint = new List<LinePoint>();

            double currentX = X; // Początkowa pozycja X
            double currentY = Y; // Początkowa pozycja Y

            // Rysujemy obrys schodów (widok od góry)
            await DrawShapeObrys(context, X, Y);

            await DrawCircleInSquare(context, X, Y);

            // Wyświetlenie informacji
            await DrawTextAsync(context, X + 10, Y + 45, $"Informacja: {Opis}");

        }

        private async Task DrawShapeObrys(Canvas2DContext context, double offsetX, double offsetY)
        {
            ClearPathAndAddFinalLineAsync();

            await context.BeginPathAsync();

            // Ustaw kolor linii na czerwony
            await context.SetStrokeStyleAsync("red");

            // Ustaw grubość linii (na przykład na 1 piksele)
            await context.SetLineWidthAsync(1);

            // Rysowanie zewnętrznego prostokąta
            await context.RectAsync(offsetX, offsetY, SrednicaSchodow * Skala, SrednicaSchodow * Skala); // #OBRYS SCHODOW
            // Dodanie punktów prostokąta do listy
            AddRectanglePoints(offsetX, offsetY, SrednicaSchodow * Skala, SrednicaSchodow * Skala);

            // Rysowanie konturu
            await context.StrokeAsync();

            // Ustaw kolor linii na czarny
            await context.SetStrokeStyleAsync("black");

            // Ustaw grubość linii (na przykład na 3 piksele)
            await context.SetLineWidthAsync(1);

        }
        private async Task DrawRadialLines(Canvas2DContext context, double centerX, double centerY, double innerDiameter, double outerDiameter,
                                   int numberOfSteps, double finalStepAngle)
        {
            double innerRadius = innerDiameter;// / 2.0;
            double outerRadius = outerDiameter;// / 2.0;

            // Obliczanie kąta między liniami (w radianach)
            double totalAngle = 2 * Math.PI - (finalStepAngle * Math.PI / 180); // Kąt całkowity do podziału
            double angleStep = totalAngle / (numberOfSteps - 1); // Odstęp między liniami

            if (Lewe == 'l')
            {
                angleStep = -angleStep;
            }

            // Rysowanie linii promienistych
            await context.BeginPathAsync();
            await context.SetStrokeStyleAsync("black"); // Kolor linii
            await context.SetLineWidthAsync(1);

            for (int i = 0; i < numberOfSteps; i++)
            {
                double currentAngle = i * angleStep;

                // Obliczanie punktów końcowych linii
                double innerX = centerX + innerRadius * Math.Cos(currentAngle);
                double innerY = centerY + innerRadius * Math.Sin(currentAngle);
                double outerX = centerX + outerRadius * Math.Cos(currentAngle);
                double outerY = centerY + outerRadius * Math.Sin(currentAngle);

                // Rysowanie linii
                await context.MoveToAsync(innerX, innerY);
                await context.LineToAsync(outerX, outerY);
            }

            // Zakończenie rysowania
            await context.StrokeAsync();

            await DrawArcBetweenLines(context, centerX, centerY, innerRadius * 2, outerRadius * 2, numberOfSteps, finalStepAngle);

        }

        private async Task DrawArcBetweenLines(Canvas2DContext context, double centerX, double centerY, double innerDiameter, double outerDiameter,
                                               int numberOfSteps, double finalStepAngle)
        {
            if (context == null)
            {
                Console.WriteLine("Kontekst rysowania jest null.");
                return;
            }

            double arcRadius = (outerDiameter - innerDiameter) / 4.0; // Promień łuku
            if (arcRadius <= 0)
            {
                Console.WriteLine("Promień łuku jest nieprawidłowy.");
                return;
            }

            double totalAngle = 2 * Math.PI - (finalStepAngle * Math.PI / 180); // Całkowity zakres kątów
            double angleStep = totalAngle / (numberOfSteps - 1); // Odstęp kątowy między liniami

            double startAngle = 0; // Kąt początkowy
            double endAngle = (numberOfSteps - 1) * angleStep; // Kąt końcowy

            if (double.IsNaN(centerX) || double.IsNaN(centerY) || double.IsNaN(startAngle) || double.IsNaN(endAngle))
            {
                Console.WriteLine("Parametry łuku są nieprawidłowe.");
                return;
            }

            try
            {
                await DrawArcWithArrow(context, centerX, centerY, innerDiameter, outerDiameter,numberOfSteps,finalStepAngle); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas rysowania łuku: {ex.Message}");
            }
        }

        private async Task DrawArcWithArrow(Canvas2DContext context, double centerX, double centerY, double innerDiameter, double outerDiameter,
                                    int numberOfSteps, double finalStepAngle)
        {
            if (context == null)
            {
                Console.WriteLine("Kontekst rysowania jest null.");
                return;
            }

            if (Lewe == 'l')
            {
                finalStepAngle = -finalStepAngle;
            }

            double arcRadius = (outerDiameter - innerDiameter) / 4.0; // Promień łuku

            if (arcRadius <= 0)
            {
                Console.WriteLine("Promień łuku jest nieprawidłowy.");
                return;
            }

            double totalAngle = 2 * Math.PI - (finalStepAngle * Math.PI / 180); // Całkowity zakres kątów
            double angleStep = totalAngle / (numberOfSteps - 1); // Odstęp kątowy między liniami
            double startAngle = 0; // Kąt początkowy
            double endAngle = (numberOfSteps - 1) * angleStep; // Kąt końcowy

            if (Lewe == 'l')
            {
                angleStep = -angleStep;
                startAngle = endAngle;
                endAngle = 0;
            }

            if (double.IsNaN(centerX) || double.IsNaN(centerY) || double.IsNaN(startAngle) || double.IsNaN(endAngle))
            {
                Console.WriteLine("Parametry łuku są nieprawidłowe.");
                return;
            }

            try
            {
                // Rysowanie łuku
                await context.BeginPathAsync();
                await context.SetStrokeStyleAsync("brown");
                await context.SetLineWidthAsync(1);
                await context.ArcAsync(centerX, centerY, arcRadius, startAngle, endAngle);
                await context.StrokeAsync();

                // Obliczanie współrzędnych końca łuku
                double arrowEndX = centerX + arcRadius * Math.Cos(endAngle);
                double arrowEndY = centerY + arcRadius * Math.Sin(endAngle);

                double obrucGrot = 29.9;

                if (Lewe == 'l')
                {
                    arrowEndX = centerX + arcRadius * Math.Cos(startAngle);
                    arrowEndY = centerY + arcRadius * Math.Sin(startAngle);

                    obrucGrot = -75;
                }

                // Obliczanie wektora stycznego w punkcie końcowym
                double tangentAngle = endAngle; // Kierunek styczny to kąt końca łuku

                // Rozmiar i kąt rozwarcia strzałki
                double arrowSize = 60 * Skala; // Długość strzałki
                double arrowAngle = Math.PI / 8; // Kąt rozwarcia 

                // Współrzędne ramion grotu
                double leftX = arrowEndX - arrowSize * Math.Cos(tangentAngle - arrowAngle - obrucGrot);
                double leftY = arrowEndY - arrowSize * Math.Sin(tangentAngle - arrowAngle - obrucGrot);

                double rightX = arrowEndX - arrowSize * Math.Cos(tangentAngle + arrowAngle - obrucGrot);
                double rightY = arrowEndY - arrowSize * Math.Sin(tangentAngle + arrowAngle - obrucGrot);

                // Rysowanie strzałki
                await context.BeginPathAsync();
                await context.MoveToAsync(arrowEndX, arrowEndY);  // Wierzchołek strzałki
                await context.LineToAsync(leftX, leftY);          // Lewe ramię
                await context.LineToAsync(rightX, rightY);        // Prawe ramię
                await context.ClosePathAsync();
                await context.SetFillStyleAsync("brown");          // Wypełnienie strzałki
                await context.FillAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas rysowania łuku ze strzałką: {ex.Message}");
            }
        }

        private async Task DrawCircleInSquare(Canvas2DContext context, double offsetX, double offsetY)
        {
            ClearPathAndAddFinalLineAsync();

            // Obliczenie promienia okręgu
            double radius = SrednicaSchodow * Skala / 2;

            double radiusRura = SrednicaRuru * Skala / 2;

            // Środek okręgu
            double centerX = offsetX + radius;
            double centerY = offsetY + radius;

            // Rozpoczęcie ścieżki
            await context.BeginPathAsync();

            // Ustaw kolor linii na niebieski
            await context.SetStrokeStyleAsync("blue");

            // Ustaw grubość linii (np. 3 piksele)
            await context.SetLineWidthAsync(3);

            // Rysowanie okręgu
            await context.ArcAsync(centerX, centerY, radius, 0, 2 * Math.PI);
            // Rysowanie konturu okręgu
            await context.StrokeAsync();

            // Rysowanie rury w środku
            await DrawFilledCircle(context, centerX, centerY, radiusRura, "black"); // Czarny okrąg o środku (100, 100) i promieniu 50

            // Rysowanie konturu okręgu
            await context.StrokeAsync();

            // Dodanie punktów dla okręgu do listy, jeśli potrzebne (opcjonalnie)
            AddCirclePoints(100, 100, 50, "Continuous", "CNC_File", "Macro1", "OBJ1", new string[] { "Z1" }, 0.1, true, 1, "Program1", "Element1");

            // Ustaw kolor linii z powrotem na czarny
            await context.SetStrokeStyleAsync("black");
            await context.SetLineWidthAsync(1);


            await DrawRadialLines(context, centerX, centerY, radiusRura, radius, (int)LiczbaPodniesienStopni, KatRozwarciaPodestuWyjsciowego);
        }

        private async Task DrawFilledCircle(Canvas2DContext context, double centerX, double centerY, double radius, string fillColor = "black")
        {
            // Rozpoczynanie nowej ścieżki
            await context.BeginPathAsync();

            // Rysowanie okręgu
            await context.ArcAsync(centerX, centerY, radius, 0, 2 * Math.PI);

            // Ustawienie koloru wypełnienia
            await context.SetFillStyleAsync(fillColor);

            // Wypełnienie okręgu
            await context.FillAsync();
        }


        // Funkcja dodająca punkty okręgu (opcjonalna, jeśli potrzebujesz ich w dalszym przetwarzaniu)
        private void AddCirclePoints(
            double centerX,
            double centerY,
            double radius,
            string typeLine = "",
            string fileNCName = "",
            string nameMacro = "",
            string idOBJ = "",
            string[]? zRobocze = null,
            double idRuchNarzWObj = 0,
            bool addGcode = false,
            int iloscSztuk = 0,
            string nazwaProgramu = "",
            string nazwaElementu = "")
        {
            if (XLinePoint == null) return;

            int segmentCount = 360; // Liczba segmentów okręgu
            double angleStep = 2 * Math.PI / segmentCount;

            double previousX = centerX + radius;
            double previousY = centerY;

            for (int i = 1; i <= segmentCount; i++)
            {
                double angle = i * angleStep;

                double currentX = centerX + radius * Math.Cos(angle);
                double currentY = centerY + radius * Math.Sin(angle);

                // Dodawanie linii między poprzednim a obecnym punktem
                XLinePoint.Add(new LinePoint(previousX, previousY, currentX, currentY, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramu, nazwaElementu));

                // Aktualizacja poprzedniego punktu
                previousX = currentX;
                previousY = currentY;
            }
        }



        // Zmienna do śledzenia punktu poprzedniego
        double? previousX = null;
        double? previousY = null;

        // Zmienna do śledzenia pierwszego punktu ścieżki
        double? firstX = null;
        double? firstY = null;

        private void ClearPathAndAddFinalLineAsync()
        {
            previousX = null;
            previousY = null;
            // Zmienna do śledzenia pierwszego punktu ścieżki
            firstX = null;
            firstY = null;
        }
        public override Task<List<Point>> ReturnPoints()
        {
            return Task.FromResult(Xpoints ?? new List<Point>());
        }

        public override Task<List<LinePoint>> ReturnLinePoints()
        {
            return Task.FromResult(XLinePoint ?? new List<LinePoint>());
        }

        private async Task DrawTextAsync(Canvas2DContext context, double x, double y, string text)
        {
            // Set text style
            await context.SetFontAsync("16px Arial");
            await context.SetFillStyleAsync("black");

            // Draw text
            await context.FillTextAsync(text, x, y);
        }

        private void AddRectanglePoints(double x, double y, double width, double height, string typeLine = "", string fileNCName = "", string nameMacro = "", string idOBJ = "", string[]? zRobocze = null, double idRuchNarzWObj = 0,
        bool addGcode = false, int iloscSztuk = 0, string nazwaProgramy = "", string nazwaElementu = "")
        {
            if (XLinePoint == null) return;

            XLinePoint.Add(new LinePoint(x, y, x + width, y, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));
            XLinePoint.Add(new LinePoint(x + width, y, x + width, y + height, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));
            XLinePoint.Add(new LinePoint(x + width, y + height, x, y + height, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));
            XLinePoint.Add(new LinePoint(x, y + height, x, y, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy));
        }
    }
}