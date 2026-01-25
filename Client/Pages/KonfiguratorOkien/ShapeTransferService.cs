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
                .Where(s => s.Points != null && s.Points.Count > 0)
                .ToList();

            if (!shapesWithPoints.Any())
                return new BoundingBox { MinX = 0, MinY = 0, MaxX = 1, MaxY = 1 };

            double minX = shapesWithPoints.Min(s => s.Points.Min(p => p.X));
            double minY = shapesWithPoints.Min(s => s.Points.Min(p => p.Y));
            double maxX = shapesWithPoints.Max(s => s.Points.Max(p => p.X));
            double maxY = shapesWithPoints.Max(s => s.Points.Max(p => p.Y));

            return new BoundingBox
            {
                MinX = minX,
                MinY = minY,
                MaxX = maxX,
                MaxY = maxY
            };
        }

        public class BoundingBox
        {
            public double MinX { get; set; }
            public double MinY { get; set; }
            public double MaxX { get; set; }
            public double MaxY { get; set; }

            public double Width => MaxX - MinX;
            public double Height => MaxY - MinY;
        }

    }

}
