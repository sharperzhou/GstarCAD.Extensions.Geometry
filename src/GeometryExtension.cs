using System;
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Enumeration of GstarCAD coordinate systems.
    /// </summary>
    public enum CoordinateSystem
    {
        /// <summary>
        /// World Coordinate System.
        /// </summary>
        Wcs = 0,

        /// <summary>
        /// Current User Coordinate System.
        /// </summary>
        Ucs,

        /// <summary>
        /// Current viewport Display Coordinate System.
        /// </summary>
        Dcs,

        /// <summary>
        /// Paper Space Display Coordinate System.
        /// </summary>
        Psdcs
    }

    /// <summary>
    /// Enumeration of the tangent types.
    /// </summary>
    [Flags]
    public enum TangentType
    {
        /// <summary>
        /// Tangents inside two circles.
        /// </summary>
        Inner = 1,

        /// <summary>
        /// Tangents outside two circles.
        /// </summary>
        Outer = 2
    }

    /// <summary>
    /// Provides internal methods.
    /// </summary>
    internal static class GeometryExtension
    {
        /// <summary>
        /// Creates a new instance of Polyline resulting from the projection on <c>plane</c>. parallel to <c>direction</c>
        /// </summary>
        /// <param name="polyline">Polyline (any type) to project.</param>
        /// <param name="plane">Projection plane..</param>
        /// <param name="direction">Projection direction.</param>
        /// <returns>The projected polyline.</returns>
        internal static Polyline GetProjectedPolyline(Curve polyline, Plane plane, Vector3d direction)
        {
            if (!(polyline is Polyline) && !(polyline is Polyline2d) && !(polyline is Polyline3d))
                return null;
            plane = new Plane(Point3d.Origin.OrthoProject(plane), direction);
            using (DBObjectCollection oldCol = new DBObjectCollection())
            using (DBObjectCollection newCol = new DBObjectCollection())
            {
                polyline.Explode(oldCol);
                foreach (DBObject obj in oldCol)
                {
                    Curve crv = obj as Curve;
                    if (crv != null)
                    {
                        Curve flat = crv.GetProjectedCurve(plane, direction);
                        newCol.Add(flat);
                    }

                    obj.Dispose();
                }

                PolylineSegmentCollection psc = new PolylineSegmentCollection();
                for (int i = 0; i < newCol.Count; i++)
                {
                    if (newCol[i] is Ellipse)
                    {
                        psc.AddRange(new PolylineSegmentCollection((Ellipse)newCol[i]));
                        continue;
                    }

                    Curve crv = (Curve)newCol[i];
                    Point3d start = crv.StartPoint;
                    Point3d end = crv.EndPoint;
                    double bulge = 0.0;
                    if (crv is Arc arc)
                    {
                        double angle = arc.Center.GetVectorTo(start)
                            .GetAngleTo(arc.Center.GetVectorTo(end), arc.Normal);
                        bulge = Math.Tan(angle / 4.0);
                    }

                    psc.Add(new PolylineSegment(start.Convert2d(plane), end.Convert2d(plane), bulge));
                }

                foreach (DBObject o in newCol)
                    o.Dispose();
                Polyline projectedPolyline = psc.Join(new Tolerance(1e-9, 1e-9))[0].ToPolyline();
                projectedPolyline.Normal = direction;
                projectedPolyline.Elevation =
                    plane.PointOnPlane.TransformBy(Matrix3d.WorldToPlane(new Plane(Point3d.Origin, direction))).Z;
                if (polyline.StartPoint.Project(plane, direction)
                    .IsEqualTo(projectedPolyline.StartPoint, new Tolerance(1e-9, 1e-9)))
                    return projectedPolyline;

                projectedPolyline.Normal = direction = direction.Negate();
                projectedPolyline.Elevation =
                    plane.PointOnPlane.TransformBy(Matrix3d.WorldToPlane(new Plane(Point3d.Origin, direction))).Z;
                return projectedPolyline;
            }
        }

        /// <summary>
        /// Creates a new instance of Polyline resulting from the projection of <c>extents</c> MinPoint and MaxPoint.
        /// </summary>
        /// <param name="extents">Extents3d of the transformed polyline from WCS plane to <c>dirplane</c>.</param>
        /// <param name="plane">Projection plane.</param>
        /// <param name="direction">Projection direction.</param>
        /// <param name="dirPlane">Plane which origin is 0, 0, 0 and normal as the polyline.</param>
        /// <returns>The newly created Polyline.</returns>
        internal static Polyline GetProjectedExtents(Extents3d extents, Plane plane, Vector3d direction, Plane dirPlane)
        {
            Point3d pt1 = extents.MinPoint.TransformBy(Matrix3d.PlaneToWorld(dirPlane));
            Point3d pt2 = extents.MaxPoint.TransformBy(Matrix3d.PlaneToWorld(dirPlane));
            Polyline projectedPolyline = new Polyline(2);
            projectedPolyline.AddVertexAt(0, pt1.Project(plane, direction).ToPoint2d(), 0.0, 0.0, 0.0);
            projectedPolyline.AddVertexAt(1, pt2.Project(plane, direction).ToPoint2d(), 0.0, 0.0, 0.0);
            return projectedPolyline;
        }
    }
}
