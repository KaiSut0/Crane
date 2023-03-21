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
            this.stiffnesses = new double[numEdges];
            for (int i = 0; i < numEdges; i++) this.stiffnesses[i] = 1;
            useDifferentStiffness = false;
        }
        public FixEdgeLength(CMesh cMesh, Line[] edges, double[] edgeLengths, double[] stiffnesses)
        {
            this.edgeIds = edges.Select(e => cMesh.GetEdgeIndex(e)).ToArray();
            this.edgeLengths = edgeLengths;
            this.numEdges = edgeIds.Length;
            averageEdgeLength = edgeLengths.Average();
            this.stiffnesses = stiffnesses;
            useDifferentStiffness = false;
        }
        public FixEdgeLength(CMesh cMesh, Line[] edges, double[] edgeLengths, double[] longerStiffnesses, double[] shorterStiffnesses)
        {
            this.edgeIds = edges.Select(e => cMesh.GetEdgeIndex(e)).ToArray();
            this.edgeLengths = edgeLengths;
            this.numEdges = edgeIds.Length;
            averageEdgeLength = edgeLengths.Average();
            this.longerStiffnesses = longerStiffnesses;
            this.shorterStiffnesses= shorterStiffnesses;
            useDifferentStiffness = true;

        }
        private readonly int numEdges;
        private readonly int[] edgeIds;
        private readonly double[] edgeLengths;
        private readonly double averageEdgeLength;
        private readonly double[] stiffnesses;
        private readonly double[] longerStiffnesses;
        private readonly double[] shorterStiffnesses;
        private readonly bool useDifferentStiffness;
        public override SparseMatrixBuilder Jacobian(CMesh cm)
        {
            Mesh m = cm.Mesh;

            var verts = m.Vertices.ToPoint3dArray();
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            if (useDifferentStiffness)
            {
                for (int i = 0; i < numEdges; i++)
                {
                    int edge_id = edgeIds[i];
                    double goalLength = edgeLengths[i];
                    double edgeLength = cm.Mesh.TopologyEdges.EdgeLine(edge_id).Length;
                    double stiffness = longerStiffnesses[i];
                    if (edgeLength < goalLength) stiffness = shorterStiffnesses[i];
                    int stPtId = m.TopologyEdges.GetTopologyVertices(edge_id).I;
                    int enPtId = m.TopologyEdges.GetTopologyVertices(edge_id).J;

                    var vst = verts[stPtId] - verts[enPtId];
                    var vet = -vst;

                    for (int j = 0; j < 3; j++)
                    {
                        double var1 = stiffness * vst[j] / (averageEdgeLength * averageEdgeLength);
                        double var2 = stiffness * vet[j] / (averageEdgeLength * averageEdgeLength);
                        elements.Add(new Tuple<int, int, double>(i, 3 * stPtId + j, var1));
                        elements.Add(new Tuple<int, int, double>(i, 3 * enPtId + j, var2));
                    }
                }
            }

            else
            {
                for (int i = 0; i < numEdges; i++)
                {
                    int edge_id = edgeIds[i];
                    int stPtId = m.TopologyEdges.GetTopologyVertices(edge_id).I;
                    int enPtId = m.TopologyEdges.GetTopologyVertices(edge_id).J;

                    var vst = verts[stPtId] - verts[enPtId];
                    var vet = -vst;

                    for (int j = 0; j < 3; j++)
                    {
                        double var1 = stiffnesses[i] * vst[j] / (averageEdgeLength * averageEdgeLength);
                        double var2 = stiffnesses[i] * vet[j] / (averageEdgeLength * averageEdgeLength);
                        elements.Add(new Tuple<int, int, double>(i, 3 * stPtId + j, var1));
                        elements.Add(new Tuple<int, int, double>(i, 3 * enPtId + j, var2));
                    }
                }
            }


            return new SparseMatrixBuilder(numEdges, 3 * verts.Length, elements);
        }
        public override double[] Error(CMesh cm)
        {
            double[] err = new double[numEdges];
            if (useDifferentStiffness)
            {
                for (int i = 0; i < numEdges; i++)
                {
                    int edge_id = edgeIds[i];
                    double goalLength = edgeLengths[i];
                    double edgeLength = cm.Mesh.TopologyEdges.EdgeLine(edge_id).Length;
                    double stiffness = longerStiffnesses[i];
                    if (edgeLength < goalLength) stiffness = shorterStiffnesses[i];
                    err[i] = stiffness * (edgeLength * edgeLength - goalLength * goalLength) / (2 * averageEdgeLength * averageEdgeLength);
                }
            }
            else
            {
                for (int i = 0; i < numEdges; i++)
                {
                    int edge_id = edgeIds[i];
                    double goalLength = edgeLengths[i];
                    double edgeLength = cm.Mesh.TopologyEdges.EdgeLine(edge_id).Length;
                    err[i] = stiffnesses[i] * (edgeLength * edgeLength - goalLength * goalLength) / (2 * averageEdgeLength * averageEdgeLength);
                }
            }


            return err;
        }
    }
}

