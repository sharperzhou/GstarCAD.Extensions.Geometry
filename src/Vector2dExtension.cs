using GrxCAD.Geometry;

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Provides extension methods for the Vector2d type.
    /// </summary>
    public static class Vector2dExtension
    {
        /// <summary>
        /// Converts a Vector2d into a Vector3d with a Z coordinate equal to 0.
        /// </summary>
        /// <param name="vector">The instance to which this method applies.</param>
        /// <returns>The corresponding Vector.</returns>
        public static Vector3d ToVector3d(this Vector2d vector) =>
            new Vector3d(vector.X, vector.Y, 0.0);

        /// <summary>
        /// Converts a Vector2d into a Vector3d according to the specified plane.
        /// </summary>
        /// <param name="vector">The instance to which this method applies.</param>
        /// <param name="plane">The plane the Point2d lies on.</param>
        /// <returns>The corresponding Vector3d.</returns>
        public static Vector3d ToVector3d(this Vector2d vector, Plane plane) =>
            vector.ToVector3d().TransformBy(Matrix3d.PlaneToWorld(plane));
    }
}
