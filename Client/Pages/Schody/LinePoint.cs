namespace GEORGE.Client.Pages.Schody
{
    public class LinePoint
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }

        private string _typeLine = string.Empty;
        public string typeLine
        {
            get => _typeLine;
            set => _typeLine = value ?? string.Empty;
        }

        private string _fileNCName = string.Empty;
        public string fileNCName
        {
            get => _fileNCName;
            set => _fileNCName = value ?? string.Empty;
        }

        private string _nameMacro = string.Empty;
        public string nameMacro
        {
            get => _nameMacro;
            set => _nameMacro = value ?? string.Empty;
        }

        private string _idOBJ = string.Empty;
        public string idOBJ
        {
            get => _idOBJ;
            set => _idOBJ = value ?? string.Empty;
        }

        private string[] _zRobocze = Array.Empty<string>();
        public string[] zRobocze
        {
            get => _zRobocze;
            set => _zRobocze = value ?? Array.Empty<string>();
        }
        public double idRuchNarzWObj { get; set; }

        public bool addGcode { get; set; }

        public int IloscSztuk { get; set; }

        private string _nazwaProgramu = string.Empty;
        public string NazwaProgramu
        {
            get => _nazwaProgramu;
            set => _nazwaProgramu = value ?? string.Empty;
        }

        private string _nazwaElementu = string.Empty;
        public string NazwaElementu
        {
            get => _nazwaElementu;
            set => _nazwaElementu = value ?? string.Empty;
        }

        public LinePoint(double x1, double y1, double x2, double y2, string? typeLine = null, string? fileNCName = null,
                         string? nameMacro = null, string? idOBJ = null, string[]? zRobocze = null, double idRuchNarzWObj = 0, 
                         bool addGcode = false, int iloscSztuk = 0, string nazwaProgramu = "", string nazwaElementu = "")
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            this.typeLine = typeLine ?? string.Empty;
            this.fileNCName = fileNCName ?? string.Empty;
            this.nameMacro = nameMacro ?? string.Empty;
            this.idOBJ = idOBJ ?? string.Empty;
            this.zRobocze = zRobocze ?? Array.Empty<string>();
            this.idRuchNarzWObj = idRuchNarzWObj;
            this.addGcode = addGcode;
            IloscSztuk = iloscSztuk;
            this.NazwaProgramu = nazwaProgramu ?? string.Empty;
            this.NazwaElementu = nazwaElementu ?? string.Empty;
        }
    }
}
