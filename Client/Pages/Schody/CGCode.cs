using GEORGE.Client.Pages.Drzwi;
using System.Globalization;
using System.Text;

namespace GEORGE.Client.Pages.Schody
{
    public class CGCode
    {
        public string GenerateGCode(List<LinePoint> lines)
        {
         
            // Przesunięcie linii do początku układu współrzędnych
            var linesX = ShiftLinesToOrigin(lines);

            // Lista na sklonowane linie
            var clonedLines = new List<LinePoint>();

            // Iteracja przez linie w `linesX`
            foreach (var line in linesX)
            {
                // Sprawdzenie, czy `zRobocze` zawiera więcej niż jedną wartość
                if (line.zRobocze != null && line.zRobocze.Length > 1)
                {
                    // Dla każdej wartości w `zRobocze` twórz nowy obiekt `LinePoint` z odpowiednią wartością
                    foreach (var zValue in line.zRobocze)
                    {
                        if (!string.IsNullOrEmpty(zValue))
                        {
                            clonedLines.Add(new LinePoint(
                            line.X1,
                            line.Y1,
                            line.X2,
                            line.Y2,
                            line.typeLine,
                            line.fileNCName,
                            line.nameMacro,
                            line.idOBJ + zValue.TrimStart('Z').Replace(".",""),
                            new[] { zValue }, // Zastąp `zRobocze` pojedynczą wartością
                            line.idRuchNarzWObj,
                            line.addGcode,
                            line.IloscSztuk,
                            line.NazwaProgramu
                            ));
                        }
                    }
                }
                else
                {
                    // Jeśli `zRobocze` jest puste lub zawiera jedną wartość, dodaj oryginalną linię
                    clonedLines.Add(line);
                }
            }

            // Zastąpienie zawartości `linesX` sklonowanymi danymi
            linesX = clonedLines;


            var gcodeBuilder = new System.Text.StringBuilder();

            // Nagłówek programu
            if (lines.Count > 0)
            {
                gcodeBuilder.AppendLine($"%_N_{lines[0].NazwaProgramu.Replace("-","_") + "_" + lines[0].fileNCName}_MPF");
                gcodeBuilder.AppendLine($";$PATH=/_N_WKS_DIR/{lines[0].NazwaProgramu.Replace("-", "_") + "_" + lines[0].fileNCName}_WPD");
            }
            else
            {
                gcodeBuilder.AppendLine($"%_N_1_MPF");
                gcodeBuilder.AppendLine($";$PATH=/_N_WKS_DIR/1_WPD");
            }

            gcodeBuilder.AppendLine("N1 G641 ADIS=1 SOFT G54 M51");
            gcodeBuilder.AppendLine("N2 CPRECON");
            gcodeBuilder.AppendLine("N3 CFIN");
            gcodeBuilder.AppendLine("N4 G00 G90 G53 D0 Z340");
            gcodeBuilder.AppendLine("N5 G53 X3740 Y1300");
            gcodeBuilder.AppendLine("N6 G53 D0 Z340 S0");
            gcodeBuilder.AppendLine("N7; (Frez FI16)");
            gcodeBuilder.AppendLine("N8 T10");
            gcodeBuilder.AppendLine("N9 DRZ=170 DRR=3");
            gcodeBuilder.AppendLine("N10 L899; PIERWSZE NARZEDZIE");
            gcodeBuilder.AppendLine("N11 D1");
            gcodeBuilder.AppendLine("N12 M21");
            gcodeBuilder.AppendLine("N13; KONTUR PIERWSZY");

            // Grupowanie według `nameMacro`
            // Grupowanie według `nameMacro`, a następnie sortowanie według `idOBJ`, `idRuchNarzWObj` i `zRobocze`
            var groupedLines = linesX
                .Where(lp => !string.IsNullOrEmpty(lp.nameMacro)) // Pomijanie linii bez `nameMacro`
                .OrderBy(lp => lp.nameMacro)                     // Sortowanie według `nameMacro`
                .ThenByDescending(lp => lp.idOBJ)                          // Sortowanie według `idOBJ`
                .ThenBy(lp => lp.idRuchNarzWObj)                 // Sortowanie według `idRuchNarzWObj`
                .ThenBy(lp => lp.zRobocze?.FirstOrDefault())     // Sortowanie według pierwszej wartości w `zRobocze`
                .GroupBy(lp => lp.nameMacro);                    // Grupowanie według `nameMacro`


            //Console.WriteLine($"------------------------------------------------------------");
            //foreach (var line in linesX)
            //{
            //    Console.WriteLine($"X2: {line.X2} Y2: {line.Y2}  line.nameMacro: {line.nameMacro} line.idOBJ:{line.idOBJ} line.zRobocze: {line.zRobocze[0]} line.zRobocze.Count: {line.zRobocze.Count()}");
            //}
            //Console.WriteLine($"------------------------------------------------------------");

            uint i = 0;

            // Iteracja przez grupy
            foreach (var group in groupedLines)
            {
                // Dodanie komentarza z nazwą makra
                gcodeBuilder.AppendLine($";(Makro: {group.Key})");
                gcodeBuilder.AppendLine("G53 D0 Z340; Podnies narzedzie przed przejsciem");

                bool startG0 = true;

                string idObj = "";

                foreach (var line in group)
                {
                    if (idObj != line.idOBJ) startG0 = true;

                    for (int j = 0; j < line.zRobocze.Count(); j++)
                    {

                     // Console.WriteLine($"line.zRobocze: {line.zRobocze[j]} startG0: {startG0} idObj: {idObj} line.idOBJ: {line.idOBJ}");  

                        if (startG0)
                        {
                            // Przejście do pierwszego punktu linii (bez cięcia)
                            gcodeBuilder.Append(WejscieWKontur(line, line.zRobocze[j]));

                            startG0 = false;
                            idObj = line.idOBJ;
                        }
                        else
                        {
                            // Ruch z cięciem do drugiego punktu linii
                            gcodeBuilder.AppendLine($"X{line.X2.ToString("F2", CultureInfo.InvariantCulture)} Y{line.Y2.ToString("F2", CultureInfo.InvariantCulture)}");

                        }

                    }

                }

                // Podniesienie narzędzia na koniec grupy
                gcodeBuilder.AppendLine("G0 G40 Z340 ; Podnies narzedzie na koniec makra");
            }

            // Stop programu
            gcodeBuilder.AppendLine("G00 G53 Z340 S0 D0; Wylacz wrzeciono");
            gcodeBuilder.AppendLine("G90 G53 X3740 Y1300");
            gcodeBuilder.AppendLine("M30; Zakoncz program");

            return gcodeBuilder.ToString();
        }

