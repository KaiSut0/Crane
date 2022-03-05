using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace Crane.Constraints
{
    public class FixEdgeLength : Constraint
    {
        public FixEdgeLength(CMesh cMesh, Line[] edges, double[] edgeLengths)
        {
            this.edgeIds = edges.Select(e => cMesh.GetEdgeIndex(e)).ToArray();
            this.edgeLengths = edgeLengths;
            this.numEdges = edgeIds.Length;
            averageEdgeLength = edgeLengths.Average();
        }
        private readonly int numEdges;
        private readonly int[] edgeIds;
        private readonly double[] edgeLengths;
        private readonly double averageEdgeLength;
        public override Matrix<double> Jacobian(CMesh cm)
        {
            Mesh m = cm.Mesh;

            var verts = m.Vertices.ToPoint3dArray();
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            for(int i = 0; i < numEdges; i++)
            {
                int edge_id = edgeIds[i];
                int stPtId = m.TopologyEdges.GetTopologyVertices(edge_id).I;
                int enPtId = m.TopologyEdges.GetTopologyVertices(edge_id).J;

                var vst = verts[stPtId] - verts[enPtId];
                var vet = -vst;

                for(int j = 0; j < 3; j++)
                {
                    double var1 = vst[j] / (averageEdgeLength*averageEdgeLength);
                    double var2 = vet[j] / (averageEdgeLength*averageEdgeLength);
                    elements.Add(new Tuple<int, int, double>(i, 3*stPtId+j, var1));
                    elements.Add(new Tuple<int, int, double>(i, 3 * enPtId + j, var2));
                }
            }

            return Matrix<double>.Build.SparseOfIndexed(numEdges, 3*verts.Length, elements);
        }
        public override Vector<double> Error(CMesh cm)
        {
            double[] err = new double[numEdges];
            for(int i = 0; i < numEdges; i++)
            {
                int edge_id = edgeIds[i];
                double goalLength = edgeLengths[i];
                double edgeLength = cm.Mesh.TopologyEdges.EdgeLine(edge_id).Length;
                err[i] = (edgeLength*edgeLength - goalLength*goalLength)/(2*averageEdgeLength*averageEdgeLength);
            }

            return Vector<double>.Build.DenseOfArray(err);
        }
    }
}

