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
    public class AlignNormal : Constraint
    {
        public AlignNormal(CMesh cMesh, List<Point3d> faceCenters, List<Vector3d> goalNormals, List<double> strengths)
        {
            alignFaceIds = new List<int>();
            foreach (var faceCenter in faceCenters)
            {
                alignFaceIds.Add(cMesh.GetFaceIdFromFaceCenter(faceCenter));
            }

            this.goalNormals = goalNormals;
            this.strengths = strengths;
        }

        private List<int> alignFaceIds;
        private List<Vector3d> goalNormals;
        private List<double> strengths;

        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            var verts = cMesh.Mesh.Vertices.ToPoint3dArray();
            List<Tuple<int, int, double>> tups = new List<Tuple<int, int, double>>();
            for (int i = 0; i < alignFaceIds.Count; i++)
            {
                var fid = alignFaceIds[i];
                var fvids = cMesh.Mesh.Faces[i];
                int id0 = fvids.A;
                int id1 = fvids.B;
                int id2 = fvids.C;
                Point3d v0 = verts[id0];
                Point3d v1 = verts[id1];
                Point3d v2 = verts[id2];
                Vector3d v01 = v1 - v0;
                Vector3d v02 = v2 - v0;
                Vector3d goalNormal = goalNormals[i];
                Vector3d a = Vector3d.CrossProduct(v01, v02);
                double l = a.Length;
                double strength = strengths[i];
                var mat1 = l * l * Matrix<double>.Build.DenseIdentity(3);
                var mat2 = Util.OutorProduct(a, a);
                var mat = mat1 - mat2;
                mat /= l * l * l;

                goalNormal = Util.MatrixVector3dMulltiplication(mat, goalNormal);

                Vector3d dcdv0 = Vector3d.CrossProduct(v01 - v02, goalNormal);
                dcdv0 *= strength;
                Vector3d dcdv1 = Vector3d.CrossProduct(v02, goalNormal);
                dcdv1 *= strength;
                Vector3d dcdv2 = Vector3d.CrossProduct(-v01, goalNormal);
                dcdv2 *= strength;

                for (int j = 0; j < 3; j++)
                {
                    tups.Add(new Tuple<int, int, double>(i, 3 * id0 + j, dcdv0[j]));
                    tups.Add(new Tuple<int, int, double>(i, 3 * id1 + j, dcdv1[j]));
                    tups.Add(new Tuple<int, int, double>(i, 3 * id2 + j, dcdv2[j]));
                }

            }

            return new SparseMatrixBuilder(alignFaceIds.Count, 3 * cMesh.Vertices.Length, tups);
        }

        public override double[] Error(CMesh cMesh)
        {
            double[] err = new double[alignFaceIds.Count];
            var verts = cMesh.Mesh.Vertices.ToPoint3dArray();
            for (int i = 0; i < alignFaceIds.Count; i++)
            {
                var face = cMesh.Mesh.Faces[i];
                var p1 = verts[face.A];
                var p2 = verts[face.B];
                var p3 = verts[face.C];
                Vector3d goalNormal = goalNormals[i];
                Vector3d normal = Vector3d.CrossProduct(p2 - p1, p3 - p1);
                normal.Unitize();
                double strength = strengths[i];
                err[i] = strength * (normal * goalNormal - 1);
            }
            return err;
        }
    }
}
