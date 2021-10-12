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
    public class ValleyOnlyFlatFoldable : Constraint
    {
        public ValleyOnlyFlatFoldable() { }
        public override Matrix<double> Jacobian(CMesh cMesh)
        {
            Mesh m = cMesh.Mesh;

            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();


            int rows;
            int cols = 3 * cMesh.NumberOfVertices;

            var topo = m.TopologyVertices;
            //topo.SortEdges();

            List<bool> isNaked = new List<bool>(m.GetNakedEdgePointStatus());
            List<int> internalVertices = new List<int>();
            int vertexCount = cMesh.NumberOfVertices;
            if (cMesh.HasDevelopment) vertexCount /= 2;

            //内部頂点インデックスリスト作成
            for (int i = 0; i < vertexCount; i++)
            {
                if (!isNaked[i])
                {
                    internalVertices.Add(i);
                }
            }

            rows = internalVertices.Count;

            //各内部頂点について
            for (int r = 0; r < internalVertices.Count; r++)
            {
                //内部頂点のインデックス、位置ベクトル
                int index_center = internalVertices[r];
                Vector3d center = new Vector3d(topo[index_center]);

                //接続点のインデックス、位置ベクトル
                List<int> index_neighbors = new List<int>();
                List<Vector3d> neighbors = new List<Vector3d>();
                //index_neighbors.AddRange(topo.ConnectedTopologyVertices(index_center));
                index_neighbors.AddRange(cMesh.ConnectedTopologyVerticesList[index_center]);

                //エッジベクトル
                List<Vector3d> vecs = new List<Vector3d>();
                List<Vector3d> normals = new List<Vector3d>();

                int n = index_neighbors.Count;

                //三角形分割の区別
                //List<int> connected_edges = new List<int>(topo.ConnectedEdges(index_center));
                List<int> connected_edges = cMesh.ConnectedTopologyEdgesList[index_center];
                List<double> signs = new List<double>();
                for (int i = -1; i < n - 1; i++)
                {
                    if (i == -1)
                    {
                        signs.Add(1);
                    }
                    else if (cMesh.edgeInfo[connected_edges[i]] != 'T' & cMesh.edgeInfo[connected_edges[i]] != 'U' & cMesh.edgeInfo[connected_edges[i]] != 'M')
                    {
                        signs.Add(-signs[i]);
                    }
                    else
                    {
                        signs.Add(signs[i]);
                    }
                }

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
                    Vector3d v1 = signs[i] * Vector3d.CrossProduct(normals[i], vecs[i]);
                    v1 /= vecs[i].SquareLength;
                    Vector3d v2 = signs[(i + 1) % n] * Vector3d.CrossProduct(normals[(i + 1) % n], vecs[i]);
                    v2 /= vecs[i].SquareLength;
                    v1 -= v2;
                    v_center -= v1;

                    elements.Add(new Tuple<int, int, double>(r, 3 * index, v1.X));
                    elements.Add(new Tuple<int, int, double>(r, 3 * index + 1, v1.Y));
                    elements.Add(new Tuple<int, int, double>(r, 3 * index + 2, v1.Z));
                }

                elements.Add(new Tuple<int, int, double>(r, 3 * index_center, v_center.X));
                elements.Add(new Tuple<int, int, double>(r, 3 * index_center + 1, v_center.Y));
                elements.Add(new Tuple<int, int, double>(r, 3 * index_center + 2, v_center.Z));

            }


            return Matrix<double>.Build.SparseOfIndexed(rows, cols, elements);
        }
        public override Vector<double> Error(CMesh cMesh)
        {
            Mesh m = cMesh.Mesh;

            List<double> err = new List<double>();

            m.Normals.ComputeNormals();
            var topo = m.TopologyVertices;
            //topo.SortEdges();

            List<bool> isNaked = new List<bool>(m.GetNakedEdgePointStatus());
            List<int> internalVertices = new List<int>();
            int vertexCount = cMesh.NumberOfVertices;
            if (cMesh.HasDevelopment) vertexCount /= 2;
            for (int i = 0; i < vertexCount; i++)
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
                //index_neighbors.AddRange(topo.ConnectedTopologyVertices(index_center));
                index_neighbors.AddRange(cMesh.ConnectedTopologyVerticesList[index_center]);

                //エッジベクトル
                List<Vector3d> vecs = new List<Vector3d>();
                double sum = 0;

                int n = index_neighbors.Count;

                //三角形分割の区別
                //List<int> connected_edges = new List<int>(topo.ConnectedEdges(index_center));
                List<int> connected_edges = cMesh.ConnectedTopologyEdgesList[index_center];
                //List<bool> is_corrects = new List<bool>();

                //for (int i = 0; i < n; i++)
                //{
                //    bool is_correct = m.TopologyEdges.GetTopologyVertices(connected_edges[i]).Contains(index_neighbors[i]);
                //    is_corrects.Add(is_correct);
                //}

                List<double> signs = new List<double>();
                for (int i = -1; i < n - 1; i++)
                {
                    char edgeInfo = cMesh.edgeInfo[connected_edges[(n + i) % n]];
                    if (i == -1)
                    {
                        signs.Add(1);
                    }
                    else if (cMesh.edgeInfo[connected_edges[i]] != 'T' & cMesh.edgeInfo[connected_edges[i]] != 'U' & cMesh.edgeInfo[connected_edges[i]] != 'M')
                    {
                        signs.Add(-signs[i]);
                    }
                    else
                    {
                        signs.Add(signs[i]);
                    }
                }

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
                        sum += signs[i] * Vector3d.VectorAngle(vecs[n - 1], vecs[0]);
                    }
                    else
                    {
                        sum += signs[i] * Vector3d.VectorAngle(vecs[i - 1], vecs[i]);
                    }
                }

                err.Add(sum);
            }

            return Vector<double>.Build.DenseOfArray(err.ToArray());
        }
    }
}
