using GEORGE.Client.Pages.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GEORGE.Client.Pages.Okna
{
    public class Generator : GenerujOkno
    {
        public new List<KsztaltElementu> ElementyRamyRysowane { get; set; } = new();

        public Generator()
        {
            // Inicjalizacja domyślnych wartości
            Szerokosc = 1000;
            Wysokosc = 1000;
            GruboscLewo = 50;
            GruboscPrawo = 50;
            GruboscGora = 50;
            GruboscDol = 50;
            KolorZewnetrzny = "#FFFFFF";
            KolorWewnetrzny = "#FFFFFF";
            Waga = 0;
            TypKsztaltu = "prostokąt";
            GruboscSzyby = 24;
            KolorSzyby = "#ADD8E6";
        }

        public void AddElements(List<ShapeRegion> regions)
        {
            if (regions == null) return;

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

                // Ustal grubości ramy
                float grLewo = GruboscLewo;
                float grPrawo = GruboscPrawo;
                float grGora = GruboscGora;
                float grDol = GruboscDol;

                // Oblicz wewnętrzny kontur
                var wewnetrznyKontur = CalculateOffsetPolygon(
                    punkty.Select(p => new XPoint(p.X, p.Y)).ToList(),
                    grLewo, grPrawo, grGora, grDol);

                // Generuj elementy ramy w zależności od typu kształtu
                if (region.TypKsztaltu == "prostokąt")
                {
                    GenerateRectangleElements(punkty, wewnetrznyKontur, grLewo, grPrawo, grGora, grDol);
                }
                else if (region.TypKsztaltu == "trójkąt")
                {
                    GenerateTriangleElements(punkty, wewnetrznyKontur, grLewo, grPrawo, grGora, grDol);
                }
                else
                {
                    GenerateGenericElements(punkty, wewnetrznyKontur);
                }
            }
        }

        private void GenerateRectangleElements(List<XPoint> outer, List<XPoint> inner,
            float leftOffset, float rightOffset, float topOffset, float bottomOffset)
        {
            // 4 elementy dla prostokąta (dół, góra, lewo, prawo)
            var minX = outer.Min(p => p.X);
            var maxX = outer.Max(p => p.X);
            var minY = outer.Min(p => p.Y);
            var maxY = outer.Max(p => p.Y);

            // Dół
            ElementyRamyRysowane.Add(new KsztaltElementu
            {
                TypKsztaltu = "prostokąt",
                Wierzcholki = new List<XPoint>
                {
                    new(minX, maxY - bottomOffset),
                    new(maxX, maxY - bottomOffset),
                    new(maxX, maxY),
                    new(minX, maxY)
                },
                WypelnienieZewnetrzne = "wood-pattern",
                WypelnienieWewnetrzne = KolorSzyby,
                Grupa = "Dol"
            });

            // Góra
            ElementyRamyRysowane.Add(new KsztaltElementu
            {
                TypKsztaltu = "prostokąt",
                Wierzcholki = new List<XPoint>
                {
                    new(minX, minY),
                    new(maxX, minY),
                    new(maxX, minY + topOffset),
                    new(minX, minY + topOffset)
                },
                WypelnienieZewnetrzne = "wood-pattern",
                WypelnienieWewnetrzne = KolorSzyby,
                Grupa = "Gora"
            });

            // Lewo
            ElementyRamyRysowane.Add(new KsztaltElementu
            {
                TypKsztaltu = "prostokąt",
                Wierzcholki = new List<XPoint>
                {
                    new(minX, minY),
                    new(minX + leftOffset, minY),
                    new(minX + leftOffset, maxY),
                    new(minX, maxY)
                },
                WypelnienieZewnetrzne = "wood-pattern",
                WypelnienieWewnetrzne = KolorSzyby,
                Grupa = "Lewo"
            });

            // Prawo
            ElementyRamyRysowane.Add(new KsztaltElementu
            {
                TypKsztaltu = "prostokąt",
                Wierzcholki = new List<XPoint>
                {
                    new(maxX - rightOffset, minY),
                    new(maxX, minY),
                    new(maxX, maxY),
                    new(maxX - rightOffset, maxY)
                },
                WypelnienieZewnetrzne = "wood-pattern",
                WypelnienieWewnetrzne = KolorSzyby,
                Grupa = "Prawo"
            });
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

                var isBase = (i == baseIndex1 && next == baseIndex2) || (i == baseIndex2 && next == baseIndex1);
                var isVertexSide = (i == vertexIndex || next == vertexIndex);

                string grupa = isBase ? "Podstawa" :
                    outer[i].X < outer[next].X ? "LewyBok" : "PrawyBok";

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


        private void GenerateGenericElements(List<XPoint> outer, List<XPoint> inner)
        {
            // Domyślna implementacja dla innych kształtów
            for (int i = 0; i < outer.Count; i++)
            {
                int next = (i + 1) % outer.Count;
                ElementyRamyRysowane.Add(new KsztaltElementu
                {
                    TypKsztaltu = "wielobok",
                    Wierzcholki = new List<XPoint>
                    {
                        outer[i],
                        outer[next],
                        inner[next],
                        inner[i]
                    },
                    WypelnienieZewnetrzne = "wood-pattern",
                    WypelnienieWewnetrzne = KolorSzyby,
                    Grupa = $"Bok{i + 1}"
                });
            }
        }

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
    }
}