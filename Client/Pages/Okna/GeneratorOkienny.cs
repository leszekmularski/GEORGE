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
                    GenerateGenericElements(punkty, wewnetrznyKontur);
                }
            }

            //0 - Lewy górny
            //1 - Prawy górny
            //2 - Prawy dolny
            //3 - Lewy dolny

        }

        private void GenerateTop(
        float profileTop, float profileLeft, float profileRight,
        (int kat, string typ) naroznik1, (int kat, string typ) naroznik2, float minX, float minY, float width)
        {
            // Czy któryś narożnik to T2?
            bool leftT2 = naroznik1.typ == "T2"; // && naroznik1.kat == 45; // dla narożnika 0
            bool rightT2 = naroznik2.typ == "T2"; // && naroznik2.kat == 45; // dla narożnika 1

            Console.WriteLine($"GenerateTop: leftT2: {leftT2}, rightT2: {rightT2} naroznik1.typ: {naroznik1.typ}  naroznik2.typ: {naroznik2.typ}");

            float x1 = minX;
            float x2 = minX + width;
            float y1 = minY;
            float y2 = minY + profileTop;

            var points = new List<XPoint>();

            // Lewy górny
            points.Add(new XPoint(x1 + (leftT2 ? profileTop : 0), y1));

            // Prawy górny
            points.Add(new XPoint(x2 - (rightT2 ? profileTop : 0), y1));

            // Prawy dolny
            points.Add(new XPoint(x2, y2));

            // Lewy dolny
            points.Add(new XPoint(x1, y2));

            ElementyRamyRysowane.Add(new KsztaltElementu
            {
                TypKsztaltu = "trapez", // możesz zachować "prostokąt", jeśli geometria działa
                Wierzcholki = points,
                WypelnienieZewnetrzne = "wood-pattern",
                WypelnienieWewnetrzne = KolorSzyby,
                Grupa = "Gora"
            });
        }


        private void GenerateRectangleElements(List<XPoint> outer, List<XPoint> inner,
    float leftOffset, float rightOffset, float topOffset, float bottomOffset,
    string typKsztalt, string polaczenia, List<KonfSystem> model)
        {
            // Oblicz bounding box
            float minX = (float)outer.Min(p => p.X);
            float maxX = (float)outer.Max(p => p.X);
            float minY = (float)outer.Min(p => p.Y);
            float maxY = (float)outer.Max(p => p.Y);
            float imageWidth = maxX - minX;
            float imageHeight = maxY - minY;

            // Pobierz konfigurację połączeń
            var polaczeniaArray = polaczenia.Split(';')
                .Select(p => p.Split('-'))
                .Select(parts => (kat: int.Parse(parts[0]), typ: parts[1].Trim()))
                .ToArray();

            // Oblicz rzeczywiste grubości z modelu
            //float profileLeft = (float)(model.FirstOrDefault(e => e.WystepujeLewa)?.PionPrawa ?? 0 -
            //                          model.FirstOrDefault(e => e.WystepujeLewa)?.PionLewa ?? 0);
            //float profileRight = (float)(model.FirstOrDefault(e => e.WystepujePrawa)?.PionPrawa ?? 0 -
            //                           model.FirstOrDefault(e => e.WystepujePrawa)?.PionLewa ?? 0);
            //float profileTop = (float)(model.FirstOrDefault(e => e.WystepujeGora)?.PionPrawa ?? 0 -
            //                         model.FirstOrDefault(e => e.WystepujeGora)?.PionLewa ?? 0);
            //float profileBottom = (float)(model.FirstOrDefault(e => e.WystepujeDol)?.PionPrawa ?? 0 -
            //                            model.FirstOrDefault(e => e.WystepujeDol)?.PionLewa ?? 0);

            float profileLeft = leftOffset;
            float profileRight = rightOffset;
            float profileTop = topOffset;
            float profileBottom = bottomOffset;

            // Korekta wymiarów na podstawie połączeń
            float goraWidth = imageWidth;
            float dolWidth = imageWidth;
            float lewaHeight = imageHeight;
            float prawaHeight = imageHeight;

            // Koryguj szerokości dla połączeń typu T3
            if (polaczeniaArray[0].typ == "T3") goraWidth -= profileLeft;
            if (polaczeniaArray[1].typ == "T3") goraWidth -= profileRight;
            if (polaczeniaArray[2].typ == "T3") dolWidth -= profileRight;
            if (polaczeniaArray[3].typ == "T3") dolWidth -= profileLeft;

            // Koryguj wysokości dla połączeń typu T1
            if (polaczeniaArray[0].typ == "T1") lewaHeight -= profileTop;
            if (polaczeniaArray[3].typ == "T1") lewaHeight -= profileBottom;
            if (polaczeniaArray[1].typ == "T1") prawaHeight -= profileTop;
            if (polaczeniaArray[2].typ == "T1") prawaHeight -= profileBottom;

            // Oblicz pozycje startowe
            float goraX = polaczeniaArray[0].typ == "T3" ? profileLeft : 0;
            float dolX = polaczeniaArray[3].typ == "T3" ? profileLeft : 0;
            float lewaY = polaczeniaArray[0].typ == "T1" ? profileTop : 0;
            float prawaY = polaczeniaArray[1].typ == "T1" ? profileTop : 0;

            // Utwórz elementy z uwzględnieniem korekt
            ElementyRamyRysowane.Add(new KsztaltElementu
            {
                TypKsztaltu = typKsztalt,
                Wierzcholki = new List<XPoint>
            {
                new(minX + goraX, maxY - profileBottom),
                new(minX + goraX + dolWidth, maxY - profileBottom),
                new(minX + goraX + dolWidth, maxY),
                new(minX + goraX, maxY)
            },
                WypelnienieZewnetrzne = "wood-pattern",
                WypelnienieWewnetrzne = KolorSzyby,
                Grupa = "Dol"
            });



            GenerateTop(profileTop, profileLeft, profileRight, polaczeniaArray[0], polaczeniaArray[1], minX, minY, goraWidth);
            //ElementyRamyRysowane.Add(new KsztaltElementu
            //{
            //    TypKsztaltu = typKsztalt,
            //    Wierzcholki = new List<XPoint>
            //{
            //    new(minX + goraX, minY),
            //    new(minX + goraX + goraWidth, minY),
            //    new(minX + goraX + goraWidth, minY + profileTop),
            //    new(minX + goraX, minY + profileTop)
            //},
            //    WypelnienieZewnetrzne = "wood-pattern",
            //    WypelnienieWewnetrzne = KolorSzyby,
            //    Grupa = "Gora"
            //});

            ElementyRamyRysowane.Add(new KsztaltElementu
            {
                TypKsztaltu = typKsztalt,
                Wierzcholki = new List<XPoint>
            {
                new(minX, minY + lewaY),
                new(minX + profileLeft, minY + lewaY),
                new(minX + profileLeft, minY + lewaY + lewaHeight),
                new(minX, minY + lewaY + lewaHeight)
            },
                WypelnienieZewnetrzne = "wood-pattern",
                WypelnienieWewnetrzne = KolorSzyby,
                Grupa = "Lewo"
            });

            ElementyRamyRysowane.Add(new KsztaltElementu
            {
                TypKsztaltu = typKsztalt,
                Wierzcholki = new List<XPoint>
        {
            new(maxX - profileRight, minY + prawaY),
            new(maxX, minY + prawaY),
            new(maxX, minY + prawaY + prawaHeight),
            new(maxX - profileRight, minY + prawaY + prawaHeight)
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