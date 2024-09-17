using Blazor.Extensions.Canvas.Canvas2D;
using netDxf;
using netDxf.Entities;
using Microsoft.JSInterop;
using netDxf.Tables;

namespace GEORGE.Client.Pages.Schody
{

    public abstract partial class C14_p_l
    {
        public abstract Task DrawAsync(Canvas2DContext context);
        public abstract Task<List<Point>> ReturnPoints();
        public abstract Task<List<LinePoint>> ReturnLinePoints();

    }

    //---------------------------------------------------------------- PIÓRO -------------------------------------------------------------------------------------------
    public class CSchody_Podest_L : Shape
    {
        private readonly IJSRuntime _jsRuntime;

        public List<Point>? Xpoints;
        public List<LinePoint>? XLinePoint;
        public double X { get; set; }
        public double Y { get; set; }
        public double Skala { get; set; }
        private double DlugoscOtworu { get; set; }
        private double SzerokoscOtworu { get; set; }
        private double WysokoscDoStropu { get; set; }
        private double WysokoscCalkowita { get; set; }
        private double LiczbaPodniesienStopni { get; set; }
        private double SzerokoscOstatniegoStopnia { get; set; }
        private double PodestDlugosc { get; set; }
        private double PodestSzerokosc { get; set; }
        private double PodestJakoStopienNr { get; set; }

        // Output properties (results)
        private double DlugoscLiniiBiegu { get; set; }
        private double KatNachylenia { get; set; }
        private double WysokoscPodniesieniaStopnia { get; set; }
        private double GlebokoscStopnia { get; set; }
        private double PrzecietnaDlugoscKroku { get; set; }
        private double PrzestrzenSwobodnaNadGlowa { get; set; }
        private string Opis { get; set; }

        public CSchody_Podest_L(IJSRuntime jsRuntime, double x, double y, double skala, double dlugoscOtworu, double szerokoscOtworu, double wysokoscDoStropu, double wysokoscCalkowita, double liczbaPodniesienStopni, 
            double szerokoscOstatniegoStopnia, double podestDlugosc, double podestSzerokosc, double podestJakoStopienNr, double dlugoscLiniiBiegu, double katNachylenia, 
            double wysokoscPodniesieniaStopnia, 
            double glebokoscStopnia, double przecietnaDlugoscKroku, double przestrzenSwobodnaNadGlowa , string opis)
        {

            X = x;
            Y = y;
            Skala = skala;
            DlugoscOtworu = dlugoscOtworu;
            SzerokoscOtworu = szerokoscOtworu;
            WysokoscDoStropu = wysokoscDoStropu;
            WysokoscCalkowita = wysokoscCalkowita;
            LiczbaPodniesienStopni = liczbaPodniesienStopni;
            SzerokoscOstatniegoStopnia = szerokoscOstatniegoStopnia;
            PodestDlugosc = podestDlugosc;
            PodestSzerokosc = podestSzerokosc;
            PodestJakoStopienNr = podestJakoStopienNr;

            DlugoscLiniiBiegu = dlugoscLiniiBiegu;
            KatNachylenia = katNachylenia;
            WysokoscPodniesieniaStopnia = wysokoscPodniesieniaStopnia;
            GlebokoscStopnia = glebokoscStopnia;
            PrzecietnaDlugoscKroku = przecietnaDlugoscKroku;
            Opis = opis;

            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        }

