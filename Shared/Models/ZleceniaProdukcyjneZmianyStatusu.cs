﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GEORGE.Shared.Models
{
    public class ZleceniaProdukcyjneZmianyStatusu
    {
        public long Id { get; set; }
        [Required]
        public string? RowId { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string? RowIdZlecenia { get; set; }
        [Required]
        public string? TypZamowienia { get; set; }
        public string? NumerZamowienia { get; set; }
        public string? NumerUmowy { get; set; }
        public DateTime DataProdukcji { get; set; } = DateTime.MinValue;
        public DateTime DataWysylki { get; set; } = DateTime.MinValue;
        public DateTime DataMontazu { get; set; } = DateTime.MinValue;
        public string? Klient { get; set; }
        public string? Adres { get; set; }
        public string? Miejscowosc { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? NazwaProduktu { get; set; }
        public string? NazwaProduktu2 { get; set; }
        public string? KodProduktu { get; set; }
        public int Ilosc { get; set; }
        public Single Wartosc { get; set; }
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public DateTime DataGotowosci { get; set; } = DateTime.MinValue;
        public string? KtoZapisal { get; set; } //= User.Identity.Name;
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
        public string[]? Tags { get; set; }
        public float JednostkiNaZlecenie { get; set; } = 0;
        public bool ZlecenieWewnatrzne { get; set; } = false;
        public DateTime DataRozpProdukcji { get; set; } = DateTime.MinValue;
        public string? NumerZlecenia { get; set; }

        [NotMapped]
        public string TagsString
        {
            get => Tags == null ? string.Empty : string.Join(", ", Tags);
            set => Tags = string.IsNullOrWhiteSpace(value) ? null : value.Split(',').Select(tag => tag.Trim()).ToArray();
        }
    }
}
