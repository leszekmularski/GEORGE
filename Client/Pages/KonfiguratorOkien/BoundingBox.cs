using GEORGE.Client.Pages.Models;

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
    }

    //public struct PointDC
    //{
    //    public double X { get; set; }
    //    public double Y { get; set; }
    //    public PointDC(double x, double y) { X = x; Y = y; }
    //}

}
