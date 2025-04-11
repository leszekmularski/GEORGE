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
        private double WydluzOstatniStopien { get; set; }
        private double ZachodzenieStopniZaSiebie { get; set; }
        private double OsadzenieOdGory { get; set; }
        private double OsadzenieOdDolu { get; set; }
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
        private string NazwaProgramuCNC { get; set; }

        private double GruboscStopnia = 40;//40 mm grubość stopni
        public CSchodyPL(IJSRuntime jsRuntime, double x, double y, double skala, double dlugoscOtworu, double szerokoscOtworu, double dlugoscNaWejsciu, double wysokoscDoStropu, double wysokoscCalkowita, double liczbaPodniesienStopni,
            double wydluzOstatniStopien, double zachodzenieStopniZaSiebie, double osadzenieOdGory, double osadzenieOdDolu, double szerokoscBieguSchodow, double dlugoscLiniiBiegu, double katNachylenia, double szerokoscSchodow, double wysokoscPodniesieniaStopnia,
            double glebokoscStopnia, double przecietnaDlugoscKroku, double przestrzenSwobodnaNadGlowa, string opis, char lewe, string nazwaProgramuCNC)
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
            WydluzOstatniStopien = wydluzOstatniStopien;
            ZachodzenieStopniZaSiebie = zachodzenieStopniZaSiebie;
            OsadzenieOdGory = osadzenieOdGory;
            OsadzenieOdDolu = osadzenieOdDolu;
            SzerokoscBieguSchodow = szerokoscBieguSchodow;

            DlugoscLiniiBiegu = dlugoscLiniiBiegu;
            KatNachylenia = katNachylenia;
            SzerokoscSchodow = szerokoscSchodow;
            WysokoscPodniesieniaStopnia = wysokoscPodniesieniaStopnia;
            GlebokoscStopnia = glebokoscStopnia;
            PrzecietnaDlugoscKroku = przecietnaDlugoscKroku;
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

            await context.ClearRectAsync(0, 0, DlugoscNaWejsciu + GlebokoscStopnia, WysokoscPodniesieniaStopnia * (LiczbaPodniesienStopni + 1) + SzerokoscOtworu + 500);

            double stepWidth = GlebokoscStopnia * Skala;  // Szerokość stopnia (długość biegu schodów)
            double stepHeight = SzerokoscBieguSchodow * Skala;  // Wysokość stopnia (szerokość biegu schodów)

            //Wyliczanie kroku w rysowaniu kolejnego stopnia
            //stepWidth = (WysokoscPodniesieniaStopnia * Math.Tan((90 - (KatNachylenia + 1.057)) * (Math.PI / 180))) * Skala;//16.12.2024
            double stepWidthTMP = (WysokoscPodniesieniaStopnia * Skala) * Math.Tan((90 - KatNachylenia) * (Math.PI / 180));//16.12.2024

            Console.WriteLine($"Wartść X={X} DlugoscLiniiBiegu:{DlugoscLiniiBiegu} ZachodzenieStopniZaSiebie:{ZachodzenieStopniZaSiebie} stepWidth:{stepWidth}");

            double gruboscStopnia = GruboscStopnia * Skala; // Zrobić zmienną jak będzie poptrzeba!!!!!!

            if (Lewe == 'l')
            {

                // Rysujemy obrys schodów (widok od góry)
                await DrawShapeObrys(context, X, Y);

                // Wyświetlenie informacji
                await DrawTextAsync(context, X + 10, Y + 45, $"Informacja: {Opis}");

                currentX = X + (DlugoscOtworu - 2 * GlebokoscStopnia) * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala + stepWidth; // Początkowa pozycja X

                //Widok z góry deska boczna tzw. wanga

                await context.SetStrokeStyleAsync("brown");
                await context.SetLineWidthAsync(3);
                await context.BeginPathAsync();
                //Lewa
                await context.RectAsync(X, currentY, DlugoscLiniiBiegu * Skala, 40 * Skala);
                AddRectanglePoints(X, currentY, DlugoscLiniiBiegu * Skala, 40 * Skala, "Continuous");
                //Prawa
                await context.RectAsync(X, currentY + (SzerokoscBieguSchodow - 40) * Skala, DlugoscLiniiBiegu * Skala, 40 * Skala); // gdzie 40 grubość wangi
                AddRectanglePoints(X, currentY + (SzerokoscBieguSchodow - 40) * Skala, DlugoscLiniiBiegu * Skala, 40 * Skala, "Continuous");
                await context.StrokeAsync();

                string zRob = "Z21,Z2,";
                //Rysowanie stopi schodów widok z góry
                for (int i = 0; i < LiczbaPodniesienStopni; i++)
                {
                    await context.BeginPathAsync();

                    if (i == LiczbaPodniesienStopni - 1)
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
                        await context.RectAsync(currentX - (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala, currentY, stepWidth + (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala, stepHeight);
                        // AddRectanglePoints(currentX - (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala, currentY, stepWidth + (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala, stepHeight, "dashed","S_G", "STOPIEN_W1", true);
                        AddPointsStopienObrys(currentX - (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala, currentY, stepWidth + (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala,
                            stepHeight, "dashed", "S_G", "STOPIEN_W2", "FO" + i.ToString(), zRob.Split(','), i, true, "Z27.1,", 1, NazwaProgramuCNC, "Stopień górny");
                        await context.StrokeAsync();
                        await context.SetLineDashAsync(new float[] { });

                        currentX -= stepWidthTMP;
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

                        if (i == LiczbaPodniesienStopni - 3 || LiczbaPodniesienStopni == 2)//Ten warunek służy wyłączmie tylko do wygenerowaniu 1 programu.
                        {
                            AddPointsStopienObrys(currentX - ZachodzenieStopniZaSiebie * Skala, currentY, stepWidth + ZachodzenieStopniZaSiebie * Skala, stepHeight, "dashed", "S_D", "STOPIEN_W2",
                                "FO" + i.ToString(), zRob.Split(','), i, true, "Z27.1,", (int)(LiczbaPodniesienStopni - 1), NazwaProgramuCNC, "Pozostałe stopnie");
                        }
                        else
                        {
                            AddRectanglePoints(currentX - ZachodzenieStopniZaSiebie * Skala, currentY, stepWidth + ZachodzenieStopniZaSiebie * Skala, stepHeight, "dashed");
                        }

                        await context.StrokeAsync();
                        await context.SetLineDashAsync(new float[] { });

                        currentX -= stepWidthTMP;
                    }

                }

                // Rysowanie grotu strzałki
                await context.BeginPathAsync();
                await context.LineToAsync(X, (SzerokoscSchodow * Skala / 2));      // Koniec linii na dużym kwadracie
                AddLineWithPreviousPointAsync(X, (SzerokoscSchodow * Skala / 2));
                await context.LineToAsync(X + 20, (SzerokoscSchodow * Skala / 2) + 5);      // Koniec linii na dużym kwadracie
                AddLineWithPreviousPointAsync(X + 20, (SzerokoscSchodow * Skala / 2) + 5);
                await context.LineToAsync(X + 20, (SzerokoscSchodow * Skala / 2) - 5);      // Koniec linii na dużym kwadracie
                AddLineWithPreviousPointAsync(X + 20, (SzerokoscSchodow * Skala / 2) - 5);
                await context.ClosePathAsync();

                ClosePathAndAddFinalLineAsync();

                await context.StrokeAsync();

                //Rysowanie kierunku biegu
                await context.BeginPathAsync();

                // 1. Pierwsza linia (linia pozioma)
                await context.MoveToAsync(X + DlugoscNaWejsciu * Skala, SzerokoscSchodow * Skala / 2);  // Początek linii
                AddLineWithPreviousPointAsync(X + DlugoscNaWejsciu * Skala, SzerokoscSchodow * Skala / 2);
                await context.LineToAsync(X, SzerokoscSchodow * Skala / 2);  // Koniec pierwszej linii
                AddLineWithPreviousPointAsync(X, SzerokoscSchodow * Skala / 2);
                await context.StrokeAsync();  // Zakończ rysowanie

                //--------------------------------------------------- Rysowanie schodów widok z boku ------------------------------------------------------------------------------------------------------------------------------

                currentX = X + ((DlugoscOtworu - 2 * GlebokoscStopnia) * Skala) + ((DlugoscNaWejsciu - DlugoscOtworu) * Skala) + stepWidth; // Początkowa pozycja X

                currentY = (SzerokoscOtworu + ((LiczbaPodniesienStopni + 1) * WysokoscPodniesieniaStopnia)) * Skala + 25; //25 stały margines

                // Zapisz początkowe wartości do obliczenia rozmiaru obwiedni
                double startX = currentX;
                double startY = currentY;
                zRob = "Z19.8,";

                string iSort = ""; //Sposób sortowanie frezowań

                for (int i = 0; i < LiczbaPodniesienStopni; i++)
                {
                    await context.BeginPathAsync();

                    iSort += "1";

                    if (i == LiczbaPodniesienStopni - 1)
                    {
                        // Ustaw kolor linii na niebieski
                        await context.SetStrokeStyleAsync("blue");
                        // Ustaw grubość linii (na przykład na 3 piksele)
                        await context.SetLineWidthAsync(3);

                        await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię
                        await DrawShapeStopinRysBok(context, currentX - (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala, currentY, stepWidth + (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala, gruboscStopnia);
                        await DrawShapeStopinRysBok(context, currentX - (ZachodzenieStopniZaSiebie + WydluzOstatniStopien - 17) * Skala, currentY + 13 * Skala, stepWidth + (ZachodzenieStopniZaSiebie + WydluzOstatniStopien - 17 * 2) * Skala,
                            gruboscStopnia - 13 * Skala, "dashed", "W_L", "WANGA_KIESZEN", iSort, zRob.Split(','), i, true, 1, NazwaProgramuCNC, "Wanga lewa");
                        await context.StrokeAsync();
                        await context.SetStrokeStyleAsync("black");
                        await context.SetLineWidthAsync(1);
                        await context.SetLineDashAsync(new float[] { });

                        currentY -= WysokoscPodniesieniaStopnia * Skala;
                    }
                    else
                    {
                        // Ustaw kolor linii na niebieski
                        await context.SetStrokeStyleAsync("blue");
                        // Ustaw grubość linii (na przykład na 3 piksele)
                        await context.SetLineWidthAsync(3);

                        await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię
                        await DrawShapeStopinRysBok(context, currentX - ZachodzenieStopniZaSiebie * Skala, currentY, stepWidth + ZachodzenieStopniZaSiebie * Skala, gruboscStopnia);
                        await DrawShapeStopinRysBok(context, currentX - ((ZachodzenieStopniZaSiebie - 17) * Skala), currentY + 13 * Skala, stepWidth + ((ZachodzenieStopniZaSiebie - 17 * 2) * Skala),
                            gruboscStopnia - 13 * Skala, "dashed", "W_L", "WANGA_KIESZEN", iSort, zRob.Split(','), i, true, 1, NazwaProgramuCNC);
                        await context.StrokeAsync();
                        await context.SetStrokeStyleAsync("black");
                        await context.SetLineWidthAsync(1);
                        await context.SetLineDashAsync(new float[] { });

                        currentX -= stepWidthTMP;
                        currentY -= WysokoscPodniesieniaStopnia * Skala;
                    }

                }

                ClosePathAndAddFinalLineAsync();

                await DrawObrysZKatem(context, startX + stepWidth, startY + WysokoscPodniesieniaStopnia * Skala, currentY, DlugoscLiniiBiegu * Skala,
                    ((LiczbaPodniesienStopni + 1) * WysokoscPodniesieniaStopnia) * Skala, 90 + KatNachylenia, stepWidth + GlebokoscStopnia * Skala, currentX - ZachodzenieStopniZaSiebie * Skala - X);


                //Rysowanie schodów widok z boku KONIEC --------------------------------------------------------------------------------------------------------------------------

            }
            else if (Lewe == 'p')
            {

                // Rysujemy obrys schodów (widok od góry)
                await DrawShapeObrys(context, X + (DlugoscNaWejsciu - DlugoscOtworu) * Skala, Y);

                // Wyświetlenie informacji
                await DrawTextAsync(context, X + 10, Y + 45, $"Informacja: {Opis}");

                currentX = X; // Początkowa pozycja X

                await context.SetStrokeStyleAsync("brown");
                await context.SetLineWidthAsync(3);
                await context.BeginPathAsync();
                //Lewa
                await context.RectAsync(X, currentY, DlugoscLiniiBiegu * Skala, 40 * Skala);
                AddRectanglePoints(X, currentY, DlugoscLiniiBiegu * Skala, 40 * Skala, "Continuous");
                //Prawa
                await context.RectAsync(X, currentY + (SzerokoscBieguSchodow - 40) * Skala, DlugoscLiniiBiegu * Skala, 40 * Skala); // gdzie 40 grubość wangi
                AddRectanglePoints(X, currentY + (SzerokoscBieguSchodow - 40) * Skala, DlugoscLiniiBiegu * Skala, 40 * Skala, "Continuous");
                await context.StrokeAsync();

                string zRob = "Z21,Z2,";

                for (int i = 0; i < LiczbaPodniesienStopni; i++)
                {
                    await context.BeginPathAsync();

                    if (i == LiczbaPodniesienStopni - 1)
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
                        await context.RectAsync(currentX - (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala, currentY, stepWidth + (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala, stepHeight);
                        // AddRectanglePoints(currentX - (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala, currentY, stepWidth + (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala, stepHeight, "dashed","S_G", "STOPIEN_W1", true);
                        AddPointsStopienObrys(currentX - (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala, currentY, stepWidth + (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala,
                            stepHeight, "dashed", "S_G", "STOPIEN_W2", "FO" + i.ToString(), zRob.Split(','), i, true, "Z27.1,", 1, NazwaProgramuCNC, "Stopień górny");
                        await context.StrokeAsync();
                        await context.SetLineDashAsync(new float[] { });

                        currentX += stepWidthTMP;
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

                        if (i == LiczbaPodniesienStopni - 3 || LiczbaPodniesienStopni == 2)//Ten warunek służy wyłączmie tylko do wygenerowaniu 1 programu.
                        {
                            AddPointsStopienObrys(currentX - ZachodzenieStopniZaSiebie * Skala, currentY, stepWidth + ZachodzenieStopniZaSiebie * Skala, stepHeight, "dashed", "S_D", "STOPIEN_W2", "FO" + i.ToString(),
                                zRob.Split(','), i, true, "Z27.1,", (int)(LiczbaPodniesienStopni - 1), NazwaProgramuCNC, "Pozostałe stopnie");
                        }
                        else
                        {
                            AddRectanglePoints(currentX - ZachodzenieStopniZaSiebie * Skala, currentY, stepWidth + ZachodzenieStopniZaSiebie * Skala, stepHeight, "dashed");
                        }

                        await context.StrokeAsync();
                        await context.SetLineDashAsync(new float[] { });

                        currentX += stepWidthTMP;
                    }

                }

                // Rysowanie grotu strzałki
                await context.BeginPathAsync();
                await context.LineToAsync(X + DlugoscOtworu * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala, (SzerokoscSchodow * Skala / 2));      // Koniec linii na dużym kwadracie
                AddLineWithPreviousPointAsync(X + DlugoscOtworu * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala, (SzerokoscSchodow * Skala / 2));
                await context.LineToAsync(X + DlugoscOtworu * Skala - 20 + (DlugoscNaWejsciu - DlugoscOtworu) * Skala, (SzerokoscSchodow * Skala / 2) + 5);      // Koniec linii na dużym kwadracie
                AddLineWithPreviousPointAsync(X + DlugoscOtworu * Skala - 20 + (DlugoscNaWejsciu - DlugoscOtworu) * Skala, (SzerokoscSchodow * Skala / 2) + 5);
                await context.LineToAsync(X + DlugoscOtworu * Skala - 20 + (DlugoscNaWejsciu - DlugoscOtworu) * Skala, (SzerokoscSchodow * Skala / 2) - 5);      // Koniec linii na dużym kwadracie
                AddLineWithPreviousPointAsync(X + DlugoscOtworu * Skala - 20 + (DlugoscNaWejsciu - DlugoscOtworu) * Skala, (SzerokoscSchodow * Skala / 2) - 5);
                await context.ClosePathAsync();

                ClosePathAndAddFinalLineAsync();

                await context.StrokeAsync();

                //Rysowanie kierunku biegu
                await context.BeginPathAsync();

                // 1. Pierwsza linia (linia pozioma)
                await context.MoveToAsync(X + DlugoscOtworu * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala, SzerokoscSchodow * Skala / 2);  // Początek linii
                AddLineWithPreviousPointAsync(X + DlugoscOtworu * Skala + (DlugoscNaWejsciu - DlugoscOtworu) * Skala, SzerokoscSchodow * Skala / 2);
                await context.LineToAsync(X, SzerokoscSchodow * Skala / 2);  // Koniec pierwszej linii
                AddLineWithPreviousPointAsync(X, SzerokoscSchodow * Skala / 2);
                await context.StrokeAsync();  // Zakończ rysowa
                ClearPathAndAddFinalLineAsync();

                //--------------------------------------------------- Rysowanie schodów widok z boku ------------------------------------------------------------------------------------------------------------------------------

                zRob = "Z21,Z2,";

                currentX = X;// + ((DlugoscOtworu - 2 * GlebokoscStopnia) * Skala) + ((DlugoscNaWejsciu - DlugoscOtworu) * Skala) + stepWidth; // Początkowa pozycja X

                currentY = (SzerokoscOtworu + ((LiczbaPodniesienStopni + 1) * WysokoscPodniesieniaStopnia)) * Skala + 25; //25 stały margines

                // Zapisz początkowe wartości do obliczenia rozmiaru obwiedni
                double startX = currentX;
                double startY = currentY;
                zRob = "Z19.8,";

                string iSort = ""; //Sposób sortowanie frezowań

                for (int i = 0; i < LiczbaPodniesienStopni; i++)
                {
                    await context.BeginPathAsync();

                    iSort += "1";

                    if (i == LiczbaPodniesienStopni - 1)
                    {
                        // Ustaw kolor linii na niebieski
                        await context.SetStrokeStyleAsync("blue");
                        // Ustaw grubość linii (na przykład na 3 piksele)
                        await context.SetLineWidthAsync(3);

                        await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię
                        await DrawShapeStopinRysBok(context, currentX - (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala, currentY, stepWidth + (ZachodzenieStopniZaSiebie + WydluzOstatniStopien) * Skala, gruboscStopnia);
                        await DrawShapeStopinRysBok(context, currentX - (ZachodzenieStopniZaSiebie + WydluzOstatniStopien - 17) * Skala, currentY + 13 * Skala, stepWidth + (ZachodzenieStopniZaSiebie + WydluzOstatniStopien - 17 * 2) * Skala,
                            gruboscStopnia - 13 * Skala, "dashed", "W_L", "WANGA_KIESZEN", iSort, zRob.Split(','), 1, true, 1, NazwaProgramuCNC, "Wanga prawa");
                        await context.StrokeAsync();
                        await context.SetStrokeStyleAsync("black");
                        await context.SetLineWidthAsync(1);
                        await context.SetLineDashAsync(new float[] { });

                        // Console.WriteLine($"-------> currentX:{currentX} X: {X}");

                        //   currentX -= stepWidthTMP;
                        currentY -= WysokoscPodniesieniaStopnia * Skala;
                    }
                    else
                    {
                        // Ustaw kolor linii na niebieski
                        await context.SetStrokeStyleAsync("blue");
                        // Ustaw grubość linii (na przykład na 3 piksele)
                        await context.SetLineWidthAsync(3);

                        await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię
                        await DrawShapeStopinRysBok(context, currentX - ZachodzenieStopniZaSiebie * Skala, currentY, stepWidth + ZachodzenieStopniZaSiebie * Skala, gruboscStopnia);
                        await DrawShapeStopinRysBok(context, currentX - ((ZachodzenieStopniZaSiebie - 17) * Skala), currentY + 13 * Skala, stepWidth + ((ZachodzenieStopniZaSiebie - 17 * 2) * Skala),
                            gruboscStopnia - 13 * Skala, "dashed", "W_L", "WANGA_KIESZEN", iSort, zRob.Split(','), 1, true, 1, NazwaProgramuCNC);
                        await context.StrokeAsync();
                        await context.SetStrokeStyleAsync("black");
                        await context.SetLineWidthAsync(1);
                        await context.SetLineDashAsync(new float[] { });

                        currentX += stepWidthTMP;
                        currentY -= WysokoscPodniesieniaStopnia * Skala;
                    }

                }

                ClosePathAndAddFinalLineAsync();

                await DrawObrysZKatemPrawe(context, startX, startY + WysokoscPodniesieniaStopnia * Skala, currentY, DlugoscLiniiBiegu * Skala,
                    ((LiczbaPodniesienStopni + 1) * WysokoscPodniesieniaStopnia) * Skala, 90 + KatNachylenia, stepWidth + GlebokoscStopnia * Skala, currentX - ZachodzenieStopniZaSiebie * Skala - X);


                //Rysowanie schodów widok z boku KONIEC --------------------------------------------------------------------------------------------------------------------------
            }

            await context.SetFontAsync("16px Arial");
            await context.SetFillStyleAsync("red");

            // Wyświetlanie dodatkowych informacji
            // Draw text
            await context.FillTextAsync($"Poziome: {LiczbaPodniesienStopni} = ({System.Math.Round(LiczbaPodniesienStopni * GlebokoscStopnia + WydluzOstatniStopien, 0)})" +
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
                                           double katNachylenia, double stepWidth, double wartoscXOstatniegoSopopniaGornego)
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

            double liniaDol = 0;

            double liniaDolStart = (GlebokoscStopnia + liniaDol) * Math.Tan(katNachylenia * (Math.PI / 180));

            //  Console.WriteLine($"wartoscXOstatniegoSopopniaGornego: {wartoscXOstatniegoSopopniaGornego} - pobrana");

            string zRob = "Z21,Z2,";//Kolejne kroki frezowania 

            double wartXGornegoStopnia = (wartoscXOstatniegoSopopniaGornego * Math.Tan(KatNachylenia * (Math.PI / 180)));

            //Console.WriteLine($"----------------->1# wartXGornegoStopnia: {wartXGornegoStopnia}");

            double odsadzStopniaSuma = (OsadzenieOdDolu * Skala) / Math.Cos(KatNachylenia * (Math.PI / 180));

            //Console.WriteLine($"odsadzStopniaSuma: {odsadzStopniaSuma} + wartXGornegoStopnia {wartXGornegoStopnia} = {wartXGornegoStopnia + odsadzStopniaSuma}");

            wartXGornegoStopnia = wartXGornegoStopnia - odsadzStopniaSuma;

            liniaDolStart = liniaDolStart - (WysokoscPodniesieniaStopnia - GruboscStopnia);

            liniaDolStart = liniaDolStart / Math.Tan(katNachylenia * (Math.PI / 180));


            // Ustaw kolor na zielony i zwiększ grubość linii
            await context.SetStrokeStyleAsync("green");
            await context.SetLineWidthAsync(2);

            // Rozpoczynamy rysowanie nachylonego prostokąta
            await context.BeginPathAsync();
            ClearPathAndAddFinalLineAsync();
            // Punkt początkowy (lewy dolny róg)
            double leftBottomX = startX - liniaDolStart * Skala;
            double leftBottomY = startY;

            await context.MoveToAsync(leftBottomX, leftBottomY); // ---------------------------------------------------------------------------- ????????????????????????????
            AddLineWithPreviousPointAsync(leftBottomX, leftBottomY, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 5, true, 1, NazwaProgramuCNC, "Wanga lewa"); // Dodanie linii z lewego do prawego dolnego rogu

            // Linia pozioma (lewy dolny do prawy dolny)
            await context.LineToAsync(startX, startY);
            AddLineWithPreviousPointAsync(startX, startY, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 6, true, 1, NazwaProgramuCNC); // Dodanie linii pionowej


            // Linia pionowa (prawy dolny do prawego górnego)
            double rightBottomX = startX;
            double startYlP1 = liniaDol * Math.Tan(katNachylenia * (Math.PI / 180));

            startYlP1 -= OsadzenieOdGory / Math.Cos(katNachylenia * (Math.PI / 180));

            double rightBottomY = startY - ((WysokoscPodniesieniaStopnia - startYlP1) * Skala); //??????????????????????????????????????????????????????????????????????????????????????????????????

            await context.LineToAsync(rightBottomX, rightBottomY);
            AddLineWithPreviousPointAsync(rightBottomX, rightBottomY, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 7, true, 1, NazwaProgramuCNC); // Dodanie linii skośnej


            double liniaXGora = (GlebokoscStopnia + liniaDol) * Math.Tan(katNachylenia * (Math.PI / 180));

            liniaXGora = ((WysokoscPodniesieniaStopnia + wysokoscZaczepuY) - liniaXGora) / Math.Tan(katNachylenia * (Math.PI / 180));

            liniaXGora = dlugoscZaczepuX - liniaXGora;

            // **Skośna linia (od prawego górnego punktu do prawego górnego stopnia)**

            double rightUpperX = X + liniaXGora * Skala;
            double rightUpperY = YStartStopienGorny - GruboscStopnia * Skala;

            double xSzukaLiniaSkosnaGora = (((WysokoscCalkowita + wysokoscZaczepuY) - (WysokoscPodniesieniaStopnia - startYlP1)) * Math.Tan((90 - KatNachylenia) * (Math.PI / 180)));


            // Rysowanie skośnej linii do prawego górnego rogu ostatniego stopnia
            await context.LineToAsync(rightBottomX - xSzukaLiniaSkosnaGora * Skala, rightUpperY); //Linia równoległa do osi schodów w odległości ~20mm od krawędzi stopnia
            AddLineWithPreviousPointAsync(rightBottomX - xSzukaLiniaSkosnaGora * Skala, rightUpperY, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 8, true, 1, NazwaProgramuCNC); // Dodanie skośnej linii

            double endYFinal = YStartStopienGorny + ((WysokoscPodniesieniaStopnia + GruboscStopnia) * Skala);// rightUpperY + Math.Sqrt(Math.Pow(przeciwProstokatna, 2) - Math.Pow((WysokoscPodniesieniaStopnia * Skala) * Math.Cos(katNachylenia * (Math.PI / 180)), 2));

            //********************************************************************************************************************************************************

            // Obliczenie przesunięć wzdłuż osi X i Y dla wydłużenia
            double deltaXWydl = 2 * X * Math.Cos((katNachylenia + 90) * (Math.PI / 180));
            double deltaYWydl = 2 * Y * Math.Sin((katNachylenia + 90) * (Math.PI / 180));
            // Dodanie linii do wydłużonego końca
            double extendedUpperX = rightUpperX + deltaXWydl;
            double extendedUpperY = rightUpperY + deltaYWydl;

            //********************************************************************************************************************************************************

            // Zaczep górny
            double hookX1 = X - dlugoscZaczepuX * Skala;
            double hookY1 = rightUpperY;// + deltaYWydl;
            double hookX2 = hookX1;
            double hookY2 = hookY1 + wysokoscZaczepuY * Skala;
            double hookX3 = X;
            double hookY3 = hookY2;
            await context.LineToAsync(hookX1, hookY1);
            AddLineWithPreviousPointAsync(hookX1, hookY1, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 9, true, 1, NazwaProgramuCNC); // Dodanie linii zaczepu
            await context.LineToAsync(hookX2, hookY2);
            AddLineWithPreviousPointAsync(hookX2, hookY2, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 0, true, 1, NazwaProgramuCNC); // Dodanie pionowej linii zaczepu

            await context.LineToAsync(hookX3, hookY3);
            AddLineWithPreviousPointAsync(hookX3, hookY3, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 1, true, 1, NazwaProgramuCNC); // Dodanie poziomej linii zaczepu

            double endYFinalTMP = endYFinal - wartXGornegoStopnia;

            await context.LineToAsync(X, endYFinalTMP);
            AddLineWithPreviousPointAsync(X, endYFinalTMP, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 2, true, 1, NazwaProgramuCNC); // Dodanie końcowej pionowej linii z uwględnieniem odsunięcia

            // Console.WriteLine($"wartXGornegoStopnia: {wartXGornegoStopnia}");  
            double xRownolegle = ((WysokoscCalkowita - (WysokoscPodniesieniaStopnia + GruboscStopnia - wartXGornegoStopnia / Skala)) * Math.Tan((90 - KatNachylenia) * (Math.PI / 180))) * Skala;


            await context.LineToAsync(X + xRownolegle, leftBottomY);
            AddLineWithPreviousPointAsync(X + xRownolegle, leftBottomY, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 3, true, 1, NazwaProgramuCNC); // Dodanie linii poziomej


            // Zamknięcie ścieżki
            await context.ClosePathAsync();

            ClosePathAndAddFinalLineAsync("", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 4, true, 1, NazwaProgramuCNC);

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
            await context.RectAsync(X + DlugoscOtworu * Skala, hookY1 + wysZaczepuY * Skala, iloscWystajacych, 140 * Skala); // 140 teoretyczna grubość stropu
            await context.FillAsync(); // Wypełnia prostokąt kolorem

            // await context.BeginBatchAsync();
            await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię
            // Rysuj obrys dla tego prostokąta
            await context.SetStrokeStyleAsync("gray");
            await context.SetLineWidthAsync(1);
            await context.RectAsync(X + 50 * Skala - dlugoscZaczepuX * Skala, hookY1 + wysZaczepuY * Skala, (DlugoscOtworu + dlugoscZaczepuX - 50) * Skala, 140 * Skala);
            await context.StrokeAsync(); // Rysuje obrys

            await context.SetLineDashAsync(new float[] { }); // Ustaw przerywaną linię
                                                             //  await context.BeginBatchAsync();

            AddRectanglePoints(X + DlugoscOtworu * Skala, hookY1 + wysZaczepuY * Skala, iloscWystajacych, 140 * Skala);
            AddRectanglePoints(X + 50 * Skala - dlugoscZaczepuX * Skala, hookY1 + wysZaczepuY * Skala, (DlugoscOtworu + dlugoscZaczepuX - 50) * Skala, 140 * Skala, "dashed");

        }
        private async Task DrawObrysZKatemPrawe(Canvas2DContext context, double startX, double startY, double YStartStopienGorny, double szerokosc, double wysokosc,
                                           double katNachylenia, double stepWidth, double wartoscXOstatniegoSopopniaGornego)
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
            double endX = startX - deltaX;
            double endY = startY - deltaY;

            double liniaDol = 0;

            double liniaDolStart = (GlebokoscStopnia + liniaDol) * Math.Tan(katNachylenia * (Math.PI / 180));

            string zRob = "Z21,Z2,";//Kolejne kroki frezowania 

            double wartoscXOstatniegoSopopniaGornegoZmiennPom = ((DlugoscOtworu - 2 * GlebokoscStopnia) * Skala) + ((DlugoscNaWejsciu - DlugoscOtworu) * Skala) + stepWidth / 2;

            wartoscXOstatniegoSopopniaGornego = wartoscXOstatniegoSopopniaGornegoZmiennPom - wartoscXOstatniegoSopopniaGornego;

            //Console.WriteLine($"PRAWE - wartoscXOstatniegoSopopniaGornego: {wartoscXOstatniegoSopopniaGornego} - pobrana stepWidth: {stepWidth / 2}");

            double wartXGornegoStopnia = (wartoscXOstatniegoSopopniaGornego * Math.Tan(KatNachylenia * (Math.PI / 180)));

            //Console.WriteLine($"----------------->1# wartXGornegoStopnia: {wartXGornegoStopnia}");

            double odsadzStopniaSuma = (OsadzenieOdDolu * Skala) / Math.Cos(KatNachylenia * (Math.PI / 180));

            //Console.WriteLine($"odsadzStopniaSuma: {odsadzStopniaSuma} + wartXGornegoStopnia {wartXGornegoStopnia} = {wartXGornegoStopnia + odsadzStopniaSuma}");

            wartXGornegoStopnia = wartXGornegoStopnia - odsadzStopniaSuma;

            liniaDolStart = liniaDolStart - (WysokoscPodniesieniaStopnia - GruboscStopnia);

            liniaDolStart = liniaDolStart / Math.Tan(katNachylenia * (Math.PI / 180));


            // Ustaw kolor na zielony i zwiększ grubość linii
            await context.SetStrokeStyleAsync("green");
            await context.SetLineWidthAsync(2);

            // Rozpoczynamy rysowanie nachylonego prostokąta
            await context.BeginPathAsync();
            ClearPathAndAddFinalLineAsync();
            // Punkt początkowy (lewy dolny róg)
            double leftBottomX = startX + liniaDolStart * Skala;
            double leftBottomY = startY;

            await context.MoveToAsync(leftBottomX, leftBottomY); // ---------------------------------------------------------------------------- ????????????????????????????
            AddLineWithPreviousPointAsync(leftBottomX, leftBottomY, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 5, true, 1, NazwaProgramuCNC, "Wanga prawa"); // Dodanie linii z lewego do prawego dolnego rogu

            // Linia pozioma (lewy dolny do prawy dolny)
            await context.LineToAsync(startX, startY);
            AddLineWithPreviousPointAsync(startX, startY, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 6, true, 1, NazwaProgramuCNC); // Dodanie linii pionowej

            // Linia pionowa (prawy dolny do prawego górnego)
            double rightBottomX = startX;
            double startYlP1 = liniaDol * Math.Tan(katNachylenia * (Math.PI / 180));

            startYlP1 -= OsadzenieOdGory / Math.Cos(katNachylenia * (Math.PI / 180));

            double rightBottomY = startY - ((WysokoscPodniesieniaStopnia - startYlP1) * Skala); //??????????????????????????????????????????????????????????????????????????????????????????????????

            await context.LineToAsync(rightBottomX, rightBottomY);
            AddLineWithPreviousPointAsync(rightBottomX, rightBottomY, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 7, true, 1, NazwaProgramuCNC); // Dodanie linii skośnej

            double liniaXGora = (GlebokoscStopnia + liniaDol) * Math.Tan(katNachylenia * (Math.PI / 180));

            liniaXGora = ((WysokoscPodniesieniaStopnia + wysokoscZaczepuY) - liniaXGora) / Math.Tan(katNachylenia * (Math.PI / 180));

            liniaXGora = dlugoscZaczepuX - liniaXGora;

            // **Skośna linia (od prawego górnego punktu do prawego górnego stopnia)**

            double rightUpperX = X + liniaXGora * Skala;
            double rightUpperY = YStartStopienGorny - GruboscStopnia * Skala;

            double xSzukaLiniaSkosnaGora = (((WysokoscCalkowita + wysokoscZaczepuY) - (WysokoscPodniesieniaStopnia - startYlP1)) * Math.Tan((90 - KatNachylenia) * (Math.PI / 180)));

            // Rysowanie skośnej linii do prawego górnego rogu ostatniego stopnia
            await context.LineToAsync(rightBottomX + xSzukaLiniaSkosnaGora * Skala, rightUpperY); //Linia równoległa do osi schodów w odległości ~20mm od krawędzi stopnia
            AddLineWithPreviousPointAsync(rightBottomX + xSzukaLiniaSkosnaGora * Skala, rightUpperY, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 8, true, 1, NazwaProgramuCNC); // Dodanie skośnej linii

            double endYFinal = YStartStopienGorny + ((WysokoscPodniesieniaStopnia + GruboscStopnia) * Skala);// rightUpperY + Math.Sqrt(Math.Pow(przeciwProstokatna, 2) - Math.Pow((WysokoscPodniesieniaStopnia * Skala) * Math.Cos(katNachylenia * (Math.PI / 180)), 2));

            //********************************************************************************************************************************************************

            // Obliczenie przesunięć wzdłuż osi X i Y dla wydłużenia
            double deltaXWydl = 2 * X * Math.Cos((katNachylenia + 90) * (Math.PI / 180));
            double deltaYWydl = 2 * Y * Math.Sin((katNachylenia + 90) * (Math.PI / 180));
            // Dodanie linii do wydłużonego końca
            double extendedUpperX = rightUpperX + deltaXWydl;
            double extendedUpperY = rightUpperY + deltaYWydl;

            //********************************************************************************************************************************************************

            // Zaczep górny
            double hookX1 = X + DlugoscLiniiBiegu * Skala + dlugoscZaczepuX * Skala;
            double hookY1 = rightUpperY;// + deltaYWydl;
            double hookX2 = hookX1;
            double hookY2 = hookY1 + wysokoscZaczepuY * Skala;
            double hookX3 = X + DlugoscLiniiBiegu * Skala;
            double hookY3 = hookY2;
            await context.LineToAsync(hookX1, hookY1);
            AddLineWithPreviousPointAsync(hookX1, hookY1, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 9, true, 1, NazwaProgramuCNC); // Dodanie linii zaczepu

            await context.LineToAsync(hookX2, hookY2);
            AddLineWithPreviousPointAsync(hookX2, hookY2, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 0, true, 1, NazwaProgramuCNC); // Dodanie pionowej linii zaczepu

            await context.LineToAsync(hookX3, hookY3);
            AddLineWithPreviousPointAsync(hookX3, hookY3, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 1, true, 1, NazwaProgramuCNC); // Dodanie poziomej linii zaczepu

            double endYFinalTMP = endYFinal - wartXGornegoStopnia;

            await context.LineToAsync(hookX3, endYFinalTMP);
            AddLineWithPreviousPointAsync(hookX3, endYFinalTMP, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 2, true, 1, NazwaProgramuCNC); // Dodanie końcowej pionowej linii z uwględnieniem odsunięcia
                                                                                                                                               // Console.WriteLine($"wartXGornegoStopnia: {wartXGornegoStopnia}"); 
            double xRownolegle = ((WysokoscCalkowita - (WysokoscPodniesieniaStopnia + GruboscStopnia - wartXGornegoStopnia / Skala)) * Math.Tan((90 - KatNachylenia) * (Math.PI / 180))) * Skala;

            await context.LineToAsync(X + DlugoscLiniiBiegu * Skala - xRownolegle, leftBottomY);
            AddLineWithPreviousPointAsync(X + DlugoscLiniiBiegu * Skala - xRownolegle, leftBottomY, "", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 3, true, 1, NazwaProgramuCNC); // Dodanie linii poziomej

            // Zamknięcie ścieżki
            await context.ClosePathAsync();

            ClosePathAndAddFinalLineAsync("", "W_L", "WANGA_OBRYS", "1", zRob.Split(','), 4, true, 1, NazwaProgramuCNC);

            await context.StrokeAsync();

            await DrawObrysSufituPrawe(context, hookY1, stepWidth, wysokoscZaczepuY, dlugoscZaczepuX);
        }

        private async Task DrawObrysSufituPrawe(Canvas2DContext context, double hookY1, double stepWidth, double wysZaczepuY, double dlugoscZaczepuX)
        {

            double iloscWystajacych = (DlugoscLiniiBiegu - DlugoscOtworu) * Skala;

            //Console.WriteLine($"iloscWystajacych:{iloscWystajacych} = ({DlugoscLiniiBiegu} - {DlugoscOtworu})");  

            // Ustaw kolor wypełnienia tylko dla prostokąta sufitu
            await context.SetFillStyleAsync("gray");
            // Rozpoczynamy rysowanie nachylonego prostokąta
            await context.BeginPathAsync();
            // Rysuj i wypełnij tylko ten prostokąt
            await context.RectAsync(X, hookY1 + wysZaczepuY * Skala, iloscWystajacych, 140 * Skala); // 140 teoretyczna grubość stropu
            await context.FillAsync(); // Wypełnia prostokąt kolorem

            // await context.BeginBatchAsync();
            await context.SetLineDashAsync(new float[] { 5, 5 }); // Ustaw przerywaną linię
            // Rysuj obrys dla tego prostokąta
            await context.SetStrokeStyleAsync("gray");
            await context.SetLineWidthAsync(1);
            await context.RectAsync(X, hookY1 + wysZaczepuY * Skala, (DlugoscLiniiBiegu + dlugoscZaczepuX - 50) * Skala, 140 * Skala);
            await context.StrokeAsync(); // Rysuje obrys

            await context.SetLineDashAsync(new float[] { }); // Ustaw przerywaną linię
                                                             //  await context.BeginBatchAsync();

            AddRectanglePoints(X + DlugoscOtworu * Skala, hookY1 + wysZaczepuY * Skala, iloscWystajacych, 140 * Skala);
            AddRectanglePoints(X + 50 * Skala - dlugoscZaczepuX * Skala, hookY1 + wysZaczepuY * Skala, (DlugoscOtworu + dlugoscZaczepuX - 50) * Skala, 140 * Skala, "dashed");

        }

        private async Task DrawShapeStopinRysBok(Canvas2DContext context, double offsetX, double offsetY, double szerStopnia, double gruboscStopnia, string typeLine = "", string fileNCName = "", string nameMacro = "",
            string idOBJ = "", string[]? zRobocze = null, double idRuchNarzWObj = 0, bool addGcode = false, int iloscSztuk = 0, string nazwaProgramu = "", string nazwaElementu = "")
        {
            await context.BeginPathAsync();

            // Ustaw kolor linii na czerwony
            await context.SetStrokeStyleAsync("blue");

            // Ustaw grubość linii (na przykład na 3 piksele)
            await context.SetLineWidthAsync(2);

            // Rysowanie zewnętrznego prostokąta
            await context.RectAsync(offsetX, offsetY, szerStopnia, gruboscStopnia); // #OBRYS SCHODOW

            // Dodanie punktów prostokąta do listy
            AddRectanglePoints(offsetX, offsetY, szerStopnia, gruboscStopnia, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramu, nazwaElementu);

            // Console.WriteLine($"DrawShapeStopinRysBok fileNCName: {fileNCName} nameMacro: {nameMacro} idOBJ: {idOBJ}");  

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
        private void AddRectanglePoints(double x, double y, double width, double height, string typeLine = "", string fileNCName = "", string nameMacro = "", string idOBJ = "", string[]? zRobocze = null, double idRuchNarzWObj = 0,
            bool addGcode = false, int iloscSztuk = 0, string nazwaProgramy = "", string nazwaElementu = "")
        {
            if (XLinePoint == null) return;

            XLinePoint.Add(new LinePoint(x, y, x + width, y, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));
            XLinePoint.Add(new LinePoint(x + width, y, x + width, y + height, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));
            XLinePoint.Add(new LinePoint(x + width, y + height, x, y + height, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));
            XLinePoint.Add(new LinePoint(x, y + height, x, y, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy));
        }

        private void AddPointsStopienObrys(double x, double y, double width, double height, string typeLine = "", string fileNCName = "", string nameMacro = "", string idOBJ = "",
            string[]? zRobocze = null, double idRuchNarzWObj = 0, bool addGcode = false, string ZPodFrez = "", int iloscSztuk = 0, string nazwaProgramy = "", string nazwaElementu = "")
        {
            if (XLinePoint == null) return;

            //Dodanie OBRYSU FREZOWANIA ZAKONCZEN STRONA 1
            AddRectanglePoints(x + 17 - 5, y - 20, width - 2 * (17 - 5), 20, typeLine, fileNCName, "A" + nameMacro, "F1", ZPodFrez.Split(','), idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu);

            //Dodanie OBRYSU FREZOWANIA ZAKONCZEN STRONA 2
            AddRectanglePoints(x + 17 - 5, y + height, width - 2 * (17 - 5), 20, typeLine, fileNCName, "A" + nameMacro, "F2", ZPodFrez.Split(','), idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu);

            XLinePoint.Add(new LinePoint(x, y, x + 17, y, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu)); //#1
            XLinePoint.Add(new LinePoint(x + 17, y, x + 17, y - 20, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));//#2
            XLinePoint.Add(new LinePoint(x + 17, y - 20, x + width - 17, y - 20, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));//#3
            XLinePoint.Add(new LinePoint(x + width - 17, y - 20, x + width - 17, y, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));//#4
            XLinePoint.Add(new LinePoint(x + width - 17, y, x + width, y, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));//#5
            XLinePoint.Add(new LinePoint(x + width, y, x + width, y + height, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));//#6
            XLinePoint.Add(new LinePoint(x + width, y + height, x + width - 17, y + height, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));//#7
            XLinePoint.Add(new LinePoint(x + width - 17, y + height, x + width - 17, y + height + 20, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));//#8
            XLinePoint.Add(new LinePoint(x + width - 17, y + height + 20, x + 17, y + height + 20, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));//#9 ???
            XLinePoint.Add(new LinePoint(x + 17 + 17, y + height + 20, x + 17, y + height + 20, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));//#10
            XLinePoint.Add(new LinePoint(x + 17, y + height + 20, x + 17, y + height, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));//#11
            XLinePoint.Add(new LinePoint(x + 17, y + height, x, y + height, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));//#12
            XLinePoint.Add(new LinePoint(x, y + height, x, y, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));//#13

        }

        private void AddLinePoints(double x1, double y1, double x2, double y2, string typeLine = "", string fileNCName = "", string nameMacro = "", string idOBJ = "", string[]? zRobocze = null, double idRuchNarzWObj = 0,
            bool addGcode = false, int iloscSztuk = 0, string nazwaProgramy = "", string nazwaElementu = "")
        {
            if (XLinePoint == null) return;

            XLinePoint.Add(new LinePoint(x1, y1, x2, y2, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu));
        }

        // Zmienna do śledzenia punktu poprzedniego
        double? previousX = null;
        double? previousY = null;

        // Zmienna do śledzenia pierwszego punktu ścieżki
        double? firstX = null;
        double? firstY = null;

        // Funkcja do dodawania linii z użyciem poprzedniego punktu
        private void AddLineWithPreviousPointAsync(double x, double y, string typeLine = "", string fileNCName = "", string nameMacro = "", string idOBJ = "", string[]? zRobocze = null, double idRuchNarzWObj = 0,
            bool addGcode = false, int iloscSztuk = 0, string nazwaProgramy = "", string nazwaElementu = "")
        {
            // Sprawdzamy, czy istnieje poprzedni punkt, aby utworzyć linię
            if (previousX.HasValue && previousY.HasValue)
            {
                // Dodajemy linię do XLinePoint od poprzedniego punktu do bieżącego
                AddLinePoints(previousX.Value, previousY.Value, x, y, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu);
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
        private void ClosePathAndAddFinalLineAsync(string typeLine = "", string fileNCName = "", string nameMacro = "", string idOBJ = "", string[]? zRobocze = null, double idRuchNarzWObj = 0,
            bool addGcode = false, int iloscSztuk = 0, string nazwaProgramy = "", string nazwaElementu = "")
        {
            if (previousX.HasValue && previousY.HasValue && firstX.HasValue && firstY.HasValue)
            {
                // Dodajemy ostatnią linię od ostatniego punktu do pierwszego
                AddLinePoints(previousX.Value, previousY.Value, firstX.Value, firstY.Value, typeLine, fileNCName, nameMacro, idOBJ, zRobocze, idRuchNarzWObj, addGcode, iloscSztuk, nazwaProgramy, nazwaElementu);

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

        public async Task SaveToDxfAsync(string nazPlikSchody)
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
                    await _jsRuntime.InvokeVoidAsync("downloadFileDXF", $"{nazPlikSchody}_{Lewe}.dxf", base64String);
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

        public async Task SaveToDxfAndGCodeAsync(string kierunekSch, HttpClient httpClient)
        {
            CGCode gcodeGenerator = new CGCode(); // Tworzenie instancji klasy

            double KatObrotuStopien = 90;

            double KatObrotuWanga = -KatNachylenia;
            double przesuniecieYOdbicia = 900; // wartość przesunięcia w osi Y dla lustrzanego odbicia

            if (kierunekSch == "p")
            {
                KatObrotuWanga = KatNachylenia + 180;
                przesuniecieYOdbicia = -1900;
            }

            try
            {
                Console.WriteLine($"Rozpoczęcie zapisu pliku DXF oraz generowania GCode. Kąty obrotu: {KatObrotuStopien} oraz {KatObrotuWanga} stopni.");

                // Konwersja kąta na radiany (dla obliczeń matematycznych)
                double KatObrotuStopientRadians = KatObrotuStopien * (Math.PI / 180);
                double KatKatObrotuWanga = KatObrotuWanga * (Math.PI / 180);

                // Inicjalizacja dokumentu DXF
                var supportFolders = new SupportFolders();
                var drawingVariables = new HeaderVariables();
                DxfDocument dxf = new(drawingVariables, supportFolders);

                // Lista do przechowywania obróconych linii
                List<LinePoint> rotatedLines = new();

                // Przekształcenie i zapis linii do DXF
                if (XLinePoint != null)
                {
                    foreach (var linePoint in XLinePoint)
                    {
                        if (linePoint.addGcode)
                        {
                            double katRadians = KatObrotuStopientRadians;

                            if (linePoint.nameMacro.StartsWith("wanga", StringComparison.OrdinalIgnoreCase))
                            {
                                katRadians = KatKatObrotuWanga;
                            }

                            // Obrót punktów według podanego kąta
                            var rotatedStart = gcodeGenerator.RotatePoint(linePoint.X1, linePoint.Y1, katRadians);
                            var rotatedEnd = gcodeGenerator.RotatePoint(linePoint.X2, linePoint.Y2, katRadians);

                            // Tworzenie obróconej linii
                            netDxf.Entities.Line dxfLine = new netDxf.Entities.Line(
                                new netDxf.Vector2(rotatedStart.X, rotatedStart.Y),
                                new netDxf.Vector2(rotatedEnd.X, rotatedEnd.Y)
                            );

                            // Ustawienie typu linii
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

                            // Zapisanie obróconej linii do nowej listy (dla GCode)
                            rotatedLines.Add(new LinePoint(rotatedStart.X, rotatedStart.Y, rotatedEnd.X, rotatedEnd.Y, linePoint.typeLine, linePoint.fileNCName, linePoint.nameMacro, linePoint.idOBJ,
                                linePoint.zRobocze, linePoint.idRuchNarzWObj, linePoint.addGcode, linePoint.IloscSztuk, linePoint.NazwaProgramu, linePoint.NazwaElementu));

                            // Sprawdzenie warunku dla linii z "wanga" i tworzenie odbicia lustrzanego
                            if (linePoint.nameMacro.StartsWith("wanga", StringComparison.OrdinalIgnoreCase))
                            {
                                var mirroredStart = gcodeGenerator.MirrorPointHorizontally(rotatedStart.X, rotatedStart.Y, przesuniecieYOdbicia);
                                var mirroredEnd = gcodeGenerator.MirrorPointHorizontally(rotatedEnd.X, rotatedEnd.Y, przesuniecieYOdbicia);

                                // Tworzenie linii odbicia lustrzanego
                                netDxf.Entities.Line mirroredLine = new netDxf.Entities.Line(
                                    new netDxf.Vector2(mirroredStart.X, mirroredStart.Y),
                                    new netDxf.Vector2(mirroredEnd.X, mirroredEnd.Y)
                                );

                                mirroredLine.Linetype = dxfLine.Linetype;

                                // Dodanie odbicia lustrzanego do dokumentu DXF
                                dxf.Entities.Add(mirroredLine);

                                // Zapisanie odbitej linii do listy (dla GCode)
                                rotatedLines.Add(new LinePoint(mirroredStart.X, mirroredStart.Y, mirroredEnd.X, mirroredEnd.Y, linePoint.typeLine, "M_" + linePoint.fileNCName, linePoint.nameMacro, linePoint.idOBJ,
                                    linePoint.zRobocze, linePoint.idRuchNarzWObj, linePoint.addGcode, linePoint.IloscSztuk, linePoint.NazwaProgramu, linePoint.NazwaElementu + "Wanga druga strona"));
                            }
                        }
                    }
                }

                // Korekta współrzędnych w DXF (jeśli wymagana)
                SetNegativeCoordinates(dxf);

                // Ustawienia stylu DXF i dopasowanie widoku
                UpdateDimensionStyleTextHeightAndFitView(dxf, "Standard", 35, "arial.ttf");

                Console.WriteLine($"Ustawienie stylu DXF zakończone.");

                // Zapis pliku DXF
                using (MemoryStream stream = new MemoryStream())
                {
                    dxf.Save(stream);
                    Console.WriteLine($"Rozmiar pliku DXF w bajtach: {stream.Length}");

                    string base64String = Convert.ToBase64String(stream.ToArray());
                    await _jsRuntime.InvokeVoidAsync("downloadFileDXF", $"schody_proste_{Lewe}_nachylenie_{Math.Abs(Math.Round(KatNachylenia, 3))}.dxf", base64String);
                }

                Console.WriteLine($"Plik DXF został wygenerowany. Ilość Wektorów: {rotatedLines.Count}");

                await GenerateCNCFilesAsync(rotatedLines, gcodeGenerator, _jsRuntime, httpClient);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd: {ex.Message} / {ex.StackTrace}");
            }
        }

        private async Task<PdfFont> LoadFontAsync(HttpClient httpClient)
        {
            // Ścieżka do czcionki w `wwwroot/fonts`
            string fontPath = "fonts/arial.ttf";

            // Pobieranie czcionki jako strumienia
            using var responseStream = await httpClient.GetStreamAsync(fontPath);
            using var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream);

            // Konwersja do tablicy bajtów
            var fontBytes = memoryStream.ToArray();

            // Tworzenie czcionki
            return PdfFontFactory.CreateFont(fontBytes, PdfEncodings.IDENTITY_H);
        }
        public async Task GenerateCNCFilesAsync(List<LinePoint> linePoints, CGCode gcodeGenerator, IJSRuntime jsRuntime, HttpClient httpClient)
        {
            try
            {
                // Grupowanie linii według fileNCName
                var groupedLines = linePoints
                    .Where(lp => !string.IsNullOrEmpty(lp.fileNCName)) // Ignoruj wpisy bez fileNCName
                    .GroupBy(lp => lp.fileNCName);

                Console.WriteLine($"Powstanie programów. Ilość groupedLines: {groupedLines.Count()}");

                foreach (var group in groupedLines)
                {
                    string fileName = group.Key; // Nazwa pliku (fileNCName)
                    if (group != null)
                    {
                        fileName = $"{group.First().NazwaProgramu}_{group.First().fileNCName}";
                    }

                    var rotatedLines = group.Select(lp =>
                    {
                        double angleRadians = 0; // tutaj oblicz kąt obrotu w radianach, jeśli jest wymagany

                        var rotatedStart = gcodeGenerator.RotatePoint(lp.X1, lp.Y1, angleRadians);
                        var rotatedEnd = gcodeGenerator.RotatePoint(lp.X2, lp.Y2, angleRadians);
                        return new LinePoint(rotatedStart.X, rotatedStart.Y, rotatedEnd.X, rotatedEnd.Y, lp.typeLine, lp.fileNCName, lp.nameMacro, lp.idOBJ, lp.zRobocze, lp.idRuchNarzWObj, lp.addGcode, lp.IloscSztuk, lp.NazwaProgramu, lp.NazwaElementu);
                    }).ToList();

                    // Generowanie G-Code dla aktualnej grupy
                    string gcode = gcodeGenerator.GenerateGCode(rotatedLines);

                    // Konwersja do Base64
                    string gcodeBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(gcode));

                    // Pobranie pliku
                    await jsRuntime.InvokeVoidAsync("downloadFileGCode", $"{fileName}.p", gcodeBase64);
                }

                // Generowanie raportu PDF
                await GeneratePDFReportAsync(linePoints, jsRuntime, httpClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas generowania plików CNC: {ex.Message}");
            }
        }

        private double currentYPosition = 0; // Początkowa pozycja generowania rysunków

        public async Task GeneratePDFReportAsync(List<LinePoint> linePoints, IJSRuntime jsRuntime, HttpClient httpClient)
        {
            currentYPosition = 750; // Ustaw początkową pozycję poniżej nagłówka

            Console.WriteLine($"Rozpoczynam generowania raportu PDF. [currentYPosition: {currentYPosition}]");

            try
            {
                if (linePoints == null || linePoints.Count == 0)
                {
                    Console.WriteLine("Brak danych wejściowych do wygenerowania raportu PDF.");
                    return;
                }

                using (var stream = new MemoryStream())
                {
                    Console.WriteLine("Strumień PDF utworzony.");

                    using (var pdfWriter = new PdfWriter(stream))
                    using (var pdfDocument = new PdfDocument(pdfWriter))
                    {
                        Console.WriteLine("Dokument PDF utworzony.");

                        var document = new Document(pdfDocument);
                        document.SetMargins(20, 20, 20, 20);

                        // Ładowanie czcionki obsługującej polskie znaki
                        var pdfFont = await LoadFontAsync(httpClient);

                        Console.WriteLine("Dodanie nagłówka dokumentu.");

                        document.Add(new Paragraph($"Materiały do wykonania schodów: {DateTime.Now.ToShortDateString()} Wysokość:{WysokoscCalkowita} Długość na wejściu:{DlugoscNaWejsciu} Szerokość całkowita:{SzerokoscBieguSchodow + 40 + 40}")
                            .SetFont(pdfFont)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetFontSize(12)
                            .SetBold());

                        // Grupowanie danych
                        var groupedLines = linePoints
                            .Where(lp => !string.IsNullOrEmpty(lp.fileNCName)) // Ignoruj elementy bez fileNCName
                            .GroupBy(lp => lp.fileNCName);
                        int idObj = 0;
                        foreach (var group in groupedLines)
                        {
                            string NazwaProgramuCNC = $"{group.First().NazwaProgramu}_{group.First().fileNCName}";

                            string onlyFileName = group
                                .Where(x => !string.IsNullOrEmpty(x.fileNCName))
                                .Select(x => x.fileNCName)
                                .FirstOrDefault() ?? string.Empty;

                            string NazwaElementu = group
                                .Where(x => !string.IsNullOrEmpty(x.NazwaElementu))
                                .Select(x => x.NazwaElementu)
                                .FirstOrDefault() ?? string.Empty;

                            AddGroupToDocument(document, group, group.Max(lp => lp.IloscSztuk), NazwaProgramuCNC, NazwaElementu, pdfFont, linePoints, onlyFileName, idObj++);
                        }

                        document.Close();
                    }

                    string pdfBase64 = Convert.ToBase64String(stream.ToArray());
                    await jsRuntime.InvokeVoidAsync("downloadFile", $"Raport_CNC.pdf", "application/pdf", pdfBase64);
                    Console.WriteLine("Plik PDF został wygenerowany i przesłany do przeglądarki.");
                }
            }
            catch (PdfException pdfEx)
            {
                Console.WriteLine($"Błąd PDF: {pdfEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Nieoczekiwany błąd podczas generowania pliku PDF: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private int intDrwHeight = 0;
        private async void AddGroupToDocument(Document document, IGrouping<string, LinePoint> group, int countEL, string programCNCName, string nazwaElementu, PdfFont pdfFont, List<LinePoint> linePoints, 
            string onlyFileName, int idObj)
        {
            string fileName = group.Key;

            document.Add(new Paragraph("").SetMarginTop(intDrwHeight)); // Odstęp przed tabelą
            // Dodanie nagłówka sekcji
            document.Add(new Paragraph($"Nazwa Programu: {programCNCName}")
                .SetFont(pdfFont)
                .SetBold()
                .SetFontSize(14)
                .SetMarginTop(10));
            Console.WriteLine($"1. ----------> Dodano sekcję dla grupy: {programCNCName} intDrwHeight: {intDrwHeight} / currentYPosition: {currentYPosition}");

            // Tabela wymiarów z równymi szerokościami kolumn
            var table = new Table(UnitValue.CreatePercentArray(5)).UseAllAvailableWidth();

            // Dodanie nagłówków tabeli
            table.AddHeaderCell(new Cell().Add(new Paragraph("Nazwa elementu").SetFont(pdfFont).SetBold()).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Długość").SetFont(pdfFont).SetBold()).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Szerokość").SetFont(pdfFont).SetBold()).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Grubość").SetFont(pdfFont).SetBold()).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Ilość sztuk").SetFont(pdfFont).SetBold()).SetBackgroundColor(ColorConstants.LIGHT_GRAY));

            // Obliczenie gabarytów elementu
            double minX = group.Min(line => Math.Min(line.X1, line.X2));
            double minY = group.Min(line => Math.Min(line.Y1, line.Y2));
            double maxX = group.Max(line => Math.Max(line.X1, line.X2));
            double maxY = group.Max(line => Math.Max(line.Y1, line.Y2));

            double gabX = Math.Ceiling(maxX - minX); // Długość
            double gabY = Math.Ceiling(maxY - minY); // Szerokość

            // Dodanie danych do tabeli
            table.AddCell(new Cell().Add(new Paragraph(nazwaElementu).SetFont(pdfFont)));  // Nazwa elementu
            table.AddCell(new Cell().Add(new Paragraph(gabX.ToString("F0")).SetFont(pdfFont))); // Długość
            table.AddCell(new Cell().Add(new Paragraph(gabY.ToString("F0")).SetFont(pdfFont))); // Szerokość
            table.AddCell(new Cell().Add(new Paragraph("40").SetFont(pdfFont)));   // Grubość (przykładowa wartość)
            table.AddCell(new Cell().Add(new Paragraph(countEL.ToString()).SetFont(pdfFont)));  // Ilość sztuk

            // Dodanie tabeli do dokumentu

            document.Add(table);
          
            // Rysowanie schodów dla grupy na tej samej stronie
            var drawingHeight = DrawLinesForGroup(document, group.ToList(), pdfFont, onlyFileName, idObj);
            intDrwHeight = (int)drawingHeight;
            // Dodanie odstępu poniżej rysunku
          //  currentYPosition -= 20; // Dodatkowy odstęp

            Console.WriteLine($"Tabela wymiarów dodana dla grupy: {fileName}");

            await Task.CompletedTask;
        }
        private double DrawLinesForGroup(Document document, List<LinePoint> linePoints, PdfFont pdfFont, string fileCNCName, int idObj)
        {
            var pdfDocument = document.GetPdfDocument();
            var currentPage = pdfDocument.GetLastPage();
            var pageSize = currentPage.GetPageSize();

            // Filtrowanie linii spełniających warunek
            var filteredLines = linePoints.Where(line => line.fileNCName == fileCNCName).ToList();

            if (!filteredLines.Any())
            {
                Console.WriteLine($"Brak linii do rysowania dla fileCNCName: {fileCNCName}");
                return 0;
            }

            // Przesunięcie linii do początku układu współrzędnych
            CGCode cgCodeInstance = new CGCode();
            var shiftedLines = cgCodeInstance.ShiftLinesToOrigin(filteredLines);

            double margin = 150; // Marginesy
            double availableWidth = pageSize.GetWidth() - 2 * margin;
            double availableHeight = currentYPosition - margin; // Dostępna przestrzeń (uwzględnia marginesy i odstęp pod rysunkiem)

            if (availableHeight <= 0)
            {
                Console.WriteLine("Brak miejsca na stronie. Rysunek zostanie pominięty.");
                return 0;
            }

            // Obliczanie zakresów współrzędnych w danych
            double minX = shiftedLines.Min(line => Math.Min(line.X1, line.X2));
            double minY = shiftedLines.Min(line => Math.Min(line.Y1, line.Y2));
            double maxX = shiftedLines.Max(line => Math.Max(line.X1, line.X2));
            double maxY = shiftedLines.Max(line => Math.Max(line.Y1, line.Y2));

            double rangeX = maxX - minX;
            double rangeY = maxY - minY;

            if (rangeX == 0 || rangeY == 0)
            {
                Console.WriteLine($"Nieprawidłowy zakres współrzędnych dla programu: {fileCNCName}");
                return 0;
            }

            // Obliczanie skali, aby dopasować rysunek do dostępnego obszaru
            double scaleX = availableWidth / rangeX;
            double scaleY = availableHeight / rangeY;
            double scale = Math.Min(scaleX, scaleY);

            intDrwHeight = (int)(rangeY * scale);

              Console.WriteLine($"2. -----> intDrwHeight: {intDrwHeight} / xcurrentYPosition: {currentYPosition} idObj: {idObj}");
            // Przesunięcie rysunku w układzie współrzędnych
            double offsetX = margin - minX * scale;

            double offsetY = 0;// currentYPosition - rangeY * scale - 60;// - intDrwHeight; // Dodano przesunięcie o przesunRysunek jednostek pomiędzy rysunkami

            if (idObj == 0)
            {
                offsetY = currentYPosition - 5 - rangeY * scale - 60;// - intDrwHeight; // Dodano przesunięcie o przesunRysunek jednostek pomiędzy rysunkami
            }
            else if (idObj == 1) 
            {
                offsetY = currentYPosition - 100 - rangeY * scale - 60;// - intDrwHeight; // Dodano przesunięcie o przesunRysunek jednostek pomiędzy rysunkami
            }
            else if (idObj == 2)
            {
                offsetY = currentYPosition - 180 - rangeY * scale - 60;// - intDrwHeight; // Dodano przesunięcie o przesunRysunek jednostek pomiędzy rysunkami
            }
            else if (idObj == 3)
            {
                offsetY = currentYPosition - 280 - rangeY * scale - 60;// - intDrwHeight; // Dodano przesunięcie o przesunRysunek jednostek pomiędzy rysunkami
            }
            else if (idObj == 4)
            {
                offsetY = currentYPosition - 370 - rangeY * scale - 60;// - intDrwHeight; // Dodano przesunięcie o przesunRysunek jednostek pomiędzy rysunkami
            }

            // Ustawienia rysowania na istniejącej stronie
            var pdfCanvas = new PdfCanvas(currentPage);
            pdfCanvas.SetLineWidth(1f);
            pdfCanvas.SetStrokeColor(ColorConstants.BLACK);

            // Rysowanie linii
            foreach (var line in shiftedLines)
            {
                double x1 = offsetX + line.X1 * scale;
                double y1 = offsetY + line.Y1 * scale;
                double x2 = offsetX  + line.X2 * scale;
                double y2 = offsetY + line.Y2 * scale;

                pdfCanvas.MoveTo((float)x1, (float)y1);
                pdfCanvas.LineTo((float)x2, (float)y2);
            }

            pdfCanvas.Stroke();

            // Dodanie opisu rysunku poniżej
            var canvas = new Canvas(pdfCanvas, pageSize);
            canvas.SetFont(pdfFont);
            canvas.Add(new Paragraph($"Obiekt: {fileCNCName}")
                .SetFont(pdfFont)
                .SetFontSize(10)
                .SetBold()
                .SetFixedPosition((float)margin, (float)(offsetY - 20), (float)(pageSize.GetWidth() - 2 * margin)));
            canvas.Close();

            // Obliczenie wysokości rysunku i opisu
            double totalHeight = rangeY * scale + 30; // Wysokość rysunku + odstęp + przesunięcie

            // Aktualizacja pozycji Y dla kolejnego rysunku
            currentYPosition -= totalHeight;
            Console.WriteLine($"currentYPosition: {currentYPosition}");
            return totalHeight;
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
            var filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"schody_proste_{Lewe}.stl");

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
