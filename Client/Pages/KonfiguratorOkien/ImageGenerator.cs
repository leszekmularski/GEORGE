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
            //EdytowanyModel.PolaczenieNaroza: 180-T1;90-T1;0-T1;270-T1 gdzie 180-90 poziomy górny | 90-0 prawy pionowy | 90-270 - poziomy dolny | 270-180 - lewy pionowy
            //EdytowanyModel.PolaczenieNaroza: 180-T2;90-T1;0-T1;270-T1

            if (polaczeniaArray[0].typ == "T1" && polaczeniaArray[1].typ == "T1")
            {

                // Generowanie elementów ramy z odpowiednimi połączeniami
                AddFrame(0, 0, (int)profileLeftWidth, imageSize, polaczeniaArray[3].typ); // Lewa
                AddFrame(imageSize - (int)profileRightWidth, 0, (int)profileRightWidth, imageSize, polaczeniaArray[1].typ); // Prawa
                AddFrame(0, 0, imageSize, (int)profileTopWidth, polaczeniaArray[0].typ); // Góra
                AddFrame(0, imageSize - (int)profileBottomWidth, imageSize, (int)profileBottomWidth, polaczeniaArray[2].typ); // Dół
            }

            if (polaczeniaArray[0].typ == "T3" && polaczeniaArray[1].typ == "T3")
            {
                // Generowanie elementów ramy z odpowiednimi połączeniami
                AddFrame(0, 0, imageSize, (int)profileTopWidth, polaczeniaArray[0].typ); // Góra
                AddFrame(0, imageSize - (int)profileBottomWidth, imageSize, (int)profileBottomWidth, polaczeniaArray[2].typ); // Dół
                AddFrame(imageSize - (int)profileRightWidth, 0, (int)profileRightWidth, imageSize, polaczeniaArray[1].typ); // Prawa
                AddFrame(0, 0, (int)profileLeftWidth, imageSize, polaczeniaArray[3].typ); // Lewa;
            }

            if (polaczeniaArray[0].typ == "T1" && polaczeniaArray[1].typ == "T3")
            {
                // Generowanie elementów ramy z odpowiednimi połączeniami
                AddFrame(0, 0, imageSize, (int)profileTopWidth, polaczeniaArray[0].typ); // Góra
                AddFrame(imageSize - (int)profileRightWidth, 0, (int)profileRightWidth, imageSize, polaczeniaArray[1].typ); // Prawa
                AddFrame(0, imageSize - (int)profileBottomWidth, imageSize, (int)profileBottomWidth, polaczeniaArray[2].typ); // Dół
                AddFrame(0, 0, (int)profileLeftWidth, imageSize, polaczeniaArray[3].typ); // Lewa
            }
            if (polaczeniaArray[0].typ == "T3" && polaczeniaArray[1].typ == "T1")
            {
                // Generowanie elementów ramy z odpowiednimi połączeniami
                AddFrame(0, 0, imageSize, (int)profileTopWidth, polaczeniaArray[0].typ); // Góra
                AddFrame(imageSize - (int)profileRightWidth, 0, (int)profileRightWidth, imageSize, polaczeniaArray[1].typ); // Prawa
                AddFrame(0, imageSize - (int)profileBottomWidth, imageSize, (int)profileBottomWidth, polaczeniaArray[2].typ); // Dół
                AddFrame(0, 0, (int)profileLeftWidth, imageSize, polaczeniaArray[3].typ); // Lewa
            }

            if (polaczeniaArray.Any(p => p.typ == "T2"))
            {
                orderedFrames.Clear();
                orderedFrames.Add((new Polygon(new PointF[] {
                    new PointF(0, (float)profileTopWidth),
                    new PointF((float)profileLeftWidth, 0),
                    new PointF(imageSize - (float)profileRightWidth, 0),
                    new PointF(imageSize, (float)profileTopWidth)
                }), woodTexture.Clone(), new Point(0, 0)));
            }

            IPath glassPath = new RectangularPolygon(
                (int)profileLeftWidth,
                (int)profileTopWidth,
                imageSize - (int)profileLeftWidth - (int)profileRightWidth,
                imageSize - (int)profileTopWidth - (int)profileBottomWidth
            );

            image.Mutate(x =>
            {
                foreach (var (path, texture, position) in orderedFrames)
                {
                    x.Fill(Color.White, path);
                    x.DrawImage(texture, position, 1f);
                    x.Draw(Pens.Solid(Color.Black, borderThickness), path);
                }

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
