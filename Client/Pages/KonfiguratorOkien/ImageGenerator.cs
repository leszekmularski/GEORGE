using System;
using System.IO;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
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

    public async Task<byte[]> GenerateImageAsync(KonfSystem model, string polaczenia, string imageUrl)
    {
        try
        {
            int imageSize = 500; // Rozmiar całego obrazu
            int profileWidth = 40; // Szerokość profilu okiennego
            int borderThickness = 5; // Grubość obramowania
            int padding = 50; // Marginesy od krawędzi
            int windowFrameThickness = 20; // Grubość ramy w środku okna (między szybą)

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

            // 🔥 Ustawienie białego tła
            image.Mutate(x => x.Fill(Color.White));

            // 🔥 Tworzenie profili okiennych pod kątem 45° i podział na 4 części
            image.Mutate(x =>
            {
                Pen borderPen = Pens.Solid(Color.Black, borderThickness);
                Brush glassBrush = Brushes.Solid(Color.LightBlue); // "Szyba" (jasnoniebieski)

                // 🔥 Definiowanie ramy okna: 2 poziome i 2 pionowe elementy
                var pathBuilder = new PathBuilder();

                // Górna część ramy
                pathBuilder.MoveTo(new PointF(padding, padding)); // lewy górny róg
                pathBuilder.LineTo(new PointF(imageSize - padding, padding)); // prawy górny róg
                pathBuilder.LineTo(new PointF(imageSize - padding, padding + profileWidth)); // prawy dolny róg
                pathBuilder.LineTo(new PointF(padding, padding + profileWidth)); // lewy dolny róg
                pathBuilder.CloseFigure();

                // Dolna część ramy
                pathBuilder.MoveTo(new PointF(padding, imageSize - padding)); // lewy dolny róg
                pathBuilder.LineTo(new PointF(imageSize - padding, imageSize - padding)); // prawy dolny róg
                pathBuilder.LineTo(new PointF(imageSize - padding, imageSize - padding - profileWidth)); // prawy górny róg
                pathBuilder.LineTo(new PointF(padding, imageSize - padding - profileWidth)); // lewy górny róg
                pathBuilder.CloseFigure();

                // Lewa część ramy
                pathBuilder.MoveTo(new PointF(padding, padding)); // lewy górny róg
                pathBuilder.LineTo(new PointF(padding + profileWidth, padding + profileWidth)); // lewy dolny róg
                pathBuilder.LineTo(new PointF(padding + profileWidth, imageSize - padding - profileWidth)); // lewy dolny róg
                pathBuilder.LineTo(new PointF(padding, imageSize - padding)); // lewy górny róg
                pathBuilder.CloseFigure();

                // Prawa część ramy
                pathBuilder.MoveTo(new PointF(imageSize - padding, padding)); // prawy górny róg
                pathBuilder.LineTo(new PointF(imageSize - padding - profileWidth, padding + profileWidth)); // prawy dolny róg
                pathBuilder.LineTo(new PointF(imageSize - padding - profileWidth, imageSize - padding - profileWidth)); // prawy dolny róg
                pathBuilder.LineTo(new PointF(imageSize - padding, imageSize - padding)); // prawy górny róg
                pathBuilder.CloseFigure();

                // 🔥 Wypełnianie ramy teksturą
                Rectangle topFrame = new Rectangle(padding, padding, imageSize - 2 * padding, profileWidth);
                Rectangle bottomFrame = new Rectangle(padding, imageSize - padding - profileWidth, imageSize - 2 * padding, profileWidth);
                Rectangle leftFrame = new Rectangle(padding, padding, profileWidth, imageSize - 2 * padding);
                Rectangle rightFrame = new Rectangle(imageSize - padding - profileWidth, padding, profileWidth, imageSize - 2 * padding);

                // Rysowanie tekstury na ramie
                x.DrawImage(woodTexture, topFrame, 1f); // Na górnej części
                x.DrawImage(woodTexture, bottomFrame, 1f); // Na dolnej części
                x.DrawImage(woodTexture, leftFrame, 1f); // Na lewej części
                x.DrawImage(woodTexture, rightFrame, 1f); // Na prawej części

                // Rysowanie obramowania
                x.Draw(borderPen, pathBuilder.Build());

                // 🔥 Dodawanie "szyby" - wypełnienie środka okna
                var glassPath = new PathBuilder();
                // Centralny prostokąt (szyba)
                glassPath.MoveTo(new PointF(padding + profileWidth + windowFrameThickness, padding + profileWidth + windowFrameThickness));
                glassPath.LineTo(new PointF(imageSize - padding - profileWidth - windowFrameThickness, padding + profileWidth + windowFrameThickness));
                glassPath.LineTo(new PointF(imageSize - padding - profileWidth - windowFrameThickness, imageSize - padding - profileWidth - windowFrameThickness));
                glassPath.LineTo(new PointF(padding + profileWidth + windowFrameThickness, imageSize - padding - profileWidth - windowFrameThickness));
                glassPath.CloseFigure();

                // Wypełnienie szyby jasnoniebieskim kolorem
                x.Fill(glassBrush, glassPath.Build());
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
