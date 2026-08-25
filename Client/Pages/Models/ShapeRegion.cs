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
        /// Wierzchołki definiujące kształt regionu (np. prostokąt, trójkąt). Tylko linie
        /// </summary>
        public List<XPoint> Wierzcholki { get; set; } = new();
        public List<XPoint>? WierzcholkiWenetrznePodRysunek { get; set; } = new();
        /// <summary>
        /// Wierzchołki definiujące kształt regionu (np. prostokąt, trójkąt, okręgi, łuki). Linie i łuki
        /// </summary>
        public List<ContourSegment> Kontur { get; set; } = new();
        public List<ContourSegment>? KonturWenetrznyPodRysunek { get; set; } = new();

        /// <summary>
        /// Typ geometryczny regionu (np. prostokąt, trójkąt, trapez, inny).
        /// </summary>
        public string TypKsztaltu { get; set; } = "nieokreślony";

        /// <summary>
        /// Dotyczy czy podział dotyczy ramy, skrzydła czy poprzeczek w skrzydle
        /// </summary>
        public string TypLiniiDzielacej { get; set; } = "BRAK";

        public bool Rama { get; set; } = false;

        /// <summary>
        /// Linie, które brały udział w podziale tego regionu (jeśli dotyczy).
        /// </summary>
        public List<XLineShape> LinieDzielace { get; set; } = new();

        /// <summary>
        /// Identyfikator regionu (opcjonalny).
        /// </summary>
        public string? Id { get; set; }
        public string? IdMaster { get; set; } = "";

        public string? IdRegionuPonizej { get; set; } = "WARSTWA-ZERO";

        // 🔥 NOWE WŁAŚCIWOŚCI DO POBRANIA KĄTA LINII

        /// <summary>
        /// Kąt linii regionu w stopniach (0-180°), tylko dla regionów typu "Linia"
        /// </summary>
        public double KatLinii
        {
            get
            {
                // Sprawdź czy region jest typu "Linia" i ma dokładnie 2 wierzchołki
                if (TypKsztaltu != "Linia" || Wierzcholki == null || Wierzcholki.Count < 2)
                {
                    return 0; // lub możesz rzucić wyjątek, w zależności od potrzeb
                }

                double x1 = Wierzcholki[0].X;
                double y1 = Wierzcholki[0].Y;
                double x2 = Wierzcholki[1].X;
                double y2 = Wierzcholki[1].Y;

                double dx = x2 - x1;
                double dy = y2 - y1;

                double katRadiany = Math.Atan2(dy, dx);
                double katStopnie = katRadiany * (180.0 / Math.PI);

                // Normalizacja do zakresu 0-180°
                if (katStopnie < 0)
                {
                    katStopnie += 180;
                }
                else if (katStopnie >= 180)
                {
                    katStopnie -= 180;
                }

                return katStopnie;
            }
        }

        /// <summary>
        /// Kąt linii regionu w radianach (0-π), tylko dla regionów typu "Linia"
        /// </summary>
        public double KatLiniiRadiany
        {
            get
            {
                if (TypKsztaltu != "Linia" || Wierzcholki == null || Wierzcholki.Count < 2)
                {
                    return 0;
                }

                double dx = Wierzcholki[1].X - Wierzcholki[0].X;
                double dy = Wierzcholki[1].Y - Wierzcholki[0].Y;

                double katRadiany = Math.Atan2(dy, dx);

                // Normalizacja do zakresu 0-π
                if (katRadiany < 0)
                {
                    katRadiany += Math.PI;
                }
                else if (katRadiany >= Math.PI)
                {
                    katRadiany -= Math.PI;
                }

                return katRadiany;
            }
        }

        /// <summary>
        /// Pełny kąt linii w stopniach (0-360°), uwzględniający kierunek
        /// </summary>
        public double KatLiniiPelny
        {
            get
            {
                if (TypKsztaltu != "Linia" || Wierzcholki == null || Wierzcholki.Count < 2)
                {
                    return 0;
                }

                double dx = Wierzcholki[1].X - Wierzcholki[0].X;
                double dy = Wierzcholki[1].Y - Wierzcholki[0].Y;

                double katRadiany = Math.Atan2(dy, dx);
                double katStopnie = katRadiany * (180.0 / Math.PI);

                // Normalizacja do zakresu 0-360°
                if (katStopnie < 0)
                {
                    katStopnie += 360;
                }

                return katStopnie;
            }
        }

        /// <summary>
        /// Sprawdza czy region jest typu "Linia"
        /// </summary>
        public bool CzyLinia => TypKsztaltu == "Linia" && Wierzcholki?.Count == 2;

        /// <summary>
        /// Długość linii (tylko dla regionów typu "Linia")
        /// </summary>
        public double DlugoscLinii
        {
            get
            {
                if (!CzyLinia)
                {
                    return 0;
                }

                double dx = Wierzcholki[1].X - Wierzcholki[0].X;
                double dy = Wierzcholki[1].Y - Wierzcholki[0].Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }
        }

        /// <summary>
        /// Tworzy głęboką kopię obiektu ShapeRegion
        /// </summary>
        public ShapeRegion Clone()
        {
            return new ShapeRegion
            {
                // Kopiuj listę wierzchołków (głęboka kopia)
                Wierzcholki = this.Wierzcholki?.Select(p => new XPoint(p.X, p.Y)).ToList() ?? new List<XPoint>(),
                WierzcholkiWenetrznePodRysunek = this.WierzcholkiWenetrznePodRysunek?.Select(p => new XPoint(p.X, p.Y)).ToList() ?? new List<XPoint>(),

                // Kopiuj kontur (głęboka kopia segmentów)
                Kontur = this.Kontur?.Select(seg =>
                {
                    if (seg.Type == SegmentType.Arc && seg.Center.HasValue)
                    {
                        return new ContourSegment(
                            new XPoint(seg.Start.X, seg.Start.Y),
                            new XPoint(seg.End.X, seg.End.Y),
                            new XPoint(seg.Center.Value.X, seg.Center.Value.Y),
                            seg.Radius,
                            seg.CounterClockwise
                        );
                    }
                    else
                    {
                        return new ContourSegment(
                            new XPoint(seg.Start.X, seg.Start.Y),
                            new XPoint(seg.End.X, seg.End.Y)
                        );
                    }
                }).ToList() ?? new List<ContourSegment>(),

                KonturWenetrznyPodRysunek = this.KonturWenetrznyPodRysunek?.Select(seg =>
                {
                    if (seg.Type == SegmentType.Arc && seg.Center.HasValue)
                    {
                        return new ContourSegment(
                            new XPoint(seg.Start.X, seg.Start.Y),
                            new XPoint(seg.End.X, seg.End.Y),
                            new XPoint(seg.Center.Value.X, seg.Center.Value.Y),
                            seg.Radius,
                            seg.CounterClockwise
                        );
                    }
                    else
                    {
                        return new ContourSegment(
                            new XPoint(seg.Start.X, seg.Start.Y),
                            new XPoint(seg.End.X, seg.End.Y)
                        );
                    }
                }).ToList() ?? new List<ContourSegment>(),

                // Kopiuj proste właściwości
                TypKsztaltu = this.TypKsztaltu,
                TypLiniiDzielacej = this.TypLiniiDzielacej,
                Rama = this.Rama,

                // Kopiuj listę linii dzielących
                LinieDzielace = this.LinieDzielace?.ToList() ?? new List<XLineShape>(),

                // Kopiuj identyfikatory
                Id = this.Id,
                IdMaster = this.IdMaster,
                IdRegionuPonizej = this.IdRegionuPonizej,
                BoundingBox = this.BoundingBox
            };
        }

        /// <summary>
        /// Zwraca prostokąt ograniczający (bounding box).
        /// </summary>
        public BoundingBox GetBoundingBox()
        {
            if (Wierzcholki == null || Wierzcholki.Count == 0)
            {
                // Zwracamy neutralny bounding box, zamiast crashować
                return new BoundingBox(0, 0, 0, 0, TypKsztaltu);
            }

            var minX = Wierzcholki.Min(p => p.X);
            var minY = Wierzcholki.Min(p => p.Y);
            var maxX = Wierzcholki.Max(p => p.X);
            var maxY = Wierzcholki.Max(p => p.Y);

            return new BoundingBox(minX, minY, maxX - minX, maxY - minY, TypKsztaltu);
        }

        public BoundingBox? BoundingBox { get; set; }

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
                { "Punkt", 1 },
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
                    1 => "Punkt",
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