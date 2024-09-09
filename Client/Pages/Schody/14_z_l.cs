using Blazor.Extensions.Canvas.Canvas2D;
using System.Linq.Expressions;
using GEORGE.Client.Pages.Schody;

namespace GEORGE.Client.Pages.Schody
{
    public abstract partial class C14_z_l
    {
        public abstract Task DrawAsync(Canvas2DContext context);
        public abstract Task<List<Point>> ReturnPoints();
        public abstract Task<List<LinePoint>> ReturnLinePoints();
    }

    //---------------------------------------------------------------- PIÓRO -------------------------------------------------------------------------------------------
    public class CSchody : C14_z_l
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

            double delatY = 0;//((WysSamegoSkrzydla - 50) / (IloscPrzeszkelen + 1)) * Skala;

            double delatX = 0;// X + (SzerDrzwi / 2 - WielkoscPrzeszklenia / 2) * Skala;
            double delatYKrok = 0;


            await DrawShapeObrys(context, X, Y);

            await DrawTextAsync(context, X + (100) * Skala, Y - 45, $"Informacja: {Opis}");
      
            for (int i = 0; i < LiczbaPodniesienStopni; i++)
            {

                delatYKrok += delatY;
                await DrawShapeStopnie(context, delatX, delatYKrok, Y);
                //Console.WriteLine(delatYKrok);
            }

            // Draw the mirrored shape
            // Translate the context to the position where the mirrored shape will be drawn
            await context.TranslateAsync(0, 0);

            // Scale by -1 on the X axis to mirror the shape
            await context.ScaleAsync(-1, 1);

            await context.RestoreAsync();

        }

        private async Task DrawShapeObrys(Canvas2DContext context, double offsetX, double offsetY)
        {

            await context.BeginPathAsync();

            // Draw outer rectangle
            await context.RectAsync(offsetX, offsetY, DlugoscOtworu * Skala, SzerokoscOtworu * Skala); // #OBRYS SCHODOW

            AddRectanglePoints(offsetX, offsetY, DlugoscOtworu * Skala, SzerokoscOtworu * Skala);

            //   await context.ClosePathAsync();

            await context.StrokeAsync();
        }

        private async Task DrawShapeStopnie(Canvas2DContext context, double offsetX, double offsetY, double Y)
        {

            await context.BeginPathAsync();

            // Draw outer rectangle
            await context.RectAsync(offsetX - ((10 + Wyspiora / 2) * Skala), offsetY + Y - ((SzerRamSk / 2 * Skala) + (Wyspiora / 2 + 10) * Skala), (WielkoscPrzeszklenia + 20) * Skala, (WielkoscPrzeszklenia + 20) * Skala); // #Ramka przeszklenia ZEW
            await context.RectAsync(offsetX - (Wyspiora / 2 * Skala), offsetY + Y - (SzerRamSk / 2 * Skala + (Wyspiora / 2 * Skala)), WielkoscPrzeszklenia * Skala, WielkoscPrzeszklenia * Skala);  // #Ramka przeszklenia WEW
                                                                                                                                                                                                    //   await context.ClosePathAsync();
            AddRectanglePoints(offsetX - ((10 + Wyspiora / 2) * Skala), offsetY + Y - ((SzerRamSk / 2 * Skala) + (Wyspiora / 2 + 10) * Skala), (WielkoscPrzeszklenia + 20) * Skala, (WielkoscPrzeszklenia + 20) * Skala); // #Ramka przeszklenia ZEW
            AddRectanglePoints(offsetX - (Wyspiora / 2 * Skala), offsetY + Y - (SzerRamSk / 2 * Skala + (Wyspiora / 2 * Skala)), WielkoscPrzeszklenia * Skala, WielkoscPrzeszklenia * Skala);  // #Ramka przeszklenia WEW

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
            //Xpoints.Add(new Point(x, y)); // Top-left corner
            //Xpoints.Add(new Point(x + width, y)); // Top-right corner
            //Xpoints.Add(new Point(x + width, y + height)); // Bottom-right corner
            //Xpoints.Add(new Point(x, y + height)); // Bottom-left corner
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
