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
                    // Wywołanie funkcji
                    GenerateWindowFrameElements(
                        punkty,          // List<XPoint> - zewnętrzne punkty konturu
                        wewnetrznyKontur,// List<XPoint> - wewnętrzne punkty przeszklenia
                        profileLeft,     // float - grubość profilu lewego
                        profileRight,    // float - grubość profilu prawego
                        profileTop,      // float - grubość profilu górnego
                        profileBottom,   // float - grubość profilu dolnego
                        region.TypKsztaltu, // string - typ kształtu (np. "prostokąt")
                        EdytowanyModel.PolaczenieNaroza, // string - np. "T1;T2;T3;T1"
                        KonfiguracjeSystemu              // List<KonfSystem> - konfiguracje systemowe
                    );
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



        private void GenerateWindowFrameElements(
            List<XPoint> outer,
            List<XPoint> inner,
            float profileLeft,
            float profileRight,
            float profileTop,
            float profileBottom,
            string typKsztalt,
            string polaczenia,
            List<KonfSystem> model)
        {
            const int EXPECTED_ELEMENTS = 4;

            if (outer.Count != 4 || inner.Count != 4)
                throw new Exception("Wymagany prostokątny kształt z 4 punktami");

            var connections = polaczenia.Split(';')
                .Select(ct => ct.Trim())
                .Take(4)
                .ToArray();

            // Generuj elementy dla każdej strony
            for (int i = 0; i < EXPECTED_ELEMENTS; i++)
            {
                int prev = (i - 1 + 4) % 4;
                int next = (i + 1) % 4;

                string currentConnection = connections.Length > i ? connections[i] : "T1";
                string prevConnection = connections.Length > prev ? connections[prev] : "T1";
                string nextConnection = connections.Length > next ? connections[next] : "T1";

                List<XPoint> elementPoints = CalculateElementPoints(
                    outer[i],
                    outer[next],
                    inner[i],
                    inner[next],
                    currentConnection,
                    prevConnection,
                    nextConnection,
                    GetProfileThickness(i, profileLeft, profileRight, profileTop, profileBottom));

                ElementyRamyRysowane.Add(new KsztaltElementu
                {
                    TypKsztaltu = "wielobok",
                    Wierzcholki = elementPoints,
                    WypelnienieZewnetrzne = "wood-pattern",
                    WypelnienieWewnetrzne = KolorSzyby,
                    Grupa = GetSideName(i)
                });
            }
        }

        private string GetSideName(int index)
        {
            return index switch
            {
                0 => "Góra",
                1 => "Prawo",
                2 => "Dół",
                3 => "Lewo",
                _ => "Bok"
            };
        }
        private List<XPoint> CalculateElementPoints(
            XPoint outerStart,
            XPoint outerEnd,
            XPoint innerStart,
            XPoint innerEnd,
            string currentConnection,
            string prevConnection,
            string nextConnection,
            float profile)
        {
            return currentConnection switch
            {
                "T1" => CalculateT1Points(outerStart, outerEnd, innerStart, innerEnd, profile, prevConnection, nextConnection),
                "T2" => CalculateT2Points(outerStart, outerEnd, innerStart, innerEnd, profile),
                "T3" => CalculateT3Points(outerStart, outerEnd, innerStart, innerEnd, profile, prevConnection, nextConnection),
                _ => new List<XPoint> { outerStart, outerEnd, innerEnd, innerStart }
            };
        }

        private List<XPoint> CalculateT1Points(
            XPoint outerStart,
            XPoint outerEnd,
            XPoint innerStart,
            XPoint innerEnd,
            float profile,
            string prevConnection,
            string nextConnection)
        {
            // Proste połączenie prostopadłe
            bool isHorizontal = IsHorizontal(outerStart, outerEnd);

            // Korekta dla sąsiednich połączeń
            float prevOffset = prevConnection == "T3" ? profile * 0.5f : 0;
            float nextOffset = nextConnection == "T3" ? profile * 0.5f : 0;

            if (isHorizontal)
            {
                return new List<XPoint>
        {
            new XPoint(outerStart.X + prevOffset, outerStart.Y),
            new XPoint(outerEnd.X - nextOffset, outerEnd.Y),
            new XPoint(innerEnd.X - nextOffset, innerEnd.Y),
            new XPoint(innerStart.X + prevOffset, innerStart.Y)
        };
            }

            return new List<XPoint>
    {
        new XPoint(outerStart.X, outerStart.Y + prevOffset),
        new XPoint(outerEnd.X, outerEnd.Y - nextOffset),
        new XPoint(innerEnd.X, innerEnd.Y - nextOffset),
        new XPoint(innerStart.X, innerStart.Y + prevOffset)
    };
        }

        private List<XPoint> CalculateT2Points(
            XPoint outerStart,
            XPoint outerEnd,
            XPoint innerStart,
            XPoint innerEnd,
            float profile)
        {
            // Połączenie pod kątem 45°
            bool isHorizontal = IsHorizontal(outerStart, outerEnd);
            float offset = profile * MathF.Tan(MathF.PI / 4);

            if (isHorizontal)
            {
                return new List<XPoint>
        {
            outerStart,
            outerEnd,
            new XPoint(innerEnd.X, innerEnd.Y - offset),
            new XPoint(innerStart.X, innerStart.Y - offset)
        };
            }

            return new List<XPoint>
    {
        outerStart,
        outerEnd,
        new XPoint(innerEnd.X - offset, innerEnd.Y),
        new XPoint(innerStart.X - offset, innerStart.Y)
    };
        }

        private List<XPoint> CalculateT3Points(
            XPoint outerStart,
            XPoint outerEnd,
            XPoint innerStart,
            XPoint innerEnd,
            float profile,
            string prevConnection,
            string nextConnection)
        {
            // Połączenie z przesunięciem
            bool isHorizontal = IsHorizontal(outerStart, outerEnd);
            float offset = profile * 0.75f;

            if (isHorizontal)
            {
                float leftOffset = prevConnection == "T1" ? offset : 0;
                float rightOffset = nextConnection == "T1" ? offset : 0;

                return new List<XPoint>
        {
            new XPoint(outerStart.X + leftOffset, outerStart.Y),
            new XPoint(outerEnd.X - rightOffset, outerEnd.Y),
            innerEnd,
            innerStart
        };
            }

            float topOffset = prevConnection == "T1" ? offset : 0;
            float bottomOffset = nextConnection == "T1" ? offset : 0;

            return new List<XPoint>
    {
        new XPoint(outerStart.X, outerStart.Y + topOffset),
        new XPoint(outerEnd.X, outerEnd.Y - bottomOffset),
        innerEnd,
        innerStart
    };
        }

        private bool IsHorizontal(XPoint a, XPoint b) => Math.Abs(a.Y - b.Y) < 0.001f;

        private float GetProfileThickness(int sideIndex, float left, float right, float top, float bottom)
        {
            return sideIndex switch
            {
                0 => top,    // Góra
                1 => right,  // Prawo
                2 => bottom, // Dół
                3 => left,    // Lewo
                _ => 10f
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