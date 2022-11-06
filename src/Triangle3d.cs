using System;
using GrxCAD.Geometry;

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Describes a triangle within the 3D space. It can be seen as a structure of three Point3d.
    /// </summary>
    public struct Triangle3d : IFormattable
    {
        private Point3d _point0;
        private Point3d _point1;
        private Point3d _point2;
        private readonly Point3d[] _points;


        /// <summary>
        /// Creates a new instance of Triangle3d.
        /// </summary>
        /// <param name="points">Array of three Point3d.</param>
        public Triangle3d(Point3d[] points)
        {
            if (points.Length != 3)
                throw new ArgumentOutOfRangeException(nameof(points), "Needs 3 points.");

            _points = points;
            _point0 = points[0];
            _point1 = points[1];
            _point2 = points[2];
        }

        /// <summary>
        /// Creates a new instance of Triangle3d.
        /// </summary>
        /// <param name="a">First point.</param>
        /// <param name="b">Second point.</param>
        /// <param name="c">Third point.</param>
        public Triangle3d(Point3d a, Point3d b, Point3d c)
        {
            _point0 = a;
            _point1 = b;
            _point2 = c;
            _points = new[] { a, b, c };
        }

        /// <summary>
        /// Creates a new instance of Triangle3d.
        /// </summary>
        /// <param name="origin">Origin of the Triangle2d (first point).</param>
        /// <param name="v1">Vector from origin to second point.</param>
        /// <param name="v2">Vector from origin to third point.</param>
        public Triangle3d(Point3d origin, Vector3d v1, Vector3d v2)

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
        public Point3d this[int i] => _points[i];

        /// <summary>
        /// Gets the area of the triangle.
        /// </summary>
        public double Area =>
            Math.Abs(((_point1.X - _point0.X) * (_point2.Y - _point0.Y) -
                      (_point2.X - _point0.X) * (_point1.Y - _point0.Y)) / 2.0);

        /// <summary>
        /// Gets the centroid.
        /// </summary>
        public Point3d Centroid => (_point0 + _point1.GetAsVector() + _point2.GetAsVector()) / 3.0;

        /// <summary>
        /// Gets the circumscribed circle.
        /// </summary>
        public CircularArc3d CircumscribedCircle
        {
            get
            {
                CircularArc2d ca2d = ToTriangle2d().CircumscribedCircle;
                return ca2d == null ? null : new CircularArc3d(ca2d.Center.ToPoint3d(GetPlane()), Normal, ca2d.Radius);
            }
        }

        /// <summary>
        /// Gets the elevation of the plane the triangle lies on.
        /// </summary>
        public double Elevation => _point0.TransformBy(Matrix3d.WorldToPlane(Normal)).Z;

        /// <summary>
        /// Gets the unit vector of the greatest slope of the plane the triangle lies on.
        /// </summary>
        public Vector3d GreatestSlope =>
            Normal.IsParallelTo(Vector3d.ZAxis) ? new Vector3d(0.0, 0.0, 0.0) :
            Normal.Z == 0.0 ? Vector3d.ZAxis.Negate() :
            new Vector3d(-Normal.Y, Normal.X, 0.0).CrossProduct(Normal).GetNormal();

        /// <summary>
        /// Gets the unit vector of the horizontal of the plane the triangle lies on.
        /// </summary>
        public Vector3d Horizontal =>
            Normal.IsParallelTo(Vector3d.ZAxis) ? Vector3d.XAxis : new Vector3d(-Normal.Y, Normal.X, 0.0).GetNormal();

        /// <summary>
        /// Gets the inscribed circle.
        /// </summary>
        public CircularArc3d InscribedCircle
        {
            get
            {
                CircularArc2d ca2d = ToTriangle2d().InscribedCircle;
                return ca2d == null ? null : new CircularArc3d(ca2d.Center.ToPoint3d(GetPlane()), Normal, ca2d.Radius);
            }
        }

        /// <summary>
        /// Get a value indicating if the plane the triangle lies on is horizontal.
        /// </summary>
        public bool IsHorizontal => Math.Abs(_point0.Z - _point1.Z) < 1e-9 && Math.Abs(_point0.Z - _point2.Z) < 1e-9;

        /// <summary>
        /// Gets the Normal of the plane the triangle lies on.
        /// </summary>
        public Vector3d Normal => (_point1 - _point0).CrossProduct(_point2 - _point0).GetNormal();

        /// <summary>
        /// Gets the slope of the triangle expressed in percent.
        /// </summary>
        public double SlopePercent =>
            Normal.Z == 0.0
                ? double.PositiveInfinity
                : Math.Abs(100.0 * Math.Sqrt(Normal.X * Normal.X + Normal.Y * Normal.Y) / Normal.Z);

        /// <summary>
        /// Gets the coordinate system of the triangle (origin = GetCentroid, X axis = Horizontal, Z axis = Normal).
        /// </summary>
        public Matrix3d SlopeUcs
        {
            get
            {
                Point3d origin = Centroid;
                Vector3d axisZ = Normal;
                Vector3d axisX = Horizontal;
                Vector3d axisY = axisZ.CrossProduct(axisX).GetNormal();
                return new Matrix3d(new[]{
                    axisX.X, axisY.X, axisZ.X, origin.X,
                    axisX.Y, axisY.Y, axisZ.Y, origin.Y,
                    axisX.Z, axisY.Z, axisZ.Z, origin.Z,
                    0.0, 0.0, 0.0, 1.0});
            }
        }

        /// <summary>
        /// Converts the current instance into a Triangle2d.
        /// </summary>
        /// <returns>The new instance of Triangle2d.</returns>
        public Triangle2d ToTriangle2d()
        {
            var plane = GetPlane();
            return new Triangle2d(Array.ConvertAll(_points, x => x.Convert2d(plane)));
        }

        /// <summary>
        /// Projects the current instance onto the XY plane.
        /// </summary>
        /// <returns>The resulting Triangle2d.</returns>
        public Triangle2d Flatten() =>
            new Triangle2d(
                new Point2d(this[0].X, this[0].Y),
                new Point2d(this[1].X, this[1].Y),
                new Point2d(this[2].X, this[2].Y));

        /// <summary>
        /// Gets the angle between two sides at the specified index.
        /// </summary>.
        /// <param name="index">Index of the vertex.</param>
        /// <returns>The angle in radians.</returns>
        public double GetAngleAt(int index) =>
            this[index].GetVectorTo(this[(index + 1) % 3]).GetAngleTo(
                this[index].GetVectorTo(this[(index + 2) % 3]));

        /// <summary>
        /// Gets the bounded plane defined by the current triangle.
        /// </summary>
        /// <returns>The bounded plane.</returns>
        public BoundedPlane GetBoundedPlane() => new BoundedPlane(_point0, _point1, _point2);

        /// <summary>
        /// Gets the unbounded plane defined by the current triangle.
        /// </summary>
        /// <returns>The plane.</returns>
        public Plane GetPlane()
        {
            Point3d origin =
                new Point3d(0.0, 0.0, Elevation).TransformBy(Matrix3d.PlaneToWorld(Normal));
            return new Plane(origin, Normal);
        }

        /// <summary>
        /// Gets the LineSegment3d at specified index.
        /// </summary>
        /// <param name="index">Index of the segment.</param>
        /// <returns>The LineSegment2d at specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// IndexOutOfRangeException is thrown if indexes is lower than 0 or greater than 2.</exception>
        public LineSegment3d GetSegment3dAt(int index)
        {
            if (index > 2)
                throw new IndexOutOfRangeException("Index out of range");
            return new LineSegment3d(this[index], this[(index + 1) % 3]);
        }

        /// <summary>
        /// Reverse the order of points without changing the origin.
        /// </summary>
        public Triangle3d Inverse()
        {
            return new Triangle3d(_point0, _point2, _point1);
        }

        /// <summary>
        /// Evaluates if the current instance is equal to another Triangle2d using Tolerance.Global.
        /// </summary>
        /// <param name="other">Triangle to be compared to.</param>
        /// <returns>true, if vertices are equal; false, otherwise.</returns>
        public bool IsEqualTo(Triangle3d other)
        {
            return IsEqualTo(other, Tolerance.Global);
        }

        /// <summary>
        /// Evaluates if the current instance is equal to another Triangle3d using the specified Tolerance.
        /// </summary>
        /// <param name="other">Triangle to be compared to.</param>
        /// <param name="tolerance">Tolerance to be used for comparisons.</param>
        /// <returns>true, if vertices are equal; false, otherwise.</returns>
        public bool IsEqualTo(Triangle3d other, Tolerance tolerance)
        {
            return other[0].IsEqualTo(_point0, tolerance) && other[1].IsEqualTo(_point1, tolerance) &&
                   other[2].IsEqualTo(_point2, tolerance);
        }

        /// <summary>
        /// Gets a value indicating if the the Point2d is strictly inside the current triangle.
        /// </summary>
        /// <param name="point">Point to be evaluated.</param>
        /// <returns>true, if the point is inside; false, otherwise.</returns>
        public bool IsPointInside(Point3d point)
        {
            Tolerance tol = new Tolerance(1e-9, 1e-9);
            Vector3d v1 = point.GetVectorTo(_point0).CrossProduct(point.GetVectorTo(_point1)).GetNormal();
            Vector3d v2 = point.GetVectorTo(_point1).CrossProduct(point.GetVectorTo(_point2)).GetNormal();
            Vector3d v3 = point.GetVectorTo(_point2).CrossProduct(point.GetVectorTo(_point0)).GetNormal();
            return v1.IsEqualTo(v2, tol) && v2.IsEqualTo(v3, tol);
        }

        /// <summary>
        /// Gets a value indicating if the the Point3d is on an segment of the current triangle.
        /// </summary>
        /// <param name="point">Point to be evaluated.</param>
        /// <returns>true, if the point is on a segment; false, otherwise.</returns>
        public bool IsPointOn(Point3d point)
        {
            Tolerance tol = new Tolerance(1e-9, 1e-9);
            Vector3d v0 = new Vector3d(0.0, 0.0, 0.0);
            Vector3d v1 = point.GetVectorTo(_point0).CrossProduct(point.GetVectorTo(_point1));
            Vector3d v2 = point.GetVectorTo(_point1).CrossProduct(point.GetVectorTo(_point2));
            Vector3d v3 = point.GetVectorTo(_point2).CrossProduct(point.GetVectorTo(_point0));
            return v1.IsEqualTo(v0, tol) || v2.IsEqualTo(v0, tol) || v3.IsEqualTo(v0, tol);
        }

        /// <summary>
        /// Converts the triangle into a Point2d array.
        /// </summary>
        /// <returns>A Point2d array containing the 3 points.</returns>
        public Point3d[] ToArray() => _points;

        /// <summary>
        /// Transforms the triangle using transformation matrix.
        /// </summary>
        /// <param name="matrix">3D transformation matrix.</param>
        /// <returns>The new instance of Triangle3d.</returns>
        public Triangle3d TransformBy(Matrix3d matrix) =>
            new Triangle3d(Array.ConvertAll(_points, p => p.TransformBy(matrix)));

        /// <summary>
        /// Evaluates if the object is equal to the current instance of Triangle3d.
        /// </summary>
        /// <param name="other">Object to be compared.</param>
        /// <returns>true, if vertices are equal; false, otherwise.</returns>
        public override bool Equals(object other) =>
            other is Triangle3d triangle3d && triangle3d.IsEqualTo(this);

        /// <summary>
        /// Serves as the Triangle3d hash function.
        /// </summary>
        /// <returns>A hash code for the current Triangle3d instance..</returns>
        public override int GetHashCode()
        {
            return _point0.GetHashCode() ^ _point1.GetHashCode() ^ _point2.GetHashCode();
        }

        /// <summary>
        /// Returns a string representing the current instance of Triangle3d.
        /// </summary>
        /// <returns>A string containing the 3 points separated by commas.</returns>
        public override string ToString() => $"({_point0},{_point1},{_point2})";

        /// <summary>
        /// Returns a string representing the current instance of Triangle3d.
        /// </summary>
        /// <param name="format">String format to be used for the points.</param>
        /// <returns>A string containing the 3 points in the specified format, separated by commas.</returns>
        public string ToString(string format) =>
            $"({_point0:format},{_point1:format},{_point2:format})";

        /// <summary>
        /// Returns a string representing the current instance of Triangle3d.
        /// </summary>
        /// <param name="format">String format to be used for the points.</param>
        /// <param name="provider">Format provider to be used to format the points.</param>
        /// <returns>A string containing the 3 points in the specified format, separated by commas.</returns>
        public string ToString(string format, IFormatProvider provider) =>
            $"({_point0.ToString(format, provider)},{_point1.ToString(format, provider)},{_point2.ToString(format, provider)})";
    }
}
