using System;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.Geometry;
#else
using GrxCAD.Geometry;
#endif

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Provides extension methods for the CircularArc2d type.
    /// </summary>
    public static class CircularArc2dExtension
    {
        /// <summary>
        /// Gets the signed area of the circular arc (negative if points are clockwise).
        /// </summary>
        /// <param name="arc">The instance to which the method applies.</param>
        /// <returns>The signed area.</returns>
        public static double GetSignedArea(this CircularArc2d arc)
        {
            double radius = arc.Radius;
            double angle = arc.IsClockWise ? arc.StartAngle - arc.EndAngle : arc.EndAngle - arc.StartAngle;
            return radius * radius * (angle - Math.Sin(angle)) / 2.0;
        }

        /// <summary>
        /// Gets the centroid of the arc.
        /// </summary>
        /// <param name="arc">The instance to which the method applies.</param>
        /// <returns>The centroid of the arc.</returns>
        public static Point2d GetCentroid(this CircularArc2d arc)
        {
            Point2d start = arc.StartPoint;
            Point2d end = arc.EndPoint;
            double area = arc.GetSignedArea();
            double chord = start.GetDistanceTo(end);
            double angle = (end - start).Angle;
            return arc.Center.ToPolar(angle - Math.PI / 2.0, chord * chord * chord / (12.0 * area));
        }

        /// <summary>
        /// Gets the tangents between the active CircularArc2d instance complete circle and the point.
        /// </summary>
        /// <remarks>
        /// Tangents start points are on the object to which this method applies, end points on the point passed as argument.
        /// Tangents are always returned in the same order: the tangent on the left side of the line from the circular arc center to the point before the other one.
        /// </remarks>
        /// <param name="arc">The instance to which the method applies.</param>
        /// <param name="point">The Point2d to which tangents are searched.</param>
        /// <returns>An array of LineSegment2d representing the tangents (2) or <c>null</c> if there is none.</returns>
        public static LineSegment2d[] GetTangentsTo(this CircularArc2d arc, Point2d point)
        {
            // check if the point is inside the circle
            Point2d center = arc.Center;
            if (point.GetDistanceTo(center) <= arc.Radius)
                return null;

            Vector2d vec = center.GetVectorTo(point) / 2.0;
            CircularArc2d circularArc2d = new CircularArc2d(center + vec, vec.Length);
            Point2d[] intersects = arc.IntersectWith(circularArc2d);
            if (intersects == null)
                return null;
            LineSegment2d[] result = new LineSegment2d[2];
            Vector2d v1 = intersects[0] - center;
            int i = vec.X * v1.Y - vec.Y - v1.X > 0 ? 0 : 1;
            int j = i ^ 1;
            result[i] = new LineSegment2d(intersects[0], point);
            result[j] = new LineSegment2d(intersects[1], point);
            return result;
        }

        /// <summary>
        /// Gets the tangents between the active CircularArc2d instance complete circle and another one.
        /// </summary>
        /// <remarks>
        /// Tangents start points are on the object to which this method applies, end points on the one passed as argument.
        /// Tangents are always returned in the same order: outer tangents before inner tangents, and for both,
        /// the tangent on the left side of the line from this circular arc center to the other one before the other one.
        /// </remarks>
        /// <param name="arc">The instance to which the method applies.</param>
        /// <param name="other">The CircularArc2d to which searched for tangents.</param>
        /// <param name="type">An enum value specifying which type of tangent is returned.</param>
        /// <returns>An array of LineSegment2d representing the tangents (maybe 2 or 4) or <c>null</c> if there is none.</returns>
        public static LineSegment2d[] GetTangentsTo(this CircularArc2d arc, CircularArc2d other, TangentType type)
        {
            // check if a circle is inside the other
            double dist = arc.Center.GetDistanceTo(other.Center);
            if (dist - Math.Abs(arc.Radius - other.Radius) <= Tolerance.Global.EqualPoint)
                return null;

            // check if circles overlap
            bool overlap = arc.Radius + other.Radius >= dist;
            if (overlap && type == TangentType.Inner)
                return null;

            CircularArc2d tmp1;
            Point2d[] intersects;
            Vector2d vec1;
            Vector2d vec2;
            var vec = other.Center - arc.Center;
            int i, j;
            LineSegment2d[] result =
                new LineSegment2d[type.Equals(TangentType.Inner | TangentType.Outer) && !overlap ? 4 : 2];

            // outer tangents
            if (type.HasFlag(TangentType.Outer))
            {
                if (Math.Abs(arc.Radius - other.Radius) < Tolerance.Global.EqualPoint)
                {
                    Line2d perp = new Line2d(arc.Center, vec.GetPerpendicularVector());
                    intersects = arc.IntersectWith(perp);
                    if (intersects == null)
                        return null;
                    vec1 = (intersects[0] - arc.Center).GetNormal();
                    (intersects[1] - arc.Center).GetNormal();
                    i = vec.X * vec1.Y - vec.Y - vec1.X > 0 ? 0 : 1;
                    j = i ^ 1;
                    result[i] = new LineSegment2d(intersects[0], intersects[0] + vec);
                    result[j] = new LineSegment2d(intersects[1], intersects[1] + vec);
                }
                else
                {
                    Point2d center = arc.Radius < other.Radius ? other.Center : arc.Center;
                    tmp1 = new CircularArc2d(center, Math.Abs(arc.Radius - other.Radius));
                    var tmp2 = new CircularArc2d(arc.Center + vec / 2.0, dist / 2.0);
                    intersects = tmp1.IntersectWith(tmp2);
                    if (intersects == null)
                        return null;
                    vec1 = (intersects[0] - center).GetNormal();
                    vec2 = (intersects[1] - center).GetNormal();
                    i = vec.X * vec1.Y - vec.Y - vec1.X > 0 ? 0 : 1;
                    j = i ^ 1;
                    result[i] = new LineSegment2d(arc.Center + vec1 * arc.Radius, other.Center + vec1 * other.Radius);
                    result[j] = new LineSegment2d(arc.Center + vec2 * arc.Radius, other.Center + vec2 * other.Radius);
                }
            }

            // not inner tangents
            if (!type.HasFlag(TangentType.Inner) || overlap)
                return result;

            double ratio = arc.Radius / (arc.Radius + other.Radius) / 2.0;
            tmp1 = new CircularArc2d(arc.Center + vec * ratio, dist * ratio);
            intersects = arc.IntersectWith(tmp1);
            if (intersects == null)
                return null;
            vec1 = (intersects[0] - arc.Center).GetNormal();
            vec2 = (intersects[1] - arc.Center).GetNormal();
            i = vec.X * vec1.Y - vec.Y - vec1.X > 0 ? 2 : 3;
            j = i == 2 ? 3 : 2;
            result[i] = new LineSegment2d(arc.Center + vec1 * arc.Radius,
                other.Center + vec1.Negate() * other.Radius);
            result[j] = new LineSegment2d(arc.Center + vec2 * arc.Radius,
                other.Center + vec2.Negate() * other.Radius);

            return result;
        }
    }
}
