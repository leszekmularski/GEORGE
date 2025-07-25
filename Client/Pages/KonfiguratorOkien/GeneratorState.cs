using GEORGE.Client.Pages.Models;
using GEORGE.Shared.Models;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class GeneratorState
    {
        public string? IdRegion { get; set; }
        public string? IdRegionWarstwaNizej { get; set; }
        public int ZIndeks { get; set; }
        // Lista wierzcholkow (w kolejnosci zgodnej z ruchem wskazowek zegara)
        public List<XPoint>? Wierzcholki { get; set; }
        public List<XPoint>? WierzcholkiWartosciNominalne { get; set; }
      //  public List<KonfSystem>? KonfiguracjeSystemu { get; set; }
        public Guid RowIdSystemu { get; set; }
        public Guid? RowIdModelu { get; set; }
        public MVCKonfModele? PowiazanyModel { get; set; }
        public KonfModele? WybranyModel { get; set; } 
        public string? WybranyKsztalt { get; set; }
        public string? LinieDzielace { get; set; }


    }

}
