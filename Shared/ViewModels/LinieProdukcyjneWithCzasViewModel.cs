namespace GEORGE.Shared.ViewModels
{
    public class LinieProdukcyjneWithCzasViewModel
    {
        public long Id { get; set; }
        public string? RowIdLinieProdukcyjne { get; set; }
        public string? RowIdZlecnia { get; set; }
        public string? IdLiniiProdukcyjnej { get; set; }
        public string? NazwaLiniiProdukcyjnej { get; set; }
        public int DziennaZdolnoscProdukcyjna { get; set; }
        public string? Uwagi { get; set; }
        public string? KtoZapisal { get; set; }
        public string? OstatniaZmiana { get; set; }
        public int CzasNaZlecenie { get; set; } = 0;
    }

}
