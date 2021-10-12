using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection.Configuration;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Crane.Patterns
{
    public abstract class TessellationOnDoubleSurface
    {

        public Mesh Tessellation { get; }
        public TessellationOnDoubleSurface(Surface S1, Surface S2, int U, int V)
        {
            Tessellation = CreateMesh(S1, S2, U, V);
        }

        private Mesh CreateMesh(Surface S1, Surface S2, int U, int V)
        {
            int m = M();
            int n = N();
            Point3d[,] ptsS0 = DivideSurface(S1, m * U, n * V);
            Point3d[,] ptsS1 = DivideSurface(S2, m * U, n * V);

            Mesh mesh = new Mesh();

            Dictionary<Tuple<int, int, int>, int> map = new Dictionary<Tuple<int, int, int>, int>();

            int vertID = 0;
            for (int i = 0; i < m * U + 1; i++)
            {
                for (int j = 0; j < n * V + 1; j++)
                {
                    var tup0 = new Tuple<int, int, int>(0, i, j);
                    var tup1 = new Tuple<int, int, int>(1, i, j);
                    mesh.Vertices.Add(ptsS0[i, j]);
                    mesh.Vertices.Add(ptsS1[i, j]);
                    map[tup0] = vertID;
                    map[tup1] = vertID + 1;
                    vertID += 2;
                }
            }

            AddFaces(CreateVList(), CreateFList1(), CreateFList2(), CreateFList3(), CreateFList4(), map, U, V, m, n, ref mesh);

            mesh.Compact();
            mesh.FaceNormals.ComputeFaceNormals();
            mesh.Normals.ComputeNormals();
            return mesh;
        }

        protected abstract List<Tuple<int, int, int>> CreateVList();
        protected abstract List<int[]> CreateFList1();
        protected abstract List<int[]> CreateFList2();
        protected abstract List<int[]> CreateFList3();
        protected abstract List<int[]> CreateFList4();

        protected abstract int M();
        protected abstract int N();

        private void AddFaces(List<Tuple<int, int, int>> vList, List<int[]> faces1, List<int[]> faces2, List<int[]> faces3, List<int[]> faces4,
            Dictionary<Tuple<int, int, int>, int> map, int U, int V, int M, int N, ref Mesh mesh)
        {
            for(int i = 0; i < U; i++)
            {
                for (int j = 0; j < V; j++)
                {
                    foreach (var face in faces1)
                    {
                        if (face.Length == 3)
                        {
                            var f0 = face[0];
                            var f1 = face[1];
                            var f2 = face[2];
                            var v0 = vList[f0];
                            var v1 = vList[f1];
                            var v2 = vList[f2];
                            mesh.Faces.AddFace(map[Tup(v0.Item1, v0.Item2 + i * M, v0.Item3 + j * N)],
                                map[Tup(v1.Item1, v1.Item2 + i * M, v1.Item3 + j * N)],
                                map[Tup(v2.Item1, v2.Item2 + i * M, v2.Item3 + j * N)]);

                        }
                        else if (face.Length == 4)
                        {
                            var f0 = face[0];
                            var f1 = face[1];
                            var f2 = face[2];
                            var f3 = face[3];
                            var v0 = vList[f0];
                            var v1 = vList[f1];
                            var v2 = vList[f2];
                            var v3 = vList[f3];
                            mesh.Faces.AddFace(map[Tup(v0.Item1, v0.Item2 + i * M, v0.Item3 + j * N)],
                                map[Tup(v1.Item1, v1.Item2 + i * M, v1.Item3 + j * N)],
                                map[Tup(v2.Item1, v2.Item2 + i * M, v2.Item3 + j * N)],
                                map[Tup(v3.Item1, v3.Item2 + i * M, v3.Item3 + j * N)]);
                        }
                    }
                    if(j < V - 1)
                    {
                        foreach (var face in faces2)
                        {
                            if (face.Length == 3)
                            {
                                var f0 = face[0];
                                var f1 = face[1];
                                var f2 = face[2];
                                var v0 = vList[f0];
                                var v1 = vList[f1];
                                var v2 = vList[f2];
                                mesh.Faces.AddFace(map[Tup(v0.Item1, v0.Item2 + i * M, v0.Item3 + j * N)],
                                    map[Tup(v1.Item1, v1.Item2 + i * M, v1.Item3 + j * N)],
                                    map[Tup(v2.Item1, v2.Item2 + i * M, v2.Item3 + j * N)]);

                            }
                            else if (face.Length == 4)
                            {
                                var f0 = face[0];
                                var f1 = face[1];
                                var f2 = face[2];
                                var f3 = face[3];
                                var v0 = vList[f0];
                                var v1 = vList[f1];
                                var v2 = vList[f2];
                                var v3 = vList[f3];
                                mesh.Faces.AddFace(map[Tup(v0.Item1, v0.Item2 + i * M, v0.Item3 + j * N)],
                                    map[Tup(v1.Item1, v1.Item2 + i * M, v1.Item3 + j * N)],
                                    map[Tup(v2.Item1, v2.Item2 + i * M, v2.Item3 + j * N)],
                                    map[Tup(v3.Item1, v3.Item2 + i * M, v3.Item3 + j * N)]);
                            }

                        }
                    }
                    if(i < U - 1)
                    {
                        foreach (var face in faces3)
                        {
                            if (face.Length == 3)
                            {
                                var f0 = face[0];
                                var f1 = face[1];
                                var f2 = face[2];
                                var v0 = vList[f0];
                                var v1 = vList[f1];
                                var v2 = vList[f2];
                                mesh.Faces.AddFace(map[Tup(v0.Item1, v0.Item2 + i * M, v0.Item3 + j * N)],
                                    map[Tup(v1.Item1, v1.Item2 + i * M, v1.Item3 + j * N)],
                                    map[Tup(v2.Item1, v2.Item2 + i * M, v2.Item3 + j * N)]);

                            }
                            else if (face.Length == 4)
                            {
                                var f0 = face[0];
                                var f1 = face[1];
                                var f2 = face[2];
                                var f3 = face[3];
                                var v0 = vList[f0];
                                var v1 = vList[f1];
                                var v2 = vList[f2];
                                var v3 = vList[f3];
                                mesh.Faces.AddFace(map[Tup(v0.Item1, v0.Item2 + i * M, v0.Item3 + j * N)],
                                    map[Tup(v1.Item1, v1.Item2 + i * M, v1.Item3 + j * N)],
                                    map[Tup(v2.Item1, v2.Item2 + i * M, v2.Item3 + j * N)],
                                    map[Tup(v3.Item1, v3.Item2 + i * M, v3.Item3 + j * N)]);
                            }

                        }
                    }
                    if (i < U - 1 & j < V - 1)
                    {
                        foreach(var face in faces4)
                        {
                            if (face.Length == 3)
                            {
                                var f0 = face[0];
                                var f1 = face[1];
                                var f2 = face[2];
                                var v0 = vList[f0];
                                var v1 = vList[f1];
                                var v2 = vList[f2];
                                mesh.Faces.AddFace(map[Tup(v0.Item1, v0.Item2 + i * M, v0.Item3 + j * N)],
                                    map[Tup(v1.Item1, v1.Item2 + i * M, v1.Item3 + j * N)],
                                    map[Tup(v2.Item1, v2.Item2 + i * M, v2.Item3 + j * N)]);

                            }
                            else if (face.Length == 4)
                            {
                                var f0 = face[0];
                                var f1 = face[1];
                                var f2 = face[2];
                                var f3 = face[3];
                                var v0 = vList[f0];
                                var v1 = vList[f1];
                                var v2 = vList[f2];
                                var v3 = vList[f3];
                                mesh.Faces.AddFace(map[Tup(v0.Item1, v0.Item2 + i * M, v0.Item3 + j * N)],
                                    map[Tup(v1.Item1, v1.Item2 + i * M, v1.Item3 + j * N)],
                                    map[Tup(v2.Item1, v2.Item2 + i * M, v2.Item3 + j * N)],
                                    map[Tup(v3.Item1, v3.Item2 + i * M, v3.Item3 + j * N)]);
                            }

                        }
                    }
                }
            }
        }

        protected Tuple<int, int, int> Tup(int i, int j, int k)
        {
            return new Tuple<int, int, int>(i, j, k);
        }

        private Point3d[,] DivideSurface(Surface S, int U, int V)
        {
            var domSU = S.Domain(0);
            var domSV = S.Domain(1);

            var SUT0 = domSU.T0;
            var SUT1 = domSU.T1;
            var SVT0 = domSV.T0;
            var SVT1 = domSV.T1;

            Point3d[,] ptsS = new Point3d[U + 1, V + 1];

            var diffSU = (SUT1 - SUT0) / (U);
            var diffSV = (SVT1 - SVT0) / (V);

            for (int i = 0; i < U + 1; i++)
            {
                var SU = SUT0 + i * diffSU;


                for (int j = 0; j < V + 1; j++)
                {
                    var SV = SVT0 + j * diffSV;
                    ptsS[i, j] = S.PointAt(SU, SV);
                }
            }

            return ptsS;
        }
    }
}
