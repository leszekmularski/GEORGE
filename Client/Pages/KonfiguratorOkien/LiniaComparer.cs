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

            var (a1, a2) = (a.Wierzcholki[0], a.Wierzcholki[1]);
            var (b1, b2) = (b.Wierzcholki[0], b.Wierzcholki[1]);

            // Porównanie bez względu na kolejność punktów
            return (CzyPunktyRowne(a1, b1) && CzyPunktyRowne(a2, b2)) ||
                   (CzyPunktyRowne(a1, b2) && CzyPunktyRowne(a2, b1));
        }
        public int GetHashCode(ShapeRegion r)
        {
            if (r?.Wierzcholki == null || r.Wierzcholki.Count != 2)
                return 0;

            var p1 = r.Wierzcholki[0];
            var p2 = r.Wierzcholki[1];

            // Porządkuj punkty po X i Y, żeby kolejność nie miała znaczenia
            var (first, second) = (p1.X < p2.X || (p1.X == p2.X && p1.Y <= p2.Y))
                ? (p1, p2)
                : (p2, p1);

            return HashCode.Combine(first.X, first.Y, second.X, second.Y);
        }

        private bool CzyPunktyRowne(XPoint p1, XPoint p2, double epsilon = 0.001)
        {
            return Math.Abs(p1.X - p2.X) < epsilon &&
                   Math.Abs(p1.Y - p2.Y) < epsilon;
        }
    }

}
