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
            int I = Utils.GetPointID(cMesh.Mesh, ptI);
            int J = Utils.GetPointID(cMesh.Mesh, ptJ);
            indexPair = new IndexPair(I, J);
        }
        public GlueVertices(CMesh cMesh, int I, int J)
        {
            edgeAverageLength = Math.Sqrt(cMesh.EdgeLengthSquared.Average());
            indexPair = new IndexPair(I, J);

        }

        private IndexPair indexPair;
        private double edgeAverageLength;
        public override Matrix<double> Jacobian(CMesh cMesh)
        {
            int rows = 3;
            int cols = cMesh.DOF;
            var elements = new List<Tuple<int, int, double>>();

            var verts = cMesh.Mesh.Vertices.ToPoint3dArray();
            var ptI = verts[indexPair.I];
            var ptJ = verts[indexPair.J];

            for(int i = 0; i < 3; i++)
            {
                elements.Add(new Tuple<int, int, double>(i, 3 * indexPair.I + i, -1.0 / edgeAverageLength));
                elements.Add(new Tuple<int, int, double>(i, 3 * indexPair.J + i,  1.0 / edgeAverageLength));
            }
            //for(int j = 0; j < 3; j++)
            //{
            //    elements.Add(new Tuple<int, int, double>(0, 3 * indexPair.I + j, (ptI - ptJ)[j] / edgeAverageLength));
            //    elements.Add(new Tuple<int, int, double>(0, 3 * indexPair.J + j, (ptJ - ptI)[j] / edgeAverageLength));
            //}
            return Matrix<double>.Build.SparseOfIndexed(rows, cols, elements);

        }
        public override Vector<double> Error(CMesh cMesh)
        {
            var verts = cMesh.Mesh.Vertices.ToPoint3dArray();
            var ptI = verts[indexPair.I];
            var ptJ = verts[indexPair.J];
            //var error = 0.5 * ptI.DistanceToSquared(ptJ) / edgeAverageLength;

            var diff = (ptJ - ptI) / edgeAverageLength;
            double[] error = new double[3];

            error[0] = diff[0];
            error[1] = diff[1];
            error[2] = diff[2];

            return Vector<double>.Build.DenseOfArray(error);
        }
    }
}

