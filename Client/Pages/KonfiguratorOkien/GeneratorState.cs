using GEORGE.Client.Pages.Models;
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
        // Lista wierzcholkow (w kolejnosci zgodnej z ruchem wskazowek zegara)
        public List<XPoint>? Wierzcholki { get; set; }
        public List<XPoint>? WierzcholkiWartosciNominalne { get; set; }
      //  public List<KonfSystem>? KonfiguracjeSystemu { get; set; }
        public Guid RowIdSystemu { get; set; }
        public Guid? RowIdModelu { get; set; }
        public MVCKonfModele? MVCKonfModelu { get; set; }
        public KonfModele? WybranyModel { get; set; } 
        public string? WybranyKsztalt { get; set; }
        public string? LinieDzielace { get; set; }
        public bool SlupekRuchomyPoLewejStronie { get; set; } = false;
        public bool SlupekRuchomyPoPrawejStronie { get; set; } = false;
        public List<DaneKwadratu>? ListaKwadratow { get; set; }
        public bool ElementLiniowy { get; set; } = false;

        public GeneratorState Clone()
        {
            return new GeneratorState
            {
                Id = this.Id,
                IdRegion = this.IdRegion,
                IdRegionWarstwaNizej = this.IdRegionWarstwaNizej,
                ZIndeks = this.ZIndeks,
                Wierzcholki = this.Wierzcholki?.Select(p => new XPoint(p.X, p.Y)).ToList(),
                WierzcholkiWartosciNominalne = this.WierzcholkiWartosciNominalne?.Select(p => new XPoint(p.X, p.Y)).ToList(),
                RowIdSystemu = this.RowIdSystemu,
                RowIdModelu = this.RowIdModelu,
                MVCKonfModelu = this.MVCKonfModelu, // jeśli ref typ → ewentualnie deep copy
                WybranyModel = this.WybranyModel,   // jw.
                WybranyKsztalt = this.WybranyKsztalt,
                LinieDzielace = this.LinieDzielace,
                SlupekRuchomyPoLewejStronie = this.SlupekRuchomyPoLewejStronie,
                SlupekRuchomyPoPrawejStronie = this.SlupekRuchomyPoPrawejStronie,
                ListaKwadratow = this.ListaKwadratow != null
                    ? this.ListaKwadratow.Select(q => q.Clone()).ToList()
                    : null,
                ElementLiniowy = this.ElementLiniowy
            };
        }

    }

}
