using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEORGE.Shared.Models
{
    public class KonfPolaczenie
    {
        public int Id { get; set; }
        public Guid RowId { get; set; } = Guid.NewGuid();

        // Element zewnętrzny np. rama
        public Guid ElementZewnetrznyId { get; set; }
        public KonfSystem? ElementZewnetrzny { get; set; }

        // Element wewnętrzny np. skrzydło
        public Guid ElementWewnetrznyId { get; set; }
        public KonfSystem? ElementWewnetrzny { get; set; }

        // Strona połączenia (np. lewa, prawa, góra, dół)
        public string? StronaPolaczenia { get; set; }

        // Luz (margines) techniczny w mm
        public double Luz { get; set; }

        // Opcjonalne warunki połączenia
        public int? KatOd { get; set; }
        public int? KatDo { get; set; }
        public string? DodatkowyWarunek { get; set; }
        public string? Uwagi { get; set; }
    }

}
