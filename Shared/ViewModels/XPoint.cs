using System;

namespace GEORGE.Shared.ViewModels
{
    public struct XPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public XPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

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
            return $"({X:0.###}, {Y:0.###})";
        }

        // ---------------------------------------------------------
        // 🔥 Dodane — Clone() dla zgodności z interfejsami
        // ---------------------------------------------------------
        public XPoint Clone()
        {
            return new XPoint(X, Y);
        }
    }
}
