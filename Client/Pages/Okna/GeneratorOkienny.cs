using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Office2016.Drawing;
using GEORGE.Client.Pages.Models;
using GEORGE.Shared.Models;
using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.WebRequestMethods;

namespace GEORGE.Client.Pages.Okna
{
    public class Generator : GenerujOkno
    {
        public new List<KsztaltElementu> ElementyRamyRysowane { get; set; } = new();
        public List<KonfSystem> KonfiguracjeSystemu { get; set; } = new(); // Dodaj tę linię

        public KonfModele? EdytowanyModel;

        public new MVCKonfModele? PowiazanyModel;


        public Generator()
        {
            // Inicjalizacja domyślnych wartości
            Szerokosc = 1000;
            Wysokosc = 1000;
            KolorZewnetrzny = "#FFFFFF";
            KolorWewnetrzny = "#FFFFFF";
            Waga = 0;
            TypKsztaltu = "prostokąt";
            GruboscSzyby = 24;
            KolorSzyby = "#ADD8E6";
            KonfiguracjeSystemu = new List<KonfSystem>();
            EdytowanyModel = null;
            //RowIdSystemu = Guid.NewGuid();
            //RowIdModelu = Guid.NewGuid();
            PowiazanyModel = null;
        }

        public void AddElements(List<ShapeRegion> regions)
        {
            if (regions == null) return;

            if(KonfiguracjeSystemu == null || PowiazanyModel == null)
            {
                Console.WriteLine($"Brak KonfiguracjeSystemu !!!!");
                return;
            }

            if (EdytowanyModel == null)
            {
                Console.WriteLine($"Brak EdytowanyModel !!!!");
                return;
            }

            Console.WriteLine($"EdytowanyModel.PolaczenieNaroza: {EdytowanyModel.PolaczenieNaroza}");  

            Console.WriteLine($"Szerokosc: {Szerokosc}"); 

            foreach (var region in regions)
            {
                var punkty = region.Wierzcholki;
                if (punkty == null || punkty.Count < 3)
                    continue;

                Console.WriteLine($"GenerujOkno: Przetwarzanie regionu typu: {region.TypKsztaltu}");

                // Wyznacz bounding box
                float minX = (float)punkty.Min(p => p.X);
                float maxX = (float)punkty.Max(p => p.X);
                float minY = (float)punkty.Min(p => p.Y);
                float maxY = (float)punkty.Max(p => p.Y);

                Console.WriteLine($"EdytowanyModel.NazwaKonfiguracji: {EdytowanyModel.NazwaKonfiguracji}");


                // Oblicz rzeczywiste grubości z modelu

                float profileLeft = (float)(PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujeLewa)?.PionPrawa ?? 0 -
                                          PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujeLewa)?.PionLewa ?? 0);
                float profileRight = (float)(PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.PionPrawa ?? 0 -
                                           PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujePrawa)?.PionLewa ?? 0);
                float profileTop = (float)(PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujeGora)?.PionPrawa ?? 0 -
                                         PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujeGora)?.PionLewa ?? 0);
                float profileBottom = (float)(PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujeDol)?.PionPrawa ?? 0 -
                                            PowiazanyModel.KonfSystem.FirstOrDefault(e => e.WystepujeDol)?.PionLewa ?? 0);

                Console.WriteLine($"System ilość konfiguracji: {KonfiguracjeSystemu.Count()} Pobrane grubości profili: profileLeft: {profileLeft} profileRight: {profileRight} profileTop: {profileTop} profileBottom: {profileBottom}");

                // Oblicz wewnętrzny kontur
                var wewnetrznyKontur = CalculateOffsetPolygon(
                    punkty.Select(p => new XPoint(p.X, p.Y)).ToList(),
                    profileLeft, profileRight, profileTop, profileBottom);

                // Generuj elementy ramy w zależności od typu kształtu
                if (region.TypKsztaltu == "prostokąt" || region.TypKsztaltu == "kwadrat")
                {
                    GenerateRectangleElements(punkty, wewnetrznyKontur, profileLeft, profileRight, profileTop, profileBottom, region.TypKsztaltu, EdytowanyModel.PolaczenieNaroza, KonfiguracjeSystemu);
                   
                }
                else if (region.TypKsztaltu == "trójkąt")
                {
                    GenerateTriangleElements(punkty, wewnetrznyKontur, profileLeft, profileRight, profileTop, profileBottom);
                }
                else
                {
                    //GenerateGenericElements(punkty, wewnetrznyKontur);
                    GenerateGenericElementsWithJoins(punkty, wewnetrznyKontur, profileLeft, profileRight, profileTop, profileBottom, region.TypKsztaltu, EdytowanyModel.PolaczenieNaroza, KonfiguracjeSystemu);
                }
            }

            //0 - Lewy górny
            //1 - Prawy górny
            //2 - Prawy dolny
            //3 - Lewy dolny

        }
        private void GenerateRectangleElements(
            List<XPoint> outer, List<XPoint> inner,
            float profileLeft, float profileRight, float profileTop, float profileBottom,
            string typKsztalt, string polaczenia, List<KonfSystem> model)
        {
            float minX = outer.Min(p => (float)p.X);
            float maxX = outer.Max(p => (float)p.X);
            float minY = outer.Min(p => (float)p.Y);
            float maxY = outer.Max(p => (float)p.Y);

            float imageWidth = maxX - minX;
            float imageHeight = maxY - minY;

            var polaczeniaArray = polaczenia.Split(';')
                .Select(p => p.Split('-'))
                .Select(parts => (kat: int.Parse(parts[0]), typ: parts[1].Trim()))
                .ToArray();

            if (polaczeniaArray.Length != 4)
                throw new Exception("Oczekiwano 4 połączeń narożników.");

            var joinTypes = new[]
            {
        (Left: polaczeniaArray[0].typ, Right: polaczeniaArray[1].typ), // Top
        (Left: polaczeniaArray[1].typ, Right: polaczeniaArray[2].typ), // Right
        (Left: polaczeniaArray[2].typ, Right: polaczeniaArray[3].typ), // Bottom
        (Left: polaczeniaArray[3].typ, Right: polaczeniaArray[0].typ), // Left
    };

            // Obliczenia pozycji i długości
            int topX = (joinTypes[0].Left == "T3") ? (int)profileLeft : 0;
            int topW = (int)imageWidth - ((joinTypes[0].Left == "T3" ? (int)profileLeft : 0) +
                                          (joinTypes[0].Right == "T3" ? (int)profileRight : 0));

            int rightY = (joinTypes[1].Left == "T1") ? (int)profileTop : 0;
            int rightH = (int)imageHeight - ((joinTypes[1].Left == "T1" ? (int)profileTop : 0) +
                                             (joinTypes[1].Right == "T1" ? (int)profileBottom : 0));

            int bottomX = (joinTypes[2].Left == "T3") ? (int)profileLeft : 0;
            int bottomW = (int)imageWidth - ((joinTypes[2].Left == "T3" ? (int)profileLeft : 0) +
                                             (joinTypes[2].Right == "T3" ? (int)profileRight : 0));

            int leftY = (joinTypes[3].Left == "T1" || joinTypes[0].Left == "T1") ? (int)profileTop : 0;
            int leftH = (int)imageHeight - ((joinTypes[3].Left == "T1" || joinTypes[2].Left == "T4" ? (int)profileTop : 0) +
                                            (joinTypes[3].Right == "T1" ? (int)profileBottom : 0));

            void AddElement(int index, int x, int y, int w, int h, string typLewy, string typPrawy, string grupa)
            {
                int offset = (index % 2 == 0) ? h : w;
                bool isLeftT2 = typLewy == "T2";
                bool isRightT2 = typPrawy == "T2";

                var points = index switch
                {
                    0 => new List<XPoint> // Góra
            {
                new(minX + x, minY + y),
                new(minX + x + w, minY + y),
                new(minX + x + w - (isRightT2 ? offset : 0), minY + y + h),
                new(minX + x + (isLeftT2 ? offset : 0), minY + y + h),
            },
                    1 => new List<XPoint> // Prawa
            {
                new(maxX - profileRight, minY + y + (isLeftT2 ? offset : 0)),
                new(maxX, minY + y),
                new(maxX, minY + y + h),
                new(maxX - profileRight, minY + y + h - (isRightT2 ? offset : 0)),
            },
                    2 => new List<XPoint> // Dół
            {
                new(minX + x + (isLeftT2 ? offset : 0), maxY - h),
                new(minX + x + w - (isRightT2 ? offset : 0), maxY - h),
                new(minX + x + w, maxY),
                new(minX + x, maxY),
            },
                    3 => new List<XPoint> // Lewa
            {
                new(minX, minY + y),
                new(minX + profileLeft, minY + y + (isRightT2 ? offset : 0)),
                new(minX + profileLeft, minY + y + h - (isLeftT2 ? offset : 0)),
                new(minX, minY + y + h),
            },
                    _ => throw new ArgumentOutOfRangeException()
                };

                ElementyRamyRysowane.Add(new KsztaltElementu
                {
                    TypKsztaltu = "trapez",
                    Wierzcholki = points,
                    WypelnienieZewnetrzne = "wood-pattern",
                    WypelnienieWewnetrzne = KolorSzyby,
                    Grupa = grupa
                });
            }

            // Generuj wszystkie ramy
            AddElement(0, topX, 0, topW, (int)profileTop, joinTypes[0].Left, joinTypes[0].Right, "Gora");
            AddElement(1, 0, rightY, (int)profileRight, rightH, joinTypes[1].Left, joinTypes[1].Right, "Prawo");
            AddElement(2, bottomX, 0, bottomW, (int)profileBottom, joinTypes[2].Left, joinTypes[2].Right, "Dol");
            AddElement(3, 0, leftY, (int)profileLeft, leftH, joinTypes[3].Left, joinTypes[3].Right, "Lewo");
        }

        private void GenerateTriangleElements(List<XPoint> outer, List<XPoint> inner,
            float leftOffset, float rightOffset, float topOffset, float bottomOffset)
        {

            double maxLength = 0;
            int baseIndex1 = 0, baseIndex2 = 1;

            for (int i = 0; i < 3; i++)
            {
                int next = (i + 1) % 3;
                double length = Math.Sqrt(Math.Pow(outer[next].X - outer[i].X, 2) +
                                          Math.Pow(outer[next].Y - outer[i].Y, 2));
                if (length > maxLength)
                {
                    maxLength = length;
                    baseIndex1 = i;
                    baseIndex2 = next;
                }
            }

            int vertexIndex = Enumerable.Range(0, 3).First(i => i != baseIndex1 && i != baseIndex2);

            for (int i = 0; i < 3; i++)
            {
                int next = (i + 1) % 3;

                bool isBase = (i == baseIndex1 && next == baseIndex2) || (i == baseIndex2 && next == baseIndex1);
                string grupa;

                if (isBase)
                {
                    grupa = "Podstawa";
                }
                else if (i == vertexIndex || next == vertexIndex)
                {
                    // Rozróżnij boki względem X wierzchołka
                    var drugiPunkt = (i == vertexIndex) ? outer[next] : outer[i];
                    grupa = drugiPunkt.X < outer[vertexIndex].X ? "LewyBok" : "PrawyBok";
                }
                else
                {
                    grupa = "NieznanyBok";
                }

                ElementyRamyRysowane.Add(new KsztaltElementu
                {
                    TypKsztaltu = isBase ? "prostokat" : "trapez",
                    Wierzcholki = new List<XPoint>
            {
                outer[i],
                outer[next],
                inner[next],
                inner[i]
            },
                    WypelnienieZewnetrzne = "wood-pattern",
                    WypelnienieWewnetrzne = KolorSzyby,
                    Grupa = grupa
                });
            }
        }

    private void GenerateGenericElementsWithJoins(
    List<XPoint> outer, List<XPoint> inner,
    float profileLeft, float profileRight, float profileTop, float profileBottom,
    string typKsztalt, string polaczenia, List<KonfSystem> model)
        {
            int vertexCount = outer.Count;
            if (vertexCount < 3)
                throw new Exception("Polygon must have at least 3 vertices.");

            var polaczeniaArray = polaczenia.Split(';')
                .Select(p => p.Split('-'))
                .Select(parts => (kat: int.Parse(parts[0]), typ: parts[1].Trim()))
                .ToArray();

            if (polaczeniaArray.Length != vertexCount)
                throw new Exception($"Expected {vertexCount} corner connections.");

            for (int i = 0; i < vertexCount; i++)
            {
                int next = (i + 1) % vertexCount;
                var leftJoin = polaczeniaArray[i].typ;
                var rightJoin = polaczeniaArray[next].typ;

                XPoint current = outer[i];
                XPoint nextPt = outer[next];

                // Oblicz wektor kierunku krawędzi
                float dx = (float)(nextPt.X - current.X);
                float dy = (float)(nextPt.Y - current.Y);
                float length = (float)Math.Sqrt(dx * dx + dy * dy);

                if (length == 0) continue;

                // Oblicz wektory normalne
                float nx = -dy / length;
                float ny = dx / length;

                // Określ grubość profilu w zależności od orientacji
                float profileThickness = Math.Abs(dx) > Math.Abs(dy)
                    ? (ny > 0 ? profileTop : profileBottom)
                    : (nx > 0 ? profileRight : profileLeft);

                // Oblicz przesunięcia dla połączeń
                float leftOffset = GetJoinOffset(leftJoin, profileThickness);
                float rightOffset = GetJoinOffset(rightJoin, profileThickness);

                // Oblicz punkty wewnętrzne
                var innerStart = new XPoint(
                    current.X + nx * profileThickness,
                    current.Y + ny * profileThickness);

                var innerEnd = new XPoint(
                    nextPt.X + nx * profileThickness,
                    nextPt.Y + ny * profileThickness);

                // Dostosuj punkty do typów połączeń
                innerStart.X += dx * leftOffset / length;
                innerStart.Y += dy * leftOffset / length;

                innerEnd.X -= dx * rightOffset / length;
                innerEnd.Y -= dy * rightOffset / length;

                ElementyRamyRysowane.Add(new KsztaltElementu
                {
                    TypKsztaltu = "trapez",
                    Wierzcholki = new List<XPoint> { current, nextPt, innerEnd, innerStart },
                    WypelnienieZewnetrzne = "wood-pattern",
                    WypelnienieWewnetrzne = KolorSzyby,
                    Grupa = $"Bok{i + 1}"
                });
            }
        }

        private float GetJoinOffset(string joinType, float profileThickness)
        {
            return joinType switch
            {
                "T1" => 0,
                "T2" => profileThickness * 0.5f,
                "T3" => profileThickness,
                "T4" => -profileThickness * 0.5f,
                _ => 0
            };
        }

        //private void GenerateGenericElements(List<XPoint> outer, List<XPoint> inner)
        //{
        //    // Domyślna implementacja dla innych kształtów
        //    for (int i = 0; i < outer.Count; i++)
        //    {
        //        int next = (i + 1) % outer.Count;
        //        ElementyRamyRysowane.Add(new KsztaltElementu
        //        {
        //            TypKsztaltu = "wielobok",
        //            Wierzcholki = new List<XPoint>
        //            {
        //                outer[i],
        //                outer[next],
        //                inner[next],
        //                inner[i]
        //            },
        //            WypelnienieZewnetrzne = "wood-pattern",
        //            WypelnienieWewnetrzne = KolorSzyby,
        //            Grupa = $"Bok{i + 1}"
        //        });
        //    }
        //}

        private List<XPoint> CalculateOffsetPolygon(
            List<XPoint> points,
            float leftOffset,
            float rightOffset,
            float topOffset,
            float bottomOffset)
        {
            if (points.Count == 4) // Dla prostokątów
            {
                float minX = (float)points.Min(p => p.X);
                float maxX = (float)points.Max(p => p.X);
                float minY = (float)points.Min(p => p.Y);
                float maxY = (float)points.Max(p => p.Y);

                return new List<XPoint>
                {
                    new XPoint(minX + leftOffset, minY + topOffset),
                    new XPoint(maxX - rightOffset, minY + topOffset),
                    new XPoint(maxX - rightOffset, maxY - bottomOffset),
                    new XPoint(minX + leftOffset, maxY - bottomOffset)
                };
            }
            else if (points.Count == 3) // Poprawiona implementacja dla trójkątów
            {
                // Znajdź podstawę (najdłuższy bok)
                double maxLength = 0;
                int baseIndex1 = 0, baseIndex2 = 1;

                for (int i = 0; i < 3; i++)
                {
                    int next = (i + 1) % 3;
                    double length = Math.Sqrt(Math.Pow(points[next].X - points[i].X, 2) +
                                           Math.Pow(points[next].Y - points[i].Y, 2));
                    if (length > maxLength)
                    {
                        maxLength = length;
                        baseIndex1 = i;
                        baseIndex2 = next;
                    }
                }

                // Oblicz wektory normalne dla każdej strony
                var normals = new Dictionary<int, XPoint>();
                for (int i = 0; i < 3; i++)
                {
                    int next = (i + 1) % 3;
                    var dx = points[next].X - points[i].X;
                    var dy = points[next].Y - points[i].Y;

                    // Wektor normalny (prostopadły) do boku
                    var normal = new XPoint(-dy, dx);

                    // Normalizacja
                    var length = Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y);
                    normals[i] = new XPoint(normal.X / length, normal.Y / length);
                }

                // Oblicz punkty offsetowe
                var resultX = new List<XPoint>();
                for (int i = 0; i < 3; i++)
                {
                    float offset;
                    if (i == baseIndex1 || i == baseIndex2)
                        offset = (i == baseIndex1 ? leftOffset : rightOffset) + bottomOffset;
                    else
                        offset = topOffset;

                    var prevNormal = normals[(i - 1 + 3) % 3];
                    var nextNormal = normals[i];

                    // Średnia normalna dla wierzchołka
                    var avgNormal = new XPoint(
                        (prevNormal.X + nextNormal.X) / 2,
                        (prevNormal.Y + nextNormal.Y) / 2);

                    // Normalizacja średniej
                    var avgLength = Math.Sqrt(avgNormal.X * avgNormal.X + avgNormal.Y * avgNormal.Y);
                    avgNormal = new XPoint(avgNormal.X / avgLength, avgNormal.Y / avgLength);

                    resultX.Add(new XPoint(
                        points[i].X + avgNormal.X * offset,
                        points[i].Y + avgNormal.Y * offset));
                }

                return resultX;
            }

            // Domyślna implementacja dla innych kształtów
            var result = new List<XPoint>();
            var boundingBox = CalculateBoundingBox(points);

            foreach (var point in points)
            {
                bool isLeft = Math.Abs(point.X - boundingBox.left) < 0.1f;
                bool isRight = Math.Abs(point.X - boundingBox.right) < 0.1f;
                bool isTop = Math.Abs(point.Y - boundingBox.top) < 0.1f;
                bool isBottom = Math.Abs(point.Y - boundingBox.bottom) < 0.1f;

                XPoint offsetPoint = point;

                if (isLeft && isTop)
                {
                    offsetPoint.X += leftOffset;
                    offsetPoint.Y += topOffset;
                }
                else if (isRight && isTop)
                {
                    offsetPoint.X -= rightOffset;
                    offsetPoint.Y += topOffset;
                }
                else if (isRight && isBottom)
                {
                    offsetPoint.X -= rightOffset;
                    offsetPoint.Y -= bottomOffset;
                }
                else if (isLeft && isBottom)
                {
                    offsetPoint.X += leftOffset;
                    offsetPoint.Y -= bottomOffset;
                }
                else if (isLeft)
                {
                    offsetPoint.X += leftOffset;
                }
                else if (isRight)
                {
                    offsetPoint.X -= rightOffset;
                }
                else if (isTop)
                {
                    offsetPoint.Y += topOffset;
                }
                else if (isBottom)
                {
                    offsetPoint.Y -= bottomOffset;
                }

                result.Add(offsetPoint);
            }

            return result;
        }

        private (float left, float right, float top, float bottom) CalculateBoundingBox(List<XPoint> points)
        {
            return (
                left: (float)points.Min(p => p.X),
                right: (float)points.Max(p => p.X),
                top: (float)points.Min(p => p.Y),
                bottom: (float)points.Max(p => p.Y)
            );
        }

        // Other properties and methods of the Generator class...

        /// <summary>
        /// Calculates the length of an element based on its vertices.
        /// </summary>
        /// <param name="vertices">List of vertices defining the shape.</param>
        /// <returns>The calculated length of the element.</returns>
        public double DlugoscElementu(List<XPoint> vertices)
        {
            if (vertices == null || vertices.Count < 2)
            {
                throw new ArgumentException("Vertices list must contain at least two points.");
            }

            double length = 0.0;

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                var point1 = vertices[i];
                var point2 = vertices[i + 1];
                length += Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
            }

            // Optionally, close the shape by connecting the last point to the first
            var firstPoint = vertices[0];
            var lastPoint = vertices[vertices.Count - 1];
            length += Math.Sqrt(Math.Pow(lastPoint.X - firstPoint.X, 2) + Math.Pow(lastPoint.Y - firstPoint.Y, 2));

            return length;
        }
    }
}