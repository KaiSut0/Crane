using System;
using System.Collections.Generic;
using System.Linq;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public sealed class OnCurve : OnGeometry
    {
        public OnCurve(CMesh cMesh, Curve goalCurve, int[] anchorVertexIDs, double strength) : base(cMesh, anchorVertexIDs, strength)
        {
            this.goalCurve = goalCurve;
        }
        private readonly Curve goalCurve;

        protected override Point3d ClosestPoint(Point3d pt)
        {
            double t;
            goalCurve.ClosestPoint(pt, out t);
            return goalCurve.PointAt(t);
        }

        protected override Matrix<double> Derivative(Point3d pt)
        {
            Point3d closestPt = new Point3d();
            double t = 0;
            goalCurve.ClosestPoint(pt, out t);
            Vector3d n = pt - goalCurve.PointAt(t);
            n.Unitize();
            Vector<double> nv = Vector<double>.Build.Dense(3);
            for (int k = 0; k < 3; k++) nv[k] = n[k];
            return nv.OuterProduct(nv);
        }
    }
}

