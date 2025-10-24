using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEORGE.Shared.ViewModels
{
    public struct XPoint
    {
        private double _x;
        private double _y;

        public double X
        {
            get => _x;
            set => _x = Math.Round(value, 4);
        }

        public double Y
        {
            get => _y;
            set => _y = Math.Round(value, 4);
        }

        public XPoint(double x, double y)
        {
            _x = Math.Round(x, 4);
            _y = Math.Round(y, 4);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not XPoint other) return false;
            return Math.Abs(X - other.X) < 0.001 && Math.Abs(Y - other.Y) < 0.001;
        }

        public override int GetHashCode() =>
            HashCode.Combine(X, Y);
    }

}
