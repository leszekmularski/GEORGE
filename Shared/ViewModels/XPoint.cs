using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEORGE.Shared.ViewModels
{
    public struct XPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public XPoint(double x, double y) { X = x; Y = y; }
        public override bool Equals(object? obj)
        {
            if (obj is not XPoint other) return false;
            return Math.Abs(X - other.X) < 0.001 && Math.Abs(Y - other.Y) < 0.001;
        }
        public override int GetHashCode() => HashCode.Combine(Math.Round(X, 3), Math.Round(Y, 3));
    }
}
