namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class ShapeTransferService
    {
        public List<IShapeDC> Shapes { get; set; } = new();

        public int _szerokosc { get; set; } = 0;
        public int _wysokosc { get; set; } = 0;

        public string GetShapesAsJson()
        {
            var shapeData = new
            {
                Szerokosc = _szerokosc,
                Wysokosc = _wysokosc,
                Shapes = Shapes
            };
            return System.Text.Json.JsonSerializer.Serialize(shapeData);
        }

        // 🔽 NOWE — bounding box wszystkich figur
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

        //public class BoundingBox
        //{
        //    public double MinX { get; set; }
        //    public double MinY { get; set; }
        //    public double MaxX { get; set; }
        //    public double MaxY { get; set; }

        //    public double Width => MaxX - MinX;
        //    public double Height => MaxY - MinY;
        //}

    }

}
