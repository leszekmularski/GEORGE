namespace GEORGE.Client.Pages.Wyroby
{
    public class LinePoint
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }

        private string _typeLine = string.Empty;

        public LinePoint(double x1, double y1, double x2, double y2, string typeLine)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            _typeLine = typeLine;
        }
    }
}
