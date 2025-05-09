﻿using System;
using System.Collections.Generic;
using System.Linq;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace Crane.Constraints
{
    public class FixFoldAngle : Constraint
    {
        public FixFoldAngle(CMesh cMesh, Line[] edges, double[] setAngles, double[] stiffness)
        {
            innerEdgeIds = edges.Select(e => cMesh.GetInnerEdgeIndex(e)).ToArray();
            this.setAngles = setAngles;
            this.stiffness = stiffness;
        }
        private readonly int[] innerEdgeIds;
        private readonly double[] setAngles;
        private readonly double[] stiffness;
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            int rows = innerEdgeIds.Length;
            int columns = cMesh.DOF;
            Mesh mesh = cMesh.Mesh;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();
            
            var vert = mesh.Vertices.ToPoint3dArray();


            for (int id = 0; id < innerEdgeIds.Length; id++)
            {
                int e_ind = innerEdgeIds[id];

                double k = Math.Sqrt(stiffness[id]);

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
                //Vector3d normal_P = mesh.FaceNormals[P];
                //Vector3d normal_Q = mesh.FaceNormals[Q];
                
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
                //double sin_Qu = Math.Sqrt(1 - cos_Qu * cos_Qu);
                double cot_Qu = cos_Qu / sin_Qu;
                double len_uq = vec_uq.Length;
                double h_Q = len_uq * sin_Qu;
                /// Compute cot_Qv
                Vector3d vec_vq = vert[q] - vert[v];
                double sin_Qv = (Vector3d.CrossProduct(vec_vq, vec_vu) / (vec_vq.Length * vec_vu.Length)).Length;
                double cos_Qv = vec_vq * vec_vu / (vec_vq.Length * vec_vu.Length);
                double cot_Qv = cos_Qv / sin_Qv;
                Vector3d normal_P = Vector3d.CrossProduct(vec_uv, vec_up);
                normal_P /= normal_P.Length;
                Vector3d normal_Q = Vector3d.CrossProduct(vec_uq, vec_uv);
                normal_Q /= normal_Q.Length;

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
                var v_ind = p;
                for (int x = 0; x < 3; x++)
                {
                    double var = normal_P_list[x] / h_P;
                    int rind = id;
                    int cind = 3 * v_ind + x;
                    Tuple<int, int, double> element = Tuple.Create(rind, cind, var);
                    elements.Add(element);
                }
                v_ind = q;
                for (int x = 0; x < 3; x++)
                {
                    double var = normal_Q_list[x] / h_Q;
                    int rind = id;
                    int cind = 3 * v_ind + x;
                    Tuple<int, int, double> element = Tuple.Create(rind, cind, var);
                    elements.Add(element);
                }
                v_ind = u;
                for (int x = 0; x < 3; x++)
                {
                    double var = co_pv * (normal_P_list[x] / h_P) + co_qv * (normal_Q_list[x] / h_Q);
                    int rind = id;
                    int cind = 3 * v_ind + x;
                    Tuple<int, int, double> element = Tuple.Create(rind, cind, var);
                    elements.Add(element);
                }
                v_ind = v;
                for (int x = 0; x < 3; x++)
                {
                    double var = co_pu * (normal_P_list[x] / h_P) + co_qu * (normal_Q_list[x] / h_Q);
                    int rind = id;
                    int cind = 3 * v_ind + x;
                    Tuple<int, int, double> element = Tuple.Create(rind, cind, var);
                    elements.Add(element);
                }
            }

            return new SparseMatrixBuilder(rows, columns, elements);
        }
        public override double[] Error(CMesh cMesh)
        {
            int rows = innerEdgeIds.Length;

            var foldAngles = cMesh.GetFoldAngles();


            var err = new double[rows];

            for (int i = 0; i < rows; i++)
            {

                int id = innerEdgeIds[i];
                double foldAngle = foldAngles[id];
                double setAngle = setAngles[i];
                double k = Math.Sqrt(stiffness[i]);

                err[i] = k * (foldAngle - setAngle);

            }

            return err;

        }
    }
}

