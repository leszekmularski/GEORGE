using GEORGE.Client.Pages.Models;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class LiniaComparer : IEqualityComparer<ShapeRegion>
    {
        public bool Equals(ShapeRegion a, ShapeRegion b)
        {
            if (a?.Wierzcholki == null || b?.Wierzcholki == null)
                return false;

            if (a.Wierzcholki.Count != 2 || b.Wierzcholki.Count != 2)
                return false;

            var a1 = a.Wierzcholki[0];
            var a2 = a.Wierzcholki[1];
            var b1 = b.Wierzcholki[0];
            var b2 = b.Wierzcholki[1];

            return (CzyPunktyRówne(a1, b1) && CzyPunktyRówne(a2, b2)) ||
                   (CzyPunktyRówne(a1, b2) && CzyPunktyRówne(a2, b1));
        }

        public int GetHashCode(ShapeRegion r)
        {
            if (r?.Wierzcholki == null || r.Wierzcholki.Count != 2)
                return 0;

            var p1 = r.Wierzcholki[0];
            var p2 = r.Wierzcholki[1];

            // Dodaj punkty niezależnie od kolejności
            var hash1 = HashCode.Combine(p1.X, p1.Y, p2.X, p2.Y);
            var hash2 = HashCode.Combine(p2.X, p2.Y, p1.X, p1.Y);

            return hash1 ^ hash2;
        }

        private bool CzyPunktyRówne(XPoint p1, XPoint p2, double epsilon = 0.001)
        {
            return Math.Abs(p1.X - p2.X) < epsilon && Math.Abs(p1.Y - p2.Y) < epsilon;
        }
    }

}
