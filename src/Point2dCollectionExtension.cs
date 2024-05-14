﻿using System;
using System.Collections.Generic;
using System.Linq;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.Geometry;
#else
using GrxCAD.Geometry;
#endif

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Provides extension methods for the Point2dCollection and IEnumerable&lt;Point2d&gt; types.
    /// </summary>
    public static class Point2dCollectionExtension
    {
        /// <summary>
        /// Removes duplicated points using Tolerance.Global.EqualPoint.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <returns>A sequence of distinct points.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if the collection is null.</exception>
        public static IEnumerable<Point2d> RemoveDuplicates(this Point2dCollection source) =>
            source.RemoveDuplicates(Tolerance.Global);

        /// <summary>
        /// Removes duplicated points using the specified Tolerance.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="tolerance">The Tolerance to be used in equality comparison.</param>
        /// <returns>A sequence of distinct points.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if the collection is null.</exception>
        public static IEnumerable<Point2d> RemoveDuplicates(this Point2dCollection source, Tolerance tolerance)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Cast<Point2d>().Distinct(new Point2dComparer(tolerance));
        }

        /// <summary>
        /// Removes duplicated points using Tolerance.Global.EqualPoint.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <returns>A sequence of distinct points.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if the collection is null.</exception>
        public static IEnumerable<Point2d> RemoveDuplicates(this IEnumerable<Point2d> source)
        {
            return source.RemoveDuplicates(Tolerance.Global);
        }

        /// <summary>
        /// Removes duplicated points using the specified Tolerance.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="tolerance">The Tolerance to be used in equality comparison.</param>
        /// <returns>A sequence of distinct points.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if the collection is null.</exception>
        public static IEnumerable<Point2d> RemoveDuplicates(this IEnumerable<Point2d> source, Tolerance tolerance)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Distinct(new Point2dComparer(tolerance));
        }

        /// <summary>
        /// Evaluates if the collection contains <c>point</c> using Tolerance.Global.EqualPoint.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="point">Point to search for.</param>
        /// <returns>true, if <c>point</c> is found; false, otherwise.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if the collection is null.</exception>
        public static bool Contains(this Point2dCollection source, Point2d point)
        {
            return source.Contains(point, Tolerance.Global);
        }

        /// <summary>
        /// Evaluates if the collection contains <c>point</c> using the specified tolerance.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="point">Point to search for.</param>
        /// <param name="tolerance">The Tolerance to be used in equality comparison..</param>
        /// <returns>true, if <c>point</c> is found; false, otherwise.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if the collection is null.</exception>
        public static bool Contains(this Point2dCollection source, Point2d point, Tolerance tolerance)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            for (int i = 0; i < source.Count; i++)
            {
                if (point.IsEqualTo(source[i], tolerance))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Provides equality comparison methods for the Point2d type.
        /// </summary>
        private class Point2dComparer : IEqualityComparer<Point2d>
        {
            private readonly Tolerance _tolerance;
            private readonly double _precise;

            /// <summary>
            /// Creates a new instance ot Point2dComparer
            /// </summary>
            /// <param name="tolerance">The Tolerance to be used in equality comparison.</param>
            public Point2dComparer(Tolerance tolerance)
            {
                _tolerance = tolerance;
                _precise = tolerance.EqualPoint * 10.0;
            }

            /// <summary>
            /// Evaluates if two points are equal.
            /// </summary>
            /// <param name="a">First point.</param>
            /// <param name="b">Second point.</param>
            /// <returns>true, if the two points are equal; false, otherwise.</returns>
            public bool Equals(Point2d a, Point2d b)
            {
                return a.IsEqualTo(b, _tolerance);
            }

            /// <summary>
            /// Serves as hashing function for the Point2d type.
            /// </summary>
            /// <param name="pt">Point.</param>
            /// <returns>The hash code.</returns>
            public int GetHashCode(Point2d pt) =>
                new Point2d(Round(pt.X), Round(pt.Y)).GetHashCode();

            private double Round(double number)
            {
                return _precise == 0.0 ? number : Math.Floor(number / _precise + 0.5) * _precise;
            }
        }
    }
}
