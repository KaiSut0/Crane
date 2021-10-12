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
        }

        private readonly int[] anchorVertexIDs;
        private readonly double edgeAverageLength = 1.0;
        private readonly double strength = 1.0;
        public override Matrix<double> Jacobian(CMesh cMesh)
        {
            Point3d[] verts = cMesh.Mesh.Vertices.ToPoint3dArray();

            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();
            //int rows = 3 * anchorVertexIDs.Length;
            int rows = anchorVertexIDs.Length;
            int cols = 3 * verts.Length;

            for (int i = 0; i < anchorVertexIDs.Length; i++)
            {
                //double param;
                int id = anchorVertexIDs[i];
                Point3d ptOnGeometry = ClosestPoint(verts[id]);

                for (int j = 0; j < 3; j++)
                {
                    //elements.Add(new Tuple<int, int, double>(3 * i + j, 3 * id + j, strength / edgeAverageLength));
                    double var = strength * (verts[id][j] - ptOnGeometry[j]) / (edgeAverageLength * edgeAverageLength);
                    int rID = i;
                    int cID = 3 * id + j;
                    elements.Add(new Tuple<int, int, double>(rID, cID, var));
                }
            }

            return Matrix<double>.Build.SparseOfIndexed(rows, cols, elements);
        }
        public override Vector<double> Error(CMesh cMesh)
        {
            Point3d[] verts = cMesh.Mesh.Vertices.ToPoint3dArray();
            double[] err = new double[anchorVertexIDs.Length];
            //double[] err = new double[3 * anchorVertexIDs.Length];

            for (int i = 0; i < anchorVertexIDs.Length; i++)
            {
                int id = anchorVertexIDs[i];
                Point3d pt = ClosestPoint(verts[id]);

                //for(int j = 0; j < 3; j++)
                //{
                //    err[3 * i + j] = strength * (verts[id][j] - pt[j]) / edgeAverageLength;
                //}

                double dist = pt.DistanceTo(verts[id]);
                err[i] = strength * dist * dist / (2 * edgeAverageLength * edgeAverageLength);
            }

            return Vector<double>.Build.DenseOfArray(err);
        }

        protected abstract Point3d ClosestPoint(Point3d pt);
    }
}
