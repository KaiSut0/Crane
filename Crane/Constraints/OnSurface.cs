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

    }
}

