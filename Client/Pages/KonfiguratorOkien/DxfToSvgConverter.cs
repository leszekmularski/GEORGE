using System.Text;
using netDxf;
using netDxf.Entities;


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

            double width = maxX - minX;
            double height = maxY - minY;

            if (width == 0 || height == 0)
            {
                width = 500;
                height = 500;
            }

            // 📌 Ustawienie viewBox na rzeczywisty obszar rysunku
            svg.Append($"<svg xmlns='http://www.w3.org/2000/svg' viewBox='{minX.ToString().Replace(',', '.')} {minY.ToString().Replace(',', '.')} {width.ToString().Replace(',', '.')} {height.ToString().Replace(',', '.')}'>");

            //// Dodanie linii prowadnicowej (przykład)
            //double guideX = (maxX + minX) / 2; // Pionowa linia w środku
            //double guideY = (maxY + minY) / 2; // Pozioma linia w środku

            // Dodanie linii prowadnicowej na całą szerokość i wysokość
            // Pionowa linia prowadnicza
            svg.Append($"<line id='guide-line-vertical' x1='{minX.ToString().Replace(',', '.')}' y1='{minY.ToString().Replace(',', '.')}' x2='{minX.ToString().Replace(',', '.')}' y2='{maxY.ToString().Replace(',', '.')}' " +
                       "stroke='red' stroke-width='1' stroke-dasharray='5,5' />");

            // Pozioma linia prowadnicza
            svg.Append($"<line id='guide-line-horizontal' x1='{minX.ToString().Replace(',', '.')}' y1='{minY.ToString().Replace(',', '.')}' x2='{maxX.ToString().Replace(',', '.')}' y2='{minY.ToString().Replace(',', '.')}' " +
                       "stroke='blue' stroke-width='1' stroke-dasharray='5,5' />");

            // Pionowa linia prowadnicza - maksymalna szerokość
            svg.Append($"<line id='guide-line-verticalMax' x1='{maxX.ToString().Replace(',', '.')}' y1='{minY.ToString().Replace(',', '.')}' x2='{maxX.ToString().Replace(',', '.')}' y2='{maxY.ToString().Replace(',', '.')}' " +
                       "stroke='green' stroke-width='1' stroke-dasharray='5,5' />");

            // Pozioma linia prowadnicza - maksymalna wysokość
            svg.Append($"<line id='guide-line-horizontalMax' x1='{minX.ToString().Replace(',', '.')}' y1='{maxY.ToString().Replace(',', '.')}' x2='{maxX.ToString().Replace(',', '.')}' y2='{maxY.ToString().Replace(',', '.')}' " +
                       "stroke='yellow' stroke-width='1' stroke-dasharray='5,5' />");


            foreach (var line in dxf.Entities.Lines)
            {
                svg.Append($"<line x1='{line.StartPoint.X.ToString().Replace(',', '.')} ' y1='{line.StartPoint.Y.ToString().Replace(',', '.')} ' " +
                           $"x2='{line.EndPoint.X.ToString().Replace(',', '.')} ' y2='{line.EndPoint.Y.ToString().Replace(',', '.')} ' " +
                           "stroke='black' stroke-width='1' />");
            }

            foreach (var circle in dxf.Entities.Circles)
            {
                svg.Append($"<circle cx='{circle.Center.X.ToString().Replace(',', '.')} ' cy='{circle.Center.Y.ToString().Replace(',', '.')} ' " +
                           $"r='{circle.Radius.ToString().Replace(',', '.')} ' " +
                           "stroke='black' stroke-width='1' fill='none' />");
            }

            foreach (var arc in dxf.Entities.Arcs)
            {
                double startX = arc.Center.X + arc.Radius * Math.Cos(arc.StartAngle * Math.PI / 180);
                double startY = arc.Center.Y + arc.Radius * Math.Sin(arc.StartAngle * Math.PI / 180);
                double endX = arc.Center.X + arc.Radius * Math.Cos(arc.EndAngle * Math.PI / 180);
                double endY = arc.Center.Y + arc.Radius * Math.Sin(arc.EndAngle * Math.PI / 180);

                svg.Append($"<path d='M {startX.ToString().Replace(',', '.')} {startY.ToString().Replace(',', '.')} A {arc.Radius.ToString().Replace(',', '.')} {arc.Radius.ToString().Replace(',', '.')} 0 0 1 {endX.ToString().Replace(',', '.')} {endY.ToString().Replace(',', '.')} ' " +
                           "stroke='black' stroke-width='1' fill='none' />");
            }

            svg.Append("</svg>");

            string result = svg.ToString();
            Console.WriteLine("SVG Content:");
            Console.WriteLine(result);

            return result;
        }


    }

}
