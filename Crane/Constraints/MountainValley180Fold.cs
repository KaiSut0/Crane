﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using Rhino.Geometry;
using Rhino;

namespace Crane.Constraints
{
    public class MountainValley180Fold : Constraint
    {
        public MountainValley180Fold() { }
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            CMesh cm = cMesh;
            Mesh m = cm.Mesh;

            int rows = 0;
            int columns = cMesh.Mesh.Vertices.Count * 3;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            var vert = cm.Vertices;

            if (cm.ValleyEdges == null && cm.MountainEdges == null)
            {
                return null;
            }

            for (int e_ind = 0; e_ind < cm.ValleyEdges.Count; e_ind++)
            {
                /// Register indices
                IndexPair edge_ind = cm.ValleyEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = cm.ValleyFacePairs[e_ind];
                int P = face_ind.I;
                int Q = face_ind.J;

                MeshFace face_P = m.Faces[P];
                MeshFace face_Q = m.Faces[Q];
                int p = 0;
                int q = 0;

                double h_P_initial = cm.ValleyFaceHeightPairs[e_ind].Item1;
                double h_Q_initial = cm.ValleyFaceHeightPairs[e_ind].Item2;
                double len_e = cm.LengthOfValleyDiagonalEdges[e_ind];

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

                Vector3d vec_up = vert[p] - vert[u];
                Vector3d vec_uv = vert[v] - vert[u];
                Vector3d vec_uq = vert[q] - vert[u];
                Vector3d vec_vp = vert[p] - vert[v];
                Vector3d vec_vq = vert[q] - vert[v];

                double volume = vec_up * Vector3d.CrossProduct(vec_uq, vec_uv);

                if (true)
                {
                    rows += 1;
                    /// Compute Jacobian
                    for (int v_ind = 0; v_ind < vert.Length; v_ind++)
                    {
                        if (v_ind == p)
                        {
                            Vector3d delS_delX_p = Vector3d.CrossProduct(vec_uq, vec_uv) / (h_P_initial * h_Q_initial * len_e);
                            List<double> vars_v = new List<double>();
                            vars_v.Add(delS_delX_p.X);
                            vars_v.Add(delS_delX_p.Y);
                            vars_v.Add(delS_delX_p.Z);
                            for (int x = 0; x < 3; x++)
                            {
                                var value = vars_v[x];
                                var cind = 3 * v_ind + x;
                                var rind = rows - 1;
                                elements.Add(new Tuple<int, int, double>(rind, cind, value));
                            }
                        }
                        if (v_ind == q)
                        {
                            Vector3d delS_delX_q = Vector3d.CrossProduct(vec_uv, vec_up) / (h_P_initial * h_Q_initial * len_e);
                            List<double> vars_q = new List<double>();
                            vars_q.Add(delS_delX_q.X);
                            vars_q.Add(delS_delX_q.Y);
                            vars_q.Add(delS_delX_q.Z);
                            for (int x = 0; x < 3; x++)
                            {
                                var value = vars_q[x];
                                var cind = 3 * v_ind + x;
                                var rind = rows - 1;
                                elements.Add(new Tuple<int, int, double>(rind, cind, value));
                            }
                        }
                        if (v_ind == u)
                        {
                            Vector3d delS_delX_u = Vector3d.CrossProduct(vec_vq, vec_vp) / (h_P_initial * h_Q_initial * len_e);
                            List<double> vars_u = new List<double>();
                            vars_u.Add(delS_delX_u.X);
                            vars_u.Add(delS_delX_u.Y);
                            vars_u.Add(delS_delX_u.Z);
                            for (int x = 0; x < 3; x++)
                            {
                                var value = vars_u[x];
                                var cind = 3 * v_ind + x;
                                var rind = rows - 1;
                                elements.Add(new Tuple<int, int, double>(rind, cind, value));

                            }
                        }
                        if (v_ind == v)
                        {
                            Vector3d delS_delX_v = Vector3d.CrossProduct(vec_up, vec_uq) / (h_P_initial * h_Q_initial * len_e);
                            List<double> vars_v = new List<double>();
                            vars_v.Add(delS_delX_v.X);
                            vars_v.Add(delS_delX_v.Y);
                            vars_v.Add(delS_delX_v.Z);
                            for (int x = 0; x < 3; x++)
                            {
                                var value = vars_v[x];
                                var cind = 3 * v_ind + x;
                                var rind = rows - 1;
                                elements.Add(new Tuple<int, int, double>(rind, cind, value));

                            }
                        }
                    }
                }

            }

