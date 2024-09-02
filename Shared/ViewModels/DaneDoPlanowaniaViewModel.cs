using GEORGE.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEORGE.Shared.ViewModels
{
    public class DaneDoPlanowaniaViewModel
    {
        public DateTime PlanowanaDataRozpoczeciaProdukcji { get; set; }
        public DateTime RzeczywistaDataRozpoczeciaProdukcji { get; set; } = DateTime.MinValue;
        public ZleceniaProdukcyjneDto? ZleceniaProdukcyjneDto { get; set; }
        public string? Wyrob { get; set; }
        public int DomyslnyCzasProdukcji{ get; set; } = 24;//  Wartość domyślna
        public float JednostkiNaZlecenie { get; set; }
        public string? NumerZlecenia { get; set; }
        public string? RowIdLiniiProdukcyjnej { get; set; }
        public bool ZlecenieWewnetrzne { get; set; } = false;
        public string? TypZamowienia { get; set; }
        public int ProcentWykonania { get; set; } = 0;//  Wartość domyślna
    }

}
