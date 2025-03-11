using System;
using System.IO;
using System.Net.Http;
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
                Console.WriteLine("❌ Błąd: Lista model jest pusta lub null.");
                return null;
            }

            int imageSize = 500; // Rozmiar całego obrazu
            int borderThickness = 5; // Grubość obramowania
            int padding = 0; // Marginesy od krawędzi

            // 🔥 Pobranie obrazka z API
            byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
            if (imageBytes == null || imageBytes.Length == 0)
            {
                Console.WriteLine("❌ Błąd: Nie udało się pobrać obrazka.");
                return null;
            }

            using Image<Rgba32> woodTexture = Image.Load<Rgba32>(imageBytes);
            woodTexture.Mutate(x => x.Resize(imageSize, imageSize)); // Dopasowanie tekstury

            using Image<Rgba32> image = new(imageSize, imageSize);
            image.Mutate(x => x.Fill(Color.White)); // Ustawienie tła na białe

            // 🔥 Pobranie szerokości i wysokości profili zgodnie z modelem
            double profileLeftWidthTMP = Math.Max(model.FirstOrDefault(e => e.WystepujeLewa)?.PionLewa ?? 1, 0); // domyślna 0
            double profileTopWidthTMP = Math.Max(model.FirstOrDefault(e => e.WystepujeGora)?.PoziomGora ?? 1, 0); // domyślna 0

            double profileRightWidthTMP = Math.Max(model.FirstOrDefault(e => e.WystepujePrawa)?.PionPrawa ?? 1, 0);
            double profileBottomWidthTMP = Math.Max(model.FirstOrDefault(e => e.WystepujeDol)?.PoziomDol ?? 1, 0);

            double profileLeftWidth = profileRightWidthTMP - profileLeftWidthTMP;
            double profileTopWidth = profileBottomWidthTMP - profileTopWidthTMP;

            double profileRightWidth = Math.Max(model.FirstOrDefault(e => e.WystepujePrawa)?.PionPrawa ?? 1, 0) - profileLeftWidthTMP;
            double profileBottomWidth = Math.Max(model.FirstOrDefault(e => e.WystepujeDol)?.PoziomDol ?? 1, 0) - profileTopWidthTMP;

            Console.WriteLine($"profileLeftWidth:{profileLeftWidth}, profileTopWidth:{profileTopWidth}, profileRightWidth:{profileRightWidth}, profileBottomWidth:{profileBottomWidth}");


            // 🔥 Parsowanie sposobu łączenia naroży
            var polaczeniaArray = polaczenia.Split(';')
                .Select(p => p.Split('-'))
                .Select(parts => (kat: int.Parse(parts[0]), typ: parts[1]))
                .ToArray();

            // 🔥 Kolejność rysowania profili dla poprawnego nakładania
            var orderedFrames = new List<(IPath path, Image<Rgba32> texture, Point position)>();

            // 🔥 Tworzenie kształtów ramy okna
            var topFrame = new RectangularPolygon(padding, padding, imageSize, (float)profileTopWidth);
            var bottomFrame = new RectangularPolygon(padding, imageSize - (float)profileBottomWidth, imageSize, (float)profileBottomWidth);
            var leftFrame = new RectangularPolygon(padding, padding, (float)profileLeftWidth, imageSize);
            var rightFrame = new RectangularPolygon(imageSize - (float)profileRightWidth, padding, (float)profileRightWidth, imageSize);

            // 🔥 Tworzenie tekstur dla każdej części ramy
            using var topTexture = woodTexture.Clone(x => x.Crop(new Rectangle(0, 0, imageSize, (int)profileTopWidth)));
            using var bottomTexture = woodTexture.Clone(x => x.Crop(new Rectangle(0, 0, imageSize, (int)profileBottomWidth)));
            using var leftTexture = woodTexture.Clone(x => x.Crop(new Rectangle(0, 0, (int)profileLeftWidth, imageSize)));
            using var rightTexture = woodTexture.Clone(x => x.Crop(new Rectangle(0, 0, (int)profileRightWidth, imageSize)));

            // 🔥 Dodanie warstw ramy w odpowiedniej kolejności
            orderedFrames.Add((leftFrame, leftTexture, new Point(padding, padding)));
            orderedFrames.Add((rightFrame, rightTexture, new Point(imageSize - (int)profileRightWidth, padding)));
            orderedFrames.Add((topFrame, topTexture, new Point(padding, padding)));
            orderedFrames.Add((bottomFrame, bottomTexture, new Point(padding, imageSize - (int)profileBottomWidth)));

            // 🔥 Tworzenie szyby na środku
            int glassX = padding + (int)profileLeftWidth;
            int glassY = padding + (int)profileTopWidth;
            int glassWidth = imageSize - (int)profileLeftWidth - (int)profileRightWidth;
            int glassHeight = imageSize - (int)profileTopWidth - (int)profileBottomWidth;
            IPath glassPath = new RectangularPolygon(glassX, glassY, glassWidth, glassHeight);

            // 🔥 Rysowanie ramy okna na obrazie w odpowiedniej kolejności
            image.Mutate(x =>
            {
                foreach (var (path, texture, position) in orderedFrames)
                {
                    x.DrawImage(texture, position, 1f);
                    x.Draw(Pens.Solid(Color.Black, borderThickness), path);
                }

                // 🔥 Dodanie szyby (jasnoniebieskie wypełnienie)
                x.Fill(Color.LightBlue.WithAlpha(0.4f), glassPath);

                // 🔥 Dodanie obramowania szyby
                x.Draw(Pens.Solid(Color.Black, borderThickness), glassPath);
            });

            // 🔥 Konwersja do byte[]
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
