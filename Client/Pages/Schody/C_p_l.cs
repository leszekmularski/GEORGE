using Blazor.Extensions.Canvas.Canvas2D;
using netDxf;
using netDxf.Entities;
using Microsoft.JSInterop;
using netDxf.Tables;
using netDxf.Units;
using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Shading;
using GEORGE.Client.Pages.Drzwi;
using System.Numerics;
using netDxf.Blocks;
using netDxf.Collections;
using GTE = netDxf.GTE;
using netDxf.Header;
using netDxf.Objects;

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
        private double ZachodzenieStopniZaSiebie { get; set; }
        private double OdsadzenieStopniaOdBrzegu {  get; set; }
        private double OdsadzeniePierwszStopniaOdBrzegu { get; set; }
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

        private double GruboscStopnia = 40;//40 mm grubość stopni
        public CSchodyPL(IJSRuntime jsRuntime, double x, double y, double skala, double dlugoscOtworu, double szerokoscOtworu, double dlugoscNaWejsciu, double wysokoscDoStropu, double wysokoscCalkowita, double liczbaPodniesienStopni,
            double szerokoscOstatniegoStopnia, double zachodzenieStopniZaSiebie, double odsadzenieStopniaOdBrzegu, double odsadzeniePierwszStopniaOdBrzegu,  double szerokoscBieguSchodow, double dlugoscLiniiBiegu, double katNachylenia, double szerokoscSchodow, double wysokoscPodniesieniaStopnia,
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
            LiczbaPodniesienStopni = liczbaPodniesienStopni - 1;//Ostatni krok stopień -> płyta podłogi
            SzerokoscOstatniegoStopnia = szerokoscOstatniegoStopnia;
            ZachodzenieStopniZaSiebie = zachodzenieStopniZaSiebie;
            OdsadzenieStopniaOdBrzegu = odsadzenieStopniaOdBrzegu;
            OdsadzeniePierwszStopniaOdBrzegu = odsadzeniePierwszStopniaOdBrzegu;
            SzerokoscBieguSchodow = szerokoscBieguSchodow;

            DlugoscLiniiBiegu = dlugoscLiniiBiegu;
            KatNachylenia = katNachylenia;
            SzerokoscSchodow = szerokoscSchodow;
            WysokoscPodniesieniaStopnia = wysokoscPodniesieniaStopnia;
            GlebokoscStopnia = glebokoscStopnia;
            PrzecietnaDlugoscKroku = przecietnaDlugoscKroku;
            Opis = opis;
            Lewe = lewe;

            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        }
        public override async Task DrawAsync(Canvas2DContext context)
        {
            Xpoints = new List<Point>();
            XLinePoint = new List<LinePoint>();

            double currentX = X; // Początkowa pozycja X
            double currentY = Y; // Początkowa pozycja Y

            Console.WriteLine($"Wartść X={X}");

            await context.ClearRectAsync(0, 0, DlugoscNaWejsciu + GlebokoscStopnia, WysokoscPodniesieniaStopnia * (LiczbaPodniesienStopni + 1) + SzerokoscOtworu + 500);

            double stepWidth = GlebokoscStopnia * Skala;  // Szerokość stopnia (długość biegu schodów)
            double stepHeight = SzerokoscBieguSchodow * Skala;  // Wysokość stopnia (szerokość biegu schodów)

            double gruboscStopnia = GruboscStopnia * Skala; // Zrobić zmienną jak będzie poptrzeba!!!!!!

            if (Lewe == 'l')
            {

                // Rysujemy obrys schodów (widok od góry)
                await DrawShapeObrys(context, (X + OdsadzeniePierwszStopniaOdBrzegu * Skala), Y);

                // Wyświetlenie informacji
                await DrawTextAsync(context, X + 10, Y + 45, $"Informacja: {Opis}");

                currentX = X + (DlugoscOtworu - 2 * GlebokoscStopnia) * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth; // Początkowa pozycja X

                //Widok z góry deska boczna tzw. wanga

                await context.SetStrokeStyleAsync("brown");
                await context.SetLineWidthAsync(3);
                await context.BeginPathAsync();
                //Lewa
                await context.RectAsync(X + OdsadzeniePierwszStopniaOdBrzegu * Skala, currentY, (DlugoscLiniiBiegu + OdsadzeniePierwszStopniaOdBrzegu) * Skala, 40 * Skala);
                AddRectanglePoints(X + OdsadzeniePierwszStopniaOdBrzegu * Skala, currentY, (DlugoscLiniiBiegu + OdsadzeniePierwszStopniaOdBrzegu) * Skala, 40 * Skala, "Continuous");
                //Prawa
                await context.RectAsync(X + OdsadzeniePierwszStopniaOdBrzegu * Skala, currentY + (SzerokoscBieguSchodow - 40) * Skala, (DlugoscLiniiBiegu + OdsadzeniePierwszStopniaOdBrzegu) * Skala, 40 * Skala); // gdzie 40 grubość wangi
                AddRectanglePoints(X + OdsadzeniePierwszStopniaOdBrzegu * Skala, currentY + (SzerokoscBieguSchodow - 40) * Skala, (DlugoscLiniiBiegu + OdsadzeniePierwszStopniaOdBrzegu) * Skala, 40 * Skala, "Continuous");
                await context.StrokeAsync();


                for (int i = 0; i < LiczbaPodniesienStopni; i++)
                {
                    await context.BeginPathAsync();

                    if (i == LiczbaPodniesienStopni - 1)
                    {
                        await context.SetStrokeStyleAsync("blue");
                        // Ustaw grubość linii (na przykład na 3 piksele)
                        await context.SetLineWidthAsync(3);
                        await context.RectAsync(currentX, currentY, SzerokoscOstatniegoStopnia * Skala, stepHeight);
                        AddRectanglePoints(currentX, currentY, SzerokoscOstatniegoStopnia * Skala, stepHeight, "Continuous");
                        await context.StrokeAsync();
                        await context.SetStrokeStyleAsync("black");
                        await context.SetLineWidthAsync(1);
                        currentX -= stepWidth + SzerokoscOstatniegoStopnia * Skala;
                    }
                    else
                    {
                        // Ustaw kolor linii na niebieski
                        await context.SetStrokeStyleAsync("blue");
                        // Ustaw grubość linii (na przykład na 3 piksele)
                        await context.SetLineWidthAsync(3);
                        await context.RectAsync(currentX, currentY, stepWidth, stepHeight);
                        await context.StrokeAsync();

                        await context.SetStrokeStyleAsync("black");
                        await context.SetLineWidthAsync(1);
                        await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię
                        await context.RectAsync(currentX - ZachodzenieStopniZaSiebie * Skala, currentY, stepWidth + ZachodzenieStopniZaSiebie * Skala, stepHeight);
                        AddRectanglePoints(currentX - ZachodzenieStopniZaSiebie * Skala, currentY, stepWidth + ZachodzenieStopniZaSiebie * Skala, stepHeight, "dashed");
                        await context.StrokeAsync();
                        await context.SetLineDashAsync(new float[] { });
                        currentX -= stepWidth;
                    }

                }

                // Rysowanie grotu strzałki
                await context.BeginPathAsync();
                await context.LineToAsync((X + OdsadzeniePierwszStopniaOdBrzegu * Skala) + 0, (SzerokoscSchodow * Skala / 2));      // Koniec linii na dużym kwadracie
                AddLineWithPreviousPointAsync((X + OdsadzeniePierwszStopniaOdBrzegu * Skala) + 0, (SzerokoscSchodow * Skala / 2));
                await context.LineToAsync((X + OdsadzeniePierwszStopniaOdBrzegu * Skala) + 20, (SzerokoscSchodow * Skala / 2) + 5);      // Koniec linii na dużym kwadracie
                AddLineWithPreviousPointAsync((X + OdsadzeniePierwszStopniaOdBrzegu * Skala) + 20, (SzerokoscSchodow * Skala / 2) + 5);
                await context.LineToAsync((X + OdsadzeniePierwszStopniaOdBrzegu * Skala) + 20, (SzerokoscSchodow * Skala / 2) - 5);      // Koniec linii na dużym kwadracie
                AddLineWithPreviousPointAsync((X + OdsadzeniePierwszStopniaOdBrzegu * Skala) + 20, (SzerokoscSchodow * Skala / 2) - 5);
                await context.ClosePathAsync();

                ClosePathAndAddFinalLineAsync();

                await context.StrokeAsync();

                //Rysowanie kierunku biegu
                await context.BeginPathAsync();

                // 1. Pierwsza linia (linia pozioma)
                await context.MoveToAsync((X + OdsadzeniePierwszStopniaOdBrzegu * Skala) + DlugoscNaWejsciu * Skala, SzerokoscSchodow * Skala / 2);  // Początek linii
                AddLineWithPreviousPointAsync((X + OdsadzeniePierwszStopniaOdBrzegu * Skala) + DlugoscNaWejsciu * Skala, SzerokoscSchodow * Skala / 2);
                await context.LineToAsync((X + OdsadzeniePierwszStopniaOdBrzegu * Skala), SzerokoscSchodow * Skala / 2);  // Koniec pierwszej linii
                AddLineWithPreviousPointAsync((X + OdsadzeniePierwszStopniaOdBrzegu * Skala), SzerokoscSchodow * Skala / 2);
                await context.StrokeAsync();  // Zakończ rysowanie

                //--------------------------------------------------- Rysowanie schodów widok z boku ------------------------------------------------------------------------------------------------------------------------------

                currentX = X + (DlugoscOtworu - 2 * GlebokoscStopnia) * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth; // Początkowa pozycja X

                currentY = (SzerokoscOtworu + ((LiczbaPodniesienStopni + 1) * WysokoscPodniesieniaStopnia)) * Skala + 25; //25 stały margines

                // Zapisz początkowe wartości do obliczenia rozmiaru obwiedni
                double startX = currentX;
                double startY = currentY;

                for (int i = 0; i < LiczbaPodniesienStopni; i++)
                {
                    await context.BeginPathAsync();

                    if (i == LiczbaPodniesienStopni - 1)
                    {
                        await context.SetStrokeStyleAsync("blue");
                        // Ustaw grubość linii (na przykład na 3 piksele)
                        await context.SetLineWidthAsync(3);
                        await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię
                        await DrawShapeStopinRysBok(context, currentX, currentY, SzerokoscOstatniegoStopnia * Skala, gruboscStopnia);
                        await context.StrokeAsync();
                        await context.SetStrokeStyleAsync("black");
                        await context.SetLineWidthAsync(1);
                        await context.SetLineDashAsync(new float[] { });
                        currentX -= stepWidth + SzerokoscOstatniegoStopnia * Skala;
                        currentY -= WysokoscPodniesieniaStopnia * Skala;
                    }
                    else
                    {
                        // Ustaw kolor linii na niebieski
                        await context.SetStrokeStyleAsync("blue");
                        // Ustaw grubość linii (na przykład na 3 piksele)
                        await context.SetLineWidthAsync(3);
                        //await DrawShapeStopinRysBok(context, currentX, currentY, stepWidth, gruboscStopnia);

                        //await context.SetStrokeStyleAsync("black");
                        //await context.SetLineWidthAsync(1);
                        await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię
                        await DrawShapeStopinRysBok(context, currentX - ZachodzenieStopniZaSiebie * Skala, currentY, stepWidth + ZachodzenieStopniZaSiebie * Skala, gruboscStopnia);
                        await context.StrokeAsync();
                        await context.SetStrokeStyleAsync("black");
                        await context.SetLineWidthAsync(1);
                        await context.SetLineDashAsync(new float[] { });

                        currentX -= stepWidth;
                        currentY -= WysokoscPodniesieniaStopnia * Skala;
                    }

                }

                ClosePathAndAddFinalLineAsync();

                await DrawObrysZKatem(context, startX + stepWidth + OdsadzeniePierwszStopniaOdBrzegu * Skala, startY + WysokoscPodniesieniaStopnia * Skala, currentY, DlugoscLiniiBiegu * Skala,
                    ((LiczbaPodniesienStopni + 1) * WysokoscPodniesieniaStopnia) * Skala, 90 + KatNachylenia, stepWidth + SzerokoscOstatniegoStopnia * Skala);


                //Rysowanie schodów widok z boku KONIEC --------------------------------------------------------------------------------------------------------------------------

            }
            else if (Lewe == 'p')
            {

                // Rysujemy obrys schodów (widok od góry)
                await DrawShapeObrys(context, (X + OdsadzeniePierwszStopniaOdBrzegu * Skala) + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, Y);

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
                await context.LineToAsync(X + DlugoscOtworu * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, (SzerokoscSchodow * Skala / 2));      // Koniec linii na dużym kwadracie
                AddLineWithPreviousPointAsync(X + DlugoscOtworu * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, (SzerokoscSchodow * Skala / 2));
                await context.LineToAsync(X + DlugoscOtworu * Skala - 20 + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, (SzerokoscSchodow * Skala / 2) + 5);      // Koniec linii na dużym kwadracie
                AddLineWithPreviousPointAsync(X + DlugoscOtworu * Skala - 20 + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, (SzerokoscSchodow * Skala / 2) + 5);
                await context.LineToAsync(X + DlugoscOtworu * Skala - 20 + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, (SzerokoscSchodow * Skala / 2) - 5);      // Koniec linii na dużym kwadracie
                AddLineWithPreviousPointAsync(X + DlugoscOtworu * Skala - 20 + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, (SzerokoscSchodow * Skala / 2) - 5);
                await context.ClosePathAsync();

                ClosePathAndAddFinalLineAsync();

                await context.StrokeAsync();

                //Rysowanie kierunku biegu
                await context.BeginPathAsync();

                // 1. Pierwsza linia (linia pozioma)
                await context.MoveToAsync(X + DlugoscOtworu * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, SzerokoscSchodow * Skala / 2);  // Początek linii
                AddLineWithPreviousPointAsync(X + DlugoscOtworu * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, SzerokoscSchodow * Skala / 2);
                await context.LineToAsync(X + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, SzerokoscSchodow * Skala / 2);  // Koniec pierwszej linii
                AddLineWithPreviousPointAsync(X + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth, SzerokoscSchodow * Skala / 2);
                await context.StrokeAsync();  // Zakończ rysowa
                ClearPathAndAddFinalLineAsync();
            }

            await context.SetFontAsync("16px Arial");
            await context.SetFillStyleAsync("red");

            // Wyświetlanie dodatkowych informacji
            // Draw text
            await context.FillTextAsync($"Poziome: {LiczbaPodniesienStopni} = ({System.Math.Round(LiczbaPodniesienStopni * GlebokoscStopnia + SzerokoscOstatniegoStopnia, 0)})" +
               $" Wysokość:{WysokoscPodniesieniaStopnia} x {(LiczbaPodniesienStopni + 1)} = {System.Math.Round(WysokoscPodniesieniaStopnia * (LiczbaPodniesienStopni + 1), 0)}" +
               $" -> Suma stopni {LiczbaPodniesienStopni}", X + 10, Y + 20);
        }

        private async Task DrawShapeObrys(Canvas2DContext context, double offsetX, double offsetY)
        {
            ClearPathAndAddFinalLineAsync();

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

            // Ustaw kolor linii na czarny
            await context.SetStrokeStyleAsync("black");

            // Ustaw grubość linii (na przykład na 3 piksele)
            await context.SetLineWidthAsync(1);

  

        }

        // Funkcja rysująca obrys pod kątem
        private async Task DrawObrysZKatem(Canvas2DContext context, double startX, double startY, double YStartStopienGorny, double szerokosc, double wysokosc,
                                           double katNachylenia, double stepWidth)
        {

            double wysokoscZaczepuY = 40;
            double dlugoscZaczepuX = 50;
            // Konwersja kąta nachylenia na radiany
            katNachylenia = katNachylenia - 90;
            double katRadians = katNachylenia * (Math.PI / 180);

            // Obliczenie przesunięć wzdłuż osi X i Y
            double deltaX = szerokosc * Math.Cos(katRadians);
            double deltaY = wysokosc * Math.Sin(katRadians);

            // Punkt początkowy
            double endX = startX + deltaX;
            double endY = startY - deltaY;

            double offsetKr = OdsadzenieStopniaOdBrzegu;

            double offset20 = Math.Abs(offsetKr / Math.Cos((katNachylenia) * (Math.PI / 180))); // OK
           
            double liniaDol = Math.Abs(offsetKr / Math.Sin((katNachylenia) * (Math.PI / 180)));



            double liniaDolStart = (GlebokoscStopnia + liniaDol) * Math.Tan(katNachylenia * (Math.PI / 180)) + OdsadzeniePierwszStopniaOdBrzegu;

            Console.WriteLine($"liniaDolStart #1: {liniaDolStart}");

            liniaDolStart = liniaDolStart - (WysokoscPodniesieniaStopnia - GruboscStopnia);

            Console.WriteLine($"liniaDolStart #2: {liniaDolStart}");

            liniaDolStart = liniaDolStart / Math.Tan(katNachylenia * (Math.PI / 180));

            Console.WriteLine($"liniaDolStart #3: {liniaDolStart}");



            // Ustaw kolor na zielony i zwiększ grubość linii
            await context.SetStrokeStyleAsync("green");
            await context.SetLineWidthAsync(2);

            // Rozpoczynamy rysowanie nachylonego prostokąta
            await context.BeginPathAsync();
            ClearPathAndAddFinalLineAsync();
            // Punkt początkowy (lewy dolny róg)
            double leftBottomX = startX - liniaDolStart * Skala;
            double leftBottomY = startY;
            Console.WriteLine($"offset20: {offset20} liniaDol: {liniaDol} leftBottomX: {leftBottomX} liniaDolStart: {liniaDolStart} katNachylenia: {katNachylenia}");
            //  await DrawTextAsync(context, leftBottomX, leftBottomY, $"X:{Math.Round(leftBottomX / Skala, 1)} Y:{Math.Round(leftBottomY / Skala, 1)}");

            await context.MoveToAsync(leftBottomX, leftBottomY);
            AddLineWithPreviousPointAsync(leftBottomX, leftBottomY); // Dodanie linii z lewego do prawego dolnego rogu

            // Linia pozioma (lewy dolny do prawy dolny)
            await context.LineToAsync(startX, startY);
            AddLineWithPreviousPointAsync(startX, startY); // Dodanie linii pionowej
             
            // Linia pionowa (prawy dolny do prawego górnego)
            double rightBottomX = startX;
            double startYlP1 = ((OdsadzeniePierwszStopniaOdBrzegu - liniaDol) * Math.Tan(katNachylenia * (Math.PI / 180)));
            double rightBottomY = startY - ((WysokoscPodniesieniaStopnia - startYlP1) * Skala); //??????????????????????????????????????????????????????????????????????????????????????????????????
            await context.LineToAsync(rightBottomX, rightBottomY);
            AddLineWithPreviousPointAsync(rightBottomX, rightBottomY); // Dodanie linii skośnej

            double przeciwProstokatna = Math.Sqrt(Math.Pow((WysokoscPodniesieniaStopnia + wysokoscZaczepuY) * Skala, 2) + Math.Pow((WysokoscPodniesieniaStopnia * Skala) * Math.Cos(katNachylenia * (Math.PI / 180)), 2));

            double cosKat = Math.Sqrt((Math.Pow(przeciwProstokatna, 2) - Math.Pow((WysokoscPodniesieniaStopnia + wysokoscZaczepuY) * Skala, 2)) / Math.Pow(WysokoscPodniesieniaStopnia * Skala, 2));

            Console.WriteLine($"przeciwProstokatna: {przeciwProstokatna}  / katNachylenia: {katNachylenia} / Punkt Y: {WysokoscPodniesieniaStopnia + wysokoscZaczepuY}");

            double liniaXGora = (GlebokoscStopnia + liniaDol) * Math.Tan(katNachylenia * (Math.PI / 180));

            Console.WriteLine($"liniaXGora #1: {liniaXGora} Suma: {(GlebokoscStopnia + liniaDol)}");

            //liniaXGora = liniaXGora - (WysokoscPodniesieniaStopnia + wysokoscZaczepuY);

            //Console.WriteLine($"liniaXGora #2: {liniaXGora}");

            liniaXGora = ((WysokoscPodniesieniaStopnia + wysokoscZaczepuY) - liniaXGora) / Math.Tan(katNachylenia * (Math.PI / 180));

            Console.WriteLine($"liniaXGora #2: {liniaXGora}");

            liniaXGora = dlugoscZaczepuX - liniaXGora + OdsadzeniePierwszStopniaOdBrzegu;

            Console.WriteLine($"liniaXGora #3: {liniaXGora}");

            // **Skośna linia (od prawego górnego punktu do prawego górnego stopnia)**

            double rightUpperX = X + liniaXGora * Skala;
            double rightUpperY = YStartStopienGorny - GruboscStopnia * Skala;

            // Rysowanie skośnej linii do prawego górnego rogu ostatniego stopnia
            await context.LineToAsync(rightUpperX, rightUpperY); //Linia równoległa do osi schodów w odległości ~20mm od krawędzi stopnia
            AddLineWithPreviousPointAsync(rightUpperX, rightUpperY); // Dodanie skośnej linii

            double endYFinal = rightUpperY + Math.Sqrt(Math.Pow(przeciwProstokatna, 2) - Math.Pow((WysokoscPodniesieniaStopnia * Skala) * Math.Cos(katNachylenia * (Math.PI / 180)), 2));

            //********************************************************************************************************************************************************

            // Obliczenie przesunięć wzdłuż osi X i Y dla wydłużenia
            double deltaXWydl = 2 * (X + OdsadzeniePierwszStopniaOdBrzegu * Skala) * Math.Cos((katNachylenia + 90) * (Math.PI / 180));
            double deltaYWydl = 2 * Y * Math.Sin((katNachylenia + 90) * (Math.PI / 180));
            // Dodanie linii do wydłużonego końca
            double extendedUpperX = rightUpperX + deltaXWydl;
            double extendedUpperY = rightUpperY + deltaYWydl;

            //********************************************************************************************************************************************************

            // Zaczep górny
            double hookX1 = (X + OdsadzeniePierwszStopniaOdBrzegu * Skala) - dlugoscZaczepuX * Skala;
            double hookY1 = rightUpperY;// + deltaYWydl;
            double hookX2 = hookX1;
            double hookY2 = hookY1 + wysokoscZaczepuY * Skala;
            double hookX3 = (X + OdsadzeniePierwszStopniaOdBrzegu * Skala);
            double hookY3 = hookY2;
            await context.LineToAsync(hookX1, hookY1);
            AddLineWithPreviousPointAsync(hookX1, hookY1); // Dodanie linii zaczepu
            await context.LineToAsync(hookX2, hookY2);
            AddLineWithPreviousPointAsync(hookX2, hookY2); // Dodanie pionowej linii zaczepu

            await context.LineToAsync(hookX3, hookY3);
            AddLineWithPreviousPointAsync(hookX3, hookY3); // Dodanie poziomej linii zaczepu

            // Linia końcowa pionowa
            double endXFinal = (X + OdsadzeniePierwszStopniaOdBrzegu * Skala);

            await context.LineToAsync(endXFinal, endYFinal + GruboscStopnia * Skala + offset20 * Skala);
            AddLineWithPreviousPointAsync(endXFinal, endYFinal + GruboscStopnia * Skala + offset20 * Skala); // Dodanie końcowej pionowej linii

            // Zamknięcie ścieżki
            await context.ClosePathAsync();

            ClosePathAndAddFinalLineAsync();

            await context.StrokeAsync();

            await DrawObrysSufitu(context, hookY1, stepWidth, wysokoscZaczepuY, dlugoscZaczepuX);

        }

        private async Task DrawObrysSufitu(Canvas2DContext context, double hookY1, double stepWidth, double wysZaczepuY, double dlugoscZaczepuX)
        {

            double iloscWystajacych = (DlugoscLiniiBiegu - DlugoscOtworu) * Skala + 400;

            if (iloscWystajacych < 800) iloscWystajacych = 800;

            // Ustaw kolor wypełnienia tylko dla prostokąta sufitu
            await context.SetFillStyleAsync("gray");
            // Rozpoczynamy rysowanie nachylonego prostokąta
            await context.BeginPathAsync();
            // Rysuj i wypełnij tylko ten prostokąt
            await context.RectAsync((X + OdsadzeniePierwszStopniaOdBrzegu * Skala) + DlugoscOtworu * Skala, hookY1 + wysZaczepuY * Skala, iloscWystajacych, 140 * Skala); // 140 teoretyczna grubość stropu
            await context.FillAsync(); // Wypełnia prostokąt kolorem

            // await context.BeginBatchAsync();
            await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię
            // Rysuj obrys dla tego prostokąta
            await context.SetStrokeStyleAsync("gray");
            await context.SetLineWidthAsync(1);
            await context.RectAsync((X + OdsadzeniePierwszStopniaOdBrzegu * Skala) + 50 * Skala - dlugoscZaczepuX * Skala, hookY1 + wysZaczepuY * Skala, (DlugoscOtworu + dlugoscZaczepuX - 50) * Skala, 140 * Skala);
            await context.StrokeAsync(); // Rysuje obrys

            await context.SetLineDashAsync(new float[] { }); // Ustaw przerywaną linię
                                                             //  await context.BeginBatchAsync();

            AddRectanglePoints((X + OdsadzeniePierwszStopniaOdBrzegu * Skala) + DlugoscOtworu * Skala, hookY1 + wysZaczepuY * Skala, iloscWystajacych, 140 * Skala);
            AddRectanglePoints((X + OdsadzeniePierwszStopniaOdBrzegu * Skala) + 50 * Skala - dlugoscZaczepuX * Skala, hookY1 + wysZaczepuY * Skala, (DlugoscOtworu + dlugoscZaczepuX - 50) * Skala, 140 * Skala, "dashed");

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
        private void AddRectanglePoints(double x, double y, double width, double height, string typeLine = "")
        {
            if (XLinePoint == null) return;

            XLinePoint.Add(new LinePoint(x, y, x + width, y, typeLine));
            XLinePoint.Add(new LinePoint(x + width, y, x + width, y + height, typeLine));
            XLinePoint.Add(new LinePoint(x + width, y + height, x, y + height, typeLine));
            XLinePoint.Add(new LinePoint(x, y + height, x, y, typeLine));
        }

        private void AddLinePoints(double x1, double y1, double x2, double y2, string typeLine = "")
        {
            if (XLinePoint == null) return;

            XLinePoint.Add(new LinePoint(x1, y1, x2, y2, typeLine));
        }

        // Zmienna do śledzenia punktu poprzedniego
        double? previousX = null;
        double? previousY = null;

        // Zmienna do śledzenia pierwszego punktu ścieżki
        double? firstX = null;
        double? firstY = null;

        // Funkcja do dodawania linii z użyciem poprzedniego punktu
        private void AddLineWithPreviousPointAsync(double x, double y, string typeLine = "")
        {
            // Sprawdzamy, czy istnieje poprzedni punkt, aby utworzyć linię
            if (previousX.HasValue && previousY.HasValue)
            {
                // Dodajemy linię do XLinePoint od poprzedniego punktu do bieżącego
                AddLinePoints(previousX.Value, previousY.Value, x, y, typeLine);
            }
            else
            {
                // Jeśli to pierwszy punkt, ustaw go jako punkt początkowy
                firstX = x;
                firstY = y;
            }

            // Ustawiamy bieżący punkt jako poprzedni
            previousX = x;
            previousY = y;
        }

        // Funkcja do zamknięcia ścieżki z dodaniem ostatniego segmentu do XLinePoint
        private void ClosePathAndAddFinalLineAsync(string typeLine = "")
        {
            if (previousX.HasValue && previousY.HasValue && firstX.HasValue && firstY.HasValue)
            {
                // Dodajemy ostatnią linię od ostatniego punktu do pierwszego
                AddLinePoints(previousX.Value, previousY.Value, firstX.Value, firstY.Value, typeLine);

                ClearPathAndAddFinalLineAsync();
            }

        }

        private void ClearPathAndAddFinalLineAsync()
        {
            previousX = null;
            previousY = null;
            // Zmienna do śledzenia pierwszego punktu ścieżki
            firstX = null;
            firstY = null;
        }
        public override Task<List<LinePoint>> ReturnLinePoints()
        {
            return Task.FromResult(XLinePoint ?? new List<LinePoint>());
        }

        // Funkcja zapisująca rysunek do pliku DXF

        public async Task SaveToDxfAsync()
        {
            try
            {
                Console.WriteLine($"Rozpoczęcie zapisu pliku DXF.");

                //var headerVariables = new HeaderVariables {AcadVer = DxfVersion.AutoCad2000 };

                //DxfDocument dxf = new(headerVariables);
                var supportFolders = new SupportFolders(); // lub zgodnie z wymaganiami
                var drawingVariables = new HeaderVariables();

                DxfDocument dxf = new(drawingVariables, supportFolders);


                // Tworzymy nowy dokument DXF
                //  DxfDocument dxf = new(DxfVersion.AutoCad2000);

                Console.WriteLine($"Załadowanie biblioteki DXF.");
                // Sprawdzamy, czy lista XLinePoint zawiera punkty
                if (XLinePoint != null)
                {
                    // Iteracja przez każdy punkt w XLinePoint
                    foreach (var linePoint in XLinePoint)
                    {
                        // Tworzenie linii w DXF na podstawie danych z LinePoint
                        netDxf.Entities.Line dxfLine = new netDxf.Entities.Line(
                            new netDxf.Vector2(linePoint.X1, linePoint.Y1),
                            new netDxf.Vector2(linePoint.X2, linePoint.Y2)
                        );

                        if (string.Equals(linePoint.typeLine, "dashed", StringComparison.OrdinalIgnoreCase))
                        {
                            dxfLine.Linetype = Linetype.Dashed;
                        }
                        else
                        {
                            dxfLine.Linetype = Linetype.Continuous;
                        }

                        // Dodanie linii do dokumentu DXF
                        dxf.Entities.Add(dxfLine);
                    }
                }

                SetNegativeCoordinates(dxf);

                UpdateDimensionStyleTextHeightAndFitView(dxf, "Standard", 35, "arial.ttf");
                Console.WriteLine($"Ustawienie stylu DXF.");
                // Zapis pliku DXF do strumienia i pobranie go

                // Zapis pliku DXF do strumienia i pobranie go
                using (MemoryStream stream = new MemoryStream())
                {
                    dxf.Save(stream);
                    Console.WriteLine($"Rozmiar pliku DXF w bajtach: {stream.Length}");

                    string base64String = Convert.ToBase64String(stream.ToArray());
                    await _jsRuntime.InvokeVoidAsync("downloadFileDXF", $"schody_proste_{Lewe}.dxf", base64String);
                }


                if (XLinePoint != null)
                {
                    Console.WriteLine($"Plik DXF został wygenerowany. Ilość Wektorów {XLinePoint.Count()}");
                }
                else
                {
                    Console.WriteLine($"Plik DXF został wygenerowany. XLinePoint - NULL");
                }

                // await SaveToStlAsync(); // to do poprawy
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd:{ex.Message} / {ex.StackTrace}");
            }
        }


        public async Task SaveToStlAsync()
        {
            // Inicjalizacja dokumentu sceny 3D
            var scene = new Scene();

            // Tworzymy materiał dla linii przerywanych, jeśli są potrzebne
            var dashedMaterial = new LambertMaterial() { Name = "DashedMaterial" };
            var continuousMaterial = new LambertMaterial() { Name = "ContinuousMaterial" };

            // Sprawdzamy, czy lista punktów istnieje
            if (XLinePoint != null)
            {
                foreach (var linePoint in XLinePoint)
                {
                    // Konwersja punktów startu i końca z netDxf.Vector3 do Aspose.ThreeD.Utilities.Vector3
                    var start = new Aspose.ThreeD.Utilities.Vector3(linePoint.X1, linePoint.Y1, 0);
                    var end = new Aspose.ThreeD.Utilities.Vector3(linePoint.X2, linePoint.Y2, 0);

                    // Obliczamy długość linii jako wysokość boxa
                    double height = start.Dot(end);
                    double thickness = 40.0; // Grubość linii 40mm

                    // Tworzymy box o grubości 40 mm i wysokości równej długości linii
                    var lineSurface = new Box(thickness, height, 0.1); // 0.1 jako minimalna głębokość

                    // Ustawiamy pozycję oraz materiał
                    var lineNode = scene.RootNode.CreateChildNode(lineSurface);
                    lineNode.Transform.Translation = start;
                    lineNode.Material = linePoint.typeLine.ToLower() == "dashed" ? dashedMaterial : continuousMaterial;

                    // Rotacja boxa, aby był w orientacji od start do end
                    var direction = (end - start).Normalize();
                    var initialDirection = new Aspose.ThreeD.Utilities.Vector3(0, 1, 0); // Domyślny kierunek w osi Y
                                                                                         // lineNode.Transform.Rotation = Quaternion.Dot(initialDirection, direction);
                }
            }

            // Ścieżka do pliku wyjściowego
            var filePath = Path.Combine(Path.GetTempPath(), $"schody_proste_{Lewe}.stl");

            // Zapis sceny do pliku STL
            scene.Save(filePath, FileFormat.STLBinary);

            // Przesyłanie pliku do przeglądarki jako Base64
            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            string base64String = Convert.ToBase64String(fileBytes);

            // Wywołanie JavaScript w celu pobrania pliku
            await _jsRuntime.InvokeVoidAsync("downloadFile", $"schody_proste_{Lewe}.stl", base64String);

            Console.WriteLine($"Plik STL został wygenerowany. Ilość Wektorów: {XLinePoint?.Count ?? 0}");

            // Ścieżka do pliku wyjściowego

        }

        void UpdateDimensionStyleTextHeightAndFitView(DxfDocument dxf, string dimensionStyleName, double newTextHeight, string fontFilePath)
        {
            try
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
                        var newTextStyle = new TextStyle(fontFilePath, "Arial")
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
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd:{ex.Message} / {ex.StackTrace}");
            }
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
                    if (line is netDxf.Entities.Line)
                    {
                        // Przypisujemy nowe współrzędne z konwersją na Vector3
                        line.StartPoint = new netDxf.Vector3(line.StartPoint.X, -line.StartPoint.Y, line.StartPoint.Z);
                        line.EndPoint = new netDxf.Vector3(line.EndPoint.X, -line.EndPoint.Y, line.EndPoint.Z);
                    }
                }

                foreach (var arc in entities.Arcs)
                {
                    if (arc is Arc)
                    {
                        // Przypisujemy nowe współrzędne z konwersją na Vector3
                        arc.Center = new netDxf.Vector3(arc.Center.X, -arc.Center.Y, arc.Center.Z);

                        // Przekształcamy kąty, aby dostosować się do nowego układu współrzędnych
                        // Zakładamy, że kąty są wyrażone w stopniach lub radianach, więc zmiana znaku jest potrzebna
                        arc.StartAngle = 90 + arc.StartAngle;
                        arc.EndAngle = 90 + arc.EndAngle;

                    }
                }

                // Dodaj inne przypadki dla różnych typów obiektów, które mogą zawierać współrzędne
            }
        }

    }

}
