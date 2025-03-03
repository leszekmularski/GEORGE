using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GEORGE.Shared.Models
{
    public class SystemyOkienne
    {
        public int Id { get; set; }
        public Guid RowId { get; set; } = Guid.NewGuid();
        public string? Nazwa_Systemu { get; set; }
        public string? Opis_Systemu { get; set; }
        public string? Uwagi { get; set; }
        public bool Wycofany_z_produkcji { get; set; } = false;
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; } = "NaN";

    }

}
