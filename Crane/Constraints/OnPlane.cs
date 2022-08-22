using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public sealed class OnPlane : OnGeometry
    {
        public OnPlane(CMesh cMesh, Plane goalPlane, int[] anchorVertexIDs, double strength) : base(cMesh, anchorVertexIDs, strength)
        {
            this.goalPlane = goalPlane;
        }
        private readonly Plane goalPlane;

        protected override Point3d ClosestPoint(Point3d pt)
        {
            return goalPlane.ClosestPoint(pt);
        }

        protected override Matrix<double> Derivative(Point3d pt)
        {
            Vector3d n = goalPlane.Normal;
            Vector<double> nv = Vector<double>.Build.Dense(3);
            for (int k = 0; k < 3; k++) nv[k] = n[k];
            return nv.OuterProduct(nv);
        }

    }
}

