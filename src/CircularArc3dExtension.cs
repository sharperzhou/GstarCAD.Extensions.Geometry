using static System.Math;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.Geometry;
using Gssoft.Gscad.Runtime;
#else
using GrxCAD.Geometry;
using GrxCAD.Runtime;
#endif

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Provides extension methods for the CircularArc2d type.
    /// </summary>
    public static class CircularArc3dExtension
    {
        /// <summary>
        /// Gets the tangents between the active CircularArc3d instance complete circle and a point.
        /// </summary>
        /// <remarks>
        /// Tangents start points are on the object to which this method applies, end points on the point passed as argument.
        /// Tangents are always returned in the same order: the tangent on the left side of the line from the circular arc center to the point before the other one.
        /// </remarks>
        /// <param name="arc">The instance to which the method applies.</param>
        /// <param name="point">The Point2d to which tangents are searched.</param>
        /// <returns>An array of LineSegment3d representing the tangents (2) or <c>null</c> if there is none.</returns>
        /// <exception cref="Exception">
        /// eNonCoplanarGeometry is thrown if the objects do not lies on the same plane.</exception>
        public static LineSegment3d[] GetTangentsTo(this CircularArc3d arc, Point3d point)
        {
            // check if arc and point lies on the plane
            Vector3d normal = arc.Normal;
            Matrix3d wcs2Ocs = Matrix3d.WorldToPlane(normal);
            double elevation = arc.Center.TransformBy(wcs2Ocs).Z;
            if (Abs(elevation - point.TransformBy(wcs2Ocs).Z) < Tolerance.Global.EqualPoint)
                throw new Exception(ErrorStatus.NonCoplanarGeometry);

            Plane plane = new Plane(Point3d.Origin, normal);
            CircularArc2d circularArc2d = new CircularArc2d(arc.Center.Convert2d(plane), arc.Radius);
            LineSegment2d[] lines2d = circularArc2d.GetTangentsTo(point.Convert2d(plane));

            if (lines2d == null)
                return null;

            LineSegment3d[] result = new LineSegment3d[lines2d.Length];
            for (int i = 0; i < lines2d.Length; i++)
            {
                LineSegment2d lineSegment2d = lines2d[i];
                result[i] = new LineSegment3d(lineSegment2d.StartPoint.ToPoint3d(normal, elevation),
                    lineSegment2d.EndPoint.ToPoint3d(normal, elevation));
            }

            return result;
        }

        /// <summary>
        /// Gets the tangents between the active CircularArc3d instance complete circle and another one.
        /// </summary>
        /// <remarks>
        /// Tangents start points are on the object to which this method applies, end points on the one passed as argument.
        /// Tangents are always returned in the same order: outer tangents before inner tangents, and for both,
        /// the tangent on the left side of the line from this circular arc center to the other one before the other one.
        /// </remarks>
        /// <param name="arc">The instance to which the method applies.</param>
        /// <param name="other">The CircularArc2d to which searched for tangents.</param>
        /// <param name="tangentType">An enum value specifying which type of tangent is returned.</param>
        /// <returns>An array of LineSegment3d representing the tangents (maybe 2 or 4) or <c>null</c> if there is none.</returns>
        /// <exception cref="Exception">
        /// eNonCoplanarGeometry is thrown if the objects do not lies on the same plane.</exception>
        public static LineSegment3d[] GetTangentsTo(this CircularArc3d arc, CircularArc3d other,
            TangentType tangentType)
        {
            // check if circles lies on the same plane
            Vector3d normal = arc.Normal;
            Matrix3d wcs2Ocs = Matrix3d.WorldToPlane(normal);
            double elevation = arc.Center.TransformBy(wcs2Ocs).Z;
            if (!(normal.IsParallelTo(other.Normal) &&
                  Abs(elevation - other.Center.TransformBy(wcs2Ocs).Z) < Tolerance.Global.EqualPoint))
                throw new Exception(ErrorStatus.NonCoplanarGeometry);

            Plane plane = new Plane(Point3d.Origin, normal);
            CircularArc2d circularArc2d1 = new CircularArc2d(arc.Center.Convert2d(plane), arc.Radius);
            CircularArc2d circularArc2d2 = new CircularArc2d(other.Center.Convert2d(plane), other.Radius);
            LineSegment2d[] lines2d = circularArc2d1.GetTangentsTo(circularArc2d2, tangentType);

            if (lines2d == null)
                return null;

            LineSegment3d[] result = new LineSegment3d[lines2d.Length];
            for (int i = 0; i < lines2d.Length; i++)
            {
                LineSegment2d lineSegment2d = lines2d[i];
                result[i] = new LineSegment3d(lineSegment2d.StartPoint.ToPoint3d(normal, elevation),
                    lineSegment2d.EndPoint.ToPoint3d(normal, elevation));
            }

            return result;
        }
    }
}
