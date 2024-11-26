public class UprawnieniaPracownikaViewModel
{
    public long Id { get; set; }
    public bool Odczyt { get; set; }
    public bool Zapis { get; set; }
    public bool Zmiana { get; set; }
    public bool Usuniecie { get; set; }
    public bool Administrator { get; set; }
    public string? TableName { get; set; }
    public string? RowId { get; set; }
    public string? RowIdPracownicy { get; set; }
    public string? RowIdRejestrejestrow { get; set; }
    public string? RowIdDzialu { get; set; }
    public string? Imie { get; set; }
    public string? Nazwisko { get; set; }
    public string? UzytkownikSQL { get; set; }
    public string? HasloSQL { get; set; }
    public string? Adres { get; set; }
    public string? Email { get; set; }
    public string? HasloDoPoczty { get; set; }
    public string? KodPocztowy { get; set; }
    public string? Miejscowosc { get; set; }
    public string? Uwagi { get; set; }
    public string? Telefon { get; set; }
    public DateTime Datautowrzenia { get; set; }
    public string? StanowiskoSystem { get; set; }

}
