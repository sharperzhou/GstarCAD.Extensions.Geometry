using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Provides extension methods for the Polyline3d type.
    /// </summary>
    public static class Polyline3dExtension
    {
        /// <summary>
        /// Creates a new Polyline which is the result of the projection of the Polyline3d on the specified plane in the specified direction.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <param name="direction">The projection direction (WCS coordinates).</param>
        /// <returns>The projected Polyline.</returns>
        public static Polyline GetProjectedPolyline(this Polyline3d polyline, Plane plane, Vector3d direction)
        {
            return plane.Normal.IsPerpendicularTo(direction, new Tolerance(1e-9, 1e-9))
                ? null
                : GeometryExtension.GetProjectedPolyline(polyline, plane, direction);
        }

        /// <summary>
        /// Creates a new Polyline which is the result of the orthogonal projection of the Polyline3d on the specified plane.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <returns>The projected Polyline.</returns>
        public static Polyline GetOrthoProjectedPolyline(this Polyline3d polyline, Plane plane) =>
            polyline.GetProjectedPolyline(plane, plane.Normal);
    }
}
