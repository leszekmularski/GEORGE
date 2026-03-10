using GEORGE.Client.Pages.Models;
using GEORGE.Client.Pages.Okna;
using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class GeneratorState
    {
        public int Id { get; set; }
        public string? IdRegion { get; set; }
        public string? IdRegionWarstwaNizej { get; set; }
        public int ZIndeks { get; set; }

        //----------------------------------------------------------------------------------------
        public List<XPoint>? Wierzcholki { get; set; }
        public List<XPoint>? WierzcholkiWartosciNominalne { get; set; }
        public List<ContourSegment>? Kontur { get; set; }
        //----------------------------------------------------------------------------------------
        public Guid RowIdSystemu { get; set; }
        public Guid? RowIdModelu { get; set; }
        public MVCKonfModele? MVCKonfModelu { get; set; }
        public KonfModele? WybranyModel { get; set; }
        public string? WybranyKsztalt { get; set; }
        public string? LinieDzielace { get; set; }

        public bool SlupekRuchomyPoLewejStronie { get; set; }
        public bool SlupekRuchomyPoPrawejStronie { get; set; }

        public List<DaneKwadratu>? ListaKwadratow { get; set; }
        public bool ElementLiniowy { get; set; }

        public bool CzyElementJestRama { get; set; } = false;

        public XPoint StaryRegionOrigin { get; set; } = new XPoint(0, 0);

        /// <summary>
        /// Elementy ramy wygenerowane do rysowania
        /// </summary>
        public List<KsztaltElementu> ElementyRamyRysowane { get; set; } = new();

        public Generator? Generator { get; set; }

        public GeneratorState Clone()
        {
            return new GeneratorState
            {
                Id = this.Id,
                IdRegion = this.IdRegion,
                IdRegionWarstwaNizej = this.IdRegionWarstwaNizej,
                ZIndeks = this.ZIndeks,

                Wierzcholki = this.Wierzcholki?
                    .Select(p => new XPoint(p.X, p.Y))
                    .ToList(),

                WierzcholkiWartosciNominalne = this.WierzcholkiWartosciNominalne?
                    .Select(p => new XPoint(p.X, p.Y))
                    .ToList(),

                Kontur = this.Kontur?
                    .Select(c =>
                    {
                        if (c.Type == SegmentType.Arc)
                        {
                            // Użyj konstruktora dla łuku
                            var segement = new ContourSegment(
                                new XPoint(c.Start.X, c.Start.Y),
                                new XPoint(c.End.X, c.End.Y),
                                c.Center.HasValue ? new XPoint(c.Center.Value.X, c.Center.Value.Y) : (XPoint?)null,
                                c.Radius,
                                c.CounterClockwise
                            );

                            segement.Informacja = c.Informacja; // Skopiuj dodatkową informację

                            return segement;

                        }
                        else
                        {
                            // Użyj konstruktora dla linii
                            var segement = new ContourSegment(
                                new XPoint(c.Start.X, c.Start.Y),
                                new XPoint(c.End.X, c.End.Y)
                            );

                            segement.Informacja = c.Informacja; // Skopiuj dodatkową informację

                            return segement;
                        }
                    })
                    .ToList(),

                RowIdSystemu = this.RowIdSystemu,
                RowIdModelu = this.RowIdModelu,
                MVCKonfModelu = this.MVCKonfModelu,
                WybranyModel = this.WybranyModel,
                WybranyKsztalt = this.WybranyKsztalt,
                LinieDzielace = this.LinieDzielace,
                CzyElementJestRama = this.CzyElementJestRama,
                Generator = this.Generator,

                SlupekRuchomyPoLewejStronie = this.SlupekRuchomyPoLewejStronie,
                SlupekRuchomyPoPrawejStronie = this.SlupekRuchomyPoPrawejStronie,

                ListaKwadratow = this.ListaKwadratow?
                    .Select(q => q.Clone())
                    .ToList(),

                ElementLiniowy = this.ElementLiniowy,

                StaryRegionOrigin = new XPoint(this.StaryRegionOrigin.X, this.StaryRegionOrigin.Y),

                ElementyRamyRysowane = this.ElementyRamyRysowane?
                    .Select(e => e.Clone())
                    .ToList() ?? new List<KsztaltElementu>()
            };
        }
    
    }
}
