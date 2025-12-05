using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Shared.ViewModels;
using System.Linq;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class XTriangleShape : IShapeDC
    {
        // Pozycje bazowe trójkąta (nominalne)
        public double BaseX1 { get; set; }
        public double BaseY { get; set; }
        public double BaseWidth { get; set; }
        public double Height { get; set; }
        public string NazwaObj { get; set; } = "Trójkąt";

        // Skala CANVAS (px per nominal unit)
        private double _scaleFactor = 1.0;

        // Właściwości interfejsu
        public double Szerokosc { get; set; }
        public double Wysokosc { get; set; }

        // ◆ NominalPoints = punkty w jednostkach logicznych (mm) — źródło prawdy geometrycznej
        public List<XPoint> NominalPoints { get; set; } = new();

        // ◆ Points = punkty przeskalowane, służą do rysowania (canvas)
        public List<XPoint> Points { get; set; } = new();

        public string ID { get; set; } = Guid.NewGuid().ToString();

        // ---------------------------------------------------------
        // Konstruktor
        // startX,startY,endX,endY traktujemy jako nominalne współrzędne
        // scaleFactor to początkowa skala canvas (np. 1.0)
        // ---------------------------------------------------------
        public XTriangleShape(double startX, double startY, double endX, double endY, double scaleFactor = 1.0)
        {
            BaseX1 = Math.Min(startX, endX);
            BaseY = Math.Max(startY, endY);
            BaseWidth = Math.Abs(endX - startX);
            Height = Math.Abs(startY - endY);

            _scaleFactor = scaleFactor;

            // ustaw nominalne punkty i przeskalowane
            NominalPoints = GeneratePoints();
            ApplyScaleToPoints();

            // ustaw wymiary nominalne
            Szerokosc = BaseWidth;
            Wysokosc = Height;
        }

        // ---------------------------------------------------------
        // Generujemy punkty na podstawie parametrów bazowych (nominal)
        // Zwraca listę punktów nominalnych (bez skali)GetPoints
        // ---------------------------------------------------------
        private List<XPoint> GeneratePoints()
        {
            var apexX = BaseX1 + BaseWidth / 2.0;
            var apexY = BaseY - Height;
            var baseX2 = BaseX1 + BaseWidth;

            return new List<XPoint>
            {
                new XPoint(apexX, apexY),      // górny wierzchołek
                new XPoint(baseX2, BaseY),     // prawy dolny
                new XPoint(BaseX1, BaseY)      // lewy dolny
            };
        }

        // ---------------------------------------------------------
        // Zastosuj aktualną skalę canvas do nominalPoints -> Points
        // ---------------------------------------------------------
        private void ApplyScaleToPoints()
        {
            Points = NominalPoints
                .Select(p => new XPoint(p.X * _scaleFactor, p.Y * _scaleFactor))
                .ToList();
        }

        // ---------------------------------------------------------
        // Aktualizacja punktów (przychodzące newPoints traktujemy jako CANVAS points lub NOMINAL?
        // W tej implementacji używamy ich jako NOMINAL (jeżeli chcesz przyjmować canvasowe, trzeba przeskalować)
        // ---------------------------------------------------------
        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count != 3)
                return;

            // Zakładamy, że newPoints są nominalnymi współrzędnymi (bez skali).
            // Jeśli twoje UI/drag zwraca canvasowe punkty, musisz je wcześniej przeskalować odwrotnie: /_scaleFactor

            // Skopiuj do nominalnych
            NominalPoints = newPoints.Select(p => p.Clone()).ToList();

            // Odzyskaj bazowe parametry (BaseX1, BaseWidth, BaseY, Height) z nominalnych
            var apex = NominalPoints[0];
            var rightBase = NominalPoints[1];
            var leftBase = NominalPoints[2];

            BaseX1 = Math.Min(leftBase.X, rightBase.X);
            BaseY = Math.Max(leftBase.Y, rightBase.Y);

            // ensure base X ordering
            BaseWidth = Math.Abs(rightBase.X - leftBase.X);
            Height = Math.Abs(apex.Y - BaseY);

            // aktualizacja wymiarów nominalnych
            Szerokosc = BaseWidth;
            Wysokosc = Height;

            // odbuduj nominalne punkty w spójnej kolejności (apex, right, left)
            NominalPoints = GeneratePoints();

            // przelicz canvasowe punkty
            ApplyScaleToPoints();
        }

        // ---------------------------------------------------------
        public IShapeDC Clone()
        {
            var clone = new XTriangleShape(BaseX1, BaseY - Height, BaseX1 + BaseWidth, BaseY, _scaleFactor);
            // Ensure same ID? new ID is ok
            return clone;
        }

        // ---------------------------------------------------------
        public async Task Draw(Canvas2DContext ctx)
        {
            // Rysujemy na podstawie Points (canvasowe)
            if (Points == null || Points.Count < 3) return;

            var apex = Points[0];
            var right = Points[1];
            var left = Points[2];

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(apex.X, apex.Y);
            await ctx.LineToAsync(right.X, right.Y);
            await ctx.LineToAsync(left.X, left.Y);
            await ctx.ClosePathAsync();
            await ctx.StrokeAsync();
        }

        // ---------------------------------------------------------
        // Editable properties - operują na wartościach nominalnych
        // Po zmianie property wywołujemy GeneratePoints() i ApplyScaleToPoints()
        // ---------------------------------------------------------
        public List<EditableProperty> GetEditableProperties() => new()
        {
            new EditableProperty("Lewa podstawa X", () => BaseX1, v => { BaseX1 = v; NominalPoints = GeneratePoints(); ApplyScaleToPoints(); Szerokosc = BaseWidth; Wysokosc = Height; }, NazwaObj),
            new EditableProperty("Pozycja Y podstawy", () => BaseY, v => { BaseY = v; NominalPoints = GeneratePoints(); ApplyScaleToPoints(); Szerokosc = BaseWidth; Wysokosc = Height; }, NazwaObj),
            new EditableProperty("Szerokość podstawy", () => BaseWidth, v => { BaseWidth = v; NominalPoints = GeneratePoints(); ApplyScaleToPoints(); Szerokosc = BaseWidth; Wysokosc = Height; }, NazwaObj),
            new EditableProperty("Wysokość", () => Height, v => { Height = v; NominalPoints = GeneratePoints(); ApplyScaleToPoints(); Szerokosc = BaseWidth; Wysokosc = Height; }, NazwaObj)
        };

        // ---------------------------------------------------------
        // Scale operuje na NOMINALNYCH parametrach (zmienia geometrię) lub
        // można implementować jako jedynie zmiana _scaleFactor by zoomować.
        // Tutaj zrobimy: Scale(factor) — traktujemy jako ZOOM = zmiana _scaleFactor
        // jeśli chcesz zmienić wymiary geometryczne, zmieniaj BaseWidth/Height i wywołaj GeneratePoints.
        // ---------------------------------------------------------
        public void Scale(double factor)
        {
            // jeśli factor jest skalą canvas (zoom), to ustaw _scaleFactor i przelicz Points
            _scaleFactor = factor;
            ApplyScaleToPoints();
        }

        // Alternatywna signature jeżeli byłaby w interfejsie: Scale(scaleX, scaleY)
        public void Scale(double scaleX, double scaleY)
        {
            // jeżeli chcesz zmieniać nominalne rozmiary:
            BaseWidth *= scaleX;
            Height *= scaleY;

            // zaktualizuj nominalne punkty i canvas points
            NominalPoints = GeneratePoints();
            ApplyScaleToPoints();

            Szerokosc = BaseWidth;
            Wysokosc = Height;
        }

        // ---------------------------------------------------------
        public void Move(double offsetX, double offsetY)
        {
            BaseX1 += offsetX;
            BaseY += offsetY;

            // zaktualizuj nominalne i canvasowe punkty
            NominalPoints = GeneratePoints();
            ApplyScaleToPoints();
        }

        // ---------------------------------------------------------
        public BoundingBox GetBoundingBox()
        {
            // bazujemy na nominalnych punktach
            double minX = NominalPoints.Min(p => p.X);
            double minY = NominalPoints.Min(p => p.Y);
            double maxX = NominalPoints.Max(p => p.X);
            double maxY = NominalPoints.Max(p => p.Y);

            return new BoundingBox(minX, minY, maxX - minX, maxY - minY, NazwaObj);
        }

        // ---------------------------------------------------------
        public List<XPoint> GetVertices() => NominalPoints.Select(p => p.Clone()).ToList();

        // ---------------------------------------------------------
        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var v = NominalPoints;
            return new List<(XPoint, XPoint)>
            {
                (v[0].Clone(), v[1].Clone()),
                (v[1].Clone(), v[2].Clone()),
                (v[2].Clone(), v[0].Clone())
            };
        }

        // ---------------------------------------------------------
        public void Transform(double scale, double offsetX, double offsetY)
        {
            // transform jako zmiana skali canvas i przesunięcie
            Scale(scale);
            Move(offsetX, offsetY);
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            Scale(scaleX, scaleY);
            Move(offsetX, offsetY);
        }

        // ---------------------------------------------------------
        // Nowe metody zgodne z IShapeDC
        // ---------------------------------------------------------
        public List<XPoint> GetPoints() => Points.Select(p => p.Clone()).ToList();

        public List<XPoint> GetNominalPoints() => NominalPoints.Select(p => p.Clone()).ToList();
    }
}
