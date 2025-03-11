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

            // 🔥 Parsowanie sposobu łączenia naroży
            var polaczeniaArray = polaczenia.Split(';')
                .Select(p => p.Split('-'))
                .Select(parts => (kat: int.Parse(parts[0]), typ: parts[1]))
                .ToArray();

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

            // 🔥 Długości elementów w zależności od typu połączenia
            double topWidth = (polaczeniaArray[0].typ == "T1" || polaczeniaArray[0].typ == "T2") ? imageSize - 2 * padding : imageSize - 2 * padding - profileTopWidth;
            double bottomWidth = (polaczeniaArray[2].typ == "T1" || polaczeniaArray[2].typ == "T2") ? imageSize - 2 * padding : imageSize - 2 * padding - profileBottomWidth;
            double leftHeight = (polaczeniaArray[1].typ == "T3" || polaczeniaArray[1].typ == "T2") ? imageSize - 2 * padding : imageSize - 2 * padding - profileLeftWidth;
            double rightHeight = (polaczeniaArray[3].typ == "T3" || polaczeniaArray[3].typ == "T2") ? imageSize - 2 * padding : imageSize - 2 * padding - profileRightWidth;

            // 🔥 Tworzenie kształtów ramy okna
            float halfProfile = (int)profileTopWidth / 2;

            // GÓRNY ELEMENT \-------/
            IPath topFrame = new Polygon(
                new PointF[]
                {
        new PointF(halfProfile, 0), // Lewy górny skos
        new PointF((float)imageSize - halfProfile, 0), // Prawy górny skos
        new PointF((float)imageSize, (float)profileTopWidth), // Prawy dolny
        new PointF(0, (float)profileTopWidth), // Lewy dolny
        new PointF(halfProfile, 0) // Powrót do początkowego punktu
                });

            // LEWY ELEMENT - obrót o 90°
            IPath leftFrame = new Polygon(
                new PointF[]
                {
        new PointF(0, halfProfile), // Lewy górny skos
        new PointF(0, (float)imageSize - halfProfile), // Lewy dolny skos
        new PointF((float)profileLeftWidth, (float)imageSize), // Prawy dolny
        new PointF((float)profileLeftWidth, 0), // Prawy górny
        new PointF(0, halfProfile) // Powrót do początkowego punktu
                });

            // DOLNY ELEMENT /-------\ (obrót o 180°)
            IPath bottomFrame = new Polygon(
                new PointF[]
                {
        new PointF(0, (float)imageSize - (float)profileBottomWidth), // Lewy górny
        new PointF((float)imageSize, (float)imageSize - (float)profileBottomWidth), // Prawy górny
        new PointF((float)imageSize - halfProfile, (float)imageSize), // Prawy dolny skos
        new PointF(halfProfile, (float)imageSize), // Lewy dolny skos
        new PointF(0, (float)imageSize - (float)profileBottomWidth) // Powrót do początkowego punktu
                });

            // PRAWY ELEMENT - obrót o 270° (lub -90°)
            IPath rightFrame = new Polygon(
                new PointF[]
                {
        new PointF((float)imageSize - (float)profileRightWidth, 0), // Lewy górny
        new PointF((float)imageSize - (float)profileRightWidth, (float)imageSize), // Lewy dolny
        new PointF((float)imageSize, (float)imageSize - halfProfile), // Prawy dolny skos
        new PointF((float)imageSize, halfProfile), // Prawy górny skos
        new PointF((float)imageSize - (float)profileRightWidth, 0) // Powrót do początkowego punktu
                });



            // 🔥 Przycięcie tekstury do kształtu ramy
            using Image<Rgba32> topTexture = woodTexture.Clone(x => x.Crop(new Rectangle(0, 0, (int)topWidth, (int)profileTopWidth)));
            using Image<Rgba32> bottomTexture = woodTexture.Clone(x => x.Crop(new Rectangle(0, 0, (int)bottomWidth, (int)profileBottomWidth)));
            using Image<Rgba32> leftTexture = woodTexture.Clone(x => x.Crop(new Rectangle(0, 0, (int)profileLeftWidth, (int)leftHeight)));
            using Image<Rgba32> rightTexture = woodTexture.Clone(x => x.Crop(new Rectangle(0, 0, (int)profileRightWidth, (int)rightHeight)));

            // 🔥 Rysowanie przyciętej tekstury na głównym obrazie
            image.Mutate(x =>
            {
                x.DrawImage(topTexture, new Point(padding, padding), 1f);
                x.DrawImage(bottomTexture, new Point(padding, imageSize - padding - (int)profileBottomWidth), 1f);
                x.DrawImage(leftTexture, new Point(padding, padding), 1f);
                x.DrawImage(rightTexture, new Point(imageSize - padding - (int)profileRightWidth, padding), 1f);

                // 🔥 Rysowanie obramowania ramy okna
                Pen borderPen = Pens.Solid(Color.Black, borderThickness);
                x.Draw(borderPen, topFrame);
                x.Draw(borderPen, bottomFrame);
                x.Draw(borderPen, leftFrame);
                x.Draw(borderPen, rightFrame);

                // 🔥 Dodanie "szyby" (jasnoniebieskie wypełnienie)
                int glassX = padding + (int)profileLeftWidth;
                int glassY = padding + (int)profileTopWidth;
                int glassWidth = imageSize - 2 * (padding + (int)profileLeftWidth);
                int glassHeight = imageSize - 2 * (padding + (int)profileTopWidth);

                IPath glassPath = new RectangularPolygon(glassX, glassY, glassWidth, glassHeight);
                x.Fill(Color.LightBlue, glassPath);
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
