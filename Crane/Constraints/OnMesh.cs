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
    }
}
