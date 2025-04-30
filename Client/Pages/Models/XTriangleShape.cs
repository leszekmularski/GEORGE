using GEORGE.Client.Pages.Models;
using Blazor.Extensions.Canvas.Canvas2D;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class XTriangleShape : IShapeDC
    {
        // 📍 Współrzędne bazowe (trójkąt równoramienny, o podstawie poziomej)
        public double BaseX1 { get; set; } // Lewy dolny X podstawy
        public double BaseY { get; set; }  // Y podstawy (stała wartość dla dolnych punktów)
        public double BaseWidth { get; set; }
        public double Height { get; set; }
        public string NazwaObj { get; set; } = "Trójkąt";

        private double _scaleFactor = 1.0; // Skala

        public XTriangleShape(double startX, double startY, double endX, double endY, double scaleFactor)
        {
            BaseX1 = Math.Min(startX, endX);
            BaseY = Math.Max(startY, endY);
            BaseWidth = Math.Abs(endX - startX);
            Height = Math.Abs(startY - endY);
            _scaleFactor = scaleFactor;
        }

        /// <summary>
        /// Rysuje trójkąt na kontekście 2D.
        /// </summary>
        public async Task Draw(Canvas2DContext ctx)
        {
            var apexX = BaseX1 + BaseWidth / 2;
            var apexY = BaseY - Height;
            var baseX2 = BaseX1 + BaseWidth;

            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync((float)(2 * _scaleFactor));

            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(apexX, apexY);
            await ctx.LineToAsync(baseX2, BaseY);
            await ctx.LineToAsync(BaseX1, BaseY);
            await ctx.ClosePathAsync();
            await ctx.StrokeAsync();
        }

        /// <summary>
        /// Zwraca listę edytowalnych właściwości trójkąta.
        /// </summary>
        public List<EditableProperty> GetEditableProperties() => new()
        {
            new EditableProperty("Lewa podstawa X", () => BaseX1, v => BaseX1 = v, NazwaObj),
            new EditableProperty("Pozycja Y podstawy", () => BaseY, v => BaseY = v, NazwaObj),
            new EditableProperty("Szerokość podstawy", () => BaseWidth, v => BaseWidth = v, NazwaObj),
            new EditableProperty("Wysokość", () => Height, v => Height = v, NazwaObj)
        };

        /// <summary>
        /// Skalowanie trójkąta względem środka.
        /// </summary>
        public void Scale(double factor)
        {
            BaseWidth *= factor;
            Height *= factor;
            BaseX1 -= (BaseWidth * (factor - 1)) / 2;
            BaseY += (Height * (factor - 1));
        }

        /// <summary>
        /// Przesunięcie trójkąta o offset (X, Y).
        /// </summary>
        public void Move(double offsetX, double offsetY)
        {
            BaseX1 += offsetX;
            BaseY += offsetY;
        }

        /// <summary>
        /// Zwraca BoundingBox obejmujący cały trójkąt.
        /// </summary>
        public BoundingBox GetBoundingBox()
        {
            double minX = BaseX1;
            double minY = BaseY - Height; // Wierzchołek trójkąta
            double maxX = BaseX1 + BaseWidth;
            double maxY = BaseY;

            return new BoundingBox(minX, minY, maxX - minX, maxY - minY, NazwaObj);
        }

        /// <summary>
        /// Zwraca 3 wierzchołki trójkąta (kolejność: góra, prawy dolny, lewy dolny).
        /// </summary>
        public List<XPoint> GetVertices()
        {
            var apexX = BaseX1 + BaseWidth / 2;
            var apexY = BaseY - Height;
            var baseX2 = BaseX1 + BaseWidth;

            return new List<XPoint>
            {
                new XPoint(apexX, apexY),        // Wierzchołek górny
                new XPoint(baseX2, BaseY),        // Prawy dolny róg
                new XPoint(BaseX1, BaseY)         // Lewy dolny róg
            };
        }

        /// <summary>
        /// Zwraca listę krawędzi trójkąta jako pary punktów (Start, End).
        /// </summary>
        public List<(XPoint Start, XPoint End)> GetEdges()
        {
            var vertices = GetVertices();
            return new List<(XPoint, XPoint)>
            {
                (vertices[0], vertices[1]),
                (vertices[1], vertices[2]),
                (vertices[2], vertices[0])
            };
        }

        /// <summary>
        /// Transformacja trójkąta (skala + przesunięcie).
        /// </summary>
        public void Transform(double scale, double offsetX, double offsetY)
        {
            Scale(scale);
            Move(offsetX, offsetY);
        }
    }
}
