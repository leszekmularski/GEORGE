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
            pdfText = pdfText.ToUpper();

            var zestawienieSz = new ZestawienieSzyb();
            var lines = pdfText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var szybyList = new List<GlassListItem>();

            int jestUG = 0;

            foreach (var line in lines)
            {
                jestUG++;
                Console.WriteLine($"line: {line} {jestUG}");  
                
                if(jestUG  == 6)
                { 
                  var items = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (items.Length >= 6) // Zakładam, że linia musi mieć co najmniej 10 elementów
                    {
                        try
                        {
                            var cutListSz = new GlassListItem
                            {
                                Lp = items[0],
                                Wysokosc = items[0],
                                Szerokosc = items[1],
                                Ilosc = items[2],
                                 RodzajSzyby = string.Join(" ", items[3], items[4], items[5], items[6]), // Scalanie elementów rodzaju szyby
                                //RodzajRamki = string.Join(" ", items.Skip(7).Take(3)), // Scalanie elementów rodzaju ramki
                                //Powierzchnia = items[8],
                                Uwagi = line // Cała linia jako uwagi
                            };

                            // Dodajemy element do listy tylko, jeśli "Ilosc" jest poprawnym numerem
                            if (int.TryParse(items[2], out _))
                            {
                                zestawienieSz.ListaSzyb.Add(cutListSz);
                            }
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

                // Sprawdzamy, czy linia zawiera kluczowy identyfikator (np. "UG=")
                if (line.ToUpper().Contains("UG="))
                {
                    jestUG = 0;
                    Console.WriteLine($"jestUG = 0");
                }
            }

            Console.WriteLine($"zestawienieSz ListaSzyb.Count: {zestawienieSz.ListaSzyb.Count()}");

            return zestawienieSz;
        }


    }
}
