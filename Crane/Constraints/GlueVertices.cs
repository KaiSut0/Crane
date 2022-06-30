using System;
using System.Collections.Generic;
using System.Linq;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class GlueVertices : Constraint
    {
        public GlueVertices(CMesh cMesh, Point3d ptI, Point3d ptJ, double tolerance) 
        {
            edgeAverageLength = Math.Sqrt(cMesh.EdgeLengthSquared.Average());
            int I = Util.GetPointID(cMesh.Mesh, ptI);
            int J = Util.GetPointID(cMesh.Mesh, ptJ);
            indexPair = new IndexPair(I, J);
        }
        public GlueVertices(CMesh cMesh, int I, int J)
        {
            edgeAverageLength = Math.Sqrt(cMesh.EdgeLengthSquared.Average());
            indexPair = new IndexPair(I, J);

        }

        private IndexPair indexPair;
        private double edgeAverageLength;
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            int rows = 3;
            int cols = cMesh.DOF;
            var elements = new List<Tuple<int, int, double>>();

            var verts = cMesh.Vertices;
            var ptI = verts[indexPair.I];
            var ptJ = verts[indexPair.J];

            for(int i = 0; i < 3; i++)
            {
                elements.Add(new Tuple<int, int, double>(i, 3 * indexPair.I + i, -1.0 / edgeAverageLength));
                elements.Add(new Tuple<int, int, double>(i, 3 * indexPair.J + i,  1.0 / edgeAverageLength));
            }

            return new SparseMatrixBuilder(rows, cols, elements);

        }
        public override double[] Error(CMesh cMesh)
        {
            var verts = cMesh.Vertices;
            var ptI = verts[indexPair.I];
            var ptJ = verts[indexPair.J];
            //var error = 0.5 * ptI.DistanceToSquared(ptJ) / edgeAverageLength;

            var diff = (ptJ - ptI) / edgeAverageLength;
            double[] error = new double[3];

            error[0] = diff[0];
            error[1] = diff[1];
            error[2] = diff[2];

            return error;
        }
    }
}

