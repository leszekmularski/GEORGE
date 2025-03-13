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

    public async Task<byte[]> GenerateImageAsync(List<KonfSystem> model, string polaczenia, string imageUrl)
    {
        try
        {
            if (model == null || model.Count == 0)
                throw new ArgumentException("Lista model jest pusta.");

            int imageSize = 500;
            int borderThickness = 5;

            byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
            if (imageBytes == null || imageBytes.Length == 0)
                throw new Exception("Nie udało się pobrać tekstury.");

            using Image<Rgba32> woodTexture = Image.Load<Rgba32>(imageBytes);
            woodTexture.Mutate(x => x.Resize(imageSize, imageSize));

            using Image<Rgba32> image = new(imageSize, imageSize);
            image.Mutate(x => x.Fill(Color.White));

            // Pobranie szerokości profili
            double profileLeft = model.FirstOrDefault(e => e.WystepujeLewa)?.PionPrawa ?? 0 - model.FirstOrDefault(e => e.WystepujeLewa)?.PionLewa ?? 0;
            double profileTop = model.FirstOrDefault(e => e.WystepujeGora)?.PionPrawa ?? 0 - model.FirstOrDefault(e => e.WystepujeGora)?.PionLewa ?? 0;
            double profileRight = model.FirstOrDefault(e => e.WystepujePrawa)?.PionPrawa ?? 0 - model.FirstOrDefault(e => e.WystepujePrawa)?.PionLewa ?? 0;
            double profileBottom = model.FirstOrDefault(e => e.WystepujeDol)?.PionPrawa ?? 0 - model.FirstOrDefault(e => e.WystepujeDol)?.PionLewa ?? 0;

            profileBottom = Math.Max(profileBottom, 1); // Zapobieganie błędom

            var polaczeniaArray = polaczenia.Split(';')
                .Select(p => p.Split('-'))
                .Select(parts => (kat: int.Parse(parts[0]), typ: parts[1]))
                .ToArray();

            if (polaczeniaArray.Length < 4)
                throw new Exception("Niepełna konfiguracja połączeń.");

            var frames = new List<(IPath path, Image<Rgba32> texture, Point position)>();

            void AddFrame(int x, int y, int width, int height, string joinType)
            {
                var frame = new RectangularPolygon(x, y, width, height);
                var texture = woodTexture.Clone(tx => tx.Crop(new Rectangle(0, 0, width, height)));
                frames.Add((frame, texture, new Point(x, y)));
            }



            void AddTrapezoidFrame(int x, int y, int width, int height, string joinType, int cornerId, Image<Rgba32> woodTexture)
            {
                PointF[] trapezoidPoints;

                Console.WriteLine($"🎨 joinType: {joinType}, cornerId: {cornerId}, X: {x}, Y: {y}, Width: {width}, Height: {height}");

                // 🔹 Rozdzielenie `joinType` na lewą i prawą stronę
                string leftJoin = joinType.Substring(0, 2);
                string rightJoin = joinType.Substring(2, 2);

                // 🔥 Sprawdzamy, czy dany narożnik ma kąt 45°
                bool hasT2Left = leftJoin == "T2";
                bool hasT2Right = rightJoin == "T2";

                float innerOffset = height;  // 🔥 Przesunięcie krótszego boku trapezu do środka

                if (cornerId == 1 || cornerId == 3) innerOffset = width;

                // 🔹 Definiowanie punktów dla każdego narożnika
                switch (cornerId)
                {
                    case 0: // 🔹 Lewy górny róg (OK)
                        trapezoidPoints = new[]
                        {
                new PointF(x, y),  // Lewy górny
                new PointF(x + width, y),  // Prawy górny
                new PointF(x + width - (hasT2Right ? innerOffset : 0), y + height), // Prawy dolny 45°
                new PointF(x + (hasT2Left ? innerOffset : 0), y + height) // Lewy dolny 45°
            };
                        break;

                    case 1: // 🔹 Prawy górny róg (Poprawiony!)
                        trapezoidPoints = new[]
                        {
                new PointF(x, y + (hasT2Left ? innerOffset : 0)),  // Lewy górny
                new PointF(x + width, y),  // Prawy górny
                new PointF(x + width, y + height), // Prawy dolny 45°
                new PointF(x , y + height - (hasT2Right ? innerOffset : 0)) // Lewy dolny 45°
            };
                        break;

                    case 2: // 🔹 Prawy dolny róg (Teraz poprawny!)
                        trapezoidPoints = new[]
                        {
                new PointF(x + (hasT2Right ? innerOffset : 0), y),  // Lewy górny
                new PointF(x + width - (hasT2Left ? innerOffset : 0), y),  // Prawy górny
                new PointF(x + width , y + height), // Prawy dolny 45°
                new PointF(x , y + height) // Lewy dolny 45°
            };
                        break;

                    case 3: // 🔹 Lewy dolny róg (DODANY!)
                        trapezoidPoints = new[]
                        {
                new PointF(x, y),  // Lewy górny
                new PointF(x + width, y + (hasT2Left ? innerOffset : 0)),  // Prawy górny
                new PointF(x + width, y + height - (hasT2Right ? innerOffset : 0)), // Prawy dolny 45°
                new PointF(x, y + height ) // Lewy dolny 45°
            };
                        break;

                    default:
                        throw new Exception($"❌ Nieznany narożnik {cornerId}!");
                }

                // 🔥 Tworzenie trapezu i dodanie do ramy okna
                var trapezoid = new Polygon(trapezoidPoints);
                var textureTrapezoid = woodTexture.Clone(x => x.Crop(new Rectangle(0, 0, width, height)));
                frames.Add((trapezoid, textureTrapezoid, new Point(x, y)));

                Console.WriteLine($"✅ Dodano trapez: {cornerId} | Lewy 45°: {hasT2Left} | Prawy 45°: {hasT2Right}");
            }








            // 🔹 Korekta wymiarów elementów na podstawie połączeń T1/T3
            int goraWidth = imageSize - ((polaczeniaArray[0].typ.Trim() == "T3") ? (int)profileLeft : 0);
            goraWidth = imageSize - ((polaczeniaArray[1].typ.Trim() == "T3") ? (int)profileRight : 0);

            int dolWidth = imageSize - ((polaczeniaArray[2].typ.Trim() == "T3") ? (int)profileRight : 0);
            dolWidth = imageSize - ((polaczeniaArray[3].typ.Trim() == "T3") ? (int)profileLeft : 0);

            int prawaHeight = imageSize - ((polaczeniaArray[1].typ.Trim()  == "T3") ? (int)profileTop : 0);
            prawaHeight = imageSize - ((polaczeniaArray[2].typ.Trim() == "T3") ? (int)profileBottom : 0);

            if (polaczeniaArray[1].typ == "T3")
            {
                prawaHeight = imageSize - (int)profileTop;
            }

            int lewaHeight = imageSize - ((polaczeniaArray[0].typ == "T3") ? (int)profileTop : 0);
            lewaHeight = imageSize - ((polaczeniaArray[3].typ == "T3") ? (int)profileBottom : 0);

            if (polaczeniaArray[0].typ == "T3" && polaczeniaArray[1].typ == "T3")
            {
                goraWidth = imageSize - (int)profileLeft - (int)profileRight;
            }
            if (polaczeniaArray[2].typ == "T3" && polaczeniaArray[3].typ == "T3")
            {
                dolWidth = imageSize - (int)profileLeft - (int)profileRight;
            }
            if (polaczeniaArray[1].typ == "T3" && polaczeniaArray[2].typ == "T3")
            {
                prawaHeight = imageSize - (int)profileTop - (int)profileBottom;
            }
            if (polaczeniaArray[0].typ == "T3" && polaczeniaArray[3].typ == "T3")
            {
                lewaHeight = imageSize - (int)profileTop - (int)profileBottom;
            }

            int goraX = (polaczeniaArray[0].typ == "T3") ? (int)profileLeft : 0;
            int dolX = (polaczeniaArray[3].typ == "T3") ? (int)profileLeft : 0;
            int prawaY = (polaczeniaArray[1].typ == "T1") ? (int)profileTop : 0;
            int lewaY = (polaczeniaArray[0].typ == "T1") ? (int)profileBottom : 0;

            Console.WriteLine($"profileLeft:{profileLeft}, profileRight:{profileRight}, profileTop:{profileTop}, profileBottom:{profileBottom}");
            Console.WriteLine($"goraWidth:{goraWidth}, dolWidth:{dolWidth}, prawaHeight:{prawaHeight}, lewaHeight:{lewaHeight}");
            Console.WriteLine($"polaczeniaArray[0].typ:{polaczeniaArray[0].typ}, polaczeniaArray[1].typ:{polaczeniaArray[1].typ}, polaczeniaArray[2].typ:{polaczeniaArray[2].typ}, polaczeniaArray[3].typ:{polaczeniaArray[3].typ}");

            // 🔹 Pozycje dla rysowania
            var positions = new (int x, int y, int width, int height)[]
            {
                (goraX, 0, goraWidth, (int)profileTop),  // Góra
                (imageSize - (int)profileRight, prawaY, (int)profileRight, prawaHeight), // Prawa
                (dolX, imageSize - (int)profileBottom, dolWidth, (int)profileBottom), // Dół
                (0, lewaY, (int)profileLeft, lewaHeight)  // Lewa
            };

            // 🔥 Kolejność rysowania: najpierw poziome, potem pionowe
            var drawOrder = new List<(int index, string type)>
            {
                (0, polaczeniaArray[0].typ + polaczeniaArray[1].typ ), // Góra
                (1, polaczeniaArray[1].typ + polaczeniaArray[2].typ), // Lewa
                (2, polaczeniaArray[2].typ + polaczeniaArray[3].typ), // Dół
                (3, polaczeniaArray[3].typ + polaczeniaArray[0].typ)  // Prawa
            }
            .OrderBy(p => p.type == "T1" ? 0 : 1) // Najpierw T1 (poziome), potem T3 (pionowe)
            .ToList();

            foreach (var (index, type) in drawOrder)
            {
                var (x, y, width, height) = positions[index];

                AddTrapezoidFrame(x, y, width, height, type, index, woodTexture);
            }

            // 🪟 Dodajemy szybę w środku ramki
            IPath glassPath = new RectangularPolygon(
                (int)profileLeft,
                (int)profileTop,
                imageSize - (int)profileLeft - (int)profileRight,
                imageSize - (int)profileTop - (int)profileBottom
            );

            frames.Add((glassPath, null, new Point(0, 0))); // Szyba jako przezroczysty element

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

                // 🎨 Szyba – przezroczysta warstwa niebieska
                x.Fill(Color.LightBlue.WithAlpha(0.4f), glassPath);
                x.Draw(Pens.Solid(Color.Black, borderThickness), glassPath);
            });

            using MemoryStream ms = new();
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
