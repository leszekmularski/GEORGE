using GEORGE.Shared.ViewModels;

namespace GEORGE.Shared.ViewModels
{
    public enum SegmentType { Line, Arc }

    public class ContourSegment
    {
        public SegmentType Type { get; set; } = SegmentType.Line;
        public XPoint Start { get; set; }
        public XPoint End { get; set; }

        // tylko dla łuków
        public XPoint? Center { get; set; }
        public double Radius { get; set; }
        public bool CounterClockwise { get; set; } = false;
        public string Informacja { get; set; } = "Brak informacji";

        public ContourSegment(XPoint start, XPoint end)
        {
            Start = start;
            End = end;
            Type = SegmentType.Line;
        }
        public ContourSegment(XPoint start, XPoint end, XPoint? center, double radius, bool counterClockwise = false)
        {
            Start = start;
            End = end;
            Center = center;
            Radius = radius;
            CounterClockwise = counterClockwise;
            Type = SegmentType.Arc;
        }

        // METODA KLONUJĄCA
        public ContourSegment Clone()
        {
            if (this.Type == SegmentType.Arc)
            {
                return new ContourSegment(this.Start, this.End, this.Center, this.Radius, this.CounterClockwise)
                {
                    Informacja = this.Informacja
                };
            }
            else
            {
                return new ContourSegment(this.Start, this.End)
                {
                    Informacja = this.Informacja
                };
            }
        }
    }
}
