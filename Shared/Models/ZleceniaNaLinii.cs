﻿using System.ComponentModel.DataAnnotations;

namespace GEORGE.Shared.Models
{
    public class ZleceniaNaLinii
    {
        public long Id { get; set; }
        public string? RowId { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string? RowIdLinieProdukcyjne { get; set; }
        [Required]
        public string? RowIdZleceniaProdukcyjne { get; set; }
        public string? Uwagi { get; set; }
        public bool ZlecenieWewnetrzne { get; set; } = false;
        public DateTime DataRozpProdukcjiNaLinii { get; set; } = DateTime.MinValue;
        public DateTime DataZapisu { get; set; } = DateTime.Now;
        public string? OstatniaZmiana { get; set; } = "Zmiana: " + DateTime.Now.ToLongDateString();
    }
}
