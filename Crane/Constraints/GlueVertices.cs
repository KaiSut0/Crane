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
            isDist = false;
        }
        public GlueVertices(CMesh cMesh, int I, int J)
        {
            edgeAverageLength = Math.Sqrt(cMesh.EdgeLengthSquared.Average());
            indexPair = new IndexPair(I, J);

        }

        private IndexPair indexPair;
        private double edgeAverageLength;
        private bool isDist = false;
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            if (isDist)
            {
                int rows = 1;
                int cols = cMesh.DOF;
                var elements = new List<Tuple<int, int, double>>();

                var verts = cMesh.Mesh.Vertices.ToPoint3dArray();
                var ptI = verts[indexPair.I];
                var ptJ = verts[indexPair.J];
                var diff = ptI - ptJ;

                for (int i = 0; i < 3; i++)
                {
                    elements.Add(new Tuple<int, int, double>(0, 3 * indexPair.I + i, diff[i] / (edgeAverageLength * edgeAverageLength)));
                    elements.Add(new Tuple<int, int, double>(0, 3 * indexPair.J + i, -diff[i] / (edgeAverageLength * edgeAverageLength)));
                }

                return new SparseMatrixBuilder(rows, cols, elements);
            }
            else
            {
                int rows = 3;
                int cols = cMesh.DOF;
                var elements = new List<Tuple<int, int, double>>();

                var verts = cMesh.Mesh.Vertices.ToPoint3dArray();
                var ptI = verts[indexPair.I];
                var ptJ = verts[indexPair.J];

                for (int i = 0; i < 3; i++)
                {
                    elements.Add(new Tuple<int, int, double>(i, 3 * indexPair.I + i, 1.0 / (edgeAverageLength)));
                    elements.Add(new Tuple<int, int, double>(i, 3 * indexPair.J + i, -1.0 / (edgeAverageLength)));
                }

                return new SparseMatrixBuilder(rows, cols, elements);
            }

        }
        public override double[] Error(CMesh cMesh)
        {
            if (isDist)
            {
                var verts = cMesh.Mesh.Vertices.ToPoint3dArray();
                var ptI = verts[indexPair.I];
                var ptJ = verts[indexPair.J];
                var err = 0.5 * ptI.DistanceToSquared(ptJ) / (edgeAverageLength * edgeAverageLength);

                double[] error = new double[] { err };

                return error;
            }

            else
            {
                var verts = cMesh.Mesh.Vertices.ToPoint3dArray();
                var ptI = verts[indexPair.I];
                var ptJ = verts[indexPair.J];

                var diff = (ptI - ptJ) / edgeAverageLength;
                double[] error = new double[] { diff[0], diff[1], diff[2] };

                return error;
            }


        }
    }
}

