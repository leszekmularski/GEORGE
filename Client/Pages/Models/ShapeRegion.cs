﻿using GEORGE.Client.Pages.KonfiguratorOkien;
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
        public List<PointDC> Wierzcholki { get; set; } = new();

        /// <summary>
        /// Typ geometryczny regionu (np. prostokąt, trójkąt, trapez, inny).
        /// </summary>
        public string TypKształtu { get; set; } = "nieokreślony";

        /// <summary>
        /// Linie, które brały udział w podziale tego regionu (jeśli dotyczy).
        /// </summary>
        public List<XLineShape> LinieDzielace { get; set; } = new();

        /// <summary>
        /// Identyfikator regionu (opcjonalny).
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Zwraca prostokąt ograniczający (bounding box).
        /// </summary>
        public BoundingBox GetBoundingBox()
        {
            var minX = Wierzcholki.Min(p => p.X);
            var minY = Wierzcholki.Min(p => p.Y);
            var maxX = Wierzcholki.Max(p => p.X);
            var maxY = Wierzcholki.Max(p => p.Y);

            return new BoundingBox(minX, minY, maxX - minX, maxY - minY, TypKształtu);
        }

        /// <summary>
        /// Sprawdza, czy punkt znajduje się wewnątrz regionu (prosty test bounding box).
        /// </summary>
        public bool ContainsPoint(PointDC point)
        {
            var bbox = GetBoundingBox();
            return point.X >= bbox.X && point.X <= bbox.X + bbox.Width &&
                   point.Y >= bbox.Y && point.Y <= bbox.Y + bbox.Height;
        }

        /// <summary>
        /// Automatyczne rozpoznanie typu kształtu na podstawie liczby wierzchołków.
        /// </summary>
        public void RozpoznajTyp()
        {
            TypKształtu = Wierzcholki.Count switch
            {
                3 => "trójkąt",
                4 => "prostokąt",
                5 => "trapez",
                _ => "niestandardowy"
            };
        }

    }
}
