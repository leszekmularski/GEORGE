using System;

namespace GEORGE.Shared.ViewModels
{
    public struct XPoint
    {
        // Wartości surowe, bez zaokrąglania
        public double X { get; set; }
        public double Y { get; set; }

        public XPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        // Porównanie z tolerancją – idealne do geometrii
        private const double Tolerance = 0.001;

        public bool EqualsWithTolerance(XPoint other)
        {
            return Math.Abs(X - other.X) <= Tolerance &&
                   Math.Abs(Y - other.Y) <= Tolerance;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not XPoint other)
                return false;

            return EqualsWithTolerance(other);
        }

        public override int GetHashCode()
        {
            // hash zgrubny, wystarczający do structa
            // bo i tak Equals używa tolerancji
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            // czytelne debugowanie: max 3 miejsca tylko do wyświetlania
            return $"({X:0.###}, {Y:0.###})";
        }
    }
}
