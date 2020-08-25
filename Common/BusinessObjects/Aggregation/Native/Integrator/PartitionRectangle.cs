namespace Common.BusinessObjects.Aggregation.Native.Integrator
{
    // TODO - Can we define public constructor with MeanValue and BaseLength as parameters? 
    // Then, we can make Set accessor of both properties as private.
    public struct PartitionRectangle
    {
        public double MeanValue { get; set; } // Converted public field to public property. Renamed as per c# naming standards.

        public CodaTime.Duration BaseLength { get; set; } // Converted public field to public property. Renamed as per c# naming standards.

        // C# 8.0 allows to make use of readonly modifier with methods of struct to indicate that they do not change state.
        public readonly double Area() // Renamed method name as per c# naming standards. We should rename this method to GetArea() or we should convert this into property.
        {
            // TODO - Can we convert Ticks() method to property?
            return BaseLength.Ticks() == 0 ? 0.0 : MeanValue * BaseLength.Ticks();
        }
    }
}