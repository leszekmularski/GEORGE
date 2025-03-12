using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using GEORGE.Shared.Models;

public class ImageGenerator
{
    private readonly HttpClient _httpClient;

    public ImageGenerator(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> GenerateImageAsync(List<KonfSystem> model, string polaczenia, string imageUrl)
    {
        try
        {
            if (model == null || model.Count == 0)
            {
                Console.WriteLine("❌ Błąd: Lista model jest pusta.");
                return null;
            }

            int imageSize = 500;
            int borderThickness = 5;

            byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
            if (imageBytes == null || imageBytes.Length == 0)
            {
                Console.WriteLine("❌ Błąd: Nie udało się pobrać tekstury.");
                return null;
            }

            using Image<Rgba32> woodTexture = Image.Load<Rgba32>(imageBytes);
            woodTexture.Mutate(x => x.Resize(imageSize, imageSize));

            using Image<Rgba32> image = new(imageSize, imageSize);
            image.Mutate(x => x.Fill(Color.White));

            double profileLeftWidth = model.FirstOrDefault(e => e.WystepujeLewa)?.PionPrawa ?? 0
                                    - model.FirstOrDefault(e => e.WystepujeLewa)?.PionLewa ?? 0;
            double profileTopWidth = model.FirstOrDefault(e => e.WystepujeGora)?.PionPrawa ?? 0
                                   - model.FirstOrDefault(e => e.WystepujeGora)?.PionLewa ?? 0;
            double profileRightWidth = model.FirstOrDefault(e => e.WystepujePrawa)?.PionPrawa ?? 0
                                     - model.FirstOrDefault(e => e.WystepujePrawa)?.PionLewa ?? 0;
            double profileBottomWidth = model.FirstOrDefault(e => e.WystepujeDol)?.PionPrawa ?? 0
                                      - model.FirstOrDefault(e => e.WystepujeDol)?.PionLewa ?? 0;

            var polaczeniaArray = polaczenia.Split(';')
                .Select(p => p.Split('-'))
                .Select(parts => (kat: int.Parse(parts[0]), typ: parts[1]))
                .ToArray();

            if (polaczeniaArray.Length < 4)
            {
                Console.WriteLine("❌ Błąd: Niepełna konfiguracja połączeń.");
                return null;
            }

            var orderedFrames = new List<(IPath path, Image<Rgba32> texture, Point position)>();

            void AddFrame(int x, int y, int width, int height, string joinType)
            {
                var frame = new RectangularPolygon(x, y, width, height);
                var texture = woodTexture.Clone(tx => tx.Crop(new Rectangle(0, 0, width, height)));
                orderedFrames.Add((frame, texture, new Point(x, y)));
            }

            // 🔥 Kolejność rysowania: najpierw T3, ale pionowe T3 (90°, 270°) przed poziomymi (0°, 180°)
            var drawOrder = new List<(int index, int angle, string type)>
            {
                (0, polaczeniaArray[0].kat, polaczeniaArray[0].typ), // Góra
                (1, polaczeniaArray[1].kat, polaczeniaArray[1].typ), // Prawa
                (2, polaczeniaArray[2].kat, polaczeniaArray[2].typ), // Dół
                (3, polaczeniaArray[3].kat, polaczeniaArray[3].typ)  // Lewa
            };

            drawOrder = drawOrder
                .OrderBy(p => p.type == "T1" ? 0 : 1) // T3 jako pierwsze
                .ThenBy(p => p.type == "T1" ? (p.angle == 90 || p.angle == 270 ? 0 : 1) : 2) // Pionowe T3 przed poziomymi T3
                .ToList();

            foreach (var (index, angle, typ) in drawOrder)
            {
                Console.WriteLine($"index: {index}, angle: {angle}, typ: {typ}");

                switch (index)
                {
                    case 0: // GÓRA
                        AddFrame(0, 0, imageSize, (int)profileTopWidth, typ);
                        break;
                    case 1: // PRAWA
                        AddFrame(imageSize - (int)profileRightWidth, 0, (int)profileRightWidth, imageSize, typ);
                        break;
                    case 2: // DÓŁ
                        AddFrame(0, imageSize - (int)profileBottomWidth, imageSize, (int)profileBottomWidth, typ);
                        break;
                    case 3: // LEWA
                        AddFrame(0, 0, (int)profileLeftWidth, imageSize, typ);
                        break;
                }
            }

            // 🔥 Obsługa narożników T2 (45°)
            if (polaczeniaArray.Any(p => p.typ == "T2"))
            {
                orderedFrames.Clear();
                orderedFrames.Add((new Polygon(new PointF[]
                {
                    new PointF(imageSize - (float)profileRightWidth, 0),
                    new PointF(imageSize, (float)profileTopWidth),
                    new PointF(0, (float)profileTopWidth),
                    new PointF((float)profileLeftWidth, 0),
           
                }), woodTexture.Clone(), new Point(0, 0)));

                orderedFrames.Add((new Polygon(new PointF[]
                {
                    new PointF(imageSize - (float)profileRightWidth, imageSize),
                    new PointF(imageSize, imageSize - (float)profileBottomWidth),
                    new PointF(0, imageSize - (float)profileBottomWidth),
                    new PointF((float)profileLeftWidth, imageSize),
                    
                }), woodTexture.Clone(), new Point(0, (int)profileTopWidth)));
            }

            // 🔥 Tworzenie szyby w środku
            IPath glassPath = new RectangularPolygon(
                (int)profileLeftWidth,
                (int)profileTopWidth,
                imageSize - (int)profileLeftWidth - (int)profileRightWidth,
                imageSize - (int)profileTopWidth - (int)profileBottomWidth
            );

            // 🔥 Rysowanie ramy okna na obrazie
            image.Mutate(x =>
            {
                foreach (var (path, texture, position) in orderedFrames)
                {
                    x.Fill(Color.White, path);
                    x.DrawImage(texture, position, 1f);
                    x.Draw(Pens.Solid(Color.Black, borderThickness), path);
                }

                // 🔥 Dodanie szyby (jasnoniebieskie wypełnienie)
                x.Fill(Color.LightBlue.WithAlpha(0.4f), glassPath);
                x.Draw(Pens.Solid(Color.Black, borderThickness), glassPath);
            });

            using MemoryStream ms = new();
            image.SaveAsPng(ms);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd (ImageGenerator): {ex.Message} / {ex.StackTrace}");
            return null;
        }
    }
}
