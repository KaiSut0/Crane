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
        protected override Matrix<double> Derivative(Point3d pt)
        {
            //Vector3d vec = goalPoint - pt;
            //Vector3d n = vec / vec.Length;
            //Vector<double> nv = Vector<double>.Build.Dense(3);
            //for (int k = 0; k < 3; k++) nv[k] = n[k];
            //return nv.OuterProduct(nv);
            return Matrix<double>.Build.DenseIdentity(3);
        }
    }
}
