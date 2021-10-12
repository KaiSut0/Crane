using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public sealed class OnPoint : OnGeometry
    {
        public OnPoint(CMesh cMesh, Point3d goalPoint, int[] anchorVertexIDs, double strength) : base(cMesh, anchorVertexIDs, strength)
        {
            this.goalPoint = goalPoint;
        }

        private readonly Point3d goalPoint;
        protected override Point3d ClosestPoint(Point3d pt)
        {
            return goalPoint;
        }
    }
}
