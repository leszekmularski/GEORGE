using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    // 🖌️ KLASA LINII
    public class XLineShape : IShapeDC
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public string NazwaObj { get; set; } = "Linia";
        public bool RuchomySlupek { get; set; } = false;
        public bool StalySlupek { get; set; } = false;
        public bool PionPoziom { get; set; } = false; //Podział słupek stały lub ślemię
        public bool DualRama { get; set; } = false;

        private double _scaleFactor = 1.0; // Skalowanie
        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }
        public bool GenerowaneZRamy { get; set; } = false; // Flaga do generowania z ramy
        public List<XPoint> Points { get; set; }
        public List<XPoint> GetPoints() => Points;

        public XLineShape(double x1, double y1, double x2, double y2, double scaleFactor, string nazwaObj, bool ruchomySlupek = false, bool pionPoziom = false, 
            bool dualRama = false, bool generowaneZRamy = false, bool stalySlupek = false)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            _scaleFactor = scaleFactor;
            NazwaObj = nazwaObj;
            RuchomySlupek = ruchomySlupek;
            PionPoziom = pionPoziom;
            DualRama = dualRama;
            GenerowaneZRamy = generowaneZRamy;
            StalySlupek = stalySlupek;

            Console.WriteLine($"Utworzono XLineShape: ({X1}, {Y1}) to ({X2}, {Y2}), RuchomySlupek: {RuchomySlupek}, PionPoziom: {PionPoziom}, DualRama: {DualRama}, StalySlupek: {StalySlupek}");

            // Wymuszenie pionowej linii
            if (RuchomySlupek)
            {
                X2 = X1;
            }

            if (PionPoziom)
            {
                if (X1 != X2)
                {
                    Y2 = Y1;
                }
                else
                {
                    X2 = X1;
                }
            }
        }
        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 2)
                return;

            Points = newPoints;

            // Aktualizujemy współrzędne linii na podstawie punktów
            X1 = Points[0].X;
            Y1 = Points[0].Y;
            X2 = Points[1].X;
            Y2 = Points[1].Y;

            // Obliczamy szerokość i wysokość
            Szerokosc = Math.Abs(X2 - X1);
            Wysokosc = Math.Abs(Y2 - Y1);

            // Wymuszamy pionową linię jeśli RuchomySlupek jest aktywny
            if (RuchomySlupek)
            {
                X2 = X1;
            }

            // Wymuszamy linię pionową lub poziomą jeśli PionPoziom jest aktywny
            if (PionPoziom)
            {
                if (X1 != X2)
                {
                    Y2 = Y1; // Linia pozioma
                }
                else
                {
                    X2 = X1; // Linia pionowa
                }
            }
        }
        public IShapeDC Clone()
        {
            return new XLineShape(X1, Y1, X2, Y2, _scaleFactor, NazwaObj, RuchomySlupek, PionPoziom, DualRama, false, StalySlupek);
        }

        public async Task Draw(Canvas2DContext ctx)
        {
            if (RuchomySlupek)
            {
                X2 = X1; // Ustawienie X2 równego X1, aby wymusić pionową linię
                await ctx.SetStrokeStyleAsync("red");
            }
            else
            {
                if (DualRama)
                {
                    await ctx.SetStrokeStyleAsync("orange");
                }
                else
                {
                    await ctx.SetStrokeStyleAsync("green");
                }
            }

            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(X1, Y1);
            await ctx.LineToAsync(X2, Y2);
            await ctx.StrokeAsync();
        }

        public List<EditableProperty> GetEditableProperties()
        {
            bool isReadOnly = RuchomySlupek; // Jeśli RuchomySlupek == false, to Y1 i Y2 mogą być edytowalne
            bool someOtherFlag = RuchomySlupek; // Jeśli masz drugi parametr, np. do blokowania

            bool someHFlag = false; // Parametr blokowania poziomego ruchu
            bool someVFlag = false; // Parametr blokowania pionowego ruchu

            if (PionPoziom)
            {
                if (X1 != X2)
                {
                    someHFlag = true;
                    Y2 = Y1; // Blokujemy ruch w pionie, ustawiając Y2 na Y1
                }
                else
                {
                    someVFlag = true;
                    X2 = X1; // Blokujemy ruch w poziomie, ustawiając X2 na X1
                }
            }

            var properties = new List<EditableProperty>
    {
        new EditableProperty("X1", () => X1, v => X1 = v, NazwaObj),
        new EditableProperty("Y1", () => Y1, v => Y1 = v, NazwaObj, isReadOnly, someOtherFlag, someHFlag),
        new EditableProperty("Y2", () => Y2, v => Y2 = v, NazwaObj, isReadOnly || someHFlag, someOtherFlag, someVFlag) // Jeśli someHFlag = true, Y2 jest tylko do odczytu
    };

            if (!RuchomySlupek)
            {
                properties.Add(new EditableProperty("X2", () => X2, v => X2 = v, NazwaObj, isReadOnly || someVFlag, someOtherFlag, someVFlag)); // Jeśli someVFlag = true, X2 jest tylko do odczytu
            }

            return properties;
        }


        public void Scale(double factor)
        {
            double centerX = (X1 + X2) / 2;
            double centerY = (Y1 + Y2) / 2;

            X1 = centerX + (X1 - centerX) * factor;
            Y1 = centerY + (Y1 - centerY) * factor;
            X2 = centerX + (X2 - centerX) * factor;
            Y2 = centerY + (Y2 - centerY) * factor;

            // Zapewniamy, że linia pozostaje pionowa, jeśli trzeba
            if (RuchomySlupek)
            {
                X2 = X1;
            }
        }

        public void Move(double offsetX, double offsetY)
        {
            X1 += offsetX;
            Y1 += offsetY;
            X2 += offsetX;
            Y2 += offsetY;

            if (RuchomySlupek)
            {
                X2 = X1; // Zapewniamy pionową linię po przesunięciu
            }

            Console.WriteLine($"Offset X: {offsetX}, Offset Y: {offsetY}");
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(Math.Min(X1, X2), Math.Min(Y1, Y2), Math.Abs(X2 - X1), Math.Abs(Y2 - Y1), NazwaObj);
        }

        public void Transform(double scale, double offsetX, double offsetY)
        {
            X1 = (X1 * scale) + offsetX;
            Y1 = (Y1 * scale) + offsetY;
            X2 = (X2 * scale) + offsetX;
            Y2 = (Y2 * scale) + offsetY;

            if (RuchomySlupek)
            {
                X2 = X1; // Zapewniamy pionową linię po transformacji
            }
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X1 = (X1 * scaleX) + offsetX;
            Y1 = (Y1 * scaleY) + offsetY;
            X2 = (X2 * scaleX) + offsetX;
            Y2 = (Y2 * scaleY) + offsetY;

            if (RuchomySlupek)
            {
                X2 = X1; // Utrzymujemy linię pionową po transformacji
            }
        }

    }

}
