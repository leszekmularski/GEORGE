namespace GEORGE.Shared.ViewModels
{
    public class DzialPracownikaViewModel
    {
        public long Id { get; set; }
        public string? RowId { get; set; }
        public string? RowIdPracownicy { get; set; }
        public string? Imie { get; set; }
        public string? Nazwisko { get; set; }
        public string? Adres { get; set; }
        public string? KodPocztowy { get; set; }
        public string? Miejscowosc { get; set; }
        public string? Uwagi { get; set; }
        public string? Telefon { get; set; }
        public DateTime Datautowrzenia { get; set; }
        public string? RowIdDzialu { get; set; }
        public string? NazwaDzialu { get; set; }
        public bool Nieaktywny { get; set; }
        public string? ImieNazwisko { get; set; }
        public string? UserSQL { get; set; }

    }
}