        public static StringBuilder WejscieWKontur(LinePoint line, string zPoziom)
        {
            var gcodeBuilder = new System.Text.StringBuilder();

            if (string.IsNullOrEmpty(zPoziom)) return gcodeBuilder;

            gcodeBuilder.AppendLine($"G54");

            if (line.nameMacro == "WANGA_KIESZEN")
            {
                gcodeBuilder.AppendLine($"G0 G40 Z50.");
                gcodeBuilder.AppendLine($"X{(line.X1 + 12).ToString("F2", CultureInfo.InvariantCulture)} Y{(line.Y1).ToString("F2", CultureInfo.InvariantCulture)}");
                gcodeBuilder.AppendLine($"G1 G41 X{line.X1.ToString("F2", CultureInfo.InvariantCulture)} Y{line.Y1.ToString("F2", CultureInfo.InvariantCulture)} {zPoziom} F2500");
                gcodeBuilder.AppendLine($"X{line.X2.ToString("F2", CultureInfo.InvariantCulture)} Y{line.Y2.ToString("F2", CultureInfo.InvariantCulture)} F6000;{line.nameMacro}");
            }
            else if (line.nameMacro == "WANGA_OBRYS")
            {
                gcodeBuilder.AppendLine($"G0 G40 Z50.");
                gcodeBuilder.AppendLine($"X{(line.X1).ToString("F2", CultureInfo.InvariantCulture)} Y{(line.Y1 + 50).ToString("F2", CultureInfo.InvariantCulture)}");
                gcodeBuilder.AppendLine($"G1 G42 X{line.X1.ToString("F2", CultureInfo.InvariantCulture)} Y{line.Y1.ToString("F2", CultureInfo.InvariantCulture)} {zPoziom} F2500");
                gcodeBuilder.AppendLine($"X{line.X2.ToString("F2", CultureInfo.InvariantCulture)} Y{line.Y2.ToString("F2", CultureInfo.InvariantCulture)} F6000;{line.idOBJ}");
            }
            else if (line.nameMacro == "STOPIEN_W2")
            {
                gcodeBuilder.AppendLine($"G0 G40 Z50.");
                gcodeBuilder.AppendLine($"X{(line.X1 + 25).ToString("F2", CultureInfo.InvariantCulture)} Y{(line.Y1 - 5).ToString("F2", CultureInfo.InvariantCulture)}");
                gcodeBuilder.AppendLine($"G1 G42 X{line.X1.ToString("F2", CultureInfo.InvariantCulture)} Y{line.Y1.ToString("F2", CultureInfo.InvariantCulture)} {zPoziom} F2500");
                gcodeBuilder.AppendLine($"X{line.X2.ToString("F2", CultureInfo.InvariantCulture)} Y{line.Y2.ToString("F2", CultureInfo.InvariantCulture)} F6000;{line.idOBJ}");
            }
            else if (line.nameMacro == "ASTOPIEN_W2")
            {
                gcodeBuilder.AppendLine($"G0 G40 Z50.");
                gcodeBuilder.AppendLine($"X{(line.X1).ToString("F2", CultureInfo.InvariantCulture)} Y{(line.Y1 - 20).ToString("F2", CultureInfo.InvariantCulture)}");
                gcodeBuilder.AppendLine($"G1 G42 X{line.X1.ToString("F2", CultureInfo.InvariantCulture)} Y{line.Y1.ToString("F2", CultureInfo.InvariantCulture)} {zPoziom} F2500");
                gcodeBuilder.AppendLine($"X{line.X2.ToString("F2", CultureInfo.InvariantCulture)} Y{line.Y2.ToString("F2", CultureInfo.InvariantCulture)} F6000;{line.nameMacro}");
            }
            else
            {
                gcodeBuilder.AppendLine($"G0 G40 Z50.");
                gcodeBuilder.AppendLine($"G0 G40 X{line.X1.ToString("F2", CultureInfo.InvariantCulture)} Y{line.Y1.ToString("F2", CultureInfo.InvariantCulture)}");
                gcodeBuilder.AppendLine($"G1 {zPoziom} F1500");
                gcodeBuilder.AppendLine($"X{line.X2.ToString("F2", CultureInfo.InvariantCulture)} Y{line.Y2.ToString("F2", CultureInfo.InvariantCulture)} ;NaN{line.idOBJ}");
            }

            return gcodeBuilder;
        }

        public (double X, double Y) RotatePoint(double x, double y, double angleRadians)
        {
            double rotatedX = x * Math.Cos(angleRadians) - y * Math.Sin(angleRadians);
            double rotatedY = x * Math.Sin(angleRadians) + y * Math.Cos(angleRadians);
            return (rotatedX, rotatedY);
        }

        public (double X, double Y) MirrorPointHorizontally(double x, double y, double shiftY)
        {
            return (x, -y + shiftY);
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
                idOBJ: line.idOBJ,
                zRobocze: line.zRobocze,
                idRuchNarzWObj: line.idRuchNarzWObj,
                addGcode: line.addGcode,
                iloscSztuk: line.IloscSztuk,
                nazwaProgramu: line.NazwaProgramu
            )).ToList();

            Console.WriteLine($"Przesunięcie o wartość minX: {minX} minY: {minY}");

            return shiftedLines;
        }


    }

}
