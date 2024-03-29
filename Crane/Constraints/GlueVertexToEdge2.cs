﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace Crane.Constraints
{
    public class GlueVertexToEdge2 : Constraint
    {
        public GlueVertexToEdge2(int vertexIds, int edgeIds)
        {
            this.vertexIds = vertexIds;
            this.edgeIds = edgeIds;
        }
        public GlueVertexToEdge2(CMesh cMesh, Point3d vertex, Line edge)
        {
            vertexIds = cMesh.VerticesCloud.ClosestPoint(vertex);
            edgeIds = cMesh.GetEdgeIndex(edge);
        }
        private int vertexIds;
        private int edgeIds;
        public override double[] Error(CMesh cMesh)
        {
            Mesh m = cMesh.Mesh;
            var verts = cMesh.Vertices;
            var endVerts = m.TopologyEdges.GetTopologyVertices(edgeIds);
            Point3d xp = verts[endVerts.I];
            Point3d xq = verts[endVerts.J];
            Point3d xu = verts[vertexIds];
            var v1 = xu - xp;
            var v2 = xq - xp;
            var t1 = v1 / v1.Length;
            var t2 = v2 / v2.Length;
            var y = Vector3d.CrossProduct(t1, t2);
            //double err = 0.5 * y * y;
            return new double[] { y.X, y.Y, y.Z };
        }
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            int rows = 3;
            int columns = cMesh.DOF;
            Mesh m = cMesh.Mesh;
            var verts = cMesh.Vertices;
            var endVerts = m.TopologyEdges.GetTopologyVertices(edgeIds);
            Point3d xp = verts[endVerts.I];
            Point3d xq = verts[endVerts.J];
            Point3d xu = verts[vertexIds];
            var v1 = xu - xp;
            var v2 = xq - xp;
            var t1 = v1 / v1.Length;
            var t2 = v2 / v2.Length;
            var y = Vector3d.CrossProduct(t1, t2);

            var drdxu = (Vector3d.CrossProduct(t2, y) - (y * Vector3d.CrossProduct(t1, t2) * t1)) / v1.Length;
            var drdxq = (-Vector3d.CrossProduct(t1, y) - (y * Vector3d.CrossProduct(t1, t2) * t2)) / v2.Length;
            var drdxp = (Vector3d.CrossProduct(y, t2) + (y * Vector3d.CrossProduct(t1, t2) * t1)) / v1.Length
                      + (-Vector3d.CrossProduct(y, t1) + (y * Vector3d.CrossProduct(t1, t2) * t2)) / v2.Length;
            var elements = new List<Tuple<int, int, double>>();
            for (int i = 0; i < 3; i++)
            {
                elements.Add(new Tuple<int, int, double>(0, 3 * vertexIds + i, drdxu[i]));
                elements.Add(new Tuple<int, int, double>(0, 3 * endVerts.J + i, drdxq[i]));
                elements.Add(new Tuple<int, int, double>(0, 3 * endVerts.I + i, drdxp[i]));
            }

            return new SparseMatrixBuilder(rows, columns, elements);
        }
    }
}
