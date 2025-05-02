using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Crane.Patterns.CreasePattern
{
    public class CreasePattern
    {
        public CreasePattern() { }

        protected CreasePatternParam param;

        protected List<Point3d> pointList;
        protected List<int[]> unitFaces;

        public int XCount { get; set; }

        public int YCount { get; set; }

        public int UnitPointCount { get; set; }

        public Mesh Tessellation { get; private set; }
        public List<Line> Mountains { get; set; }
        public List<Line> Valleyes { get; set; }



    }
}
