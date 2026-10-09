using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.Models
{
    public class XLineShape : IShapeDC
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }

        public string NazwaObj { get; set; } = "Linia";
        public bool RuchomySlupek { get; set; } = false;
        public bool StalySlupek { get; set; } = false;
        public bool PionPoziom { get; set; } = false;
        public bool DualRama { get; set; } = false;
        public bool IsSkosna { get; set; } = false;

        private double _scaleFactor = 1.0;

        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }

        public bool GenerowaneZRamy { get; set; } = false;

        public string? KsztaltModelu { get; set; } = "XLineShape";

        public List<XPoint> Points { get; set; } = new();
        public List<XPoint> NominalPoints { get; set; } = new();
        public List<XPoint> GetPoints() => Points;
        public List<XPoint> GetNominalPoints() =>
            NominalPoints.Select(p => new XPoint(p.X, p.Y)).ToList();

        public string ID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Identyfikator linii źródłowej dla odcinków powstałych po jej podziale.
        /// Brak wartości oznacza zwykłą, niepodzieloną linię.
        /// </summary>
        public string? SplitGroupId { get; set; }

        public List<ContourSegment> ContourSegments => GetContourSegments();

        // =====================================================================
        // WŁAŚCIWOŚCI POCHODNE (kąt, długość, orientacja)
        // =====================================================================

        public double KatLinii
        {
            get
            {
                double dx = X2 - X1;
                double dy = Y2 - Y1;

                double katRadiany = Math.Atan2(dy, dx);
                double katStopnie = katRadiany * (180.0 / Math.PI);

                if (katStopnie < 0) katStopnie += 180;
                else if (katStopnie >= 180) katStopnie -= 180;

                return katStopnie;
            }
        }

        public double KatLiniiRadiany
        {
            get
            {
                double dx = X2 - X1;
                double dy = Y2 - Y1;

                double katRadiany = Math.Atan2(dy, dx);

                if (katRadiany < 0) katRadiany += Math.PI;
                else if (katRadiany >= Math.PI) katRadiany -= Math.PI;

                return katRadiany;
            }
        }

        public double KatLiniiPelny
        {
            get
            {
                double dx = X2 - X1;
                double dy = Y2 - Y1;

                double katRadiany = Math.Atan2(dy, dx);
                double katStopnie = katRadiany * (180.0 / Math.PI);

                if (katStopnie < 0) katStopnie += 360;

                return katStopnie;
            }
        }

        public double DlugoscLinii
        {
            get
            {
                double dx = X2 - X1;
                double dy = Y2 - Y1;
                return Math.Sqrt(dx * dx + dy * dy);
            }
        }

        /// <summary>
        /// Linia pionowa: |X2 - X1| ≈ 0
        /// </summary>
        public bool CzyPionowa => Math.Abs(X2 - X1) < 0.001;

        /// <summary>
        /// Linia pozioma: |Y2 - Y1| ≈ 0
        /// </summary>
        public bool CzyPozioma => Math.Abs(Y2 - Y1) < 0.001;

        /// <summary>
        /// Linia skośna: nie pionowa i nie pozioma
        /// </summary>
        public bool CzyUkosna => !CzyPionowa && !CzyPozioma;

        // =====================================================================
        // KONSTRUKTOR – BEZ SORTOWANIA PUNKTÓW
        // =====================================================================

        public XLineShape(
            double x1, double y1, double x2, double y2, double scaleFactor,
            string nazwaObj, bool ruchomySlupek = false, bool pionPoziom = false,
            bool dualRama = false, bool generowaneZRamy = false,
            bool stalySlupek = false, bool isSkosna = false)
        {
            // ⭐ ZACHOWAJ ORYGINALNY KIERUNEK (bez sortowania)
            X1 = x1; Y1 = y1;
            X2 = x2; Y2 = y2;

            _scaleFactor = scaleFactor;
            NazwaObj = nazwaObj;
            RuchomySlupek = ruchomySlupek;
            PionPoziom = pionPoziom;
            DualRama = dualRama;
            GenerowaneZRamy = generowaneZRamy;
            StalySlupek = stalySlupek;
            IsSkosna = isSkosna;

            EnforceLineType();
            UpdateSize();
            UpdatePoints();
        }

        // =====================================================================
        // WYMUSZANIE TYPU LINII – POMIJA SKOSY
        // =====================================================================

        public void EnforceLineType()
        {
            if (RuchomySlupek)
            {
                // Ruchomy słupek: wymuś pionową linię w X1
                X2 = X1;
                return;
            }

            // Skosne linie – nie wymuszamy typu
            if (IsSkosna)
                return;

            if (PionPoziom)
            {
                double dx = Math.Abs(X2 - X1);
                double dy = Math.Abs(Y2 - Y1);

                if (dx >= dy)
                    Y2 = Y1;   // pozioma
                else
                    X2 = X1;   // pionowa
            }
        }

        // =====================================================================
        // AKTUALIZACJA PUNKTÓW – BEZ EnforceLineType
        // =====================================================================

        public void GeneratePoints()
        {
            // ⭐ BEZ EnforceLineType – typ jest już wymuszony wcześniej
            UpdatePoints();
        }

        private void UpdatePoints()
        {
            Points = new List<XPoint>
            {
                new XPoint(X1, Y1),
                new XPoint(X2, Y2)
            };

            NominalPoints = Points.Select(p => new XPoint(p.X, p.Y)).ToList();
        }

        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 2)
                return;

            X1 = newPoints[0].X;
            Y1 = newPoints[0].Y;
            X2 = newPoints[1].X;
            Y2 = newPoints[1].Y;

            // ⭐ NIE wymuszaj typu – punkty są źródłem prawdy
            UpdateSize();
            UpdatePoints();
        }

        private void UpdateSize()
        {
            Szerokosc = Math.Abs(X2 - X1);
            Wysokosc = Math.Abs(Y2 - Y1);
        }

        // =====================================================================
        // KLONOWANIE – ZACHOWUJE WSZYSTKIE FLAGI I ID
        // =====================================================================

        public IShapeDC Clone()
        {
            var clone = new XLineShape(
                X1, Y1, X2, Y2, _scaleFactor, NazwaObj,
                RuchomySlupek, PionPoziom, DualRama, GenerowaneZRamy,
                StalySlupek, IsSkosna)
            {
                SplitGroupId = SplitGroupId,
                ID = this.ID,
                Points = this.Points?.Select(p => new XPoint(p.X, p.Y)).ToList() ?? new(),
                NominalPoints = this.NominalPoints?.Select(p => new XPoint(p.X, p.Y)).ToList() ?? new(),

                Szerokosc = this.Szerokosc,
                Wysokosc = this.Wysokosc,
                KsztaltModelu = this.KsztaltModelu
            };

            return clone;
        }

        // =====================================================================
        // RYSOWANIE
        // =====================================================================

        public async Task Draw(Canvas2DContext ctx)
        {
            UpdateSize();

            if (RuchomySlupek)
                await ctx.SetStrokeStyleAsync("red");
            else if (DualRama)
                await ctx.SetStrokeStyleAsync("orange");
            else
                await ctx.SetStrokeStyleAsync("green");

            await ctx.SetLineWidthAsync(4);
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(X1, Y1);
            await ctx.LineToAsync(X2, Y2);
            await ctx.StrokeAsync();
        }

        // =====================================================================
        // PRZESUWANIE / SKALOWANIE / TRANSFORMACJA
        // =====================================================================

        public void Move(double offsetX, double offsetY)
        {
            X1 += offsetX;
            Y1 += offsetY;
            X2 += offsetX;
            Y2 += offsetY;

            UpdateSize();
            UpdatePoints();
        }

        public void Scale(double factor)
        {
            double cx = (X1 + X2) / 2;
            double cy = (Y1 + Y2) / 2;

            X1 = cx + (X1 - cx) * factor;
            Y1 = cy + (Y1 - cy) * factor;
            X2 = cx + (X2 - cx) * factor;
            Y2 = cy + (Y2 - cy) * factor;

            UpdateSize();
            UpdatePoints();
        }

        public void Transform(double scale, double offsetX, double offsetY)
        {
            X1 = X1 * scale + offsetX;
            Y1 = Y1 * scale + offsetY;
            X2 = X2 * scale + offsetX;
            Y2 = Y2 * scale + offsetY;

            UpdateSize();
            UpdatePoints();
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X1 = X1 * scaleX + offsetX;
            Y1 = Y1 * scaleY + offsetY;
            X2 = X2 * scaleX + offsetX;
            Y2 = Y2 * scaleY + offsetY;

            UpdateSize();
            UpdatePoints();
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(
                Math.Min(X1, X2),
                Math.Min(Y1, Y2),
                Math.Abs(X2 - X1),
                Math.Abs(Y2 - Y1),
                NazwaObj
            );
        }

        // =====================================================================
        // WŁAŚCIWOŚCI EDYTOWALNE – DYNAMICZNE CZY PIONOWA/POZIOMA
        // =====================================================================

        public List<EditableProperty> GetEditableProperties()
        {
            string? splitGroupId = SplitGroupId;
            string id = this.ID;

            // Statyczne wartości do wyświetlania w UI (nie do logiki)
            bool czyPionowaStatic = CzyPionowa;
            bool czyPoziomaStatic = CzyPozioma;
            bool czySkosnaStatic = CzyUkosna;

            // ⭐ Kierunek z chwili utworzenia właściwości (nie zmienia się w trakcie edycji)
            bool liniaJestPionowa = czyPionowaStatic || RuchomySlupek;
            bool liniaJestPozioma = czyPoziomaStatic;

            return new()
            {
            // ============ X1 ============
            new EditableProperty(
                RuchomySlupek ? "Podział linii w osi X1" : "X1 ",
                () => X1,
                v => {
                    double parsedValue = ParseExpression(v.ToString());

                    //// ⭐ Sprawdź kierunek PRZED zmianą
                    //bool bylaPionowa = Math.Abs(X2 - X1) < 0.001;
                    //bool bylaOsiowa = bylaPionowa || RuchomySlupek || StalySlupek;

                    X1 = parsedValue;

                    // ⭐ Jeśli linia była pionowa / słupkowa – przesuń całą linię
                    if (liniaJestPionowa) X2 = X1;

                    UpdateSize();
                    UpdatePoints();
                },
                NazwaObj,
                IsReadOnly: liniaJestPozioma,
                ShapeId: id)
                {
                    IsPionowa = czyPionowaStatic,
                    IsPozioma = czyPoziomaStatic,
                    IsSkosna = czySkosnaStatic,
                    SplitGroupId = splitGroupId
                },

            // ============ Y1 ============
            new EditableProperty(
                RuchomySlupek ? "Podział linii w osi Y1" : "Y1 ",
                () => Y1,
                v => {
                    double parsedValue = ParseExpression(v.ToString());

                    // ⭐ Sprawdź kierunek PRZED zmianą
                   // bool bylaPozioma = Math.Abs(Y2 - Y1) < 0.001;

                    Y1 = parsedValue;

                    // ⭐ Jeśli linia była pozioma – przesuń całą linię
                    if (liniaJestPozioma) Y2 = Y1;

                    UpdateSize();
                    UpdatePoints();
                },
                NazwaObj,
                IsReadOnly: liniaJestPionowa,
                ShapeId: id)
                {
                    IsPionowa = czyPionowaStatic,
                    IsPozioma = czyPoziomaStatic,
                    IsSkosna = czySkosnaStatic,
                    SplitGroupId = splitGroupId
                },

            // ============ X2 ============
            new EditableProperty(
                RuchomySlupek ? "Podział linii w osi X2" : "X2 ",
                () => X2,
                v => {
                    double parsedValue = ParseExpression(v.ToString());

                    // ⭐ Sprawdź kierunek PRZED zmianą
                    bool bylaPionowa = Math.Abs(X2 - X1) < 0.001;
                    bool bylaOsiowa = bylaPionowa || RuchomySlupek || StalySlupek;

                    X2 = parsedValue;

                    // ⭐ Jeśli linia była pionowa / słupkowa – przesuń całą linię
                    if (bylaOsiowa) X1 = X2;

                    UpdateSize();
                    UpdatePoints();
                },
                NazwaObj,
                IsReadOnly: liniaJestPionowa || liniaJestPozioma,
                ShapeId: id)
                {
                    IsPionowa = czyPionowaStatic,
                    IsPozioma = czyPoziomaStatic,
                    IsSkosna = czySkosnaStatic,
                    SplitGroupId = splitGroupId
                },

            // ============ Y2 ============
            new EditableProperty(
                RuchomySlupek ? "Podział linii w osi Y2" : "Y2 ",
                () => Y2,
                v => {
                    double parsedValue = ParseExpression(v.ToString());

                    // ⭐ Sprawdź kierunek PRZED zmianą
                    bool bylaPozioma = Math.Abs(Y2 - Y1) < 0.001;

                    Y2 = parsedValue;

                    // ⭐ Jeśli linia była pozioma – przesuń całą linię
                    if (bylaPozioma && !StalySlupek && !RuchomySlupek) Y1 = Y2;

                    UpdateSize();
                    UpdatePoints();
                },
                NazwaObj,
                IsReadOnly: liniaJestPozioma || liniaJestPionowa,
                ShapeId: id)
                {
                    IsPionowa = czyPionowaStatic,
                    IsPozioma = czyPoziomaStatic,
                    IsSkosna = czySkosnaStatic,
                    SplitGroupId = splitGroupId
                },

            // ============ Kąt (tylko odczyt) ============
            new EditableProperty(
                RuchomySlupek ? "Kąt linii" : "Kąt w stopniach ",
                () => KatLinii,
                v => { /* tylko odczyt */ },
                NazwaObj,
                IsReadOnly: true,
                ShapeId: id)
                {
                    IsPionowa = false,
                    IsPozioma = false,
                    IsSkosna = false,
                    SplitGroupId = splitGroupId
                },
            };
        }

        // =====================================================================
        // KONTUR (dla generatora regionów)
        // =====================================================================

        public List<ContourSegment> GetContourSegments()
        {
            var segments = new List<ContourSegment>();

            if (NominalPoints == null || NominalPoints.Count < 2)
                return segments;

            segments.Add(new ContourSegment(
                NominalPoints[0].Clone(),
                NominalPoints[1].Clone()
            ));

            return segments;
        }

        // =====================================================================
        // PARSER WYRAŻEŃ (bez zmian)
        // =====================================================================

        private double ParseExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return 0;

            if (double.TryParse(expression, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double result))
                return result;

            try
            {
                expression = expression.Replace(" ", "");

                if (expression.Contains('+') || expression.Contains('-'))
                {
                    var parts = System.Text.RegularExpressions.Regex.Split(expression, @"(?=[+-])");
                    double sum = 0;
                    foreach (var part in parts)
                    {
                        if (string.IsNullOrEmpty(part)) continue;
                        if (part.Contains('*') || part.Contains('/'))
                            sum += ParseMultiplicationDivision(part);
                        else
                            sum += double.Parse(part, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    return sum;
                }

                return ParseMultiplicationDivision(expression);
            }
            catch
            {
                return 0;
            }
        }

        private double ParseMultiplicationDivision(string expression)
        {
            if (expression.Contains('*') || expression.Contains('/'))
            {
                var parts = System.Text.RegularExpressions.Regex.Split(expression, @"(?=[*/])");
                double result = 1;
                bool firstPart = true;

                foreach (var part in parts)
                {
                    if (string.IsNullOrEmpty(part)) continue;

                    if (part.StartsWith("*"))
                    {
                        result *= double.Parse(part.Substring(1), System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else if (part.StartsWith("/"))
                    {
                        result /= double.Parse(part.Substring(1), System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else if (firstPart)
                    {
                        result = double.Parse(part, System.Globalization.CultureInfo.InvariantCulture);
                        firstPart = false;
                    }
                }

                return result;
            }

            return double.Parse(expression, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}