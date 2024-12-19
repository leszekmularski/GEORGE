using GEORGE.Client.Pages.Drzwi;
using System.Globalization;

namespace GEORGE.Client.Pages.Schody
{
    public class CGCode
    {
        public string GenerateGCode(List<LinePoint> lines)
        {

           var linesX = ShiftLinesToOrigin(lines);

            var gcodeBuilder = new System.Text.StringBuilder();

            // Nagłówek programu
            gcodeBuilder.AppendLine($"%_N_{lines[0].fileNCName}_MPF");
            gcodeBuilder.AppendLine($";$PATH=/_N_WKS_DIR/{lines[0].fileNCName}_WPD");
            gcodeBuilder.AppendLine("N1 G40 G641 ADIS=1 SOFT G54 M51");
            gcodeBuilder.AppendLine("N2 CPRECON");
            gcodeBuilder.AppendLine("N3 CFIN");
            gcodeBuilder.AppendLine("N4 G00 G90 G53 D0 Z340");
            gcodeBuilder.AppendLine("N5 G00 G90 G53 X3740 Y1300");
            gcodeBuilder.AppendLine("N6 G00 G90 G53 D0 Z340 S0");
            gcodeBuilder.AppendLine("N7; (Frez FI16)");
            gcodeBuilder.AppendLine("N8 T01");
            gcodeBuilder.AppendLine("N9 DRZ=170 DRR=3");
            gcodeBuilder.AppendLine("N10 L899; PIERWSZE NARZEDZIE");
            gcodeBuilder.AppendLine("N11 D1");
            gcodeBuilder.AppendLine("N12 M21");
            gcodeBuilder.AppendLine("N13; KONTUR PIERWSZY");

            // Grupowanie według `nameMacro`
            var groupedLines = linesX
                .Where(lp => !string.IsNullOrEmpty(lp.nameMacro)) // Pomijanie linii bez `nameMacro`
                .GroupBy(lp => lp.nameMacro);

            // Iteracja przez grupy
            foreach (var group in groupedLines)
            {
                // Dodanie komentarza z nazwą makra
                gcodeBuilder.AppendLine($"(Makro: {group.Key})");
                gcodeBuilder.AppendLine("G0 Z340; Podnies narzedzie przed przejsciem");

                bool startG0 = true;

                string idObj = "";

                foreach (var line in group)
                {
                    if (idObj != line.idOBJ) startG0 = true;

                    if (startG0) 
                    {
                        // Przejście do pierwszego punktu linii (bez cięcia)
                        gcodeBuilder.AppendLine($"G0 X{line.X1.ToString("F3", CultureInfo.InvariantCulture)} Y{line.Y1.ToString("F3", CultureInfo.InvariantCulture)} Z50.");
                        startG0 = false;
                        idObj = line.idOBJ;
                    }
                    else
                    {
                        // Ruch z cięciem do drugiego punktu linii
                        gcodeBuilder.AppendLine($"G1 X{line.X2.ToString("F3", CultureInfo.InvariantCulture)} Y{line.Y2.ToString("F3", CultureInfo.InvariantCulture)}");
                    }
                  
                }

                // Podniesienie narzędzia na koniec grupy
                gcodeBuilder.AppendLine("G0 Z340 ; Podnies narzedzie na koniec makra");
            }

            // Stop programu
            gcodeBuilder.AppendLine("G00 G53 Z340 S0 D0; Wylacz wrzeciono");
            gcodeBuilder.AppendLine("G90 G53 X3740 Y1300; Podnies narzedzie");
            gcodeBuilder.AppendLine("M30; Zakoncz program");
  
            return gcodeBuilder.ToString();
        }


        public (double X, double Y) RotatePoint(double x, double y, double angleRadians)
        {
            double rotatedX = x * Math.Cos(angleRadians) - y * Math.Sin(angleRadians);
            double rotatedY = x * Math.Sin(angleRadians) + y * Math.Cos(angleRadians);
            return (rotatedX, rotatedY);
        }

        public List<LinePoint> ShiftLinesToOrigin(List<LinePoint> lines)
        {
            if (lines == null || lines.Count == 0)
            {
                throw new ArgumentException("Lista linii jest pusta lub null.");
            }

            // Znalezienie minimalnych wartości X i Y
            double minX = lines.Min(line => Math.Min(line.X1, line.X2));
            double minY = lines.Min(line => Math.Min(line.Y1, line.Y2));

            // Przesunięcie wszystkich punktów
            var shiftedLines = lines.Select(line => new LinePoint(
                x1: line.X1 - minX,
                y1: line.Y1 - minY,
                x2: line.X2 - minX,
                y2: line.Y2 - minY,
                typeLine: line.typeLine,
                fileNCName: line.fileNCName,
                nameMacro: line.nameMacro,
                addGcode: line.addGcode
            )).ToList();

            Console.WriteLine($"Przesunięcie o wartość minX: {minX} minY: {minY}");  

            return shiftedLines;
        }


    }

}
