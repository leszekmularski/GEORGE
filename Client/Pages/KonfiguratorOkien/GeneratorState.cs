using GEORGE.Shared.Models;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class GeneratorState
    {
        public string? IdRegion { get; set; }
        public string? IdRegionWarstwaNizej { get; set; }
        public int ZIndeks { get; set; }
        public List<KonfSystem>? KonfiguracjeSystemu { get; set; }
        public Guid RowIdSystemu { get; set; }
        public Guid? RowIdModelu { get; set; }
        public MVCKonfModele? PowiazanyModel { get; set; }
        public KonfModele? WybranyModel { get; set; } 
        public string? WybranyKsztalt { get; set; }
 
    }

}
