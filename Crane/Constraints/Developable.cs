using System;
using System.Collections.Generic;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;


namespace Crane.Constraints
{
    public class Developable : Constraint
    {
        public Developable() { }
        public override Matrix<double> Jacobian(CMesh cMesh)
        {
            Mesh m = cMesh.Mesh;

            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();
            int cols = 3 * m.Vertices.Count;
     
            var topo = m.TopologyVertices;
            topo.SortEdges();

            List<bool> isNaked = new List<bool>(m.GetNakedEdgePointStatus());
            List<int> internalVertices = new List<int>();

            //内部頂点インデックスリスト作成
            for (int i = 0; i < topo.Count; i++)
            {
                if (!isNaked[i])
                {
                    internalVertices.Add(i);
                }
            }

            int rows = internalVertices.Count;

            //各内部頂点について
            for (int r = 0; r < internalVertices.Count; r++)
            {
                //内部頂点のインデックス、位置ベクトル
                int index_center = internalVertices[r];
                Vector3d center = new Vector3d(topo[index_center]);

                //接続点のインデックス、位置ベクトル
                List<int> index_neighbors = new List<int>();
                List<Vector3d> neighbors = new List<Vector3d>();
                index_neighbors.AddRange(topo.ConnectedTopologyVertices(index_center));

                //エッジベクトル
                List<Vector3d> vecs = new List<Vector3d>();
                List<Vector3d> normals = new List<Vector3d>();

                int n = index_neighbors.Count;

                //位置ベクトル取得
                foreach (int index_neighbor in index_neighbors)
                {
                    neighbors.Add(new Vector3d(topo[index_neighbor]));
                }

                //方向ベクトル取得
                foreach (Vector3d neighbor in neighbors)
                {
                    Vector3d temp = neighbor - center;
                    vecs.Add(temp);
                }

                //法線ベクトル取得
                for (int i = 0; i < n; i++)
                {
                    Vector3d temp = new Vector3d();
                    if (i == 0)
                    {
                        temp = Vector3d.CrossProduct(vecs[n - 1], vecs[0]);
                    }
                    else
                    {
                        temp = Vector3d.CrossProduct(vecs[i - 1], vecs[i]);
                    }
                    temp.Unitize();
                    normals.Add(temp);
                }

                Vector3d v_center = new Vector3d();

                for (int i = 0; i < n; i++)
                {
                    int index = index_neighbors[i];
                    Vector3d v1 = Vector3d.CrossProduct(normals[i], vecs[i]);
                    v1 *= 1 / vecs[i].SquareLength;
                    Vector3d v2 = Vector3d.CrossProduct(normals[(i + 1) % n], vecs[i]);
                    v2 *= 1 / vecs[i].SquareLength;
                    v1 -= v2;
                    v_center -= v1;

                    elements.Add(new Tuple<int, int, double>(r, index*3, v1.X));
                    elements.Add(new Tuple<int, int, double>(r, index*3+1, v1.Y));
                    elements.Add(new Tuple<int, int, double>(r, index*3+2, v1.Z));



                    //var.Add((double)v1.X);
                    //var.Add((double)v1.Y);
                    //var.Add((double)v1.Z);
                    //c_index.Add(index * 3);
                    //c_index.Add(index * 3 + 1);
                    //c_index.Add(index * 3 + 2);
                    //r_index.Add(r);
                    //r_index.Add(r);
                    //r_index.Add(r);
                }

                elements.Add(new Tuple<int, int, double>(r, index_center * 3, v_center.X));
                elements.Add(new Tuple<int, int, double>(r, index_center * 3 + 1, v_center.Y));
                elements.Add(new Tuple<int, int, double>(r, index_center * 3 + 2, v_center.Z));

            }

            return Matrix<double>.Build.SparseOfIndexed(rows, cols, elements);
        }
        public override Vector<double> Error(CMesh cMesh)
        {
            
            Mesh m = cMesh.Mesh;

            List<double> err = new List<double>();

            m.Normals.ComputeNormals();
            var topo = m.TopologyVertices;
            topo.SortEdges();

            List<bool> isNaked = new List<bool>(m.GetNakedEdgePointStatus());
            List<int> internalVertices = new List<int>();
            for (int i = 0; i < topo.Count; i++)
            {
                if (!isNaked[i])
                {
                    internalVertices.Add(i);
                }
            }

            //各内部頂点について
            for (int r = 0; r < internalVertices.Count; r++)
            {
                //内部頂点のインデックス、位置ベクトル
                int index_center = internalVertices[r];
                Vector3d center = new Vector3d(topo[index_center]);

                //接続点のインデックス、位置ベクトル
                List<int> index_neighbors = new List<int>();
                List<Vector3d> neighbors = new List<Vector3d>();
                index_neighbors.AddRange(topo.ConnectedTopologyVertices(index_center));

                //エッジベクトル
                List<Vector3d> vecs = new List<Vector3d>();
                double sum = 0;

                int n = index_neighbors.Count;

                //位置ベクトル取得
                foreach (int index_neighbor in index_neighbors)
                {
                    neighbors.Add(new Vector3d(topo[index_neighbor]));
                }

                //方向ベクトル取得
                foreach (Vector3d neighbor in neighbors)
                {
                    Vector3d temp = neighbor - center;
                    vecs.Add(temp);
                }

                for (int i = 0; i < n; i++)
                {
                    if (i == 0)
                    {
                        sum += Vector3d.VectorAngle(vecs[n - 1], vecs[0]);
                    }
                    else
                    {
                        sum += Vector3d.VectorAngle(vecs[i - 1], vecs[i]);
                    }
                }

                sum -= 2 * Math.PI;

                err.Add((double)sum);
            }

            

            return Vector<double>.Build.DenseOfArray(err.ToArray());

        }
    }
}

