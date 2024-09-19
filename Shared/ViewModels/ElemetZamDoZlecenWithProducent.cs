using GEORGE.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEORGE.Shared.ViewModels
{
    public class ElemetZamDoZlecenWithProducent
    {
        public ElemetZamDoZlecen? ElemetZamDoZlecen { get; set; }

        public ProducenciPodwykonawcy? ProducenciPodwykonawcy { get; set; }

        public string? DodatkowaInformacja { get; set; } = "";
    }

}
