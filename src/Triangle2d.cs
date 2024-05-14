using System;
using System.Collections.Generic;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.Geometry;
#else
using GrxCAD.Geometry;
#endif

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Describes a triangle within a plane. It can be seen as a structure of three Point2d.
    /// </summary>
    public struct Triangle2d : IFormattable
    {
        private Point2d _point0;
        private Point2d _point1;
        private Point2d _point2;
        private readonly Point2d[] _points;

        /// <summary>
        /// Creates a new instance of Triangle2d.
        /// </summary>
        /// <param name="points">Array of three Point2d.</param>
        public Triangle2d(Point2d[] points)
        {
            if (points.Length != 3)
                throw new ArgumentOutOfRangeException(nameof(points), "Needs 3 points.");

            _points = points;
            _point0 = points[0];
            _point1 = points[1];
            _point2 = points[2];
        }

        /// <summary>
        /// Creates a new instance of Triangle2d.
        /// </summary>
        /// <param name="a">First point.</param>
        /// <param name="b">Second point.</param>
        /// <param name="c">Third point.</param>
        public Triangle2d(Point2d a, Point2d b, Point2d c)
        {
            _point0 = a;
            _point1 = b;
            _point2 = c;
            _points = new[] { a, b, c };
        }

        /// <summary>
        /// Creates a new instance of Triangle2d.
        /// </summary>
        /// <param name="origin">Origin of the Triangle2d (first point).</param>
        /// <param name="v1">Vector from origin to second point.</param>
        /// <param name="v2">Vector from origin to third point.</param>
        public Triangle2d(Point2d origin, Vector2d v1, Vector2d v2)

        {
            _point0 = origin;
            _point1 = origin + v1;
            _point2 = origin + v2;
            _points = new[] { _point0, _point1, _point2 };
        }

        /// <summary>
        /// Gets the point at specified index.
        /// </summary>
        /// <param name="i">Index of the point.</param>
        /// <returns>The point at specified index..</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// IndexOutOfRangeException is thrown if index is lower than 0 or greater than 2.</exception>
        public Point2d this[int i] => _points[i];

        /// <summary>
        /// Gets the centroid.
        /// </summary>
        public Point2d Centroid =>
            (_point0 + _point1.GetAsVector() + _point2.GetAsVector()) / 3.0;

        /// <summary>
        /// Gets the circumscribed circle.
        /// </summary>
        public CircularArc2d CircumscribedCircle
        {
            get
            {
                Line2d l1 = GetSegment2dAt(0).GetBisector();
                Line2d l2 = GetSegment2dAt(1).GetBisector();
                Point2d[] inters = l1.IntersectWith(l2);
                return inters == null ? null : new CircularArc2d(inters[0], inters[0].GetDistanceTo(_point0));
            }
        }

        /// <summary>
        /// Gets the inscribed circle.
        /// </summary>
        public CircularArc2d InscribedCircle
        {
            get
            {
                Vector2d v1 = _point0.GetVectorTo(_point1).GetNormal();
                Vector2d v2 = _point0.GetVectorTo(_point2).GetNormal();
                Vector2d v3 = _point1.GetVectorTo(_point2).GetNormal();
                if (v1.IsEqualTo(v2) || v2.IsEqualTo(v3))
                    return null;
                Line2d l1 = new Line2d(_point0, v1 + v2);
                Line2d l2 = new Line2d(_point1, v1.Negate() + v3);
                Point2d[] inters = l1.IntersectWith(l2);
                return new CircularArc2d(inters[0], GetSegment2dAt(0).GetDistanceTo(inters[0]));
            }
        }

        /// <summary>
        /// Gets a value indicating if the points are clockwise.
        /// </summary>
        public bool IsClockwise => SignedArea < 0.0;

        /// <summary>
        /// Gets the signed area (negative if points are clockwise).
        /// </summary>
        public double SignedArea =>
            ((_point1.X - _point0.X) * (_point2.Y - _point0.Y) -
             (_point2.X - _point0.X) * (_point1.Y - _point0.Y)) / 2.0;

        /// <summary>
        /// Converts the current instance into a Triangle3d according to the specified plane.
        /// </summary>
        /// <param name="plane">Plane of the Triangle3d.</param>
        /// <returns>The new instance of Triangle3d.</returns>
        public Triangle3d ToTriangle3d(Plane plane) =>
            new Triangle3d(Array.ConvertAll(_points, x => x.ToPoint3d(plane)));

        /// <summary>
        /// Converts the current instance into a Triangle3d according to the plane defined by its Normal and its Elevation.
        /// </summary>
        /// <param name="normal">Normal of the plane.</param>
        /// <param name="elevation">Elevation of the plane.</param>
        /// <returns>The new instance of Triangle3d.</returns>
        public Triangle3d ToTriangle3d(Vector3d normal, double elevation) =>
            new Triangle3d(Array.ConvertAll(_points, x => x.ToPoint3d(normal, elevation)));

        /// <summary>
        /// Gets the angle between two sides at the specified index.
        /// </summary>.
        /// <param name="index">Index of the vertex.</param>
        /// <returns>The angle in radians.</returns>
        public double GetAngleAt(int index)
        {
            double angle =
                this[index].GetVectorTo(this[(index + 1) % 3]).GetAngleTo(
                    this[index].GetVectorTo(this[(index + 2) % 3]));
            if (angle > Math.PI * 2)
                return Math.PI * 2 - angle;
            return angle;
        }

        /// <summary>
        /// Gets the LineSegment2d at specified index.
        /// </summary>
        /// <param name="index">Index of the segment.</param>
        /// <returns>The LineSegment2d at specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// IndexOutOfRangeException is thrown if indexes is lower than 0 or greater than 2.</exception>
        public LineSegment2d GetSegment2dAt(int index)
        {
            if (index > 2)
                throw new IndexOutOfRangeException("Index out of range");
            return new LineSegment2d(this[index], this[(index + 1) % 3]);
        }

        /// <summary>
        /// Gets the intersection points between the current instance and a line using Tolerance.Global.
        /// </summary>
        /// <param name="line2d">The line for which the intersections are searched.</param>
        /// <returns>The list of intersection points (an empty list if none was found).</returns>
        public List<Point2d> IntersectWith(LinearEntity2d line2d) => IntersectWith(line2d, Tolerance.Global);

        /// <summary>
        /// Gets the intersection points between the current instance and a line using the specified Tolerance.
        /// </summary>
        /// <param name="line2d">The line for which the intersections are searched.</param>
        /// <param name="tolerance">Tolerance to be used for comparisons.</param>
        /// <returns>The list of intersection points (an empty list if none was found).</returns>
        public List<Point2d> IntersectWith(LinearEntity2d line2d, Tolerance tolerance)
        {
            List<Point2d> result = new List<Point2d>();
            for (int i = 0; i < 3; i++)
            {
                Point2d[] inters = line2d.IntersectWith(GetSegment2dAt(i), tolerance);
                if (inters != null && inters.Length != 0 && !result.Contains(inters[0]))
                    result.Add(inters[0]);
            }

            return result;
        }

        /// <summary>
        /// Reverse the order of points without changing the origin.
        /// </summary>
        public Triangle2d Inverse()
        {
            return new Triangle2d(_point0, _point2, _point1);
        }

        /// <summary>
        /// Evaluates if the current instance is equal to another Triangle2d using Tolerance.Global.
        /// </summary>
        /// <param name="other">Triangle to be compared to.</param>
        /// <returns>true, if vertices are equal; false, otherwise.</returns>
        public bool IsEqualTo(Triangle2d other) => IsEqualTo(other, Tolerance.Global);

        /// <summary>
        /// Evaluates if the current instance is equal to another Triangle2d using the specified Tolerance.
        /// </summary>
        /// <param name="other">Triangle to be compared to.</param>
        /// <param name="tolerance">Tolerance to be used for comparisons.</param>
        /// <returns>true, if vertices are equal; false, otherwise.</returns>
        public bool IsEqualTo(Triangle2d other, Tolerance tolerance) =>
            other[0].IsEqualTo(_point0, tolerance) && other[1].IsEqualTo(_point1, tolerance) && other[2].IsEqualTo(_point2, tolerance);

        /// <summary>
        /// Gets a value indicating if the the Point2d is strictly inside the current triangle.
        /// </summary>
        /// <param name="point">Point to be evaluated.</param>
        /// <returns>true, if the point is inside; false, otherwise.</returns>
        public bool IsPointInside(Point2d point)
        {
            if (IsPointOn(point))
                return false;
            List<Point2d> inters = IntersectWith(new Ray2d(point, Vector2d.XAxis));
            if (inters.Count != 1)
                return false;
            Point2d p = inters[0];
            return !p.IsEqualTo(this[0]) && !p.IsEqualTo(this[1]) && !p.IsEqualTo(this[2]);
        }

        /// <summary>
        /// Gets a value indicating if the the Point2d is on an segment of the current triangle.
        /// </summary>
        /// <param name="point">Point to be evaluated.</param>
        /// <returns>true, if the point is on a segment; false, otherwise.</returns>
        public bool IsPointOn(Point2d point) =>
            point.IsEqualTo(this[0]) ||
            point.IsEqualTo(this[1]) ||
            point.IsEqualTo(this[2]) ||
            point.IsBetween(this[0], this[1]) ||
            point.IsBetween(this[1], this[2]) ||
            point.IsBetween(this[2], this[0]);

        /// <summary>
        /// Converts the triangle into a Point2d array.
        /// </summary>
        /// <returns>A Point2d array containing the 3 points.</returns>
        public Point2d[] ToArray() => _points;

        /// <summary>
        /// Transforms the triangle using transformation matrix.
        /// </summary>
        /// <param name="matrix">2D transformation matrix.</param>
        /// <returns>The new instance of Triangle2d.</returns>
        public Triangle2d TransformBy(Matrix2d matrix) =>
            new Triangle2d(Array.ConvertAll(_points, p => p.TransformBy(matrix)));

        /// <summary>
        /// Evaluates if the object is equal to the current instance of Triangle2d.
        /// </summary>
        /// <param name="other">Object to be compared.</param>
        /// <returns>true, if vertices are equal; false, otherwise.</returns>
        public override bool Equals(object other) =>
            other is Triangle2d triangle2d && triangle2d.IsEqualTo(this);

        /// <summary>
        /// Serves as the Triangle2d hash function.
        /// </summary>
        /// <returns>A hash code for the current Triangle2d instance..</returns>
        public override int GetHashCode()
        {
            return _point0.GetHashCode() ^ _point1.GetHashCode() ^ _point2.GetHashCode();
        }

        /// <summary>
        /// Returns a string representing the current instance of Triangle2d.
        /// </summary>
        /// <returns>A string containing the 3 points separated with commas.</returns>
        public override string ToString() =>
            $"({_point0},{_point1},{_point2})";

        /// <summary>
        /// Returns a string representing the current instance of Triangle2d.
        /// </summary>
        /// <param name="format">String format to be used for the points.</param>
        /// <returns>A string containing the 3 points in the specified format, separated by commas.</returns>
        public string ToString(string format) =>
            $"({_point0:format},{_point1:format},{_point2:format})";

        /// <summary>
        /// Returns a string representing the current instance of Triangle2d.
        /// </summary>
        /// <param name="format">String format to be used for the points.</param>
        /// <param name="provider">Format provider to be used to format the points.</param>
        /// <returns>A string containing the 3 points in the specified format, separated by commas.</returns>
        public string ToString(string format, IFormatProvider provider) =>
            $"({_point0.ToString(format, provider)},{_point1.ToString(format, provider)},{_point2.ToString(format, provider)})";
    }
}
