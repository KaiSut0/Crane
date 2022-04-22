using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using OpenCvSharp.CPlusPlus;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class EqualEdgeLength : Constraint
    {
        public EqualEdgeLength(CMesh cMesh, Line[] firstEdges, Line[] secondEdges, double[] lengthRatios)
        { 
            firstEdgeIds = firstEdges.Select(e => cMesh.GetEdgeIndex(e)).ToArray(); 
            secondEdgeIds = secondEdges.Select(e => cMesh.GetEdgeIndex(e)).ToArray();
            if (firstEdgeIds.Length != secondEdgeIds.Length)
                throw new Exception("The number of first edges is different to the number of second edges.");
            numEdgePairs = firstEdgeIds.Length;
            if (lengthRatios.Length != numEdgePairs)
            {
                this.lengthRatios = new double[numEdgePairs];
                for (int i = 0; i < numEdgePairs; i++) this.lengthRatios[i] = lengthRatios[0];
            }
            else this.lengthRatios = lengthRatios;
            averageEdgeLength =
                (firstEdges.Select(e => e.Length).Average() + secondEdges.Select(e => e.Length).Average()) / 2;
        }

        private readonly int numEdgePairs;
        private readonly int[] firstEdgeIds;
        private readonly int[] secondEdgeIds;
        private readonly double[] lengthRatios;
        private readonly double averageEdgeLength;

        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            var elements = new List<Tuple<int, int, double>>();
            var verts = cMesh.Mesh.Vertices.ToPoint3dArray();

            for (int i = 0; i < numEdgePairs; i++)
            {
                int firstEdgeId = firstEdgeIds[i];
                int secondEdgeId = secondEdgeIds[i];

                var fIdPair = cMesh.Mesh.TopologyEdges.GetTopologyVertices(firstEdgeId);
                var sIdPair = cMesh.Mesh.TopologyEdges.GetTopologyVertices(secondEdgeId);

                var vfst = verts[fIdPair.I] - verts[fIdPair.J];
                var vfet = -vfst;
                var vsst = verts[sIdPair.I] - verts[sIdPair.J];
                var vset = -vsst;

                double ratio = lengthRatios[i];

                for(int j = 0; j < 3; j++)
                {
                    double varf1 = ratio * ratio * vfst[j] / (averageEdgeLength * averageEdgeLength);
                    double varf2 = ratio * ratio * vfet[j] / (averageEdgeLength * averageEdgeLength);
                    double vars1 = -vsst[j] / (averageEdgeLength * averageEdgeLength);
                    double vars2 = -vset[j] / (averageEdgeLength * averageEdgeLength);
                    elements.Add(new Tuple<int, int, double>(i, 3 * fIdPair.I + j, varf1));
                    elements.Add(new Tuple<int, int, double>(i, 3 * fIdPair.J + j, varf2));
                    elements.Add(new Tuple<int, int, double>(i, 3 * sIdPair.I + j, vars1));
                    elements.Add(new Tuple<int, int, double>(i, 3 * sIdPair.J + j, vars2));
                }
            }

            return new SparseMatrixBuilder(numEdgePairs, 3 * verts.Length, elements);
        }

        public override double[] Error(CMesh cMesh)
        {
            var err = new double[numEdgePairs];

            for(int i = 0; i < numEdgePairs; i++)
            {
                int firstEdgeId = firstEdgeIds[i];
                int secondEdgeId = secondEdgeIds[i];

                double firstEdgeLength = cMesh.Mesh.TopologyEdges.EdgeLine(firstEdgeId).Length;
                double secondEdgeLength = cMesh.Mesh.TopologyEdges.EdgeLine(secondEdgeId).Length;
                double ratio = lengthRatios[i];
                err[i] = ((ratio * ratio * firstEdgeLength * firstEdgeLength -secondEdgeLength * secondEdgeLength) /
                          (2 * averageEdgeLength * averageEdgeLength));
            }

            return err;

        }
    }
}
