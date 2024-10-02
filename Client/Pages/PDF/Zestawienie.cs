using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

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

            //Console.WriteLine(pdfText);

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

                if (line.Contains("KANTÓWKA"))
                {
                    capturing = true;
                    tempLine.Clear();

                    strUwagi = line;

                }

                if (capturing)
                {
                    var items = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    tempLine.AddRange(items);
                    // Console.WriteLine(line);
                    // Check if the accumulated line has the required number of elements
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
                var items = kantowka.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (items.Length >= 7)
                {
                    try
                    {
                        var cutListItem = new CutListItem
                        {
                            Lp = items[0].ToString(),
                            Symbol = items[1],
                            Kolor = items[2],
                            Ilosc = items[3],
                            Wymiar = items[4],
                            Kat = (items.Length > 6 ? items[6] : ""),
                            WymiarNaZamowienie = DlugoscHandlowa(items[4]),
                            Uwagi = strUwagi.Substring(0, strUwagi.IndexOf("-")).TrimEnd()//ZwrocOpis(kantowka)//strUwagi,
                        };

                        if (int.TryParse(items[3], out _))
                        {
                            zestawienie.ListaCieci.Add(cutListItem);
                        }
                       
                    }
                    catch (Exception ex)
                    {
                        throw new FormatException($"Błąd w parsowaniu linii: {ex.Message}");
                    }
                }
            }

            return zestawienie;
        }

        private string ZwrocOpis(string linia)
        {
            if (linia.Contains("KANTÓWKA"))
            {

                return linia;

            }
            else
            {
                var tempLine = new List<string>();
                var items = linia.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                tempLine.AddRange(items);
                // Console.WriteLine(line);
                // Check if the accumulated line has the required number of elements
                if (tempLine.Count >= 7)
                {
                    var matchedLine = string.Join(" ", tempLine);
                    return matchedLine;
                }
                else
                {
                    return "Brak danych....";
                }
            }
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

    }
}