            for (int e_ind = 0; e_ind < cm.MountainEdges.Count; e_ind++)
            {
                /// Register indices
                IndexPair edge_ind = cm.MountainEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = cm.MountainFacePairs[e_ind];
                int P = face_ind.I;
                int Q = face_ind.J;

                MeshFace face_P = m.Faces[P];
                MeshFace face_Q = m.Faces[Q];
                int p = 0;
                int q = 0;

                double h_P_initial = cm.MountainFaceHeightPairs[e_ind].Item1;
                double h_Q_initial = cm.MountainFaceHeightPairs[e_ind].Item2;
                double len_e = cm.LengthOfMountainDiagonalEdges[e_ind];

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

                Vector3d vec_up = vert[p] - vert[u];
                Vector3d vec_uv = vert[v] - vert[u];
                Vector3d vec_uq = vert[q] - vert[u];
                Vector3d vec_vp = vert[p] - vert[v];
                Vector3d vec_vq = vert[q] - vert[v];

                double volume = vec_up * Vector3d.CrossProduct(vec_uq, vec_uv);

                if (true)
                {
                    rows += 1;
                    /// Compute Jacobian
                    for (int v_ind = 0; v_ind < vert.Length; v_ind++)
                    {
                        if (v_ind == p)
                        {
                            Vector3d delS_delX_p = Vector3d.CrossProduct(vec_uq, vec_uv) / (h_P_initial * h_Q_initial * len_e);
                            List<double> vars_v = new List<double>();
                            vars_v.Add(delS_delX_p.X);
                            vars_v.Add(delS_delX_p.Y);
                            vars_v.Add(delS_delX_p.Z);
                            for (int x = 0; x < 3; x++)
                            {
                                var value = vars_v[x];
                                var cind = 3 * v_ind + x;
                                var rind = rows - 1;
                                elements.Add(new Tuple<int, int, double>(rind, cind, value));
                            }
                        }
                        if (v_ind == q)
                        {
                            Vector3d delS_delX_q = Vector3d.CrossProduct(vec_uv, vec_up) / (h_P_initial * h_Q_initial * len_e);
                            List<double> vars_q = new List<double>();
                            vars_q.Add(delS_delX_q.X);
                            vars_q.Add(delS_delX_q.Y);
                            vars_q.Add(delS_delX_q.Z);
                            for (int x = 0; x < 3; x++)
                            {
                                var value = vars_q[x];
                                var cind = 3 * v_ind + x;
                                var rind = rows - 1;
                                elements.Add(new Tuple<int, int, double>(rind, cind, value));
                            }
                        }
                        if (v_ind == u)
                        {
                            Vector3d delS_delX_u = Vector3d.CrossProduct(vec_vq, vec_vp) / (h_P_initial * h_Q_initial * len_e);
                            List<double> vars_u = new List<double>();
                            vars_u.Add(delS_delX_u.X);
                            vars_u.Add(delS_delX_u.Y);
                            vars_u.Add(delS_delX_u.Z);
                            for (int x = 0; x < 3; x++)
                            {
                                var value = vars_u[x];
                                var cind = 3 * v_ind + x;
                                var rind = rows - 1;
                                elements.Add(new Tuple<int, int, double>(rind, cind, value));

                            }
                        }
                        if (v_ind == v)
                        {
                            Vector3d delS_delX_v = Vector3d.CrossProduct(vec_up, vec_uq) / (h_P_initial * h_Q_initial * len_e);
                            List<double> vars_v = new List<double>();
                            vars_v.Add(delS_delX_v.X);
                            vars_v.Add(delS_delX_v.Y);
                            vars_v.Add(delS_delX_v.Z);
                            for (int x = 0; x < 3; x++)
                            {
                                var value = vars_v[x];
                                var cind = 3 * v_ind + x;
                                var rind = rows - 1;
                                elements.Add(new Tuple<int, int, double>(rind, cind, value));

                            }
                        }
                    }
                }

            }


            if (rows == 0)
                return null;
            return new SparseMatrixBuilder(rows, columns, elements);
        }
        public override double[] Error(CMesh cMesh)
        {
            CMesh cm = cMesh;
            Mesh m = cm.Mesh;
            List<double> ans = new List<double>();


            var vert = cm.Vertices;


            for (int e_ind = 0; e_ind < cm.ValleyEdges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = cm.ValleyEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = cm.ValleyFacePairs[e_ind];
                int P = face_ind.I;
                int Q = face_ind.J;

                MeshFace face_P = m.Faces[P];
                MeshFace face_Q = m.Faces[Q];
                int p = 0;
                int q = 0;

                double h_P_initial = cm.ValleyFaceHeightPairs[e_ind].Item1;
                double h_Q_initial = cm.ValleyFaceHeightPairs[e_ind].Item2;
                double len_e = cm.LengthOfValleyDiagonalEdges[e_ind];

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

                //Compute Volume
                Vector3d vec_uv = vert[v] - vert[u];
                Vector3d vec_up = vert[p] - vert[u];
                Vector3d vec_uq = vert[q] - vert[u];

                double volume = vec_up * Vector3d.CrossProduct(vec_uq, vec_uv);
                double S_e = (volume / (h_P_initial * h_Q_initial * len_e)) - 1e-3;
                ans.Add(S_e);
            }
            for (int e_ind = 0; e_ind < cm.MountainEdges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = cm.MountainEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = cm.MountainFacePairs[e_ind];
                int P = face_ind.I;
                int Q = face_ind.J;

                MeshFace face_P = m.Faces[P];
                MeshFace face_Q = m.Faces[Q];
                int p = 0;
                int q = 0;

                double h_P_initial = cm.MountainFaceHeightPairs[e_ind].Item1;
                double h_Q_initial = cm.MountainFaceHeightPairs[e_ind].Item2;
                double len_e = cm.LengthOfMountainDiagonalEdges[e_ind];

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

                //Compute Volume
                Vector3d vec_uv = vert[v] - vert[u];
                Vector3d vec_up = vert[p] - vert[u];
                Vector3d vec_uq = vert[q] - vert[u];

                double volume = vec_up * Vector3d.CrossProduct(vec_uq, vec_uv);
                double S_e = (volume / (h_P_initial * h_Q_initial * len_e)) + 1e-3;
                ans.Add(S_e);
            }


            if (ans.Count == 0)
                return null;
            return ans.ToArray();
        }

    }
}
