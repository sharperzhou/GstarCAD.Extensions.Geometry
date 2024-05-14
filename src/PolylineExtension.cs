using System;

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
    /// Provides extension methods for the Polyline type.
    /// </summary>
    public static class PolylineExtension
    {
        /// <summary>
        /// Cuts the Polyline at specified point (closest point if the point does not lies on the polyline).
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <param name="point">the point where to cut the Polyline.</param>
        /// <returns>An array containing the two resulting polylines.</returns>
        public static Polyline[] BreakAt(this Polyline polyline, Point3d point)
        {
            point = polyline.GetClosestPointTo(point, false);

            if (point.IsEqualTo(polyline.StartPoint))
                return new[] { null, (Polyline)polyline.Clone() };

            if (point.IsEqualTo(polyline.EndPoint))
                return new[] { (Polyline)polyline.Clone(), null };

            double param = polyline.GetParameterAtPoint(point);
            int index = (int)param;
            int num = polyline.NumberOfVertices;
            Polyline pl1 = (Polyline)polyline.Clone();
            if (polyline.Closed)
            {
                pl1.AddVertexAt(
                    polyline.NumberOfVertices,
                    polyline.GetPoint2dAt(0),
                    polyline.GetStartWidthAt(num - 1),
                    polyline.GetEndWidthAt(num - 1),
                    polyline.GetBulgeAt(num - 1));
                pl1.Closed = false;
            }

            Polyline pl2 = (Polyline)pl1.Clone();

            if (Math.Abs(Math.Round(param, 6) - index) < 1e-9)
            {
                for (int i = pl1.NumberOfVertices - 1; i > index; i--)
                {
                    pl1.RemoveVertexAt(i);
                }

                for (int i = 0; i < index; i++)
                {
                    pl2.RemoveVertexAt(0);
                }

                return new[] { pl1, pl2 };
            }

            Point2d pt = point.Convert2d(new Plane(Point3d.Origin, polyline.Normal));
            for (int i = pl1.NumberOfVertices - 1; i > index + 1; i--)
            {
                pl1.RemoveVertexAt(i);
            }

            pl1.SetPointAt(index + 1, pt);
            for (int i = 0; i < index; i++)
            {
                pl2.RemoveVertexAt(0);
            }

            pl2.SetPointAt(0, pt);
            if (polyline.GetBulgeAt(index) != 0.0)
            {
                double bulge = polyline.GetBulgeAt(index);
                pl1.SetBulgeAt(index, ScaleBulge(bulge, param - index));
                pl2.SetBulgeAt(0, ScaleBulge(bulge, index + 1 - param));
            }

            return new[] { pl1, pl2 };
        }

        /// <summary>
        /// Gets the Polyline centroid.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <returns>The Polyline centroid (OCS coordinates).</returns>
        public static Point2d GetCentroid2d(this Polyline polyline)
        {
            Point2d center = new Point2d();
            CircularArc2d arc;
            double tmpArea;
            double area = 0.0;
            int last = polyline.NumberOfVertices - 1;
            Point2d p0 = polyline.GetPoint2dAt(0);

            if (polyline.GetSegmentType(0) == SegmentType.Arc)
            {
                arc = polyline.GetArcSegment2dAt(0);
                area = arc.GetSignedArea();
                center = arc.GetCentroid() * area;
            }

            for (int i = 1; i < last; i++)
            {
                Triangle2d tri = new Triangle2d(p0, polyline.GetPoint2dAt(i), polyline.GetPoint2dAt(i + 1));
                tmpArea = tri.SignedArea;
                center += (tri.Centroid * tmpArea).GetAsVector();
                area += tmpArea;
                if (polyline.GetSegmentType(i) != SegmentType.Arc)
                    continue;

                arc = polyline.GetArcSegment2dAt(i);
                tmpArea = arc.GetSignedArea();
                area += tmpArea;
                center += (arc.GetCentroid() * tmpArea).GetAsVector();
            }

            if ((polyline.GetSegmentType(0) != SegmentType.Arc) || (polyline.Closed != true))
                return center.DivideBy(area);

            arc = polyline.GetArcSegment2dAt(last);
            tmpArea = arc.GetSignedArea();
            area += tmpArea;
            center += (arc.GetCentroid() * tmpArea).GetAsVector();
            return center.DivideBy(area);
        }

        /// <summary>
        /// Gets the Polyline centroid.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <returns>The Polyline centroid (WCS coordinates).</returns>
        public static Point3d GetCentroid(this Polyline polyline)
        {
            return polyline.GetCentroid2d().ToPoint3d(polyline.Normal, polyline.Elevation);
        }

        /// <summary>
        /// Adds an arc (fillet), when possible, at each vertex.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <param name="radius">The arc radius.</param>
        public static void FilletAll(this Polyline polyline, double radius)
        {
            int n = polyline.Closed ? 0 : 1;
            for (int i = n; i < polyline.NumberOfVertices - n; i += 1 + polyline.FilletAt(i, radius))
            {
            }
        }

        /// <summary>
        /// Adds an arc (fillet), when possible, at specified vertex.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <param name="index">The vertex index.</param>
        /// <param name="radius">The arc radius.</param>
        /// <returns>1, if the operation succeed; 0, if it failed</returns>
        public static int FilletAt(this Polyline polyline, int index, double radius)
        {
            int prev = index == 0 && polyline.Closed ? polyline.NumberOfVertices - 1 : index - 1;
            if (polyline.GetSegmentType(prev) != SegmentType.Line ||
                polyline.GetSegmentType(index) != SegmentType.Line)
            {
                return 0;
            }

            LineSegment2d seg1 = polyline.GetLineSegment2dAt(prev);
            LineSegment2d seg2 = polyline.GetLineSegment2dAt(index);
            Vector2d vec1 = seg1.StartPoint - seg1.EndPoint;
            Vector2d vec2 = seg2.EndPoint - seg2.StartPoint;
            double angle = (Math.PI - vec1.GetAngleTo(vec2)) / 2.0;
            double dist = radius * Math.Tan(angle);
            if (dist == 0.0 || dist > seg1.Length || dist > seg2.Length)
            {
                return 0;
            }

            Point2d pt1 = seg1.EndPoint + vec1.GetNormal() * dist;
            Point2d pt2 = seg2.StartPoint + vec2.GetNormal() * dist;
            double bulge = Math.Tan(angle / 2.0);
            if (IsClockwise(seg1.StartPoint, seg1.EndPoint, seg2.EndPoint))
            {
                bulge = -bulge;
            }

            polyline.AddVertexAt(index, pt1, bulge, 0.0, 0.0);
            polyline.SetPointAt(index + 1, pt2);
            return 1;
        }

        /// <summary>
        /// Creates a new Polyline which is the result of the projection of the Polyline on the specified plane in the specified direction.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <param name="direction">The projection direction (WCS coordinates).</param>
        /// <returns>The projected Polyline.</returns>
        public static Polyline GetProjectedPolyline(this Polyline polyline, Plane plane, Vector3d direction)
        {
            Tolerance tol = new Tolerance(1e-9, 1e-9);
            if (plane.Normal.IsPerpendicularTo(direction, tol))
                return null;

            if (!polyline.Normal.IsPerpendicularTo(direction, tol))
                return GeometryExtension.GetProjectedPolyline(polyline, plane, direction);

            Plane dirPlane = new Plane(Point3d.Origin, direction);
            if (!polyline.IsWriteEnabled) polyline.UpgradeOpen();
            polyline.TransformBy(Matrix3d.WorldToPlane(dirPlane));
            Extents3d extents = polyline.GeometricExtents;
            polyline.TransformBy(Matrix3d.PlaneToWorld(dirPlane));
            return GeometryExtension.GetProjectedExtents(extents, plane, direction, dirPlane);
        }

        /// <summary>
        /// Creates a new Polyline which is the result of the orthogonal projection of the Polyline on the specified plane.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <returns>The projected Polyline.</returns>
        public static Polyline GetOrthoProjectedPolyline(this Polyline polyline, Plane plane) =>
            polyline.GetProjectedPolyline(plane, plane.Normal);

        // /// <summary>
        // /// Defines the way the point is contained.
        // /// </summary>
        // public enum PointContainment
        // {
        //     /// <summary>
        //     /// The point is inside the boundary.
        //     /// </summary>
        //     Inside,
        //
        //     /// <summary>
        //     /// The point is outside the boundary.
        //     /// </summary>
        //     OutSide,
        //
        //     /// <summary>
        //     /// The point is on the boundary.
        //     /// </summary>
        //     OnBoundary
        // }
        //
        // /// <summary>
        // /// Evaluates if the point is inside, outside or on the Polyline using Tolerance.Global.
        // /// </summary>
        // /// <param name="polyline">The instance to which this method applies.</param>
        // /// <param name="point">The point to evaluate.</param>
        // /// <returns>A value of PointContainment.</returns>
        // public static PointContainment GetPointContainment(this Polyline polyline, Point3d point)
        // {
        //     return polyline.GetPointContainment(point, Tolerance.Global.EqualPoint);
        // }
        //
        // /// <summary>
        // /// Evaluates if the point is inside, outside or on the Polyline using the specified Tolerance.
        // /// </summary>
        // /// <param name="polyline">The instance to which this method applies.</param>
        // /// <param name="point">The point to evaluate.</param>
        // /// <param name="tolerance">The tolerance used for comparison.</param>
        // /// <returns>A value of PointContainment.</returns>
        // public static PointContainment GetPointContainment(this Polyline polyline, Point3d point, double tolerance)
        // {
        //     if (polyline == null)
        //         throw new ArgumentNullException(nameof(polyline));
        //
        //     if (!polyline.Closed)
        //         throw new InvalidOperationException("Polyline must be closed");
        //
        //     string filename = "AcMPolygonObj" + Application.Version.Major + ".dbx";
        //     if (!SystemObjects.DynamicLinker.IsModuleLoaded(filename))
        //         SystemObjects.DynamicLinker.LoadModule(filename, false, false);
        //
        //     using (MPolygon mPolygon = new MPolygon())
        //     {
        //         mPolygon.AppendLoopFromBoundary(polyline, false, tolerance);
        //         mPolygon.Elevation = polyline.Elevation;
        //         mPolygon.Normal = polyline.Normal;
        //         for (int i = 0; i < mPolygon.NumMPolygonLoops; i++)
        //         {
        //             if (mPolygon.IsPointOnLoopBoundary(point, i, tolerance))
        //                 return PointContainment.OnBoundary;
        //         }
        //
        //         return mPolygon.IsPointInsideMPolygon(point, tolerance).Count == 1
        //             ? PointContainment.Inside
        //             : PointContainment.OutSide;
        //     }
        // }

        /// <summary>
        /// Applies a scale factor to a bulge value.
        /// </summary>
        /// <param name="bulge">The bulge value.</param>
        /// <param name="factor">The scale factor.</param>
        /// <returns>The scaled bulge value.</returns>
        public static double ScaleBulge(double bulge, double factor)
        {
            return Math.Tan(Math.Atan(bulge) * factor);
        }

        /// <summary>
        /// Converts the Polyline into a Spline.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <returns>The newly created instance of Spline.</returns>
        public static Spline ToSpline(this Polyline polyline)
        {
            using (Polyline2d poly2d = polyline.ConvertTo(false))
            {
                return poly2d.Spline;
            }
        }

        /// <summary>
        /// Evaluates if the points are clockwise.
        /// </summary>
        /// <param name="p1">First point.</param>
        /// <param name="p2">Second point</param>
        /// <param name="p3">Third point</param>
        /// <returns>true, if the points are clockwise; false, otherwise.</returns>
        private static bool IsClockwise(Point2d p1, Point2d p2, Point2d p3)
        {
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X) < 1e-8;
        }
    }
}
