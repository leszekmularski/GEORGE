using Aspose.ThreeD.Entities;
using GEORGE.Shared.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

public class ImageGenerator
{
    private readonly HttpClient _httpClient;

    public ImageGenerator(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<byte[]> GenerateImageAsync(List<KonfSystem> model, string polaczenia, int imageWidth, int imageHeight, string imageUrl, string glassColorHex)
    {
        try
        {
            if (model == null || model.Count == 0)
                throw new ArgumentException("Lista model jest pusta.");

            int borderThickness = 2;

            byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
            if (imageBytes == null || imageBytes.Length == 0)
                throw new Exception("Nie udało się pobrać tekstury.");

            using var woodTexture = Image.Load<Rgba32>(imageBytes);
            woodTexture.Mutate(x => x.Resize(imageWidth, imageHeight));

            // Zmiana: tworzenie obrazu z przezroczystym tłem zamiast białego
            using var image = new Image<Rgba32>(imageWidth, imageHeight, Color.Transparent);

            double profileTop = (model.FirstOrDefault(e => e.WystepujeGora)?.PionPrawa ?? 0) - (model.FirstOrDefault(e => e.WystepujeGora)?.PionLewa ?? 0);
            double profileRight = (model.FirstOrDefault(e => e.WystepujePrawa)?.PionPrawa ?? 0) - (model.FirstOrDefault(e => e.WystepujePrawa)?.PionLewa ?? 0);
            double profileBottom = (model.FirstOrDefault(e => e.WystepujeDol)?.PionPrawa ?? 0) - (model.FirstOrDefault(e => e.WystepujeDol)?.PionLewa ?? 0);
            double profileLeft = (model.FirstOrDefault(e => e.WystepujeLewa)?.PionPrawa ?? 0) - (model.FirstOrDefault(e => e.WystepujeLewa)?.PionLewa ?? 0);

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

            var frames = new List<(IPath path, Image<Rgba32> texture, Point position)>();

            void AddMiterFrame(int index, int x, int y, int width, int height, string leftType, string rightType)
            {
                Console.WriteLine($"AddMiterFrame ---> index: {index} leftType: {leftType} rightType: {rightType} x: {x} y: {y}");
                PointF[] points;
                int offset = (index % 2 == 0) ? height : width;
                bool isLeftT2 = leftType == "T2";
                bool isRightT2 = rightType == "T2";

                switch (index)
                {
                    case 0: // Top
                        points = new[]
                        {
                        new PointF(x, y),
                        new PointF(x + width, y),
                        new PointF(x + width - (isRightT2 ? offset : 0), y + height),
                        new PointF(x + (isLeftT2 ? offset : 0), y + height)
                    };
                        break;
                    case 1: // Right (odbicie lustrzane w poziomie)
                        {
                            var x0 = x;
                            var x1 = x + width;

                            var y0 = y;
                            var y1 = y + height;

                            // Zmienne do przesunięć 45°
                            var offsetTop = isRightT2 ? offset : 0;
                            var offsetBottom = isLeftT2 ? offset : 0;

                            // Zmodyfikowane punkty (odbite poziomo)
                            points = new[]
                            {
                            new PointF(x0, y0 + offsetTop),          // góra lewa
                            new PointF(x1, y0),                      // góra prawa
                            new PointF(x1, y1),                      // dół prawa
                            new PointF(x0, y1 - offsetBottom)        // dół lewa
                        };
                            break;
                        }
                    case 2: // Bottom
                        points = new[]
                        {
                        new PointF(x + (isLeftT2 ? offset : 0), y),
                        new PointF(x + width - (isRightT2 ? offset : 0), y),
                        new PointF(x + width, y + height),
                        new PointF(x, y + height)
                    };
                        break;
                    case 3: // Left
                        points = new[]
                        {
                        new PointF(x + width, y + (isLeftT2 ? offset : 0)),
                        new PointF(x + width, y + height - (isRightT2 ? offset : 0)),
                        new PointF(x, y + height),
                        new PointF(x, y)
                    };
                        break;
                    default:
                        throw new Exception($"Nieznany indeks ramki {index}");
                }

                if (height == 0) height = 1;
                if (width == 0) width = 1;

                var polygon = new Polygon(points);
                var cropped = woodTexture.Clone(c => c.Crop(new Rectangle(0, 0, width, height)));
                frames.Add((polygon, cropped, new Point(x, y)));
            }

            // Obliczenia pozycji i szerokości
            int topX = (joinTypes[0].Left == "T3") ? (int)profileLeft : 0;
            int topW = imageWidth - ((joinTypes[0].Left == "T3" ? (int)profileLeft : 0) +
                                     (joinTypes[0].Right == "T3" ? (int)profileRight : 0));

            int rightY = (joinTypes[1].Left == "T1") ? (int)profileTop : 0;
            int rightH = imageHeight - ((joinTypes[1].Left == "T1" ? (int)profileTop : 0) +
                                        (joinTypes[1].Right == "T1" ? (int)profileBottom : 0));

            int bottomX = 0;
            if (joinTypes[2].Right == "T3")
                bottomX += (int)profileLeft;

            int bottomW = imageWidth;
            if (joinTypes[2].Left == "T3")
                bottomW -= (int)profileLeft;
            if (joinTypes[2].Right == "T3")
                bottomW -= (int)profileRight;

            int leftY = (joinTypes[3].Right == "T1") ? (int)profileTop : 0;
            int leftH = imageHeight - ((joinTypes[3].Left == "T1" ? (int)profileTop : 0) +
                                       (joinTypes[3].Right == "T1" ? (int)profileBottom : 0));

            int wydSzybe = 0;

            if (joinTypes[2].Right == "T4" && joinTypes[2].Left == "T4")
            {
                bottomW = 1;
                wydSzybe += (int)profileLeft;
            }

            if (joinTypes[0].Left == "T5" && joinTypes[3].Right == "T5")
            {
                AddMiterFrame(3, (int)imageWidth / 2 - (int)profileLeft / 2, leftY, (int)profileLeft, leftH, joinTypes[3].Right, joinTypes[3].Left);

                image.Mutate(x =>
                {
                    foreach (var (path, texture, pos) in frames)
                    {
                        if (texture != null)
                        {
                            // Zmiana: usunięcie wypełnienia białym kolorem
                            x.DrawImage(texture, pos, 1f);
                        }
                        x.Draw(Pens.Solid(Color.Black, borderThickness), path);
                    }
                });
            }
            else
            {
                AddMiterFrame(0, topX, 0, topW, (int)profileTop, joinTypes[0].Left, joinTypes[0].Right);
                AddMiterFrame(1, imageWidth - (int)profileRight, rightY, (int)profileRight, rightH, joinTypes[1].Right, joinTypes[1].Left);
                AddMiterFrame(2, bottomX, imageHeight - (int)profileBottom, bottomW, (int)profileBottom, joinTypes[2].Right, joinTypes[2].Left);
                AddMiterFrame(3, 0, leftY, (int)profileLeft, leftH, joinTypes[3].Right, joinTypes[3].Left);

                // Szyba
                var glass = new RectangularPolygon(
                    (int)profileLeft,
                    (int)profileTop,
                    imageWidth - (int)profileLeft - (int)profileRight,
                    imageHeight - (int)profileTop - (int)profileBottom + wydSzybe
                );
                frames.Add((glass, null, new Point(0, 0)));

                image.Mutate(x =>
                {
                    foreach (var (path, texture, pos) in frames)
                    {
                        if (texture != null)
                        {
                            // Zmiana: usunięcie wypełnienia białym kolorem
                            x.DrawImage(texture, pos, 1f);
                        }
                        x.Draw(Pens.Solid(Color.Black, borderThickness), path);
                    }

                    // Jeśli chcesz, aby szkło też było przezroczyste, zmień kolor na przezroczysty
                    // x.Fill(Color.Transparent, glass);
                    // lub pozostaw obecny kolor jeśli chcesz zachować kolor szkła
                    x.Fill(Color.ParseHex(glassColorHex), glass);
                    x.Draw(Pens.Solid(Color.Black, borderThickness), glass);
                });
            }

            using var ms = new MemoryStream();
            image.SaveAsPng(ms); // Format PNG obsługuje przezroczystość
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Błąd generowania: {ex.Message} -> {ex.StackTrace}");
            return null;
        }
    }
}
