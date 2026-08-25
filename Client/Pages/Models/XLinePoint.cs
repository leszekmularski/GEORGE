namespace GEORGE.Client.Pages.Models
{
    public class XLinePoint
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }

        private string _typeLine = string.Empty;

        public XLinePoint(double x1, double y1, double x2, double y2, string typeLine)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            _typeLine = typeLine;
        }

        // Właściwość zwracająca kąt linii w stopniach (0-180°)
        public double KatLinii
        {
            get
            {
                // Obliczamy różnice współrzędnych
                double dx = X2 - X1;
                double dy = Y2 - Y1;

                // Obliczamy kąt w radianach
                double katRadiany = Math.Atan2(dy, dx);

                // Konwertujemy na stopnie
                double katStopnie = katRadiany * (180.0 / Math.PI);

                // Normalizujemy kąt do zakresu 0-180° (niezależnie od kierunku)
                if (katStopnie < 0)
                {
                    katStopnie += 180;
                }
                else if (katStopnie >= 180)
                {
                    katStopnie -= 180;
                }

                return katStopnie;
            }
        }

        // Właściwość zwracająca kąt w radianach (0-π)
        public double KatLiniiRadiany
        {
            get
            {
                double dx = X2 - X1;
                double dy = Y2 - Y1;

                double katRadiany = Math.Atan2(dy, dx);

                // Normalizujemy do zakresu 0-π
                if (katRadiany < 0)
                {
                    katRadiany += Math.PI;
                }
                else if (katRadiany >= Math.PI)
                {
                    katRadiany -= Math.PI;
                }

                return katRadiany;
            }
        }

        // Właściwość zwracająca długość linii
        public double Dlugosc
        {
            get
            {
                double dx = X2 - X1;
                double dy = Y2 - Y1;
                return Math.Sqrt(dx * dx + dy * dy);
            }
        }

        // Metoda zwracająca kąt w stopniach z możliwością wyboru zakresu
        public double PobierzKat(bool wStopniach = true)
        {
            return wStopniach ? KatLinii : KatLiniiRadiany;
        }

        // Metoda zwracająca kąt względem osi X (0-360°)
        public double PobierzKatPelny()
        {
            double dx = X2 - X1;
            double dy = Y2 - Y1;

            double katRadiany = Math.Atan2(dy, dx);
            double katStopnie = katRadiany * (180.0 / Math.PI);

            // Normalizujemy do zakresu 0-360°
            if (katStopnie < 0)
            {
                katStopnie += 360;
            }

            return katStopnie;
        }
    }
}