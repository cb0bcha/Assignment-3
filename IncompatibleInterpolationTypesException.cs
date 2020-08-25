using System;

namespace Assignment3
{
    public class IncompatibleInterpolationTypesException : Exception
    {
        public int Mean { get; set; }

        public int InterpolationType1 { get; set; }
        
        public int InterpolationType2 { get; set; }

        public IncompatibleInterpolationTypesException(string message, int mean, int interpolationType1, int interpolationType2) : base(message)
        {
            Mean = mean;
            InterpolationType1 = interpolationType1;
            InterpolationType2 = interpolationType2;
        }
    }
}
