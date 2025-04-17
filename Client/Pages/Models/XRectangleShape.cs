using GEORGE.Client.Pages.KonfiguratorOkien;

namespace GEORGE.Client.Pages.Models
{
    public class XRectangleShape
    {
        public List<XPoint>? Xpoints;
        public List<XLinePoint>? XLinePoint;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string? NazwaObiektu { get; set; } = "Prostokąt";

        public XRectangleShape(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        
        public XRectangleShape() { }

        public XRectangleShape(double x, double y, double width, double height, string? nazwaObiektu = null)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            NazwaObiektu = nazwaObiektu;
        }

        /// <summary>
        /// Zwraca listę krawędzi (odcinków) prostokąta jako pary punktów Start-End.
        /// </summary>
        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            return new List<(XPoint, XPoint)>
            {
                (new XPoint(X, Y), new XPoint(X + Width, Y)), // Górna
                (new XPoint(X + Width, Y), new XPoint(X + Width, Y + Height)), // Prawa
                (new XPoint(X + Width, Y + Height), new XPoint(X, Y + Height)), // Dolna
                (new XPoint(X, Y + Height), new XPoint(X, Y)) // Lewa
            };
        }

        /// <summary>
        /// Zwraca nową instancję kopii tego bounding boxa.
        /// </summary>
        public XBoundingBox GetBoundingBox()
        {
            return new XBoundingBox(X, Y, Width, Height, NazwaObiektu);
        }

        /// <summary>
        /// Zwraca listę wierzchołków prostokąta w kolejności (zgodnie z ruchem wskazówek zegara).
        /// </summary>
        public List<PointDC> GetCorners()
        {
            return new List<PointDC>
        {
            new(X, Y),
            new(X + Width, Y),
            new(X + Width, Y + Height),
            new(X, Y + Height)
        };
        }
    }
}
