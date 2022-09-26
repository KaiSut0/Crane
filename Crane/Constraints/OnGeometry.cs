using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public abstract class OnGeometry : Constraint
    {
        public OnGeometry(CMesh cMesh, int[] anchorVertexIDs, double strength)
        {
            this.anchorVertexIDs = anchorVertexIDs;
            this.strength = strength;
            edgeAverageLength = Math.Sqrt(cMesh.EdgeLengthSquared.Average());
            isDist = false;
        }

        private readonly int[] anchorVertexIDs;
        private readonly double edgeAverageLength = 1.0;
        private readonly double strength = 1.0;
        private readonly bool isDist = false;
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            if (isDist)
            {
                Point3d[] verts = cMesh.Mesh.Vertices.ToPoint3dArray();

                List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();
                int rows = anchorVertexIDs.Length;
                int cols = 3 * verts.Length;

                for (int i = 0; i < anchorVertexIDs.Length; i++)
                {
                    int id = anchorVertexIDs[i];
                    Point3d ptOnGeometry = ClosestPoint(verts[id]);

                    for (int j = 0; j < 3; j++)
                    {
                        double var = strength * (verts[id][j] - ptOnGeometry[j]) / (edgeAverageLength * edgeAverageLength);
                        int rID = i;
                        int cID = 3 * id + j;
                        elements.Add(new Tuple<int, int, double>(rID, cID, var));
                    }
                }

                return new SparseMatrixBuilder(rows, cols, elements);
            }
            else
            {
                Point3d[] verts = cMesh.Mesh.Vertices.ToPoint3dArray();

                List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();
                int rows = 3 * anchorVertexIDs.Length;
                int cols = 3 * verts.Length;

                for (int i = 0; i < anchorVertexIDs.Length; i++)
                {
                    //double param;
                    int id = anchorVertexIDs[i];
                    Point3d pt = verts[id];
                    //Point3d ptOnGeometry = ClosestPoint(verts[id]);

                    //Vector3d vec = pt - ptOnGeometry;
                    Matrix<double> nn = Derivative(pt);

                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            elements.Add(new Tuple<int, int, double>(3 * i + j, 3 * id + k, strength * nn[j, k] / edgeAverageLength));
                        }
                    }
                }

                return new SparseMatrixBuilder(rows, cols, elements);
            }

        }
        public override double[] Error(CMesh cMesh)
        {
            if (isDist)
            {
                Point3d[] verts = cMesh.Mesh.Vertices.ToPoint3dArray();
                double[] err = new double[anchorVertexIDs.Length];
                for (int i = 0; i < anchorVertexIDs.Length; i++)
                {
                    int id = anchorVertexIDs[i];
                    Point3d pt = ClosestPoint(verts[id]);
                    double dist = pt.DistanceTo(verts[id]);
                    err[i] = 0.5 * strength * dist * dist / (edgeAverageLength * edgeAverageLength);
                }

                return err;
            }
            else
            {
                Point3d[] verts = cMesh.Mesh.Vertices.ToPoint3dArray();
                double[] err = new double[3 * anchorVertexIDs.Length];

                for (int i = 0; i < anchorVertexIDs.Length; i++)
                {
                    int id = anchorVertexIDs[i];
                    Point3d pt = verts[id];
                    Point3d ptOnGeometry = ClosestPoint(verts[id]);

                    for (int j = 0; j < 3; j++)
                    {
                        err[3 * i + j] = strength * (verts[id][j] - ptOnGeometry[j]) / edgeAverageLength;
                    }
                }
                return err;
            }

        }

        protected abstract Point3d ClosestPoint(Point3d pt);
        protected abstract Matrix<double> Derivative(Point3d pt);
    }
}
