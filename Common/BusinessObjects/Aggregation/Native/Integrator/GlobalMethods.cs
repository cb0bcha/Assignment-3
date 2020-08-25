using Assignment3;
using Assignment3.Common.BusinessInterfaces.Aggregation.Native;
using Assignment3.Common.BusinessObjects.Aggregation.Native;
using CodaTime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.BusinessObjects.Aggregation.Native.Integrator
{
	public static class GlobalMethods
	{
		// TODO - We should implement c# naming standards.
		// TODO - We should convert many methods to properties such as getValue(), WidthInPoints() etc.
		public static Integrator.PartitionRectangle ComputeIntegral(AggregationTimeSeries timeSeries, BinBoundaries bin)
		{
			var pointsInBin = PointsInBin(timeSeries, bin, BoundaryPointHandling.IsWeightedComputation);

			// TODO - We need to replace Duration class
			// Duration describes a type that holds a time interval, which is an elapsed time between two time points.
			// I think we can use TimeSpan class of c#.
			var binLengthMinusGaps = TimeSpan.Zero;

			Func<Point, Point, double> TrapezoidalArea = (Point firstPoint, Point secondPoint) =>
			{
				var averageRectangle = MeanRectangleBetweenPoints(firstPoint, secondPoint);

				binLengthMinusGaps += averageRectangle.BaseLength; // TODO - can we implement this using += operator instead of =

				return averageRectangle.Area();
			};

			// TODO - Converted WidthAsDuration() to Property. We need to find c# equivalent for Duration.Epsilon
			if (bin.WidthAsDuration <= Duration.Epsilon)
			{
				// TODO - Convert WidthInPoints() and size() to property.
				// IsEmptyValue checks first item in pointsInBin for empty.
				// TODO - Should we check pointsInBin for null? What if, PointsInBin method returns null instead of empty collection.
				if (bin.WidthInPoints == 0 || pointsInBin.Count == 0 || AggregationConstants.IsEmptyValue(pointsInBin.First().Value))
				{
					return new Integrator.PartitionRectangle() { MeanValue = AggregationConstants.EmptyValue(), BaseLength = TimeSpan.Zero };
				}

				return new Integrator.PartitionRectangle() { MeanValue = pointsInBin.First().Value, BaseLength = bin.WidthAsDuration };
			}

			// TODO - Resolve std::inner_product and std::plus methods with c# equivalent.
			// std::inner_product - Computes cumulative inner product of range
			var integral = std::inner_product(pointsInBin.begin(), pointsInBin.end() - 1, pointsInBin.begin() + 1, 0.0, std::plus<double>(), TrapezoidalArea);

			// TODO - Convert Ticks() method to property.
			if (binLengthMinusGaps.Ticks == 0)
			{
				return new Integrator.PartitionRectangle() { MeanValue = AggregationConstants.EmptyValue(), BaseLength = TimeSpan.Zero };
			}

			return new Integrator.PartitionRectangle() { MeanValue = integral / binLengthMinusGaps.Ticks, BaseLength = binLengthMinusGaps };
		}

		public static Integrator.PartitionRectangle MeanRectangleBetweenPoints(Point firstPoint, Point secondPoint)
		{
			// TODO - Convert getInterpolationType() method to property
			// We need to know more about Interpolation system.
			var interpolationMethod = Interpolation.InterpolationSystem.GetInterpolationMethod(firstPoint.InterpolationType, secondPoint.InterpolationType);

			if (!interpolationMethod.IsValid())
			{
				throw new IncompatibleInterpolationTypesException("Incompatible interpolation types in weighted calculation", StatisticType.Mean, firstPoint.InterpolationType, secondPoint.InterpolationType);
			}

			var interpolationType = interpolationMethod.GetResultInterpolationType();

			if (StatisticContinuityType.UseWeightedWhenPossible().RequiresDiscrete(interpolationType))
			{
				// TODO - Replaced std::logic_error with Exception class.
				throw new Exception("Weighted calculations not defined for this interpolation type");
			}

			if (Point.IsTimeGap(firstPoint, secondPoint) || interpolationMethod.IsValueGap(firstPoint, secondPoint))
			{
				return new Integrator.PartitionRectangle() { MeanValue = 0.0, BaseLength = TimeSpan.Zero };
			}

			// TODO - Convert getTime() method to property. 
			// We can use DateTime.Subtract method. We need to know more about ToDuration() method, but it looks like that we need to define
			// extension method for double datatype.
			var durationBetweenPoints = Period.Between(firstPoint.Time, secondPoint.Time, PeriodUnits.Ticks).ToDuration();

			var midpointTime = firstPoint.Time + Period.FromTicks(durationBetweenPoints.Ticks() / 2);

			var meanValue = interpolationMethod.InterpolateValue(firstPoint, secondPoint, midpointTime);

			return new Integrator.PartitionRectangle() { MeanValue = meanValue, BaseLength = durationBetweenPoints };
		}


		// Placeholder method
		public static IList<Point> PointsInBin(AggregationTimeSeries timeSeries, BinBoundaries bin, bool isWeightedComputation)
		{
			return new List<Point>();
		}
	}
}