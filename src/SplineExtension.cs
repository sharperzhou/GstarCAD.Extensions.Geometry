#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.Geometry;
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Runtime;
#else
using GrxCAD.Geometry;
using GrxCAD.DatabaseServices;
using GrxCAD.Runtime;
#endif

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
        /// <exception cref="Exception">
        /// eNonPlanarEntity is thrown if the spline is not planar.</exception>
        /// <exception cref="Exception">
        /// eNotApplicable is thrown if the spline is not closed.</exception>
        public static Point3d GetCentroid(this Spline spline)
        {
            if (!spline.IsPlanar)
                throw new Exception(ErrorStatus.NonPlanarEntity);
            if (spline.Closed != true)
                throw new Exception(ErrorStatus.NotApplicable);
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
