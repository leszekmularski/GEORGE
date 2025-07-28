using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;
using netDxf;
using netDxf.Entities;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public static class SvgToDxfConverter
    {
        public static DxfDocument ConvertSvgToDxf(string svgContent)
        {
            var doc = new XmlDocument();
            doc.LoadXml(svgContent);

            var dxf = new DxfDocument();

            // ✅ Konwersja <line>
            var lineNodes = doc.GetElementsByTagName("line");
            foreach (XmlNode node in lineNodes)
            {
                if (IsGuideLine(node)) continue;

                double x1 = double.Parse(node.Attributes["x1"].Value, CultureInfo.InvariantCulture);
                double y1 = double.Parse(node.Attributes["y1"].Value, CultureInfo.InvariantCulture);
                double x2 = double.Parse(node.Attributes["x2"].Value, CultureInfo.InvariantCulture);
                double y2 = double.Parse(node.Attributes["y2"].Value, CultureInfo.InvariantCulture);

                var line = new Line(new Vector2(x1, y1), new Vector2(x2, y2));
                dxf.Entities.Add(line);
            }

            // ✅ Konwersja <path> z łukiem (A)
            var pathNodes = doc.GetElementsByTagName("path");
            foreach (XmlNode node in pathNodes)
            {
                if (IsGuideLine(node)) continue;

                var d = node.Attributes?["d"]?.Value;
                if (string.IsNullOrWhiteSpace(d)) continue;

                var lwPolylines = ParseSvgArcsToPolylines(d);
                foreach (var poly in lwPolylines)
                {
                    dxf.Entities.Add(poly);
                }
            }

            return dxf;
        }

        // ✅ Pomijamy linie pomocnicze SVG
        private static bool IsGuideLine(XmlNode node)
        {
            var id = node.Attributes?["id"]?.Value;
            return !string.IsNullOrEmpty(id) && id.StartsWith("guide-line-");
        }

        // ✅ Parsuje "M x y A rx ry angle largeArcFlag sweepFlag x y"
        private static List<Polyline2D> ParseSvgArcsToPolylines(string d)
        {
            var polylines = new List<Polyline2D>();

            // Szukamy tylko prostych komend: M x y A rx ry xAxisRotation largeArcFlag sweepFlag x y
            var arcRegex = new Regex(@"M\s*(-?\d+(\.\d+)?)\s*(-?\d+(\.\d+)?)\s*A\s*(-?\d+(\.\d+)?)\s*(-?\d+(\.\d+)?)\s*(-?\d+(\.\d+)?)\s+([01])\s+([01])\s*(-?\d+(\.\d+)?)\s*(-?\d+(\.\d+)?)");

            var matches = arcRegex.Matches(d);
            foreach (Match match in matches)
            {
                double x0 = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                double y0 = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                double rx = double.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                double ry = double.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture);
                double xAxisRotation = double.Parse(match.Groups[9].Value, CultureInfo.InvariantCulture);
                bool largeArcFlag = match.Groups[11].Value == "1";
                bool sweepFlag = match.Groups[12].Value == "1";
                double x1 = double.Parse(match.Groups[13].Value, CultureInfo.InvariantCulture);
                double y1 = double.Parse(match.Groups[15].Value, CultureInfo.InvariantCulture);

                // Tylko łuki kołowe (rx == ry)
                if (Math.Abs(rx - ry) > 0.01)
                    continue;

                var poly = new Polyline2D();

                var start = new Polyline2DVertex(x0, y0);
                var end = new Polyline2DVertex(x1, y1);

                double angle = CalculateArcAngle(x0, y0, x1, y1, rx, largeArcFlag, sweepFlag);
                double bulge = Math.Tan(angle / 4.0);

                if (!sweepFlag)
                    bulge *= -1;

                start.Bulge = bulge;

                poly.Vertexes.Add(start);
                poly.Vertexes.Add(end);
                poly.IsClosed = false;

                polylines.Add(poly);
            }

            return polylines;
        }

        // ✅ Oblicza kąt łuku (radiany) na podstawie geometrii SVG
        private static double CalculateArcAngle(double x0, double y0, double x1, double y1, double radius, bool largeArc, bool sweep)
        {
            double dx = x1 - x0;
            double dy = y1 - y0;
            double chord = Math.Sqrt(dx * dx + dy * dy);

            // Ograniczenie: chord ≤ 2r
            if (chord > 2 * radius)
                radius = chord / 2;

            // θ = 2 * arcsin(chord / (2r))
            double angle = 2 * Math.Asin(chord / (2 * radius));

            if (double.IsNaN(angle)) angle = Math.PI;

            if (largeArc) angle = 2 * Math.PI - angle;

            return angle;
        }
    }
}
