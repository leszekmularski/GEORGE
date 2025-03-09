using System;
using System.IO;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using GEORGE.Shared.Models;

public class ImageGenerator
{
    public static byte[] GenerateImage(KonfSystem model, string polaczenia)
    {
        try
        {
            int imageSize = 500;
            int borderThickness = 10;
            int padding = 50;

            string texturePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "textures", "wood.jpg");

            if (!File.Exists(texturePath))
            {
                Console.WriteLine($"❌ Błąd: Plik {texturePath} nie istnieje!");
                return null;
            }

            // 🔥 Wczytaj teksturę drewna
            using Image<Rgba32> woodTexture = Image.Load<Rgba32>("/textures/wood.jpg");

            // 🔥 Dopasowanie tekstury do wymiarów obrazu
            woodTexture.Mutate(x => x.Resize(imageSize, imageSize));

            // 🔥 Tworzymy nowy obraz i rysujemy tło
            using Image<Rgba32> image = new(imageSize, imageSize);
            image.Mutate(x => x.DrawImage(woodTexture, new Point(0, 0), 1f)); // Wypełnienie teksturą

            // 🔥 Obliczenie szerokości linii na podstawie modelu
            int leftWidth = (int)(model.PionLewa ?? 10);
            int rightWidth = (int)(model.PionPrawa ?? 10);
            int topWidth = (int)(model.PoziomGora ?? 10);
            int bottomWidth = (int)(model.PoziomDol ?? 10);

            // 🔥 Rysowanie ramki, linii i tekstu
            image.Mutate(x =>
            {
                // 🔥 Rysowanie ramki (czarny kwadrat)
                x.Draw(Color.Black, borderThickness, new RectangularPolygon(padding, padding, imageSize - 2 * padding, imageSize - 2 * padding));

                Pen pen = Pens.Solid(Color.Blue, 5);

                // 🔥 Tworzenie linii pionowych
                var pathBuilder = new PathBuilder();
                pathBuilder.AddLine(new PointF(padding + leftWidth, padding), new PointF(padding + leftWidth, imageSize - padding));
                pathBuilder.AddLine(new PointF(imageSize - padding - rightWidth, padding), new PointF(imageSize - padding - rightWidth, imageSize - padding));

                // 🔥 Tworzenie linii poziomych
                pathBuilder.AddLine(new PointF(padding, padding + topWidth), new PointF(imageSize - padding, padding + topWidth));
                pathBuilder.AddLine(new PointF(padding, imageSize - padding - bottomWidth), new PointF(imageSize - padding, imageSize - padding - bottomWidth));

                // 🔥 Rysowanie linii
                IPath path = pathBuilder.Build();
                x.Draw(pen, path);

                // 🔥 Dodanie opisu sposobu łączenia
                FontCollection collection = new();
                FontFamily family = collection.AddSystemFonts().Families.FirstOrDefault();
                if (family == null)
                {
                    family = SystemFonts.Families.First();
                }
                Font font = family.CreateFont(16);
                x.DrawText(polaczenia, font, Color.Black, new PointF(10, 10));
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
