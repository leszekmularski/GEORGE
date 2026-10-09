using System.Globalization;
using System.Text.RegularExpressions;

namespace GEORGE.Client.Pages.PDF
{
    public class OfertaPdfParser
    {
        public ZestawienieElementZamowienia ParsePdfData(string pdfText)
        {
            var zestawienie = new ZestawienieElementZamowienia();

            if (string.IsNullOrWhiteSpace(pdfText))
                return zestawienie;

            // ============================================================
            // ZABEZPIECZENIE - tylko oferty producenta VBH POLSKA
            // ============================================================

            const string wymaganyProducent = "VBH POLSKA SP. Z O.O.";

            if (!pdfText.Contains(
                    wymaganyProducent,
                    StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(
                    $"❌ Nieprawidłowy producent oferty. " +
                    $"Wymagany: {wymaganyProducent}");

                zestawienie.NrZestawienia = "***** Nieprawidłowy producent oferty ******";
                return zestawienie;
            }

            try
            {
                // =========================================================
                // NORMALIZACJA TEKSTU
                // =========================================================

                pdfText = pdfText
                    .Replace("\r\n", "\n")
                    .Replace("\r", "\n");

                var lines = pdfText
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();


                // =========================================================
                // NUMER OFERTY
                // =========================================================

                var ofertaMatch = Regex.Match(
                    pdfText,
                    @"Oferta\s+sprzedaży\s+([A-Z0-9\/\-]+)",
                    RegexOptions.IgnoreCase);

                if (ofertaMatch.Success)
                {
                    zestawienie.NrZestawienia =
                        ofertaMatch.Groups[1].Value.Trim();
                }


                // =========================================================
                // DATA
                // =========================================================

                var dataMatch = Regex.Match(
                    pdfText,
                    @"Data\s+dokumentu\s+(\d{4}/\d{2}/\d{2})",
                    RegexOptions.IgnoreCase);

                if (dataMatch.Success &&
                    DateTime.TryParseExact(
                        dataMatch.Groups[1].Value,
                        "yyyy/MM/dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime data))
                {
                    zestawienie.Data = data;
                }
                else
                {
                    zestawienie.Data = DateTime.Now;
                }


                // =========================================================
                // ODBIORCA
                // =========================================================

                int indexNabywca = lines.FindIndex(x =>
                    x.Equals("Nabywca",
                        StringComparison.OrdinalIgnoreCase));

                if (indexNabywca >= 0 &&
                    indexNabywca + 1 < lines.Count)
                {
                    zestawienie.Odbiorca =
                        lines[indexNabywca + 1].Trim();
                }


                // =========================================================
                // SZUKAMY POCZĄTKU TABELI
                // =========================================================

                int indexTabeli = lines.FindIndex(x =>
                    x.Contains("Poz.",
                        StringComparison.OrdinalIgnoreCase));

                if (indexTabeli < 0)
                {
                    Console.WriteLine(
                        "Nie znaleziono nagłówka tabeli pozycji.");

                    return zestawienie;
                }


                // =========================================================
                // ZBIERAMY TEKST TABELI
                // =========================================================

                var tabela = lines
                    .Skip(indexTabeli + 1)
                    .ToList();


                // =========================================================
                // SZUKAMY WSZYSTKICH POCZĄTKÓW POZYCJI
                //
                // Nie zakładamy, że numer pozycji jest na osobnej
                // linii. Szukamy wzorca:
                //
                // 1 111075 100151
                // 2 104915 149398
                // 3 021134 198948
                // itd.
                // =========================================================

                var pozycje = new List<(int Index, string Lp,
                    string NumerKatalogowy, string Typ, string JednostakaMiary)>();


                for (int i = 0; i < tabela.Count; i++)
                {
                    string line = tabela[i];

                    var match = Regex.Match(
                        line,
                        @"(?:^|\s)(\d+)\s+(\d{3,})\s+(\d{3,})(?:\s|$)");

                    if (match.Success)
                    {
                        string lp = match.Groups[1].Value;
                        string numerKatalogowy = match.Groups[2].Value;
                        string typ = match.Groups[3].Value;
                        string jednostakaMiary = match.Groups[4].Value;
                        // Dodatkowe zabezpieczenie:
                        // pozycja musi mieć LP 1,2,3,4,5...
                        if (int.TryParse(lp, out int numerLp))
                        {
                            pozycje.Add((
                                i,
                                lp,
                                numerKatalogowy,
                                typ,
                                jednostakaMiary));

                            Console.WriteLine(
                                $"Znaleziono początek pozycji: " +
                                $"{lp} | {numerKatalogowy} | {typ}");
                        }
                    }
                }


                // =========================================================
                // PARSOWANIE POSZCZEGÓLNYCH POZYCJI
                // =========================================================

                for (int p = 0; p < pozycje.Count; p++)
                {
                    try
                    {
                        int start = pozycje[p].Index;

                        int end;

                        if (p + 1 < pozycje.Count)
                        {
                            end = pozycje[p + 1].Index;
                        }
                        else
                        {
                            end = tabela.Count;
                        }


                        // -------------------------------------------------
                        // Tworzymy blok jednej pozycji
                        // -------------------------------------------------

                        var blokLinii = tabela
                            .Skip(start)
                            .Take(end - start)
                            .ToList();


                        var element =
                            ParsujPozycje(
                                blokLinii,
                                pozycje[p].Lp,
                                pozycje[p].NumerKatalogowy,
                                pozycje[p].Typ,
                                pozycje[p].JednostakaMiary);


                        if (element != null)
                        {
                            zestawienie.ListaElementow.Add(element);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Błąd parsowania pozycji {p + 1}: " +
                            ex.Message);
                    }
                }


                Console.WriteLine(
                    $"==========================================");

                Console.WriteLine(
                    $"Oferta PDF: znaleziono " +
                    $"{zestawienie.ListaElementow.Count} pozycji.");

                Console.WriteLine(
                    $"==========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Błąd parsowania oferty PDF: {ex.Message}");
            }

            return zestawienie;
        }


        // =============================================================
        // PARSOWANIE JEDNEJ POZYCJI
        // =============================================================

        private ZestawienieElementZamowieniaListItem? ParsujPozycje(
            List<string> blokLinii,
            string lp,
            string numerKatalogowy,
            string typ,
            string jednostakaMiary)
        {
            if (blokLinii == null || blokLinii.Count == 0)
                return null;


            // ---------------------------------------------------------
            // Łączymy cały blok w jeden tekst.
            // ---------------------------------------------------------

            string tekst =
                string.Join(
                    " ",
                    blokLinii);


            // ---------------------------------------------------------
            // Usuwamy wielokrotne spacje
            // ---------------------------------------------------------

            tekst = Regex.Replace(
                tekst,
                @"\s+",
                " ").Trim();


            Console.WriteLine(
                $"------------------------------------------");

            Console.WriteLine(
                $"Parsowanie pozycji {lp}");

            Console.WriteLine(
                $"BLok: {tekst}");


            // ---------------------------------------------------------
            // Tworzymy obiekt
            // ---------------------------------------------------------

            var element =
                new ZestawienieElementZamowieniaListItem
                {
                    Lp = lp,

                    NumerKatalogowy =
                        numerKatalogowy,

                    Typ = typ,

                    Szerokosc = "0",

                    Wysokosc = "0",

                    JednostakaMiary = jednostakaMiary
                };


            // =========================================================
            // ZNAJDUJEMY ILOŚĆ
            //
            // Szukamy:
            //
            // 4,00 SZT
            // 1,00 SZT
            // =========================================================

            var iloscMatch = Regex.Match(
                tekst,
                @"(\d+(?:[,.]\d+)?)\s+SZT\b",
                RegexOptions.IgnoreCase);


            if (!iloscMatch.Success)
            {
                Console.WriteLine(
                    $"Nie znaleziono ilości dla pozycji {lp}");

                return null;
            }


            // ---------------------------------------------------------
            // ILOŚĆ
            // ---------------------------------------------------------

            string iloscText =
                iloscMatch.Groups[1].Value
                    .Replace(",", ".");


            if (decimal.TryParse(
                iloscText,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal ilosc))
            {
                element.IloscSztuk =
                    ((int)ilosc).ToString();
            }
            else
            {
                element.IloscSztuk =
                    iloscText;
            }


            // ---------------------------------------------------------
            // JEDNOSTKA / UWAGI
            // ---------------------------------------------------------

            string jednostkaText =
            iloscMatch.Groups[0].Value.Substring(iloscMatch.Groups[0].Value.Length - 3, 3);

            element.JednostakaMiary = jednostkaText.Trim();

            element.Uwagi = $"Zaimportowano dnia: {DateTime.Now:yyyy-MM-dd}";

            // =========================================================
            // NAZWA PRODUKTU
            //
            // Wycinamy:
            //
            // Lp
            // Numer katalogowy
            // Typ
            //
            // aż do ilości.
            // =========================================================

            int startOpis =
                tekst.IndexOf(
                    typ,
                    StringComparison.Ordinal);

            if (startOpis >= 0)
            {
                startOpis += typ.Length;

                int koniecOpis =
                    iloscMatch.Index;

                if (koniecOpis > startOpis)
                {
                    string nazwa =
                        tekst.Substring(
                            startOpis,
                            koniecOpis - startOpis)
                        .Trim();

                    element.NazwaProduktu =
                        nazwa;
                }
            }


            // =========================================================
            // CENA NETTO
            //
            // Po ilości mamy np.:
            //
            // SZT 2,67 23% 10,16 12,50
            //
            // więc:
            //
            // 2,67  = cena jednostkowa
            // 23%   = VAT
            // 10,16 = netto
            // 12,50 = brutto
            // =========================================================

            string poIlosci = tekst
             .Substring(iloscMatch.Index + iloscMatch.Length)
             .Trim();


            var cenaMatch = Regex.Match(
                poIlosci,
                @"(\d+[,.]\d+)\s+(\d+)%\s+(\d+[,.]\d+)\s+(\d+[,.]\d+)",
                RegexOptions.IgnoreCase);


            if (cenaMatch.Success)
            {
                element.CenaNetto =
                    cenaMatch.Groups[1].Value;
            }


            // =========================================================
            // DEBUG
            // =========================================================

            Console.WriteLine(
                $"LP             : {element.Lp}");

            Console.WriteLine(
                $"NumerKatalogowy: {element.NumerKatalogowy}");

            Console.WriteLine(
                $"Typ            : {element.Typ}");

            Console.WriteLine(
                $"NazwaProduktu  : {element.NazwaProduktu}");

            Console.WriteLine(
                $"IloscSztuk     : {element.IloscSztuk}");

            Console.WriteLine(
             $"JednostakaMiary : {element.JednostakaMiary}");

            Console.WriteLine(
                $"Uwagi          : {element.Uwagi}");

            Console.WriteLine(
                $"CenaNetto      : {element.CenaNetto}");


            return element;
        }

    }
}