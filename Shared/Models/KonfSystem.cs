using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        public string? Typ { get; set; } // Typ- Rama, skrzydło, słupek ruchomy, słupek stały
        public bool CzyMozeBycFix { get; set; } = false;
        public bool WystepujeDol { get; set; } = false;
        public bool WystepujeLewa { get; set; } = false;
        public bool WystepujeGora { get; set; } = false;
        public bool WystepujePrawa { get; set; } = false;
        public int? KatWystapieniaZakresOdMin { get; set; } = 0;
        public int? KatWystapieniaZakresOdMax { get; set; } = 360;
        public int? ZakresStosDlugoscOdMin { get; set; } = 0;
        public int? ZakresStosDlugoscOdMax { get; set; } = 6500;
        public string? DodatkowyFiltrWystepowania { get; set; }
        public string? Uwagi { get; set; }
        public string? SVG { get; set; }
        public double? Cena1MB { get; set; }
        public double? Waga { get; set; }
        public int? WymiarXKantowki1 { get; set; }
        public int? WymiarYKantowki1 { get; set; }
        public double? Cena1MBKantowki1 { get; set; }
        public double? WagaKantowki1 { get; set; }
        public double? DlugoscKantowki1 { get; set; }
        public double? IloscSztukKantowki1 { get; set; }
        public int? WymiarXKantowki2 { get; set; }
        public int? WymiarYKantowki2 { get; set; }
        public double? Cena1MBKantowki2 { get; set; }
        public double? WagaKantowki2 { get; set; }
        public double? DlugoscKantowki2 { get; set; }
        public double? IloscSztukKantowki2 { get; set; }
        public int? WymiarXKantowki3 { get; set; }
        public int? WymiarYKantowki3 { get; set; }
        public double? Cena1MBKantowki3 { get; set; }
        public double? WagaKantowki3 { get; set; }
        public double? DlugoscKantowki3 { get; set; }
        public double? IloscSztukKantowki3 { get; set; }
        public int? WymiarXKantowki4 { get; set; }
        public int? WymiarYKantowki4 { get; set; }
        public double? Cena1MBKantowki4 { get; set; }
        public double? WagaKantowki4 { get; set; }
        public double? DlugoscKantowki4 { get; set; }
        public double? IloscSztukKantowki4 { get; set; }
        public int? WymiarXKantowki5 { get; set; }
        public int? WymiarYKantowki5 { get; set; }
        public double? Cena1MBKantowki5 { get; set; }
        public double? WagaKantowki5 { get; set; }
        public double? DlugoscKantowki5 { get; set; }
        public double? IloscSztukKantowki5 { get; set; }

        // Obrazek DXF w bazie
        public byte[] Rysunek { get; set; } = Array.Empty<byte>();

        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? KtoZapisal { get; set; } = "NaN";
    }

}
