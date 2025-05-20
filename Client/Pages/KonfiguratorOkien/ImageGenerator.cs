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

            int borderThickness = 5;
            byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
            if (imageBytes == null || imageBytes.Length == 0)
                throw new Exception("Nie udało się pobrać tekstury.");

            using Image<Rgba32> woodTexture = Image.Load<Rgba32>(imageBytes);
            woodTexture.Mutate(x => x.Resize(imageWidth, imageHeight));
            using Image<Rgba32> image = new(imageWidth, imageHeight);
            image.Mutate(x => x.Fill(Color.White));

            double profileTop = model.FirstOrDefault(e => e.WystepujeGora)?.PionPrawa ?? 0 - model.FirstOrDefault(e => e.WystepujeGora)?.PionLewa ?? 0;
            double profileRight = model.FirstOrDefault(e => e.WystepujePrawa)?.PionPrawa ?? 0 - model.FirstOrDefault(e => e.WystepujePrawa)?.PionLewa ?? 0;
            double profileBottom = model.FirstOrDefault(e => e.WystepujeDol)?.PionPrawa ?? 0 - model.FirstOrDefault(e => e.WystepujeDol)?.PionLewa ?? 0;
            double profileLeft = model.FirstOrDefault(e => e.WystepujeLewa)?.PionPrawa ?? 0 - model.FirstOrDefault(e => e.WystepujeLewa)?.PionLewa ?? 0;

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
            (Left: polaczeniaArray[3].typ, Right: polaczeniaArray[2].typ), // Bottom
            (Left: polaczeniaArray[0].typ, Right: polaczeniaArray[3].typ), // Left
        };

            var frames = new List<(IPath path, Image<Rgba32> texture, Point position)>();

            void AddFrameElement(int index, int x, int y, int width, int height, string leftType, string rightType)
            {
                PointF[] points;
                int offset = (index % 2 == 0) ? height : width;
                bool isLeftT2 = leftType == "T2";
                bool isRightT2 = rightType == "T2";

                switch (index)
                {
                    case 0: // Top
                        points = new[]
                        {
                        new PointF(x + (isLeftT2 ? offset : 0), y + height),
                        new PointF(x + width - (isRightT2 ? offset : 0), y + height),
                        new PointF(x + width, y),
                        new PointF(x, y)
                    };
                        break;
                    case 1: // Right
                        points = new[]
                        {
                        new PointF(x, y),
                        new PointF(x + width, y + (isRightT2 ? offset : 0)),
                        new PointF(x + width, y + height - (isLeftT2 ? offset : 0)),
                        new PointF(x, y + height)
                    };
                        break;
                    case 2: // Bottom
                        points = new[]
                        {
                        new PointF(x, y),
                        new PointF(x + width, y),
                        new PointF(x + width - (isRightT2 ? offset : 0), y + height),
                        new PointF(x + (isLeftT2 ? offset : 0), y + height)
                    };
                        break;
                    case 3: // Left
                        points = new[]
                        {
                        new PointF(x + width, y),
                        new PointF(x + width, y + height),
                        new PointF(x, y + height - (isRightT2 ? offset : 0)),
                        new PointF(x, y + (isLeftT2 ? offset : 0))
                    };
                        break;
                    default:
                        throw new Exception($"Nieznany index {index}");
                }

                var trapezoid = new Polygon(points);
                var cropped = woodTexture.Clone(x => x.Crop(new Rectangle(0, 0, width, height)));
                frames.Add((trapezoid, cropped, new Point(x, y)));
            }

            // Pozycje i wymiary
            int topX = (joinTypes[0].Left == "T3") ? (int)profileLeft : 0;
            int topWidth = imageWidth - ((joinTypes[0].Left == "T3" ? (int)profileLeft : 0) + (joinTypes[0].Right == "T3" ? (int)profileRight : 0));

            int rightY = (joinTypes[1].Left == "T1") ? (int)profileTop : 0;
            int rightHeight = imageHeight - ((joinTypes[1].Left == "T1" ? (int)profileTop : 0) + (joinTypes[1].Right == "T1" ? (int)profileBottom : 0));

            int bottomX = (joinTypes[2].Left == "T3") ? (int)profileLeft : 0;
            int bottomWidth = imageWidth - ((joinTypes[2].Left == "T3" ? (int)profileLeft : 0) + (joinTypes[2].Right == "T3" ? (int)profileRight : 0));

            int leftY = (joinTypes[3].Left == "T1") ? (int)profileTop : 0;
            int leftHeight = imageHeight - ((joinTypes[3].Left == "T1" ? (int)profileTop : 0) + (joinTypes[3].Right == "T1" ? (int)profileBottom : 0));

            // Dodaj ramki
            AddFrameElement(0, topX, 0, topWidth, (int)profileTop, joinTypes[0].Left, joinTypes[0].Right);
            AddFrameElement(1, imageWidth - (int)profileRight, rightY, (int)profileRight, rightHeight, joinTypes[1].Left, joinTypes[1].Right);
            AddFrameElement(2, bottomX, imageHeight - (int)profileBottom, bottomWidth, (int)profileBottom, joinTypes[2].Left, joinTypes[2].Right);
            AddFrameElement(3, 0, leftY, (int)profileLeft, leftHeight, joinTypes[3].Left, joinTypes[3].Right);

            // Dodaj szybę
            var glassPath = new RectangularPolygon(
                (int)profileLeft,
                (int)profileTop,
                imageWidth - (int)profileLeft - (int)profileRight,
                imageHeight - (int)profileTop - (int)profileBottom
            );
            frames.Add((glassPath, null, new Point(0, 0)));

            image.Mutate(x =>
            {
                foreach (var (path, texture, position) in frames)
                {
                    if (texture != null)
                    {
                        x.Fill(Color.White, path);
                        x.DrawImage(texture, position, 1f);
                    }
                    x.Draw(Pens.Solid(Color.Black, borderThickness), path);
                }

                Color glassColor = Color.ParseHex(glassColorHex);
                x.Fill(glassColor, glassPath);
                x.Draw(Pens.Solid(Color.Black, borderThickness), glassPath);
            });

            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message} / {ex.StackTrace}");
            return null;
        }
    }

}
