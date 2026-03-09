using System.Globalization;

namespace GEORGE.Client.Pages.PDF
{
    public class Zestawienie
    {
        public string? NrZestawienia { get; set; }
        public DateTime Data { get; set; }
        public string? Odbiorca { get; set; }
        public List<CutListItem> ListaCieci { get; set; } = new List<CutListItem>();
    }

    public class CutListItem
    {
        public string? Lp { get; set; }
        public string? Symbol { get; set; }
        public string? Kolor { get; set; }
        public string? Ilosc { get; set; }
        public string? Wymiar { get; set; }
        public string? Kat { get; set; }
        public string? WymiarNaZamowienie { get; set; }
        public string? Uwagi { get; set; }
    }
    public class PdfDataParser
    {
        public Zestawienie ParsePdfData(string pdfText)
        {
            pdfText = pdfText.ToUpper();

            var zestawienie = new Zestawienie();
            var lines = pdfText.ToUpper().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var tempLine = new List<string>();
            var kantowkaList = new List<string>();

            try
            {
                zestawienie.NrZestawienia = lines[0].Split(new[] { "NR ZEST " }, StringSplitOptions.None)[1].Split()[0];
                zestawienie.Data = DateTime.ParseExact(lines[0].Split(" DN. ")[1].Split()[0], "yyyy/MM/dd", CultureInfo.InvariantCulture);
                zestawienie.Odbiorca = lines[0].Split(" P.")[1];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd w parsowaniu nagłówka zestawienia: " + ex.Message);
                zestawienie.NrZestawienia = "Błąd, lub brak danych";
                zestawienie.Data = DateTime.Now;
                zestawienie.Odbiorca = "";
            }

            string strUwagi = "";

            bool capturing = false;
            foreach (var line in lines)
            {
                if (line.ToUpper().Contains("KANTÓWKA"))
                {
                    capturing = true;
                    tempLine.Clear();
                    strUwagi = line;
                }

                if (capturing)
                {
                    var items = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    tempLine.AddRange(items);

                    if (tempLine.Count >= 7)
                    {
                        var matchedLine = string.Join(" ", tempLine);
                        kantowkaList.Add(matchedLine);
                        capturing = false;
                        Console.WriteLine($"matchedLine: {matchedLine}");
                    }
                }
            }

            Console.WriteLine(kantowkaList.Count());

            foreach (var kantowka in kantowkaList)
            {
                try
                {
                    var items = kantowka.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (items.Length >= 7)
                    {
                        // Bezpieczne pobieranie uwag - obsługa przypadku gdy nie ma znaku "-"
                        string uwagi = "";
                        try
                        {
                            int indexOfDash = strUwagi.IndexOf("-");
                            if (indexOfDash >= 0)
                            {
                                uwagi = strUwagi.Substring(0, indexOfDash).TrimEnd();
                            }
                            else
                            {
                                // Jeśli nie znaleziono znaku "-", użyj całego tekstu
                                uwagi = strUwagi.TrimEnd();
                                Console.WriteLine($"Uwaga: Brak znaku '-' w tekście: {strUwagi}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Błąd podczas przetwarzania uwag: {ex.Message}");
                            uwagi = strUwagi; // W razie błędu użyj całego tekstu
                        }

                        uwagi = BezpiecznySubstring(strUwagi, "-");

                        var cutListItem = new CutListItem
                        {
                            Lp = items[0].ToString(),
                            Symbol = items[1],
                            Kolor = items[2],
                            Ilosc = items[3],
                            Wymiar = items[4],
                            Kat = (items.Length > 6 ? items[6] : ""),
                            WymiarNaZamowienie = DlugoscHandlowa(items[4]),
                            Uwagi = uwagi
                        };

                        if (int.TryParse(items[3], out _))
                        {
                            zestawienie.ListaCieci.Add(cutListItem);
                        }
                        else
                        {
                            if ((items[3].ToUpper() == "HG" || items[3].ToUpper() == "HB") && int.TryParse(items[4], out _))
                            {
                                cutListItem.Ilosc = items[4];
                                cutListItem.Wymiar = items[5];
                                cutListItem.Kat = (items.Length > 6 ? items[6] : "");
                                cutListItem.WymiarNaZamowienie = DlugoscHandlowa(items[5]);
                                cutListItem.Uwagi = uwagi;

                                zestawienie.ListaCieci.Add(cutListItem);
                            }
                            else
                            {
                                Console.WriteLine($"Nie dodano wiersza: {kantowka}, items.Length: {items.Length}, ilość sztuk: {items[3]}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Logujemy błąd ale kontynuujemy przetwarzanie kolejnych linii
                    Console.WriteLine($"Błąd w parsowaniu linii '{kantowka}': {ex.Message}");
                    // Nie rzucamy wyjątku - tylko logujemy i kontynuujemy
                }
            }

            return zestawienie;
        }
        private string BezpiecznySubstring(string tekst, string separator)
        {
            if (string.IsNullOrEmpty(tekst))
                return "";

            int index = tekst.IndexOf(separator);
            if (index >= 0)
            {
                try
                {
                    return tekst.Substring(0, index).TrimEnd();
                }
                catch
                {
                    return tekst.TrimEnd();
                }
            }
            return tekst.TrimEnd();
        }
        public string DlugoscHandlowa(string dlugoscWyliczona)
        {
            if (!int.TryParse(dlugoscWyliczona, out int dlugoscX))
            {
                return "0";
            }

            int dlugoscKrok = 100;
            int zaokraglonaDlugosc = (int)Math.Ceiling((double)dlugoscX / dlugoscKrok) * dlugoscKrok;

            // Oblicz różnicę
            int roznica = zaokraglonaDlugosc - dlugoscX;

            // Dostosowanie w przypadku małej różnicy
            if (roznica < 80)
            {
                zaokraglonaDlugosc += dlugoscKrok;
            }

            return zaokraglonaDlugosc.ToString();
        }

        public Zestawienie ParseClipboardData(string clipboardText)
        {
            var zestawienie = new Zestawienie();
            var lines = clipboardText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines.Skip(2)) // Pomijamy pierwsze dwie linie (Data/Godzina i nagłówki)
            {
                var items = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (items.Length >= 9) // Zakładamy, że linia musi mieć co najmniej 10 elementów
                {
                    try
                    {
                        var cutItem = new CutListItem
                        {
                            Ilosc = items[0].Trim(),
                            Wymiar = items[items.Count() - 1].Trim(),
                            Kat = items[1].Trim(),
                            WymiarNaZamowienie = DlugoscHandlowa(items[5]),
                            Lp = items[6].Trim(),
                            Uwagi = line//items[9].Trim() // Przyjmuję, że ostatni element jest opisem, ale możesz to dostosować
                        };

                        zestawienie.ListaCieci.Add(cutItem);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Błąd w parsowaniu linii: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Za mało elementów w linii: {line}");
                }
            }


            //cutListItem.Ilosc = items[4];
            //cutListItem.Wymiar = items[5];
            //cutListItem.Kat = (items.Length > 6 ? items[6] : "");
            //cutListItem.WymiarNaZamowienie = DlugoscHandlowa(items[5]);
            //cutListItem.Uwagi = strUwagi.Substring(0, strUwagi.IndexOf("-")).TrimEnd();

            Console.WriteLine($"ZestawienieSz ListaSzyb.Count: {zestawienie.ListaCieci.Count()}");
            return zestawienie;
        }

    }
}
