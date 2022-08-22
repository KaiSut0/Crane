using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public sealed class OnSurface : OnGeometry
    {
        public OnSurface(CMesh cMesh, Surface goalSurface, int[] anchorVertexIDs, double strength) : base(cMesh, anchorVertexIDs, strength)
        {
            this.goalSurface = goalSurface;

        }

        private readonly Surface goalSurface;
        protected override Point3d ClosestPoint(Point3d pt)
        {
            double u, v;
            goalSurface.ClosestPoint(pt, out u, out v);
            return goalSurface.PointAt(u, v);
        }

        protected override Matrix<double> Derivative(Point3d pt)
        {
            double u, v;
            goalSurface.ClosestPoint(pt, out u, out v);
            Vector3d n = goalSurface.NormalAt(u, v);
            Vector<double> nv = Vector<double>.Build.Dense(3);
            for (int k = 0; k < 3; k++) nv[k] = n[k];
            return nv.OuterProduct(nv);
        }

    }
}

