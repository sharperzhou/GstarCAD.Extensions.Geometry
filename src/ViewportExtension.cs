using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Provides extension methods for the Viewport type.
    /// </summary>
    public static class ViewportExtension
    {
        /// <summary>
        /// Gets the transformation matrix of the display coordinate system (DCS)
        /// of the specified window to the world coordinate system (WCS).
        /// </summary>
        /// <param name="viewport">The instance to which this method applies.</param>
        /// <returns>The transformation matrix from DCS to WCS.</returns>
        public static Matrix3d DcsToWcs(this Viewport viewport)
        {
            return
                Matrix3d.Rotation(-viewport.TwistAngle, viewport.ViewDirection, viewport.ViewTarget) *
                Matrix3d.Displacement(viewport.ViewTarget.GetAsVector()) *
                Matrix3d.PlaneToWorld(viewport.ViewDirection);
        }

        /// <summary>
        /// Gets the transformation matrix of the world coordinate system (WCS)
        /// to the display coordinate system (DCS) of the specified window.
        /// </summary>
        /// <param name="viewport">The instance to which this method applies.</param>
        /// <returns>The transformation matrix from WCS to DCS.</returns>
        public static Matrix3d WcsToDcs(this Viewport viewport)
        {
            return
                Matrix3d.WorldToPlane(viewport.ViewDirection) *
                Matrix3d.Displacement(viewport.ViewTarget.GetAsVector().Negate()) *
                Matrix3d.Rotation(viewport.TwistAngle, viewport.ViewDirection, viewport.ViewTarget);
        }

        /// <summary>
        /// Gets the transformation matrix of the display coordinate system of the specified paper space window (DCS)
        /// to the paper space display coordinate system (PSDCS).
        /// </summary>
        /// <param name="viewport">The instance to which this method applies.</param>
        /// <returns>The transformation matrix from DCS to PSDCS.</returns>
        public static Matrix3d DcsToPsdcs(this Viewport viewport)
        {
            return
                Matrix3d.Scaling(viewport.CustomScale, viewport.CenterPoint) *
                Matrix3d.Displacement(viewport.ViewCenter.ToPoint3d().GetVectorTo(viewport.CenterPoint));
        }

        /// <summary>
        /// Gets the transformation matrix of the paper space display coordinate system (PSDCS)
        /// to the display coordinate system of the specified paper space window (DCS).
        /// </summary>
        /// <param name="viewport">The instance to which this method applies.</param>
        /// <returns>The transformation matrix from PSDCS to DCS.</returns>
        public static Matrix3d PsdcsToDcs(this Viewport viewport)
        {
            return
                Matrix3d.Displacement(viewport.CenterPoint.GetVectorTo(viewport.ViewCenter.ToPoint3d())) *
                Matrix3d.Scaling(1.0 / viewport.CustomScale, viewport.CenterPoint);
        }
    }
}
