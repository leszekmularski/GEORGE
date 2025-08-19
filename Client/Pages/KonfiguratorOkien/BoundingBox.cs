using GEORGE.Client.Pages.Models;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class BoundingBox
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string NazwaObiektu { get; set; }

        public BoundingBox(double x, double y, double width, double height, string nazwaObiektu)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            NazwaObiektu = nazwaObiektu;
        }

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            return new List<(XPoint, XPoint)>
    {
        (new XPoint(X, Y), new XPoint(X + Width, Y)),         // Górna krawędź
        (new XPoint(X + Width, Y), new XPoint(X + Width, Y + Height)), // Prawa krawędź
        (new XPoint(X + Width, Y + Height), new XPoint(X, Y + Height)), // Dolna krawędź
        (new XPoint(X, Y + Height), new XPoint(X, Y))          // Lewa krawędź
    };
        }

        public bool Contains(double pointX, double pointY) =>
            pointX >= X && pointX <= X + Width &&
            pointY >= Y && pointY <= Y + Height;

        public static BoundingBox Union(BoundingBox b1, BoundingBox b2)
        {
            double minX = Math.Min(b1.X, b2.X);
            double minY = Math.Min(b1.Y, b2.Y);
            double maxX = Math.Max(b1.X + b1.Width, b2.X + b2.Width);
            double maxY = Math.Max(b1.Y + b1.Height, b2.Y + b2.Height);

            return new BoundingBox(
                minX,
                minY,
                maxX - minX,
                maxY - minY,
                $"{b1.NazwaObiektu}+{b2.NazwaObiektu}"
            );
        }


    }

}
