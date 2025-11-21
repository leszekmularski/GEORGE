using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GEORGE.Client.Pages.Models
{
    public class XRoundedTopRectangleShape : IShapeDC
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();

        // podstawowe pola geometryczne
        public double X { get; set; }      // lewy górny róg konteneru (pozycja Y to góra)
        public double Y { get; set; }      // górna krawędź obiektu (uwaga: Y oznacza górę)
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }

        // zgodność z interfejsem
        public double Szerokosc { get => Width; set => Width = value; }
        public double Wysokosc { get => Height; set => Height = value; }

        // punkty kontrolne (zawsze utrzymywane w spójności z polami)
        public List<XPoint> Points { get; set; } = new();
        public List<XPoint> GetPoints() => Points;

        private readonly double _scaleFactor;
        public string NazwaObj { get; set; } = "Prostokąt z zaokr. u góry";

        public XRoundedTopRectangleShape(double x, double y, double width, double height, double radius, double scaleFactor = 1.0)
        {
            X = x;
            Y = y;
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            Radius = Math.Max(0, radius);
            _scaleFactor = scaleFactor;

            ClampRadius();
            RegeneratePointsFromGeometry();
        }

        // ----- Synchronizacja ------------------------------------------------
        // Wywołaj po każdej zmianie Width/Height/X/Y/Radius aby zaktualizować Points
        private void RegeneratePointsFromGeometry()
        {
            ClampRadius();

            // Points: 0 = lewy dolny, 1 = prawy dolny, 2 = punkt środkowy łuku (górny)
            // Uwaga: Y jest górną krawędzią, więc lewy dolny to (X, Y + Height)
            Points = new List<XPoint>
            {
                new XPoint(X, Y + Height),              // 0 — lewy dolny
                new XPoint(X + Width, Y + Height),      // 1 — prawy dolny
                new XPoint(X + Width / 2.0, Y + Radius) // 2 — punkt pod łukiem (środek łuku Y + Radius)
            };
        }

        // Wywołaj po zmianie Points — zaktualizuje pola geometryczne
        public void UpdatePoints(List<XPoint> newPoints)
        {
            if (newPoints == null || newPoints.Count < 3)
                return;

            // Przyjmujemy semantykę: [0]=lewy dolny, [1]=prawy dolny, [2]=punkt środkowy łuku
            Points = newPoints.ToList();

            // aktualizuj bounding box i wysokość/pozycję górną
            var minX = Points.Min(p => p.X);
            var maxX = Points.Max(p => p.X);
            var minY = Points.Min(p => p.Y);
            var maxY = Points.Max(p => p.Y);

            // Dolna krawędź przyjmujemy jako maxY (bo Y rośnie w dół)
            X = minX;
            Width = Math.Abs(maxX - minX);

            // punkt 2 powinien leżeć nad dolną krawędzią (środek łuku)
            var arcPoint = Points[2];
            // Height = dolnyY - (arcPoint.Y - Radius)  => ale nie znamy Radius; wyznaczmy Radius na podstawie odległości do środka prostokąta
            double bottomY = maxY;
            // oblicz kandydacki Radius jako odległość między arcPoint.Y a (arcCenterY) - uproszczenie:
            // centerX = X + Width/2
            double centerX = X + Width / 2.0;

            // jeżeli arcPoint.X nie jest idealnie na środku, weź centerX
            double assumedRadius = Math.Abs((arcPoint.X - centerX));
            // oblicz Height jako dolnyY - (arcPoint.Y - assumedRadius)
            // ale bezpieczniej: ustaw Height = bottomY - (arcPoint.Y - assumedRadius)
            double computedHeight = bottomY - (arcPoint.Y - assumedRadius);

            // zabezpieczenia
            if (computedHeight <= 0) computedHeight = Math.Max(1, Height);

            Height = computedHeight;

            // oblicz Radius bardziej bezpośrednio: distance horizontally from center or distance vertically to arcPoint
            double verticalDist = bottomY - arcPoint.Y;
            Radius = Math.Min(Math.Min(Width / 2.0, verticalDist), Math.Max(2.0, assumedRadius));

            // ustaw Y tak, aby arc center był w Y + Radius
            // arcPoint.Y powinien być centerY - Radius  => centerY = arcPoint.Y + Radius
            Y = (arcPoint.Y + Radius) - Radius; // = arcPoint.Y (ale chcemy Y = centerY - Radius) -> simplified:
            Y = arcPoint.Y - Radius;

            ClampRadius();
            RegeneratePointsFromGeometry();
        }

        private void ClampRadius()
        {
            // Radius nie może być większy niż połowa szerokości ani większy niż wysokość
            if (Width <= 0) Width = 1;
            if (Height <= 0) Height = 1;

            Radius = Math.Max(0, Radius);
            Radius = Math.Min(Radius, Width / 2.0);
            Radius = Math.Min(Radius, Height);
        }

        // ----- Rysowanie ------------------------------------------------------
        public async Task Draw(Canvas2DContext ctx)
        {
            ClampRadius();
            RegeneratePointsFromGeometry(); // upewnij się, że Points są spójne

            double arcCenterX = X + Width / 2.0;
            double arcCenterY = Y + Radius;

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();

            // dolna krawędź
            await ctx.MoveToAsync(X, Y + Height);
            await ctx.LineToAsync(X + Width, Y + Height);

            // prawa ściana do punktu przy łuku
            await ctx.LineToAsync(X + Width, arcCenterY);

            // łuk — górna część (from right to left across top)
            // canvas: 0 = +x axis (to the right), PI = -x axis (to the left)
            // chcemy łuk od kąta 0 do PI (przechodząc „górą”).
            await ctx.ArcAsync(arcCenterX, arcCenterY, Radius, 0, Math.PI, true);

            // lewa ściana od punktu przy łuku do dolnej krawędzi
            await ctx.LineToAsync(X, arcCenterY);
            await ctx.LineToAsync(X, Y + Height);

            await ctx.ClosePathAsync();
            await ctx.StrokeAsync();
        }

        // ----- geometria ułatwiająca edycję / kolizje -------------------------
        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(X, Y, Width, Height, NazwaObj);
        }

        public List<XPoint> GetVertices()
        {
            // Zwróć kluczowe punkty (bez gładzenia łuku) — konsystentnie z RegeneratePointsFromGeometry
            double arcCenterY = Y + Radius;
            return new List<XPoint>
            {
                new XPoint(X, Y + Height),           // 0 lewy dolny
                new XPoint(X + Width, Y + Height),   // 1 prawy dolny
                new XPoint(X + Width, arcCenterY),   // 2 prawy przy łuku
                new XPoint(X, arcCenterY)            // 3 lewy przy łuku
            };
        }

        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var v = GetVertices();
            var edges = new List<(XPoint, XPoint)>();
            for (int i = 0; i < v.Count - 1; i++)
            {
                edges.Add((v[i], v[i + 1]));
            }
            edges.Add((v[^1], v[0])); // zamknięcie
            return edges;
        }

        // ----- transformacje -------------------------------------------------
        public void Scale(double factor)
        {
            if (factor == 0) return;
            X *= factor;
            Y *= factor;
            Width *= factor;
            Height *= factor;
            Radius *= factor;
            RegeneratePointsFromGeometry();
        }

        public void Move(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = new XPoint(Points[i].X + offsetX, Points[i].Y + offsetY);
            }
        }

        public void Transform(double scale, double offsetX, double offsetY)
        {
            Transform(scale, scale, offsetX, offsetY);
        }

        public void Transform(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            X = X * scaleX + offsetX;
            Y = Y * scaleY + offsetY;
            Width *= scaleX;
            Height *= scaleY;
            Radius *= (scaleX + scaleY) / 2.0;
            RegeneratePointsFromGeometry();
        }

        // ----- pozostałe ----------------------------------------------------
        public IShapeDC Clone()
        {
            var clone = new XRoundedTopRectangleShape(X, Y, Width, Height, Radius, _scaleFactor)
            {
                Points = Points?.Select(p => new XPoint(p.X, p.Y)).ToList() ?? new List<XPoint>(),
            };
            clone.ID = Guid.NewGuid().ToString();
            return clone;
        }

        public List<EditableProperty> GetEditableProperties() => new()
        {
            new EditableProperty("X", () => X, v => { X = v; RegeneratePointsFromGeometry(); }, NazwaObj, true),
            new EditableProperty("Y", () => Y, v => { Y = v; RegeneratePointsFromGeometry(); }, NazwaObj, true),
            new EditableProperty("Szerokość", () => Width, v => { Width = v; ClampRadius(); RegeneratePointsFromGeometry(); }, NazwaObj),
            new EditableProperty("Wysokość", () => Height, v => { Height = v; ClampRadius(); RegeneratePointsFromGeometry(); }, NazwaObj),
            new EditableProperty("Promień łuku", () => Radius, v => { Radius = v; ClampRadius(); RegeneratePointsFromGeometry(); }, NazwaObj),
        };
    }
}
