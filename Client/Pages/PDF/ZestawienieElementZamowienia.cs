using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GEORGE.Client.Pages.PDF
{

    public class ZestawienieElementZamowienia
    {
        public string? NrZestawienia { get; set; }
        public DateTime Data { get; set; }
        public string? Odbiorca { get; set; }
        public List<ZestawienieElementZamowieniaListItem> ListaElementow { get; set; } = new List<ZestawienieElementZamowieniaListItem>();
    }

    public class ZestawienieElementZamowieniaListItem
    {
        public string? Lp { get; set; }
        public string? RowIdZlecenia { get; set; }
        public string? RowIdProducent { get; set; }
        public string? Szerokosc { get; set; }
        public string? Wysokosc { get; set; }
        public string? Dlugosc { get; set; }
        public string? Waga { get; set; }
        public string? Powierzchnia { get; set; }
        public string? Objetosc { get; set; }
        public string? CenaNetto { get; set; }
        public string? Typ { get; set; }
        public string? NumerKatalogowy { get; set; }
        public string? NazwaProduktu { get; set; }
        public string? Opis { get; set; }
        public string? Kolor { get; set; }
        public string IloscSztuk { get; set; } = "0";
        public string? Uwagi { get; set; }
        public DateTime DataZamowienia { get; set; } = DateTime.Now;
        public DateTime DataRealizacji { get; set; } = DateTime.Now.AddDays(14);
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; } = "NaN";
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
        public bool CzyZamowiono { get; set; } = false;
        public bool PozDostarczono { get; set; } = false;
        public DateTime DataDostarczenia { get; set; } = DateTime.MinValue;
    }

    public class PdfDataParserElementy
    {
        public ZestawienieElementZamowienia ParsePdfDataElementy(string pdfText)
        {
           // pdfText = pdfText.ToUpper();

            var zestawienieEl = new ZestawienieElementZamowienia();
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
                        var cutListEl = new ZestawienieElementZamowieniaListItem
                        {
                            Lp = (zestawienieEl.ListaElementow.Count() + 1).ToString(),// items[0],
                            Wysokosc = items[0], //items[2],
                            Szerokosc = items[1], //items[1],
                            IloscSztuk = items[2], //items[3],
                            NazwaProduktu = string.Join(" ", items.Skip(3).Take(items.Length - 4)), // Łączenie elementów rodzaju szyby
                            Powierzchnia = items.Length >= 8 ? items[items.Length - 1] : "0", // Powierzchnia, jeśli dostępna
                            NumerKatalogowy = line, // Ramka, jeśli dostępna
                            Uwagi = items.Length >= 9 ? items.Last() : "" // Uwagi, jeśli dostępne
                        };

                        zestawienieEl.ListaElementow.Add(cutListEl);
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

            Console.WriteLine($"zestawienieSz ListaSzyb.Count: {zestawienieEl.ListaElementow.Count()}");
            return zestawienieEl;
        }


        public ZestawienieElementZamowienia ParseClipboardData(string clipboardText)
        {
            var zestawienieEl = new ZestawienieElementZamowienia();
            var lines = clipboardText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines.Skip(2)) // Pomijamy pierwsze dwie linie (Data/Godzina i nagłówki)
            {
                var items = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (items.Length >= 9) // Zakładamy, że linia musi mieć co najmniej 10 elementów
                {
                    try
                    {
                        var glassItem = new ZestawienieElementZamowieniaListItem
                        {
                            NazwaProduktu = items[0].Trim(),
                            NumerKatalogowy = items[items.Count() - 1].Trim(),
                            IloscSztuk = items[1].Trim(),
                            Szerokosc = items[2].Trim(),
                            Wysokosc = items[3].Trim(),
                            Powierzchnia = items[4].Trim(),
                            Lp = items[6].Trim(),
                            Uwagi = line//items[9].Trim() // Przyjmuję, że ostatni element jest opisem, ale możesz to dostosować
                        };

                        zestawienieEl.ListaElementow.Add(glassItem);
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

            Console.WriteLine($"ZestawienieSz ListaSzyb.Count: {zestawienieEl.ListaElementow.Count()}");
            return zestawienieEl;
        }

    }
}