        public override async Task DrawAsync(Canvas2DContext context)
        {
            Xpoints = new List<Point>();
            XLinePoint = new List<LinePoint>();

            double currentX = X; // Początkowa pozycja X
            double currentY = Y; // Początkowa pozycja Y

            double stepWidth = GlebokoscStopnia * Skala;  // Szerokość stopnia (długość biegu schodów)
            double stepHeightPoz = PodestSzerokosc * Skala;  // Wysokość stopnia (szerokość biegu schodów)
            double stepHeightPion = PodestDlugosc * Skala;  // Wysokość stopnia (szerokość biegu schodów)

            double delatYKrok = 0;  // Przesunięcie po osi Y

            // Rysujemy obrys schodów (widok od góry)
            await DrawShapeObrys(context, X, Y);

            // Wyświetlenie informacji
            await DrawTextAsync(context, X + 10, Y + 45, $"Informacja: {Opis}");

            double poziomeStopnie = LiczbaPodniesienStopni - PodestJakoStopienNr;

            // Rysowanie poziomych stopni - 1 pomijam spocznik
            for (int i = 0; i < poziomeStopnie - 1; i++)
            {
                await context.BeginPathAsync();
                await context.RectAsync(currentX, currentY, stepWidth, stepHeightPoz);
                await context.StrokeAsync();
                currentX += stepWidth;
            }

            //Spocznik

            await context.BeginPathAsync();
            await context.RectAsync(currentX, currentY, stepHeightPion, stepHeightPoz);
            await context.StrokeAsync();
       
            // Po trapezach resetujemy pozycje X i Y dla pionowych stopni (po bocznej krawędzi)
            // currentX = X + DlugoscOtworu * Skala - stepHeightPion;  // Przesunięcie na dolną krawędź prostokąta
            currentY = Y + stepHeightPoz;  // Ustawiamy Y poniżej trapezów

            delatYKrok = currentY;

            double centerXOsi = currentX;
            double centerYOsi = currentY;

            double pionoweStopnie = LiczbaPodniesienStopni - poziomeStopnie;

            // Rysowanie pionowych stopni
            for (int i = 0; i < pionoweStopnie - 1; i++)
            {
                currentY = Y + delatYKrok;
                await context.BeginPathAsync();
                await context.RectAsync(currentX, currentY, stepHeightPion, stepWidth);
                await context.StrokeAsync();
                delatYKrok += stepWidth;
            }

            // Rysowanie fragmentu okręgu (łuku) - oś schodów
            await context.BeginPathAsync();
            await context.LineToAsync(0, (stepHeightPoz / 2));      // Koniec linii na dużym kwadracie
            await context.LineToAsync(20, (stepHeightPoz / 2) + 5);      // Koniec linii na dużym kwadracie
            await context.LineToAsync(20, (stepHeightPoz / 2) - 5);      // Koniec linii na dużym kwadracie
            await context.ClosePathAsync();
            await context.StrokeAsync();

            double startAngle = 270 * (Math.PI / 180);
            double endAngle = 0 * (Math.PI / 180);

            await context.BeginPathAsync();
            await context.MoveToAsync(0, stepHeightPoz / 2);
            await context.LineToAsync((poziomeStopnie - 1) * GlebokoscStopnia * Skala, stepHeightPoz / 2);
            await context.ArcAsync(centerXOsi, centerYOsi, (stepHeightPoz / 2), startAngle, endAngle);  // Rysujemy łuk
            await context.LineToAsync(centerXOsi + (stepHeightPoz / 2), (SzerokoscOtworu + GlebokoscStopnia) * Skala);
            await context.StrokeAsync();

            //// Rysowanie fragmentu okręgu (łuku) - oś schodów
            //await context.BeginPathAsync();
            //await context.LineToAsync(DlugoscOtworu * Skala, (stepHeightPoz / 2));      // Koniec linii na dużym kwadracie
            //await context.LineToAsync(DlugoscOtworu * Skala - 20, (stepHeightPoz / 2) + 5);      // Koniec linii na dużym kwadracie
            //await context.LineToAsync(DlugoscOtworu * Skala - 20, (stepHeightPoz / 2) - 5);      // Koniec linii na dużym kwadracie
            //await context.ClosePathAsync();
            //await context.StrokeAsync();

            //await context.BeginPathAsync();
            //await context.LineToAsync(X + (stepHeightPoz / 2) * Skala, SzerokoscOtworu * Skala);
            //await context.ArcAsync(centerXOsi, centerYOsi, (stepHeightPoz / 2), 270, 0);  // Rysujemy łuk
            //await context.LineToAsync(DlugoscOtworu * Skala, (stepHeightPoz / 2));
            //await context.StrokeAsync();


            await context.SetFontAsync("16px Arial");
            await context.SetFillStyleAsync("red");

            // Wyświetlanie dodatkowych informacji
            // Draw text
            await context.FillTextAsync($"Wysokość:{WysokoscPodniesieniaStopnia} x {(poziomeStopnie + pionoweStopnie)} = {WysokoscPodniesieniaStopnia * (poziomeStopnie + pionoweStopnie - 1)}" +
                $" - Suma stopni {poziomeStopnie + pionoweStopnie - 1}", X + 10, Y + 20);
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

        //***************************************************************************************************************************************************
        //                                              Funkcja zapisująca rysunek do pliku DXF
        //***************************************************************************************************************************************************
        public async Task SaveToDxfAsync()
        {
            //DXF -------------------------------------------------------------------------------------------------------------------------------------------

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
