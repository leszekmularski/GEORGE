using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEORGE.Shared.Models
{
    public class KonfPolaczenie
    {
        public int Id { get; set; }
        public Guid RowId { get; set; } = Guid.NewGuid();
        public Guid RowIdSystem { get; set; } = Guid.NewGuid();
        public Guid RowIdModelu { get; set; } = Guid.NewGuid();
        // Element zewnętrzny np. rama
        public Guid ElementZewnetrznyId { get; set; }
        // Element wewnętrzny np. skrzydło
        public Guid ElementWewnetrznyId { get; set; }
        // Strona połączenia (np. lewa, prawa, góra, dół)
        public string? StronaPolaczenia { get; set; }
        // wymiary techniczne w mm odległość pomiędzy liniami dolnymi
        public double PrzesuniecieX { get; set; } = 0;
        public double PrzesuniecieY { get; set; } = 0;
        public int ZapisanyKat { get; set; } = 0; //Kąt zapisywany w stopniach położenia elemnty 2 względem elementu 1
        // Opcjonalne warunki połączenia
        public int? KatOd { get; set; }
        public int? KatDo { get; set; }
        public string? DodatkowyWarunek { get; set; } 
        public string? OpisPolaczenia { get; set; }
        public string? Uwagi { get; set; }
        public byte[] RysunekPrzekroju { get; set; } = Array.Empty<byte>();

        [NotMapped]
        public KonfSystem? ElementWewnetrzny { get; set; }

        [NotMapped]
        public KonfSystem? ElementZewnetrzny { get; set; }
    }

}
