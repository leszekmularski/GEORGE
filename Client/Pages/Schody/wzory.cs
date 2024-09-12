namespace GEORGE.Client.Pages.Schody
{
    public class Wzory
    {
        public double WartoscZaczepPromnienia(double b, double alphaDegrees)
        {
            // Wartości początkowe
            //b = 1000;  // Długość boku b
            //alphaDegrees = 45;  // Kąt w stopniach

            // Konwersja kąta na radiany
            double alphaRadians = alphaDegrees * (Math.PI / 180);

            // Obliczenie cos(alpha)
            double cosAlpha = Math.Cos(alphaRadians);

            // Obliczenie c ze wzoru c = b / cos(alpha)
            double c = b / cosAlpha;
                  
            double x = (c - b) * cosAlpha;

            // Wyświetlenie wyniku wydłużenie zaczepienia promienia w pozomie i pionie
            return x + b;

        }
        public double KatZaczepPromnienia(double b, double alphaDegrees)
        {

            return 0;
        }
    }
}
