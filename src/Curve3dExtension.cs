using System;
using System.Collections.Generic;
using System.Linq;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.Geometry;
#else
using GrxCAD.Geometry;
#endif

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Provides extension methods for the Curve3d type.
    /// </summary>
    public static class Curve3dExtension
    {
        /// <summary>
        /// Order the collection by contiguous curves ([n].EndPoint equals to [n+1].StartPoint)
        /// </summary>
        /// <param name="source">Collection this method applies to.</param>
        /// <returns>Ordered array of Curve3d.</returns>
        public static Curve3d[] ToOrderedArray(this IEnumerable<Curve3d> source)
        {
            var list = source.ToList();
            int count = list.Count;
            var array = new Curve3d[count];
            int i = 0;
            array[0] = list[0];
            list.RemoveAt(0);
            while (i < count - 1)
            {
                var pt = array[i++].EndPoint;
                int index;
                if ((index = list.FindIndex(c => c.StartPoint.IsEqualTo(pt))) != -1)
                    array[i] = list[index];
                else if ((index = list.FindIndex(c => c.EndPoint.IsEqualTo(pt))) != -1)
                    array[i] = list[index].GetReverseParameterCurve();
                else
                    throw new ArgumentException("Not contiguous curves.");
                list.RemoveAt(index);
            }

            return array;
        }
    }
}
