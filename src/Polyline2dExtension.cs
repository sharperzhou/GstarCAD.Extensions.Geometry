using System;
using System.Collections.Generic;
using System.Linq;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.Geometry;
using Gssoft.Gscad.DatabaseServices;
using AcRx = Gssoft.Gscad.Runtime;
#else
using GrxCAD.Geometry;
using GrxCAD.DatabaseServices;
using AcRx = GrxCAD.Runtime;
#endif

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Provides extension methods for the Polyline2d type.
    /// </summary>
    public static class Polyline2dExtension
    {
        /// <summary>
        /// Gets the list of vertices.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <returns>The list of vertices.</returns>
        /// <exception cref="AcRx.Exception">
        /// eNoActiveTransactions is thrown if the method is called outside of a Transaction.</exception>
        public static List<Vertex2d> GetVertices(this Polyline2d polyline)
        {
            Transaction tr = polyline.Database.TransactionManager.TopTransaction;
            if (tr == null)
                throw new AcRx.Exception(AcRx.ErrorStatus.NoActiveTransactions);

            return polyline.Cast<ObjectId>()
                .Select(id => (Vertex2d)tr.GetObject(id, OpenMode.ForRead))
                .Where(vx => vx.VertexType != Vertex2dType.SplineControlVertex).ToList();
        }

        /// <summary>
        /// Gets the linear 3D segment at specified index.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>An instance of LineSegment3d (WCS coordinates).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException if the index id out of the indices range.</exception>
        public static LineSegment3d GetLineSegment3dAt(this Polyline2d polyline, int index)
        {
            try
            {
                return new LineSegment3d(
                    polyline.GetPointAtParameter(index),
                    polyline.GetPointAtParameter(index + 1));
            }
            catch
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Gets the linear 2D segment at specified index.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>An instance of LineSegment2d (OCS coordinates).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException if the index id out of the indices range.</exception>
        public static LineSegment2d GetLineSegment2dAt(this Polyline2d polyline, int index)
        {
            try
            {
                Matrix3d wcs2Ecs = polyline.Ecs.Inverse();
                return new LineSegment2d(
                    polyline.GetPointAtParameter(index).TransformBy(wcs2Ecs).ToPoint2d(),
                    polyline.GetPointAtParameter(index + 1.0).TransformBy(wcs2Ecs).ToPoint2d());
            }
            catch
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Gets the circular arc 3D segment at specified index.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>An instance of CircularArc3d (WCS coordinates).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException if the index id out of the indices range.</exception>
        public static CircularArc3d GetArcSegment3dAt(this Polyline2d polyline, int index)
        {
            try
            {
                return new CircularArc3d(
                    polyline.GetPointAtParameter(index),
                    polyline.GetPointAtParameter(index + 0.5),
                    polyline.GetPointAtParameter(index + 1));
            }
            catch
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Gets the circular arc 2D segment at specified index.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>An instance of CircularArc2d (OCS coordinates).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException if the index id out of the indices range.</exception>
        public static CircularArc2d GetArcSegment2dAt(this Polyline2d polyline, int index)
        {
            try
            {
                Matrix3d wcs2Ecs = polyline.Ecs.Inverse();
                return new CircularArc2d(
                    polyline.GetPointAtParameter(index).TransformBy(wcs2Ecs).ToPoint2d(),
                    polyline.GetPointAtParameter(index + 0.5).TransformBy(wcs2Ecs).ToPoint2d(),
                    polyline.GetPointAtParameter(index + 1.0).TransformBy(wcs2Ecs).ToPoint2d());
            }
            catch
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Gets the centroid.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <returns>The centroid (WCS coordinates).</returns>
        public static Point3d GetCentroid(this Polyline2d polyline)
        {
            Vertex2d[] vertices = polyline.GetVertices().ToArray();
            int last = vertices.Length - 1;
            Vertex2d vertex = vertices[0];
            Point2d p0 = vertex.Position.ToPoint2d();
            Point2d center = new Point2d(0.0, 0.0);
            double area = 0.0;
            double bulge = vertex.Bulge;
            double tmpArea;
            Point2d tmpPt;
            CircularArc2d arc;
            if (bulge != 0.0)
            {
                arc = polyline.GetArcSegment2dAt(0);
                tmpArea = arc.GetSignedArea();
                tmpPt = arc.GetCentroid();
                area += tmpArea;
                center += (new Point2d(tmpPt.X, tmpPt.Y) * tmpArea).GetAsVector();
            }

            for (int i = 1; i < last; i++)
            {
                Point2d p1 = vertices[i].Position.ToPoint2d();
                Point2d p2 = vertices[i + 1].Position.ToPoint2d();
                Triangle2d triangle = new Triangle2d(p0, p1, p2);
                tmpArea = triangle.SignedArea;
                area += tmpArea;
                center += (triangle.Centroid * tmpArea).GetAsVector();
                bulge = vertices[i].Bulge;
                if (bulge == 0.0)
                    continue;

                arc = polyline.GetArcSegment2dAt(i);
                tmpArea = arc.GetSignedArea();
                tmpPt = arc.GetCentroid();
                area += tmpArea;
                center += (new Point2d(tmpPt.X, tmpPt.Y) * tmpArea).GetAsVector();
            }

            bulge = vertices[last].Bulge;
            if (bulge != 0.0 && polyline.Closed)
            {
                arc = polyline.GetArcSegment2dAt(last);
                tmpArea = arc.GetSignedArea();
                tmpPt = arc.GetCentroid();
                area += tmpArea;
                center += (new Point2d(tmpPt.X, tmpPt.Y) * tmpArea).GetAsVector();
            }

            center = center.DivideBy(area);
            return new Point3d(center.X, center.Y, polyline.Elevation).TransformBy(
                Matrix3d.PlaneToWorld(polyline.Normal));
        }

        /// <summary>
        /// Creates a new Polyline which is the result of the projection of the Polyline2d on the specified plane in the specified direction.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <param name="direction">The projection direction (WCS coordinates).</param>
        /// <returns>The projected Polyline.</returns>
        public static Polyline GetProjectedPolyline(this Polyline2d polyline, Plane plane, Vector3d direction)
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
        /// Creates a new Polyline which is the result of the orthogonal projection of the Polyline2d on the specified plane.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <returns>The projected Polyline.</returns>
        public static Polyline GetOrthoProjectedPolyline(this Polyline2d polyline, Plane plane) =>
            polyline.GetProjectedPolyline(plane, plane.Normal);
    }
}
