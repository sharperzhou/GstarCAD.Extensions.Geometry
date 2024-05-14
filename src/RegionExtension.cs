﻿using System.Collections.Generic;
using System.Linq;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.Geometry;
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.BoundaryRepresentation;
#else
using GrxCAD.Geometry;
using GrxCAD.DatabaseServices;
#endif

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Provides extension methods for the Region type.
    /// </summary>
    public static class RegionExtension
    {
        /// <summary>
        /// Gets the centroid of the region.
        /// </summary>
        /// <param name="region">The instance to which this method applies.</param>
        /// <returns>The centroid of the region (WCS coordinates).</returns>
        public static Point3d GetCentroid(this Region region)
        {
            using (Solid3d sol = new Solid3d())
            {
                sol.Extrude(region, 2.0, 0.0);
                return sol.MassProperties.Centroid - region.Normal;
            }
        }

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
        /// <summary>
        /// Gets the curves constituting the boundaries of the region.
        /// </summary>
        /// <param name="region">The region this method applies to.</param>
        /// <returns>Curve collection.</returns>
        public static IEnumerable<Curve> GetCurves(this Region region)
        {
            using (var brep = new Brep(region))
            {
                var loops = brep.Faces.SelectMany(face => face.Loops);
                foreach (var loop in loops)
                {
                    var curves3d = loop.Edges.Select(edge => ((ExternalCurve3d)edge.Curve).NativeCurve).ToList();
                    if (1 < curves3d.Count)
                    {
                        if (curves3d.All(curve3d => curve3d is CircularArc3d || curve3d is LineSegment3d))
                        {
                            var polyline =
                                (Polyline)Curve.CreateFromGeCurve(new CompositeCurve3d(curves3d.ToOrderedArray()));
                            polyline.Closed = true;
                            yield return polyline;
                        }
                        else
                        {
                            foreach (Curve3d curve3d in curves3d)
                            {
                                yield return Curve.CreateFromGeCurve(curve3d);
                            }
                        }
                    }
                    else
                    {
                        yield return Curve.CreateFromGeCurve(curves3d.First());
                    }
                }
            }
        }
#endif
    }
}