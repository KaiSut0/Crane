using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.FSharp.Core;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace Crane.Constraints
{
    public class SetMaxFoldAngle : Constraint
    {
        public SetMaxFoldAngle(double maxFoldAngle, bool mountainOn, bool valleyOn)
        {
            this.maxFoldAngle = maxFoldAngle;
            this.mountainOn = mountainOn;
            this.valleyOn = valleyOn;
        }
        private double maxFoldAngle;
        private bool mountainOn;
        private bool valleyOn;
        public override Matrix<double> Jacobian(CMesh cMesh)
        {
            var foldAngles = cMesh.GetFoldAngles();
            var excededAngles = new List<double>();
            var excededEdgeIds = new List<int>();
            for (int i = 0; i < foldAngles.Count; i++)
            {
                var foldAngle = foldAngles[i];
                if(Math.Abs(foldAngle) > maxFoldAngle)
                {
                    if ((foldAngle > 0 && valleyOn) || (foldAngle < 0 && mountainOn))
                    { 
                        excededAngles.Add(foldAngle);
                        excededEdgeIds.Add(i);
                    }
                }
            }

            int rows = excededEdgeIds.Count;
            int columns = cMesh.DOF;
            Mesh mesh = cMesh.Mesh;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            mesh.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = mesh.Vertices;


            for (int id = 0; id < excededEdgeIds.Count; id++)
            {
                int e_ind = excededEdgeIds[id];


                /// Register indices
                IndexPair edge_ind = cMesh.inner_edges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = cMesh.face_pairs[e_ind];
                int P = face_ind.I;
                int Q = face_ind.J;

                MeshFace face_P = mesh.Faces[P];
                MeshFace face_Q = mesh.Faces[Q];
                int p = 0;
                int q = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (!edge_ind.Contains(face_P[i]))
                    {
                        p = face_P[i];
                    }
                    if (!edge_ind.Contains(face_Q[i]))
                    {
                        q = face_Q[i];
                    }
                }
                /// Compute normals
                Vector3d normal_P = mesh.FaceNormals[P];
                Vector3d normal_Q = mesh.FaceNormals[Q];
                /// Compute h_P & cot_Pu
                Vector3d vec_up = vert[p] - vert[u];
                Vector3d vec_uv = vert[v] - vert[u];
                double sin_Pu = (Vector3d.CrossProduct(vec_up, vec_uv) / (vec_up.Length * vec_uv.Length)).Length;
                double cos_Pu = (vec_up * vec_uv) / (vec_up.Length * vec_uv.Length);
                double cot_Pu = cos_Pu / sin_Pu;
                double len_up = vec_up.Length;
                double h_P = len_up * sin_Pu;
                /// Compute cot_Pv
                Vector3d vec_vp = vert[p] - vert[v];
                Vector3d vec_vu = vert[u] - vert[v];
                double sin_Pv = (Vector3d.CrossProduct(vec_vp, vec_vu) / (vec_vp.Length * vec_vu.Length)).Length;
                double cos_Pv = (vec_vp * vec_vu) / (vec_vp.Length * vec_vu.Length);
                double cot_Pv = cos_Pv / sin_Pv;
                /// Compute h_Q & cot_Qu
                Vector3d vec_uq = vert[q] - vert[u];
                double sin_Qu = (Vector3d.CrossProduct(vec_uq, vec_uv) / (vec_uq.Length * vec_uv.Length)).Length;
                double cos_Qu = (vec_uq * vec_uv) / (vec_uq.Length * vec_uv.Length);
                double cot_Qu = cos_Qu / sin_Qu;
                double len_uq = vec_uq.Length;
                double h_Q = len_uq * sin_Qu;
                /// Compute cot_Qv
                Vector3d vec_vq = vert[q] - vert[v];
                double sin_Qv = (Vector3d.CrossProduct(vec_vq, vec_vu) / (vec_vq.Length * vec_vu.Length)).Length;
                double cos_Qv = vec_vq * vec_vu / (vec_vq.Length * vec_vu.Length);
                double cot_Qv = cos_Qv / sin_Qv;
                List<double> normal_P_list = new List<double>();
                List<double> normal_Q_list = new List<double>();
                normal_P_list.Add(normal_P.X);
                normal_P_list.Add(normal_P.Y);
                normal_P_list.Add(normal_P.Z);
                normal_Q_list.Add(normal_Q.X);
                normal_Q_list.Add(normal_Q.Y);
                normal_Q_list.Add(normal_Q.Z);
                /// Compute coefficients
                double co_pv = (-1 * cot_Pv) / (cot_Pu + cot_Pv);
                double co_qv = (-1 * cot_Qv) / (cot_Qu + cot_Qv);
                double co_pu = (-1 * cot_Pu) / (cot_Pu + cot_Pv);
                double co_qu = (-1 * cot_Qu) / (cot_Qu + cot_Qv);
                /// Compute Jacobian
                for (int v_ind = 0; v_ind < vert.Count; v_ind++)
                {
                    if (v_ind == p)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = normal_P_list[x] / h_P;
                            //var *= foldAngle - setAngle;
                            int rind = id;
                            int cind = 3 * v_ind + x;
                            Tuple<int, int, double> element = Tuple.Create(rind, cind, var);
                            elements.Add(element);
                        }
                    }
                    if (v_ind == q)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = normal_Q_list[x] / h_Q;
                            //var *= foldAngle - setAngle;
                            int rind = id;
                            int cind = 3 * v_ind + x;
                            Tuple<int, int, double> element = Tuple.Create(rind, cind, var);
                            elements.Add(element);
                        }
                    }
                    if (v_ind == u)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = co_pv * (normal_P_list[x] / h_P) + co_qv * (normal_Q_list[x] / h_Q);
                            //var *= foldAngle - setAngle;
                            int rind = id;
                            int cind = 3 * v_ind + x;
                            Tuple<int, int, double> element = Tuple.Create(rind, cind, var);
                            elements.Add(element);
                        }
                    }
                    if (v_ind == v)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = co_pu * (normal_P_list[x] / h_P) + co_qu * (normal_Q_list[x] / h_Q);
                            //var *= foldAngle - setAngle;
                            int rind = id;
                            int cind = 3 * v_ind + x;
                            Tuple<int, int, double> element = Tuple.Create(rind, cind, var);
                            elements.Add(element);
                        }
                    }
                }
            }
            return Matrix<double>.Build.SparseOfIndexed(rows, columns, elements);
        }

        public override Vector<double> Error(CMesh cMesh)
        {
            var foldAngles = cMesh.GetFoldAngles();
            var errors = new List<double>();
            foreach (var foldAngle in foldAngles)
            {
                if(Math.Abs(foldAngle) > maxFoldAngle)
                {
                    if(foldAngle > 0 && valleyOn) errors.Add(foldAngle - maxFoldAngle);
                    else if (foldAngle < 0 && mountainOn) errors.Add(foldAngle + maxFoldAngle);
                }
            }

            return Vector<double>.Build.DenseOfArray(errors.ToArray());
        }
    }
}
