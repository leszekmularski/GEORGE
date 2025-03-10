using System;
using System.IO;
using System.Net.Http;
using System.Linq;
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
            int windowFrameThickness = 20; // Grubość ramy wewnętrznej

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

            // 🔥 Długości elementów w zależności od typu połączenia
            int topWidth = (polaczeniaArray[0].typ == "T1" || polaczeniaArray[0].typ == "T2") ? imageSize - 2 * padding : imageSize - 2 * padding - profileWidth;
            int bottomWidth = (polaczeniaArray[2].typ == "T1" || polaczeniaArray[2].typ == "T2") ? imageSize - 2 * padding : imageSize - 2 * padding - profileWidth;
            int leftHeight = (polaczeniaArray[1].typ == "T3" || polaczeniaArray[1].typ == "T2") ? imageSize - 2 * padding : imageSize - 2 * padding - profileWidth;
            int rightHeight = (polaczeniaArray[3].typ == "T3" || polaczeniaArray[3].typ == "T2") ? imageSize - 2 * padding : imageSize - 2 * padding - profileWidth;

            // 🔥 Tworzenie kształtów ramy okna
            IPath topFrame = new RectangularPolygon(padding, padding, topWidth, profileWidth);
            IPath bottomFrame = new RectangularPolygon(padding, imageSize - padding - profileWidth, bottomWidth, profileWidth);
            IPath leftFrame = new RectangularPolygon(padding, padding, profileWidth, leftHeight);
            IPath rightFrame = new RectangularPolygon(imageSize - padding - profileWidth, padding, profileWidth, rightHeight);

            // 🔥 Przycięcie tekstury do kształtu ramy
            using Image<Rgba32> topTexture = woodTexture.Clone(x => x.Crop(new Rectangle(0, 0, topWidth, profileWidth)));
            using Image<Rgba32> bottomTexture = woodTexture.Clone(x => x.Crop(new Rectangle(0, 0, bottomWidth, profileWidth)));
            using Image<Rgba32> leftTexture = woodTexture.Clone(x => x.Crop(new Rectangle(0, 0, profileWidth, leftHeight)));
            using Image<Rgba32> rightTexture = woodTexture.Clone(x => x.Crop(new Rectangle(0, 0, profileWidth, rightHeight)));

            // 🔥 Rysowanie przyciętej tekstury na głównym obrazie
            image.Mutate(x =>
            {
                x.DrawImage(topTexture, new Point(padding, padding), 1f);
                x.DrawImage(bottomTexture, new Point(padding, imageSize - padding - profileWidth), 1f);
                x.DrawImage(leftTexture, new Point(padding, padding), 1f);
                x.DrawImage(rightTexture, new Point(imageSize - padding - profileWidth, padding), 1f);

                // 🔥 Rysowanie obramowania ramy okna
                Pen borderPen = Pens.Solid(Color.Black, borderThickness);
                x.Draw(borderPen, topFrame);
                x.Draw(borderPen, bottomFrame);
                x.Draw(borderPen, leftFrame);
                x.Draw(borderPen, rightFrame);

                // 🔥 Dodanie "szyby" (jasnoniebieskie wypełnienie)
                int glassX = padding + profileWidth;
                int glassY = padding + profileWidth;
                int glassWidth = imageSize - 2 * (padding + profileWidth);
                int glassHeight = imageSize - 2 * (padding + profileWidth);

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
