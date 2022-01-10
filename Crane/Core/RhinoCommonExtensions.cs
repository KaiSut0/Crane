using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhino.Geometry
{
    static class Extensions
    {
        public static double[] ToArray(this Point3f pt)
        {
            return new double[] { pt.X, pt.Y, pt.Z };
        }
        public static double[] ToArray(this Point3d pt)
        {
            return new double[] { pt.X, pt.Y, pt.Z };
        }

    }
}
