using System.Text;
using System.Text.RegularExpressions;
using netDxf;
using netDxf.Entities;
using System.Globalization;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class DxfToSvgConverter
    {
        public string ConvertToSvg(DxfDocument dxf)
        {
            StringBuilder svg = new StringBuilder();

            // 🔍 Obliczanie minimalnych i maksymalnych współrzędnych
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;

            foreach (var line in dxf.Entities.Lines)
            {
                minX = Math.Min(minX, Math.Min(line.StartPoint.X, line.EndPoint.X));
                minY = Math.Min(minY, Math.Min(line.StartPoint.Y, line.EndPoint.Y));
                maxX = Math.Max(maxX, Math.Max(line.StartPoint.X, line.EndPoint.X));
                maxY = Math.Max(maxY, Math.Max(line.StartPoint.Y, line.EndPoint.Y));
            }

            foreach (var circle in dxf.Entities.Circles)
            {
                minX = Math.Min(minX, circle.Center.X - circle.Radius);
                minY = Math.Min(minY, circle.Center.Y - circle.Radius);
                maxX = Math.Max(maxX, circle.Center.X + circle.Radius);
                maxY = Math.Max(maxY, circle.Center.Y + circle.Radius);
            }

            foreach (var arc in dxf.Entities.Arcs)
            {
                minX = Math.Min(minX, arc.Center.X - arc.Radius);
                minY = Math.Min(minY, arc.Center.Y - arc.Radius);
                maxX = Math.Max(maxX, arc.Center.X + arc.Radius);
                maxY = Math.Max(maxY, arc.Center.Y + arc.Radius);
            }

            //if (maxX > maxY)
            //{
            //    maxY = maxX;
            //}

            // 🔥 Przesunięcie rysunku do dodatniej ćwiartki (X, Y >= 0)
            double offsetX = -minX; // Przesuwamy całość tak, aby minX = 0
            double offsetY = -minY; // Przesuwamy całość tak, aby minY = 0

            // 📏 Obliczamy szerokość i wysokość po przesunięciu
            double width = maxX + offsetX;
            double height = maxY + offsetY;

            CultureInfo culture = CultureInfo.InvariantCulture;

            // 📌 Ustawienie `viewBox` tak, aby zaczynał się od (0,0)
            svg.Append($"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 {width.ToString().Replace(',', '.')} {height.ToString().Replace(',', '.')}' width='{width}' height='{height}'>");

            // 🔥 Dodanie grupy, którą będziemy obracać
            svg.Append("<g id='rotate-group'>");

            // 🖌️ Rysowanie linii
            foreach (var line in dxf.Entities.Lines)
            {
                svg.Append($"<line x1='{(line.StartPoint.X + offsetX).ToString().Replace(',', '.')}' " +
                           $"y1='{(line.StartPoint.Y + offsetY).ToString().Replace(',', '.')}' " +
                           $"x2='{(line.EndPoint.X + offsetX).ToString().Replace(',', '.')}' " +
                           $"y2='{(line.EndPoint.Y + offsetY).ToString().Replace(',', '.')}' " +
                           "stroke='black' stroke-width='1' />");
            }

            // 🖌️ Rysowanie okręgów
            foreach (var circle in dxf.Entities.Circles)
            {
                svg.Append($"<circle cx='{(circle.Center.X + offsetX).ToString().Replace(',', '.')}' " +
                           $"cy='{(circle.Center.Y + offsetY).ToString().Replace(',', '.')}' " +
                           $"r='{circle.Radius.ToString().Replace(',', '.')}' " +
                           "stroke='black' stroke-width='1' fill='none' />");
            }

            // 🖌️ Rysowanie łuków
            foreach (var arc in dxf.Entities.Arcs)
            {
                double startX = arc.Center.X + arc.Radius * Math.Cos(arc.StartAngle * Math.PI / 180);
                double startY = arc.Center.Y + arc.Radius * Math.Sin(arc.StartAngle * Math.PI / 180);
                double endX = arc.Center.X + arc.Radius * Math.Cos(arc.EndAngle * Math.PI / 180);
                double endY = arc.Center.Y + arc.Radius * Math.Sin(arc.EndAngle * Math.PI / 180);

                svg.Append($"<path d='M {(startX + offsetX).ToString().Replace(',', '.')} {(startY + offsetY).ToString().Replace(',', '.')} " +
                           $"A {arc.Radius.ToString().Replace(',', '.')} {arc.Radius.ToString().Replace(',', '.')} 0 0 1 " +
                           $"{(endX + offsetX).ToString().Replace(',', '.')} {(endY + offsetY).ToString().Replace(',', '.')}' " +
                           "stroke='black' stroke-width='1' fill='none' />");
            }

            svg.Append("</g>");

            // ➕ Pionowe prowadnice
            svg.AppendLine($"<line id='guide-line-vertical' x1='0' y1='0' x2='0' y2='{height.ToString("G", culture)}' stroke='red' stroke-width='1' stroke-dasharray='5,5' />");
            svg.AppendLine($"<line id='guide-line-verticalMax' x1='{width.ToString("G", culture)}' y1='0' x2='{width.ToString("G", culture)}' y2='{height.ToString("G", culture)}' stroke='green' stroke-width='1' stroke-dasharray='5,5' />");
            svg.AppendLine($"<line id='guide-line-verticalOdlSzyby' x1='{(width / 3).ToString("G", culture)}' y1='0' x2='{(width / 3).ToString("G", culture)}' y2='{height.ToString("G", culture)}' stroke='#43277c' stroke-width='1' stroke-dasharray='5,5' />");
            svg.AppendLine($"<line id='guide-line-verticalOsSymetrii' x1='{(width / 2).ToString("G", culture)}' y1='0' x2='{(width / 2).ToString("G", culture)}' y2='{height.ToString("G", culture)}' stroke='#ffa726' stroke-width='1' stroke-dasharray='5,5' />");

            // ➕ Poziome prowadnice
            svg.AppendLine($"<line id='guide-line-horizontal' x1='0' y1='0' x2='{width.ToString("G", culture)}' y2='0' stroke='blue' stroke-width='1' stroke-dasharray='5,5' />");
            svg.AppendLine($"<line id='guide-line-horizontalMax' x1='0' y1='{height.ToString("G", culture)}' x2='{width.ToString("G", culture)}' y2='{height.ToString("G", culture)}' stroke='yellow' stroke-width='1' stroke-dasharray='5,5' />");
            svg.AppendLine($"<line id='guide-line-horizontal-korpus' x1='0' y1='{(height * 0.2).ToString("G", culture)}' x2='{width.ToString("G", culture)}' y2='{(height * 0.2).ToString("G", culture)}' stroke='brown' stroke-width='1' stroke-dasharray='5,5' />");
            svg.AppendLine($"<line id='guide-line-horizontal-liniaSzklenia' x1='0' y1='{(height * 0.4).ToString("G", culture)}' x2='{width.ToString("G", culture)}' y2='{(height * 0.4).ToString("G", culture)}' stroke='#43277c' stroke-width='1' stroke-dasharray='5,5' />");
            svg.AppendLine($"<line id='guide-line-horizontal-okucie' x1='0' y1='{(height * 0.6).ToString("G", culture)}' x2='{width.ToString("G", culture)}' y2='{(height * 0.6).ToString("G", culture)}' stroke='#fadb14' stroke-width='1' stroke-dasharray='5,5' />");
            svg.AppendLine($"<line id='guide-line-horizontal-dormas' x1='0' y1='{(height * 0.8).ToString("G", culture)}' x2='{width.ToString("G", culture)}' y2='{(height * 0.8).ToString("G", culture)}' stroke='#f5222d' stroke-width='1' stroke-dasharray='5,5' />");
            svg.AppendLine($"<line id='guide-line-horizontal-OsSymetrii' x1='0' y1='{(height / 2).ToString("G", culture)}' x2='{width.ToString("G", culture)}' y2='{(height / 2).ToString("G", culture)}' stroke='#ffa726' stroke-width='1' stroke-dasharray='5,5' />");
            svg.Append("</svg>");
            string result = svg.ToString();

            Console.WriteLine("✅ SVG Content OK");
            return result;
        }
    }
}
