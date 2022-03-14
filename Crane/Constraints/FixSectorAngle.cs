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
    class FixSectorAngle : Constraint
    {
        public FixSectorAngle(CMesh cMesh, Point3d[] centerPts, Point3d[] leftPts, Point3d[] rightPts, double[] setAngles,
            double[] stiffness)
        {
            centerPtIds = new int[centerPts.Length];
            leftPtIds = new int[leftPts.Length];
            rightPtIds = new int[rightPts.Length];
            faceIds = new int[centerPts.Length];
            this.stiffness = stiffness;
            this.setAngles = setAngles;
            for (int i = 0; i < centerPts.Length; i++)
            {
                centerPtIds[i] = cMesh.GetVertexId(centerPts[i]);
                leftPtIds[i] = cMesh.GetVertexId(leftPts[i]);
                rightPtIds[i] = cMesh.GetVertexId(rightPts[i]);
                faceIds[i] = cMesh.GetFaceIdFrom3PtIds(centerPtIds[i], leftPtIds[i], rightPtIds[i]);
            }
        }

        private readonly int[] centerPtIds;
        private readonly int[] leftPtIds;
        private readonly int[] rightPtIds;
        private readonly int[] faceIds;
        private readonly double[] setAngles;
        private readonly double[] stiffness;

        public override Matrix<double> Jacobian(CMesh cMesh)
        {
            int rows = centerPtIds.Length;
            int columns = cMesh.DOF;
            Mesh mesh = cMesh.Mesh;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            var verts = mesh.Vertices;
            var normals = mesh.FaceNormals;

            for (int i = 0; i < rows; i++)
            {
                var center = mesh.Vertices[centerPtIds[i]];
                var left = mesh.Vertices[leftPtIds[i]];
                var right = mesh.Vertices[rightPtIds[i]];
                double sectorAngle = Utils.ComputeAngleFrom3Pts(left, center, right);
                var n = normals[faceIds[i]];
                double k = Math.Sqrt(stiffness[i]);

                var p12 = left - center;
                var p32 = right - center;
                var p12SqDist = p12.SquareLength;
                var p32SqDist = p32.SquareLength;
                var np12 = Vector3d.CrossProduct(n, p12);
                var np32 = Vector3d.CrossProduct(n, p32);

                var dp1da = np12 / p12SqDist;
                var dp2da = -np12 / p12SqDist + np32 / p32SqDist;
                var dp3da = -np32 / p32SqDist;

                for (int j = 0; j < 3; j++)
                {
                    double val = k * dp1da[j];
                    int rind = i;
                    int cind = 3 * leftPtIds[i] + j;
                    var element = Tuple.Create(rind, cind, val);
                    elements.Add(element);
                }
                for (int j = 0; j < 3; j++)
                {
                    double val = k * dp2da[j];
                    int rind = i;
                    int cind = 3 * centerPtIds[i] + j;
                    var element = Tuple.Create(rind, cind, val);
                    elements.Add(element);
                }
                for (int j = 0; j < 3; j++)
                {
                    double val = k * dp3da[j];
                    int rind = i;
                    int cind = 3 * rightPtIds[i] + j;
                    var element = Tuple.Create(rind, cind, val);
                    elements.Add(element);
                }

            }
            return Matrix<double>.Build.SparseOfIndexed(rows, columns, elements);
        }

        public override Vector<double> Error(CMesh cMesh)
        {
            int rows = centerPtIds.Length;
            int columns = cMesh.DOF;
            Mesh mesh = cMesh.Mesh;

            var err = new double[rows];

            for (int i = 0; i < rows; i++)
            {
                var center = mesh.Vertices[centerPtIds[i]];
                var left = mesh.Vertices[leftPtIds[i]];
                var right = mesh.Vertices[rightPtIds[i]];
                double sectorAngle = Utils.ComputeAngleFrom3Pts(left, center, right);
                double setAngle = setAngles[i];
                double k = Math.Sqrt(stiffness[i]);

                err[i] = k * (sectorAngle - setAngle);

            }


            return Vector<double>.Build.DenseOfArray(err);
        }
    }
}
