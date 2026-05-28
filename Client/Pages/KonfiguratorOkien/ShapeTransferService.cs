namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class ShapeTransferService
    {
        public List<IShapeDC> Shapes { get; set; } = new();

        // Wszystkie publiczne właściwości
        public double Szerokosc { get; set; } = 0;
        public double Wysokosc { get; set; } = 0;
        public bool RysujTylkoKontur { get; set; } = false;
        public bool OknaMogaBycLukowe { get; set; } = false;
        public List<EditableProperty> EditableProperties { get; set; } = new();

        public void UpdateShapes(List<IShapeDC> shapes, double szerokosc, double wysokosc)
        {
            Shapes = shapes;
            Szerokosc = szerokosc;
            Wysokosc = wysokosc;
            UpdateEditableProperties();
        }

        public void UpdateEditableProperties()
        {

            EditableProperties.Clear();

            if (Shapes != null && Shapes.Any())
            {
                bool ukrySzerokoscIWysokosc = false;
                var s = Shapes.FirstOrDefault()?.GetEditableProperties().FirstOrDefault(x => x.Label.Contains("Promień Okna:", StringComparison.OrdinalIgnoreCase) 
                || x.Label.Contains("Wymiar okna kwadratowego", StringComparison.OrdinalIgnoreCase));

                if (s != null)
                {
                    ukrySzerokoscIWysokosc = true;
                }

                // Sprawdź czy jakikolwiek kształt ma właściwość "Szerokość"
                bool maSzerokosc = Shapes.Any(x =>
                    x.GetEditableProperties()?.Any(y => y.Label.Contains("Szerokość", StringComparison.OrdinalIgnoreCase)) == true);

                if (!maSzerokosc)
                {
                    EditableProperties.Add(new EditableProperty(
                        "Szerokość: ",
                        () => Szerokosc,
                        v => { Szerokosc = (int)v; },
                        "Wymiar okna", false, false, false, ukrySzerokoscIWysokosc
                    ));
                }

                // Sprawdź czy jakikolwiek kształt ma właściwość "Wysokość"
                bool maWysokosc = Shapes.Any(x =>
                x.GetEditableProperties()?.Any(y => y.Label.Contains("Wysokość", StringComparison.OrdinalIgnoreCase)) == true);

                if (!maWysokosc)
                {
                    EditableProperties.Add(new EditableProperty(
                        "Wysokość: ",
                        () => Wysokosc,
                        v => { Wysokosc = (int)v; },
                        "Wymiar okna", false, false, false, ukrySzerokoscIWysokosc
                    ));
                }

                // Dodaj właściwości z kształtów
                foreach (var shape in Shapes)
                {
                    var shapeProperties = shape.GetEditableProperties();
                    if (shapeProperties != null && shapeProperties.Any())
                    {
                        EditableProperties.AddRange(shapeProperties);
                    }
                }

            }
        }

        public string GetShapesAsJson()
        {
            var shapeData = new
            {
                Szerokosc = Szerokosc,
                Wysokosc = Wysokosc,
                RysujTylkoKontur = RysujTylkoKontur,
                OknaMogaBycLukowe = OknaMogaBycLukowe,
                Shapes = Shapes,
                EditableProperties = EditableProperties,

            };
            return System.Text.Json.JsonSerializer.Serialize(shapeData);
        }
        public BoundingBox GetGlobalBoundingBox()
        {
            var shapesWithPoints = Shapes
                .Where(s => s.Points is { Count: > 0 })
                .ToList();

            if (!shapesWithPoints.Any())
                return new BoundingBox
                {
                    MinX = 0,
                    MinY = 0,
                    MaxX = 1,
                    MaxY = 1
                };

            var minX = shapesWithPoints.Min(s => s.Points.Min(p => p.X));
            var minY = shapesWithPoints.Min(s => s.Points.Min(p => p.Y));
            var maxX = shapesWithPoints.Max(s => s.Points.Max(p => p.X));
            var maxY = shapesWithPoints.Max(s => s.Points.Max(p => p.Y));

            return new BoundingBox
            {
                MinX = minX,
                MinY = minY,
                MaxX = maxX,
                MaxY = maxY
            };
        }

        public readonly struct BoundingBox
        {
            public double MinX { get; init; }
            public double MinY { get; init; }
            public double MaxX { get; init; }
            public double MaxY { get; init; }

            public double Width => MaxX - MinX;
            public double Height => MaxY - MinY;

            public double CenterX => (MinX + MaxX) * 0.5;
            public double CenterY => (MinY + MaxY) * 0.5;
        }

    }
}