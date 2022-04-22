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
    public class EqualEdgeLengthSum : Constraint
    {
        public EqualEdgeLengthSum(CMesh cMesh, Line[] firstEdges, Line[] secondEdges)
        {
            firstEdgeIds = firstEdges.Select(e => cMesh.GetEdgeIndex(e)).ToArray();
            secondEdgeIds = secondEdges.Select(e => cMesh.GetEdgeIndex(e)).ToArray();
        }
        private readonly int[] firstEdgeIds;
        private readonly int[] secondEdgeIds;

        public override Matrix<double> Jacobian(CMesh cMesh)
        {
            int rows = 1;
            int columns = cMesh.DOF;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            Dictionary<int, Vector3d> derivative = new Dictionary<int, Vector3d>();
            foreach (var eId in firstEdgeIds)
            {
                var vPair = cMesh.Mesh.TopologyEdges.GetTopologyVertices(eId);
                var vSt = vPair.I;
                var vEn = vPair.J;
                var elenDerivatives = cMesh.ComputeDerivativeOfEdgeLength(eId);
                if (derivative.ContainsKey(vSt))
                {
                    derivative[vSt] += elenDerivatives[0]/cMesh.AverageEdgeLength;
                }
                else
                {
                    derivative[vSt] = elenDerivatives[0]/cMesh.AverageEdgeLength;
                }
                if (derivative.ContainsKey(vEn))
                {
                    derivative[vEn] += elenDerivatives[1]/cMesh.AverageEdgeLength;
                }
                else
                {
                    derivative[vEn] = elenDerivatives[1]/cMesh.AverageEdgeLength;
                }
            }
            foreach (var eId in secondEdgeIds)
            {
                var vPair = cMesh.Mesh.TopologyEdges.GetTopologyVertices(eId);
                var vSt = vPair.I;
                var vEn = vPair.J;
                var elenDerivatives = cMesh.ComputeDerivativeOfEdgeLength(eId);
                if (derivative.ContainsKey(vSt))
                {
                    derivative[vSt] -= elenDerivatives[0]/cMesh.AverageEdgeLength;
                }
                else
                {
                    derivative[vSt] = -elenDerivatives[0]/cMesh.AverageEdgeLength;
                }
                if (derivative.ContainsKey(vEn))
                {
                    derivative[vEn] -= elenDerivatives[1]/cMesh.AverageEdgeLength;
                }
                else
                {
                    derivative[vEn] = -elenDerivatives[1]/cMesh.AverageEdgeLength;
                }
            }
            var keys = derivative.Keys.ToArray();
            foreach (var key in keys)
            {
                var DEDV = derivative[key];
                for (int j = 0; j < 3; j++)
                {
                    var val = DEDV[j];
                    elements.Add(Tuple.Create(0, 3*key+j, val));
                }
            }
            return Matrix<double>.Build.SparseOfIndexed(rows, columns, elements);


        }

        public override Vector<double> Error(CMesh cMesh)
        {
            double error = 0;
            double firstEdgeLength = 0;
            double secondEdgeLength = 0;
            foreach (int eid in firstEdgeIds) firstEdgeLength += cMesh.Mesh.TopologyEdges.EdgeLine(eid).Length;
            foreach (int eid in secondEdgeIds) secondEdgeLength += cMesh.Mesh.TopologyEdges.EdgeLine(eid).Length;
            error = (firstEdgeLength - secondEdgeLength) / cMesh.AverageEdgeLength;
            return Vector<double>.Build.DenseOfArray(new double[] { error });
        }
    }
}
