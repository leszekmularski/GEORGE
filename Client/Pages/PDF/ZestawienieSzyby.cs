using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GEORGE.Client.Pages.PDF
{

    public class ZestawienieSzyb
    {
        public string? NrZestawienia { get; set; }
        public DateTime Data { get; set; }
        public string? Odbiorca { get; set; }
        public List<GlassListItem> ListaSzyb { get; set; } = new List<GlassListItem>();
    }

    public class GlassListItem
    {
        public string? Lp { get; set; }
        public string? Szerokosc { get; set; }
        public string? Wysokosc { get; set; }
        public string? Ilosc { get; set; }
        public string? RodzajSzyby { get; set; }
        public string? RodzajRamki { get; set; }
        public string? Powierzchnia { get; set; }
        public string? Uwagi { get; set; }
    }

    public class PdfDataParserSzyby
    {
        public ZestawienieSzyb ParsePdfDataSzyby(string pdfText)
        {
           // pdfText = pdfText.ToUpper();

            var zestawienieSz = new ZestawienieSzyb();
            var lines = pdfText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            string liniaZPoprawnymiDanymi = "";
            int foundUG = 0;

            foreach (var line in lines)
            {
                // Usuwanie dodatkowych spacji
                var items = Regex.Split(liniaZPoprawnymiDanymi.Trim(), @"\s+");

                Console.WriteLine($"liniaZPoprawnymiDanymi: {liniaZPoprawnymiDanymi} bierząca: {line}"); 

                if (items.Length >= 7 && foundUG == 2) // Minimalna liczba elementów
                {
                    try
                    {
                        // Przygotowanie elementu GlassListItem
                        var cutListSz = new GlassListItem
                        {
                            Lp = (zestawienieSz.ListaSzyb.Count() + 1).ToString(),// items[0],
                            Wysokosc = items[1], //items[2],
                            Szerokosc = items[0], //items[1],
                            Ilosc = items[2], //items[3],
                            RodzajSzyby = string.Join(" ", items.Skip(3).Take(items.Length - 4)), // Łączenie elementów rodzaju szyby
                            Powierzchnia = items.Length >= 8 ? items[items.Length - 1] : "0", // Powierzchnia, jeśli dostępna
                            RodzajRamki = line, // Ramka, jeśli dostępna
                            Uwagi = items.Length >= 9 ? items.Last() : "" // Uwagi, jeśli dostępne
                        };

                        zestawienieSz.ListaSzyb.Add(cutListSz);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Błąd w parsowaniu linii: {ex.Message}");
                    }

                    liniaZPoprawnymiDanymi = "";
                    foundUG = 0;
                }
                else
                {
                    Console.WriteLine($"Za mało elementów w linii: {line}");
                }

                // Wykrywanie ramki międzyszybowej
                if (foundUG == 2)
                {
                    liniaZPoprawnymiDanymi += " " + line.Trim();
                    Console.WriteLine($"foundUG == 2: {liniaZPoprawnymiDanymi}");
                }


                // Wykrywanie powierzchni
                if (foundUG == 1)
                {
                    liniaZPoprawnymiDanymi += " " + line.Trim();
                    Console.WriteLine($"foundUG == 1: {liniaZPoprawnymiDanymi}");
                    foundUG++;
                }

                // Sprawdzanie, czy linia zawiera "UG="
                if (line.ToUpper().Contains("UG="))
                {
                    liniaZPoprawnymiDanymi += " " + line.Trim();
                    Console.WriteLine($"UG=: {liniaZPoprawnymiDanymi}");
                    foundUG++;
                }
            }

            Console.WriteLine($"zestawienieSz ListaSzyb.Count: {zestawienieSz.ListaSzyb.Count()}");
            return zestawienieSz;
        }


        public ZestawienieSzyb ParseClipboardData(string clipboardText)
        {
            var zestawienieSz = new ZestawienieSzyb();
            var lines = clipboardText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines.Skip(2)) // Pomijamy pierwsze dwie linie (Data/Godzina i nagłówki)
            {
                var items = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (items.Length >= 9) // Zakładamy, że linia musi mieć co najmniej 10 elementów
                {
                    try
                    {
                        var glassItem = new GlassListItem
                        {
                            RodzajSzyby = items[0].Trim(),
                            RodzajRamki = items[items.Count() - 1].Trim(),
                            Ilosc = items[1].Trim(),
                            Szerokosc = items[2].Trim(),
                            Wysokosc = items[3].Trim(),
                            Powierzchnia = items[4].Trim(),
                            Lp = items[6].Trim(),
                            Uwagi = line//items[9].Trim() // Przyjmuję, że ostatni element jest opisem, ale możesz to dostosować
                        };

                        zestawienieSz.ListaSzyb.Add(glassItem);
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

            Console.WriteLine($"ZestawienieSz ListaSzyb.Count: {zestawienieSz.ListaSzyb.Count()}");
            return zestawienieSz;
        }

    }
}
