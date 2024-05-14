using System;
using System.Collections.Generic;
using System.Linq;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.Geometry;
using Gssoft.Gscad.DatabaseServices;
#else
using GrxCAD.Geometry;
using GrxCAD.DatabaseServices;
#endif

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Provides extension methods for Point3dCollection and IEnumerable&lt;Point3d&gt; types.
    /// </summary>
    public static class Point3dCollectionExtension
    {
        /// <summary>
        /// Removes duplicated points in the collection using Tolerance.Global.EqualPoint.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <returns>A sequence of distinct points.</returns>
        public static IEnumerable<Point3d> RemoveDuplicates(this Point3dCollection source) =>
            source.RemoveDuplicates(Tolerance.Global);

        /// <summary>
        /// Removes duplicated points in the collection using the specified Tolerance.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="tolerance">The tolerance to be used in equality comparison.</param>
        /// <returns>A sequence of distinct points.</returns>
        public static IEnumerable<Point3d> RemoveDuplicates(this Point3dCollection source, Tolerance tolerance)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Cast<Point3d>().Distinct(new Point3dComparer(tolerance));
        }

        /// <summary>
        /// Removes duplicated points in the sequence using Tolerance.Global.EqualPoint.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <returns>A sequence of distinct points.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if the collection is null.</exception>
        public static IEnumerable<Point3d> RemoveDuplicates(this IEnumerable<Point3d> source) =>
            source.RemoveDuplicates(Tolerance.Global);

        /// <summary>
        /// Removes duplicated points in the sequence using the specified Tolerance.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="tolerance">The tolerance to be used in equality comparison.</param>
        /// <returns>A sequence of distinct points.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if the collection is null.</exception>
        public static IEnumerable<Point3d> RemoveDuplicates(this IEnumerable<Point3d> source, Tolerance tolerance)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Distinct(new Point3dComparer(tolerance));
        }

        /// <summary>
        /// Gets a value indicating if the the collection contains the point using Tolerance.Global.EqualPoint.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="point">The point to search.</param>
        /// <returns>true, if the point is found ; false, otherwise.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if the collection is null.</exception>
        public static bool Contains(this Point3dCollection source, Point3d point) =>
            source.Contains(point, Tolerance.Global);

        /// <summary>
        /// Gets a value indicating if the the collection contains the point using the specified Tolerance.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="point">The point to search.</param>
        /// <param name="tolerance">The Tolerance to be use in comparisons.</param>
        /// <returns>true, if the point is found ; false, otherwise.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if the collection is null.</exception>
        public static bool Contains(this Point3dCollection source, Point3d point, Tolerance tolerance)
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
        /// Gets the extents of the collection of points.
        /// </summary>
        /// <param name="points">The instance to which this method applies.</param>
        /// <returns>An Extents3d instance.</returns>
        /// <exception cref="ArgumentException">ArgumentException is thrown if the collection is null or empty.</exception>
        public static Extents3d GetExtents(this Point3dCollection points)
        {
            return points.Cast<Point3d>().GetExtents();
        }

        /// <summary>
        /// Gets the extents of the sequence of points.
        /// </summary>
        /// <param name="points">The instance to which this method applies.</param>
        /// <returns>An Extents3d instance.</returns>
        /// <exception cref="ArgumentException">ArgumentException is thrown if the collection is null or empty.</exception>
        public static Extents3d GetExtents(this IEnumerable<Point3d> points)
        {
            var ret = new Extents3d();
            bool ok = false;
            foreach (var point in points ?? Enumerable.Empty<Point3d>())
            {
                ok = true;
                ret.AddPoint(point);
            }

            if (points == null || !ok)
                throw new ArgumentException("Null or empty sequence");

            return ret;
        }

        /// <summary>
        /// Provides equality comparison methods for the Point3d type.
        /// </summary>
        private class Point3dComparer : IEqualityComparer<Point3d>
        {
            private readonly Tolerance _tolerance;
            private readonly double _precise;

            /// <summary>
            /// Creates a new instance ot Point3dComparer
            /// </summary>
            /// <param name="tolerance">The Tolerance to be used in equality comparison.</param>
            public Point3dComparer(Tolerance tolerance)
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
            public bool Equals(Point3d a, Point3d b) => a.IsEqualTo(b, _tolerance);

            /// <summary>
            /// Serves as hashing function for the Point2d type.
            /// </summary>
            /// <param name="point">Point.</param>
            /// <returns>The hash code.</returns>
            public int GetHashCode(Point3d point) =>
                new Point3d(Round(point.X), Round(point.Y), Round(point.Z)).GetHashCode();

            private double Round(double number) =>
                _precise == 0.0 ? number : Math.Floor(number / _precise + 0.5) * _precise;
        }
    }
}
