using static System.Math;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.Geometry;
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.ApplicationServices.Core;
using Gssoft.Gscad.EditorInput;
using Gssoft.Gscad.Runtime;
#else
using GrxCAD.Geometry;
using GrxCAD.DatabaseServices;
using GrxCAD.ApplicationServices;
using GrxCAD.EditorInput;
using GrxCAD.Runtime;
#endif


namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Provides extension methods for the Point3d type.
    /// </summary>
    public static class Point3dExtension
    {
        /// <summary>
        /// Converts a Point3d into a Point2d (projection on XY plane).
        /// </summary>
        /// <param name="point">The instance to which this method applies.</param>
        /// <returns>The corresponding Point3d.</returns>
        public static Point2d ToPoint2d(this Point3d point)
        {
            return new Point2d(point.X, point.Y);
        }

        /// <summary>
        /// Projects the point on the WCS XY plane.
        /// </summary>
        /// <param name="point">The point to be projected.</param>
        /// <returns>The projected point.</returns>
        public static Point3d Flatten(this Point3d point)
        {
            return new Point3d(point.X, point.Y, 0.0);
        }

        /// <summary>
        /// Gets a value indicating if <c>point</c> lies on the segment <c>p1</c> <c>p2</c> using Tolerance.Global.
        /// </summary>
        /// <param name="point">The instance to which this method applies.</param>
        /// <param name="p1">The start point of the segment.</param>
        /// <param name="p2">The end point of the segment.</param>
        /// <returns>true, if the point lies on the segment; false, otherwise.</returns>
        public static bool IsBetween(this Point3d point, Point3d p1, Point3d p2)
        {
            return p1.GetVectorTo(point).GetNormal().Equals(point.GetVectorTo(p2).GetNormal());
        }

        /// <summary>
        /// Gets a value indicating if <c>point</c> lies on the segment <c>p1</c> <c>p2</c> using the specified Tolerance.
        /// </summary>
        /// <param name="point">The instance to which this method applies.</param>
        /// <param name="p1">The start point of the segment.</param>
        /// <param name="p2">The end point of the segment.</param>
        /// <param name="tolerance">The tolerance used for comparisons.</param>
        /// <returns>true, if the point lies on the segment; false, otherwise.</returns>
        public static bool IsBetween(this Point3d point, Point3d p1, Point3d p2, Tolerance tolerance)
        {
            return p1.GetVectorTo(point).GetNormal(tolerance).Equals(point.GetVectorTo(p2).GetNormal(tolerance));
        }

        /// <summary>
        /// Get a value indicating if the specified point is inside the extents.
        /// </summary>
        /// <param name="point">The instance to which this method applies.</param>
        /// <param name="extents">The extents 2d to test against.</param>
        /// <returns>true, if the point us inside the extents; false, otherwise.</returns>
        public static bool IsInside(this Point3d point, Extents3d extents)
        {
            return
                point.X >= extents.MinPoint.X &&
                point.Y >= extents.MinPoint.Y &&
                point.Z >= extents.MinPoint.Z &&
                point.X <= extents.MaxPoint.X &&
                point.Y <= extents.MaxPoint.Y &&
                point.Z <= extents.MaxPoint.Z;
        }

        /// <summary>
        /// Defines a point with polar coordinates relative to a base point.
        /// </summary>
        /// <param name="point">The instance to which this method applies.</param>
        /// <param name="angle">The angle in radians from the X axis.</param>
        /// <param name="distance">The distance from the base point.</param>
        /// <returns>The new point3d.</returns>
        public static Point3d ToPolar(this Point3d point, double angle, double distance)
        {
            return new Point3d(
                point.X + distance * Cos(angle),
                point.Y + distance * Sin(angle),
                point.Z);
        }

        /// <summary>
        /// Converts a point from a coordinate system to another one.
        /// </summary>
        /// <param name="point">The instance to which this method applies.</param>
        /// <param name="from">The origin coordinate system.</param>
        /// <param name="to">The destination coordinate system.</param>
        /// <returns>The corresponding Point3d.</returns>
        /// <exception cref="Exception">
        /// eInvalidInput thrown of 3 (CoordinateSystem.PSDCS) is used with another flag than 2 (CoordinateSystem.DCS).
        /// </exception>
        public static Point3d Transform(this Point3d point, CoordinateSystem from, CoordinateSystem to)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            return point.Transform(ed, from, to);
        }

        /// <summary>
        /// Converts a point from a coordinate system to another one.
        /// </summary>
        /// <param name="point">The instance to which this method applies.</param>
        /// <param name="editor">Current instance of Editor.</param>
        /// <param name="from">The origin coordinate system.</param>
        /// <param name="to">The destination coordinate system.</param>
        /// <returns>The corresponding Point3d.</returns>
        /// <exception cref="Exception">
        /// eInvalidInput thrown of 3 (CoordinateSystem.PSDCS) is used with another flag than 2 (CoordinateSystem.DCS).
        /// </exception>
        public static Point3d Transform(this Point3d point, Editor editor, CoordinateSystem from, CoordinateSystem to)
        {
            Matrix3d mat = new Matrix3d();
            switch (from)
            {
                case CoordinateSystem.Wcs:
                    switch (to)
                    {
                        case CoordinateSystem.Ucs:
                            mat = editor.WcsToUcs();
                            break;
                        case CoordinateSystem.Dcs:
                            mat = editor.WcsToDcs();
                            break;
                        case CoordinateSystem.Psdcs:
                            throw new Exception(ErrorStatus.InvalidInput, "To be used only with DCS");
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }

                    break;
                case CoordinateSystem.Ucs:
                    switch (to)
                    {
                        case CoordinateSystem.Wcs:
                            mat = editor.UcsToWcs();
                            break;
                        case CoordinateSystem.Dcs:
                            mat = editor.UcsToWcs() * editor.WcsToDcs();
                            break;
                        case CoordinateSystem.Psdcs:
                            throw new Exception(ErrorStatus.InvalidInput, "To be used only with DCS");
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }

                    break;
                case CoordinateSystem.Dcs:
                    switch (to)
                    {
                        case CoordinateSystem.Wcs:
                            mat = editor.DcsToWcs();
                            break;
                        case CoordinateSystem.Ucs:
                            mat = editor.DcsToWcs() * editor.WcsToUcs();
                            break;
                        case CoordinateSystem.Psdcs:
                            mat = editor.DcsToPsdcs();
                            break;
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }

                    break;
                case CoordinateSystem.Psdcs:
                    switch (to)
                    {
                        case CoordinateSystem.Wcs:
                            throw new Exception(ErrorStatus.InvalidInput, "To be used only with DCS");
                        case CoordinateSystem.Ucs:
                            throw new Exception(ErrorStatus.InvalidInput, "To be used only with DCS");
                        case CoordinateSystem.Dcs:
                            mat = editor.PsdcsToDcs();
                            break;
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }

                    break;
            }

            return point.TransformBy(mat);
        }
    }
}
