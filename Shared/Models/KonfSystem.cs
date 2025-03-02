using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEORGE.Shared.Models
{
    public class KonfSystem
    {
        public int Id { get; set; }
        public Guid RowId { get; set; } = Guid.NewGuid();
        public Guid RowIdSystem { get; set; }

        // Prowadnice pionowe
        public int? PionLewa { get; set; }
        public int? PionPrawa { get; set; }
        public int? PionOdSzybaOdZew { get; set; }
        public int? PionDodatkowa4 { get; set; }
        public int? PionDodatkowa5 { get; set; }

        // Prowadnice poziome
        public int? PoziomDol { get; set; }
        public int? PoziomGora { get; set; }
        public int? PoziomKorpus { get; set; }
        public int? PoziomLiniaSzkla { get; set; }
        public int? PoziomLiniaOkucia { get; set; }
        public int? PoziomOsDormas { get; set; }
        public int? PoziomDodatkowa6 { get; set; }
        public int? PoziomDodatkowa7 { get; set; }
        public string? Indeks { get; set; }
        public string? Nazwa { get; set; }
        public string? Uwagi { get; set; }
        public string? SVG { get; set; }
        public double? Cena1MB { get; set; }
        public double? Waga { get; set; }
        public int? WymiarXKantowki { get; set; }
        public int? WymiarYKantowki { get; set; }
        public double? Cena1MBKantowki { get; set; }
        public double? WagaKantowki { get; set; }
        // Obrazek DXF w bazie
        public byte[] Rysunek { get; set; } = Array.Empty<byte>();
    }

}
