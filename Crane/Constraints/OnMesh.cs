using System;
using System.Collections.Generic;
using System.Linq;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public sealed class OnMesh : OnGeometry
    {
        public OnMesh(CMesh cMesh, Mesh goalMesh, int[] anchorVertexIDs, double strength) :base(cMesh, anchorVertexIDs, strength)
        {
            this.goalMesh = goalMesh;

        }
        private readonly Mesh goalMesh;
        protected override Point3d ClosestPoint(Point3d pt)
        {
            return goalMesh.ClosestPoint(pt);
        }
        protected override Matrix<double> Derivative(Point3d pt)
        {
            Point3d closestPt = new Point3d();
            Vector3d normal = new Vector3d();
            goalMesh.ClosestPoint(pt, out closestPt, out normal, 1e+5);
            Vector3d n = normal;
            Vector<double> nv = Vector<double>.Build.Dense(3);
            for (int k = 0; k < 3; k++) nv[k] = n[k];
            return nv.OuterProduct(nv);
        }
    }
}
