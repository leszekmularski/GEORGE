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
        public ZleceniaProdukcyjne? ZleceniaProdukcyjne { get; set; }
        public string? Wyrob { get; set; }
        public int DomyslnyCzasProdukcji{ get; set; } = 350;//  14 dni
        public string? NumerZlecenia { get; set; }
        public string? RowIdLiniiProdukcyjnej { get; set; }
    }

}
