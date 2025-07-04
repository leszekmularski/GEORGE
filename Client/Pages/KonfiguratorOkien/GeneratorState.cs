using GEORGE.Shared.Models;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    class GeneratorState
    {
        public List<KonfSystem>? KonfiguracjeSystemu { get; set; }
        public Guid RowIdSystemu { get; set; }
        public Guid? RowIdModelu { get; set; }
        public MVCKonfModele? PowiazanyModel { get; set; }
        public KonfModele? WybranyModel { get; set; } 
        public string? WybranyKsztalt { get; set; }
        public int GruboscDol { get; set; } = 82;
        public int GruboscGora { get; set; } = 82;
        public int GruboscLewo { get; set; } = 82;
        public int GruboscPrawo { get; set; } = 82;
 
    }

}
