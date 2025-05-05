namespace GEORGE.Client.Pages.KonfiguratorOkien
{

    public static class GeometryUtilsOffest
    {
        public static List<Point> OffsetPolygon(List<Point> polygon, float offset)
        {
            var result = new List<Point>();

            int count = polygon.Count;
            for (int i = 0; i < count; i++)
            {
                var prev = polygon[(i - 1 + count) % count];
                var curr = polygon[i];
                var next = polygon[(i + 1) % count];

                var v1 = Normalize(new Point(curr.X - prev.X, curr.Y - prev.Y));
                var v2 = Normalize(new Point(next.X - curr.X, next.Y - curr.Y));

                var n1 = new Point(-v1.Y, v1.X);
                var n2 = new Point(-v2.Y, v2.X);

                var bisector = Normalize(new Point(n1.X + n2.X, n1.Y + n2.Y));

                float dot = Dot(bisector, n1);
                if (Math.Abs(dot) < 0.0001f) dot = 0.0001f; // zabezpieczenie przed dzieleniem przez 0

                var newPoint = new Point(
                    curr.X + bisector.X * offset / dot,
                    curr.Y + bisector.Y * offset / dot
                );

                result.Add(newPoint);
            }

            return result;
        }

        private static float Dot(Point a, Point b) => a.X * b.X + a.Y * b.Y;

        private static Point Normalize(Point v)
        {
            float length = MathF.Sqrt(v.X * v.X + v.Y * v.Y);
            return length == 0 ? new Point(0, 0) : new Point(v.X / length, v.Y / length);
        }
    }

    public struct Point
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

}
