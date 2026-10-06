using ClosedXML.Excel;

namespace GEORGE.Client.Pages.PDF
{
    public class ExcelDataParser
    {
        public Zestawienie ParseExcelData(Stream excelStream)
        {
            var zestawienie = new Zestawienie();

            try
            {
                using var workbook = new XLWorkbook(excelStream);

                var worksheet = workbook.Worksheet("Zamówienie");

                // ---------------------------------------------------------
                // ODBIORCA - drugi wiersz, pierwsza kolumna
                // ---------------------------------------------------------
                var odbiorca = worksheet.Cell(2, 1).GetString().Trim();

                zestawienie.Odbiorca = odbiorca;

                // Excel nie posiada w tym pliku numeru zestawienia
                zestawienie.NrZestawienia = "";

                // Data importu
                zestawienie.Data = DateTime.Now;

                // ---------------------------------------------------------
                // NAGŁÓWKI
                // ---------------------------------------------------------
                // Wiersz 1:
                // A = Gatunek kantówki
                // B = Przekrój
                // C = Nazwa produktu
                // D = Długość zamówiona
                // E = Ilość sztuk
                // ---------------------------------------------------------

                int ostatniWiersz = worksheet.LastRowUsed()?.RowNumber() ?? 0;

                for (int row = 3; row <= ostatniWiersz; row++)
                {
                    try
                    {
                        string gatunek = worksheet.Cell(row, 1).GetString().Trim();
                        string przekroj = worksheet.Cell(row, 2).GetString().Trim();
                        string nazwaProduktu = worksheet.Cell(row, 3).GetString().Trim();

                        // Długość
                        string dlugosc = "";

                        if (worksheet.Cell(row, 4).TryGetValue<double>(out double dlugoscDouble))
                        {
                            dlugosc = ((int)dlugoscDouble).ToString();
                        }
                        else
                        {
                            dlugosc = worksheet.Cell(row, 4).GetString().Trim();
                        }

                        // Ilość
                        string ilosc = "";

                        if (worksheet.Cell(row, 5).TryGetValue<double>(out double iloscDouble))
                        {
                            ilosc = ((int)iloscDouble).ToString();
                        }
                        else
                        {
                            ilosc = worksheet.Cell(row, 5).GetString().Trim();
                        }

                        // Puste wiersze pomijamy
                        if (string.IsNullOrWhiteSpace(gatunek) &&
                            string.IsNullOrWhiteSpace(przekroj) &&
                            string.IsNullOrWhiteSpace(nazwaProduktu) &&
                            string.IsNullOrWhiteSpace(dlugosc) &&
                            string.IsNullOrWhiteSpace(ilosc))
                        {
                            continue;
                        }

                        // Sprawdzamy podstawowe dane
                        if (!int.TryParse(ilosc, out _))
                        {
                            Console.WriteLine(
                                $"Excel: pominięto wiersz {row} - niepoprawna ilość: '{ilosc}'");

                            continue;
                        }

                        if (!int.TryParse(dlugosc, out _))
                        {
                            Console.WriteLine(
                                $"Excel: pominięto wiersz {row} - niepoprawna długość: '{dlugosc}'");

                            continue;
                        }

                        // ---------------------------------------------------------
                        // Tworzymy CutListItem dokładnie w formacie używanym
                        // przez Twój SaveAllGrupa()
                        // ---------------------------------------------------------
                        var cutItem = new CutListItem
                        {
                            Lp = (row - 2).ToString(),

                            // Nazwa produktu
                            Symbol = nazwaProduktu,

                            // Przekrój
                            Kolor = przekroj,

                            // Ilość sztuk
                            Ilosc = ilosc,

                            // Długość na gotowo
                            Wymiar = dlugosc,

                            // Excel nie zawiera kąta
                            Kat = "",

                            // Długość handlowa
                            WymiarNaZamowienie = DlugoscHandlowa(dlugosc),

                            // Gatunek kantówki
                            // SaveAllGrupa() wykorzystuje właśnie Uwagi
                            // jako GatunekKantowki
                            Uwagi = gatunek
                        };

                        zestawienie.ListaCieci.Add(cutItem);

                        Console.WriteLine(
                            $"Excel [{row}]: " +
                            $"Gatunek={gatunek}, " +
                            $"Przekrój={przekroj}, " +
                            $"Produkt={nazwaProduktu}, " +
                            $"Długość={dlugosc}, " +
                            $"Ilość={ilosc}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Błąd podczas parsowania wiersza Excel {row}: {ex.Message}");
                    }
                }

                Console.WriteLine(
                    $"Excel: odczytano {zestawienie.ListaCieci.Count} pozycji.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Błąd podczas otwierania pliku Excel: {ex.Message}");

                throw;
            }

            return zestawienie;
        }


        public string DlugoscHandlowa(string dlugoscWyliczona)
        {
            if (!int.TryParse(dlugoscWyliczona, out int dlugoscX))
            {
                return "0";
            }

            int dlugoscKrok = 100;

            int zaokraglonaDlugosc =
                (int)Math.Ceiling(
                    (double)dlugoscX / dlugoscKrok) * dlugoscKrok;

            int roznica =
                zaokraglonaDlugosc - dlugoscX;

            if (roznica < 20)
            {
                zaokraglonaDlugosc += dlugoscKrok;
            }

            return zaokraglonaDlugosc.ToString();
        }
    }
}