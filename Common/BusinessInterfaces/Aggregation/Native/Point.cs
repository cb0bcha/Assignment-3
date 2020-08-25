using System;

namespace Assignment3.Common.BusinessInterfaces.Aggregation.Native
{
    public class Point
    {
        public int InterpolationType { get; set; }

        public DateTime Time { get; set; }

        public double Value { get; set; }

        public static bool IsTimeGap(Point firstPoint, Point secondPoint)
        {
            return true;
        }
    }
}
