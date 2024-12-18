namespace GEORGE.Client.Pages.Schody
{
    public class CGCode
    {
        public string GenerateGCode(List<LinePoint> lines)
        {
            var gcodeBuilder = new System.Text.StringBuilder();

            gcodeBuilder.AppendLine("G21 ; Ustaw jednostki na milimetry");
            gcodeBuilder.AppendLine("G17 ; Wybierz płaszczyznę XY");
            gcodeBuilder.AppendLine("G90 ; Ustaw tryb współrzędnych absolutnych");
            gcodeBuilder.AppendLine("M3 ; Włącz wrzeciono");

            foreach (var line in lines)
            {
                // Przejście do pierwszego punktu linii (bez cięcia)
                gcodeBuilder.AppendLine($"G0 X{line.X1:F3} Y{line.Y1:F3}");
                // Ruch z cięciem do drugiego punktu linii
                gcodeBuilder.AppendLine($"G1 X{line.X2:F3} Y{line.Y2:F3}");
            }

            gcodeBuilder.AppendLine("M5 ; Wyłącz wrzeciono");
            gcodeBuilder.AppendLine("G0 Z10 ; Podnieś narzędzie");
            gcodeBuilder.AppendLine("M30 ; Zakończ program");

            return gcodeBuilder.ToString();
        }

        public (double X, double Y) RotatePoint(double x, double y, double angleRadians)
        {
            double rotatedX = x * Math.Cos(angleRadians) - y * Math.Sin(angleRadians);
            double rotatedY = x * Math.Sin(angleRadians) + y * Math.Cos(angleRadians);
            return (rotatedX, rotatedY);
        }

    }

}
