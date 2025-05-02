using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;
using Rhino;

namespace Crane.Constraints
{
    public class EqualFoldAngle : Constraint
    {
        public EqualFoldAngle(CMesh cMesh, Line[] primaryEdges, Line[] secondaryEdges, double[] strength)
        {
            primaryEdgeIds = primaryEdges.Select(e => cMesh.GetInnerEdgeIndex(e)).ToArray();
            secondaryEdgeIds = secondaryEdges.Select(e => cMesh.GetInnerEdgeIndex(e)).ToArray();
            this.strength = strength;
        }
        private readonly int[] primaryEdgeIds;
        private readonly int[] secondaryEdgeIds;
        private readonly double[] strength;
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            int rows = primaryEdgeIds.Length;
            int columns = cMesh.DOF;
            Mesh mesh = cMesh.Mesh;
            SparseMatrixBuilder J = new SparseMatrixBuilder(rows, columns);
            
            var vert = mesh.Vertices.ToPoint3dArray();

            for(int id = 0; id < primaryEdgeIds.Length; id++)
            {
                int e_ind = primaryEdgeIds[id];

                double k = strength[id];

                /// Register indices
                IndexPair edge_ind = cMesh.InnerEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = cMesh.FacePairs[e_ind];
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
                for (int v_ind = 0; v_ind < vert.Length; v_ind++)
                {
                    if (v_ind == p)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = k * normal_P_list[x] / h_P;
                            //var *= foldAngle - setAngle;
                            int rind = id;
                            int cind = 3 * v_ind + x;
                            J.Add(rind, cind, var);
                        }
                    }
                    if (v_ind == q)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = k * normal_Q_list[x] / h_Q;
                            //var *= foldAngle - setAngle;
                            int rind = id;
                            int cind = 3 * v_ind + x;
                            J.Add(rind, cind, var);
                        }
                    }
                    if (v_ind == u)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = k * (co_pv * (normal_P_list[x] / h_P) + co_qv * (normal_Q_list[x] / h_Q));
                            //var *= foldAngle - setAngle;
                            int rind = id;
                            int cind = 3 * v_ind + x;
                            J.Add(rind, cind, var);
                        }
                    }
                    if (v_ind == v)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = k * (co_pu * (normal_P_list[x] / h_P) + co_qu * (normal_Q_list[x] / h_Q));
                            //var *= foldAngle - setAngle;
                            int rind = id;
                            int cind = 3 * v_ind + x;
                            J.Add(rind, cind, var);
                        }
                    }
                }
            }

            for(int id = 0; id < primaryEdgeIds.Length; id++)
            {
                int e_ind = secondaryEdgeIds[id];

                double k = strength[id];

                /// Register indices
                IndexPair edge_ind = cMesh.InnerEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = cMesh.FacePairs[e_ind];
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
                for (int v_ind = 0; v_ind < vert.Length; v_ind++)
                {
                    if (v_ind == p)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = k * normal_P_list[x] / h_P;
                            //var *= foldAngle - setAngle;
                            int rind = id;
                            int cind = 3 * v_ind + x;
                            J.Add(rind, cind, -var);
                        }
                    }
                    if (v_ind == q)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = k * normal_Q_list[x] / h_Q;
                            //var *= foldAngle - setAngle;
                            int rind = id;
                            int cind = 3 * v_ind + x;
                            J.Add(rind, cind, -var);
                        }
                    }
                    if (v_ind == u)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = k * (co_pv * (normal_P_list[x] / h_P) + co_qv * (normal_Q_list[x] / h_Q));
                            //var *= foldAngle - setAngle;
                            int rind = id;
                            int cind = 3 * v_ind + x;
                            J.Add(rind, cind, -var);
                        }
                    }
                    if (v_ind == v)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = k * (co_pu * (normal_P_list[x] / h_P) + co_qu * (normal_Q_list[x] / h_Q));
                            //var *= foldAngle - setAngle;
                            int rind = id;
                            int cind = 3 * v_ind + x;
                            J.Add(rind, cind, -var);
                        }
                    }
                }
            }

            return J;
        }

        public override double[] Error(CMesh cMesh)
        {
            int rows = primaryEdgeIds.Length;
            var err = new double[rows];
            var foldAngles = cMesh.GetFoldAngles();

            for (int i = 0; i < rows; i++)
            {
                int id1 = primaryEdgeIds[i];
                int id2 = secondaryEdgeIds[i];
                double fa1 = foldAngles[id1];
                double fa2 = foldAngles[id2];
                err[i] = strength[i] * (fa1 - fa2);
            }

            return err;
        }
    }
}
