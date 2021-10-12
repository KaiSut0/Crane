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
    }
}

