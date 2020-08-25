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
		// TODO - We should convert many methods to properties such as getValue(), WidthInPoints() etc.
		public static Integrator.PartitionRectangle ComputeIntegral(AggregationTimeSeries timeSeries, BinBoundaries bin)
		{
			var pointsInBin = PointsInBin(timeSeries, bin, BoundaryPointHandling.IsWeightedComputation);

			// Replaced Duration class by TimeSpan class.
			// Duration describes a type that holds a time interval, which is an elapsed time between two time points.
			var binLengthMinusGaps = TimeSpan.Zero;

			Func<Point, Point, double> trapezoidalArea = (Point firstPoint, Point secondPoint) =>
			{
				var averageRectangle = MeanRectangleBetweenPoints(firstPoint, secondPoint);

				binLengthMinusGaps += averageRectangle.BaseLength; // Used += operator instead of =

				return averageRectangle.Area();
			};

			// TODO - We need to find c# equivalent for Duration.Epsilon
			if (bin.WidthAsDuration <= Duration.Epsilon)
			{
				// IsEmptyValue checks first item in pointsInBin for empty.
				// TODO - Should we check pointsInBin for null? What if, PointsInBin method returns null instead of empty collection.
				if (bin.WidthInPoints == 0 || pointsInBin.Count == 0 || AggregationConstants.IsEmptyValue(pointsInBin.First().Value))
				{
					return new Integrator.PartitionRectangle() { MeanValue = AggregationConstants.EmptyValue(), BaseLength = TimeSpan.Zero };
				}

				return new Integrator.PartitionRectangle() { MeanValue = pointsInBin.First().Value, BaseLength = bin.WidthAsDuration };
			}

			// Replaced std::inner_product with Zip operator.
			// std::inner_product - Computes cumulative inner product of range.
			// Replaced std::plus with Sum operator.
			var count = pointsInBin.Count();
			var integral = pointsInBin.Take(count - 1).Zip(x => pointsInBin.TakeLast(count - 1), trapezoidalArea).Sum();

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
				// TODO - We need information about StatisticType.
				throw new IncompatibleInterpolationTypesException("Incompatible interpolation types in weighted calculation", StatisticType.Mean, firstPoint.InterpolationType, secondPoint.InterpolationType);
			}

			var interpolationType = interpolationMethod.GetResultInterpolationType();

			// TODO - We need information about StatisticContinuityType.
			if (StatisticContinuityType.UseWeightedWhenPossible().RequiresDiscrete(interpolationType))
			{
				// Replaced std::logic_error with LogicException class.
				throw new LogicException("Weighted calculations not defined for this interpolation type");
			}

			if (Point.IsTimeGap(firstPoint, secondPoint) || interpolationMethod.IsValueGap(firstPoint, secondPoint))
			{
				return new Integrator.PartitionRectangle() { MeanValue = 0.0, BaseLength = TimeSpan.Zero };
			}

			// We need to know more about ToDuration() method.
			var durationBetweenPoints = secondPoint.Time.Subtract(firstPoint.Time).Ticks.ToDuration();

			var midpointTime = firstPoint.Time + TimeSpan.FromTicks(durationBetweenPoints.Ticks / 2);

			var meanValue = interpolationMethod.InterpolateValue(firstPoint, secondPoint, midpointTime);

			return new Integrator.PartitionRectangle() { MeanValue = meanValue, BaseLength = durationBetweenPoints };
		}

		// Placeholder method
		public static IEnumerable<Point> PointsInBin(AggregationTimeSeries timeSeries, BinBoundaries bin, bool isWeightedComputation)
		{
			return new List<Point>();
		}
	}
}