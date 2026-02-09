using GEORGE.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

public static class StronaOknaHelper
{
    /// <summary>
    /// Pełna wersja – indeks krawędzi
    /// </summary>
    public static string OkreslStrone(
        float angleDegrees,
        int i,
        List<XPoint> outer)
    {
        if (outer == null || outer.Count < 2)
            return OkreslStrone(angleDegrees);

        int next = (i + 1) % outer.Count;

        double edgeCenterX = (outer[i].X + outer[next].X) / 2;
        double edgeCenterY = (outer[i].Y + outer[next].Y) / 2;

        return OkreslStroneInternal(angleDegrees, edgeCenterX, edgeCenterY, outer);
    }

    /// <summary>
    /// NOWE przeciążenie – bez indeksu (do testów)
    /// </summary>
    public static string OkreslStrone(
        float angleDegrees,
        List<XPoint> outer)
    {
        if (outer == null || outer.Count == 0)
            return OkreslStrone(angleDegrees);

        // Środek całego wielokąta – stabilny punkt odniesienia
        double centerX = outer.Average(p => p.X);
        double centerY = outer.Average(p => p.Y);

        return OkreslStroneInternal(angleDegrees, centerX, centerY, outer);
    }

    /// <summary>
    /// Fallback – tylko kąt
    /// </summary>
    public static string OkreslStrone(float angleDegrees)
    {
        angleDegrees = (angleDegrees + 360) % 360;

        return angleDegrees switch
        {
            >= 60 and < 120 => "Prawa",
            >= 120 and < 240 => "Dół",
            >= 240 and < 300 => "Lewa",
            _ => "Góra"
        };
    }

    /// <summary>
    /// WSPÓLNA logika – jedno źródło prawdy
    /// </summary>
    private static string OkreslStroneInternal(
        float angleDegrees,
        double refX,
        double refY,
        List<XPoint> outer)
    {
        double minX = outer.Min(p => p.X);
        double maxX = outer.Max(p => p.X);
        double minY = outer.Min(p => p.Y);
        double maxY = outer.Max(p => p.Y);

        double height = maxY - minY;
        double width = maxX - minX;

        // ===== DÓŁ – NAJOSTRZEJSZY WARUNEK =====
        bool isBottom =
            refY > maxY - height * 0.20 &&
            angleDegrees > 120 &&
            angleDegrees < 240;

        if (isBottom)
            return "Dół";

        // ===== RESZTA – WIĘKSZA SWOBODA =====
        if (refY < minY + height * 0.30)
            return "Góra";

        if (refX < minX + width * 0.30)
            return "Lewa";

        if (refX > maxX - width * 0.30)
            return "Prawa";

        // ===== OSTATECZNOŚĆ – kąt =====
        return OkreslStrone(angleDegrees);
    }

    public static string OkreslStroneNaPodstawieKataLinii(double katLinii)
    {
        katLinii = (katLinii + 360) % 360;

        const double tolerancja = 12.0;

        bool jestPionowa =
            Math.Abs(katLinii - 90) <= tolerancja ||
            Math.Abs(katLinii - 270) <= tolerancja;

        bool jestPozioma =
            Math.Abs(katLinii) <= tolerancja ||
            Math.Abs(katLinii - 180) <= tolerancja ||
            Math.Abs(katLinii - 360) <= tolerancja;

        // ===== PRZYPADKI PROSTE =====
        if (jestPozioma)
            return katLinii > 90 && katLinii < 270 ? "Dół" : "Góra";

        if (jestPionowa)
            return katLinii < 180 ? "Prawa" : "Lewa";

        // ===== SKOŚNE – DOMINUJĄCY KIERUNEK =====

        // Odległość od osi
        double distToHorizontal = Math.Min(
            Math.Abs(katLinii),
            Math.Abs(katLinii - 180));

        double distToVertical = Math.Min(
            Math.Abs(katLinii - 90),
            Math.Abs(katLinii - 270));

        if (distToHorizontal < distToVertical)
        {
            // bliżej poziomu → Góra / Dół
            // Dół tylko gdy WYRAŹNIE
            return katLinii > 120 && katLinii < 240
                ? "Dół"
                : "Góra";
        }
        else
        {
            // bliżej pionu → Lewa / Prawa
            return katLinii < 180
                ? "Prawa"
                : "Lewa";
        }
    }

}

//namespace GEORGE.Client.Pages.Okna
//{
//    public class StronaOknaHelper
//    {
//    }
//}
