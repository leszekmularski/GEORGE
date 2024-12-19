namespace GEORGE.Client.Pages.Schody
{
    public class LinePoint
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public string typeLine { get; set; }
        public bool addGcode { get; set; }
        public string fileNCName { get; set; }
        public string nameMacro { get; set; }
        public string idOBJ { get; set; }

        public LinePoint(double x1, double y1, double x2, double y2, string typeLine = "", string fileNCName = "", string nameMacro = "", string idOBJ = "", bool addGcode = false)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            this.typeLine = typeLine;
            this.fileNCName = fileNCName;
            this.nameMacro = nameMacro;
            this.idOBJ = idOBJ;
            this.addGcode = addGcode;
        }
    }
}
