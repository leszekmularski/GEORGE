using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;
using GEORGE.Client.Pages.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GEORGE.Client.Pages.Models
{
    public class ShapeRegion
    {
        /// <summary>
        /// Wierzchołki definiujące kształt regionu (np. prostokąt, trójkąt).
        /// </summary>
        public List<XPoint> Wierzcholki { get; set; } = new();

        /// <summary>
        /// Typ geometryczny regionu (np. prostokąt, trójkąt, trapez, inny).
        /// </summary>
        public string TypKsztaltu { get; set; } = "nieokreślony";

        /// <summary>
        /// Dotyczy czy podział dotyczy ramy, skrzydła czy poprzeczek w skrzydle
        /// </summary>
        public string TypLiniiDzielacej { get; set; } = "";

        public bool Rama { get; set; } = false;

        /// <summary>
        /// Linie, które brały udział w podziale tego regionu (jeśli dotyczy).
        /// </summary>
        public List<XLineShape> LinieDzielace { get; set; } = new();

        /// <summary>
        /// Identyfikator regionu (opcjonalny).
        /// </summary>
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        public string? IdMaster { get; set; } = "";

        public string? IdRegionuPonizej { get; set; } = "WARSTWA-ZERO";

        /// <summary>
        /// Zwraca prostokąt ograniczający (bounding box).
        /// </summary>
        public BoundingBox GetBoundingBox()
        {
            var minX = Wierzcholki.Min(p => p.X);
            var minY = Wierzcholki.Min(p => p.Y);
            var maxX = Wierzcholki.Max(p => p.X);
            var maxY = Wierzcholki.Max(p => p.Y);
           // Console.WriteLine($"GetBoundingBox --> BoundingBox: minX={minX}, minY={minY}, maxX={maxX}, maxY={maxY}");
            return new BoundingBox(minX, minY, maxX - minX, maxY - minY, TypKsztaltu);
        }

        /// <summary>
        /// Sprawdza, czy punkt znajduje się wewnątrz regionu (prosty test bounding box).
        /// </summary>
        public bool ContainsPoint(XPoint point)
        {
            var bbox = GetBoundingBox();
            return point.X >= bbox.X && point.X <= bbox.X + bbox.Width &&
                   point.Y >= bbox.Y && point.Y <= bbox.Y + bbox.Height;
        }

        /// <summary>
        /// Automatyczne rozpoznanie typu kształtu na podstawie liczby wierzchołków z uwzględnieniem typu domyślnego.
        /// </summary>
        public void RozpoznajTyp(string typDomyslny)
        {
            // Słownik mapujący nazwy kształtów na oczekiwaną liczbę wierzchołków
            var oczekiwaneWierzcholki = new Dictionary<string, int>
            {
                { "Linia", 2 },
                { "Trójkąt", 3 },
                { "Trapezoid", 4 },
                { "Domek", 5 },
                { "niestandardowy", -1 } // -1 oznacza dowolną liczbę wierzchołków
            };

            // Sprawdź czy typ domyślny jest w słowniku i czy liczba wierzchołków pasuje
            if (oczekiwaneWierzcholki.TryGetValue(typDomyslny, out int oczekiwana) &&
                (oczekiwana == Wierzcholki.Count || oczekiwana == -1))
            {
                TypKsztaltu = typDomyslny;
            }
            else
            {
                // Automatyczne rozpoznanie gdy typ domyślny nie pasuje
                TypKsztaltu = Wierzcholki.Count switch
                {
                    2 => "Linia",
                    3 => "Trójkąt",
                    4 => "Trapezoid",
                    5 => "Domek",
                    _ => "niestandardowy"
                };
            }
        }

    }
}
