using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

using Rhino.Geometry;
using Crane.Core;

namespace Crane.Constraints
{
    public class Anchor : Constraint
    {
        public Anchor(int anchorIndex, double strength) 
        {
            this.AnchorIndex = anchorIndex;
            this.Strength = strength;
        }
        public int AnchorIndex;
        public double Strength;
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            throw new NotImplementedException();
        }
        public override double[] Error(CMesh cMesh)
        {
            double[] error_ = new double[1];
            CMesh cm = cMesh;
            Mesh m = cMesh.Mesh;
            List<Point3d> verts = new List<Point3d>(m.Vertices.ToPoint3dArray());
            //List<Point3d> ancherVertices = new List<Point3d> 

            //double dist = verts[AnchorIndex].DistanceToSquared(anchors_position[i]);
            //error_[i] = (strength * dist / (2 * edge_avarage_length * edge_avarage_length));

            throw new NotImplementedException();
        }
    }
}

