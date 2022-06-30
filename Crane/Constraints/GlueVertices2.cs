using System;
using System.Collections.Generic;
using System.Linq;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class GlueVertices2 : Constraint
    {
        public GlueVertices2(CMesh cMesh, Point3d ptI, Point3d ptJ, double tolerance) 
        {
            edgeAverageLength = Math.Sqrt(cMesh.EdgeLengthSquared.Average());
            int I = Util.GetPointID(cMesh.Mesh, ptI);
            int J = Util.GetPointID(cMesh.Mesh, ptJ);
            indexPair = new IndexPair(I, J);
        }
        public GlueVertices2(CMesh cMesh, int I, int J)
        {
            edgeAverageLength = Math.Sqrt(cMesh.EdgeLengthSquared.Average());
            indexPair = new IndexPair(I, J);

        }

        private IndexPair indexPair;
        private double edgeAverageLength;
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            int rows = 1;
            int cols = cMesh.DOF;
            var elements = new List<Tuple<int, int, double>>();

            var verts = cMesh.Vertices;
            var ptI = verts[indexPair.I];
            var ptJ = verts[indexPair.J];
            var vec = ptJ - ptI;

            for(int i = 0; i < 3; i++)
            {
                elements.Add(new Tuple<int, int, double>(0, 3 * indexPair.I + i, -vec[i] / edgeAverageLength));
                elements.Add(new Tuple<int, int, double>(0, 3 * indexPair.J + i,  vec[i] / edgeAverageLength));
            }

            return new SparseMatrixBuilder(rows, cols, elements);

        }
        public override double[] Error(CMesh cMesh)
        {
            var verts = cMesh.Vertices;
            var ptI = verts[indexPair.I];
            var ptJ = verts[indexPair.J];
            //var error = 0.5 * ptI.DistanceToSquared(ptJ) / edgeAverageLength;

            var diff = 0.5 * (ptJ - ptI) * (ptJ - ptI) / edgeAverageLength;
            double[] error = new double[1];

            error[0] = diff;
            return error;
        }
    }
}

