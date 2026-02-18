using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// Klasa do przechowywania informacji o przesunięciach elementów, takich jak linie, w bazie danych.
namespace GEORGE.Shared.ViewModels
{
    public class PrzesuniecieDto
    {
        public double PrzesuniecieX { get; set; }
        public double PrzesuniecieY { get; set; }
        public string Strona { get; set; } = string.Empty; // Pobrane z bazy informacje o stronie lewa/prawa/góra/dół
    }
}
