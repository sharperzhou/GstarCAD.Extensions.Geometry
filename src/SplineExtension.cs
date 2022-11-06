using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using AcRx = GrxCAD.Runtime;

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Provides extension methods for the Spline type.
    /// </summary>
    public static class SplineExtension
    {
        /// <summary>
        /// Gets the centroid of a closed planar spline.
        /// </summary>
        /// <param name="spline">The instance to which this method applies.</param>
        /// <returns>The centroid of the spline (WCS coordinates).</returns>
        /// <exception cref="GrxCAD.Runtime.Exception">
        /// eNonPlanarEntity is thrown if the spline is not planar.</exception>
        /// <exception cref="GrxCAD.Runtime.Exception">
        /// eNotApplicable is thrown if the spline is not closed.</exception>
        public static Point3d GetCentroid(this Spline spline)
        {
            if (!spline.IsPlanar)
                throw new AcRx.Exception(AcRx.ErrorStatus.NonPlanarEntity);
            if (spline.Closed != true)
                throw new AcRx.Exception(AcRx.ErrorStatus.NotApplicable);
            using (DBObjectCollection curves = new DBObjectCollection())
            {
                curves.Add(spline);
                using (DBObjectCollection regions = Region.CreateFromCurves(curves))
                {
                    return ((Region)regions[0]).GetCentroid();
                }
            }
        }
    }
}
