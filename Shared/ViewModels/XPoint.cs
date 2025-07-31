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
    }
}
