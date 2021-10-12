﻿using System;
using System.Collections.Generic;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;
using Rhino;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class RigidEdge : Constraint
    {
        public RigidEdge() { }
        public override Matrix<double> Jacobian(CMesh cMesh)
        {
            int rows = cMesh.Mesh.TopologyEdges.Count;
            int columns = cMesh.Mesh.Vertices.Count * 3;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            for (int i = 0; i < cMesh.Mesh.TopologyEdges.Count; i++)
            {
                Rhino.IndexPair ind = cMesh.Mesh.TopologyEdges.GetTopologyVertices(i);
                Point3d a = cMesh.Mesh.Vertices[ind.I];
                Point3d b = cMesh.Mesh.Vertices[ind.J];

                for (int j = 0; j < 3; j++)
                {
                    double var1 = ((double)a[j] - (double)b[j]) / cMesh.EdgeLengthSquared[i];
                    int rind1 = i;
                    int cind1 = 3 * ind.I + j;
                    Tuple<int, int, double> element1 = Tuple.Create(rind1, cind1, var1);

                    double var2 = ((double)b[j] - (double)a[j]) / cMesh.EdgeLengthSquared[i];
                    int rind2 = i;
                    int cind2 = 3 * ind.J + j;
                    Tuple<int, int, double> element2 = Tuple.Create(rind2, cind2, var2);

                    elements.Add(element1);
                    elements.Add(element2);
                }
            }

            Matrix<double> jacobian = SparseMatrix.Build.SparseOfIndexed(rows, columns, elements);
            return jacobian;
        }
        public override Vector<double> Error(CMesh cMesh)
        {
            int n = cMesh.Mesh.TopologyEdges.Count;
            double[] error_ = new double[n];
            for (int i = 0; i < n; i++)
            {
                IndexPair ind = cMesh.Mesh.TopologyEdges.GetTopologyVertices(i);
                Point3d a = cMesh.Mesh.Vertices[ind.I];
                Point3d b = cMesh.Mesh.Vertices[ind.J];
                error_[i] = ((double)a.DistanceToSquared(b) / cMesh.EdgeLengthSquared[i] - 1) / 2;
            }
            Vector<double> error = DenseVector.Build.DenseOfArray(error_);
            return error;
        }
    }
}
